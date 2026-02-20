//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023-2026 by LEAP 71
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

using System.Numerics;
using System.Diagnostics;

namespace PicoGK
{
    public partial class Utils
    {
        /// <summary>
        /// Strip quotes of a quoted path like "/usr/lib/" -> /usr/lib/
        /// </summary>
        /// <param name="strPath"></param>Path that is potentially quoted
        /// <returns></returns>Unquoted path
        static public string strStripQuotesFromPath(string strPath)
        {
            if (strPath.StartsWith("\"") && strPath.EndsWith("\""))
            {
                return strPath.Substring(1, strPath.Length - 2);
            }

            return strPath;
        }

        /// <summary>
        /// Strips the extension from a filename
        /// </summary>
        static public string strStripExtension(string strPath)
        {
            return Path.Combine(
                                    Path.GetDirectoryName(strPath) ?? "",
                                    Path.GetFileNameWithoutExtension(strPath));
        }

        /// <summary>
        /// Wait for a file's creation
        /// </summary>
        /// <param name="strFile"></param>
        /// File to check for existence
        /// <param name="fTimeOut"></param> T
        /// imeout in seconds
        /// <returns></returns>true if file exists, false if timeout
        static public bool bWaitForFileExistence(string strFile, float fTimeOut=1000000f)
        {
            Stopwatch oWatch = new();
            oWatch.Start();
            long lTimeout = oWatch.ElapsedMilliseconds + (long) (fTimeOut * 1000);

            while (oWatch.ElapsedMilliseconds < lTimeout)
            {
                if (File.Exists(strFile))
                    return true;

                Thread.Sleep(100);
            }

            return false;
        }

        /// <summary>
        /// Returns the path to the home folder (cross platform compatible)
        /// </summary>
        /// <returns>Path to the home folder</returns>
        /// <exception cref="Exception">Excepts, if not found</exception>
        static public string strHomeFolder()
        {
            string? str = null;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                str = Environment.GetEnvironmentVariable("HOME");
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                str = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            }

            if (str is null)
            {
                throw new Exception("Could not find home folder for " + Environment.OSVersion.Platform.ToString());
            }

            return str;
        }

        /// <summary>
        /// Returns the path to the documents folder (cross platform compatible)
        /// </summary>
        /// <returns>The path th documents folder</returns>
        /// <exception cref="Exception">Excepts, if unable to find</exception>
        static public string strDocumentsFolder()
        {
            string? str = null;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                str = Environment.GetEnvironmentVariable("HOME");

                if (str is null)
                {
                    throw new Exception("Could not find home folder for " + Environment.OSVersion.Platform.ToString());
                }

                return Path.Combine(str, "Documents");
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                str = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            }

            if (str is null)
            {
                throw new Exception("Could not find home folder for " + Environment.OSVersion.Platform.ToString());
            }

