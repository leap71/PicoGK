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

namespace PicoGK
{
    public class Animation
    {
        public interface IAction
        {
            // time from 0..1
            void Do(float fTime);
        }

        public enum EType   { Once, Repeat, Wiggle };

        public Animation(   IAction         xAction,
                            float           fDurationInSeconds,
                            EType           eType,
                            Easing.EEasing  eEasing)
        {
            m_xAction = xAction;
            m_fDuration = fDurationInSeconds;
            m_eType = eType;
            m_eEasing = eEasing;
        }

        public void End()
        {
            m_xAction.Do(1.0f);
        }

        public bool bAnimate(float fCurrentTime)
        {
            if (m_fStartTime == 0.0f)
            {
                // Start
                m_xAction.Do(0.0f);
                m_fStartTime = fCurrentTime;
                return true;
            }

            if ((fCurrentTime - m_fStartTime) >= m_fDuration)
            {
                // At end

                m_xAction.Do(1.0f);

                if (m_eType == EType.Once)
                    return false; // We are done

                if (m_eType == EType.Wiggle)
                    m_bReverse = !m_bReverse;

                if ((fCurrentTime - m_fStartTime) > m_fDuration)
                    m_fStartTime += m_fDuration;

                return true; // Go on
            }

            // in the midst

            float fPos = (fCurrentTime - m_fStartTime) / m_fDuration;

            if (m_bReverse)
                fPos = 1.0f - fPos;

            float fInterpolated = Easing.fEasingFunction(fPos, m_eEasing);

            m_xAction.Do(fInterpolated);
            
            return true;
        }

        float m_fStartTime = 0.0f;
        bool m_bReverse = false;

        IAction         m_xAction;
        float           m_fDuration;
        EType           m_eType;
        Easing.EEasing  m_eEasing;
    }

    public class AnimationQueue
    {
        public AnimationQueue()
        {
            m_oWatch.Start();
        }

        public void Clear()
        {
            lock (m_oAnimations)
            {
                foreach (Animation anim in m_oAnimations)
                {
                    anim.End();
                }

                m_oAnimations.Clear();
            }
        }

        public bool bPulse()
        {
            float fCurrentTimeSeconds = m_oWatch.ElapsedMilliseconds / 1000.0f;
            bool bUpdateNeeded = false;

            lock (m_oAnimations)
            {
                List<Animation> oToRemove = new();

                foreach (Animation anim in m_oAnimations)
                {
                    bUpdateNeeded = true;
                    if (!anim.bAnimate(fCurrentTimeSeconds))
                        oToRemove.Add(anim);

                    m_fLastActionTime = fCurrentTimeSeconds;
                }

                foreach (Animation anim in oToRemove)
                {
                    bUpdateNeeded = true;

                    if (!anim.bAnimate(fCurrentTimeSeconds))
                        m_oAnimations.Remove(anim);
                }
            }

            return bUpdateNeeded;
        }

        public bool bIsIdle()
        {
            float fIdleSeconds = (m_oWatch.ElapsedMilliseconds / 1000.0f) - m_fLastActionTime;
            return (fIdleSeconds > m_fIdleTime);
        }

        public void Add(Animation oAnim)
        {
            lock (m_oAnimations)
            {
                m_oAnimations.Add(oAnim);
            }
        }

        Stopwatch       m_oWatch            = new Stopwatch();
        float           m_fLastActionTime   = 0.0f;
        float           m_fIdleTime         = 5.0f;
        List<Animation> m_oAnimations       = new List<Animation>();
    }
}