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

using System.Numerics;

namespace PicoGK
{
    /// <summary>
    /// Encapsulates a rigid local coordinate system (position + orientation).
    /// Backed by a Matrix4x4 with row-vector semantics:
    /// <para>• Right-handed basis: <c>Y = Z × X</c></para>
    /// <para>• Row-vector convention: <c>p_world = p_local * M</c> (left-to-right composition)</para>
    /// <para>• Rigid only (no scale/shear). <see cref="mat"/> is orthonormal with translation.</para>
    /// <para>• Thread-safe by immutability. </para>
    /// <para>• <see cref="matInv"/> is the inverse rigid transform (precomputed).</para>
    /// </summary>
    public sealed class LocalFrame
    {
        /// <summary>
        /// This constant defines the threshold under which two vectors are seen as parallel
        /// </summary>
        const float EPS_PARALLEL = 1e-9f;

        /// <summary>
        /// Transformation matrix from Local to World
        /// </summary>
        public Matrix4x4 mat        => m_mat;

        /// <summary>
        /// Inverse transformation from World to Local
        /// </summary>
        public Matrix4x4 matInv     => m_matInv;    

        /// <summary>
        /// Position of the origin of our frame
        /// </summary>
        public Vector3 vecPos => new(mat.M41, mat.M42, mat.M43);

        /// <summary>
        /// Access to Local X basis direction in world space.
        /// Row-vector semantics: the rows of the 3×3 linear part are the world-space images of local X/Y/Z.
        /// </summary>       
        public Vector3 vecLocalX => new(mat.M11, mat.M12, mat.M13); // image of (1,0,0) local in world

        /// <summary>
        /// Access to Local Y basis direction in world space.
        /// Row-vector semantics: the rows of the 3×3 linear part are the world-space images of local X/Y/Z.
        /// </summary>      
        public Vector3 vecLocalY => new(mat.M21, mat.M22, mat.M23); // image of (0,1,0) local in world

        /// <summary>
        /// Access to Local Z basis direction in world space.
        /// Row-vector semantics: the rows of the 3×3 linear part are the world-space images of local X/Y/Z.
        /// </summary>      
        public Vector3 vecLocalZ => new(mat.M31, mat.M32, mat.M33); // image of (0,0,1) local in world

        /// <summary>
        /// Identity local frame with right handed X,Y,Z coordinate system, aligned with world X,Y,Z
        /// positioned at world origin 0,0,0
        /// </summary>
        public static LocalFrame frmIdentity => frmFromBasis(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ);

        /// <summary>
        /// Creates a new LocalFrame at the specified position aligned with world X,Y,Z
        /// </summary>
        /// <param name="vecPos"></param>
        /// <returns>New LocalFrame</returns>
        public static LocalFrame frmFromPosition(Vector3 vecPos)
            => frmFromBasis(vecPos, Vector3.UnitX, Vector3.UnitZ);

        /// <summary>
        /// Creates a new LocalFrame object with the specified vector as local Z axis direction
        /// </summary>
        /// <param name="vecPos">Local origin in world coordinates</param>
        /// <param name="vecZ">Direction of the local Z axis in world coordinates</param>
        /// <returns>New LocalFrame</returns>
        public static LocalFrame frmFromZ(Vector3 vecPos, Vector3 vecZ)
        {
            var vecAxisZ = vecSafeNormalized(vecZ);
            var vecAxisX = vecOrthonormalX(vecAxisZ);
            return frmFromBasis(vecPos, vecAxisX, vecAxisZ);
        }

        /// <summary>
        /// Creates a new LocalFrame object with the spefified Z and X axis directions
        /// </summary>
        /// <param name="vecPos">Local origin in world coordinates</param>
        /// <param name="vecZ">Z axis direction in world coordinates</param>
        /// <param name="vecX">X axis direction in world coordinates</param>
        /// <returns>New LocalFrame</returns>
        public static LocalFrame frmFromZX(Vector3 vecPos, Vector3 vecZ, Vector3 vecX)
        {
            Vector3 vecAxisZ = vecSafeNormalized(vecZ);
            Vector3 vecAxisX = vecSafeNormalized(vecX);

            // Re-orthonormalize to ensure right-handedness:
            vecAxisX = Vector3.Normalize(vecAxisX - Vector3.Dot(vecAxisX, vecAxisZ) * vecAxisZ);
            return frmFromBasis(vecPos, vecAxisX, vecAxisZ);
        }

        /// <summary>
        /// Creates a new LocalFrame object at the specified position and using a quaternion
        /// as the orinentation
        /// </summary>
        /// <param name="vecPos">Position of the local origin</param>
        /// <param name="q">Orientation specified as quaternion</param>
        /// <returns>New LocalFrame</returns>
        public static LocalFrame frmFromQuaternion(Vector3 vecPos, Quaternion q)
        {
            Matrix4x4 matR  = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(q));
            Matrix4x4 mat   = matR; 
            
