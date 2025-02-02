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
using System.Numerics;

namespace PicoGK
{
    public partial class Mesh
    {
        /// <summary>
        /// Create an empty Mesh
        /// </summary>
        public Mesh()
        {
            m_hThis = _hCreate();
            Debug.Assert(m_hThis != IntPtr.Zero);
        }

        /// <summary>
        /// Create a mesh from the specified voxels object
        /// </summary>
        /// <param name="vox">Voxels to create a mesh from</param>
        public Mesh(in Voxels vox)
        {
            m_hThis = _hCreateFromVoxels(vox.m_hThis);
            Debug.Assert(m_hThis != IntPtr.Zero);
        }

        /// <summary>
        /// Create a transformed mesh by offsetting and scaling it
        /// </summary>
        /// <param name="vecScale">Scale the mesh (first step)</param>
        /// <param name="vecOffset">Offset the mesh (second step)</param>
        /// <returns>A new mesh that has the transformation applied</returns>
        public Mesh mshCreateTransformed(   Vector3 vecScale,
                                            Vector3 vecOffset)
        {
            Mesh mshTrans = new Mesh();
            for (int n = 0; n < nTriangleCount(); n++)
            {
                GetTriangle(    n,
                                out Vector3 A,
                                out Vector3 B,
                                out Vector3 C);

                A *= vecScale.X;
                B *= vecScale.Y;
                C *= vecScale.Z;

                A += vecOffset;
                B += vecOffset;
                C += vecOffset;

                mshTrans.nAddTriangle(A, B, C);
            }

            return mshTrans;
        }

        /// <summary>
        /// Create a transformed mesh by applying a transformation matrix
        /// </summary>
        /// <param name="matTrans">Transformation Matrix to apply</param>
        /// <returns>A new mesh that has the transformation applied</returns>
        public Mesh mshCreateTransformed(Matrix4x4 matTrans)
        {
            Mesh mshTrans = new Mesh();
            for (int n = 0; n < nTriangleCount(); n++)
            {
                GetTriangle(    n,
                                out Vector3 A,
                                out Vector3 B,
                                out Vector3 C);

               mshTrans.nAddTriangle(   A.vecTransformed(matTrans),
                                        B.vecTransformed(matTrans),
                                        C.vecTransformed(matTrans));
            }

            return mshTrans;
        }

        //// <summary>
        /// Mirrors a mesh at the specified plane.
        /// </summary>
        /// <param name="msh">The mesh to mirror.</param>
        /// <param name="vecPlanePoint">A point through which the mirror plane passes.</param>
        /// <param name="vecPlaneNormal">The normal vector of the mirror plane.</param>
        /// <returns>The mirrored mesh.</returns>
        public Mesh mshCreateMirrored(  Vector3 vecPlanePoint,
									    Vector3 vecPlaneNormal)
		{
			Mesh mshResult = new();

            vecPlaneNormal = vecPlaneNormal.vecNormalized();

			for (int n=0; n<nTriangleCount();n++)
			{
				GetTriangle(n, out Vector3 vecA, out Vector3 vecB, out Vector3 vecC);

				mshResult.nAddTriangle(	vecA.vecMirrored(vecPlanePoint, vecPlaneNormal),
										vecB.vecMirrored(vecPlanePoint, vecPlaneNormal),
										vecC.vecMirrored(vecPlanePoint, vecPlaneNormal));
			}

			return mshResult;
		}

        /// <summary>
        /// Add a new vertex to the mesh so that it can be used in mesh triangles
        /// </summary>
        /// <param name="vec">The vertex to add</param>
        /// <returns>The index of the vertex (to be used in triangles)</returns>
        public int nAddVertex(in Vector3 vec)
        {
            return _nAddVertex(m_hThis, vec);
        }

        public void AddVertices(    in  IEnumerable<Vector3> avecVertices,
                                    out int[] anVertexIndex)
        {
            int nVertexCount = avecVertices.Count();
            anVertexIndex = new int[nVertexCount];
            
            int n=0;
            foreach (Vector3 vec in avecVertices)
            {
                anVertexIndex[n] = nAddVertex(vec);
                n++;
            }
        }

        /// <summary>
        /// Get the vertex at the specified index
        /// </summary>
        /// <param name="nVertex">The vertex index</param>
        public Vector3 vecVertexAt(int nVertex)
        {
            Vector3 vec = new ();
            _GetVertex(m_hThis, nVertex, ref vec);
            return vec;
        }

        /// <summary>
        /// Get the number of vertices in the mesh
        /// </summary>
        /// <returns>The number of vertices in the mesh</returns>
        public int nVertexCount()
        {
            return _nVertexCount(m_hThis);
        }

