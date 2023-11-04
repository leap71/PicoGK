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
using System.Runtime.InteropServices;

namespace PicoGK
{
    public partial class Viewer
    {
        public interface IViewerAction
        {
            void Do(Viewer oViewer);
        };

        Queue<IViewerAction> m_oActions = new();

        class SetGroupVisibleAction : IViewerAction
        {
            public SetGroupVisibleAction(   int nGroupID,
                                            bool bVisible)
            {
                m_nGroupID = nGroupID;
                m_bVisible = bVisible;
            }

            public void Do(Viewer oViewer)
            {
                _SetGroupVisible(   oViewer.m_hThis,
                                    m_nGroupID,
                                    m_bVisible);
            }

            int m_nGroupID;
            bool m_bVisible;
        }

        class SetGroupStaticAction : IViewerAction
        {
            public SetGroupStaticAction(    int nGroupID,
                                            bool bStatic)
            {
                m_nGroupID  = nGroupID;
                m_bStatic   = bStatic;
            }

            public void Do(Viewer oViewer)
            {
                _SetGroupStatic(    oViewer.m_hThis,
                                    m_nGroupID,
                                    m_bStatic);
            }

            int m_nGroupID;
            bool m_bStatic;
        }

        class SetGroupMaterialAction : IViewerAction
        {
            public SetGroupMaterialAction(  int         nGroupID,
                                            ColorFloat  clr,
                                            float       fMetallic,
                                            float       fRoughness)
            {
                m_nGroupID = nGroupID;
                m_clr = clr;
                m_fMetallic = fMetallic;
                m_fRoughness = fRoughness;
            }

            public void Do(Viewer oViewer)
            {
                _SetGroupMaterial(  oViewer.m_hThis,
                                    m_nGroupID,
                                    m_clr,
                                    m_fMetallic,
                                    m_fRoughness);
            }

            int         m_nGroupID;
            ColorFloat  m_clr;
            float       m_fMetallic;
            float       m_fRoughness;
        }

        class SetGroupMatrixAction : IViewerAction
        {
            public SetGroupMatrixAction(    int nGroupID,
                                            Matrix4x4 mat)
            {
                m_nGroupID = nGroupID;
                m_mat = mat;
            }

            public void Do(Viewer oViewer)
            {
                _SetGroupMatrix(    oViewer.m_hThis,
                                    m_nGroupID,
                                    m_mat);
            }

            int         m_nGroupID;
            Matrix4x4   m_mat;
        }

        class RequestUpdateAction : IViewerAction
        {
            public RequestUpdateAction()
            {
                
            }

            public void Do(Viewer oViewer)
            {
                _RequestUpdate(oViewer.m_hThis);
            }
        }

        class RequestScreenShotAction : IViewerAction
        {
            public RequestScreenShotAction(string strScreenShotPath)
            {
                m_strScreenShotPath = strScreenShotPath;
            }

            public void Do(Viewer oViewer)
            {
                _RequestScreenShot( oViewer.m_hThis,
                                    m_strScreenShotPath);
            }

            string m_strScreenShotPath;
        }

        class AddMeshAction : IViewerAction
        {
            public AddMeshAction(   Mesh msh,
                                    int nGroupID)
            {
                m_msh = msh;
                m_nGroupID = nGroupID;
            }

            public void Do(Viewer oViewer)
            {
                oViewer.m_oBBox.Include(m_msh.oBoundingBox());

                lock (oViewer.m_oMeshes)
                {
                    oViewer.m_oMeshes.Add(m_msh);
                }

                _AddMesh(   oViewer.m_hThis,
                            m_nGroupID,
                            m_msh.m_hThis);
            }

            Mesh m_msh;
            int m_nGroupID;
        }

        class RemoveMeshAction : IViewerAction
        {
            public RemoveMeshAction(Mesh msh)
            {
                m_msh = msh;
            }

            public void Do(Viewer oViewer)
            {
                oViewer.DoRemove(m_msh);
            }

            Mesh m_msh;
        }

        class AddPolyLineAction : IViewerAction
        {
            public AddPolyLineAction(PolyLine poly,
                                        int nGroupID)
            {
                m_poly = poly;
                m_nGroupID = nGroupID;
            }

