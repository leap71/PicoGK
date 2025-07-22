//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023-2025 by LEAP 71
// https://leap71.com
//
// Computational Engineering will profoundly change our physical world in the
// years ahead. Thank you for being part of the journey.
//
// We have developed this library to be used widely, for both commercial and
// non-commercial projects alike. Therefore, we have released it under a 
// permissive open-source license.
//
// The foundation of PicoGK is a thin layer on top of the powerful open-source
// OpenVDB project, which in turn uses many other Free and Open Source Software
// libraries. We are grateful to be able to stand on the shoulders of giants.
//
// LEAP 71 licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with the
// License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, THE SOFTWARE IS
// PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// See the License for the specific language governing permissions and
// limitations under the License.   
//

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;

namespace PicoGK
{
    public partial class Library
    {
        
        public static Library oLibrary()
        {
            lock (m_mtxGlobalLibrary)
            {
                if (m_oGlobalLibrary is null)
                    throw new Exception("Your code relies on being called using Library::Go");

                return m_oGlobalLibrary;
            }
        }

        public static void RegisterGlobalLibrary(Library oLibrary)
        {
            lock (m_mtxGlobalLibrary)
            {
                if (m_oGlobalLibrary is not null)
                    throw new Exception("Cannot register more than one global PicoGK library");

                m_oGlobalLibrary = oLibrary;
            }
        }

        public static void UnregisterGlobalLibrary()
        {
            lock (m_mtxGlobalLibrary)
            {
                m_oGlobalLibrary = null;
            }
        }

        public static Viewer oViewer()
        {
            lock (m_mtxGlobalViewer)
            {
                if (m_oGlobalViewer is null)
                    throw new Exception("Your code relies on being called using Library::Go");

                return m_oGlobalViewer;
            }
        }

        public static void RegisterGlobalViewer(Viewer oViewer)
        {
            lock (m_mtxGlobalViewer)
            {
                if (m_oGlobalViewer is not null)
                    throw new Exception("Cannot register more than one global PicoGK viewer");

                m_oGlobalViewer = oViewer;
            }
        }

        public static void UnregisterGlobalViewer()
        {
            lock (m_mtxGlobalViewer)
            {
                m_oGlobalViewer = null;
            }
        }

        public static LogFile oLogFile()
        {
            lock (m_mtxGlobalLogFile)
            {
                if (m_oGlobalLogFile is null)
                    throw new Exception("Your code relies on being called using Library::Go");

                return m_oGlobalLogFile;
            }
        }

        public static void RegisterGlobalLogFile(LogFile oLogFile)
        {
            lock (m_mtxGlobalLogFile)
            {
                if (m_oGlobalLogFile is not null)
                    throw new Exception("Cannot register more than one global PicoGK log file");

                m_oGlobalLogFile = oLogFile;
            }
        }

        public static void UnregisterGlobalLogFile()
        {
            lock (m_mtxGlobalLogFile)
            {
                m_oGlobalLogFile = null;
            }
        }

        static Library? m_oGlobalLibrary = null;
        static object   m_mtxGlobalLibrary = new();

        static LogFile? m_oGlobalLogFile = null;
        static object   m_mtxGlobalLogFile = new();

        static Viewer?  m_oGlobalViewer = null;
        static object   m_mtxGlobalViewer = new();

