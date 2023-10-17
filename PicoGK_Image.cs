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
    public abstract partial class Image
    {
        public enum EType
        {
            BW,
            GRAY,
            COLOR
        };

        public readonly int nWidth;
        public readonly int nHeight;
        public readonly EType eType;

        public abstract ColorFloat  clrValue(int x, int y);
        public abstract float       fValue(int x, int y);
        public abstract bool        bValue(int x, int y);

        public abstract void SetValue(int x, int y, in ColorFloat clr);
        public abstract void SetValue(int x, int y, float fGray);
        public abstract void SetValue(int x, int y, bool bValue);

        public byte byGetValue(int x, int y)
        {
            return (byte)(Math.Clamp(fValue(x, y), 0.0f, 1.0f) * 255.0f);
        }

        public void SetValue(int x, int y, byte byValue)
        {
            SetValue(x, y, byValue / 255.0f);
        }

        public ColorBgr24 sGetBgr24(int x, int y)
        {
            ColorFloat clr = clrValue(x, y);
            ColorBgr24 sClr;
            sClr.B = (byte)(Math.Clamp(clr.B, 0.0f, 1.0f) * 255.0f);
            sClr.G = (byte)(Math.Clamp(clr.G, 0.0f, 1.0f) * 255.0f);
            sClr.R = (byte)(Math.Clamp(clr.R, 0.0f, 1.0f) * 255.0f);
            return sClr;
        }

        public void SetBgr24(int x, int y, ColorBgr24 sClr)
        {
            SetValue(x, y, new ColorFloat(sClr));
        }

        public ColorRgb24 sGetRgb24(int x, int y)
        {
            ColorFloat clr = clrValue(x, y);
            ColorRgb24 sClr;
            sClr.R = (byte)(Math.Clamp(clr.R, 0.0f, 1.0f) * 255.0f);
            sClr.G = (byte)(Math.Clamp(clr.G, 0.0f, 1.0f) * 255.0f);
            sClr.B = (byte)(Math.Clamp(clr.B, 0.0f, 1.0f) * 255.0f);
            return sClr;
        }

        public void SetRgb24(int x, int y, ColorRgb24 sClr)
        {
            SetValue(x, y, new ColorFloat(sClr));
        }

        public void DrawLine(int x0, int y0, int x1, int y1, ColorFloat clr)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetValue(x0, y0, clr);

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, float fGrayscale)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetValue(x0, y0, fGrayscale);

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, bool bValue)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetValue(x0, y0, bValue);

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        protected Image(    int _nWidth,
                            int _nHeight,
                            EType _eType)
        {
            Debug.Assert(_nWidth > 0);
            Debug.Assert(_nHeight > 0);

            nWidth = _nWidth;
            nHeight = _nHeight;
            eType = _eType;
        }

    }

    public abstract partial class ImageBWAbstract : Image
    {
        public ImageBWAbstract( int _nWidth,
                                int _nHeight)
            : base(_nWidth,
                    _nHeight,
                    EType.BW)
        {
        }

        public override float fValue(int x, int y)
        {
            return bValue(x, y) ? 1.0f : 0.0f;
        }

        public override ColorFloat clrValue(int x, int y)
        {
            return new ColorFloat(bValue(x, y) ? 1.0f : 0.0f);
        }

        public override void SetValue(int x, int y, float fValue)
        {
            SetValue(x, y, fValue > 0.5f);
        }

        public override void SetValue(int x, int y, in ColorFloat clr)
        {
            SetValue(x, y, ((clr.R + clr.G + clr.B) / 3.0f) > 0.5f);
        }
    }

    public abstract partial class ImageGrayscaleAbstract : Image
    {
        public ImageGrayscaleAbstract(  int _nWidth,
                                        int _nHeight)
            : base(_nWidth,
                    _nHeight,
        EType.GRAY)
        {
        }

        public override ColorFloat clrValue(int x, int y)
        {
            float f = fValue(x, y);
            return new ColorFloat(f, f, f);
        }

        public override bool bValue(int x, int y)
        {
            return fValue(x, y) > 0.5f;
        }

        public override void SetValue(int x, int y, bool bValue)
        {
            SetValue(x, y, bValue ? 1.0f : 0.0f);
        }

        public override void SetValue(int x, int y, in ColorFloat clr)
        {
            SetValue(x, y, (clr.R + clr.G + clr.B) / 3.0f);
        }
    }

    public abstract partial class ImageColorAbstract : Image
    {
        public ImageColorAbstract(  int _iWidth,
                                    int _iHeight)
            : base(_iWidth,
                    _iHeight,
                    EType.COLOR)
        {
        }

        public override float fValue(int x, int y)
        {
            ColorFloat clr = clrValue(x, y);
            return (clr.R + clr.G + clr.B) / 3.0f;
        }

        public override bool bValue(int x, int y)
        {
            return fValue(x, y) > 0.5f;
        }

        public override void SetValue(int x, int y, float f)
        {
            SetValue(x, y, new ColorFloat(f));
        }

        public override void SetValue(int x, int y, bool bValue)
        {
            SetValue(x, y, new ColorFloat(bValue ? 1.0f : 0.0f));
        }
    }

    public partial class ImageGrayScale : ImageGrayscaleAbstract
    {
        public ImageGrayScale(  int _nWidth,
                                int _nHeight)
                : base( _nWidth,
                        _nHeight)
        {
            m_afValues = new float[nWidth * nHeight];
        }

        public override void SetValue(int x, int y, float fGray)
        {
            if (    (x < 0) ||
                    (y < 0) ||
                    (x >= nWidth) ||
                    (y >= nHeight))
            {
                return;
            }

            m_afValues[x + (y * nWidth)] = fGray;
        }

        public override float fValue(int x, int y)
        {
            if (    (x < 0) ||
                    (y < 0) ||
                    (x >= nWidth) ||
                    (y >= nHeight))
            {
                return 0.0f;
            }

            return m_afValues[x + (y * nWidth)];
        }

        public ImageColor imgGetColorCodedSDF(float fBackground)
        {
            ImageColor img = new(nWidth, nHeight);

            ColorFloat clrInsideBackground  = new("006600");
            ColorFloat clrOutsideBackground = new("00");
            
            for (int x=0; x<nWidth; x++)
            {
                for (int y = 0; y < nHeight; y++)
                {
                    float fV = fValue(x, y);

                    if (fV < 0f)
                    {
                        // Inside
                        if (fV <= -fBackground)
                            img.SetValue(x, y, clrInsideBackground);
                        else
                        {
                            fV /= -fBackground;
                            img.SetValue(x, y, new ColorFloat(0f, 1f - fV, 0f));
                        }
                    }
                    else
                    {
                        // Outside
                        if (fV >= fBackground)
                            img.SetValue(x, y, clrOutsideBackground);
                        else
                        {
                            fV /= fBackground;
                            img.SetValue(x, y, new ColorFloat(1f - fV, 1f - fV, 1f - fV));
                        }
                    }
                }
            }

            return img;
        }

        public static ImageGrayScale imgGetInterpolated(    ImageGrayScale oImg1,
                                                            ImageGrayScale oImg2,
                                                            float fWeight = 0.5f)
        {
            Debug.Assert(oImg1.nWidth == oImg2.nWidth);
            Debug.Assert(oImg1.nHeight == oImg2.nHeight);

            if (fWeight <= 0.0f)
                return oImg1;

            if (fWeight >= 1.0f)
                return oImg2;

            float fFac1 = 1.0f - fWeight;

            ImageGrayScale oNew = new(oImg1.nWidth, oImg1.nHeight);

            for (int n=0; n<oNew.nWidth * oNew.nHeight; n++)
            {
                oNew.m_afValues[n] = (fFac1 * oImg1.m_afValues[n]) + (fWeight * oImg2.m_afValues[n]);
            }

            return oNew;
        }

        public float[] m_afValues;
    }

    public partial class ImageColor : ImageColorAbstract
    {
        public ImageColor(int _nWidth,
                            int _nHeight)
                : base(_nWidth,
                        _nHeight)
        {
            m_aclrValues = new ColorFloat[nWidth * nHeight];
        }

        public override void SetValue(int x, int y, in ColorFloat clr)
        {
            if (    (x < 0) ||
                    (y < 0) ||
                    (x >= nWidth) ||
                    (y >= nHeight))
            {
                return;
            }

            m_aclrValues[x + (y * nWidth)] = clr;
        }

        public override ColorFloat clrValue(int x, int y)
        {
            if (    (x < 0) ||
                    (y < 0) ||
                    (x >= nWidth) ||
                    (y >= nHeight))
            {
                return new ColorFloat(0, 0, 0);
            }

            return m_aclrValues[x + (y * nWidth)];
        }

        ColorFloat[] m_aclrValues;
    }
}

