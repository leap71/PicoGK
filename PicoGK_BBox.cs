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

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PicoGK
{
    /// <summary>
    /// 2D Bounding Box object
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BBox2
    {
        /// <summary>
        /// Creates an empty Bounding Box
        /// </summary>
        public BBox2()
        {
            vecMin.X = float.MaxValue;
            vecMin.Y = float.MaxValue;

            vecMax.X = float.MinValue;
            vecMax.Y = float.MinValue;
        }

        public BBox2(   float fMinX,
                        float fMinY,
                        float fMaxX,
                        float fMaxY)
        {
            vecMin.X = fMinX;
            vecMin.Y = fMinY;

            vecMax.X = fMaxX;
            vecMax.Y = fMaxY;

            // Making sure you have set this correctly
            Debug.Assert(vecMin.X <= vecMax.X);
            Debug.Assert(vecMin.Y <= vecMax.Y);
        }

        /// <summary>
        /// Creates a Bounding Box with the specified min/max
        /// </summary>
        /// <param name="vecSetMin">Minimum Coord</param>
        /// <param name="vecSetMax">Maximum Coord</param>
        public BBox2(   in Vector2 vecSetMin,
                        in Vector2 vecSetMax)
        {
            vecMin = vecSetMin;
            vecMax = vecSetMax;

            // Making sure you have set this correctly
            Debug.Assert(vecMin.X <= vecMax.X);
            Debug.Assert(vecMin.Y <= vecMax.Y);
        }

        /// <summary>
        /// Is the BoundingBox empty?
        /// </summary>
        /// <returns>True: Empty</returns>
        public bool bIsEmpty()
        {
            if (vecMin.X == float.MaxValue)
            {
                Debug.Assert(vecMin.Y == float.MaxValue);
                Debug.Assert(vecMax.X == float.MinValue);
                Debug.Assert(vecMax.Y == float.MinValue);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Include the specified vector in the bounding box
        /// </summary>
        /// <param name="vec">Vector to include</param>
        public void Include(Vector2 vec)
        {
            vecMin.X = Math.Min(vecMin.X, vec.X);
            vecMin.Y = Math.Min(vecMin.Y, vec.Y);
            vecMax.X = Math.Max(vecMax.X, vec.X);
            vecMax.Y = Math.Max(vecMax.Y, vec.Y);
        }

        /// <summary>
        /// Include the specified Bounding Box in this Box
        /// </summary>
        /// <param name="oBox">Bounding Box to include</param>
        public void Include(BBox2 oBox)
        {
            Include(oBox.vecMin);
            Include(oBox.vecMax);
        }

        /// <summary>
        /// Grows the bounding box by the specified value on each side
        /// I.E. the width, for example is width + 2*fGrowBy afterwards
        /// </summary>
        /// <param name="fGrowBy"></param>
        public void Grow(float fGrowBy)
        {
            if (bIsEmpty())
            {
                vecMin.X = -fGrowBy;
                vecMin.Y = -fGrowBy;
                vecMax.X = fGrowBy;
                vecMax.Y = fGrowBy;
            }
            else
            {
                vecMin.X -= fGrowBy;
                vecMin.Y -= fGrowBy;
                vecMax.X += fGrowBy;
                vecMax.Y += fGrowBy;
            }
        }

        /// <summary>
        /// Size of the Bounding Box
        /// </summary>
        /// <returns>A vector corresponding to width and height</returns>
        public Vector2 vecSize()
        {
            return vecMax - vecMin;
        }

        /// <summary>
        /// Center point of the bounding box
        /// </summary>
        /// <returns>Vector representing the center point</returns>
        public Vector2 vecCenter()
        {
            return vecMin + vecSize() / 2;
        }

        /// <summary>
        /// A string representation of the Bounding Box
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<Min: " + vecMin.ToString() + " | Max: " + vecMax.ToString() + ">";
        }

        public Vector2 vecMin;
        public Vector2 vecMax;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BBox3
    {
        /// <summary>
        /// Create an empty Bounding Box
        /// </summary>
        public BBox3()
        {
            vecMin.X = float.MaxValue;
            vecMin.Y = float.MaxValue;
            vecMin.Z = float.MaxValue;

            vecMax.X = float.MinValue;
            vecMax.Y = float.MinValue;
            vecMax.Z = float.MinValue;
        }

        public BBox3(   float fMinX,
                        float fMinY,
                        float fMinZ,
                        float fMaxX,
                        float fMaxY,
                        float fMaxZ)
        {
            vecMin.X = fMinX;
            vecMin.Y = fMinY;
            vecMin.Z = fMinZ;

            vecMax.X = fMaxX;
            vecMax.Y = fMaxY;
            vecMax.Z = fMaxZ;

            // Making sure you have set this correctly
            Debug.Assert(vecMin.X <= vecMax.X);
            Debug.Assert(vecMin.Y <= vecMax.Y);
            Debug.Assert(vecMin.Z <= vecMax.Z);
        }

        /// <summary>
        /// Create a Bounding Box based on the specified min/max vectors
        /// </summary>
        /// <param name="vecSetMin">Minimum vector</param>
        /// <param name="vecSetMax">Maximum vector</param>
        public BBox3(   in Vector3 vecSetMin,
                        in Vector3 vecSetMax)
        {
            vecMin = vecSetMin;
            vecMax = vecSetMax;

            // Making sure you have set this correctly
            Debug.Assert(vecMin.X <= vecMax.X);
            Debug.Assert(vecMin.Y <= vecMax.Y);
            Debug.Assert(vecMin.Z <= vecMax.Z);
        }

        /// <summary>
        /// Size of the Bounding Box
        /// </summary>
        /// <returns>A 3D Vector corresponding to width, height, depth of the box</returns>
        public Vector3 vecSize()
        {
            return vecMax - vecMin;
        }

        /// <summary>
        /// Is the Bounding Box empty>
        /// </summary>
        /// <returns>True: Empty</returns>
        public bool bIsEmpty()
        {
            if (vecMin.X == float.MaxValue)
            {
                Debug.Assert(vecMin.Y == float.MaxValue);
                Debug.Assert(vecMin.Z == float.MaxValue);
                Debug.Assert(vecMax.X == float.MinValue);
                Debug.Assert(vecMax.Y == float.MinValue);
                Debug.Assert(vecMax.Z == float.MinValue);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Include the specified vector in the Bounding Box
        /// </summary>
        /// <param name="vec">Vector to include</param>
        public void Include(Vector3 vec)
        {
            vecMin.X = Math.Min(vecMin.X, vec.X);
            vecMin.Y = Math.Min(vecMin.Y, vec.Y);
            vecMin.Z = Math.Min(vecMin.Z, vec.Z);
            vecMax.X = Math.Max(vecMax.X, vec.X);
            vecMax.Y = Math.Max(vecMax.Y, vec.Y);
            vecMax.Z = Math.Max(vecMax.Z, vec.Z);
        }

        /// <summary>
        /// Include the specified Bounding Box into this box
        /// </summary>
        /// <param name="oBox">The box to include</param>
        public void Include(BBox3 oBox)
        {
            Include(oBox.vecMin);
            Include(oBox.vecMax);
        }

        /// <summary>
        /// Include the specified 2D Bounding Box, with optional Z coord
        /// </summary>
        /// <param name="oBox">2D Bounding Box to include</param>
        /// <param name="fZ">At which Z coord</param>
        public void Include(BBox2 oBox, float fZ = 0.0f)
        {
            Include(new Vector3(oBox.vecMin.X, oBox.vecMin.Y, fZ));
            Include(new Vector3(oBox.vecMax.X, oBox.vecMax.Y, fZ));
        }

        /// <summary>
        /// Grows the bounding box by the specified value on each side
        /// I.E. the width, for example is width + 2*fGrowBy afterwards
        /// </summary>
        /// <param name="fGrowBy"></param>
        public void Grow(float fGrowBy)
        {
            if (bIsEmpty())
            {
                vecMin.X = -fGrowBy;
                vecMin.Y = -fGrowBy;
                vecMin.Z = -fGrowBy;
                vecMax.X = fGrowBy;
                vecMax.Y = fGrowBy;
                vecMax.Z = fGrowBy;
            }
            else
            {
                vecMin.X -= fGrowBy;
                vecMin.Y -= fGrowBy;
                vecMin.Z -= fGrowBy;
                vecMax.X += fGrowBy;
                vecMax.Y += fGrowBy;
                vecMax.Z += fGrowBy;
            }
        }

        /// <summary>
        /// Center of the Bounding Box
        /// </summary>
        /// <returns>The center vector of the Bounding Box</returns>
        public Vector3 vecCenter()
        {
            return vecMin + vecSize() / 2;
        }

        /// <summary>
        /// Fit the specified Bounding Box into this box, returning Scale and Offset
        /// </summary>
        /// <param name="oBounds">Bounding box to fit into this box</param>
        /// <param name="fScale">How much does it need to be scaled?</param>
        /// <param name="vecOffset">How much does it need to be offset after scale</param>
        /// <returns></returns>
        public BBox3 oFitInto(  in  BBox3   oBounds,
                                out float   fScale,
                                out Vector3 vecOffset)
        {
            Vector3 vecNewMin = vecMin;
            Vector3 vecNewMax = vecMax;

            float fScaleX = oBounds.vecSize().X / vecSize().X;
            float fScaleY = oBounds.vecSize().Y / vecSize().Y;
            float fScaleZ = oBounds.vecSize().Z / vecSize().Z;

            fScale = Math.Min(Math.Min(fScaleX, fScaleY), fScaleZ);

            vecNewMin *= fScale;
            vecNewMax *= fScale;

            BBox3 oBB = new BBox3(vecNewMin, vecNewMax);
            vecOffset = oBounds.vecCenter() - oBB.vecCenter();

            oBB.vecMin += vecOffset;
            oBB.vecMax += vecOffset;

            return oBB;
        }

        /// <summary>
        /// A function to return a random point in a Bounding Box
        /// </summary>
        /// <param name="oRand">Random object</param>
        /// <returns>A random vector inside of this Bounding Box</returns>
        public Vector3 vecRandomVectorInside(ref Random oRand)
        {
            return new Vector3( oRand.NextSingle() * vecSize().X + vecMin.X,
                                oRand.NextSingle() * vecSize().Y + vecMin.Y,
                                oRand.NextSingle() * vecSize().Z + vecMin.Z);
        }

        /// <summary>
        /// Return the 2D extent of this Bounding Box
        /// </summary>
        /// <returns>A 2D Bounding Box with the X/Y extent of this Bounding Box</returns>
        public BBox2 oAsBoundingBox2()
        {
            BBox2 oBB2 = new();
            oBB2.Include(new Vector2(vecMin.X, vecMin.Y));
            oBB2.Include(new Vector2(vecMax.X, vecMax.Y));
            return oBB2;
        }

        /// <summary>
        /// Return the Bounding Box as string
        /// </summary>
        /// <returns>String with the extent of the box</returns>
        public override string ToString()
        {
            return "<Min: " + vecMin.ToString() + " | Max: " + vecMax.ToString() + ">";
        }

        public Vector3 vecMin;
        public Vector3 vecMax;
    }
}