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
    public partial class Viewer
    {
        public class AnimGroupMatrixRotate : Animation.IAction
        {
            public AnimGroupMatrixRotate(Viewer oViewer,
                                            int nGroup,
                                            Matrix4x4 matInit,
                                            Vector3 vecAxis,
                                            float fDegrees)
            {
                m_oViewer = oViewer;
                m_nGroup = nGroup;
                m_matInit = matInit;
                m_vecAxis = vecAxis;
                m_fDegrees = fDegrees;
            }

            public void Do(float fFactor)
            {
                double dAngle = (fFactor * m_fDegrees * Math.PI) / 180.0;
                Matrix4x4 matMul = Matrix4x4.CreateFromQuaternion(  Quaternion.CreateFromAxisAngle(m_vecAxis,
                                                                    (float)dAngle));

                Matrix4x4 mat = m_matInit * matMul;
                m_oViewer.SetGroupMatrix(m_nGroup, mat);
            }

            Viewer m_oViewer;
            Matrix4x4 m_matInit;
            Vector3 m_vecAxis;
            float m_fDegrees;
            int m_nGroup;
        }

        public class AnimViewRotate : Animation.IAction
        {
            public AnimViewRotate(Viewer oViewer,
                                    Vector2 vecFrom,
                                    Vector2 vecTo)
            {
                m_oViewer = oViewer;
                m_vecFrom = vecFrom;
                m_vecTo = vecTo;
            }

            public void Do(float fFactor)
            {
                Vector2 vec = (m_vecTo - m_vecFrom) * fFactor;
                vec += m_vecFrom;

                m_oViewer.SetViewAngles(vec.X, vec.Y);
            }

            Viewer m_oViewer;
            Vector2 m_vecFrom;
            Vector2 m_vecTo;
        }

        public void AddAnimation(Animation oAnim)
        {
            lock (m_oAnims)
            {
                m_oAnims.Add(oAnim);
            }
        }

        public void RemoveAllAnimations()
        {
            lock (m_oAnims)
            {
                m_oAnims.Clear();
            }
        }

        AnimationQueue m_oAnims = new();
    }
}
    