        /// <summary>
        /// This is the one library function that you call to run your code
        /// it sets up the PicoGK library, with the specified voxel size and
        /// builds the PicoGK environment with viewer, log and other internals
        /// The fnTask you pass is called after everything is set up correctly
        /// inside of fnTask, you do your processing, displaying it in
        /// Library::oTheViewer and logging info with Library::Log()
        /// </summary>
        /// <param name="_fVoxelSizeMM">
        /// The global voxel size in MM, for example 0.1
        /// </param>
        /// <param name="fnTask">
        /// The task to be executed (it will run in a separate thread)
        /// </param>
        /// <param name="strLogFolder">
        /// The folder where you want the log file (defaults to your
        /// documents folder
        /// </param>
        /// <param name="strLogFileName">
        /// The file name for your log. Defaults to PicoGK_ with date and time
        /// appended. If your specify the same log file name here, it prevents
        /// PicoGK from creating a new log file name everytime.
        /// </param>
        /// <param name="strSrcFolder">
        /// This is purely a helper for you, it's not used internally. But you
        /// can access this folder name throug Library::strSrcFolder, which is
        /// convenient.
        /// </param>
        /// <exception cref="Exception">
        /// Throws an exception, for a number of scenarios, for example, if the
        /// library cannot be found, folders, etc. cannot be created, etc.
        /// Always handle the exception to understand what's going on.
        /// </exception>
        public static void Go(  float fVoxelSizeMM,
                                ThreadStart fnTask,
                                string strLogFolder     = "",
                                string strLogFileName   = "",
                                string strSrcFolder     = "",
                                string strLightsFile    = "",
                                bool bEndAppWithTask    = false)
        {
            lock (m_mtxGlobalLibrary)
            {
                if (m_oGlobalLibrary != null)
                    throw new Exception("PicoGK only supports running one global library configuration at one time");
            }

            if (fVoxelSizeMM <= 0.0f)
                throw new Exception("Voxel size needs to be larger than 0mm");

            if (strLogFolder == "")
                strLogFolder = Utils.strDocumentsFolder();

            if (strLogFileName == "")
                strLogFileName = "PicoGK.log";

            string strLog = Path.Combine(   strLogFolder,
                                            strLogFileName);

            LogFile oLog;

            try
            {
                oLog = new(strLog);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to create PicoGK log file: {strLog}");
                Console.WriteLine($"{e.Message}");
                throw;
            }

            using (oLog)
            {
                Library oLib;

                try
                {
                    oLib = new (fVoxelSizeMM);
                }
                catch (Exception e)
                {
                    oLog.Log($"-----------------------------------------");
                    oLog.Log($"-- Could not initialize PicoGK Library --");
                    oLog.Log($"-----------------------------------------");
                    oLog.Log($"Most likely cause is that the PicoGK runtime library wasn't found");
                    oLog.Log($"Make sure {Config.strPicoGKLib}.dylib/.dll is accessible and has execution rights.");
                    oLog.Log($"See PicoGK documentation on GitHub for troubleshooting info");
                    oLog.Log($"Terribly long error string follows (usually devoid of real information):");
                    oLog.Log($"--------------------------------");
                    oLog.Log($"-");
                    oLog.Log($"{e}\n");
                    oLog.Log($"-");
                    oLog.Log($"--------------------------------");
                    
                    throw new Exception("Failed to load PicoGK library");
                }

                using (oLib)
                {
                    try
                    {
                        TestAssumptions();
                        // Check memory layout and other things
                    }
                    catch
                    {
                        oLog.Log("Internal compatibility error between runtime and C# code");
                        throw new Exception("Failed to load PicoGK library");
                    }

                    if (!oLib.bSetup(oLog))
                    {
                        oLog.Log("Unable to use PicoGK library");
                        throw new Exception("Unable to use PicoGK library");
                    }

                    oLog.Log("Welcome to PicoGK");
                    oLog.Log("Loading Viewer");

                    Viewer oViewer;

                    try
                    {
                        oViewer = new Viewer(   "PicoGK", 
                                                new Vector2(2048f, 1024f),
                                                oLog);
                    }

                    catch (Exception e)
                    {
                        oLog.Log("Failed to create viewer");
                        oLog.Log(e.Message);

                        throw new Exception("Failed to create all necessary objects");
                    }

                    using (oViewer)
                    {
                        try
                        {
                            // Load lights
                            string strSearched = "";

                            if (strLightsFile == "")
                            {
                                // No lights file specified, let's try to load the embedded environment first

                                try
                                {
                                    oLog.Log($"Loading lights embedded environment");

                                    Assembly oAssembly = typeof(Library).Assembly;
                                    using Stream oStream = oAssembly.GetManifestResourceStream("PicoGK.Resources.Environment.zip")
                                                                    ?? throw new FileNotFoundException("Embedded environmet not found.");
                                    oViewer.LoadLightSetup(oStream);
                                }

                                catch (Exception)
                                {
                                    oLog.Log($"Could not load lights embedded environment, trying to load from disk instead.");

                                    strLightsFile = strFindLightSetupFile(  strSrcFolder, 
                                                                            out strSearched);

                                    if (!File.Exists(strLightsFile))
                                    {
                                        strSearched += strLightsFile + "\n";

                                        oLog.Log($"Could not find a lights file - your viewer will look quite dark.");
                                        oLog.Log($"Searched in:");
                                        oLog.Log($"{strSearched}");
                                        oLog.Log("You can fix this by placing the file PicoGKLights.zip into one of these folders");
                                        oLog.Log("or providing the file as a parameter at Library.Go()");
                                    }

                                    oLog.Log($"Using light setup {strLightsFile} from disk");
                                    oViewer.LoadLightSetup(strLightsFile);
                                }
                            }
                            
                            oViewer.SetBackgroundColor("FF");
                        }

                        catch (Exception e)
                        {
                            oLog.Log($"Failed to load Light Setup - your viewer will look dark\n{e.Message}");
                        }
                        
                    }

                    {
                        RegisterGlobalLogFile(oLog);
                        RegisterGlobalLibrary(oLib);
                        RegisterGlobalViewer(oViewer);

                        Thread oThread = new Thread(fnTask);

                        Log("Starting tasks.\n");
                        oThread.Start();

                        while (oViewer.bPoll())
                        {
                            Thread.Sleep(5); // 200 Hz is plenty

                            if (bEndAppWithTask)
                            {
                                // Close app when task ends

                                if (!oThread.IsAlive)
                                {
                                    // Task is done
                                    // Check if viewer has pending actions
                                    // if not, we are done
                                    if (oViewer.bIsIdle())
                                        break;

                                    // Otherwise we do another cycle
                                }
                            }
                        }

                        m_bAppExit = true;
                        Log("Viewer Window Closed");
                    }
                }
            }
        }

