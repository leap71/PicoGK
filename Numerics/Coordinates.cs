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
    public struct Spherical
    {
        /// <summary>
        /// Distance from the sphere center.
        /// </summary>
        public float R = 0;

        /// <summary>
        /// Azimuth angle in the XY plane, measured from +X toward +Y.
        /// Range from Cartesian conversion: [-π .. +π].
        /// </summary>
        public Rad Phi = Rad.Zero;

        /// <summary>
        /// Polar angle measured from +Z toward the XY plane and onward to -Z.
        /// Range: [0 .. π].
        /// </summary>
        public Rad Theta = Rad.Zero;

        /// <summary>
        /// Initializes a new Spherical coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Spherical(float fR, Rad rPhi, Rad rTheta)
        {
            if (!fR.bIsFinite() || fR < 0f)
                throw new ArgumentException(
                "Radius must be finite and non-negative.",
                nameof(fR));

            if (!rPhi.bIsFinite())
                throw new ArgumentException("Phi must be finite.", nameof(rPhi));

            if (!rTheta.bIsFinite() || rTheta < Rad.Zero || rTheta > Rad.Half)
                throw new ArgumentException("Theta must be finite and in the range [0, π].", nameof(rTheta));

            Theta   = rTheta;
            R       = fR;
            Phi     = rPhi;
        }

        /// <summary>
        /// Initializes a new Spherical coordinate by converting from a cartesian coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Spherical(Vector3 vecCartesian)
        {
            if (!vecCartesian.bIsFinite())
                throw new ArgumentException(
                "Cartesian vector must be finite.",
                nameof(vecCartesian));
            
            R = vecCartesian.Length();

            if (R.bAlmostZero())
            {
                R     = 0;
                Phi   = Rad.Zero;
                Theta = Rad.Zero;
                return;
            }

            Phi   = Rad.rAtan2(vecCartesian.vecStripZ());
            Theta = Rad.rAcosClamped(vecCartesian.Z / R);
        }

        /// <summary>
        /// Create a Spherical coordinate from a Cylindrical coordinate.
        /// </summary>
        public Spherical(Cylindrical oCylindrical)
            : this(oCylindrical.vecAsCartesian())
        {
            
        }

        /// <summary>
        /// Convert the spherical coordinate to a cartesian coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 vecAsCartesian()
        {
            float fSinTheta = Theta.fSin();

            return new Vector3(
                R * fSinTheta * Phi.fCos(),
                R * fSinTheta * Phi.fSin(),
                R * Theta.fCos());
        }

        /// <summary>
        /// Linear interpolation between two Spherical coordinates (in Spherical coordinate space).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Spherical oLerp(Spherical oA, Spherical oB, float fT)
        {
            Rad rDeltaPhi = (oB.Phi - oA.Phi).rNormalizedSigned();

            return new Spherical(
                fR:     oA.R     + fT * (oB.R     - oA.R),
                rPhi:   oA.Phi   + fT * rDeltaPhi,
                rTheta: oA.Theta + fT * (oB.Theta - oA.Theta));
        }

        /// <summary>
        /// Linear interpolation between this coordinate and another (in Spherical coordinate space).
        /// fT = 0: this coordinate;
        /// fT = 1: the other coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Spherical oLerp(Spherical oOther, float fT)
            => oLerp(this, oOther, fT);

        /// <summary>
        /// Convert the spherical coordinate to a string.
        /// </summary>
        public override string ToString()
            => $"(Radius={R}, Phi={Phi}, Theta={Theta})";
    }

    /// <summary>
    /// A coordinate in a cylindrical coordinate system.
    /// </summary>
    public struct Cylindrical
    {
        /// <summary>
        /// Distance from the cylinder's axis.
        /// </summary>
        public float R = 0;
        /// <summary>
        /// Azimuth angle in the XY plane.
        /// Range from cartesian conversion: [-π .. +π].
        /// </summary>
        public Rad Phi = Rad.Zero;
        /// <summary>
        /// Position along the Z axis
        /// </summary>
        public float Z = 0;

        /// <summary>
        /// Initialize a new cylindrical coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Cylindrical( float   fR, 
                            Rad     rPhi, 
                            float   fZ)
        {
            if (!fR.bIsFinite() || fR < 0f)
                throw new ArgumentException(
                "Radius must be finite and non-negative.",
                nameof(fR));

            if (!rPhi.bIsFinite())
                throw new ArgumentException("Phi must be finite.", nameof(rPhi));

            if (!fZ.bIsFinite())
                throw new ArgumentException("Z must be finite.", nameof(fZ));

            R   = fR;
            Phi = rPhi;
            Z   = fZ;
        }

        /// <summary>
        /// Create a new cylindrical coordinate from a polar coordinate plus Z height.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Cylindrical( Polar oPolar, 
                            float fZ)
            : this(oPolar.R, oPolar.Phi, fZ)
        {
        }

        /// <summary>
        /// Create a new cylindrical coordinate from a cartesian coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Cylindrical(Vector3 vecCartesian)
            : this(new Polar(vecCartesian.vecStripZ()), vecCartesian.Z)
        {
        }

        /// <summary>
        /// Create a cylindrical coordinate from a Spherical coordinate.
        /// </summary>
        /// <param name="oSpherical"></param>
        public Cylindrical( Spherical oSpherical)
            : this(oSpherical.vecAsCartesian())
        {    
        }
        
        /// <summary>
        /// Convert a cylindrical coordinate into a cartesian coordinate.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 vecAsCartesian()
        {
            return new Vector3(
                R * Phi.fCos(),
                R * Phi.fSin(),
                Z);
        }

        /// <summary>
        /// Linear interpolation between two Cylindrical coordinates (in Cylindrical coordinate space).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Cylindrical oLerp(Cylindrical oA, Cylindrical oB, float fT)
        {
            Rad rDeltaPhi = (oB.Phi - oA.Phi).rNormalizedSigned();

            return new Cylindrical(
                fR:   oA.R   + fT * (oB.R - oA.R),
                rPhi: oA.Phi + fT * rDeltaPhi,
                fZ:   oA.Z   + fT * (oB.Z - oA.Z));
        }

        /// <summary>
        /// Linear interpolation between this coordinate and another (in Cylindrical coordinate space).
        /// fT = 0: this coordinate;
        /// fT = 1: the other coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Cylindrical oLerp(Cylindrical oOther, float fT)
            => oLerp(this, oOther, fT);

        /// <summary>
        /// Convert the cylindrical coordinate to a string.
        /// </summary>
        public override string ToString()
            => $"(Radius={R}, Phi={Phi}, Z={Z})";
    }

    /// <summary>
    /// A polar coordinate 
    /// </summary>
    public struct Polar
    {
        /// <summary>
        /// Distance from the center of the coordinate system.
        /// </summary>
        public float R = 0;

        /// <summary>
        /// Azimuth angle in the XY plane.
        /// Range from cartesian conversion [-π .. +π].
        /// </summary>
        public Rad Phi = Rad.Zero;

        /// <summary>
        /// Initialize a new polar coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polar(float fR, Rad rPhi)
        {
            if (!fR.bIsFinite() || fR < 0f)
                throw new ArgumentException(
                "Radius must be finite and non-negative.",
                nameof(fR));

            if (!rPhi.bIsFinite())
                throw new ArgumentException("Phi must be finite.", nameof(rPhi));

            R   = fR;
            Phi = rPhi;
        }

        /// <summary>
        /// Initialize a new polar coordinate from a 2D cartesian coordinate.
        /// </summary>
        /// <param name="vecCartesian"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polar(Vector2 vecCartesian)
        {
            if (!vecCartesian.bIsFinite())
                throw new ArgumentException(
                "Cartesian vector must be finite.",
                nameof(vecCartesian));

            float fLenSq    =   vecCartesian.X * vecCartesian.X +
                                vecCartesian.Y * vecCartesian.Y;

            if (fLenSq <= Tolerances.fZeroSquared)
            {
                R   = 0;
                Phi = Rad.Zero;   // Azimuth is undefined at the origin
                return;
            }

            R   = float.Sqrt(fLenSq);
            Phi = Rad.rAtan2(vecCartesian);
        }

        /// <summary>
        /// Return the polar coordinate as a cartesian coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 vecAsCartesian()
        {
            return new Vector2( R * Phi.fCos(),
                                R * Phi.fSin());
        }

        /// <summary>
        /// Linear interpolation between two polar coordinates (in Polar coordinate space).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Polar oLerp(Polar oA, Polar oB, float fT)
        {
            Rad rDeltaPhi = (oB.Phi - oA.Phi).rNormalizedSigned();

            return new Polar(
                fR:   oA.R   + fT * (oB.R - oA.R),
                rPhi: oA.Phi + fT * rDeltaPhi);
        }

        /// <summary>
        /// Linear interpolation between this coordinate and another (in Polar coordinate space).
        /// fT = 0: this coordinate;
        /// fT = 1: the other coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Polar oLerp(Polar oOther, float fT)
        => oLerp(this, oOther, fT);

        /// <summary>
        /// Convert the polar coordinate to a string.
        /// </summary>
        public override string ToString()
            => $"(Radius={R}, Phi={Phi})";
    }
}
