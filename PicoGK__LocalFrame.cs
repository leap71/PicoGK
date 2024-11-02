//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2024 by LEAP 71
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
    /// <summary>
    /// The LocalFrame object encapsulates a local reference 
    /// coordinate system
    /// </summary>
    public readonly struct LocalFrame
    {
        /// <summary>
        /// Creates a local frame defaulting to
        /// the world coordinate system
        /// Origin at 0/0/0 and X,Y,Z axes
        /// </summary>
        public LocalFrame()
        {
            m_vecPosition   = Vector3.Zero;
            m_vecLocalZ     = Vector3.UnitZ;
            m_vecLocalX     = Vector3.UnitX;
            m_vecLocalY     = Vector3.UnitY;
        }

        /// <summary>
        /// Creates a local frame at the specified
        /// position, with X,Y,Z aligned with world
        /// axes
        /// </summary>
        /// <param name="vecPos">Origin of frame</param>
        public LocalFrame(Vector3 vecPos)
        {
            m_vecPosition   = vecPos;
            m_vecLocalZ     = Vector3.UnitZ;
            m_vecLocalX     = Vector3.UnitX;
            m_vecLocalY     = Vector3.UnitY;

            Debug.Assert(m_vecLocalY == Vector3.Cross(vecLocalZ, vecLocalX));
        }

        /// <summary>
        /// Creates a copy of the specified frame
        /// </summary>
        /// <param name="oFrame">Frame to copy from</param>
        public LocalFrame(LocalFrame oFrame)
        {
            m_vecPosition   = oFrame.vecPosition;
            m_vecLocalZ     = oFrame.vecLocalZ;
            m_vecLocalX     = oFrame.vecLocalX;
            m_vecLocalY     = oFrame.vecLocalY;

            Debug.Assert(m_vecLocalY == Vector3.Cross(vecLocalZ, vecLocalX));
        }

        /// <summary>
        /// Creates a local frame with the same
        /// coordinates as the specified frame
        /// but at a new position
        /// </summary>
        /// <param name="oFrame"></param>
        /// <param name="vecNewPos"></param>
        public LocalFrame(  LocalFrame oFrame,
                            Vector3 vecNewPos)
        {
            m_vecPosition   = vecNewPos;
            m_vecLocalZ     = oFrame.vecLocalZ;
            m_vecLocalX     = oFrame.vecLocalX;
            m_vecLocalY     = oFrame.vecLocalY;
        }

        /// <summary>
        /// Create a local frame at the specified
        /// position with the specified Z and X axis directions
        /// The Y is calculated using the right hand rule
        /// </summary>
        /// <param name="vecPos"></param>
        /// <param name="vecLocalZ"></param>
        /// <param name="vecLocalX"></param>
        public LocalFrame(  Vector3 vecPos,
                            Vector3 vecLocalZ,
                            Vector3 vecLocalX)
        {
            m_vecPosition   = vecPos;
            m_vecLocalZ     = Vector3.Normalize(vecLocalZ);
            m_vecLocalX     = Vector3.Normalize(vecLocalX);
            m_vecLocalY     = Vector3.Cross(m_vecLocalZ, m_vecLocalX);
        }

        /// <summary>
        /// Create a LocalFrame that is mirrored at the specified axes
        /// </summary>
        /// <param name="vecAxis">Axis to rotate around</param>
        /// <param name="fAngle">Angle in rad</param>
        /// <returns>The resulting LocalFrame</returns>
        public LocalFrame oMirrored(    bool bMirrorZ, 
                                        bool bMirrorX)
        {
           return new(  m_vecPosition,
                        bMirrorZ ? -m_vecLocalZ : m_vecLocalZ,
                        bMirrorX ? -m_vecLocalX : m_vecLocalX);   
        }

        /// <summary>
        /// Create a LocalFrame that is moved by the specified distance
        /// </summary>
        /// <param name="vecAxis">Axis to rotate around</param>
        /// <param name="fAngle">Angle in rad</param>
        /// <returns>The resulting LocalFrame</returns>
        public LocalFrame oTranslated(Vector3 vecOffset)
        {
           return new(  m_vecPosition + vecOffset,
                        m_vecLocalZ,
                        m_vecLocalX);   
        }

        /// <summary>
        /// Create a LocalFrame that is rotated around the specified axis
        /// </summary>
        /// <param name="vecAxis">Axis to rotate around</param>
        /// <param name="fAngle">Angle in rad</param>
        /// <returns>The resulting LocalFrame</returns>
        public LocalFrame oRotated( Vector3 vecAxis,
                                    float fAngle)
        {
            Quaternion qt = Quaternion.CreateFromAxisAngle(vecAxis, fAngle);
            Matrix4x4 mat = Matrix4x4.CreateFromQuaternion(qt);

            return oTransformed(mat);
        }

        /// <summary>
        /// Create a LocalFrame that is transformed by the matrix
        /// </summary>
        /// <param name="mat">Transformation matrix</param>
        /// <returns>The resulting LocalFrame</returns>
        public LocalFrame oTransformed(Matrix4x4 mat)
        {
            return new( Vector3.Transform(m_vecPosition, mat),
                        Vector3.Transform(m_vecLocalZ, mat),
                        Vector3.Transform(m_vecLocalX, mat));   
        }
        
        public Vector3 vecPosition  => m_vecPosition;
        public Vector3 vecLocalX    => m_vecLocalX;
        public Vector3 vecLocalY    => m_vecLocalY;
        public Vector3 vecLocalZ    => m_vecLocalZ;

        readonly Vector3 m_vecPosition;
        readonly Vector3 m_vecLocalX;
        readonly Vector3 m_vecLocalY;
        readonly Vector3 m_vecLocalZ;
    }
}