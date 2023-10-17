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
using System.Numerics;

namespace PicoGK
{
    public partial class Lattice
    {
        public Lattice()
        {
            m_hThis = _hCreate();
            Debug.Assert(m_hThis != IntPtr.Zero);
        }

        public void AddSphere(  in Vector3 vecCenter,
                                float fRadius)
        {
            _AddSphere(m_hThis, vecCenter, fRadius);
        }

        public void AddBeam(    in Vector3 vecA,
                                float fRadA,
                                in Vector3 vecB,
                                float fRadB,
                                bool bRoundCap = true)
        {
            _AddBeam(   m_hThis,
                        in vecA,
                        in vecB,
                        fRadA,
                        fRadB,
                        bRoundCap);
        }

        public void AddBeam(    in Vector3 vecA,
                                in Vector3 vecB,
                                float fRadA,
                                float fRadB,
                                bool bRoundCap = true)
        {
            _AddBeam(   m_hThis,
                        in vecA,
                        in vecB,
                        fRadA,
                        fRadB,
                        bRoundCap);
        }
    }

}