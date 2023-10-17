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
using System.Runtime.InteropServices;

namespace PicoGK
{
    public class LogFile : IDisposable
    {
        public LogFile(in string strFileName = "")
        {
            string strFile = strFileName;

            if (strFile == "")
            {
                strFile = Path.Combine( Utils.strDocumentsFolder(),
                                        Utils.strDateTimeFilename("PicoGK_", ".log"));
            }

            m_oWriter = new StreamWriter(strFile, false);

            if (m_oWriter is null)
                throw new FileNotFoundException("Unable to create file " + strFile);

            m_oStopwatch = new Stopwatch();
            m_oStopwatch.Start();

            m_fTimeStartSeconds = m_oStopwatch.ElapsedMilliseconds / 1000.0f;
            m_fLastTimeSeconds = 0.0f;

            Log("Opened " + strFile);
            Log("\n----------------------------------------\n");
            LogTime();
            Log("\n----------------------------------------\n");
            Log("System Info:\n");
            Log("Machine Name:         {0}", Environment.MachineName);
            Log("Operating System      {0}", RuntimeInformation.OSDescription);
            Log("Version:              {0}", Environment.OSVersion);
            Log("OS Architecture:      {0}", RuntimeInformation.OSArchitecture);
            Log("Proc Architecture:    {0}", RuntimeInformation.ProcessArchitecture);
            Log("64 Bit OS:            {0}", Environment.Is64BitOperatingSystem ? "Yes" : "No");
            Log("64 Bit Process:       {0}", Environment.Is64BitProcess ? "Yes" : "No");
            Log("Processor Count:      {0}", Environment.ProcessorCount);
            Log("Working Set:          {0}MB", Environment.WorkingSet / 1024 / 1024);
            Log("C# Framework:         {0}", RuntimeInformation.FrameworkDescription);
            Log("C# CLR Version:       {0}", Environment.Version);
            Log("PicoGK Path:          {0}", Config.strPicoGKLib);
            Log("Command Line:         {0}", Environment.CommandLine);

            Log("\n----------------------------------------\n");
        }

        public void Log(in string strFormat,
                            params object[] args)
        {

            float fSeconds = (m_oStopwatch.ElapsedMilliseconds / 1000.0f) - m_fTimeStartSeconds;
            float fDiff = fSeconds - m_fLastTimeSeconds;

            string strPrefix = string.Format("{0,7:0.}s ", fSeconds)
                                + string.Format("{0,6:0.0}+ ", fDiff);

            string[] lines = string.Format(strFormat, args).Split(new char[] { '\n' });

            lock (m_oMtx)
            {
                foreach (string str in lines)
                {
                    Console.WriteLine(strPrefix + str);
                    m_oWriter?.WriteLine(strPrefix + str);

                    m_oWriter?.Flush();
                    m_fLastTimeSeconds = fSeconds;
                }
            }
        }

        public void LogTime()
        {
            Log("Current time (UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss (UTC)"));
            Log("Current local time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss (zzz)"));
        }

        ~LogFile()
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

            if (bDisposing)
            {
                lock (m_oMtx)
                {
                    // Release managed resources (e.g., close the file).
                    if (m_oWriter != null)
                    {
                        Log("\n----------------------------------------\n");
                        Log("Closing log file.");
                        LogTime();
                        Log("Done.");
                        m_oWriter.Dispose();
                        m_oWriter = null;
                    }
                }
            }

            m_bDisposed = true;
        }

        object m_oMtx = new object();
        StreamWriter? m_oWriter = null;
        Stopwatch m_oStopwatch;
        float m_fTimeStartSeconds;
        float m_fLastTimeSeconds;
        bool m_bDisposed = false;

    }
} // namespace PicoGK

