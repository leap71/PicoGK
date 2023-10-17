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

using System.Runtime.InteropServices;

namespace PicoGK
{
    // Note these classes define the memory layout to be compatible with
    // the PicoGK C++ types. Do not add data members to these classes, even
    // if they are defined as "partial" to be extended with additional methods 

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct Coord
    {
        public int X;
        public int Y;
        public int Z;

        public Coord(   int x,
                        int y,
                        int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct Triangle
    {
        public int A;
        public int B;
        public int C;

        public Triangle(    int a,
                            int b,
                            int c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }
    }
}

