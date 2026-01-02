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
using System.Runtime.InteropServices;

namespace PicoGK
{
    /// <summary>
    /// A floating point color value with R,G,B,A values
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct ColorFloat
    {
        /// <summary>
        /// Red value (1 is full color)
        /// </summary>
        public float R;

        /// <summary>
        /// Green value (1 is full color)
        /// </summary>
        public float G;

        /// <summary>
        /// Blue value (1 is full color)
        /// </summary>
        public float B;

        /// <summary>
        /// Alpha value (1 is opaque, 0 is transparent)
        /// </summary>
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

        /// <summary>
        /// Creates a floating point color from a RGB 24 bit color
        /// </summary>
        /// <param name="clr">RGB 24 bit color value</param>
        public ColorFloat(ColorRgb24 clr)
        {
            R = clr.R / 255.0f;
            G = clr.G / 255.0f;
            B = clr.B / 255.0f;
            A = 1.0f;
        }

        /// <summary>
        /// Creates a floating point color from a RGBA 32 bit color
        /// </summary>
        /// <param name="clr">RGBA 32 bit color value</param>
        public ColorFloat(ColorRgba32 clr)
        {
            R = clr.R / 255.0f;
            G = clr.G / 255.0f;
            B = clr.B / 255.0f;
            A = clr.A / 255.0f;
        }

        /// <summary>
        /// Creates a floating point color from a BGR 24 bit color
        /// </summary>
        /// <param name="clr">BGR 24 bit color value</param>
        public ColorFloat(ColorBgr24 clr)
        {
            R = clr.R / 255.0f;
            G = clr.G / 255.0f;
            B = clr.B / 255.0f;
            A = 1.0f;
        }

        /// <summary>
        /// Creates a floating point color from a BGRA 32 bit color
        /// </summary>
        /// <param name="clr">BGRA 32 bit color value</param>
        public ColorFloat(ColorBgra32 clr)
        {
            R = clr.R / 255.0f;
            G = clr.G / 255.0f;
            B = clr.B / 255.0f;
            A = clr.A / 255.0f;
        }

        /// <summary>
        /// Creates a new color from an existing color, overriding the alpha
        /// </summary>
        /// <param name="clr">Source color</param>
        /// <param name="fAlphaOverride">Alpha override</param>
        public ColorFloat(ColorFloat clr, float fAlphaOverride)
        {
            R = clr.R;
            G = clr.G;
            B = clr.B;
            A = fAlphaOverride;
        }

        /// <summary>
        /// Creates a new color value from a Hue Saturation value (HSV)
        /// </summary>
        /// <param name="clrHSV">HSV color</param>
        public ColorFloat(ColorHSV clrHSV)
        {
            ColorFloat clr = clrHSV;
            R = clr.R;
            G = clr.G;
            B = clr.B;
            A = 1f;
        }

        /// <summary>
        /// Create a new color value from a Hue Lightness Saturation value (HLS)
        /// </summary>
        /// <param name="clrHLS">HLS color</param>
        public ColorFloat(ColorHLS clrHLS)
        {
            ColorFloat clr = clrHLS;
            R = clr.R;
            G = clr.G;
            B = clr.B;
            A = 1f;
        }

        /// <summary>
        /// Returns the color as a hex code such as "FF" for white, "AAAA" for transparent gray
        /// "AABBCC" for an RGB color value or "DDEEFF99" for a RGBA value 
        /// </summary>
        /// <returns>Hex-encoded color value with 2 .. 8 characters</returns>
        public string strAsHexCode()
        {
            ColorRgba32 clr = this;
            string strResult;

            if (clr.R == clr.G && clr.G == clr.B)
            {
                // grayscale
                strResult = $"{clr.R:x2}";
            }
            else
            {
                // rgb
                strResult = $"{clr.R:x2}{clr.G:x2}{clr.B:x2}";
            }

            if (clr.A != 1)
                strResult += $"{clr.A:x2}";

            return strResult;
        }

        /// <summary>
        /// Returns the color value as an ABGR hex code (always 8 chars)
        /// </summary>
        /// <returns>8 character ABGR hex string</returns>
        public string strAsABGRHexCode()
        {
            ColorRgba32 clr = this;
            return $"{clr.A:x2}{clr.B:x2}{clr.G:x2}{clr.R:x2}";
        }

        /// <summary>
        /// Returns the color as hex string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return strAsHexCode();
        }