            public void Do(Viewer oViewer)
            {
                oViewer.m_oBBox.Include(m_poly.oBoundingBox());

                _AddPolyLine(oViewer.m_hThis,
                                m_nGroupID,
                                m_poly.m_hThis);

                lock (oViewer.m_oPolyLines)
                {
                    oViewer.m_oPolyLines.Add(m_poly);
                }
            }

            PolyLine m_poly;
            int m_nGroupID;
        }

        class RemovePolyLineAction : IViewerAction
        {
            public RemovePolyLineAction(PolyLine poly)
            {
                m_poly = poly;
            }

            public void Do(Viewer oViewer)
            {
                _RemovePolyLine(    oViewer.m_hThis,
                                    m_poly.m_hThis);
            }

            PolyLine m_poly;
        }

        class RemoveAllObjectsAction : IViewerAction
        {
            public void Do(Viewer oViewer)
            {
                lock (oViewer.m_oPolyLines)
                {
                    foreach (PolyLine oPoly in oViewer.m_oPolyLines)
                    {
                        _RemovePolyLine(oViewer.m_hThis, oPoly.m_hThis);
                    }

                    oViewer.m_oPolyLines.Clear();
                }

                lock (oViewer.m_oVoxels)
                {
                    oViewer.m_oVoxels.Clear();
                }

                lock (oViewer.m_oMeshes)
                {
                    foreach (Mesh oMesh in oViewer.m_oMeshes)
                    {
                        _RemoveMesh(oViewer.m_hThis, oMesh.m_hThis);
                    }

                    oViewer.m_oMeshes.Clear();
                }
            }
        }

        class LoadLightSetupAction : IViewerAction
        {
            public LoadLightSetupAction(    byte [] abyDiffuseDds,
                                            byte [] abySpecularDds)
            {
                m_abyDiffuseDds     = abyDiffuseDds;
                m_abySpecularDds    = abySpecularDds;
            }

            public void Do(Viewer oViewer)
            {
                if (!_bLoadLightSetup(  oViewer.m_hThis,
                                        m_abyDiffuseDds,
                                        m_abyDiffuseDds.Length,
                                        m_abySpecularDds,
                                        m_abySpecularDds.Length))
                {
                    Library.Log($"Failed to load light setup");
                }
            }

            byte [] m_abyDiffuseDds;
            byte [] m_abySpecularDds;
        }

        public class RotateToNextRoundAngleAction : IViewerAction
        {
            public enum EDir
            {
                Dir_Up,
                Dir_Down,
                Dir_Left,
                Dir_Right
            }

            public RotateToNextRoundAngleAction(EDir eDir)
            {
                m_eDir = eDir;
            }

            public void Do(Viewer oViewer)
            {
                oViewer.RemoveAllAnimations();

                Vector2 vecTo = new Vector2(    oViewer.m_fOrbit,
                                                oViewer.m_fElevation);

                switch (m_eDir)
                {
                    case EDir.Dir_Left:
                    case EDir.Dir_Right:
                        {
                            int iStep = (int)(vecTo.X / 45.0f);

                            float fStep = (m_eDir == EDir.Dir_Left) ? 45f : -45f;
                            vecTo.X = (float)iStep * 45f + fStep;
                        }
                        break;

                    case EDir.Dir_Up:
                    case EDir.Dir_Down:
                        {
                            int iStep = (int)(vecTo.Y / 45.0f);
                            float fStep = (m_eDir == EDir.Dir_Up) ? 45f : -45f;
                            vecTo.Y = (float)iStep * 45f + fStep;
                        }
                        break;
                }

                Animation.IAction xAction
                    = new AnimViewRotate(   oViewer,
                                            new Vector2(    oViewer.m_fOrbit,
                                                            oViewer.m_fElevation),
                                            vecTo);

                Animation oAnim
                    = new Animation(    xAction, 0.7f,
                                        Animation.EType.Once,
                                        Easing.EEasing.CUBIC_OUT);

                oViewer.AddAnimation(oAnim);
            }

            EDir m_eDir;
        }
    }
}
    