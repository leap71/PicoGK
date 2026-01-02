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
    /// <summary>
    /// A colored 3D polyline for use in the viewer
    /// </summary>
    public partial class PolyLine
    {
        /// <summary>
        /// Create a new polyline with the specified color
        /// </summary>
        /// <param name="clr"></param>
        public PolyLine(ColorFloat clr)
        {
            m_hThis = _hCreate(in clr);
        }

        /// <summary>
        /// Add a vertex to the polyline
        /// </summary>
        /// <param name="vec">The specified vertex</param>
        /// <returns>The vertex index</returns>
        public int nAddVertex(in Vector3 vec)
        {
            m_oBoundingBox.Include(vec);
            return _nAddVertex( m_hThis,
                                in vec);
        }

        /// <summary>
        /// Adds all vertices from a container (such as a List<>)
        /// </summary>
        /// <param name="avec">list/array etc. of vertices</param>
        public void Add(IEnumerable<Vector3> avec)
        {
            foreach (Vector3 vec in avec)
                nAddVertex(vec);
        }

        /// <summary>
        /// Return number of vertices in the PolyLine
        /// </summary>
        /// <returns>Number of vertices</returns>
        public int nVertexCount()
        {
            return _nVertexCount(m_hThis);
        }

        /// <summary>
        /// Get the vertex in the polyline at the specified vertex index
        /// </summary>
        /// <param name="nIndex">Vertex index to retrieve</param>
        public Vector3 vecVertexAt(int nIndex)
        {
            Vector3 vec = new();
            _GetVertex(m_hThis, nIndex, ref vec);
            return vec;
        }

        /// <summary>
        /// Return the color of the PolyLine
        /// </summary>
        /// <param name="clr">PolyLine color</param>
        public void GetColor(out ColorFloat clr)
        {
            clr = new();
            _GetColor(m_hThis, ref clr);
        }

        /// <summary>
        /// Return BoundingBox of PolyLine
        /// </summary>
        /// <returns>Bounding Box</returns>
        public BBox3 oBoundingBox() => m_oBoundingBox;

        /// <summary>
        /// Adds an arrow to the tip of the current polyline
        /// The arrow points in the direction of the last polyline segment,
        /// unless you explicitly set a direction
        /// If you do not supply a direction, and there are less than
        /// two vertices in the polyline segment, the arrow points in Z+
        /// The polyline ends in the tip of the arrow, so you can cascade
        /// multiple arrows.
        /// </summary>
        /// <param name="fSizeMM">
        /// Optional size of the base of the arrow,
        /// and the distance from the tip. Defaults to 1mm
        /// </param>
        /// <param name="_vecDir">Optional direction of the arrow</param>
        public void AddArrow(   float fSizeMM = 1.0f,
                                Vector3? _vecDir = null)
        {
            if (nVertexCount() < 1)
                return;

            if ((_vecDir == null) && nVertexCount() < 2)
            {
                // Arrow has no direction, nothing to do
                return;
            }

            Vector3 vecDir = _vecDir ?? Vector3.UnitZ;

            if (_vecDir == null)
            {
                // Calculate direction

                Vector3 vecS = vecVertexAt(nVertexCount() - 2);
                Vector3 vecE = vecVertexAt(nVertexCount() - 1);
                vecDir = vecE - vecS;

                if (vecDir.Length() <= 1e-6f)
                {
                    // last two vertices are identical
                    // cannot find a direction
                    return;
                }
            }

            vecDir = Vector3.Normalize(vecDir);

            Vector3 vecInit = Vector3.UnitX;

            if (    (vecDir == Vector3.UnitX) ||
                    (vecDir == -Vector3.UnitX))
            {
                vecInit = Vector3.UnitY;
            }    

            Vector3 vecU = Vector3.Normalize(Vector3.Cross(vecDir, vecInit));
            Vector3 vecV = Vector3.Normalize(Vector3.Cross(vecDir, vecU));

            Vector3 vecTip  = vecVertexAt(nVertexCount() - 1);
            Vector3 vecBase = vecTip - vecDir * fSizeMM;

            nAddVertex(vecBase + vecU * fSizeMM / 2f);
            nAddVertex(vecBase - vecU * fSizeMM / 2f);
            nAddVertex(vecTip);
            nAddVertex(vecBase + vecV * fSizeMM / 2f);
            nAddVertex(vecBase - vecV * fSizeMM / 2f);
            nAddVertex(vecTip);
        }

        /// <summary>
        /// Adds a cross at the end of the current polyline, oriented in X,Y,Z
        /// The cross ends in the current position, so no change in the last
        /// polyline position is made
        /// </summary>
        /// <param name="fSizeMM">
        /// Optional size of the cross,
        /// and the distance from the tip. Defaults to 1mm
        /// </param>
        /// <param name="_vecDir">Optional direction of the arrow</param>
        public void AddCross(float fSizeMM = 1.0f)
        {
            if (nVertexCount() < 1)
                return;

            Vector3 vecCenter = vecVertexAt(nVertexCount()-1);

            nAddVertex(vecCenter + Vector3.UnitX * fSizeMM);
            nAddVertex(vecCenter - Vector3.UnitX * fSizeMM);
            nAddVertex(vecCenter);
            nAddVertex(vecCenter + Vector3.UnitY * fSizeMM);
            nAddVertex(vecCenter - Vector3.UnitY * fSizeMM);
            nAddVertex(vecCenter);
            nAddVertex(vecCenter + Vector3.UnitZ * fSizeMM);
            nAddVertex(vecCenter - Vector3.UnitZ * fSizeMM);
            nAddVertex(vecCenter);
        }

        BBox3 m_oBoundingBox = new BBox3();
    }
}

