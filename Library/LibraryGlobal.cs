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

using System.Reflection;

namespace PicoGK
{
    public partial class Library
    {
        public class GlobalInstance : IDisposable
        {
            public Viewer   oViewer     => m_oViewer;
            public Library  oLibrary    => m_oLib;
            public LogFile  xLog        => m_xLog;


            public GlobalInstance(  float fVoxelSizeMM,
                                    string strLogPath = "",
                                    string strViewerTitle = "PicoGK",
                                    string strViewerEnvironment = "")
            {
                m_xLog      = new LogFile(strLogPath != "" ? strLogPath : Path.Combine(Utils.strDocumentsFolder(), "PicoGK.log"));

                try
                {
                    m_oLib = new(fVoxelSizeMM);
                } 

                catch (Exception e)
                { 
                    m_xLog.Log($"-----------------------------------------");
                    m_xLog.Log($"-- Could not initialize PicoGK Library --");
                    m_xLog.Log($"-----------------------------------------");
                    m_xLog.Log($"Most likely cause is that the PicoGK runtime library wasn't found");
                    m_xLog.Log($"Make sure {Config.strPicoGKLib}.dylib/.dll is accessible and has execution rights.");
                    m_xLog.Log($"See PicoGK documentation on GitHub for troubleshooting info");
                    m_xLog.Log($"Terribly long error string follows (usually devoid of real information):");
                    m_xLog.Log($"--------------------------------");
                    m_xLog.Log($"-");
                    m_xLog.Log($"{e}\n");
                    m_xLog.Log($"-");
                    m_xLog.Log($"--------------------------------");
                    
                    throw new Exception("Failed to load PicoGK library");
                }

                try
                {    
                    m_xLog.Log($"PicoGK:    {Library.strName()}");
                    m_xLog.Log($"           {Library.strVersion()}");
                    m_xLog.Log($"           {Library.strBuildInfo()}\n");
                    m_xLog.Log($"VoxelSize: {fVoxelSizeMM} (mm)");

                    m_xLog.Log("Happy Computational Engineering!\n\n");
                }

                catch (Exception e)
                {
                    m_xLog.Log($"Failed to get PicoGK library info: {e.Message}");
                    throw;
                }

                // Test if we can create the most basic of objects in PicoGK
                // if this fails, something is wrong in the interplay with the
                // runtime (should never happen in practice)

                try
                {
                    //Lattice     lat     = new(m_oLib);
                    Voxels      vox     = new(m_oLib);
                    Mesh        msh     = new(m_oLib);
                    Voxels      voxM    = new(msh);
                    //Voxels      voxL    = new(lat);
                    PolyLine    oPoly   = new(m_oLib, "FF0000");
                }

                catch (Exception e)
                {
                    m_xLog.Log($"Failed to instantiate basic PicoGK types:\n\n{e.Message}");
                    throw;
                }

                // Let's create the viewer environment

                m_oViewer   = new(strViewerTitle, new(2000,2000), m_xLog);

                if (strViewerEnvironment == "")
                {
                    try
                    {
                        m_xLog.Log($"Loading lights embedded environment");

                        Assembly oAssembly = typeof(Library).Assembly;
                        using Stream oStream = oAssembly.GetManifestResourceStream("PicoGK.Resources.Environment.zip")
                                                                    ?? throw new FileNotFoundException("Embedded environmet not found.");
                        m_oViewer.LoadLightSetup(oStream);
                    }

                    catch (Exception)
                    {
                        m_xLog.Log($"Could not load lights embedded environment, trying to load from disk instead.");

                        string strLightsFile = strFindLightSetupFile(out string strSearched);

                        if (!File.Exists(strLightsFile))
                        {
                            strSearched += strLightsFile + "\n";

                            m_xLog.Log($"Could not find a lights file - your viewer will look quite dark.");
                            m_xLog.Log($"Searched in:");
                            m_xLog.Log($"{strSearched}");
                            m_xLog.Log("You can fix this by placing the file PicoGKLights.zip into one of these folders");
                            m_xLog.Log("or providing the file as a parameter at Library.Go()");
                        }

                        m_xLog.Log($"Using light setup {strLightsFile} from disk");
                        m_oViewer.LoadLightSetup(strLightsFile);
                    }
                }
                else
                {
                    if (!File.Exists(strViewerEnvironment))
                        throw new FileNotFoundException(strViewerEnvironment);

                    m_oViewer.LoadLightSetup(strViewerEnvironment);
                }

                m_oViewer.SetBackgroundColor("FF");

                // Make everything known globally

                RegisterGlobalLibrary(m_oLib);
                RegisterGlobalLog(m_xLog);
                RegisterGlobalViewer(m_oViewer);
            }

            Library m_oLib;
            Viewer  m_oViewer;
            LogFile m_xLog;

            ~GlobalInstance()
            {
                Dispose(false);
            }
        
