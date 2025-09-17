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
using System.Runtime.CompilerServices;

namespace PicoGK.Shapes
{
    /// <summary>
    /// The LocalFrame object stores a local coordinate system, i.e. a rigid
    /// transformation from a local coordinate to a world coordinate.
    /// The transformation consists of translation (move) and rotation, but not 
    /// scaling, shearing or other complex transformations.
    /// </summary>
    [DebuggerDisplay("O=({vecPos.X:n3},{vecPos.Y:n3},{vecPos.Z:n3})")]
    public readonly struct LocalFrame : IEquatable<LocalFrame>
    {
        /// <summary>
        /// Local frame representing the world coordinate system
        /// </summary>
        public static readonly LocalFrame frmWorld = new(Vector3.Zero, Vector3.UnitZ, Vector3.UnitX);

        /// <summary>
        /// Position of the origin of the LocalFrame
        /// </summary>
        public Vector3 vecPos { get; }

        /// <summary>
        /// Direction of the local X axis in world coordinates
        /// </summary>
        public Vector3 vecLx  { get; }

        /// <summary>
        /// Direction of the local Y axis in world coordinates
        /// </summary>
        public Vector3 vecLy  { get; }

        /// <summary>
        /// Direction of the local Z axis in world coordinates
        /// </summary>
        public Vector3 vecLz  { get; }

        /// <summary>
        /// Create a LocalFrame at the specified position with axes
        /// aligned with world X,Y,Z
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalFrame frmFromPos(Vector3 vecPos)
            => new(vecPos);

        /// <summary>
        /// Create a LocalFrame at the specified position with
        /// local axes aligned with the specified world Z and X
        /// directions. The function constructs a right-handed
        /// coordinate system that is orthogonal, even if the directions
        /// specified are only approximate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalFrame frmFromZX(     Vector3 vecPos, 
                                                Vector3 vecApproxZ, 
                                                Vector3 vecApproxX)
            => new(vecPos, vecApproxZ, vecApproxX);

        /// <summary>
        /// Creates a local coordinate system with world-aligned axes
        /// at the specified position
        /// </summary>
        /// <param name="vecPos">Position of the origin</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame(Vector3 vecPos)
            : this(vecPos, Vector3.UnitZ, Vector3.UnitX) 
        { 

        }

