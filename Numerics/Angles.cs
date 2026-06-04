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

using System.Globalization;
using System.Runtime.CompilerServices;

namespace PicoGK.Numerics
{    
    /// <summary>
    /// This class implements helper functions and constants relevant to angles in Radians
    /// </summary>
    public static class Rad
    {
        /// <summary>
        /// Defines 2*Pi, which is constantly being used in Rad angles
        /// </summary>
        public const float TwoPi = float.Pi * 2f;

        /// <summary>
        /// Ensure the passed angles is in the range [-π, +π]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float fNormalizedAngleRad(this float fAngle)
        {
            // Returns angle in the range [-π, +π]
            float fNormalized = MathF.IEEERemainder(fAngle, 2f * float.Pi);

            // Canonicalize negative zero
            if (fNormalized == 0f)
                return 0f;

            return fNormalized;
        }
    }

    public readonly struct Overhang :   IComparable<Overhang>,
                                        IEquatable<Overhang>
    {
        /// <summary>
        /// No overhang (0%)
        /// </summary>
        public static Overhang uNone => new(0f);

        /// <summary>
        /// Maximum overhang (100%)
        /// </summary>
        public static Overhang uFull => new(1f);

        /// <summary>
        /// Normalized overhang severity from 0..1 
        /// - 0.0: No overhang, vertical, self-supporting (0%)
        /// - 0.5: 45º overhang
        /// - 1.0: Maximum overhang, horizontal (100%)
        /// </summary>
        public float fNormalized 
            => m_fNormalized;
        
        /// <summary>
        /// Normalized overhang severity from 0..100%
        /// - 0:    No overhang, vertical, self-supporting (0%)
        /// - 50:   45º overhang
        /// - 100:  Maximum overhang, horizontal (100%)
        /// </summary>
        public float fPercent 
            => m_fNormalized * 100.0f;

        /// <summary>
        /// Overhang angle in radians
        /// - 0:    No overhang, vertical, self-supporting (0%)
        /// - Pi/4: 45º overhang
        /// - Pi/2: Maximum overhang, horizontal (100%)  
        /// </summary>
        public float fRad 
            => m_fNormalized * float.Pi / 2f;

        /// <summary>
        /// Overhang angle in degrees
        /// - 0:    No overhang, vertical, self-supporting (0%)
        /// - 45º:  45º overhang
        /// - 90º:  Maximum overhang, horizontal (100%)  
        /// </summary>
        public float fDeg 
            => m_fNormalized * 90f;

        /// <summary>
        /// Overhang angle in degrees, measured from the horizontal plane
        /// Used by some 3D printing manufacturers. Avoid, unless exchanging
        /// information with a specific vendor.
        /// - 0:    100% (maximum) overhang, horizontal feature requiring support
        /// - 45º:  45º overhang
        /// - 90º:  No overhang, vertical feature, self-supporting (0%)  
        /// </summary>
        public float fDegFromHorizontal 
            => 90f - (m_fNormalized * 90f);

        /// <summary>
        /// Create a new Overhang, using normalized overhang severity from 0..1 
        /// - 0.0: No overhang, vertical, self-supporting (0%)
        /// - 0.5: 45º overhang
        /// - 1.0: Maximum overhang, horizontal (100%)
        /// </summary>
        public static Overhang uFromNormalized(float f)
        {
            if (!float.IsFinite(f) || f < 0f || f > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(f), f, 
                    "Normalized overhang must be in the range 0..1");
            }
            
            return new(f);
        }

        /// <summary>
        /// Create a new Overhang, based on percent value (0..100)
        /// - 0:   No overhang, vertical, self-supporting (0%)
        /// - 50:  45º overhang
        /// - 100: Maximum overhang, horizontal (100%)
        /// </summary>
        public static Overhang uFromPercent(float f)
        {
            if (!float.IsFinite(f) || f < 0f || f > 100f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(f), f, 
                    "Overhang percentage must be in the range 0..100");
            }

            return new(f/100f);
        }

        /// <summary>
        /// Create a new Overhang, based on radians value (0..Pi/2)
        /// - 0:    No overhang, vertical, self-supporting (0%)
        /// - Pi/4: 45º overhang
        /// - Pi/2: Maximum overhang, horizontal (100%)  
        /// </summary>
        public static Overhang uFromRad(float f)
        {
            if (!float.IsFinite(f) || f < 0f || f > float.Pi / 2f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(f), f, 
                    "Overhang angle in rad must be in the range 0..Pi/2");
            }

            return new(f/(float.Pi/2f));
        }

        /// <summary>
        /// Create a new Overhang, based on degrees value (0..90)
        /// - 0:    No overhang, vertical, self-supporting (0%)
        /// - 45º:  45º overhang
        /// - 90º:  Maximum overhang, horizontal (100%)  
        /// </summary>
        public static Overhang uFromDeg(float f)
        {
            if (!float.IsFinite(f) || f < 0f || f > 90f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(f), f, 
                    "Overhang angle must be in the range 0..90");
            }

            return new(f/90f);
        }

        /// <summary>
        /// Create a new Overhang from an angle in degrees, 
        /// measured from horizontal plane. This convention is used
        /// by some 3D printing manufacturers. 
        /// Avoid, unless exchanging information with a specific vendor.
        /// - 0:    Maximum overhang (100%), horizontal feature
        /// - 45:   45º overhang
        /// - 90:   No overhang (0%), vertical feature, self-supporting
        /// </summary>
        public static Overhang uFromDegFromHorizontal(float f)
        {
            if (!float.IsFinite(f) || f < 0f || f > 90f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(f), f, 
                    "Overhang angle must be in the range 0..90");
            }
            return new((90f - f) / 90f);
        }

        /// <summary>
        /// Allows you to write something like uOverhang.bExceeds(Overhang.uFromPercent(50))
        /// You can also use comparison operators
        /// </summary>
        public bool bExceeds(Overhang threshold)
            => this > threshold;

        // Internals dealing with string conversion and comparability 

        public override string ToString()
            => $"{fPercent.ToString("0.###", CultureInfo.InvariantCulture)}%";

        public int CompareTo(Overhang other)
            => m_fNormalized.CompareTo(other.m_fNormalized);

        public bool Equals(Overhang other)
            => m_fNormalized.Equals(other.m_fNormalized);

        public override bool Equals(object? obj)
            => obj is Overhang other && Equals(other);

        public override int GetHashCode()
            => m_fNormalized.GetHashCode();

        public static bool operator <(Overhang left, Overhang right)
            => left.m_fNormalized < right.m_fNormalized;

        public static bool operator >(Overhang left, Overhang right)
            => left.m_fNormalized > right.m_fNormalized;

        public static bool operator <=(Overhang left, Overhang right)
            => left.m_fNormalized <= right.m_fNormalized;

        public static bool operator >=(Overhang left, Overhang right)
            => left.m_fNormalized >= right.m_fNormalized;

        public static bool operator ==(Overhang left, Overhang right)
            => left.Equals(right);

        public static bool operator !=(Overhang left, Overhang right)
            => !left.Equals(right);

        /// <summary>
        /// Normalized overhang severity from 0..1
        /// </summary>
        readonly float m_fNormalized = 0;

        /// <summary>
        /// Private constructor which takes a normalized overhang severity
        /// Not exposed publicly to avoid confusion. Use static functions uFrom... instead.
        /// </summary>
        Overhang(float fNorm)
        {
            if (!float.IsFinite(fNorm) || fNorm < 0f || fNorm > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fNorm), fNorm,
                    "Normalized overhang must be finite and in the range 0..1.");
            }
            
            m_fNormalized = fNorm;
        }
    }
}
    