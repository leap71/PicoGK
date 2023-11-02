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
using System.IO.Compression;
using System.Numerics;

namespace PicoGK
{
    public partial class Viewer
    {
        public Viewer(  string strTitle,
                        Vector2 vecSize)
        {
            m_iMainThreadID = Environment.CurrentManagedThreadId;

            m_oHandler.AddAction(new KeyAction(
                                    new RotateToNextRoundAngleAction(
                                        RotateToNextRoundAngleAction.EDir.Dir_Down),
                                        EKeys.Key_Down));

            m_oHandler.AddAction(new KeyAction(
                                    new RotateToNextRoundAngleAction(
                                        RotateToNextRoundAngleAction.EDir.Dir_Up),
                                        EKeys.Key_Up));

            m_oHandler.AddAction(new KeyAction(
                                    new RotateToNextRoundAngleAction(
                                        RotateToNextRoundAngleAction.EDir.Dir_Left),
                                        EKeys.Key_Left));

            m_oHandler.AddAction(new KeyAction(
                                    new RotateToNextRoundAngleAction(
                                        RotateToNextRoundAngleAction.EDir.Dir_Right),
                                        EKeys.Key_Right));

            AddKeyHandler(m_oHandler);

            // Assign the delegate functions, to make sure the
            // garbage collector doesn't destroy them during the
            // lifetime of the objects

            m_fnInfoCB          = InfoCB;
            m_fnUpdateCB        = UpdateCB;
            m_fnKeyPressedCB    = KeyPressedCB;
            m_fnMouseMovedCB    = MouseMovedCB;
            m_fnMouseButtonCB   = MouseButtonCB;
            m_fnScrollWheelCB   = ScrollWheelCB;
            m_fnWindowSizeCB    = WindowSizeCB;

            m_hThis = _hCreate( strTitle,
                                vecSize,
                                m_fnInfoCB,
                                m_fnUpdateCB,
                                m_fnKeyPressedCB,
                                m_fnMouseMovedCB,
                                m_fnMouseButtonCB,
                                m_fnScrollWheelCB,
                                m_fnWindowSizeCB);
         
            Debug.Assert(m_hThis != IntPtr.Zero);
        }

        public bool bPoll()
        {
            Debug.Assert(m_iMainThreadID == Environment.CurrentManagedThreadId);
            // Call this function only from the main() thread of the application

            Thread.Yield();

            bool bUpdateNeeded = false;

            lock (m_oAnims)
            {
                if (m_oAnims.bPulse())
                    bUpdateNeeded = true;
            }

            lock (m_oActions)
            {
                while (m_oActions.Count > 0)
                {
                    IViewerAction oAction = m_oActions.Dequeue();
                    oAction.Do(this);
                    bUpdateNeeded = true;
                }
            }

            lock (m_oTLLock)
            {
                if (m_oTimeLapse != null)
                {
                    if (m_oTimeLapse.bDue(out string strScreenShotPath))
                    {
                        _RequestScreenShot(m_hThis, strScreenShotPath);
                        bUpdateNeeded = true;
                    }
                }
            }

            if (bUpdateNeeded)
                _RequestUpdate(m_hThis);

            Debug.Assert(_bIsValid(m_hThis));
            return _bPoll(m_hThis);
        }

