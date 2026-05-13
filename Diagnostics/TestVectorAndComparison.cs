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
using PicoGK.Numerics;
using PicoGK.Shapes;

namespace PicoGK.Diagnostics
{
    public static class TestVectorAndComparison
    {
        public static void Test()
        {
            Vector3 vec3 = Vector3.UnitX;
            Vector2 vec2 = vec3.vecStripZ();
            vec3 = vec2.vecAsVector3(1);
            Console.WriteLine(vec3);

            vec3 = new(Tolerances.fDef,0,0);
            
            if (vec3.bAlmostZero())
                Console.WriteLine("Almost Zero");

            if (vec3.bAlmostEqual(Vector3.Zero))
                Console.WriteLine("Almost Equal Zero");

            Frame3d frm = new(Vector3.Zero, Vector3.UnitY, Vector3.UnitX);

            Vector3 vec = Vector3.UnitZ;
            
            Vector3 vecW = vec.vecPtWorld(frm);
            Console.WriteLine(vecW);

            Vector3 vecL = vecW.vecPtLocal(frm);
            Console.WriteLine(vecL);

        }
    }
}