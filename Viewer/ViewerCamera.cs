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
                SPIN,
                PAN
            };

           public  abstract void SetViewPort(   Vector2 vecSize,
                                                float fSceneDepth);

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


        public class CamPerspectiveArcball : Camera
        {
            public CamPerspectiveArcball(float fFovVerticalDeg = 45f)
            {
                SetVerticalFov(fFovVerticalDeg);
                UpdateMatrices();
            }

            public void SetVerticalFov(float fFovDeg)
            {
                float f = fFovDeg * float.Pi / 180f;
                if (f != m_fVerticalFoV)
                {
                    m_fVerticalFoV = f;
                    UpdateMatrices();
                }
            }

            public override void SetViewPort(   Vector2 vecSizePx,
                                                float fSceneRadius)
            {
                if (    (vecSizePx != m_vecViewport) ||
                        (fSceneRadius != m_fSceneRadius))
                {
                    m_fAspect = vecSizePx.X / float.Max(1f, vecSizePx.Y);
                    m_vecViewport = vecSizePx;
                    m_fSceneRadius = fSceneRadius;
                    UpdateMatrices();
                }
            }

            public override void LookAt(Vector3 vec)
            {
                m_vecPivot = vec;
                UpdateMatrices();
            }

            public override void ZoomToFit(BBox3 oBBox)
            {
                m_vecPivot = oBBox.vecCenter();
                
                float fDistVert         = m_fSceneRadius / float.Tan(m_fVerticalFoV * 0.5f);
                float fHorizontalFovX   = 2f * float.Atan(MathF.Tan(m_fVerticalFoV * 0.5f) * m_fAspect);
                float fDistH            = m_fSceneRadius / float.Tan(fHorizontalFovX * 0.5f);

                m_fDistance = float.Max(fDistVert, fDistH) * 1.1f;
                UpdateMatrices();
            }

            public override void MouseDrag(Vector2 vecMouseRel, EDragType eType)
            {
                switch (eType)
                {
                case EDragType.ROTATE:
                    Rotate(vecMouseRel);
                    break;

                case EDragType.SPIN:
                    Spin(vecMouseRel);
                    break;

                case EDragType.PAN:
                    Pan(vecMouseRel);
                    break;

                default:
                    break;
                }

                UpdateMatrices();
            }

            public override void Scroll(Vector2 vecMouseRel)
            {
                float fSteps = vecMouseRel.Y;
                float fFactor = MathF.Pow(1.0f - ZOOM_SPEED, fSteps);
                m_fDistance = float.Clamp(m_fDistance * fFactor, 0.01f, 1e6f);

                UpdateMatrices();
            }

            public override Matrix4x4   matVP => m_matV * m_matP;
            public override Vector3     vecEye => m_vecEye;

            void UpdateMatrices()
            {
                // Camera eye from pivot + rotated local -Z axis at distance
                Vector3 vecLocal = new(0, 0, m_fDistance);
                Vector3 vecOffset = Vector3.Transform(vecLocal, m_qRotation);
                m_vecEye = m_vecPivot + vecOffset;

                Vector3 vecUp = Vector3.Transform(Vector3.UnitY, m_qRotation);
                if (vecUp.LengthSquared() < 1e-10f) 
                    vecUp = Vector3.UnitY; // Safe fallback

                float fHorizontalFov = 2f * float.Atan(float.Tan(m_fVerticalFoV * 0.5f) * m_fAspect);

                float fFarPlaneSafe = float.Max(    m_fSceneRadius / float.Tan(m_fVerticalFoV/2), 
                                                    m_fSceneRadius / float.Tan(fHorizontalFov/2) ) * 2f;

                m_matV = Matrix4x4.CreateLookAt(m_vecEye, m_vecPivot, Vector3.Normalize(vecUp));
                m_matP = Matrix4x4.CreatePerspectiveFieldOfView(    m_fVerticalFoV, 
                                                                    float.Max(1e-5f, m_fAspect), 
                                                                    0.1f, 
                                                                    fFarPlaneSafe);
            }

            // Build camera basis from view matrix (stable near poles)
            void CalculateBasis(out Vector3 vecRight, out Vector3 vecUp, out Vector3 vecBack)
            {
                // m_matV maps world -> view; its columns encode world axes in view space.
                // For transforming a view-space axis to world, use the inverse (or directly from rotation part of inverse).
                Matrix4x4.Invert(m_matV, out Matrix4x4 invV);
                vecRight = Vector3.Normalize(new Vector3(invV.M11, invV.M12, invV.M13));
                vecUp    = Vector3.Normalize(new Vector3(invV.M21, invV.M22, invV.M23));
                vecBack  = Vector3.Normalize(new Vector3(invV.M31, invV.M32, invV.M33)); // camera forward is -vecBack
            }

            void Spin(Vector2 vecDeltaPx)
            {
                 if (vecDeltaPx.LengthSquared() < 1e-12f) 
                    return;

                CalculateBasis(out _, out _, out Vector3 vecBack);
    
                float dTheta = (vecDeltaPx.X - vecDeltaPx.Y) * (2f * float.Pi / PIXELS_PER_SPIN);
               
                Quaternion qDelta = Quaternion.CreateFromAxisAngle(-vecBack, -dTheta);
                m_qRotation = Quaternion.Normalize(qDelta * m_qRotation);
            }

            void Rotate(Vector2 vecDeltaPx)
            {
                if (vecDeltaPx.LengthSquared() < 1e-12f) 
                    return;

                float s = 1.0f / float.Max( m_vecViewport.X,  m_vecViewport.Y); // normalized small angle
                Vector2 vecNormalizedDelta = vecDeltaPx * s;

                // Axis in view space: (dy, dx, 0) -> right-handed screen space
                Vector3 vecAxisView = Vector3.Normalize(new Vector3(vecNormalizedDelta.Y, vecNormalizedDelta.X, 0f));
                float fAngle = ROTATE_SPEED * vecNormalizedDelta.Length();

                CalculateBasis( out Vector3 vecRight, 
                                out Vector3 vecUp, 
                                out Vector3 vecBack);

                Vector3 vecAxisWorld =  vecAxisView.X * vecRight + 
                                        vecAxisView.Y * vecUp + 
                                        vecAxisView.Z * vecBack;

                if (vecAxisWorld.LengthSquared() > 1e-12f)
                {
                    vecAxisWorld = Vector3.Normalize(vecAxisWorld);
                    Quaternion qDelta = Quaternion.CreateFromAxisAngle(vecAxisWorld, -fAngle);
                    m_qRotation = Quaternion.Normalize(qDelta * m_qRotation);
                }
            }

            void Pan(Vector2 vecDeltaPx)
            {
                if (vecDeltaPx.LengthSquared() < 1e-12f) 
                    return;

                // Convert pixel motion to world units at the pivot depth:
                float fWorldPerPixel = PAN_SPEED * float.Tan(m_fVerticalFoV * 0.5f) * m_fDistance / float.Max(1, m_vecViewport.X);

                CalculateBasis( out Vector3 vecRight, 
                                out Vector3 vecUp, 
                                out _); // forward = -back

                Vector3 vecDeltaWorld   = (-vecDeltaPx.X * fWorldPerPixel) * vecRight
                                        + ( vecDeltaPx.Y * fWorldPerPixel) * vecUp;

                m_vecPivot += vecDeltaWorld;
                m_vecEye   += vecDeltaWorld;
            }

            Vector3     m_vecEye        = new Vector3(0, 0, 1);
            Vector3     m_vecPivot      = Vector3.Zero;
            Quaternion  m_qRotation     = Quaternion.Identity;

            float       m_fVerticalFoV  = 45f * float.Pi / 180f;
            float       m_fAspect       = 1f;
            float       m_fDistance     = 1f;

            Vector2     m_vecViewport   = new (1920,1080);
            float       m_fSceneRadius  = 1000;

            Matrix4x4   m_matV;
            Matrix4x4   m_matP;

            const float ZOOM_SPEED      = 0.1f;
            const float ROTATE_SPEED    = 15f;
            const float PAN_SPEED       = 10f;
            const float PIXELS_PER_SPIN = 900f; // 900 px ≈ 360°
        }
    }
}
    