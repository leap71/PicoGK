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
    public partial class Viewer
    {
        public abstract class Camera
        {
            public enum EDragType
            {
                ROTATE,
                PAN
            };

            public abstract void SetAspect(float fAspectRatio);

            public void SetAspect(Vector2 vecSize)
            {
                SetAspect(vecSize.X / vecSize.Y);
            }

            public abstract void LookAt(Vector3 vec);

            public abstract void ZoomToFit(BBox3 oBBox);

            public abstract void Scroll(Vector2 vecMouseRel);

            public abstract void MouseDrag(Vector2 vecMouseRel, EDragType eType);

            public abstract Matrix4x4 matVP
            {
                get;
            }

            public abstract Vector3 vecEye
            {
                get;
            }
        }

        public class CamPerspective : Camera
        {
            public CamPerspective(float fFoVDeg = 45)
            {
                SetFov(fFoVDeg);
            }
            
            public void SetFov(float fFovDeg)
            {
                float fFovRad = fFovDeg * float.Pi / 180f; 
                if (fFovRad != m_fFoV)
                {
                    m_fFoV = fFovRad;
                    UpdateMatrices();
                }
            }

            public override void SetAspect(float fAspectRatio)
            {
                if (fAspectRatio != m_fAspect)
                {
                    m_fAspect = fAspectRatio;
                    UpdateMatrices();
                }
            }

            public override void LookAt(Vector3 vec)
            {
                if (vec != m_vecPivot)
                {
                    m_vecPivot = vec;
                    UpdateMatrices();
                }
            }

            public override void ZoomToFit(BBox3 oBBox)
            {
                m_vecPivot  = oBBox.vecCenter();

                // Compute bounding sphere radius
                float fR = oBBox.vecSize().Length() * 0.5f;

                // Compute the required distance to fit vertically:
                float fDistVert = fR / float.Tan(m_fFoV / 2f);

                // Compute horizontal FoV from aspect ratio
                float fFovXRad  = 2f * float.Atan(MathF.Tan(m_fFoV / 2f) * m_fAspect);
                float fDistHorz = fR / float.Tan(fFovXRad / 2f);

                // Choose the tighter one (which dominates)
                m_fDistance = float.Max(fDistHorz, fDistVert) * 1.1f;

                UpdateMatrices();
            }

            public override void MouseDrag(Vector2 vecMouseRel, EDragType eType)
            {
                const float ROTATE_SPEED = 0.005f;
                const float PAN_SPEED    = 0.0025f;

                if (eType == EDragType.ROTATE)
                {
                    m_vecPitchYaw.X -= vecMouseRel.X * ROTATE_SPEED;
                    m_vecPitchYaw.Y += vecMouseRel.Y * ROTATE_SPEED;

                    // Clamp pitch to avoid flipping
                    m_vecPitchYaw.Y = float.Clamp(m_vecPitchYaw.Y, -float.Pi / 2f + 0.01f, float.Pi / 2f - 0.01f);
                }
                else if (eType == EDragType.PAN)
                {
                    Vector3 vecForward = Vector3.Normalize(m_vecPivot - m_vecEye);
                    Vector3 vecRight   = Vector3.Normalize(Vector3.Cross(vecForward, Vector3.UnitY));
                    Vector3 vecUp      = Vector3.Normalize(Vector3.Cross(vecRight, vecForward));

                    float fPanScale = m_fDistance * PAN_SPEED;

                    m_vecPivot -= vecRight * vecMouseRel.X * fPanScale;
                    m_vecPivot += vecUp    * vecMouseRel.Y * fPanScale;
                }

                UpdateMatrices();
            }

            public override void Scroll(Vector2 vecMouseRel)
            {
                const float ZOOM_SPEED = .1f;

                // Exponential zoom scaling
                m_fDistance *= float.Pow(1.0f - ZOOM_SPEED, vecMouseRel.Y);
                m_fDistance = float.Clamp(m_fDistance, 0.01f, 1000f);
                
                UpdateMatrices();
            }

            public override Matrix4x4 matVP => m_matV * m_matP;

            public override Vector3 vecEye =>  m_vecEye;

            void UpdateMatrices()
            {
                float yaw   = m_vecPitchYaw.X;
                float pitch = m_vecPitchYaw.Y;
                float x     = m_fDistance * float.Cos(pitch) * float.Sin(yaw);
                float y     = m_fDistance * float.Sin(pitch);
                float z     = m_fDistance * float.Cos(pitch) * float.Cos(yaw);
                m_vecEye    = m_vecPivot + new Vector3(x, y, z);

                m_matV = Matrix4x4.CreateLookAt(    m_vecEye, 
                                                    m_vecPivot, 
                                                    Vector3.UnitY);

                m_matP = Matrix4x4.CreatePerspectiveFieldOfView(    m_fFoV, 
                                                                    m_fAspect, 
                                                                    0.1f, 
                                                                    1000f);

                Console.WriteLine($"Eye: {m_vecEye}");
            }

            Vector3 m_vecEye        = Vector3.One;
            float   m_fFoV          = float.Pi / 2f;
            float   m_fAspect       = 1;
            float   m_fDistance     = 1;
            Vector3 m_vecPivot      = Vector3.Zero;
            Vector2 m_vecPitchYaw   = Vector2.Zero;
            Matrix4x4 m_matV;
            Matrix4x4 m_matP;
        }
   
        public class CamPerspectiveArc : Camera
        {
            public CamPerspectiveArc(float fFoVDeg = 45)
            {
                SetFov(fFoVDeg);
            }

            public void SetFov(float fFovDeg)
            {
                float fFovRad = fFovDeg * float.Pi / 180f; 
                if (fFovRad != m_fFoV)
                {
                    m_fFoV = fFovRad;
                    UpdateMatrices();
                }
            }

            public override void SetAspect(float fAspectRatio)
            {
                if (fAspectRatio != m_fAspect)
                {
                    m_fAspect = fAspectRatio;
                    UpdateMatrices();
                }
            }

            public override void LookAt(Vector3 vec)
            {
                if (vec != m_vecPivot)
                {
                    m_vecPivot = vec;
                    UpdateMatrices();
                }
            }

            public override void ZoomToFit(BBox3 oBBox)
            {
                m_vecPivot = oBBox.vecCenter();

                float fR = oBBox.vecSize().Length() * 0.5f;
                float fDistVert = fR / float.Tan(m_fFoV / 2f);
                float fFovXRad = 2f * float.Atan(MathF.Tan(m_fFoV / 2f) * m_fAspect);
                float fDistHorz = fR / float.Tan(fFovXRad / 2f);

                m_fDistance = float.Max(fDistHorz, fDistVert) * 1.1f;

                UpdateMatrices();
            }

            public override void MouseDrag(Vector2 vecMouseRel, EDragType eType)
            {
                const float ROTATE_SPEED = 0.005f;
                const float PAN_SPEED = 0.0025f;

                if (eType == EDragType.ROTATE)
                {
                    Quaternion qYaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -vecMouseRel.X * ROTATE_SPEED);
                    Vector3 right = Vector3.Transform(Vector3.UnitX, m_qRotation);
                    Quaternion qPitch = Quaternion.CreateFromAxisAngle(right, -vecMouseRel.Y * ROTATE_SPEED);
                    m_qRotation = Quaternion.Normalize(qPitch * qYaw * m_qRotation);
                }
                else if (eType == EDragType.PAN)
                {
                    Vector3 vecForward = Vector3.Normalize(m_vecPivot - m_vecEye);
                    Vector3 vecRight   = Vector3.Normalize(Vector3.Cross(vecForward, Vector3.UnitY));
                    Vector3 vecUp      = Vector3.Normalize(Vector3.Cross(vecRight, vecForward));

                    float fPanScale = m_fDistance * PAN_SPEED;

                    m_vecPivot -= vecRight * vecMouseRel.X * fPanScale;
                    m_vecPivot += vecUp    * vecMouseRel.Y * fPanScale;
                }

                UpdateMatrices();
            }

            public override void Scroll(Vector2 vecMouseRel)
            {
                const float ZOOM_SPEED = 0.1f;

                m_fDistance *= float.Pow(1.0f - ZOOM_SPEED, vecMouseRel.Y);
                m_fDistance = float.Clamp(m_fDistance, 0.01f, 1000f);

                UpdateMatrices();
            }

            public override Matrix4x4 matVP => m_matV * m_matP;
            public override Vector3 vecEye => m_vecEye;

            void UpdateMatrices()
            {
                Vector3 offset = Vector3.Transform(new Vector3(0, 0, m_fDistance), m_qRotation);
                m_vecEye = m_vecPivot + offset;

                Vector3 up = Vector3.Transform(Vector3.UnitY, m_qRotation);

                m_matV = Matrix4x4.CreateLookAt(m_vecEye, m_vecPivot, up);
                m_matP = Matrix4x4.CreatePerspectiveFieldOfView(m_fFoV, m_fAspect, 0.1f, 1000f);
            }

            Vector3 m_vecEye = Vector3.One;
            float m_fFoV = float.Pi / 2f;
            float m_fAspect = 1;
            float m_fDistance = 1;
            Vector3 m_vecPivot = Vector3.Zero;
            Quaternion m_qRotation = Quaternion.Identity;
            Matrix4x4 m_matV;
            Matrix4x4 m_matP;
        }

    }
}
    