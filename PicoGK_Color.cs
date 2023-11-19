//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023 by LEAP 71
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

using System.Runtime.InteropServices;

namespace PicoGK
{
    /// <summary>
    /// A floating point color value with R,G,B,A values
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct ColorFloat
    {
        public float R;
        public float G;
        public float B;
        public float A;

        /// <summary>
        /// Create a color from a hex string
        /// #FF0000 is red, for example (# is optional)
        /// #FF000000 is a fully transparent color (0 is transparent FF/1.0 is full opaque)
        /// #FF is grayscale (white)
        /// #FF99 is semi-transparent white
        /// </summary>
        /// <param name="strHex">A 6 character or 8 character string with the color</param>
        /// <exception cref="ArgumentException">Throws an exception if different sizes</exception>
        public ColorFloat(string strHex)
        {
            if (strHex.StartsWith("#"))
                strHex = strHex.Substring(1); // Remove the '#' character if present

            if (    (strHex.Length == 2) ||
                    (strHex.Length == 4))
            {
                // Grayscale "FF" or "FFFF"
                R = Convert.ToInt32(strHex.Substring(0, 2), 16) / 255f;
                G = R;
                B = G;

                A = (strHex.Length == 2) ? 1.0f : Convert.ToInt32(strHex.Substring(2, 2), 16) / 255f;
            }
            else if (   (strHex.Length == 6) ||
                        (strHex.Length == 8))
            {
                R = Convert.ToInt32(strHex.Substring(0, 2), 16) / 255f;
                G = Convert.ToInt32(strHex.Substring(2, 2), 16) / 255f;
                B = Convert.ToInt32(strHex.Substring(4, 2), 16) / 255f;

                A = (strHex.Length == 6) ? 1.0f : Convert.ToInt32(strHex.Substring(6, 2), 16) / 255f;
            }
            else
            {
                throw new ArgumentException($"Invalid Hexcode format: {strHex}. The Hexcode should be either 2, 4 (Gray), 6 or 8 (Color) characters long (not counting optional leading #).");
            }
        }

        /// <summary>
        /// Allows you to pass a hex string to any function that requires a FloatColor
        /// </summary>
        /// <param name="hex">Hexcode string</param>
        public static implicit operator ColorFloat(string hex)
        {
            return new ColorFloat(hex);
        }

        /// <summary>
        /// Creates a gray color based on the float value and optional alpha
        /// </summary>
        /// <param name="fGray">Gray value from 0.0 .. 1.0</param>
        /// <param name="fAlpha">Optional Alpha, 0.0 is transparent</param>
        public ColorFloat(  float fGray,
                            float fAlpha = 1.0f)
        {
            R = fGray;
            G = fGray;
            B = fGray;
            A = fAlpha;
        }

        /// <summary>
        /// A color value from the 3/4 color components
        /// </summary>
        /// <param name="fR">Red value</param>
        /// <param name="fG">Green value</param>
        /// <param name="fB">Blue value</param>
        /// <param name="fAlpha">Optional Alpha, 1.0 is fully opaque</param>
        public ColorFloat(  float fR,
                            float fG,
                            float fB,
                            float fAlpha = 1.0f)
        {
            R = fR;
            G = fG;
            B = fB;
            A = fAlpha;
        }

        public ColorFloat(ColorRgb24 clr)
        {
            R = clr.R / 255.0f;
            G = clr.G / 255.0f;
            B = clr.B / 255.0f;
            A = 1.0f;
        }

        public ColorFloat(ColorRgba32 clr)
        {
            R = clr.R / 255.0f;
            G = clr.G / 255.0f;
            B = clr.B / 255.0f;
            A = clr.A / 255.0f;
        }

        public ColorFloat(ColorBgr24 clr)
        {
            R = clr.R / 255.0f;
            G = clr.G / 255.0f;
            B = clr.B / 255.0f;
            A = 1.0f;
        }

        public ColorFloat(ColorBgra32 clr)
        {
            R = clr.R / 255.0f;
            G = clr.G / 255.0f;
            B = clr.B / 255.0f;
            A = clr.A / 255.0f;
        }

        public ColorFloat(ColorFloat clr, float fAlphaOverride)
        {
            R = clr.R;
            G = clr.G;
            B = clr.B;
            A = fAlphaOverride;
        }

        /// <summary>
        /// Return a random color
        /// </summary>
        /// <returns>Random color</returns>
        public static ColorFloat clrRandom(Random? oRand = null)
        {
            Random oRandom = oRand ?? new Random();

            return new ColorFloat(  oRandom.NextSingle(),
                                    oRandom.NextSingle(),
                                    oRandom.NextSingle());
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorRgb24
    {
        public ColorRgb24(  byte byR,
                            byte byG,
                            byte byB)
        {
            R = byR;
            G = byG;
            B = byB;
        }

        public ColorRgb24(ColorFloat clr)
        {
            R = (byte)(clr.R * 255.0f);
            G = (byte)(clr.G * 255.0f);
            B = (byte)(clr.B * 255.0f);
        }

        /// <summary>
        /// Allows you to pass a ColorFloat to any function that needs a ColorRgb24
        /// </summary>
        /// <param name="clr">The ColorFloat to use</param>
        public static implicit operator ColorRgb24(ColorFloat clr)
        {
            return new ColorRgb24(clr);
        }

        public byte R;
        public byte G;
        public byte B;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorRgba32
    {
        public ColorRgba32( byte byR,
                            byte byG,
                            byte byB,
                            byte byA = 255)
        {
            R = byR;
            G = byG;
            B = byB;
            A = byA;
        }

        public ColorRgba32(ColorFloat clr)
        {
            R = (byte)(clr.R * 255.0f);
            G = (byte)(clr.G * 255.0f);
            B = (byte)(clr.B * 255.0f);
            A = (byte)(clr.A * 255.0f);
        }

        /// <summary>
        /// Allows you to pass a ColorFloat to any function that needs a ColorRgba32
        /// </summary>
        /// <param name="clr">The ColorFloat to use</param>
        public static implicit operator ColorRgba32(ColorFloat clr)
        {
            return new ColorRgba32(clr);
        }

        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorBgr24
    {
        public ColorBgr24(  byte byB,
                            byte byG,
                            byte byR)
        {
            B = byB;
            G = byG;
            R = byR;
        }

        public ColorBgr24(  ColorFloat clr)
        {
            R = (byte)(clr.R * 255.0f);
            G = (byte)(clr.G * 255.0f);
            B = (byte)(clr.B * 255.0f);
        }

        /// <summary>
        /// Allows you to pass a ColorFloat to any function that needs a ColorBgr24
        /// </summary>
        /// <param name="clr">The ColorFloat to use</param>
        public static implicit operator ColorBgr24(ColorFloat clr)
        {
            return new ColorBgr24(clr);
        }

        public byte B;
        public byte G;
        public byte R;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorBgra32
    {
        public ColorBgra32( byte byB,
                            byte byG,
                            byte byR,
                            byte byA = 255)
        {
            B = byB;
            G = byG;
            R = byR;
            A = byA;
        }

        public ColorBgra32( ColorFloat clr)
        {
            R = (byte)(clr.R * 255.0f);
            G = (byte)(clr.G * 255.0f);
            B = (byte)(clr.B * 255.0f);
            A = (byte)(clr.A * 255.0f);
        }

        /// <summary>
        /// Allows you to pass a ColorFloat to any function that needs a ColorBgra32
        /// </summary>
        /// <param name="clr">The ColorFloat to use</param>
        public static implicit operator ColorBgra32(ColorFloat clr)
        {
            return new ColorBgra32(clr);
        }

        public byte B;
        public byte G;
        public byte R;
        public byte A;
    }

}

