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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PicoGK
{
    class TgaIo
    {
        public static void SaveTga( string strFilename,
                                    in Image img)
        {
            using (var oFile = File.Open(strFilename, FileMode.Create))
            {
                using (var oWriter = new BinaryWriter(oFile))
                {
                    SaveTga(oWriter, img);
                }
            }
        }

        public static void SaveTga( in BinaryWriter oWriter,
                                    in Image img)
        {
            if (img.nWidth > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("The width of the image " + nameof(img) + " is too large to store in a TGA");

            if (img.nHeight > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("The height of the image " + nameof(img) + " is too large to store in a TGA");

            ushort nSizeX = (ushort)img.nWidth;
            ushort nSizeY = (ushort)img.nHeight;

            STgaHeader sHeader = new STgaHeader(nSizeX, nSizeY);

            bool bColor;

            if (    (img.eType == Image.EType.BW) ||
                    (img.eType == Image.EType.GRAY))
            {
                bColor = false;
                sHeader.byImageType = 3;
                sHeader.byPixelDepth = 8;
            }
            else
            {
                bColor = true;
                sHeader.byImageType = 2;
                sHeader.byPixelDepth = 24;
            }

            var oHeaderSpan = MemoryMarshal.CreateSpan(ref sHeader, 1);
            oWriter.Write(MemoryMarshal.AsBytes(oHeaderSpan));

            if (bColor)
            {
                for (int y = 0; y < nSizeY; y++)
                {
                    for (int x = 0; x < nSizeX; x++)
                    {
                        ColorBgr24 sClr = img.sGetBgr24(x, y);
                        var oSpan = MemoryMarshal.CreateSpan(ref sClr, 1);
                        oWriter.Write(MemoryMarshal.AsBytes(oSpan));
                    }
                }
            }
            else
            {
                for (int y = 0; y < nSizeY; y++)
                {
                    for (int x = 0; x < nSizeX; x++)
                    {
                        oWriter.Write(img.byGetValue(x, y));
                    }
                }
            }
        }

        public static void GetFileInfo( string strFilename,
                                        out Image.EType eType,
                                        out int nWidth,
                                        out int nHeight)

        {
            using (var oFile = File.Open(strFilename, FileMode.Open))
            {
                using (var oReader = new BinaryReader(oFile))
                {
                    GetFileInfo(    in oReader,
                                    out eType,
                                    out nWidth,
                                    out nHeight);
                }
            }
        }

        public static void GetFileInfo( in BinaryReader oReader,
                                        out Image.EType eType,
                                        out int nWidth,
                                        out int nHeight)
        {
            STgaHeader sHeader = new STgaHeader(0, 0);

            var oHeaderSpan = MemoryMarshal.CreateSpan(ref sHeader, 1);
            oReader.Read(MemoryMarshal.AsBytes(oHeaderSpan));

            nWidth = sHeader.ushImageWidth;
            nHeight = sHeader.ushImageHeight;

            eType = Image.EType.GRAY;

            if (sHeader.byImageType == 2)
            {
                eType = Image.EType.COLOR;
            }
            else if (sHeader.byImageType != 3)
            {
                throw new ArgumentException("TGA has unsupported format (expecting grayscale or color)");
            }
        }

        public static void LoadTga( string strFilename,
                                    out Image img)
        {
            using (var oFile = File.Open(strFilename, FileMode.Open))
            {
                using (var oReader = new BinaryReader(oFile))
                {
                    LoadTga(oReader, out img);
                }
            }
        }

        public static void LoadTga( in BinaryReader oReader,
                                    out Image img)
        {
            STgaHeader sHeader = new STgaHeader(0, 0);

            var oHeaderSpan = MemoryMarshal.CreateSpan(ref sHeader, 1);
            oReader.Read(MemoryMarshal.AsBytes(oHeaderSpan));

            bool bColor = false;

            if (sHeader.byImageType == 2)
            {
                bColor = true;
            }
            else if (sHeader.byImageType != 3)
            {
                throw new ArgumentException("TGA has unsupported format (expecting grayscale or color)");
            }

            if (bColor)
            {
                if (sHeader.byPixelDepth != 24)
                    throw new ArgumentException("TGA has unsupported bit depth (expecting 24) for color TGAs");

                img = new ImageColor(sHeader.ushImageWidth, sHeader.ushImageHeight);
            }
            else
            {
                if (sHeader.byPixelDepth != 8)
                    throw new ArgumentException("TGA has unsupported bit depth (expecting 8) for grayscale TGAs");

                img = new ImageGrayScale(sHeader.ushImageWidth, sHeader.ushImageHeight);
            }

            ColorBgr24 sClr = new ColorBgr24();
            var oBgrSpan = MemoryMarshal.CreateSpan(ref sClr, 1);

            for (int y = 0; y < sHeader.ushImageHeight; y++)
            {
                for (int x = 0; x < sHeader.ushImageWidth; x++)
                {
                    if (bColor)
                    {
                        oReader.Read(MemoryMarshal.AsBytes(oBgrSpan));
                        img.SetBgr24(x, y, sClr);
                    }
                    else
                    {
                        byte[] aby = new byte[1];

                        oReader.Read(aby);
                        img.SetValue(x, y, aby[0] / 255.0f);
                    }
                }
            }
        }

        // TGA Header

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct STgaHeader
        {
            byte byIDLength;
            byte byColorMapType;
            public byte byImageType;
            byte byColorMapSpec1;
            byte byColorMapSpec2;
            byte byColorMapSpec3;
            byte byColorMapSpec4;
            byte byColorMapSpec5;
            ushort ushXOrigin;
            ushort ushYOrigin;
            public ushort ushImageWidth;
            public ushort ushImageHeight;
            public byte byPixelDepth;
            byte byImageDesc;

            public STgaHeader(ushort ushWidth, ushort ushHeight)
            {
                byIDLength = 0;
                byColorMapType = 0;
                byImageType = 3; // Grayscale
                byColorMapSpec1 = 0;
                byColorMapSpec2 = 0;
                byColorMapSpec3 = 0;
                byColorMapSpec4 = 0;
                byColorMapSpec5 = 0;
                ushXOrigin = 0;
                ushYOrigin = 0;
                ushImageWidth = ushWidth;
                ushImageHeight = ushHeight;
                byPixelDepth = 8; // Grayscale
                byImageDesc = 32;
            }
        }

    }
}

