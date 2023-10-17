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

namespace PicoGK
{
    public partial class Mesh
    {
        public bool bFindTriangleFromSurfacePoint(  Vector3 vecSurfacePoint,
                                                    out int nTriangle)
        {
            for (int n = 0; n < nTriangleCount(); n++)
            {
                GetTriangle(    n,
                                out Vector3 vecA,
                                out Vector3 vecB,
                                out Vector3 vecC);

                if (bPointLiesOnTriangle(vecSurfacePoint, vecA, vecB, vecC))
                {
                    nTriangle = n;
                    return true;
                }
            }

            nTriangle = int.MaxValue;
            return false;
        }

        static public bool bPointLiesOnTriangle(    Vector3 vecP,
                                                    Vector3 vecA,
                                                    Vector3 vecB,
                                                    Vector3 vecC)
        {
            // Move the triangle so that the point becomes the 
            // triangles origin

            Vector3 a = vecA - vecP;
            Vector3 b = vecB - vecP;
            Vector3 c = vecC - vecP;

            // Compute the normal vectors for triangles:
            // u = normal of PBC
            // v = normal of PCA
            // w = normal of PAB

            Vector3 u = Vector3.Cross(b, c);
            Vector3 v = Vector3.Cross(c, a);
            Vector3 w = Vector3.Cross(a, b);

            // Test to see if the normals are facing 
            // the same direction, return false if not
            if (Vector3.Dot(u, v) < 0)
            {
                return false;
            }
            if (Vector3.Dot(u, w) < 0)
            {
                return false;
            }

            // All normals facing the same way, return true
            return true;
        }
    }
}
