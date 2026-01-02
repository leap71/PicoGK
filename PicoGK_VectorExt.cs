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
using System.Numerics;

namespace PicoGK
{
    public static class Vector3Ext
	{
        static readonly float fSigma = 1e6f;
        /// <summary>
        /// Returns the normalized version of the vector
        /// If too small to normalize, returns Vector3.Zero
        /// </summary>
        /// 
        public static Vector3 vecNormalized(this Vector3 vec)
        {
            return Vector3.Normalize(vec);
        }
        /// <summary>
        /// Returns a mirrored version of the vector.
        /// </summary>
        /// <param name="vec">The vector to be mirrored (this).</param>
        /// <param name="vecPlanePoint">A point through which the mirror plane passes.</param>
        /// <param name="vecPlaneNormalUnitVector">The normal vector of the mirror plane,
        /// expected to be a unit vector.</param>
        /// <returns>The mirrored vector.</returns>
		public static Vector3 vecMirrored(	this Vector3 vec,
											Vector3 vecPlanePoint,
											Vector3 vecPlaneNormalUnitVector)
		{
            // expecting normal to be unit vector

            Debug.Assert(float.Abs(vecPlaneNormalUnitVector.Length()-1) < fSigma);

			return vec - 2 * Vector3.Dot(   vec - vecPlanePoint, 
                                            vecPlaneNormalUnitVector) * vecPlaneNormalUnitVector;
		}

        /// <summary>
        /// Returns a transformed version of the vector.
        /// </summary>
        /// <param name="vec">The vector to be mirrored (this).</param>
        /// <param name="mat">The matrix to be applied to transform the vector.</param>
        /// <returns>The transformed vector.</returns>
		public static Vector3 vecTransformed(	this Vector3 vec,
												Matrix4x4 mat)
		{
			return Vector3.Transform(vec, mat);
		}
	}
}
