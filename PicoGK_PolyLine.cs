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
        /// <param name="vec">Coordinate of the vertex</param>
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

        BBox3 m_oBoundingBox = new BBox3();
    }
}

