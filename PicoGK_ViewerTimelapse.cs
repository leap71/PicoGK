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

namespace PicoGK
{
    public partial class Viewer
    {
        public void StartTimeLapse( float   fIntervalInMilliseconds,
                                    string  strPath,
                                    string  strFileName = "frame_",
                                    uint    nStartFrame = 0,
                                    bool    bPaused = false)
        {
            lock (m_oTLLock)
            {
                m_oTimeLapse = new( fIntervalInMilliseconds,
                                    strPath,
                                    strFileName,
                                    nStartFrame,
                                    bPaused);      
            }
        }

        public void PauseTimeLapse()
        {
            lock (m_oTLLock)
            {
                if (m_oTimeLapse != null)
                    m_oTimeLapse.Pause();
            }
        }

        public void ResumeTimeLapse()
        {
            lock (m_oTLLock)
            {
                if (m_oTimeLapse != null)
                    m_oTimeLapse.Resume();
            }
        }

        public void StopTimeLapse()
        {
            lock (m_oTLLock)
            {
                m_oTimeLapse = null;
            }
        }

        object      m_oTLLock       = new();
        TimeLapse?  m_oTimeLapse    = null;

        class TimeLapse
        {
            public TimeLapse(   float   fIntervalInMilliseconds,
                                string  strPath,
                                string  strFileName,
                                uint    nStartFrame,
                                bool    bPaused = false)
            {
                m_fInterval     = fIntervalInMilliseconds;
                m_strPath       = strPath;
                m_strFileName   = strFileName;
                m_nCurrentFrame = nStartFrame;
                m_bPaused       = bPaused;
                
                m_oStopwatch.Start();
                UpdateInterval();
            }

            public void Pause()
            {
                m_bPaused = true;
            }

            public void Resume()
            {
                m_bPaused = false;
                UpdateInterval();
            }

            public bool bDue(out string strFramePath)
            {
                if (!m_bPaused)
                {
                    if (m_oStopwatch.ElapsedMilliseconds >= m_fNextTime)
                    {
                        strFramePath = Path.Combine(m_strPath, m_strFileName + m_nCurrentFrame.ToString("00000") + ".tga");
                        m_nCurrentFrame++;
                        UpdateInterval();
                        return true;
                    }
                }

                strFramePath = "";
                return false;
            }

            void UpdateInterval()
            {
                m_fNextTime = m_oStopwatch.ElapsedMilliseconds + m_fInterval;
            }

            float   m_fInterval;
            float   m_fNextTime;

            string  m_strPath;
            string  m_strFileName;
            uint    m_nCurrentFrame;
            bool    m_bPaused;

            Stopwatch m_oStopwatch = new();
        }
    }
}
    