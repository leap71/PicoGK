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
using System.Numerics;

namespace PicoGK.Numerics
{    
    /// <summary>
    /// This type encapsulates an angle in Radians, with helper functions to convert from Degrees
    /// </summary>
    public readonly struct Rad :   IComparable<Rad>,
                                   IEquatable<Rad>
    {
        readonly float m_fRad = 0;

        /// <summary>
        /// Defines 2*Pi, which is constantly being used in Rad angles
        /// </summary>
        public const float TwoPi = float.Tau;

        /// <summary>
        /// Zero degrees angles
        /// </summary>
        public static readonly Rad Zero = new(0f);

        /// <summary>
        /// 360º angle
        /// </summary>
        public static readonly Rad Full = new(TwoPi);

        /// <summary>
        /// 180º angle
        /// </summary>
        public static readonly Rad Half = new(TwoPi / 2f);

        /// <summary>
        /// 90º angle
        /// </summary>
        public static readonly Rad Quarter = new(TwoPi / 4f);

        /// <summary>
        /// 0º angle
        /// </summary>
        public static readonly Rad Deg0 = Zero;

        /// <summary>
        /// 360º angle
        /// </summary>
        public static readonly Rad Deg360 = Full;

        /// <summary>
        /// 180º angle
        /// </summary>
        public static readonly Rad Deg180 = Half;

        /// <summary>
        /// 90º angle
        /// </summary>
        public static readonly Rad Deg90 = Quarter;

        /// <summary>
        /// 45º angle
        /// </summary>
        public static readonly Rad Deg45 = rFromDeg(45);

        /// <summary>
        /// Initialize a new Rad value from a float radians angle
        /// </summary>
        public Rad(float fRad)
        {
            m_fRad = fRad;
        }

        /// <summary>
        /// Create new Rad value from a float radians angle
        /// </summary>
        public static Rad rFromRad(float fRad)
        {
            return new Rad(fRad);
        }

        /// <summary>
        /// Create a new Rad value from a floating point angle in degrees
        /// </summary>
        public static Rad rFromDeg(float fDegrees)
        {
            return new Rad(fDegrees * TwoPi / 360f);
        }

        /// <summary>
        /// Create a new Rad value from a normalized value 0..1, mapped to 0..360º.
        /// Values outside the range are clamped.
        /// </summary>
        public static Rad rFromNormalized(float fNormalized)
        {
            if (fNormalized <= 0f)
                return Zero;

            if (fNormalized >= 1f)
                return Full;

            return Full * fNormalized;
        }

        /// <summary>
        /// float value of the angle in radians
        /// </summary>
        public float fRad => m_fRad;

        /// <summary>
        /// angle in degrees 
        /// </summary>
        public float fDeg => m_fRad * 360f / TwoPi;

        /// <summary>
        /// Return the angle normalized to the range 
        /// -π .. +π
        /// </summary>
        public Rad rNormalizedSigned()
        {
            float f = MathF.IEEERemainder(m_fRad, TwoPi);

            if (f == 0f)
                return Zero;

            return new Rad(f);
        }

        /// <summary>
        /// Return the angle normalized to the range [0, 2π)
        /// </summary>
        public Rad rNormalizedPositive()
        {
            float f = m_fRad % TwoPi;

            if (f < 0f)
                f += TwoPi;

            if (f == 0f)
                return Zero;

            return new Rad(f);
        }

        /// <summary>
        /// Implicit conversion from a Rad value into float
        /// for seamless passing in to functions that require floats
        /// float f=float.Cos(rAngle)
        /// </summary>
        public static implicit operator float(Rad r)
        {
            return r.m_fRad;
        }

        /// <summary>
        /// Explicit conversion from float to Rad value.
        /// Allows you to write Rad rAngle = (Rad) float.Pi;
        /// Note the explicit cast to make it clear you are
        /// expecting a radians value in the float.
        /// </summary>
        public static explicit operator Rad(float fRad)
        {
            return new Rad(fRad);
        }

        /// <summary>
        /// Test for fuzzy equality
        /// </summary>
        public bool bAlmostEqual(Rad other, float fToleranceRad = Tolerances.fDef)
        {
            return float.Abs(m_fRad - other.m_fRad) <= fToleranceRad;
        }

        /// <summary>
        /// Tests for fuzzy equality of the normalized angle (0º == 360º == 720º)
        /// </summary>
        public bool bAlmostEqualPeriodic(Rad other, float fToleranceRad = Tolerances.fDef)
        {
            return float.Abs((this - other).rNormalizedSigned().m_fRad) <= fToleranceRad;
        }

        /// <summary>
        /// Checks whether the angle value is finite, i.e. not NaN and not positive or negative infinity.
        /// </summary>
        public bool bIsFinite()
        {
            return float.IsFinite(m_fRad);
        }

        /// <summary>
        /// Returns the sine of the angle
        /// </summary>
        public float fSin()
        {
            return float.Sin(m_fRad);
        }

        /// <summary>
        /// Returns the cosine of the angle
        /// </summary>
        public float fCos()
        {
            return float.Cos(m_fRad);
        }

        /// <summary>
        /// Returns the tangent of the angle.
        /// </summary>
        public float fTan()
        {
            return float.Tan(m_fRad);
        }

        /// <summary>
        /// Computes the angle of the vector from the positive X axis,
        /// using the signs of X and Y to determine the correct quadrant.
        /// </summary>
        public static Rad rAtan2(Vector2 vec)
        {
            return rAtan2(vec.Y, vec.X);
        }

        /// <summary>
        /// Computes the angle of the vector from the positive X axis,
        /// using the signs of X and Y to determine the correct quadrant.
        /// </summary>
        public static Rad rAtan2(float fY, float fX)
        {
            return new Rad(float.Atan2(fY, fX));
        }

        /// <summary>
        /// Computes the arc tangent of the value
        /// </summary>
        public static Rad rAtan(float f)
        {
            return new Rad(float.Atan(f));
        }
        
        /// <summary>
        /// Returns the arc cosine of the value and returns the angle
        /// </summary>
        public static Rad rAcos(float fValue)
        {
            return new Rad(float.Acos(fValue));
        }

        /// <summary>
        /// Returns the arc cosine of the value after clamping it to [-1, +1].
        /// Useful for geometric calculations affected by floating-point drift.
        /// </summary>
        public static Rad rAcosClamped(float fValue)
        {
            return new Rad(float.Acos(float.Clamp(fValue, -1f, 1f)));
        }

        /// <summary>
        /// Returns the arc sine of the value and returns the angle
        /// </summary>
        public static Rad rAsin(float fValue)
        {
            return new Rad(float.Asin(fValue));
        }

        /// <summary>
        /// Returns the arc cosine of the value after clamping it to [-1, +1].
        /// Useful for geometric calculations affected by floating-point drift.
        /// </summary>
        public static Rad rAsinClamped(float fValue)
        {
            return new Rad(float.Asin(float.Clamp(fValue, -1f, 1f)));
        }

        // Rad +/- Rad

        public static Rad operator +(Rad a, Rad b)
        {
            return new Rad(a.m_fRad + b.m_fRad);
        }

        public static Rad operator -(Rad a, Rad b)
        {
            return new Rad(a.m_fRad - b.m_fRad);
        }

        // Scaling

        public static Rad operator *(Rad r, float fScale)
        {
            return new Rad(r.m_fRad * fScale);
        }

        public static Rad operator *(float fScale, Rad r)
        {
            return new Rad(fScale * r.m_fRad);
        }

        public static Rad operator /(Rad r, float fScale)
        {
            return new Rad(r.m_fRad / fScale);
        }

        // Dimensionless ratio

        public static float operator /(Rad a, Rad b)
        {
            return a.m_fRad / b.m_fRad;
        }

        // Unary operators

        public static Rad operator +(Rad r)
        {
            return r;
        }

        public static Rad operator -(Rad r)
        {
            return new Rad(-r.m_fRad);
        }

        public override string ToString()
            => $"{fDeg.ToString("0.#", CultureInfo.InvariantCulture)}º";

        public int CompareTo(Rad other)
            => m_fRad.CompareTo(other.m_fRad);

        public bool Equals(Rad other)
            => m_fRad.Equals(other.m_fRad);

        public override bool Equals(object? obj)
            => obj is Rad other && Equals(other);

        public override int GetHashCode()
            => m_fRad.GetHashCode();

        public static bool operator <(Rad left, Rad right)
            => left.m_fRad < right.m_fRad;

        public static bool operator >(Rad left, Rad right)
            => left.m_fRad > right.m_fRad;

        public static bool operator <=(Rad left, Rad right)
            => left.m_fRad <= right.m_fRad;

        public static bool operator >=(Rad left, Rad right)
            => left.m_fRad >= right.m_fRad;

        public static bool operator ==(Rad left, Rad right)
            => left.Equals(right);

        public static bool operator !=(Rad left, Rad right)
            => !left.Equals(right);
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
    