            public void Dispose()
            {
                // Dispose of unmanaged resources.
                Dispose(true);
                // Suppress finalization.
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool bDisposing)
            {
                if (m_bDisposed)
                    return;

                if (bDisposing)
                {
                    UnregisterGlobalLog();
                    UnregisterGlobalViewer();
                    UnregisterGlobalLibrary();

                    m_oLib      .Dispose();
                    m_oViewer   .Dispose();
                    m_xLog      .Dispose();
                }

                m_bDisposed = true;
            }

             bool m_bDisposed = false;
        }

        public static float fVoxelSizeMM
        {
            get
            {
                lock (m_mtxGlobalLibrary)
                {
                    if (m_oGlobalLibrary is null)
                        throw new Exception("Your code relies on being called using Library::Go");

                    return m_oGlobalLibrary.fVoxelSize;
                }
            }
        }

        public static string strLogFolder
        {
            get
            {
                lock (m_mtxGlobalLibrary)
                {
                    if (m_oGlobalLibrary is null)
                        throw new Exception("Your code relies on being called using Library::Go");

                    return m_strLogPath;
                }
            }
        }

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

        public static ILog xLog()
        {
            lock (m_mtxGlobalLog)
            {
                if (m_xGlobalLog is null)
                    throw new Exception("Your code relies on being called using Library::Go");

                return m_xGlobalLog;
            }
        }

        public static void RegisterGlobalLog(ILog xLog)
        {
            lock (m_mtxGlobalLog)
            {
                if (m_xGlobalLog is not null)
                    throw new Exception("Cannot register more than one global PicoGK log file");

                m_xGlobalLog = xLog;
            }
        }

        public static void UnregisterGlobalLog()
        {
            lock (m_mtxGlobalLog)
            {
                m_xGlobalLog = null;
            }
        }

        static Library? m_oGlobalLibrary = null;
        static object   m_mtxGlobalLibrary = new();

        static string   m_strLogPath    = "";

        static ILog?    m_xGlobalLog = null;
        static object   m_mtxGlobalLog = new();

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
        /// <param name="fVoxelSizeMM">Voxel size in millimeters</param>
        /// <param name="fnTask">The task to execute</param>
        /// <param name="strLogFilePath">Filename of the logfile</param>
        /// <param name="bEndAppWithTask">If true, the viewer exits when your task is done</param>
        /// <param name="strWindowTitle">The title of your viewer window</param>
        /// <param name="strLightsFile">A specific lighting environment to load</param>
        /// <exception cref="Exception">
        /// Throws an exception, for a number of scenarios, for example, if the
        /// library cannot be found, folders, etc. cannot be created, etc.
        /// Always handle the exception to understand what's going on.
        /// </exception>
        public static void Go(  float fVoxelSizeMM,
                                ThreadStart fnTask,
                                string strLogFilePath   = "",
                                bool bEndAppWithTask    = false,
                                string strWindowTitle   = "PicoGK",
                                string strLightsFile    = "")
        {
            m_bAppExit      = false;
            m_bContinueTask = true;

            lock (m_mtxGlobalLibrary)
            {
                if (m_oGlobalLibrary != null)
                    throw new Exception("PicoGK only supports running one global library configuration at one time");
            }

            if (fVoxelSizeMM <= 0.0f)
                throw new Exception("Voxel size needs to be larger than 0mm");

            if (strLogFilePath == "")
                strLogFilePath = Path.Combine(Utils.strDocumentsFolder(), "PicoGK.log");

            m_strLogPath = Path.GetDirectoryName(strLogFilePath) ?? Utils.strDocumentsFolder();

            using (GlobalInstance oInstance = new(fVoxelSizeMM, strLogFilePath, strWindowTitle, strLightsFile))
            {
                Thread oThread = new Thread(fnTask);
                Log("Starting tasks.\n");
                oThread.Start();

                while (oInstance.oViewer.bPoll())
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
                            if (oInstance.oViewer.bIsIdle())
                                break;

                            // Otherwise we do another cycle
                        }
                    }
                }

                // Request task to end
                EndTask();

                int n=0;
                while (oThread.IsAlive)
                {
                    // Continue polling viewer to show GUI state

                    oInstance.oViewer.bPoll();
                    Thread.Sleep(5);
                    
                    if (n==0)
                    {
                        Log("Viewer window closing requested - Waiting for task to end.");
                    }

                    n++;
                    if (n>200)
                        n=0;
                }

                m_bAppExit = true;
                Log("Viewer Window Closed");
            }
        }

        public static void Log(string strFormat, params object[] args)
        {
            xLog().Log(strFormat, args);
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

        public static string strFindLightSetupFile(out string strSearched)
        {
            strSearched = "";

            string strLightsFile    = Path.Combine( Utils.strPicoGKSourceCodeFolder(), 
                                                    "assets/ViewerEnvironment.zip");

            if (File.Exists(strLightsFile))
                return strLightsFile;

            strSearched += strLightsFile + "\n";

            strLightsFile = Path.Combine(   Utils.strDocumentsFolder(), 
                                            "ViewerEnvironment.zip");

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