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
using System.Runtime.CompilerServices;
using PicoGK.Shapes;

namespace PicoGK.Numerics
{
    public static class VectorExt
    {
        /// <summary>
        /// Returns the normalized version of this vector (length 1)
        /// Can be used like this vec = vec.vecNormalized();
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the vector length is zero or almost zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 vecNormalized(this Vector3 vec)
        {
            if (vec.Length().bAlmostZero())
                throw new ArgumentException(
                    "Cannot normalize a zero-length vector.",
                    nameof(vec));

            return Vector3.Normalize(vec);
        }

        /// <summary>
        /// Returns the normalized version of this vector (length 1)
        /// Returns (0,0,0) if supplied vector length is 0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 vecSafeNormalized(this Vector3 vec)
        {
            if (vec.Length().bAlmostZero())
                return Vector3.Zero;

            return Vector3.Normalize(vec);
        }

        /// <summary>
        /// Returns the normalized version of this vector (length 1)
        /// Can be used like this vec = vec.vecNormalized();
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the vector length is zero or almost zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 vecNormalized(this Vector2 vec)
        {
            if (vec.Length().bAlmostZero())
                throw new ArgumentException(
                    "Cannot normalize a zero-length vector.",
                    nameof(vec));

            return Vector2.Normalize(vec);
        }

        /// <summary>
        /// Returns the normalized version of this vector (length 1)
        /// Returns (0,0) if supplied vector length is 0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 vecSafeNormalized(this Vector2 vec)
        {
            if (vec.Length().bAlmostZero())
                return Vector2.Zero;

            return Vector2.Normalize(vec);
        }

        /// <summary>
        /// Converts a Vector3 into a Vector2 by stripping the Z coordinate
        /// Can be used like this Vector2 vec2 = vec3.vecStripZ();
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 vecStripZ(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }

        /// <summary>
        /// Converts a Vector2 into a Vector3 by adding a Z coordinate (defaults to 0)
        /// Can be used like this: Vector3 vec3 = vec2.vecAsVector3();
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="fZ"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 vecAsVector3(this Vector2 vec, float fZ=0.0f)
        {
            return new Vector3(vec.X, vec.Y, fZ);
        }

        /// <summary>
        /// Helper function to convert a point to world coordinates using a supplied frame.
        /// Usage: Vector3 vecWorld = vecLocal.vecPtWorld(frm);
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 vecPtWorld(this Vector3 vec, Frame3d frm)
        {
            return frm.vecPtToWorld(vec);
        }

        /// <summary>
        /// Helper function to convert a direction to world coordinates using a supplied frame.
        /// Usage: Vector3 vecWorld = vecLocal.vecDirWorld(frm);
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 vecDirWorld(this Vector3 vec, Frame3d frm)
        {
            return frm.vecDirToWorld(vec);
        }

        /// <summary>
        /// Helper function to convert a point to local coordinates using a supplied frame.
        /// Usage: Vector3 vecLocal = vecWorld.vecPtLocal(frm);
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 vecPtLocal(this Vector3 vec, Frame3d frm)
        {
            return frm.vecPtFromWorld(vec);
        }

        /// <summary>
        /// Helper function to convert a direction to local coordinates using a supplied frame.
        /// Usage: Vector3 vecLocal = vecWorld.vecDirLocal(frm);
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 vecDirLocal(this Vector3 vec, Frame3d frm)
        {
            return frm.vecDirFromWorld(vec);
        }
    }
}