//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023 by LEAP 71
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

namespace PicoGK
{
    public partial class Library
    {
        /// <summary>
        /// Returns the library name (from the C++ side)
        /// </summary>
        /// <returns>The name of the dynamically loaded C++ library</returns>
        public static string strName()
        {
            const int iLen = 255;
            StringBuilder oBuilder = new StringBuilder(iLen);
            _GetName(oBuilder);
            return oBuilder.ToString();
        }

        /// <summary>
        /// Returns the library version (from the C++ side)
        /// </summary>
        /// <returns>The library version of the C++ library</returns>
        public static string strVersion()
        {
            const int iLen = 255;
            StringBuilder oBuilder = new StringBuilder(iLen);
            _GetVersion(oBuilder);
            return oBuilder.ToString();
        }

        /// <summary>
        /// Returns internal build info, such as build date/time
        /// of the C++ library
        /// </summary>
        /// <returns>Internal build info of the C++ library</returns>
        public static string strBuildInfo()
        {
            const int iLen = 255;
            StringBuilder oBuilder = new StringBuilder(iLen);
            _GetBuildInfo(oBuilder);
            return oBuilder.ToString();
        }

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
        public static void Go(  float _fVoxelSizeMM,
                                ThreadStart fnTask,
                                string strLogFolder     = "",
                                string strLogFileName   = "",
                                string strSrcFolder     = "",
                                string strLightsFile    = "")
        {
            Debug.Assert(_fVoxelSizeMM > 0.0f);
            fVoxelSizeMM = _fVoxelSizeMM;

            TestAssumptions();

            if (strLogFolder == "")
                strLogFolder = Utils.strDocumentsFolder();

            if (strLogFileName == "")
                strLogFileName = "PicoGK.log";

           string strLog = Path.Combine(    strLogFolder,
                                            strLogFileName);

            using (LogFile oLog = new LogFile(strLog))
            {

                lock (oMtxLog)
                {
                    if (oTheLog is not null)
                        throw new Exception("You cannot call PicoGK.Library.Go() more than once per app (1)");

                    if (oTheViewer is not null)
                        throw new Exception("You cannot call PicoGK.Library.Go() more than once per app (2)");
                    oTheLog = oLog;

                    Library.strLogFolder = strLogFolder;
                    Library.strSrcFolder = strSrcFolder;
                }

                Log("Welcome to PicoGK");

                try
                {
                    // Create a config using physical coordinates
                    _Init(fVoxelSizeMM);
                    // Done creating C++ Library
                }

                catch (Exception e)
                {
                    Log($"-----------------------------------------");
                    Log($"-- Could not initialize PicoGK Library --");
                    Log($"-----------------------------------------");
                    Log($"Most likely cause is that the PicoGK runtime library wasn't found");
                    Log($"Make sure {Config.strPicoGKLib}.dylib/.dll is accessible and has execution rights.");
                    Log($"See PicoGK documentation on GitHub for troubleshooting info");
                    Log($"Terribly long error string follows (usually devoid of real information):");
                    Log($"--------------------------------");
                    Log($"-");
                    Log($"{e}\n");
                    Log($"-");
                    Log($"--------------------------------");
                    throw new Exception("Failed to load PicoGK Library");
                }

                if (strLightsFile == "")
                {
                    string strSearched = "";

                    strLightsFile = Path.Combine(Utils.strPicoGKSourceCodeFolder(), "ViewerEnvironment/PicoGKDefaultEnv.zip");

                    if (!File.Exists(strLightsFile))
                    {
                        strSearched += strLightsFile + "\n";

                        if (strSrcFolder == "")
                        {
                            strLightsFile = Path.Combine(Utils.strDocumentsFolder(), "PicoGKDefaultEnv.zip");
                        }
                        else
                        {
                            strLightsFile = Path.Combine(strSrcFolder, "PicoGKDefaultEnv.zip");
                        }

                        if (!File.Exists(strLightsFile))
                        {
                            strSearched += strLightsFile + "\n";

                            strLightsFile = Path.Combine(Utils.strExecutableFolder(), "ViewerEnvironment.zip");

                            if (!File.Exists(strLightsFile))
                            {
                                strSearched += strLightsFile + "\n";

                                Log($"Could not find a lights file - your viewer will look quite dark.");
                                Log($"Searched in:");
                                Log($"{strSearched}");
                                Log("You can fix this by placing the file PicoGKLights.zip into one of these folders");
                                Log("or providing the file as a parameter at Library.Go()");
                            }
                        }
                    }
                }

                Log("Creating Viewer");

                Viewer? oViewer = null;

                try
                {
                    oViewer = new Viewer("PicoGK", new Vector2(2048f, 1024f));
                }

                catch (Exception e)
                {
                    Log("Failed to create viewer");
                    Log(e.ToString());

                    throw new Exception("Failed to create all necessary objects");
                }

                using (oViewer)
                {
                    if (!bSetup())
                    {
                        Log("!! Failed to initialize !!");
                        return;
                    }

                    try
                    {
                        oViewer.LoadLightSetup(strLightsFile);
                        oViewer.SetBackgroundColor("FF");
                    }

                    catch (Exception e)
                    {
                        Log($"Failed to load Light Setup - your viewer will look dark\n{e.Message}");
                    }
                    

                    lock (oMtxViewer)
                    {
                        oTheViewer = oViewer;
                    }

                    Thread oThread = new Thread(fnTask);

                    Log("Starting tasks.\n");
                    oThread.Start();

                    while (oViewer.bPoll())
                    {
                        Thread.Sleep(5); // 200 Hz is plenty
                    }

                    m_bAppExit = true;
                    Log("Viewer Window Closed");
                }
            }
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
        /// Thread-safe loging function
        /// </summary>
        /// <param name="strFormat">
        /// Use like Console.Write and others, so you can do Library.Log($"My variable {fVariable}") etc.
        /// </param>
        /// <exception cref="Exception">
        /// Will throw an exception if called before you call Library::Go, which shouldn't happen
        /// </exception>
        public static void Log(in string strFormat, params object[] args)
        {
            lock (oMtxLog)
            {
                if (oTheLog == null)
                    throw new Exception("Trying to access Log before Library::Go() was called");

                oTheLog.Log(strFormat, args);
            }
        }

        /// <summary>
        /// Thread-safe access to the viewer
        /// </summary>
        /// <returns>The viewer object</returns>
        /// <exception cref="Exception">
        /// Only throws an exception if called before you call Library::Go, which shouldn't happen
        /// </exception>
        public static Viewer oViewer()
        {
            lock (oMtxViewer)
            {
                if (oTheViewer is null)
                    throw new Exception("Trying to access Viewer before Library::Go() was called");

                return oTheViewer;
            }
        }

        /// <summary>
        /// This is an internal helper that tests if the data types have the
        /// memory layout that we assume, so we don't run into interoperability issues
        /// with the C++ side
        /// </summary>
        private static void TestAssumptions()
        {
            // Test a few assumptions
            // Built in data type Vector3 is implicit,
            // so should be compatible with our own
            // structs, but let's be sure

            Vector3     vec3    = new();
            Vector2     vec2    = new();
            Matrix4x4   mat4    = new();
            Coord       xyz     = new(0, 0, 0);
            Triangle    tri     = new(0, 0, 0);
            ColorFloat  clr     = new(0f);
            BBox2       oBB2    = new();
            BBox3       oBB3    = new();

            Debug.Assert(sizeof(bool)           == 1);                  // 8 bit for bool assumed
            Debug.Assert(Marshal.SizeOf(vec3)   == ((32 * 3) / 8));     // 3 x 32 bit float
            Debug.Assert(Marshal.SizeOf(vec2)   == ((32 * 2) / 8));     // 2 x 32 bit float
            Debug.Assert(Marshal.SizeOf(mat4)   == ((32 * 16) / 8));    // 4 x 4 x 32 bit float 
            Debug.Assert(Marshal.SizeOf(xyz)    == ((32 * 3) / 8));     // 3 x 32 bit integer
            Debug.Assert(Marshal.SizeOf(tri)    == ((32 * 3) / 8));     // 3 x 32 bit integer
            Debug.Assert(Marshal.SizeOf(clr)    == ((32 * 4) / 8));     // 4 x 32 bit float
            Debug.Assert(Marshal.SizeOf(oBB2)   == ((32 * 2 * 2) / 8)); // 2 x vec2
            Debug.Assert(Marshal.SizeOf(oBB3)   == ((32 * 3 * 2) / 8)); // 2 x vec3

            // If any of these assert, then something is wrong with the
            // memory layout, and the interface to compatible C libraries
            // will fail - this should never happen, as all these types
            // are well-defined
        }

        /// <summary>
        /// Logs the information from the library, usually the first line of
        /// defence, if something is misconfigured, for example the library path
        /// Also attempts to create all data types - if this blows up, then
        /// something is wrong with the C++/C# interplay
        /// </summary>
        /// <returns></returns>
        private static bool bSetup()
        {
            try
            {
                Log($"PicoGK:    {Library.strName()}");
                Log($"           {Library.strVersion()}");
                Log($"           {Library.strBuildInfo()}\n");
                Log($"VoxelSize: {Library.fVoxelSizeMM} (mm)");

                Log("Happy Computational Engineering!\n\n");
            }

            catch (Exception e)
            {
                Log("Failed to get PicoGK library info:\n\n{0}", e.Message);
                return false;
            }

            try
            {
                Lattice     lat     = new();
                Voxels      vox     = new();
                Mesh        msh     = new();
                Voxels      voxM    = new(msh);
                Voxels      voxL    = new(lat);
                PolyLine    oPoly   = new("FF0000");
            }

            catch (Exception e)
            {
                Log("Failed to instantiate basic PicoGK types:\n\n{0}", e.Message);
                return false;
            }

            return true;
        }

        public static   float   fVoxelSizeMM = 0.0f;
        public static   string  strLogFolder = "";
        public static   string  strSrcFolder = "";

        private static object   oMtxLog     = new object();
        private static object   oMtxViewer  = new object();
        private static LogFile? oTheLog     = null;
        private static Viewer?  oTheViewer  = null;
    }
}