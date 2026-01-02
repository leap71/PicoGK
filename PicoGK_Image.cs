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

        public virtual byte byGetValue(int x, int y)
        {
            return (byte)(Math.Clamp(fValue(x, y), 0.0f, 1.0f) * 255.0f);
        }

        public virtual void SetValue(int x, int y, byte byValue)
        {
            SetValue(x, y, byValue / 255.0f);
        }

        public virtual ColorBgr24 sGetBgr24(int x, int y)
        {
            ColorFloat clr = clrValue(x, y);
            ColorBgr24 sClr;
            sClr.B = (byte)(Math.Clamp(clr.B, 0.0f, 1.0f) * 255.0f);
            sClr.G = (byte)(Math.Clamp(clr.G, 0.0f, 1.0f) * 255.0f);
            sClr.R = (byte)(Math.Clamp(clr.R, 0.0f, 1.0f) * 255.0f);
            return sClr;
        }

        public virtual void SetBgr24(int x, int y, ColorBgr24 sClr)
        {
            SetValue(x, y, new ColorFloat(sClr));
        }

        public virtual ColorBgra32 sGetBgra32(int x, int y)
        {
            ColorFloat clr = clrValue(x, y);
            ColorBgra32 sClr;
            sClr.B = (byte)(Math.Clamp(clr.B, 0.0f, 1.0f) * 255.0f);
            sClr.G = (byte)(Math.Clamp(clr.G, 0.0f, 1.0f) * 255.0f);
            sClr.R = (byte)(Math.Clamp(clr.R, 0.0f, 1.0f) * 255.0f);
            sClr.A = (byte)(Math.Clamp(clr.A, 0.0f, 1.0f) * 255.0f);
            return sClr;
        }

        public virtual void SetBgra32(int x, int y, ColorBgra32 sClr)
        {
            SetValue(x, y, new ColorFloat(sClr));
        }

        public virtual ColorRgb24 sGetRgb24(int x, int y)
        {
            ColorFloat clr = clrValue(x, y);
            ColorRgb24 sClr;
            sClr.R = (byte)(Math.Clamp(clr.R, 0.0f, 1.0f) * 255.0f);
            sClr.G = (byte)(Math.Clamp(clr.G, 0.0f, 1.0f) * 255.0f);
            sClr.B = (byte)(Math.Clamp(clr.B, 0.0f, 1.0f) * 255.0f);
            return sClr;
        }

        public virtual ColorRgba32 sGetRgba32(int x, int y)
        {
            ColorFloat clr = clrValue(x, y);
            ColorRgba32 sClr;
            sClr.R = (byte)(Math.Clamp(clr.R, 0.0f, 1.0f) * 255.0f);
            sClr.G = (byte)(Math.Clamp(clr.G, 0.0f, 1.0f) * 255.0f);
            sClr.B = (byte)(Math.Clamp(clr.B, 0.0f, 1.0f) * 255.0f);
            sClr.A = (byte)(Math.Clamp(clr.A, 0.0f, 1.0f) * 255.0f);
            return sClr;
        }

        public virtual void SetRgb24(int x, int y, ColorRgb24 sClr)
        {
            SetValue(x, y, new ColorFloat(sClr));
        }

        public virtual void SetRgba32(int x, int y, ColorRgba32 sClr)
        {
            SetValue(x, y, new ColorFloat(sClr));
        }

        /// <summary>
        /// Returns the interpolated color value at a normalized
        /// coordinate going from 0..1
        /// </summary>
        /// <param name="fTX">X coordinate 0..1</param>
        /// <param name="fTY">Y coordinate 0..1</param>
        /// <returns></returns>
        public ColorFloat clrGetAtNormalized(float fTX, float fTY)
        {
            float fRealX = fTX * nWidth-1;
            float fRealY = fTY * nHeight-1;
            
            int x0 = (int) Math.Floor(fRealX);
            int x1 = x0 + 1;
            int y0 = (int) Math.Floor(fRealY);
            int y1 = y0 + 1;

            x0 = int.Clamp(x0, 0, nWidth - 1);
            x1 = int.Clamp(x1, 0, nWidth - 1);
            y0 = int.Clamp(y0, 0, nHeight - 1);
            y1 = int.Clamp(y1, 0, nHeight - 1);

            float dx = fRealX - x0;
            float dy = fRealY - y0;

            ColorFloat clr00 = clrValue(x0, y0);
            ColorFloat clr10 = clrValue(x1, y0);
            ColorFloat clr01 = clrValue(x0, y1);
            ColorFloat clr11 = clrValue(x1, y1);

            ColorFloat clr0 = ColorFloat.clrWeighted(clr00, clr10, dx);
            ColorFloat clr1 = ColorFloat.clrWeighted(clr01, clr11, dx);

            return ColorFloat.clrWeighted(clr0, clr1, dy);
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
            if (_nWidth < 1)
                throw new ArgumentOutOfRangeException("Image width must be larger 0");

            if (_nHeight < 1)
                throw new ArgumentOutOfRangeException("Image width must be larger 0");

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

        /// <summary>
        /// Returns whether the image has any pixels set to a value
        /// smaller or equal to the specified value
        /// This is useful to find out if a signed distance field slice
        /// contains any active voxels.
        /// </summary>
        /// <param name="fThreshold"></param>
        /// <returns></returns>
        public bool bContainsActivePixels(float fThreshold = 0.0f)
        {
            for (int x = 0; x < nWidth; x++)
            {
                for (int y = 0; y < nHeight; y++)
                {
                    if (fValue(x, y) <= fThreshold)
                        return true;
                }
            }

            return false;
        }
    }

    public abstract partial class ImageColorAbstract : Image
    {
        public ImageColorAbstract(  int _iWidth,
                                    int _iHeight)
            : base( _iWidth,
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
            if (oImg1.nWidth != oImg2.nWidth)
                throw new ArgumentOutOfRangeException("Interpolation between images requires same width and height");

            if (oImg1.nHeight != oImg2.nHeight)
                throw new ArgumentOutOfRangeException("Interpolation between images requires same width and height");

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

    public partial class ImageRgba32 : ImageColorAbstract
    {
        public ImageRgba32(  int _nWidth,
                             int _nHeight)
                : base( _nWidth,
                        _nHeight)
        {
            m_aclrValues = new ColorRgba32[nWidth * nHeight];
        }

        public ImageRgba32(Image imgSource)
            : this( imgSource.nWidth,
                    imgSource.nHeight)
        {
            for (int x=0; x<nWidth; x++)
            {
                for (int y = 0; y < nHeight; y++)
                {
                    SetRgba32(x,y,imgSource.clrValue(x,y));
                }
            }
        }

        public override ColorFloat clrValue(int x, int y)
        {
            return new ColorFloat(sGetRgba32(x,y));
        }

        public override void SetValue(int x, int y, in ColorFloat clr)
        {
            SetRgba32(x,y,new ColorRgba32(clr));
        }

        public override void SetRgba32(int x, int y, ColorRgba32 clr)
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

        public override ColorRgba32 sGetRgba32(int x, int y)
        {
            if (    (x < 0) ||
                    (y < 0) ||
                    (x >= nWidth) ||
                    (y >= nHeight))
            {
                return new ColorRgba32(0, 0, 0, 0);
            }

            return m_aclrValues[x + (y * nWidth)];
        }

        ColorRgba32[] m_aclrValues;
    }

    public partial class ImageRgb24 : ImageColorAbstract
    {
        public ImageRgb24(  int _nWidth,
                            int _nHeight)
                : base( _nWidth,
                        _nHeight)
        {
            m_aclrValues = new ColorRgb24[nWidth * nHeight];
        }

        public ImageRgb24(Image imgSource)
            : this( imgSource.nWidth,
                    imgSource.nHeight)
        {
            for (int x=0; x<nWidth; x++)
            {
                for (int y = 0; y < nHeight; y++)
                {
                    SetRgb24(x,y,imgSource.clrValue(x,y));
                }
            }
        }

        public override ColorFloat clrValue(int x, int y)
        {
            return new ColorFloat(sGetRgb24(x,y));
        }

        public override void SetValue(int x, int y, in ColorFloat clr)
        {
            SetRgb24(x,y,new ColorRgb24(clr));
        }

        public override void SetRgb24(int x, int y, ColorRgb24 clr)
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

        public override ColorRgb24 sGetRgb24(int x, int y)
        {
            if (    (x < 0) ||
                    (y < 0) ||
                    (x >= nWidth) ||
                    (y >= nHeight))
            {
                return new ColorRgb24(0, 0, 0);
            }

            return m_aclrValues[x + (y * nWidth)];
        }

        ColorRgb24[] m_aclrValues;
    }

    public partial class ImageColor : ImageColorAbstract
    {
        public ImageColor(  int _nWidth,
                            int _nHeight)
                : base( _nWidth,
                        _nHeight)
        {
            m_aclrValues = new ColorFloat[nWidth * nHeight];
        }

        public ImageColor(Image imgSource)
            : this( imgSource.nWidth,
                    imgSource.nHeight)
        {
            for (int x=0; x<nWidth; x++)
            {
                for (int y = 0; y < nHeight; y++)
                {
                    SetValue(x,y,imgSource.clrValue(x,y));
                }
            }
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

