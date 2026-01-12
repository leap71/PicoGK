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

using System.Diagnostics;
using System.Globalization;

namespace PicoGK
{
    /// <summary>
    /// A generic progress reporting interface
    /// </summary>
    public interface IProgress
    {
        /// <summary>
        /// Report progress from 0..1
        /// </summary>
        void Progress(float f);
    }

    /// <summary>
    /// A progress reporting class that does nothing (can be used as default)
    /// </summary>
    public class ProgressNoop : IProgress
    {
        /// <summary>
        /// Progress from 0..1
        /// </summary>
        public void Progress(float f)
        {
            
        }
    }

    /// <summary>
    /// A progress reporting class that outputs to a log interface
    /// </summary>
    public class LogProgress : IProgress, IDisposable
    {
        /// <summary>
        /// Initialize a new progress reporting object
        /// </summary>
        /// <param name="xLog">Log interface to output to</param>
        /// <param name="strInfo">Identifying string</param>
        /// <param name="fIntervalSeconds">Minimum interval to leave between reporting entries</param>
        public LogProgress( ILog xLog,
                            string strInfo = "Progress",
                            float fIntervalSeconds = 1f)
        {
            m_xLog      = xLog;
            m_strInfo   = strInfo;
            m_fInterval = fIntervalSeconds;
            m_oWatch    = new();
            m_fPrevTime = 0f; // force first log entry
            m_oWatch.Start();
        }

        /// <summary>
        /// Report progress from 0..1
        /// </summary>
        public void Progress(float f)
        {
            f = float.Clamp(f, 0, 1);
            float fCurrentTime = m_oWatch.ElapsedMilliseconds / 1000f;
            if ((fCurrentTime - m_fPrevTime) > m_fInterval)
            {
                m_fPrevTime = fCurrentTime;
                m_xLog.Log($"[{m_strInfo}] {f*100:F1}% complete ");
            }
        }

        /// <summary>
        /// Cleanup (just reports that the task is finished)
        /// </summary>
        public void Dispose()
        {
            m_xLog.Log($"[{m_strInfo}] (finished)");
        }

        ILog m_xLog;
        string m_strInfo;
        float m_fInterval;
        Stopwatch m_oWatch;
        float m_fPrevTime;
    }

    /// <summary>
    /// A progress counting class for counting up items to 100%
    /// </summary>
    public class ProgressCounter
    {
        /// <summary>
        /// Create a new progress counter object
        /// </summary>
        /// <param name="xProgress">Progress reporting interface to use</param>
        /// <param name="nItemCount">Number of items representing 100%</param>
        public ProgressCounter( IProgress xProgress,
                                int nItemCount)
        {
            m_xProgress     = xProgress;
            m_nItemCount    = nItemCount;
            
        }

        /// <summary>
        /// Set the item (nItemCount == 100%)
        /// </summary>
        public void SetItem(int nItem)
        {
            if (m_nItemCount <= 0)
                return;

            m_nCurrent = nItem;
            m_xProgress.Progress(m_nCurrent / (float) m_nItemCount);
        }

        /// <summary>
        /// Allow you to use ++ to count up to the next item
        /// </summary>
        public static ProgressCounter operator ++(ProgressCounter pc)
        {
            pc.SetItem(pc.m_nCurrent + 1);
            return pc;
        }

        IProgress   m_xProgress;
        int m_nItemCount;
        int m_nCurrent;
    }

    /// <summary>
    /// This class allows you to split progress reporting into multiple
    /// subtasks. The object reports progress from 0..1, but allows each
    /// subtask to individually progress from 0..1
    /// </summary>
    public class SplitProgress : IProgress
    {
        /// <summary>
        /// Create a new SplitProgress object
        /// </summary>
        /// <param name="xProgress">Progress reporting interface to use</param>
        /// <param name="nSubTasks">Number of subtasks, each with their independet 0..1 progress</param>
        public SplitProgress(   IProgress xProgress, 
                                int nSubTasks)
        {
            m_xProgress = xProgress;
            m_nSubTasks = nSubTasks <= 0 ? 1 : nSubTasks;
        }

        /// <summary>
        /// Report progress from 0..1 - this function automatically
        /// scales the value to reflect the current subtask
        /// </summary>
        public void Progress(float f)
        {
            f = float.Clamp(f, 0f, 1f);
            int nSub = m_nCurrent;

            if (nSub < 0) 
                nSub = 0;

            if (nSub >= m_nSubTasks)
            {
                m_xProgress.Progress(1.0f);
                return;
            }

            m_xProgress.Progress((nSub + f) / (float)m_nSubTasks);
        }

        /// <summary>
        /// Set current subtask (0..m_nSubTasks-1)
        /// </summary>
        /// <param name="nSub"></param>
        void SetSubTask(int nSub)
        {
            m_nCurrent = nSub;
            Progress(0); // move onto next
        }

        /// <summary>
        /// Allow you to use ++ to count up to the next subtask
        /// </summary>
        public static SplitProgress operator ++(SplitProgress pc)
        {
            pc.SetSubTask(pc.m_nCurrent + 1);
            return pc;
        }

        IProgress m_xProgress;
        int m_nSubTasks;
        int m_nCurrent;
    }
}