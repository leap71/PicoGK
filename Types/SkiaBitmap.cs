//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023-2024 by LEAP 71
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
    public static class SKHelpers
    {
        public static SKColor oAsSkColor(this ColorRgba32 clr)
        {
            return new SKColor( clr.R,
                                clr.G,
                                clr.B,
                                clr.A);
        }

        public static SKColor oAsSkColor(this ColorFloat clr)
        {
            return new SKColor( (byte) (clr.R * 255f),
                                (byte) (clr.G * 255f),
                                (byte) (clr.B * 255f),
                                (byte) (clr.A * 255f));
        }

        public static ColorRgba32 clrAsColorRgba32(this SKColor clr)
        {
             return new ColorRgba32(clr.Red,
                                    clr.Green,
                                    clr.Blue,
                                    clr.Alpha);
        }
    }

    public partial class Image
	{
        public static ImageRgba32 imgFromSKBitmap(SKBitmap oSKBitmap)
        {
            ImageRgba32 img = new ImageRgba32(oSKBitmap.Width, oSKBitmap.Height);

            for (int x = 0; x < img.nWidth; x++)
            {
                for (int y = 0; y < img.nHeight; y++)
                {
                    SKColor oSKColor = oSKBitmap.GetPixel(x,y);

                    img.SetRgba32(  x,
                                    y,
                                    oSKColor.clrAsColorRgba32());
                }
            }

            return img;
        }

        public static implicit operator SKBitmap(Image img)
        {
            SKBitmap skBitmap = new SKBitmap(   img.nWidth,
                                                img.nHeight,
                                                SKColorType.Rgba8888,
                                                SKAlphaType.Unpremul);

            for (int x = 0; x < img.nWidth; x++)
            {
                for (int y = 0; y < img.nHeight; y++)
                {
                    skBitmap.SetPixel(  x,
                                        y,
                                        img.sGetRgba32(x, y).oAsSkColor());
                }
            }

            return skBitmap;
        }

        public void SavePng(    string strFileName,
                                int iQuality = 100)
		{
            using (SKImage  skImg   = SKImage.FromBitmap(this))
            using (SKData   skData  = skImg.Encode(SKEncodedImageFormat.Png, iQuality))
            {
                using (FileStream fileStream = File.OpenWrite(strFileName))
                {
                    skData.SaveTo(fileStream);
                }
            }
        }

        public void SaveJpg(    string strFileName,
                                int iQuality = 100)
		{
            using (SKImage  skImg   = SKImage.FromBitmap(this))
            using (SKData   skData  = skImg.Encode(SKEncodedImageFormat.Jpeg, iQuality))
            {
                using (FileStream fileStream = File.OpenWrite(strFileName))
                {
                    skData.SaveTo(fileStream);
                }
            }
        }

        public void SaveTga(string strFileName)
		{
            TgaIo.SaveTga(strFileName, this);
        }

        public static Image imgLoadFromFile(string strFileName)
        {
            try
            {
                using (var skData = SKData.CreateCopy(File.ReadAllBytes(strFileName)))
                {
                    using (SKCodec? oCodec = SKCodec.Create(skData))
                    {
                        if (oCodec == null)
                            throw new Exception("Unknown file format for file " + Path.GetFileName(strFileName));

                        var oInfo = new SKImageInfo(    oCodec.Info.Width,
                                                        oCodec.Info.Height);

                        var oBitmap = new SKBitmap(oInfo);
            
                        var oResult = oCodec.GetPixels(  oInfo,
                                                        oBitmap.GetPixels(out _));
            
                    
                        if (    oResult == SKCodecResult.Success
                                || oResult == SKCodecResult.IncompleteInput)
                        {
                            return imgFromSKBitmap(oBitmap);
                        }
                        else
                        {
                            throw new Exception("Failed to load bitmap image.");
                        }
                    }
                }
            }

            catch (Exception)
            {
                // Check if we have a TGA file, if yes, try to load that using native PicoGK

                if (Path.GetExtension(strFileName).ToLower() != ".tga")
                {
                    throw;
                }

                TgaIo.LoadTga(strFileName, out Image img);
                return img;
            }
        }
	}
}