        /// <summary>
        /// Creates a local coordinate system from approximate Z and X; 
        /// enforces orthonormality and right-handedness.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame(  Vector3 vecOrigin, 
                            Vector3 vecApproxZ, 
                            Vector3 vecApproxX)
        {
            Orthonormalize( vecApproxZ, vecApproxX, 
                            out Vector3 vecZ, 
                            out Vector3 vecX, 
                            out Vector3 vecY);
            vecPos  = vecOrigin; 
            vecLz   = vecZ; 
            vecLx   = vecX; 
            vecLy   = vecY;
#if DEBUG
            AssertOrthonormal(vecZ, vecX, vecY);
#endif
        }

        /// <summary>
        /// Creates a LocalFrame from a column-vector Matrix4x4.
        /// Rows convention: [X; Y; Z; Origin] with translation in last column.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalFrame frmFromMatrix4x4(in Matrix4x4 mat)
        {
            Vector3 vecX = new(mat.M11, mat.M12, mat.M13);
            Vector3 vecZ = new(mat.M31, mat.M32, mat.M33);
            Vector3 vecP = new(mat.M14, mat.M24, mat.M34);
            return new LocalFrame(vecP, vecZ, vecX);
        }

        /// <summary>
        /// Convert a local coordinate to world coordinates
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 vecPtToWorld(Vector3 vecLocal)
            => vecLocal.X * vecLx + vecLocal.Y * vecLy + vecLocal.Z * vecLz + vecPos;

        /// <summary>
        /// Convert a local 2D coordinate to world (3D) coordinates
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 vecPtToWorld(Vector2 vecLocal)
        {
            return vecPtToWorld(new Vector3(vecLocal.X, vecLocal.Y, 0));
        }

        /// <summary>
        /// Convert a local direction to a world direction
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 vecDirToWorld(Vector3 vecLocalDir)
        {
            Vector3 vec = vecLocalDir.X * vecLx 
                            + vecLocalDir.Y * vecLy 
                            + vecLocalDir.Z * vecLz;

            return vecSafeNormalize(vec);
        }

        /// <summary>
        /// Return a 2D direction in local coordinates to 3D world coords
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 vecDirToWorld(Vector2 vecLocalDir)
        {
            Vector3 vec = vecLocalDir.X * vecLx 
                            + vecLocalDir.Y * vecLy;

            return vecSafeNormalize(vec);
        }

        /// <summary>
        /// Return local coordinate from world coordinates
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 vecFromWorld(Vector3 vecWorld)
        {
            Vector3 vecR = vecWorld - vecPos;
            
            return new Vector3( Vector3.Dot(vecR, vecLx), 
                                Vector3.Dot(vecR, vecLy), 
                                Vector3.Dot(vecR, vecLz));
        }

        /// <summary>
        /// Return local direction from world direction
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 vecDirFromWorld(Vector3 vecWorldDir)
        {
            Vector3 vec = new Vector3(  Vector3.Dot(vecWorldDir, vecLx), 
                                        Vector3.Dot(vecWorldDir, vecLy), 
                                        Vector3.Dot(vecWorldDir, vecLz));

            return vecSafeNormalize(vec);
        }

        /// <summary>
        /// Create a combined LocalFrame from this frame and another
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmCompose(in LocalFrame frmOther)
        {
            Vector3 vecP = vecPtToWorld(frmOther.vecPos);
            Vector3 vecX = vecDirToWorld(frmOther.vecLx);
            Vector3 vecZ = vecDirToWorld(frmOther.vecLz);
            return new LocalFrame(vecP, vecZ, vecX);
        }

        /// <summary>
        /// Create an inverted LocalFrame object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmInverse()
        {
            // Columns of R are {Lx, Ly, Lz}; R^T rows are those same vectors.
            Vector3 vecTInv = new ( -Vector3.Dot(vecLx, vecPos),
                                    -Vector3.Dot(vecLy, vecPos),
                                    -Vector3.Dot(vecLz, vecPos));

            // Inverse maps world to local; its Z/X are rows of R (i.e., original basis as world dirs).
            // Reuse ctor’s orthonormalization to rebuild consistent handedness.
            Vector3 vecRtZ = new (vecLz.X, vecLz.Y, vecLz.Z);
            Vector3 vecRtX = new (vecLx.X, vecLx.Y, vecLx.Z);
            return new LocalFrame(vecTInv, vecRtZ, vecRtX);
        }

        /// <summary>
        /// Move the origin of the LocalFrame object by the specified distance in local space
        /// </summary>
        /// <param name="vecDistance">Distance to move the origin</param>
        /// <returns>A LocalFrame object that is moved by the specified distance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMovedLocal(Vector3 vecDistance)
            => new( vecPos  + vecDistance.X * vecLx 
                            + vecDistance.Y * vecLy 
                            + vecDistance.Z * vecLz, 
                    vecLz, 
                    vecLx);

        /// <summary>
        /// Move the LocalFrame origin by the specified distance in X in local space
        /// </summary>
        /// <param name="fDistanceX">Distance to move the origin in X local space</param>
        /// <returns>A LocalFrame object that is moved by the specified distance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMovedLocalX(float fDistanceX)
            => new(vecPos + fDistanceX * vecLx, vecLz, vecLx);

        /// <summary>
        /// Move the LocalFrame origin by the specified distance in Y in local space
        /// </summary>
        /// <param name="fDistanceY">Distance to move the origin in Y in local space</param>
        /// <returns>A LocalFrame object that is moved by the specified distance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMovedLocalY(float fDistanceY)
            => new(vecPos + fDistanceY * vecLy, vecLz, vecLx);

        /// <summary>
        /// Move the LocalFrame origin by the specified distance in Z in local space
        /// </summary>
        /// <param name="fDistanceZ">Distance to move the origin in Z in local space</param>
        /// <returns>A LocalFrame object that is moved by the specified distance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMovedLocalZ(float fDistanceZ)
            => new(vecPos + fDistanceZ * vecLz, vecLz, vecLx);

        /// <summary>
        /// Rotate the LocalFrame around an arbitrary (world-space) axis through the frame’s origin.
        /// </summary>
        /// <param name="vecAxis">Rotation axis in world coordinates</param>
        /// <param name="fAngleRad">Rotation angle in radians</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmRotatedWorld(Vector3 vecAxis, float fAngleRad)
        {
            var q = Quaternion.CreateFromAxisAngle(vecSafeNormalize(vecAxis), fAngleRad);
            var x = Vector3.Transform(vecLx, q);
            var y = Vector3.Transform(vecLy, q);
            var z = Vector3.Transform(vecLz, q);
            return new LocalFrame(vecPos, z, x);
        }

        /// <summary>
        /// Move the origin of the LocalFrame object by the specified distance in world space
        /// </summary>
        /// <param name="vecDistance">Distance to move the origin in world space</param>
        /// <returns>A LocalFrame object that is moved by the specified distance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMovedWorld(Vector3 vecDistance)
            => new(vecPos + vecDistance, vecLz, vecLx);

        /// <summary>
        /// Move the LocalFrame origin by the specified distance in X in world space
        /// </summary>
        /// <param name="fDistanceX">Distance to move the origin in X in world space</param>
        /// <returns>A LocalFrame object that is moved by the specified distance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMovedWorldX(float fDistanceX)
            => new(vecPos + new Vector3(fDistanceX,0,0), vecLz, vecLx);

        /// <summary>
        /// Move the LocalFrame origin by the specified distance in Y in world space
        /// </summary>
        /// <param name="fDistanceY">Distance to move the origin in Y in world space</param>
        /// <returns>A LocalFrame object that is moved by the specified distance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMovedWorldY(float fDistanceY)
            => new(vecPos + new Vector3(0,fDistanceY,0), vecLz, vecLx);

        /// <summary>
        /// Move the LocalFrame origin by the specified distance in Z in world space
        /// </summary>
        /// <param name="fDistanceZ">Distance to move the origin in Z in world space</param>
        /// <returns>A LocalFrame object that is moved by the specified distance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMovedWorldZ(float fDistanceZ)
            => new(vecPos + new Vector3(0,0,fDistanceZ), vecLz, vecLx);

        /// <summary>
        /// Mirror LocalFrame by flipping selected local axes.
        /// Note, it preserves right-handedness of the coordinate system
        /// so some transformations will be equivalent to a 180º rotation
        /// </summary>
        /// <param name="bMirrorZ">Mirror local Z axis</param>
        /// <param name="bMirrorX">Mirror local X axis</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame frmMirrored(bool bMirrorZ, bool bMirrorX)
        {
            Vector3 vecZ = bMirrorZ ? -vecLz : vecLz;
            Vector3 vecX = bMirrorX ? -vecLx : vecLx;
            return new LocalFrame(vecPos, vecZ, vecX);
        }

        /// <summary>
        /// Convert the LocalFrame transformation to an equivalent
        /// Matrix4x4 transform (basis in rows, translation last column)
        /// This layout is compatible with typical OpenGL shaders
        /// </summary>
        /// <returns>Rigid transform Matrix4x4</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 matAsMatrix4x4()
            => new Matrix4x4(
                    vecLx.X, vecLx.Y, vecLx.Z, 0f,
                    vecLy.X, vecLy.Y, vecLy.Z, 0f,
                    vecLz.X, vecLz.Y, vecLz.Z, 0f,
                    vecPos.X, vecPos.Y, vecPos.Z, 1f);

        /// <summary>
        /// Return the transformation as Quaternion plus Origin
        /// </summary>
        /// <param name="q">Rotation component as Quaternion</param>
        /// <param name="vecOrigin">Origin (same as vecPos)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AsRigid(out Quaternion q, out Vector3 vecOrigin)
        {
            q = Quaternion.CreateFromRotationMatrix(matAsMatrix4x4());
            vecOrigin = vecPos;
        }

        /// <summary>
        /// Helper function to drawing a scaled quad aligned to this
        /// LocalFrame object
        /// Model = Scale * Frame.M
        /// </summary>
        /// <param name="vecScale">Scale to apply</param>
        /// <returns>A Matrix4x4 matrix containing the rigid transformation and the additional scale applied</returns>
        public Matrix4x4 matComposeWithScale(in Vector3 vecScale)
        {
            Matrix4x4 matS = Matrix4x4.CreateScale(vecScale);
            return matS * matAsMatrix4x4(); // row-vector: scale -> then frame
        }
        
        /// <summary>
        /// Convert local point to a world coordinate (same as vecToWorld)
        /// Enables you to write vecWorld = vecLocal * frmLocalFrame
        /// </summary>
        /// <param name="frm">LocalFrame</param>
        /// <param name="vecLocal">Local coordinate point</param>
        /// <returns>Point in vorld coordinates</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in LocalFrame frm, Vector3 vecLocal) 
            => frm.vecPtToWorld(vecLocal);

        /// <summary>
        /// Combine the transformation of two LocalFrame objects into one
        /// (indentical to frmCompose)
        /// </summary>
        /// <param name="frmA">First LocalFrame object</param>
        /// <param name="frmB">Second LocalFrame object</param>
        /// <returns>LocalFrame object that combines both transformations</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalFrame operator *(in LocalFrame frmA, in LocalFrame frmB) 
            => frmA.frmCompose(frmB);


        /// <summary>
        /// Test for equality (IEquatable)
        /// </summary>
        public bool Equals(LocalFrame frm)
            =>  vecPos.Equals(frm.vecPos) 
                    && vecLx.Equals(frm.vecLx)
                    && vecLy.Equals(frm.vecLy) 
                    && vecLz.Equals(frm.vecLz);

       
        /// <summary>
        /// Test for equality (IEquatable)
        /// </summary>
        public override bool Equals(object? obj) 
            => obj is LocalFrame lf && Equals(lf);

        /// <summary>
        /// Create hash code (IEquatable)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => HashCode.Combine(vecPos, vecLx, vecLy, vecLz);

        /// <summary>
        /// Safely normalize a Vector3 without creating NaNs
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 vecSafeNormalize(Vector3 vec)
            => vec.LengthSquared() > 0f ? Vector3.Normalize(vec) : vec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Orthonormalize( in Vector3 vecInZ, 
                                            in Vector3 vecInX,
                                            out Vector3 vecZ, 
                                            out Vector3 vecX, 
                                            out Vector3 vecY)
        {
            vecZ = vecSafeNormalize(vecInZ);
            vecX = vecInX - Vector3.Dot(vecInX, vecZ) * vecZ;
            vecX = vecSafeNormalize(vecX);
            vecY = Vector3.Cross(vecZ, vecX); 
            // right-handed; |Y| = 1 if Z,X are unit & orthogonal
        }

        [Conditional("DEBUG")]
        private static void AssertOrthonormal(  in Vector3 vecZ, 
                                                in Vector3 vecX, 
                                                in Vector3 vecY)
        {
            const float eps = 1e-5f;
            bool bUnit =    float.Abs(vecZ.LengthSquared() - 1f) < eps
                            && float.Abs(vecX.LengthSquared() - 1f) < eps
                            && float.Abs(vecY.LengthSquared() - 1f) < eps;
            bool bOrtho =   float.Abs(Vector3.Dot(vecZ, vecX)) < 1e-4f
                            && float.Abs(Vector3.Dot(vecZ, vecY)) < 1e-4f
                            && float.Abs(Vector3.Dot(vecX, vecY)) < 1e-4f;
            bool bRightH =  Vector3.DistanceSquared(Vector3.Normalize(Vector3.Cross(vecZ, vecX)), vecY) < 1e-6f;

            Debug.Assert(bUnit && bOrtho && bRightH);
        }
    }
}