        /// <summary>
        /// Weighted linear interpolation between two colors
        /// </summary>
        /// <param name="clr1">First color</param>
        /// <param name="clr2">Second color</param>
        /// <param name="fWeight">Weight 0..1 to interpolate the color from First...Second</param>
        /// <returns></returns>
        public static ColorFloat clrWeighted(   ColorFloat clr1,
                                                ColorFloat clr2,
                                                float fWeight)
        {
            Debug.Assert(fWeight >= 0);
            Debug.Assert(fWeight <= 1);

            clr1.R *= fWeight;
            clr1.G *= fWeight;
            clr1.B *= fWeight;
            clr1.A *= fWeight;

            clr1.R *= (1 - fWeight);
            clr1.G *= (1 - fWeight);
            clr1.B *= (1 - fWeight);
            clr1.A *= (1 - fWeight);

            return new ColorFloat(  clr1.R + clr2.R,
                                    clr1.G + clr2.G,
                                    clr1.B + clr2.B,
                                    clr1.A + clr2.A);
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

    /// <summary>
    /// 24 bit RGB color
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorRgb24
    {
        /// <summary>
        /// Construct a 24 bit RGB value from 3 byes
        /// </summary>
        /// <param name="byR">Red value</param>
        /// <param name="byG">Green value</param>
        /// <param name="byB">Blue value</param>
        public ColorRgb24(  byte byR,
                            byte byG,
                            byte byB)
        {
            R = byR;
            G = byG;
            B = byB;
        }

        /// <summary>
        /// Construct a 24 bit RGB value from a float color
        /// </summary>
        /// <param name="clr">Float color value</param>
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

        /// <summary>
        /// Red value (0..255)
        /// </summary>
        public byte R;

        /// <summary>
        /// Green value (0..255)
        /// </summary>
        public byte G;

        /// <summary>
        /// Blue value (0..255)
        /// </summary>
        public byte B;
    }

    /// <summary>
    /// 32 bit RGBA color
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorRgba32
    {
        /// <summary>
        /// Create a color from 3 or 4 bytes
        /// </summary>
        /// <param name="byR">Red color 0..255</param>
        /// <param name="byG">Green color 0..255</param>
        /// <param name="byB">Blue color 0..255</param>
        /// <param name="byA">Alpha channel 0..255 (255 is opaque)</param>
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

        /// <summary>
        /// Create an 8 bit RGBA color from a float color
        /// Values below 0 and above 1 are truncated (0 .. 255)
        /// </summary>
        /// <param name="clr">Float color</param>
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

        /// <summary>
        /// Red value (0..255)
        /// </summary>
        public byte R;

        /// <summary>
        /// Green value (0..255)
        /// </summary>
        public byte G;

        /// <summary>
        /// Blue value (0..255)
        /// </summary>
        public byte B;

        /// <summary>
        /// Alpha value 0..255 (255 is opaque)
        /// </summary>
        public byte A;
    }

    /// <summary>
    /// BGR 24 bit color value
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorBgr24
    {
        /// <summary>
        /// Construct a BGR value from 3 bytes
        /// </summary>
        /// <param name="byB">Blue value</param>
        /// <param name="byG">Green value</param>
        /// <param name="byR">Red value</param>
        public ColorBgr24(  byte byB,
                            byte byG,
                            byte byR)
        {
            B = byB;
            G = byG;
            R = byR;
        }

        /// <summary>
        /// Create a BGR 24 bit color from a float color
        /// </summary>
        /// <param name="clr">Source color</param>
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

        /// <summary>
        /// Blue value (0..255)
        /// </summary>
        public byte B;

        /// <summary>
        /// Green value (0..255)
        /// </summary>
        public byte G;

        /// <summary>
        /// Red value (0..255)
        /// </summary>
        public byte R;
    }

    /// <summary>
    /// BGRA 32 bit color value
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorBgra32
    {
        /// <summary>
        /// Construct a 32 bit BGRA color value from 4 bytes
        /// </summary>
        /// <param name="byB">Blue value (0..255)</param>
        /// <param name="byG">Green value (0..255)</param>
        /// <param name="byR">Red value (0..255)</param>
        /// <param name="byA">Alpha value (0..255)</param>
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

        /// <summary>
        /// Construct a BGRA 32 bit value from a float color
        /// </summary>
        /// <param name="clr">Floating point color</param>
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

        /// <summary>
        /// Blue value (0..255)
        /// </summary>
        public byte B;
        /// <summary>
        /// Green value (0..255)
        /// </summary>
        public byte G;
        /// <summary>
        /// Red value (0..255)
        /// </summary>
        public byte R;
        /// <summary>
        /// Alpha value (0..255)
        /// </summary>
        public byte A;
    }

    /// <summary>
    /// Hue Saturation Value (HSV) color
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorHSV
    {
        /// <summary>
        /// Hue (0..360º)
        /// </summary>
        public float H;
        /// <summary>
        /// Saturation (0..1)
        /// </summary>
        public float S;
        /// <summary>
        /// Value component
        /// </summary>
        public float V;

        /// <summary>
        /// Create an HSV value from its three components
        /// </summary>
        /// <param name="fH"></param>
        /// <param name="fS"></param>
        /// <param name="fV"></param>
        public ColorHSV( float fH,
                         float fS,
                         float fV)
        {
            H = fH;
            S = fS;
            V = fV;
        }

        /// <summary>
        /// Create an HSV color from three linear RGB components
        /// </summary>
        /// <param name="clr">RGB color</param>
        public ColorHSV(ColorFloat clr)
        {
            // make sure we are in valid range for HSV conversion
            // No HDR values
            clr.R = float.Clamp(clr.R, 0, 1);
            clr.G = float.Clamp(clr.G, 0, 1);
            clr.B = float.Clamp(clr.B, 0, 1);

            float min = Math.Min(Math.Min(clr.R, clr.G), clr.B);
            float max = Math.Max(Math.Max(clr.R, clr.G), clr.B);
            V = max; // Value is the maximum of RGB components.

            float delta = max - min;

            if (max != 0)
            {
                S = delta / max; // Saturation is the ratio of delta and max.
            }
            else
            {
                // R = G = B = 0 (S = 0, V is undefined)
                S = 0;
                H = 0;
                return;
            }

            if (delta != 0) // Ensure delta is not zero to avoid division by zero
            {
                if (clr.R == max)
                {
                    H = (clr.G - clr.B) / delta; // Between yellow & magenta
                }
                else if (clr.G == max)
                {
                    H = 2 + (clr.B - clr.R) / delta; // Between cyan & yellow
                }
                else
                {
                    H = 4 + (clr.R - clr.G) / delta; // Between magenta & cyan
                }

                H *= 60; // Convert to degrees

                if (H < 0)
                    H += 360;
            }
            else
            {
                H = 0; // If delta is zero, hue is 0
            }
        }

        /// <summary>
        /// Implicit conversion that allows you to pass a ColorFloat
        /// to any function requiring and HSV color
        /// </summary>
        /// <param name="clr">ColorFloat to be converted to HSV</param>
        public static implicit operator ColorHSV(ColorFloat clr)
        {
            return new ColorHSV(clr);
        }

        /// <summary>
        /// Implicit conversion that allows to use an HSV color value
        /// for any function that requires a ColorFloat
        /// </summary>
        /// <param name="clrHSV">HSV color which is implicitly converted</param>
        public static implicit operator ColorFloat(ColorHSV clrHSV)
        {   
            float h = clrHSV.H;
            float s = clrHSV.S;
            float v = clrHSV.V;

            if (s == 0)
            {
                // Achromatic (grey)
                return new ColorFloat(v, v, v);
            }

            h /= 60; // sector 0 to 5
            int i = (int)Math.Floor(h);
            float f = h - i; // factorial part of h
            float p = v * (1 - s);
            float q = v * (1 - s * f);
            float t = v * (1 - s * (1 - f));

            switch (i)
            {
                case 0:
                    return new ColorFloat(v, t, p);
                case 1:
                    return new ColorFloat(q, v, p);
                case 2:
                    return new ColorFloat(p, v, t);
                case 3:
                    return new ColorFloat(p, q, v);
                case 4:
                    return new ColorFloat(t, p, v);
                default: // case 5:
                    return new ColorFloat(v, p, q);
            }
        }
    }

    /// <summary>
    /// A color value in HSV space
    /// </summary>
    public struct ColorHLS
    {
        /// <summary>
        /// Hue value (0..360º)
        /// </summary>
        public float H;

        /// <summary>
        /// Lightness value (0..1)
        /// </summary>
        public float L;

        /// <summary>
        /// Saturation value (0..1)
        /// </summary>
        public float S; // Saturation (0-1)

        /// <summary>
        /// Create an HLS color from its three components
        /// </summary>
        /// <param name="fH">Hue (0..360º)</param>
        /// <param name="fL">Lightness (0..1)</param>
        /// <param name="fS">Saturation (0..1)</param>
        public ColorHLS(float fH, float fL, float fS)
        {
            H = fH;
            L = fL;
            S = fS;
        }

        /// <summary>
        /// Convert an RGB color into an HLS color
        /// </summary>
        /// <param name="clr">ColorFloat to be converted to HLS</param>
        public ColorHLS(ColorFloat clr)
        {
            // make sure we are in valid range for HSV conversion
            // No HDR values
            clr.R = float.Clamp(clr.R, 0, 1);
            clr.G = float.Clamp(clr.G, 0, 1);
            clr.B = float.Clamp(clr.B, 0, 1);

            float min = float.Min(float.Min(clr.R, clr.G), clr.B);
            float max = float.Max(float.Max(clr.R, clr.G), clr.B);
            float delta = max - min;

            // Lightness calculation
            L = (max + min) / 2;

            // Saturation calculation
            if (delta == 0)
            {
                S = 0; // Achromatic (grey), no saturation
                H = 0; // Hue is undefined
            }
            else
            {
                // Saturation is calculated differently for light and dark colors
                S = L < 0.5 ? delta / (max + min) : delta / (2.0f - max - min);

                // Hue calculation
                if (clr.R == max)
                {
                    H = (clr.G - clr.B) / delta; // Between yellow & magenta
                }
                else if (clr.G == max)
                {
                    H = 2 + (clr.B - clr.R) / delta; // Between cyan & yellow
                }
                else if (clr.B == max)
                {
                    H = 4 + (clr.R - clr.G) / delta; // Between magenta & cyan
                }

                H *= 60; // Convert to degrees
                if (H < 0)
                {
                    H += 360;
                }
            }
        }

        /// <summary>
        /// Implicit conversion from ColorFloat to ColorHLS
        /// </summary>
        /// <param name="clr">ColorFloat to be converted to ColorHLS</param>
        public static implicit operator ColorHLS(ColorFloat clr)
        {
            return new ColorHLS(clr);
        }

        /// <summary>
        /// Implicit conversion from ColorHLS to ColorFloat
        /// </summary>
        /// <param name="clrHLS">HLS color to be converted</param>
        public static implicit operator ColorFloat(ColorHLS clrHLS)
        {
            float r, g, b;
            float h = clrHLS.H;
            float l = clrHLS.L;
            float s = clrHLS.S;

            if (s == 0)
            {
                // Achromatic case (grey)
                r = g = b = l; // all components equal to lightness
            }
            else
            {
                float temp2 = l < 0.5 ? l * (1.0f + s) : l + s - l * s;
                float temp1 = 2.0f * l - temp2;

                r = GetColorComponent(temp1, temp2, h + 120);
                g = GetColorComponent(temp1, temp2, h);
                b = GetColorComponent(temp1, temp2, h - 120);
            }

            return new ColorFloat(r, g, b);
        }

        /// <summary>
        /// Helper function to return the color component
        /// </summary>
        private static float GetColorComponent(float temp1, float temp2, float temp3)
        {
            temp3 = NormalizeHue(temp3);
            if (temp3 < 60)
                return temp1 + (temp2 - temp1) * temp3 / 60.0f;
            if (temp3 < 180)
                return temp2;
            if (temp3 < 240)
                return temp1 + (temp2 - temp1) * (240 - temp3) / 60.0f;
            return temp1;
        }

        /// <summary>
        /// Normalizes the hue to be within 0..360 bounds
        /// </summary>
        /// <param name="hue"></param>
        /// <returns></returns>
        private static float NormalizeHue(float hue)
        {
            hue = hue % 360; // Normalize hue to be within 0-360
            if (hue < 0)
                hue += 360;

            return hue;
        }
    }
}