        /// <summary>
        /// Add a triangle to the mesh with the specified vertex indices
        /// </summary>
        /// <param name="t">Triangle with the vertex indices set to existing vertices</param>
        /// <returns>The triangle index of the added triangle</returns>
        public int nAddTriangle(in Triangle t)
        {
            return _nAddTriangle(m_hThis, t);
        }

        //// <summary>
        ///  Add a triangle to the mesh with the specified vertex indices
        /// </summary>
        /// <param name="A">First vertex in the triangle</param>
        /// <param name="B">Second vertex in the triangle</param>
        /// <param name="C">Third vertex in the triangle</param>
        /// <returns>The triangle index of the added triangle in the mesh</returns>
        public int nAddTriangle(int A, int B, int C)
        {
            return nAddTriangle(new Triangle(A, B, C));
        }

        /// <summary>
        /// Return number of triangles in the mesh
        /// </summary>
        /// <returns>Triangle count in mesh</returns>
        public int nTriangleCount()
        {
            return _nTriangleCount(m_hThis);
        }

        /// <summary>
        /// Add a triangle specified by the three vertices to the mesh
        /// First adds the vertices and then the triangle based on the vertex
        /// index
        /// </summary>
        /// <param name="vecA">First vertex of the triangle</param>
        /// <param name="vecB">Second vertex of the triangle</param>
        /// <param name="vecC">Third vertex of the triangle</param>
        /// <returns>The triangle index of the added triangle in the mesh</returns>
        public int nAddTriangle(    in Vector3 vecA,
                                    in Vector3 vecB,
                                    in Vector3 vecC)
        {
            int A = nAddVertex(vecA);
            int B = nAddVertex(vecB);
            int C = nAddVertex(vecC);
            return nAddTriangle(new Triangle(A, B, C));
        }

        /// <summary>
        /// Adds a quad, defined by four corner vertices
        /// Helper function, which calls nAddTriangle in
        /// the background.
        /// </summary>
        public void AddQuad(    int n0,
                                int n1,
                                int n2,
                                int n3,
                                bool bFlipped = false)
        {
            if (bFlipped)
            {
                nAddTriangle(n0, n2, n1);
                nAddTriangle(n0, n3, n2);
            }
            else
            {
                nAddTriangle(n0, n1, n2);
                nAddTriangle(n0, n2, n3);
            }
        }

        /// <summary>
        /// Adds a quad, defined by four corner vertices
        /// Helper function, which calls nAddTriangle in
        /// the background.
        /// </summary>
        public void AddQuad(    in Vector3 vec0,
                                in Vector3 vec1,
                                in Vector3 vec2,
                                in Vector3 vec3,
                                bool bFlipped = false)
        {
            int n0 = nAddVertex(vec0);
            int n1 = nAddVertex(vec1);
            int n2 = nAddVertex(vec2);
            int n3 = nAddVertex(vec3);

            AddQuad(n0,n1,n2,n3, bFlipped);
        }

        /// <summary>
        /// Get the triangle with the specified index
        /// </summary>
        /// <param name="nTriangle">Triangle index in the mesh</param>
        public Triangle oTriangleAt(int nTriangle)
        {
            Triangle t = new();
            _GetTriangle(   m_hThis,
                            nTriangle,
                            ref t);

            return t;
        }

        /// <summary>
        /// Get the triangle with the specified index
        /// </summary>
        /// <param name="nTriangle">Triangle index in the mesh</param>
        /// <param name="vecA">First vertex in the triangle</param>
        /// <param name="vecB">Second vertex in the triangle</param>
        /// <param name="vecC">Third vertex in the triangle</param>
        public void GetTriangle(    int nTriangle,
                                    out Vector3 vecA,
                                    out Vector3 vecB,
                                    out Vector3 vecC)
        {
            vecA = new();
            vecB = new();
            vecC = new();

            _GetTriangleV(  m_hThis,
                            nTriangle,
                            ref vecA,
                            ref vecB,
                            ref vecC);
        }

        /// <summary>
        /// Append one mesh to another
        /// Note, no deduplication is done
        /// and no "boolean"
        /// The source mesh remains unchanged
        /// </summary>
        public void Append(Mesh msh)
        {
            for (int n=0; n<msh.nTriangleCount(); n++)
            {
                msh.GetTriangle(    n,
                                    out Vector3 vecA,
                                    out Vector3 vecB,
                                    out Vector3 vecC);

                nAddTriangle(vecA, vecB, vecC);
            }
        }

        /// <summary>
        /// Return the BoundingBox of the Mesh
        /// </summary>
        /// <returns>BoundingBox of the Mesh</returns>
        public BBox3 oBoundingBox()
        {
            BBox3 oBBox = new BBox3();
            _GetBoundingBox(m_hThis, ref oBBox);
            return oBBox;
        }
    }

}