        public static void Log(string strFormat, params object[] args)
        {
            oLogFile().Log(strFormat, args);
        }

        /// <summary>
        /// Checks whether the task started using Go() should continue, and returns true if that's the case or false otherwise.
        /// If your task can take a non-trivial amount of time, check this function periodically.
        /// If it returns false, exit the task function as soon as possible.
        /// </summary>
        /// <param name="bAppExitOnly">If true, the bContinueTask function will only take into consideration if the application is about to exit. Any pending EndTask() requests will be ignored.</param>
        /// <returns></returns>
        public static bool bContinueTask(bool bAppExitOnly = false)
        {
            return !m_bAppExit && (bAppExitOnly || m_bContinueTask);
        }

        /// <summary>
        /// Requests the task started by the Go() function to end.
        /// Note that it's the responsability of the task to call the bContinueTask() function periodically and to honor these requests.
        /// </summary>
        public static void EndTask()
        {
            m_bContinueTask = false;
        }

        /// <summary>
        /// Cancels any pending request to end the task.
        /// </summary>
        public static void CancelEndTaskRequest()
        {
            m_bContinueTask = true;
        }

        static bool m_bAppExit = false;
        static bool m_bContinueTask = true;   

        /// <summary>
        /// Logs the information from the library, usually the first line of
        /// defence, if something is misconfigured, for example the library path
        /// Also attempts to create all data types - if this blows up, then
        /// something is wrong with the C++/C# interplay
        /// </summary>
        /// <returns></returns>
        bool bSetup(LogFile log)
        {
            try
            {
                log.Log($"PicoGK:    {Library.strName}");
                log.Log($"           {Library.strVersion}");
                log.Log($"           {Library.strBuildInfo}\n");
                log.Log($"VoxelSize: {fVoxelSize} (mm)");

                log.Log("Happy Computational Engineering!\n\n");
            }

            catch (Exception e)
            {
                log.Log("Failed to get PicoGK library info:\n\n{0}", e.Message);
                return false;
            }

            try
            {
                Lattice     lat     = new(this);
                Voxels      vox     = new(this);
                Mesh        msh     = new(this);
                Voxels      voxM    = new(msh);
                Voxels      voxL    = new(lat);
                PolyLine    oPoly   = new(this, "FF0000");
            }

            catch (Exception e)
            {
                log.Log("Failed to instantiate basic PicoGK types:\n\n{0}", e.Message);
                return false;
            }

            return true;
        }

        public static string strFindLightSetupFile( string strInputFolder,
                                                    out string strSearched)
        {
            strSearched = "";

            string strLightsFile    = Path.Combine( Utils.strPicoGKSourceCodeFolder(), 
                                                    "assets/ViewerEnvironment.zip");

            if (File.Exists(strLightsFile))
                return strLightsFile;

            strSearched += strLightsFile + "\n";

            if (strInputFolder == "")
            {
                strLightsFile = Path.Combine(   Utils.strDocumentsFolder(), 
                                                "ViewerEnvironment.zip");

                strSearched += strLightsFile + "\n";
            }
            else
            {
                strLightsFile = Path.Combine(   strInputFolder, 
                                                "ViewerEnvironment.zip");

                strSearched += strLightsFile + "\n";
            }

            if (!File.Exists(strLightsFile))
            {
                strLightsFile = Path.Combine(    Utils.strExecutableFolder(), 
                                                "ViewerEnvironment.zip");

                strSearched += strLightsFile + "\n";
            }

            return strLightsFile;
        }
    }
}