        public void RequestUpdate()
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new RequestUpdateAction());
            }
        }

        public void LoadLightSetup(string strFilePath)
        {
            using (ZipArchive oZip = ZipFile.OpenRead(strFilePath))
            {
                // Find the entry for the diffTexture
                ZipArchiveEntry? oDiffuseEntry = oZip.GetEntry("Diffuse.dds");
                if (oDiffuseEntry == null)
                {
                    throw new FileNotFoundException("Diffuse.dds texture entry not found in the ZIP archive.");
                }

                // Find the entry for the specTexture
                ZipArchiveEntry? oSpecularEntry = oZip.GetEntry("Specular.dds");
                if (oSpecularEntry == null)
                {
                    throw new FileNotFoundException("SpecTexture entry not found in the ZIP archive.");
                }

                byte[] abyDiffuseData;
                using (Stream oDiffuseStream = oDiffuseEntry.Open())
                {
                    using (MemoryStream oDiffuseMemStream = new MemoryStream())
                    {
                        oDiffuseStream.CopyTo(oDiffuseMemStream);
                        abyDiffuseData = oDiffuseMemStream.ToArray();
                    }
                }

                byte[] abySpecularData;
                using (Stream oSpecularStream = oSpecularEntry.Open())
                {
                    using (MemoryStream oSpecularMemStream = new MemoryStream())
                    {
                        oSpecularStream.CopyTo(oSpecularMemStream);
                        abySpecularData = oSpecularMemStream.ToArray();
                    }
                }

                lock (m_oActions)
                {
                    m_oActions.Enqueue(new LoadLightSetupAction(    abyDiffuseData,
                                                                    abySpecularData));
                }
            }
        }

        public void Add(    in Voxels vox,
                            int nGroupID = 0)
        {
            Mesh msh = new Mesh(vox);

            lock (m_oVoxels)
            {
                m_oVoxels.Add(vox, msh);
            }

            Add(msh, nGroupID);
        }

        public void Remove(Voxels vox)
        {
            Mesh? msh;

            lock (m_oVoxels)
            {
                if (!m_oVoxels.TryGetValue(vox, out msh))
                {
                    throw new Exception("Tried to remove voxels that were never added");
                }

                m_oVoxels.Remove(vox);
            }

            Remove(msh);
        }

        public void Add(    Mesh msh,
                            int nGroupID = 0)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new AddMeshAction(msh, nGroupID));
            }
        }

        public void Remove(Mesh msh)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new RemoveMeshAction(msh));
            }
        }

        public void Add(    PolyLine oPoly,
                            int nGroupID = 0)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new AddPolyLineAction(oPoly, nGroupID));
            }
        }

        public void Remove(PolyLine oPoly)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new RemovePolyLineAction(oPoly));
            }
        }

        public void RemoveAllObjects()
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new RemoveAllObjectsAction());
            }
        }

        public void RequestScreenShot(string strScreenShotPath)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new RequestScreenShotAction(strScreenShotPath));
            }
        }

        public void SetGroupVisible(int nGroupID,
                                        bool bVisible)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new SetGroupVisibleAction(nGroupID, bVisible));
            }
        }

        public void SetGroupStatic( int nGroupID,
                                    bool bStatic)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new SetGroupStaticAction(nGroupID, bStatic));
            }
        }

        public void SetGroupMaterial    (int        nGroupID,
                                        ColorFloat  clr,
                                        float       fMetallic,
                                        float       fRoughness)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new SetGroupMaterialAction(  nGroupID,
                                                                clr,
                                                                fMetallic,
                                                                fRoughness));
            }
        }

        public void SetGroupMatrix(int nGroupID,
                                    Matrix4x4 mat)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new SetGroupMatrixAction(    nGroupID,
                                                                mat));
            }
        }

        public void SetBackgroundColor(ColorFloat clr)
        {
            m_clrBackground = clr;
            RequestUpdate();
        }

        public void AdjustViewAngles(   float fOrbitRelative,
                                        float fElevationRelative)
        {
            SetViewAngles(  m_fOrbit + fOrbitRelative,
                            m_fElevation += fElevationRelative);
        }

        public void SetViewAngles(  float fOrbit,
                                    float fElevation)
        {
            m_fElevation = fElevation;

            if (m_fElevation > 180.0f)
                fElevation = 90.0f;
            else if (m_fElevation < 180.0f)
                fElevation = -90.0f;

            m_fOrbit = fOrbit;

            while (m_fOrbit > 360.0f)
                m_fOrbit -= 360.0f;
            while (m_fOrbit < 0.0f)
                m_fOrbit += 360.0f;

            RequestUpdate();
        }

        public void SetFov(float fAngle)
        {
            m_fFov = fAngle;
            RequestUpdate();
        }

        public void LogStatistics()
        {
            float fTriangles = 0;
            float fVertices = 0;
            ulong nMeshes = 0;

            lock (m_oMeshes)
            {
                foreach (Mesh msh in m_oMeshes)
                {
                    fTriangles += (float)msh.nTriangleCount();
                    fVertices += (float)msh.nVertexCount();
                    nMeshes++;
                }
            }

            fTriangles /= 1000000.0f;
            fVertices /= 1000000.0f;

            Library.Log($"Viewer Stats:");
            Library.Log($"   Number of Meshes: {nMeshes}");
            lock (m_oVoxels)
            {
                Library.Log($"   Voxel Objects:    {m_oVoxels.Count()}");
            }
            Library.Log($"   Total Triangles:  {fTriangles:F1} mio");
            Library.Log($"   Total Vertices:   {fVertices:F1} mio");
        }

        public float m_fElevation = 30.0f;
        public float m_fOrbit = 45.0f;
        float m_fFov = 45.0f;
        float m_fZoom = 1.0f;
        bool m_bPerspective = true;

        int m_iMainThreadID = -1;

        ColorFloat m_clrBackground = new(0.3f);
        List<Mesh> m_oMeshes = new();
        List<PolyLine> m_oPolyLines = new();
        Dictionary<Voxels, Mesh> m_oVoxels = new();
        BBox3 m_oBBox = new();

        void RecalculateBoundingBox()
        {
            Debug.Assert(m_iMainThreadID == Environment.CurrentManagedThreadId);
            // Call this function only from the main() thread of the application

            // start from fresh
            m_oBBox = new();

            lock (m_oMeshes)
            {
                foreach (Mesh msh in m_oMeshes)
                {
                    m_oBBox.Include(msh.oBoundingBox());
                }
            }

            lock (m_oPolyLines)
            {
                foreach (PolyLine poly in m_oPolyLines)
                {
                    m_oBBox.Include(poly.oBoundingBox());
                }
            }
        }

        void DoRemove(Mesh msh)
        {
            Debug.Assert(m_iMainThreadID == Environment.CurrentManagedThreadId);
            // Call this function only from the main() thread of the application

            lock (m_oMeshes)
            {
                if (!m_oMeshes.Contains(msh))
                    throw new Exception("Tried to remove mesh that was never added");

                m_oMeshes.Remove(msh);

                _RemoveMesh(m_hThis,
                                msh.m_hThis);
            }

            RecalculateBoundingBox();
            RequestUpdate();
        }

        void DoRemove(PolyLine poly)
        {
            Debug.Assert(m_iMainThreadID == Environment.CurrentManagedThreadId);
            // Call this function only from the main() thread of the application

            lock (m_oPolyLines)
            {
                if (!m_oPolyLines.Contains(poly))
                    throw new Exception("Tried to remove mesh that was never added");

                m_oPolyLines.Remove(poly);

                _RemovePolyLine(    m_hThis,
                                    poly.m_hThis);
            }

            RecalculateBoundingBox();
            RequestUpdate();
        }

        Matrix4x4 m_matModelTrans           = Matrix4x4.Identity;
        Matrix4x4 m_matModelViewProjection  = Matrix4x4.Identity;
        Matrix4x4 m_matModelViewStatic      = Matrix4x4.Identity;
        Matrix4x4 m_matProjectionStatic     = Matrix4x4.Identity;
        Matrix4x4 m_matStatic               = Matrix4x4.Identity;
        Vector3 m_vecEye                    = new Vector3(1.0f);
        Vector3 m_vecEyeStatic              = new Vector3(0f, 10f, 0f);
        Vector2 m_vecPrevPos                = new();
        bool m_bOrbit                       = false;

        ///////// Internals

        void InfoCB(    string strMessage,
                        bool bFatalError)
        {
            Library.Log(strMessage);
        }

        void UpdateCB(  IntPtr hViewer,
                        in Vector2 vecViewport,
                        ref ColorFloat clrBackground,
                        ref Matrix4x4 matModelViewProjection,
                        ref Matrix4x4 matModelTransform,
                        ref Matrix4x4 matStatic,
                        ref Vector3 vecEyePosition,
                        ref Vector3 vecEyeStatic)
        {
            try
            {
                Debug.Assert(hViewer == m_hThis);

                Vector3 vecSceneCenter = m_oBBox.vecCenter();

                double fR = ((m_oBBox.vecMax - vecSceneCenter).Length() * 3.0f) * m_fZoom;
                double fRElev = Math.Cos((double)m_fElevation * Math.PI / 180.0f) * fR;

                m_vecEye.X = (float)(Math.Cos((double)m_fOrbit * Math.PI / 180.0) * fRElev);
                m_vecEye.Y = (float)(Math.Sin((double)m_fOrbit * Math.PI / 180.0) * fRElev);
                m_vecEye.Z = (float)(Math.Sin((double)m_fElevation * Math.PI / 180.0) * fR);

                float fFar = (vecSceneCenter - m_vecEye).Length() * 2.0f;

                Matrix4x4 matModelView = Utils.matLookAt(m_vecEye, vecSceneCenter);

                Matrix4x4 matProjection;

                if (m_bPerspective)
                {
                    matProjection = Matrix4x4.CreatePerspectiveFieldOfView(
                        (float)(m_fFov * Math.PI / 180.0),
                        vecViewport.X / vecViewport.Y,
                        0.1f,
                        fFar);
                }
                else
                {
                    matProjection = Matrix4x4.CreateOrthographic(   m_oBBox.vecSize().X * 2,
                                                                    m_oBBox.vecSize().Y * 2,
                                                                    0.1f,
                                                                    fFar);
                }

                m_matModelViewStatic  = Utils.matLookAt(m_vecEyeStatic, new Vector3(0, 0, 0));
                m_matProjectionStatic = Matrix4x4.CreateOrthographic(100f * vecViewport.X / vecViewport.Y, 100f, 0.1f, 100f);

                m_matModelViewProjection    = matModelView * matProjection;
                m_matStatic                 = m_matModelViewStatic * m_matProjectionStatic;

                vecEyeStatic                = m_vecEyeStatic;
                vecEyePosition              = m_vecEye;
                matStatic                   = m_matStatic;
                matModelViewProjection      = m_matModelViewProjection;
                matModelTransform           = m_matModelTrans;
                clrBackground               = m_clrBackground;
            }

            catch (Exception e)
            {
                Library.Log($"Caught exception in Viewer update callback:\n{e.ToString()}\n");
            }
        }

        void KeyPressedCB(IntPtr hViewer,
                            int iKey,
                            int iScancode,
                            int iAction,
                            int iModifiers)
        {
            Debug.Assert(hViewer == m_hThis);

            EKeys eKey = (EKeys)iKey;

            if ((iAction == 0) || (iAction == 1)) // ignore 2, which is repeat, only down and up
            {
                foreach (IKeyHandler xHandler in m_oKeyHandlers)
                {
                    if (xHandler.bHandleEvent(this,
                                                eKey, iAction == 1, // Pressed
                                                (iModifiers & 0x0001) != 0,
                                                (iModifiers & 0x0002) != 0,
                                                (iModifiers & 0x0004) != 0,
                                                (iModifiers & 0x0008) != 0))
                        return; // Handled
                }
            }
        }

        void MouseMovedCB(IntPtr hViewer,
                            in Vector2 vecMousePos)
        {
            Debug.Assert(hViewer == m_hThis);
            if (m_bOrbit)
            {
                Vector2 vecDist = vecMousePos - m_vecPrevPos;
                AdjustViewAngles(-vecDist.X / 2.0f, vecDist.Y / 2);
                m_vecPrevPos = vecMousePos;

                lock (m_oAnims)
                {
                    m_oAnims.Clear();
                }
            }
        }

        void MouseButtonCB(IntPtr hViewer,
                            int iButton,
                            int iAction,
                            int iModifiers,
                            in Vector2 vecMousePos)
        {
            Debug.Assert(hViewer == m_hThis);
            if (iAction == 1)
            {
                m_bOrbit = true;
                m_vecPrevPos = vecMousePos;

                lock (m_oAnims)
                {
                    m_oAnims.Clear();
                }
            }
            else if (iAction == 0)
            {
                m_bOrbit = false;
            }
        }

        void ScrollWheelCB(IntPtr hViewer,
                            in Vector2 vecScrollWheel,
                            in Vector2 vecMousePos)
        {
            Debug.Assert(hViewer == m_hThis);

            m_fZoom -= vecScrollWheel.Y / 50f;

            if (m_fZoom < 0.1f)
                m_fZoom = 0.1f;

            RequestUpdate();
        }

        void WindowSizeCB(  IntPtr hViewer,
                            in Vector2 vecWindowSize)
        {
            Debug.Assert(hViewer == m_hThis);

            RequestUpdate();
        }
    }
}
    