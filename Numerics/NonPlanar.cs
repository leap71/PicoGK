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
    public readonly struct Spherical
    {
        /// <summary>
        /// Distance from the sphere center.
        /// </summary>
        public readonly float R;

        /// <summary>
        /// Azimuth angle in the XY plane, measured from +X toward +Y.
        /// Range from Cartesian conversion: [-π .. +π].
        /// </summary>
        public readonly float Phi;

        /// <summary>
        /// Polar angle measured from +Z toward the XY plane and onward to -Z.
        /// Range: [0 .. π].
        /// </summary>
        public readonly float Theta;

        /// <summary>
        /// Initializes a new Spherical coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Spherical(float fR, float fPhi, float fTheta)
        {
            if (!float.IsFinite(fR) || fR < 0f)
                throw new ArgumentException(
                "Radius must be finite and non-negative.",
                nameof(fR));

            if (fTheta < -Tolerances.fDef || fTheta > float.Pi + Tolerances.fDef)
                throw new ArgumentException("Theta must be in the range [0, π].", nameof(fTheta));

            Theta   = float.Clamp(fTheta, 0f, float.Pi);
            R       = fR;
            Phi     = fPhi.fNormalizedAngleRad();
        }

        /// <summary>
        /// Initializes a new Spherical coordinate by converting from a cartesian coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Spherical(Vector3 vecCartesian)
        {
            R = vecCartesian.Length();

            if (R.bAlmostZero())
            {
                R     = 0;
                Phi   = 0;
                Theta = 0;
                return;
            }

            Phi   = float.Atan2(vecCartesian.Y, vecCartesian.X);
            Theta = float.Acos(float.Clamp(vecCartesian.Z / R, -1f, 1f));
        }

        /// <summary>
        /// Convert the spherical coordinate to a cartesian coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 vecAsCartesian()
        {
            float fSinTheta = float.Sin(Theta);

            return new Vector3(
                R * fSinTheta * float.Cos(Phi),
                R * fSinTheta * float.Sin(Phi),
                R * float.Cos(Theta));
        }

        /// <summary>
        /// Convert the spherical coordinate to a string
        /// </summary>
        public override string ToString()
            => $"(Radius={R}, Phi={Phi}, Theta={Theta})";
    }

    /// <summary>
    /// A coordinate in a cylindrical coordinate system
    /// </summary>
    public readonly struct Cylindrical
    {
        /// <summary>
        /// Distance from the cylinder's axis
        /// </summary>
        public readonly float R;
        /// <summary>
        /// Azimuth angle in the XY plane in the range [-π .. +π].
        /// </summary>
        public readonly float Phi;
        /// <summary>
        /// Position along the Z axis
        /// </summary>
        public readonly float Z;

        /// <summary>
        /// Initialize a new cylindrical coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Cylindrical( float fR, 
                            float fPhi, 
                            float fZ)
        {
            if (!float.IsFinite(fR) || fR < 0f)
                throw new ArgumentException(
                "Radius must be finite and non-negative.",
                nameof(fR));

            R   = fR;
            Phi = fPhi.fNormalizedAngleRad();
            Z   = fZ;
        }

        /// <summary>
        /// Create a new cylindrical coordinate from a polar coordinate plus Z height
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Cylindrical( Polar oPolar, 
                            float fZ)
        {
            R   = oPolar.R;
            Phi = oPolar.Phi;
            Z   = fZ;
        }

        /// <summary>
        /// Create a new cylindrical coordinate from a cartesian coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Cylindrical(Vector3 vecCartesian)
        {
            Polar oPolar = new(vecCartesian.vecStripZ());

            R   = oPolar.R;
            Phi = oPolar.Phi;
            Z   = vecCartesian.Z;
        }

        /// <summary>
        /// Convert a cylindrical coordinate into a cartesian coordinate
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 vecAsCartesian()
        {
            return new Vector3(
                R * float.Cos(Phi),
                R * float.Sin(Phi),
                Z);
        }

        /// <summary>
        /// Convert the cylindrical coordinate to a string
        /// </summary>
        public override string ToString()
            => $"(Radius={R}, Phi={Phi}, Z={Z})";
    }

    /// <summary>
    /// A polar coordinate 
    /// </summary>
    public readonly struct Polar
    {
        /// <summary>
        /// Distance from the center of the coordinate system
        /// </summary>
        public readonly float R;

        /// <summary>
        /// Azimuth angle in the XY plane in the range [-π .. +π].
        /// </summary>
        public readonly float Phi;

        /// <summary>
        /// Initialize a new polar coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polar(float fR, float fPhi)
        {
            if (!float.IsFinite(fR) || fR < 0f)
                throw new ArgumentException(
                "Radius must be finite and non-negative.",
                nameof(fR));

            R   = fR;
            Phi = fPhi.fNormalizedAngleRad();
        }

        /// <summary>
        /// Initialize a new polar coordinate from a 2D cartesian coordinate
        /// </summary>
        /// <param name="vecCartesian"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polar(Vector2 vecCartesian)
        {
            float fLenSq    =   vecCartesian.X * vecCartesian.X +
                                vecCartesian.Y * vecCartesian.Y;

            if (fLenSq <= Tolerances.fZeroSquared)
            {
                R   = 0;
                Phi = 0;   // Azimuth is undefined at the origin
                return;
            }

            R   = float.Sqrt(fLenSq);
            Phi = float.Atan2(vecCartesian.Y, vecCartesian.X);
        }

        /// <summary>
        /// Return the polar coordinate as a cartesian coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 vecAsCartesian()
        {
            return new Vector2(
                R * float.Cos(Phi),
                R * float.Sin(Phi));
        }

        /// <summary>
        /// Convert the polar coordinate to a string
        /// </summary>
        public override string ToString()
            => $"(Radius={R}, Phi={Phi})";
    }
}
