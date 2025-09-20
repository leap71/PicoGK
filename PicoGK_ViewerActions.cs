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
                _SetGroupVisible(   oViewer.hThis,
                                    m_nGroupID,
                                    m_bVisible);
            }

            int m_nGroupID;
            bool m_bVisible;
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
                _SetGroupMaterial(  oViewer.hThis,
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
                _SetGroupMatrix(    oViewer.hThis,
                                    m_nGroupID,
                                    m_mat);
            }

            int         m_nGroupID;
            Matrix4x4   m_mat;
        }

        class EnableGroupOverhangWarningAction : IViewerAction
        {
            public EnableGroupOverhangWarningAction(    int nGroupID,
                                                        int nWarningAngleDeg,
                                                        int nErrorAngleDeg)
            {
                m_nGroupID      = nGroupID;
                m_nWarningAngle = nWarningAngleDeg;
                m_nErrorAngle   = nErrorAngleDeg;
            }

            public void Do(Viewer oViewer)
            {
                _EnableGroupWarnOverhang(   oViewer.hThis,
                                            m_nGroupID,
                                            m_nWarningAngle,
                                            m_nErrorAngle);
            }

            int m_nGroupID;
            int m_nWarningAngle;
            int m_nErrorAngle;
        }

        class DisableGroupOverhangWarningAction : IViewerAction
        {
            public DisableGroupOverhangWarningAction(int nGroupID)
            {
                m_nGroupID      = nGroupID;
            }

            public void Do(Viewer oViewer)
            {
                _DisableGroupWarnOverhang(  oViewer.hThis,
                                            m_nGroupID);
            }

            int m_nGroupID;
        }

        class RequestUpdateAction : IViewerAction
        {
            public RequestUpdateAction()
            {
                
            }

            public void Do(Viewer oViewer)
            {
                _RequestUpdate(oViewer.hThis);
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
                _RequestScreenShot( oViewer.hThis,
                                    m_strScreenShotPath);
            }

            string m_strScreenShotPath;
        }

        
        class AddVoxelsAction : IViewerAction
        {
            public AddVoxelsAction( Voxels vox,
                                    int nGroupID)
            {
                m_vox = vox;
                m_nGroupID = nGroupID;
            }

            public void Do(Viewer oViewer)
            {
                _AddVoxels( m_vox.lib.hThis,
                            oViewer.hThis,
                            m_nGroupID,
                            m_vox.hThis);
            }
         
            Voxels m_vox;
            int m_nGroupID;
        }

        class RemoveVoxelsAction : IViewerAction
        {
            public RemoveVoxelsAction(Voxels vox)
            {
                m_vox = vox;
            }

            public void Do(Viewer oViewer)
            {
                _RemoveVoxels(  m_vox.lib.hThis,
                                oViewer.hThis,
                                m_vox.hThis);
            }

            Voxels m_vox;

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
                _AddMesh(   m_msh.lib.hThis,
                            oViewer.hThis,
                            m_nGroupID,
                            m_msh.hThis);
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
                _RemoveMesh(    m_msh.lib.hThis,
                                oViewer.hThis,
                                m_msh.hThis);
            }

            Mesh m_msh;
        }

        class AddPolyLineAction : IViewerAction
        {
            public AddPolyLineAction(   PolyLine poly,
                                        int nGroupID)
            {
                m_poly = poly;
                m_nGroupID = nGroupID;
            }

            public void Do(Viewer oViewer)
            {
                _AddPolyLine(   m_poly.lib.hThis,
                                oViewer.hThis,
                                m_nGroupID,
                                m_poly.hThis);

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
                _RemovePolyLine(    m_poly.lib.hThis,
                                    oViewer.hThis,
                                    m_poly.hThis);
            }

            PolyLine m_poly;
        }

        class RemoveAllObjectsAction : IViewerAction
        {
            public void Do(Viewer oViewer)
            {
                _RemoveAllObjects(oViewer.hThis);
            }
        }

        class LoadLightSetupAction : IViewerAction
        {
            public LoadLightSetupAction(    ILog xLog,
                                            byte [] abyDiffuseDds,
                                            byte [] abySpecularDds)
            {
                m_xLog              = xLog;
                m_abyDiffuseDds     = abyDiffuseDds;
                m_abySpecularDds    = abySpecularDds;
            }

            public void Do(Viewer oViewer)
            {
                if (!_bLoadLightSetup(  oViewer.hThis,
                                        m_abyDiffuseDds,
                                        m_abyDiffuseDds.Length,
                                        m_abySpecularDds,
                                        m_abySpecularDds.Length))
                {
                    m_xLog.Log($"Failed to load light setup");
                }
            }

            ILog m_xLog;
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

                /// TODO

                /*Vector2 vecTo = new Vector2(    oViewer.m_fOrbit,
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

                oViewer.AddAnimation(oAnim);*/
            }

            EDir m_eDir;
        }
    }
}
    