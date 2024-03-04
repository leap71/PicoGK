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

using System.Numerics;

namespace PicoGK
{
    public class SdfVisualizer
    {
        public static ImageColor imgEncodeFromSdf(  ScalarField oField,
                                                    float fBackgroundValue,
                                                    int nSlice,
                                                    ColorFloat? _clrBackground  = null,
                                                    ColorFloat? _clrSurface     = null,
                                                    ColorFloat? _clrInside      = null,
                                                    ColorFloat? _clrOutside     = null)
        {
            ColorFloat clrBackGround    = _clrBackground ?? new ColorFloat("0066ff");
            ColorFloat clrSurface       = _clrSurface    ?? new ColorFloat("FF");
            ColorFloat clrInside        = _clrInside     ?? new ColorFloat("cc33ff");
            ColorFloat clrOutside       = _clrOutside    ?? new ColorFloat("33cc33");
            
            oField.GetVoxelDimensions(  out int nXOrigin,
                                        out int nYOrigin,
                                        out int nZOrigin,
                                        out int nXSize,
                                        out int nYSize,
                                        out int nZSize);

            ImageColor imgResult = new(nXSize, nYSize);
            
            if (nSlice >= nZSize)
                return imgResult;

            for (int x=0; x<nXSize; x++)
            {
                for (int y=0; y<nYSize; y++)
                {
                    Vector3 vecCoord = Library.vecVoxelsToMm(   nXOrigin + x,
                                                                nYOrigin + y,
                                                                nZOrigin + nSlice);

                    bool bSet = oField.bGetValue(vecCoord, out float fValue);

                    ColorHLS clr;

                    if (float.Abs(fValue) < float.Epsilon)
                    {
                        clr = clrSurface;
                    }
                    else if (fValue == fBackgroundValue)
                    {
                        clr = clrBackGround;
                    }
                    else
                    {
                        if (fValue < 0)
                        {
                            clr = clrInside;
                            fValue = -fValue;
                        }
                        else
                        {
                            clr = clrOutside;
                        }

                        if (fValue > fBackgroundValue)
                        {
                            // outside the narrow band
                            // oversaturate the color
                            clr.S = 1.0f;
                        }
                        else
                        {
                            clr.L = 0.7f - (fValue / fBackgroundValue / 2.0f); 
                        }
                    }

                    if (!bSet)
                    {
                        clr.S = 0.3f; // desaturate significantly
                    }

                    imgResult.SetValue(x,y,clr);
                }
            }

            return imgResult;
        }
    }

    public class ActiveVoxelCounterScalar : ITraverseScalarField
    {
        public static int nCount(ScalarField oField)
        {
            ActiveVoxelCounterScalar oCounter = new(oField);
            oCounter.Run();

            return oCounter.m_nCount;
        }

        protected ActiveVoxelCounterScalar(ScalarField oField)
        {
            m_oField = oField;
        }

        protected void Run()
        {
            m_oField.TraverseActive(this);
        }

        public void InformActiveValue(in Vector3 vecPosition, float fValue)
        {
            m_nCount++;    
        }

        int         m_nCount = 0;
        ScalarField m_oField;
    }

    public class SurfaceNormalFieldExtractor : ITraverseScalarField
    {
        public static VectorField oExtract(   Voxels vox,
                                              float fSurfaceThresholdVx = 0.5f)
        {
            VectorField oField = new();

            SurfaceNormalFieldExtractor oExtractor
                = new(vox, oField, fSurfaceThresholdVx);

            oExtractor.Run();

            return oField;
        }

        protected SurfaceNormalFieldExtractor(  Voxels voxSource,
                                                VectorField oDestination,
                                                float fSurfaceThresholdVx)
        {
            m_voxSource     = voxSource;
            m_oSource       = new(voxSource);
            m_oDestination  = oDestination;
            m_fThreshold    = fSurfaceThresholdVx;
        }

        protected void Run()
        {
            m_oSource.TraverseActive(this);
        }

        public void InformActiveValue(in Vector3 vecPosition, float fValue)
        {
            if (float.Abs(fValue) < m_fThreshold)
            {
                m_oDestination.SetValue(    vecPosition,
                                            m_voxSource.vecSurfaceNormal(vecPosition));
            }
        }

        float           m_fThreshold;
        Voxels          m_voxSource;
        ScalarField     m_oSource;
        VectorField     m_oDestination;
    }
}

