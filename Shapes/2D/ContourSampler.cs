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

namespace PicoGK.Shapes
{
    /// <summary>
    /// Implements a way to adaptively sample a contour to retrieve
    /// a) the correct length
    /// b) sample the contour in adaptive arc T vs. linear T. this follows the
    /// contour exactly at constant speed
    /// </summary>
    public class ContourSampler2d
    {
        /// <summary>
        /// This interface enables a contour to be sampled in linear time 
        /// </summary>
        public interface ISampleable
        {
            /// <summary>
            /// Return the uncorrected position at linear t (uncorrected)
            /// </summary>
            Vector2 vecPtAtTLinear(float t);
        }

        /// <summary>
        /// Adaptively sample the contour to map the linear time to
        /// corrected arc-length t, for constant speed
        /// </summary>
        /// <param name="xContour">Contour to sample</param>
        /// <param name="fPrecision">Precision (distance between points)</param>
        /// <param name="nMaxDepth">Maximum recursion depth</param>
        public ContourSampler2d(    ISampleable xContour, 
                                    float   fPrecision  = 0.01f, 
                                    int     nMaxDepth   = 100)
        {
            AdaptiveSample( xContour, 
                            0f, 1f, 
                            fPrecision, 
                            nMaxDepth);

            m_fTotalLength = m_afArcLengths[^1];
        }

        /// <summary>
        /// Return sum of all arc segement lengths 
        /// </summary>
        public float fTotalLength => m_fTotalLength;

        /// <summary>
        /// Convert from linear t to arc-length t
        /// </summary>
        public float fArcTFromLinearT(float fLinearT)
        {
            fLinearT = Math.Clamp(fLinearT, 0, 1);
            float fTargetLength = fLinearT * m_fTotalLength;

            int nSegIndex = nFindSegmentForLength(fTargetLength);

            float fLen0 = nSegIndex == 0 ? 0f : m_afArcLengths[nSegIndex];
            float fLen1 = m_afArcLengths[nSegIndex + 1];

            float fLocalT = (fTargetLength - fLen0) / (fLen1 - fLen0);
            float t0 = m_afTs[nSegIndex];
            float t1 = m_afTs[nSegIndex + 1];

            return t0 + fLocalT * (t1 - t0);
        }

        void AdaptiveSample(    ISampleable xContour, 
                                float fT0, 
                                float fT1, 
                                float fPrecision, 
                                int nDepthRemaining)
        {
            Vector2 vec0 = xContour.vecPtAtTLinear(fT0);
            Vector2 vec1 = xContour.vecPtAtTLinear(fT1);

            float fChord = Vector2.Distance(vec0, vec1);

            float   fMidT   = (fT0 + fT1) * 0.5f;
            Vector2 vecMid  = xContour.vecPtAtTLinear(fMidT);

            float fFirstLeg  = Vector2.Distance(vec0, vecMid);
            float fSecondLeg = Vector2.Distance(vecMid, vec1);

            float fPolylineLength = fFirstLeg + fSecondLeg;

            bool bNeedsRefine = float.Abs(fPolylineLength - fChord) > fPrecision && nDepthRemaining > 0;

            if (bNeedsRefine)
            {
                AdaptiveSample( xContour, 
                                fT0, fMidT, 
                                fPrecision, 
                                nDepthRemaining - 1);

                AdaptiveSample( xContour, 
                                fMidT, 
                                fT1, 
                                fPrecision, 
                                nDepthRemaining - 1);
            }
            else
            {
                if (m_afArcLengths.Count == 0)
                {
                    m_afArcLengths.Add(0f);
                    m_afTs.Add(fT0);
                }

                float fLastLength = m_afArcLengths[^1];
                m_afArcLengths.Add(fLastLength + fPolylineLength);
                m_afTs.Add(fT1);
            }
        }

        int nFindSegmentForLength(float fLength)
        {
            int nLow = 0;
            int nHigh = m_afArcLengths.Count - 2;

            while (nLow <= nHigh)
            {
                int nMid = (nLow + nHigh) / 2;

                if (fLength < m_afArcLengths[nMid])
                    nHigh = nMid - 1;
                else if (fLength >= m_afArcLengths[nMid + 1])
                    nLow = nMid + 1;
                else
                    return nMid;
            }

            return int.Max(0, int.Min(m_afArcLengths.Count - 2, nLow));
        }

        readonly List<float>            m_afArcLengths = [];
        readonly List<float>            m_afTs         = [];
        readonly float                  m_fTotalLength;
    }
}