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

using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PicoGK
{
    public partial class Viewer
    {
        public Viewer(  string strTitle,
                        Vector2 vecSize,
                        ILog xLog)
        {
            m_xLog = xLog;

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

            hThis = _hCreate(   strTitle,
                                vecSize,
                                m_fnInfoCB,
                                m_fnUpdateCB,
                                m_fnKeyPressedCB,
                                m_fnMouseMovedCB,
                                m_fnMouseButtonCB,
                                m_fnScrollWheelCB,
                                m_fnWindowSizeCB);
         
            Debug.Assert(hThis != IntPtr.Zero);
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
                // Set idle flag, if we did not encounter
                // any actions for one entire poll period
                if (m_oActions.Count() == 0)
                    m_bIdle = true;

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
                        _RequestScreenShot(hThis, strScreenShotPath);
                        bUpdateNeeded = true;
                    }
                }
            }

            if (bUpdateNeeded)
                _RequestUpdate(hThis);

            Debug.Assert(_bIsValid(hThis));
            return _bPoll(hThis);
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
            using Stream oStream = File.OpenRead(strFilePath);
            LoadLightSetup(oStream);
        }

        public void LoadLightSetup(Stream oStream)
        {
            using (ZipArchive oZip = new(oStream, ZipArchiveMode.Read))
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

                try
                {
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
                        m_oActions.Enqueue(new LoadLightSetupAction(    m_xLog,
                                                                        abyDiffuseData,
                                                                        abySpecularData));
                    }
                }

                catch
                {
                    Console.WriteLine("Unable to load lights");
                }
            }
        }

        public void Add(    in Voxels vox,
                            int nGroupID = 0)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new AddVoxelsAction(vox, nGroupID));
            }
        }

        public void Remove(Voxels vox)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new RemoveVoxelsAction(vox));
            }
        }

        public void SetObjectMatrix(    Voxels vox,
                                        in Matrix4x4 mat)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new SetVoxelsMatrixAction(vox, mat));
            }
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

        public void SetObjectMatrix(    Mesh msh,
                                        in Matrix4x4 mat)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new SetMeshMatrixAction(msh, mat));
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

        public void SetObjectMatrix(    PolyLine poly,
                                        in Matrix4x4 mat)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new SetPolyLineMatrixAction(poly, mat));
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

        public void SetGroupMatrix( int nGroupID,
                                    Matrix4x4 mat)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new SetGroupMatrixAction(    nGroupID,
                                                                mat));
            }
        }

        public void EnableOverhangWarning(  int nGroupID,
                                            int nWarningAngleDeg,
                                            int nErrorAngleDeg)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new EnableGroupOverhangWarningAction(    nGroupID, 
                                                                            nWarningAngleDeg, 
                                                                            nErrorAngleDeg));
            }   
        }

        public void DisableOverhangWarning(int nGroupID)
        {
            lock (m_oActions)
            {
                m_oActions.Enqueue(new DisableGroupOverhangWarningAction(nGroupID));
            }   
        }

        public BBox3 oBBox()
        {
            BBox3 oBBox = new();
            _GetBoundingBox(hThis, ref oBBox);
            return oBBox;
        }

        public void SetBackgroundColor(ColorFloat clr)
        {
            m_clrBackground = clr;
            RequestUpdate();
        }

        public void SetFov(float fAngle)
        {
            m_oCamera.SetFov(fAngle);
            RequestUpdate();
        }

        /// <summary>
        /// Allows you to query if all viewer actions are complete
        /// </summary>
        /// <returns>t
        /// true: No more viewer actions pending
        /// false: Viewer actions still pending (busy)</returns>
        public bool bIsIdle()
        {
            lock (m_oActions)
            {
                if (m_oActions.Count() > 0)
                    m_bIdle = false;
            }

            return m_bIdle;
        }

        bool m_bIdle = false;

        int m_iMainThreadID = -1;

        ColorFloat m_clrBackground = new(0.3f);

    
               void DoRemove(Mesh msh)
        {
            _RemoveMesh(    msh.lib.hThis,
                            hThis,
                            msh.hThis);
        }

        ILog m_xLog;

        CamPerspectiveArcball  m_oCamera               = new(45);
        bool            m_bEmptyViewer          = true;
        bool            m_bHadCamInteractions   = false;

        bool            m_bMouseDrag            = false;
        Vector2         m_vecMousePos           = Vector2.Zero;

        ///////// Internals

        void InfoCB(    string strMessage,
                        bool bFatalError)
        {
            m_xLog.Log(strMessage);
        }

        void UpdateCB(  IntPtr          hViewer,
                        in Vector2      vecViewport,
                        ref ColorFloat  clrBackground,
                        ref Matrix4x4   matVP,
                        ref Vector3     vecEye)
        {
            m_oCamera.SetViewPort(vecViewport);
            
            try
            {
                Debug.Assert(hViewer == hThis);

                BBox3 oBox = oBBox();

                if (!oBox.bIsEmpty())
                {
                    if (    m_bEmptyViewer ||         // Viewer had no content before
                            (!m_bHadCamInteractions))   // Viewer was never interacted with
                    {
                        m_oCamera.ZoomToFit(oBox);
                    }

                    m_bEmptyViewer = false;
                }

                matVP           =   m_oCamera.matVP;
                vecEye          =   m_oCamera.vecEye;
                clrBackground   =   m_clrBackground;
            }

            catch (Exception e)
            {
                m_xLog.Log($"Caught exception in Viewer update callback:\n{e.ToString()}\n");
            }
        }

        void KeyPressedCB(  IntPtr hViewer,
                            int iKey,
                            int iScancode,
                            int iAction,
                            int iModifiers)
        {
            Debug.Assert(hViewer == hThis);

            EKeys eKey = (EKeys)iKey;

            if ((iAction == 0) || (iAction == 1)) // ignore 2, which is repeat, only down and up
            {
                foreach (IKeyHandler xHandler in m_oKeyHandlers)
                {
                    if (xHandler.bHandleEvent(  this,
                                                eKey, iAction == 1, // Pressed
                                                (iModifiers & 0x0001) != 0,
                                                (iModifiers & 0x0002) != 0,
                                                (iModifiers & 0x0004) != 0,
                                                (iModifiers & 0x0008) != 0))
                        return; // Handled
                }
            }
        }

        void MouseMovedCB(  IntPtr hViewer,
                            in Vector2 vecMousePos,
                            bool        bShift,
                            bool        bCtrl,
                            bool        bAlt,
                            bool        bCmd)
        {
            Debug.Assert(hViewer == hThis);

            if (m_bMouseDrag)
            {
                Vector2 vecDist         = vecMousePos - m_vecMousePos;
                Camera.EDragType eType  = Camera.EDragType.ROTATE;

                if (bShift)
                    eType = Camera.EDragType.PAN;
                else if (bAlt)
                    eType = Camera.EDragType.SPIN;

                m_oCamera.MouseDrag(vecDist, eType);
                m_bHadCamInteractions = true;

                lock (m_oAnims)
                {
                    m_oAnims.Clear();
                }

                RequestUpdate();
            }

            m_vecMousePos = vecMousePos;
        }

        void MouseButtonCB( IntPtr hViewer,
                            int iButton,
                            int iAction,
                            int iModifiers,
                            in Vector2 vecMousePos)
        {
            Debug.Assert(hViewer == hThis);
            m_vecMousePos = vecMousePos;

            if (iAction == 1)
            {
                m_bMouseDrag = true;
                
                lock (m_oAnims)
                {
                    m_oAnims.Clear();
                }

                RequestUpdate();
            }
            else if (iAction == 0)
            {
                m_bMouseDrag = false;
            }
        }

        void ScrollWheelCB( IntPtr      hViewer,
                            in Vector2  vecScrollWheel,
                            in Vector2  vecMousePos,
                            bool        bShift,
                            bool        bCtrl,
                            bool        bAlt,
                            bool        bCmd)
        {
            Debug.Assert(hViewer == hThis);

            if (bCtrl || bCmd)
            {
                m_oCamera.Scroll(vecScrollWheel);
            }
            else
            {
                Camera.EDragType eType = Camera.EDragType.ROTATE;

                if (bShift)
                    eType = Camera.EDragType.PAN;
                else if (bAlt)
                    eType = Camera.EDragType.SPIN;

                m_oCamera.MouseDrag(vecScrollWheel * 3, eType);
            }

            m_bHadCamInteractions = true;
            RequestUpdate();
        }

        void WindowSizeCB(  IntPtr hViewer,
                            in Vector2 vecWindowSize)
        {
            Debug.Assert(hViewer == hThis);

            m_oCamera.SetViewPort(vecWindowSize);

            RequestUpdate();
        }
    }
}
    