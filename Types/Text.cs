//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023-2025 by LEAP 71
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

using SkiaSharp;

namespace PicoGK
{
	public class Text
	{
        static public SKTypeface oDefaultTypeface
        {
            get
            {
                lock (g_oTFLock) // Be thread safe
                {
                    if (g_oTypeFace is null)
                    {
                        try
                        {
                            // Try to load from embedded resource (when in Nuget mode)
                            using Stream? oStream = typeof(Library).Assembly.GetManifestResourceStream("PicoGK.Resources.Font.ttf");

                            if (oStream is null)
                            {
                                // Load font from disk
                                g_oTypeFace = SKTypeface.FromFile(Path.Combine(Utils.strPicoGKSourceCodeFolder(), "assets", "Jost.ttf"));
                            }
                            else
                            {
                                // Load font from embedded resource
                                g_oTypeFace = SKTypeface.FromStream(oStream);
                            }
                        }
                            
                        catch
                        {
                            
                        }

                        if (g_oTypeFace == null)
                            g_oTypeFace = SKTypeface.CreateDefault();
                    }

                    return g_oTypeFace;
                }
            }
        }

        static public ImageRgba32 imgRenderText(    string strText,
                                                    int nFontHeight,
                                                    int nPadding                = 10,
                                                    ColorFloat? _clrBackground = null,
                                                    ColorFloat? _clrText       = null,
                                                    SKTypeface? _oTypeface     = null)
        {
            ColorFloat  clrBackground = _clrBackground ?? new("FF");
            ColorFloat  clrText       = _clrText       ?? new("00");
            SKTypeface  oTypeface     = _oTypeface     ?? oDefaultTypeface;

            using SKFont oFont = new(oTypeface, nFontHeight);

            using SKPaint oPaint = new SKPaint
            {
                IsAntialias = true,
                Color       = clrText.oAsSkColor()
            };

            // Measure text bounds using SKFont
            oFont.MeasureText(strText, out SKRect oBounds);

            int nWidth  = (int)(oBounds.Width + 0.5f) + 2 * nPadding;
            int nHeight = (int)(oBounds.Height + 0.5f) + 2 * nPadding;

            using SKBitmap oSkBitmap = new(nWidth, nHeight);
            using SKCanvas oSkCanvas = new(oSkBitmap);

            oSkCanvas.Clear(clrBackground.oAsSkColor());

            float fX = nPadding - oBounds.Left;
            float fY = nPadding - oBounds.Top;

            oSkCanvas.DrawText(strText, fX, fY, SKTextAlign.Left, oFont, oPaint);

            return Image.imgFromSKBitmap(oSkBitmap);
        }

        static readonly object       g_oTFLock = new();
        static SKTypeface?  g_oTypeFace = null;
    }  
}

