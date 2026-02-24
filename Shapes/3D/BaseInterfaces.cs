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
    /// Interface to represent a normalized path in 2D space
    /// which travels from 0..1
    /// </summary>
    public interface IPath3d
    {
        /// <summary>
        /// Returns the point at position t (0..1)
        /// As t increases monotonically, the point moves along the contour at
        /// constant speed with respect to arc length
        /// </summary>
        public Vector3 vecPtAtT(float t);

        /// <summary>
        /// Length of the entire contour
        /// </summary>
        public float fLength {get;}
    }

    /// <summary>
    /// A two dimensional closed contour aligned in a plane in 3D space
    /// Note: While a IPath3d can contain random segments in space, a
    /// contour needs to have all its points aligned in one plane
    /// The point at 0 and 1 are identical (closed contour)
    /// </summary>
    public interface IContour3d : IPath3d
    {
        /// <summary>
        /// Returns the point and normal at position t (0..1)
        /// As t increases monotonically, the point moves along the contour at
        /// constant speed with respect to arc length
        /// </summary>
        public void PtAtT(  float t,
                            out Vector3 vecPt,
                            out Vector3 vecNormal);
    }
}