            mat.M41 = vecPos.X; 
            mat.M42 = vecPos.Y; 
            mat.M43 = vecPos.Z;
            mat.M44 = 1f;

            return new LocalFrame(mat);
        }

        /// <summary>
        /// Creates a LocalFrame at <paramref name="vecPos"/> with local Z pointing toward <paramref name="vecTarget"/>.
        /// If <paramref name="vecUpHint"/> is parallel to the view direction, a fallback up (least aligned with Z) is chosen.
        /// </summary>
        /// <param name="vecPos">Position of the LocalFrame origin in world coordinates</param>
        /// <param name="vecTarget">Target to look at</param>
        /// <param name="vecUpHint">Direction in which "up" should be considered</param>
        /// <returns>New LocalFrame object</returns>
        public static LocalFrame frmLookAt( Vector3 vecPos, 
                                            Vector3 vecTarget, 
                                            Vector3 vecUpHint)
        {
            Vector3 vecAxisZ = vecSafeNormalized(vecTarget - vecPos);
            Vector3 vecAxisX = Vector3.Cross(vecUpHint, vecAxisZ);
            
            float l2 = vecAxisX.LengthSquared();
            if (l2 < EPS_PARALLEL)
            {
                // Up is parallel; pick a robust fallback axis least aligned with Z
                Vector3 vecFallback = float.Abs(vecAxisZ.X) < 0.5f ? Vector3.UnitX : Vector3.UnitY;
                vecAxisX = Vector3.Normalize(Vector3.Cross(vecFallback, vecAxisZ));
            }
            else
            {
                vecAxisX /= float.Sqrt(l2);
            }
            return frmFromBasis(vecPos, vecAxisX, vecAxisZ);
        }

        /// <summary>
        /// Convert a coordinate in the local coordinate system to world coordinates
        /// </summary>
        /// <param name="vecLocal">Local coordinate</param>
        /// <returns>World coordinate</returns>
        public Vector3 vecPtToWorld(Vector3 vecLocal)   => Vector3.Transform(vecLocal, mat);

        /// <summary>
        /// Return a direction in the local coordinate system to a world direction
        /// </summary>
        /// <param name="vecLocal">Local direction</param>
        /// <returns>World direction</returns>
        public Vector3 vecDirToWorld(Vector3 vecLocal)  => Vector3.TransformNormal(vecLocal, mat);

        /// <summary>
        /// Convert a world coordinate to a coordinate in the local coordinate system
        /// </summary>
        /// <param name="vecWorld">World coordinate</param>
        /// <returns>Local coordinate</returns>
        public Vector3 vecPtToLocal(Vector3 vecWorld)   => Vector3.Transform(vecWorld, matInv);

        /// <summary>
        /// Convert a world direction into a local direction
        /// </summary>
        /// <param name="vecWorld">Direction in the world coordinate system</param>
        /// <returns>local direction</returns>
        public Vector3 vecDirToLocal(Vector3 vecWorld)  => Vector3.TransformNormal(vecWorld, matInv);


        /// <summary>
        /// Creates a new LocalFrame with the same orientation, but a new position
        /// </summary>
        /// <param name="vecNewPos">New position in space</param>
        /// <returns>New LocalFrame object</returns>
        public LocalFrame frmWithPosition(Vector3 vecNewPos)
        {
            Matrix4x4 matNew = mat; 
            matNew.M41 = vecNewPos.X; 
            matNew.M42 = vecNewPos.Y; 
            matNew.M43 = vecNewPos.Z;
            return new LocalFrame(matNew);
        }

        /// <summary>
        /// Creates a new LocalFrame object that was moved into the direction specified
        /// </summary>
        /// <param name="vecDirWorld">Relative vector to move towards in world coordinates</param>
        /// <returns>New LocalFrame object</returns>
        public LocalFrame frmMovedWorld(Vector3 vecDirWorld) => frmWithPosition(vecPos + vecDirWorld);

        /// <summary>
        /// Creates a new LocalFrame object that was moved into the direction specified
        /// </summary>
        /// <param name="vecDirLocal">Relative vector to move towards in local coordinates</param>
        /// <returns>New LocalFrame object</returns>
        public LocalFrame frmMovedLocal(Vector3 vecDirLocal) => frmMovedWorld(vecDirToWorld(vecDirLocal)); 

