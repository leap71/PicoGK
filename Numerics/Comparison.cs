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

namespace PicoGK.Numerics
{
    /// <summary>
    /// Default tolerances for comparisons
    /// </summary>
    public static class Tolerances
    {
        /// <summary>
        /// Default tolerance for fuzzy comparisons.
        /// Chosen to be relatively universal for float precision.
        /// </summary>
        public const float fDef         = 1e-6f;

        /// <summary>
        /// Default squared tolerance for fuzzy comparisons.
        /// </summary>
        public const float fDefSquared  = fDef * fDef;

        /// <summary>
        /// Default number regarded as zero for fuzzy zero check
        /// Chosen to be relatively universal for float precision.
        /// </summary>
        public const float fZero         = 1e-8f;

        /// <summary>
        /// Default squared number regarded as zero for fuzzy zero check
        /// </summary>
        public const float fZeroSquared = fZero * fZero;
    }

    /// <summary>
    /// Extensions that allow for fuzzy comparisons of types
    /// </summary>
    public static class ComparisonExtensions
    {
        /// <summary>
        /// Fuzzy comparison function to determine equality between two floats
        /// Can be used like this: fValue.bAlmostEqual(fOtherValue)
        /// The optional parameter determines the tolerance used for comparison
        /// </summary>
        public static bool bAlmostEqual(    this float a,
                                            float b,
                                            float fAbsTol = Tolerances.fDef,
                                            float fRelTol = Tolerances.fDef)
        {
            if (a == b)
                return true;

            float fDiff = float.Abs(a - b);

            if (fDiff <= fAbsTol)
                return true;

            float fMaxAbs = float.Max(float.Abs(a), float.Abs(b));
            return fDiff <= fMaxAbs * fRelTol;
        }

        /// <summary>
        /// Fuzzy test for zero. Can be used like this: fValue.bAlmostZero()
        /// The optional parameter determines the tolerance used for comparison
        /// </summary>
        public static bool bAlmostZero( this float f,
                                        float fZero = Tolerances.fZero)
            => float.Abs(f) <= fZero;

        /// <summary>
        /// Fuzzy comparison function to determine equality
        /// between two vectors. Can be used like this: vecA.bAlmostEqual(vecB)
        /// The optional parameter determines the distance (squared!) used for comparison
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool bAlmostEqual(    this Vector3 a,
                                            Vector3 b,
                                            float fDistSquared = Tolerances.fDefSquared)
            => Vector3.DistanceSquared(a, b) <= fDistSquared;


        /// <summary>
        /// Fuzzy comparison function to determine whether the length of the vector
        /// is approximately 0. Can be used like this: vecA.bAlmostZero()
        /// The optional parameter determines the distance (squared!) used for comparison
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool bAlmostZero( this Vector3 vec,
                                        float fZeroSquared = Tolerances.fZeroSquared)
            => vec.LengthSquared() <= fZeroSquared;

        /// <summary>
        /// Fuzzy comparison function to determine equality
        /// between two vectors. Can be used like this: vecA.bAlmostEqual(vecB)
        /// The optional parameter determines the distance (squared) used for comparison
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool bAlmostEqual(    this Vector2 a,
                                            Vector2 b,
                                            float fDistSquared = Tolerances.fDefSquared)
            => Vector2.DistanceSquared(a, b) <= fDistSquared;

        /// <summary>
        /// Fuzzy comparison function to determine whether the length of the vector
        /// is approximately 0. Can be used like this: vecA.bAlmostZero()
        /// The optional parameter determines the distance (squared!) used for comparison
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool bAlmostZero( this Vector2 vec,
                                        float fZeroSquared = Tolerances.fZeroSquared)
            => vec.LengthSquared() <= fZeroSquared;
    }
}