            return str;
        }

        /// <summary>
        /// Returns the path to the source folder of your project, under the
        /// assumption that the executable .NET DLL is in its usual place.
        /// 
        /// This function is can be used to load files in a subdirectory
        /// of your source code, such as the viewer environment or fonts.
        /// </summary>
        /// <returns>The assumed path to the source code root</returns>
        static public string strProjectRootFolder()
        {
            string strPath = AppContext.BaseDirectory;

            for (int n = 0; n < 4; n++)
            {
                strPath = Path.GetDirectoryName(strPath) ?? "";
            }

            return strPath;
        }

        /// <summary>
        /// Returns the path to the source folder of PicoGK, making the following
        /// Assumptions:
        /// - PicoGK is contained in a subfolder named "PicoGK" inside your
        ///   main project, so your path is MyProject/PicoGK
        /// - The executable .NET DLL is in its usual place
        /// This function is used mainly to load the viewer environment and
        /// test files used in the PicoGK examples
        /// </summary>
        /// <returns>The assumed path to the PicoGK source code</returns>
        static public string strPicoGKSourceCodeFolder()
        {
            return Path.Combine(strProjectRootFolder(), "PicoGK");
        }

        /// <summary>
        /// Returns the path in which your current executable resides
        /// </summary>
        /// <returns>The folder in which your executable resides</returns>
        static public string strExecutableFolder()
        {
            string strExePath = System.Reflection.Assembly.GetExecutingAssembly().Location ?? "";
            return System.IO.Path.GetDirectoryName(strExePath) ?? "";
        }

        /// <summary>
        /// Returns a file name in the form 20230930_134500 to be used in log files etc.
        /// </summary>
        /// <param name="strPrefix">Prepended before the date/time stamp</param>
        /// <param name="strPostfix">Appended after the date/time stamp</param>
        /// <returns></returns>
        static public string strDateTimeFilename(   in string strPrefix,
                                                    in string strPostfix)
        {
            return strPrefix + DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss") + strPostfix;
        }

        /// <summary>
        /// Shorted a string, IF it is too long. If it is not too long, nothing happens
        /// C# is lacking such a function (all other functions throw exceptions)
        /// </summary>
        /// <param name="str">String to shorten (if too long)</param>
        /// <param name="iMaxCharacters">Number of max characters in the string</param>
        /// <returns></returns>
        public static string strShorten(string str, int iMaxCharacters)
        {
            if (str.Length < iMaxCharacters)
                return str;

            // in their infinite wisdom, the coders of C# decided to
            // throw an exception, if str is shorter than iMaxCharacters
            // that's why this function is even necessary
            return str[..iMaxCharacters];
        }

        static public Mesh mshCreateCube(   Library lib,
                                            BBox3 oBox)
        {
            return mshCreateCube(lib, oBox.vecSize(), oBox.vecCenter());
        }

        static public Mesh mshCreateCube(   Library lib,
                                            Vector3? vecScale       = null,
                                            Vector3? vecOffsetMM    = null)
        {
            Vector3 vecS        = vecScale      ?? new Vector3(1.0f);
            Vector3 vecOffset   = vecOffsetMM   ?? new Vector3(0.0f);

            Mesh oMesh = new Mesh(lib);

            Vector3[] cubeVertices =
            {
                new Vector3(-0.5f * vecS.X, -0.5f * vecS.Y, -0.5f * vecS.Z) + vecOffset,
                new Vector3(-0.5f * vecS.X, -0.5f * vecS.Y,  0.5f * vecS.Z) + vecOffset,
                new Vector3(-0.5f * vecS.X,  0.5f * vecS.Y, -0.5f * vecS.Z) + vecOffset,
                new Vector3(-0.5f * vecS.X,  0.5f * vecS.Y,  0.5f * vecS.Z) + vecOffset,
                new Vector3( 0.5f * vecS.X, -0.5f * vecS.Y, -0.5f * vecS.Z) + vecOffset,
                new Vector3( 0.5f * vecS.X, -0.5f * vecS.Y,  0.5f * vecS.Z) + vecOffset,
                new Vector3( 0.5f * vecS.X,  0.5f * vecS.Y, -0.5f * vecS.Z) + vecOffset,
                new Vector3( 0.5f * vecS.X,  0.5f * vecS.Y,  0.5f * vecS.Z) + vecOffset
            };

            // Front face
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[1], cubeVertices[3]);
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[3], cubeVertices[2]);

            // Back face
            oMesh.nAddTriangle(cubeVertices[4], cubeVertices[6], cubeVertices[7]);
            oMesh.nAddTriangle(cubeVertices[4], cubeVertices[7], cubeVertices[5]);

            // Left face
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[2], cubeVertices[6]);
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[6], cubeVertices[4]);

            // Right face
            oMesh.nAddTriangle(cubeVertices[1], cubeVertices[5], cubeVertices[7]);
            oMesh.nAddTriangle(cubeVertices[1], cubeVertices[7], cubeVertices[3]);

            // Top face
            oMesh.nAddTriangle(cubeVertices[2], cubeVertices[3], cubeVertices[7]);
            oMesh.nAddTriangle(cubeVertices[2], cubeVertices[7], cubeVertices[6]);

            // Bottom face
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[4], cubeVertices[5]);
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[5], cubeVertices[1]);

            return oMesh;
        }
            

        /// <summary>
        /// Creates a temporary folder with an arbitrary filename
        /// in the system's default temp directory, which is guaranteed to be
        /// writable (we don't check this, but the system should guarantee it)
        /// Use the "using" syntax to automatically cleanup after the object
        /// runs out of scope. It will clean up all the files inside and then
        /// delete the temporary folder.
        /// Note: It intentionally doesn't delete any subdirectories you may
        /// create. If you create subdirs, please clean them up yourself.
        /// We intentionally do not recursively wipe out everything out of an
        /// abundance of caution.
        ///
        /// Access the temp folder using oFolder.strFolder
        /// 
        /// </summary>
        public class TempFolder : IDisposable
        {
            public TempFolder()
            {
                strFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(strFolder);
            }

            public string strFolder;

            ~TempFolder()
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
                {
                    return;
                }

                try
                {
                    // We will cleanup all files, but will not cleanup subfolders
                    // this would require recursive delete, and I am afraid it
                    // could wipe out an entire disk, if used erroneously,
                    // however unlikely

                    string[] astrFiles = Directory.GetFiles(strFolder);
                    foreach (string strFile in astrFiles)
                    {
                        File.Delete(strFile);
                    }

                    // Remove the temp directory
                    Directory.Delete(strFolder);
                }
                catch (Exception)
                {
                    // Failed to cleanup, hopefully the system will do it for us at some point
                }

                m_bDisposed = true;
            }

            bool m_bDisposed = false;
        }
    }
}