        /// <summary>
        /// Rotate orientation of LocalFrame around world-coordinate axis
        /// </summary>
        /// <param name="phi">Angle in rad</param>
        /// <param name="vecWorldAxis">World coordinate axis to rotate around</param>
        /// <returns>New LocalFrame</returns>
        public LocalFrame frmRotatedWorld(float phi, Vector3 vecWorldAxis)
        {
            Matrix4x4 matR = Matrix4x4.CreateFromAxisAngle(vecSafeNormalized(vecWorldAxis), phi);
            // p' = (p - pos)*R + pos  ->  M' = (M - Tpos)*R + Tpos -> easier via compose:
            Matrix4x4 matTm = Matrix4x4.CreateTranslation(-vecPos);
            Matrix4x4 matTp = Matrix4x4.CreateTranslation( vecPos);
            
            return new LocalFrame(mat * matTm * matR * matTp);
        }

       
       /// <summary>
       /// Rotate orientation of LocalFrame around local-coordinate axis
       /// </summary>
       /// <param name="phi">Angle in rad</param>
       /// <param name="vecLocalAxis">Axis in local coordinates to rotate around</param>
       /// <returns>New LocalFrame</returns>
        public LocalFrame frmRotatedLocal(float phi, Vector3 vecLocalAxis)
            => frmRotatedWorld(phi, vecDirToWorld(vecLocalAxis));

        /// <summary>
        /// Returns a mirrored LocalFrame by negating the selected local axes (X and/or Z).
        /// Handedness is preserved by recomputing <c>Y = Z × X</c>.
        /// </summary>
        /// <param name="bMirrorZ">Mirror along the local Z axis</param>
        /// <param name="bMirrorX">Mirror along the local X axis</param>
        /// <returns>New LocalFrame</returns>
        public LocalFrame frmMirrored(bool bMirrorZ, bool bMirrorX)
        {
            Vector3 vecNewX = vecLocalX * (bMirrorX ? -1f : 1f);
            Vector3 vecNewZ = vecLocalZ * (bMirrorZ ? -1f : 1f);
            
            return frmFromBasis(vecPos, vecNewX, vecNewZ);
        }
        /// <summary>
        /// For drawing a scaled quad aligned to this frame:
        /// Model = Scale * Frame.M
        /// </summary>
        public Matrix4x4 matComposeWithScale(in Vector3 vecScale)
        {
            Matrix4x4 matS = Matrix4x4.CreateScale(vecScale);
            return matS * mat; // row-vector: scale -> then frame
        }

        /// <summary>
        /// Internal helper to create a valid local frame
        /// </summary>
        private static LocalFrame frmFromBasis( Vector3 vecPos, 
                                                Vector3 vecX, 
                                                Vector3 vecZ)
        {
            // Final orthonormalization (robust against nearly-colinear inputs)
            Vector3 vecAxisZ    = vecSafeNormalized(vecZ);
            Vector3 vecAxisXt   = vecX;
            Vector3 vecAxisXo   = vecAxisXt - Vector3.Dot(vecAxisXt, vecAxisZ) * vecAxisZ;
            
            float l2 = vecAxisXo.LengthSquared();
            
            // Near parallel
            if (l2 < EPS_PARALLEL)
            {
                Vector3 vecFallback = float.Abs(vecAxisZ.X) < 0.5f ? Vector3.UnitX : Vector3.UnitY;
                vecAxisXo = Vector3.Normalize(Vector3.Cross(vecFallback, vecAxisZ));
            }
            else
            {
               vecAxisXo /= float.Sqrt(l2);
            }

            Vector3 vecAxisY    = Vector3.Cross(vecAxisZ, vecAxisXo);

            Matrix4x4 mat = new Matrix4x4(
                vecAxisXo.X,    vecAxisXo.Y,    vecAxisXo.Z,    0f,
                vecAxisY.X,     vecAxisY.Y,     vecAxisY.Z,     0f,
                vecAxisZ.X,     vecAxisZ.Y,     vecAxisZ.Z,     0f,
                vecPos.X,       vecPos.Y,       vecPos.Z,       1f);

            return new LocalFrame(mat);
        }

        private LocalFrame(Matrix4x4 m)
        {
            m_mat = m;
            Matrix4x4.Invert(m_mat, out m_matInv);
        }

        private static Vector3 vecSafeNormalized(in Vector3 vec)
        {
            var l2 = vec.LengthSquared();
            if (l2 == 0f) 
                throw new ArgumentException($"Vector has zero length.");

            return vec / float.Sqrt(l2);
        }

        private static Vector3 vecOrthonormalX(in Vector3 vecZ)
        {
            // Pick the least aligned axis to avoid numerical issues
            Vector3 vecA = float.Abs(vecZ.X) < 0.5f ? Vector3.UnitX : Vector3.UnitY;
            Vector3 vecX = Vector3.Normalize(Vector3.Cross(vecA, vecZ));
            return vecX;
        }

        readonly Matrix4x4 m_mat;
        readonly Matrix4x4 m_matInv;
    }
}