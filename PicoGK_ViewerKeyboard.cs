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

namespace PicoGK
{
    public partial class Viewer
    {
        LinkedList<IKeyHandler> m_oKeyHandlers = new();
        KeyHandler m_oHandler = new();

        public void AddKeyHandler(IKeyHandler xKeyHandler)
        {
            // Add to the beginning, so the last added is processed first
            m_oKeyHandlers.AddFirst(xKeyHandler);
        }

        public interface IKeyHandler
        {
            bool bHandleEvent(  Viewer oViewer,
                                EKeys eKey,
                                bool bPressed,
                                bool bShift,
                                bool bCtrl,
                                bool bAlt,
                                bool bCmd);
        }

        public enum EKeys
        {
            Key_Space       = 32,
            Key_0           = 48,
            Key_1,
            Key_2,
            Key_3,
            Key_4,
            Key_5,
            Key_6,
            Key_7,
            Key_8,
            Key_9,
            Key_A           = 65,
            Key_B,
            Key_C,
            Key_D,
            Key_E,
            Key_F,
            Key_G,
            Key_H,
            Key_I,
            Key_J,
            Key_K,
            Key_L,
            Key_M,
            Key_N,
            Key_O,
            Key_P,
            Key_Q,
            Key_R,
            Key_S,
            Key_T,
            Key_U,
            Key_V,
            Key_W,
            Key_X,
            Key_Y,
            Key_Z           = 90,
            Key_ESC         = 256,
            Key_Enter,
            Key_Tab,
            Key_Backspace,
            Key_Insert,
            Key_Delete,
            Key_Right,
            Key_Left,
            Key_Down,
            Key_Up,
            Key_PgUp,
            Key_PgDn,
            Key_Home,
            Key_End         = 269,
            Key_F1          = 290,
            Key_F2,
            Key_F3,
            Key_F4,
            Key_F5,
            Key_F6,
            Key_F7,
            Key_F8,
            Key_F9,
            Key_F10,
            Key_F11,
            Key_F12
        };

        public class KeyAction
        {
            public KeyAction(   IViewerAction xAction,
                                EKeys eKey,
                                bool bPressed = false, // Handle on release by default
                                bool bShift = false,
                                bool bCtrl = false,
                                bool bAlt = false,
                                bool bCmd = false)
            {
                m_xAction = xAction;
                m_eKey = eKey;
                m_bPressed = bPressed;
                m_bShift = bShift;
                m_bCtrl = bCtrl;
                m_bAlt = bAlt;
                m_bCmd = bCmd;
            }

            public bool bKeyEquals( EKeys   eKey,
                                    bool    bPressed,
                                    bool    bShift,
                                    bool    bCtrl,
                                    bool    bAlt,
                                    bool    bCmd)
            {
                return (    (m_eKey     == eKey)        &&
                            (m_bPressed == bPressed)    &&
                            (m_bShift   == bShift)      &&
                            (m_bCtrl    == bCtrl)       &&
                            (m_bAlt     == bAlt)        &&
                            (m_bCmd     == bCmd));
            }

            public void Do(Viewer oViewer)
            {
                m_xAction.Do(oViewer);
            }

            IViewerAction   m_xAction;
            EKeys           m_eKey;
            bool            m_bPressed;
            bool            m_bShift;
            bool            m_bCtrl;
            bool            m_bAlt;
            bool            m_bCmd;
        }

        public class KeyHandler : IKeyHandler
        {
            public void AddAction(KeyAction oAction)
            {
                m_oKeyActions.AddFirst(oAction);
            }

            LinkedList<KeyAction> m_oKeyActions = new();

            public bool bHandleEvent(   Viewer  oViewer,
                                        EKeys   eKey,
                                        bool    bPressed,
                                        bool    bShift,
                                        bool    bCtrl,
                                        bool    bAlt,
                                        bool    bCmd)
            {
                foreach (KeyAction oAction in m_oKeyActions)
                {
                    if (oAction.bKeyEquals(     eKey,
                                                bPressed,
                                                bShift,
                                                bCtrl,
                                                bAlt,
                                                bCmd))
                    {
                        oAction.Do(oViewer);
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
    