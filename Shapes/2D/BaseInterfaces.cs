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
    public interface IPath2d
    {
        /// <summary>
        /// Returns the point at position t (0..1)
        /// As t increases monotonically, the point moves along the contour at
        /// constant speed with respect to arc length
        /// </summary>
        //
        // IMPLEMENTATION NOTE: t is expected to be arc-length reparameterized.
        // If you implement this interface for complex curves you can use 
        // ContourSampler2d and fArcTFromLinearT to convert to constant speed.
        // 
        public Vector2 vecPtAtT(float t);

        /// <summary>
        /// Length of the entire contour
        /// </summary>
        //
        // IMPLEMENTATION NOTE: For complex curves you may have to determine
        // the length numerically. ContourSampler2d allows you to do this.
        //
        public float fLength {get;}
    }

    /// <summary>
    /// Interface to represent a normalized closed contour in 2D
    /// which travels from 0..1
    /// - Position at 0 and 1 are identical
    /// - The contour is centered around the coordinate 0/0
    /// - The contour is in counter-clockwise winding order
    /// </summary>
    public interface IContour2d : IPath2d
    {
        /// <summary>
        /// Function to return both point and normal at t
        /// </summary>
        //
        // IMPLEMENTATION NOTE: If your class has an analytical
        // way to calculate the normal, implement this function to
        // calculate the normal mathematically, to avoid sampling.
        //
        public void PtAtT(  in  float t,
                            out Vector2 vecPt,
                            out Vector2 vecNormal)
        {
            vecPt = vecPtAtT(t);
            vecNormal = vecSampleNormalAt(t);
        }

        /// <summary>
        /// Sample the normal at fT
        /// Helper function used by PtAtT
        /// </summary>
        public Vector2 vecSampleNormalAt(   float fT,
                                            float fSampleDist = 1e-5f)
        {
            fT = float.Clamp(fT, 0f, 1f);
            float fTSample = (fT < (1f-fSampleDist)) ? fT + fSampleDist : fT + fSampleDist;
            
            Vector2 vecTangent = vecPtAtT(fTSample)-vecPtAtT(fT);
            return Vector2.Normalize(new Vector2(-vecTangent.Y, vecTangent.X));
        }
    }
}