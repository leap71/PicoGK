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

namespace PicoGK
{
    public partial class Mesh
    {
        public Voxels voxVoxelizeHollow(float fThickness)
        {
            return new Voxels(new ImplicitMesh(this, fThickness));

            /// TODO enable when multithreaded PicoGK is released
            
            /*Voxels [] avox = new Voxels[nTriangleCount()];

            Parallel.For(0, nTriangleCount(), n =>
            {
                GetTriangle(    n, 
                                    out Vector3 A, 
                                    out Vector3 B, 
                                    out Vector3 C);

                ImplicitTriangle tri = new(A,B,C,fThickness);
                avox[n] = new Voxels(tri, tri.oBounds);
            });

            Voxels voxResult = new();
            voxResult.BoolAddAll(avox);
            return voxResult;*/
        }
    }

    /// <summary>
    /// Enables treating a mesh as an implicit with a thickness (making it hollow)
    /// </summary>
    public class ImplicitMesh : IBoundedImplicit
    {
        public ImplicitMesh(Mesh msh, float fThickness)
        {
            m_aTriangles = new ImplicitTriangle[msh.nTriangleCount()];
            
            for (int n=0; n<msh.nTriangleCount(); n++)
            {
                msh.GetTriangle(n, out Vector3 A, out Vector3 B, out Vector3 C);
                m_aTriangles[n] = new(A,B,C,fThickness);
                m_oBBox.Include(m_aTriangles[n].oBounds);
            }
        }

        public float fSignedDistance(in Vector3 vec)
        {
            float [] afDist = new float[m_aTriangles.Count()];

            Vector3 vecPt = vec;

            Parallel.For(0, m_aTriangles.Count(), n =>
            {
                afDist[n] = m_aTriangles[n].fSignedDistance(vecPt);
            });

            float fDist = float.MaxValue;

            for (int n=0; n<m_aTriangles.Count(); n++)
            {
                fDist = float.Min(fDist, afDist[n]);
            }

            return fDist;
        }

        ImplicitTriangle [] m_aTriangles;

        public BBox3 oBounds => m_oBBox;

        BBox3 m_oBBox = new();
    }

    /// <summary>
    /// Enables treating a triangle as an implicit with a thickness
    /// </summary>
    public class ImplicitTriangle : IBoundedImplicit
    {
        public ImplicitTriangle(    Vector3 vecA, 
                                    Vector3 vecB, 
                                    Vector3 vecC,
                                    float fThickness)
        {
            A = vecA;
            B = vecB;
            C = vecC;
            m_fThickness = fThickness;
            m_oBBox.Include(A);
            m_oBBox.Include(B);
            m_oBBox.Include(C);
            m_oBBox.Grow(fThickness);
        }

        public float fSignedDistance(in Vector3 vecPoint)
        {
            Vector3 vecClosestPoint = vecClosestPointOnTriangle2(vecPoint, A, B, C);
            float f = Vector3.Distance(vecPoint, vecClosestPoint);
            return f-m_fThickness;
        }

        private Vector3 vecClosestPointOnTriangle2(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
        {
            // Compute vectors from triangle vertices
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ap = point - a;

            // Compute dot products
            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f) return a; // Closest to vertex A

            // Check vertex region outside B
            Vector3 bp = point - b;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3) return b; // Closest to vertex B

            // Check edge AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                return a + v * ab; // Closest to edge AB
            }

            // Check vertex region outside C
            Vector3 cp = point - c;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6) return c; // Closest to vertex C

            // Check edge AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                return a + w * ac; // Closest to edge AC
            }

            // Check edge BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + w * (c - b); // Closest to edge BC
            }

            // Inside the face region
            float denom = 1.0f / (va + vb + vc);
            float vAB = vb * denom;
            float vAC = vc * denom;
            return a + vAB * ab + vAC * ac;
        }   

        readonly Vector3 A;
        readonly Vector3 B;
        readonly Vector3 C;
        readonly float   m_fThickness;

        public BBox3 oBounds => m_oBBox;

        BBox3 m_oBBox = new();
    }
}