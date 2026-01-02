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

using SkiaSharp;

namespace PicoGK
{
    public partial class Viewer
    {
        public SideBar oCreateSideBarLeft(  int nMin,
                                            int nMax,
                                            int nDef,
                                            ColorFloat clrNormal,
                                            ColorFloat clrHovered)
        {
            if (m_oSideBarLeft is not null)
                m_oSideBarLeft.Dispose();

            m_oSideBarLeft = new(this, true, nMin, nMax, nDef, clrNormal, clrHovered);
            return m_oSideBarLeft; 
        }

        public SideBar oCreateSideBarRight( int nMin,
                                            int nMax,
                                            int nDef,
                                            ColorFloat clrNormal,
                                            ColorFloat clrHovered)
        {
            if (m_oSideBarRight is not null)
                m_oSideBarRight.Dispose();

            m_oSideBarRight = new(this, false, nMin, nMax, nDef, clrNormal, clrHovered);
            return m_oSideBarRight; 
        }

        SideBar? m_oSideBarLeft  = null;
        SideBar? m_oSideBarRight = null;

        public partial class SideBar
        {
            public SideBar( Viewer oSetViewer,
                            bool bLeft,
                            int nMin,
                            int nMax,
                            int nDef,
                            ColorFloat clrNormal,
                            ColorFloat clrHovered)
            {
                oViewer = oSetViewer;
                
                hThis = _hCreate(   oViewer.hThis, 
                                    bLeft, 
                                    nMin, 
                                    nMax, 
                                    nDef, 
                                    clrNormal,
                                    clrHovered);
            }
        }
    }
}

