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
using PicoGK.Numerics;

namespace PicoGK.Shapes
{
    /// <summary>
    /// A 2d line
    /// </summary>
    public readonly struct Line2d : IPath2d
    {   
        /// <summary>
        /// Construct a line with the specified start and end coordinates
        /// </summary>
        public Line2d(  Vector2 vecA,
                        Vector2 vecB)
        {
            if (!vecA.bIsFinite())
                throw new ArgumentException("Start point must be finite.", nameof(vecA));

            if (!vecB.bIsFinite())
                throw new ArgumentException("End point must be finite.", nameof(vecB));

            m_vecA      = vecA;
            m_vecB      = vecB;
            m_fLength   = (m_vecB - m_vecA).Length(); 
        }

        /// <summary>
        ///  Start coordinate
        /// </summary>
        public Vector2  vecA    => m_vecA;

        /// <summary>
        /// End coordinate
        /// </summary>
        public Vector2  vecB    => m_vecB;

        public Vector2 vecPtAtT(float fT)
        {
            if (fT<=0)
                return m_vecA;

            if (fT>=1)
                return m_vecB;

            return Vector2.Lerp(m_vecA, m_vecB, fT);
        }

        public float fLength    => m_fLength;

        readonly Vector2 m_vecA;
        readonly Vector2 m_vecB;
        readonly float m_fLength;
    }

    /// <summary>
    /// A circular arc in 2D space
    /// </summary>
    public readonly struct Arc2d : IPath2d
    {
        /// <summary>
        /// Construct a new 2D arc with the specified start point,
        /// around the specified center and with the supplied angle in radians
        /// </summary>
        public Arc2d(   Vector2 vecStart,
                        Vector2 vecCenter,
                        Rad     rAngle)
        {
            if (!vecStart.bIsFinite())
                throw new ArgumentException("Start point must be finite.", nameof(vecStart));

            if (!vecCenter.bIsFinite())
                throw new ArgumentException("Center point must be finite.", nameof(vecCenter));

            if (!rAngle.bIsFinite())
                throw new ArgumentException("Angle must be finite.", nameof(rAngle));

            m_vecStart  = vecStart;
            m_vecCenter = vecCenter;
            m_rAngle    = rAngle;

            m_vec0      = m_vecStart - m_vecCenter;
            m_fRadius   = m_vec0.Length();

            // Handle degenerate radius (start == center)
            if (m_fRadius.bAlmostZero())
            {
                m_bDegenerate   = true;
                m_vecEnd        = m_vecStart;
                m_fLength       = 0f;
                return;
            }

            m_bDegenerate = false;
            
            // Rotate start-around-center by m_rAngle to get end
            float c = m_rAngle.fCos();
            float s = m_rAngle.fSin();
            Vector2 v1 = new (m_vec0.X * c - m_vec0.Y * s, m_vec0.X * s + m_vec0.Y * c);

            m_vecEnd  = m_vecCenter + v1;
            m_fLength = m_fRadius * float.Abs(m_rAngle.fRad);
        }

        /// <summary>
        /// Start coordinate
        /// </summary>
        public Vector2 vecStart     => m_vecStart;

        /// <summary>
        /// End coordinate
        /// </summary>
        public Vector2 vecEnd       => m_vecEnd;

        /// <summary>
        /// Center point
        /// </summary>
        public Vector2 vecCenter    => m_vecCenter;

        /// <summary>
        /// Angle in radians (positive is counter clockwise)
        /// </summary>
        public Rad rAngle           => m_rAngle;

        /// <summary>
        /// Radius of the arc
        /// </summary>
        public float fRadius        => m_fRadius;
        
        public float fLength        => m_fLength;

        public Vector2 vecPtAtT(float fT)
        {
            if (fT <= 0f)
                return m_vecStart;

            if (fT >= 1f)
                return m_vecEnd;

            if (m_bDegenerate)
                return m_vecStart;

            Rad rAng = m_rAngle * fT;
            float c = rAng.fCos();
            float s = rAng.fSin();

            Vector2 vt = new (m_vec0.X * c - m_vec0.Y * s, m_vec0.X * s + m_vec0.Y * c);
            return m_vecCenter + vt;
        }

        readonly Vector2    m_vecStart;
        readonly Vector2    m_vecEnd;
        readonly Vector2    m_vecCenter;
        readonly float      m_fRadius;
        readonly Rad        m_rAngle;
        readonly float      m_fLength;

        readonly bool       m_bDegenerate;

        readonly Vector2    m_vec0;
    } 

    /// <summary>
    /// A compound path which consists of a list of other paths
    /// </summary>
    public sealed class Path2d : IPath2d
    {
        /// <summary>
        /// Add another path to the compound path
        /// Note, the start coordinate of the added path needs to be
        /// coincide with the current end point
        /// </summary>
        public void Add(IPath2d xPath)
        {
            if (m_axPaths.Count > 0)
            {
                if (!m_axPaths[^1].vecPtAtT(1f).bAlmostEqual(xPath.vecPtAtT(0f)))
                    throw new ArgumentException(
                        "Added path must begin at the end of the previous path.",
                        nameof(xPath));
            }

            m_axPaths.Add(xPath);
            m_fLength += xPath.fLength;
        }

        /// <summary>
        /// Append a line to the specified coordinate
        /// </summary>
        public void AddLine(Vector2 vecTo)
        {
            Line2d oLine = new(vecPtAtT(1), vecTo);
            Add(oLine);
        }

        /// <summary>
        /// Append a line relative to current end point
        /// </summary>
        public void AddLineRel(Vector2 vecRel)
        {
            Vector2 vecStart = vecPtAtT(1);
            Line2d oLine = new(vecStart, vecStart + vecRel);
            Add(oLine);
        }

        /// <summary>
        /// Append an arc with the specified center and angle
        /// The start coordinate is the current end coordinate
        /// </summary>
        public void AddArc(Vector2 vecCenter, Rad rAngle)
        {
            Arc2d oArc = new(vecPtAtT(1), vecCenter, rAngle);
            Add(oArc);
        }

        /// <summary>
        /// Add an arc with the specified center, relative to the
        /// current end point
        /// The start coordinate is the current end coordinate
        /// </summary>
        public void AddArcRel(Vector2 vecCenterRel, Rad rAngle)
        {
            Vector2 vecStart = vecPtAtT(1);
            Arc2d oArc = new(vecStart, vecStart + vecCenterRel, rAngle);
            Add(oArc);
        }

        public Vector2 vecPtAtT(float fT)
        {
            if (m_axPaths.Count == 0)
                return Vector2.Zero;

            fT = float.Clamp(fT, 0f, 1f);

            float fCurrent = fT * m_fLength;
            Vector2 vecLast = m_axPaths[0].vecPtAtT(0f);

            foreach (IPath2d xPath in m_axPaths)
            {
                if (xPath.fLength <= 0f)
                {
                    vecLast = xPath.vecPtAtT(1f);
                    continue;
                }

                if (fCurrent <= xPath.fLength)
                {
                    float fLocalT = fCurrent / xPath.fLength;
                    return xPath.vecPtAtT(fLocalT);
                }

                fCurrent -= xPath.fLength;
                vecLast = xPath.vecPtAtT(1f);
            }

            return vecLast;
        }

        public float fLength    => m_fLength;
        
        readonly List<IPath2d> m_axPaths = [];
        float m_fLength = 0; 
    }
}