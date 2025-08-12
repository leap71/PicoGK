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

using System.Numerics;

namespace PicoGK
{
    public class SliceViz : IDisposable
    {
        public SliceViz(    Viewer oViewer,
                            Voxels vox,
                            Voxels.ESliceAxis eAxis = Voxels.ESliceAxis.Z)
        {
            m_oViewer   = oViewer;
            m_vox       = vox;
            m_eAxis     = eAxis;

            m_img = vox.imgAllocateSlice(out m_nSlices, m_eAxis);
            m_vecScale  = vox.lib.vecVoxelsToMm(m_img.nWidth, m_img.nHeight, 1);

            BBox3 oBox = vox.oCalculateBoundingBox();
            Vector3 vecPos = oBox.vecCenter();

            switch (eAxis)
            {
                case Voxels.ESliceAxis.X:
                vecPos.X = oBox.vecMin.X; 
                m_frm = LocalFrame.frmFromZX(vecPos, Vector3.UnitX, Vector3.UnitY);
                m_bFlipY = true;
                break;

                case Voxels.ESliceAxis.Y:
                vecPos.Y = oBox.vecMin.Y;
                m_frm = LocalFrame.frmFromZX(vecPos, Vector3.UnitY, Vector3.UnitX);
                m_bFlipY = false;
                break;

                case Voxels.ESliceAxis.Z:
                default:
                vecPos.Z = oBox.vecMin.Z;
                m_frm = LocalFrame.frmFromZX(vecPos, Vector3.UnitZ, Vector3.UnitX);
                m_bFlipY = true;
                break;
            }
        }

        public int nSliceCount => m_nSlices;

        public void Visualize(float fNormalized)
        {
            int nSlice = (int) (nSliceCount * float.Clamp(fNormalized, 0,1) + 0.5f);
            Visualize(nSlice);
        }

        public void Visualize(int nSlice)
        {
            m_oQuad ??= new(    m_oViewer,
                                new(m_img),
                                "AAAA",
                                0.8f,
                                Matrix4x4.Identity,
                                false,
                                m_bFlipY,
                                true);

            if (m_poly != null)
            {
                m_oViewer.Remove(m_poly);
                m_poly.Dispose();
            }

            LocalFrame frm = m_frm.frmMovedLocal(new(0,0,nSlice * m_vox.lib.fVoxelSize));

            m_poly = new(m_vox.lib, "FF0000");
            m_poly.nAddVertex(  frm.vecPtToWorld(new( m_vecScale.X / 2,   m_vecScale.Y / 2, 0)));
            m_poly.nAddVertex(  frm.vecPtToWorld(new( m_vecScale.X / 2,  -m_vecScale.Y / 2, 0)));
            m_poly.nAddVertex(  frm.vecPtToWorld(new(-m_vecScale.X / 2,  -m_vecScale.Y / 2, 0)));
            m_poly.nAddVertex(  frm.vecPtToWorld(new(-m_vecScale.X / 2,   m_vecScale.Y / 2, 0)));
            m_poly.nAddVertex(  frm.vecPtToWorld(new( m_vecScale.X / 2,   m_vecScale.Y / 2, 0)));
            m_oViewer.Add(m_poly, 3);

            Console.WriteLine(m_oViewer.oBBox().ToString());

            m_vox.GetVoxelSlice(nSlice, ref m_img, Voxels.ESliceMode.Antialiased, m_eAxis);

            m_oQuad.UpdateMatrix(frm.matComposeWithScale(m_vecScale));
            m_oQuad.UpdateImage(new(m_img));
        }

        public void Dispose()
        {
            if (m_poly != null)
            {
                m_oViewer.Remove(m_poly);
                m_poly.Dispose();
                m_poly = null;
            }

            if (m_oQuad != null)
            {
                m_oQuad.Dispose();
                m_oQuad = null;
            }
        }

        Viewer              m_oViewer;
        Voxels              m_vox;
        Voxels.ESliceAxis   m_eAxis;
        bool                m_bFlipY;
        LocalFrame          m_frm;
        int                 m_nSlices;
        Vector3             m_vecScale;
        ImageGrayScale      m_img;
        Viewer.ImageQuad?   m_oQuad = null;
        PolyLine?           m_poly = null;
    }
}