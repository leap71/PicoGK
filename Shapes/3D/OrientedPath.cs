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
    /// Interface to represent a normalized 2D path oriented in space
    /// which travels from 0..1
    /// - Position at 0 and 1 are identical
    /// - The contour is centered around the coordinate 0/0
    /// - The contour is in counter-clockwise winding order
    /// </summary>
    public sealed class OrientedPath : IPath3d
    {
        public OrientedPath(    IPath2d xPath,
                                Frame3d frm)
        {
            m_xPath = xPath;
            m_frm   = frm;
        }
        public float fLength 
            => m_xPath.fLength;

        public Vector3 vecPtAtT(float t)
            => m_frm.vecPtToWorld(m_xPath.vecPtAtT(t));

        readonly IPath2d m_xPath;
        readonly Frame3d m_frm;
    }

    /// <summary>
    /// Represents an oriented 2D contour placed in 3D space by a Frame3d
    /// </summary>
    public sealed class OrientedContour : IContour3d
    {
        /// <summary>
        /// Create an oriented contour from a 2D contour and a local coordinate system
        /// </summary>
        public OrientedContour( IContour2d xContour,
                                Frame3d frm)
        {
            m_xContour = xContour;
            m_frm = frm;
        }

        public float fLength 
            => m_xContour.fLength;

        public Vector3 vecPtAtT(float t)
            => m_frm.vecPtToWorld(m_xContour.vecPtAtT(t));

        public void PtAtT(float t, out Vector3 vecPt, out Vector3 vecNormal)
        {
            m_xContour.PtAtT(t, out Vector2 vecPt2d, out Vector2 vecNormal2d);
            vecPt       = m_frm.vecPtToWorld(vecPt2d);
            vecNormal   = m_frm.vecDirToWorld(vecNormal2d);
        }

        readonly Frame3d     m_frm;
        readonly IContour2d m_xContour;
    }
}

   