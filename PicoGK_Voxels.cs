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
    /// Function signature for signed distance implicts
    /// </summary>
    public interface IImplicit
    {
        /// <summary>
        /// Return the signed distance to the iso surface
        /// </summary>
        /// <param name="vec">Real world point to sample</param>
        /// <returns>
        /// Distance to the Iso surface in real world values
        /// 0.0 is at the surface
        /// Negative values indicate the inside of the object
        /// Positive values indicate the outside of the object
        /// </returns>
        public abstract float fSignedDistance(in Vector3 vec);
    }

    public partial class Voxels
    {
        /// <summary>
        /// Default constructor, builds a new empty voxel field
        /// </summary>
        public Voxels()
        {
            m_hThis = _hCreate();
            Debug.Assert(m_hThis != IntPtr.Zero);
        }

        /// <summary>
        /// Copy constructor, create a duplicate
        /// of the supplied voxel field
        /// </summary>
        /// <param name="oSource">Source to copy from</param>
        public Voxels(in Voxels voxSource)
        {
            m_hThis = _hCreateCopy(voxSource.m_hThis);
            Debug.Assert(m_hThis != IntPtr.Zero);
        }

        /// <summary>
        /// Creates a new voxel field and renders it using the
        /// implicit function specified
        /// </summary>
        /// <param name="oImplicit">Object producing a signed distance field</param>
        public Voxels(  in IImplicit xImplicit,
                        in BBox3 oBounds) : this()
        {
            RenderImplicit(xImplicit, oBounds);
        }

        /// <summary>
        /// Creates a new voxel field form a mesh
        /// </summary>
        /// <param name="msh">The mesh that is rendered into the voxels</param>
        public Voxels(in Mesh msh) : this()
        {
            RenderMesh(msh);
        }

        /// <summary>
        /// Creates a new voxel field from a lattice
        /// </summary>
        /// <param name="lat">The lattice used</param>
        public Voxels(in Lattice lat) : this()
        {
            RenderLattice(lat);
        }

        /// <summary>
        /// Return the current voxel field as a mesh
        /// </summary>
        /// <returns>The meshed result of the voxel field</returns>
        public Mesh mshAsMesh()
        {
            return new Mesh(this);
        }

        /// <summary>
        /// Performs a boolean union between two voxel fields
        /// Our voxelfield will have all the voxels set that the operands also has set
        /// </summary>
        /// <param name="voxOperand">Voxels to add to our field</param>
        public void BoolAdd(in Voxels voxOperand)
            => _BoolAdd(m_hThis, voxOperand.m_hThis);

        /// <summary>
        /// Performs a boolean difference between the two voxel fields
        /// Our voxel field's voxel will have all the matter removed
        /// that is set in the operand
        /// </summary>
        /// <param name="voxOperand">Voxels to remove from our field</param>
        public void BoolSubtract(in Voxels voxOperand)
            => _BoolSubtract(m_hThis, voxOperand.m_hThis);

        /// <summary>
        /// Performs a boolean intersection between two voxel fields.
        /// Our fields will have all voxels removed, that are not
        /// inside the Operand's field
        /// </summary>
        /// <param name="voxOperand">Voxels masking our voxel field</param>
        public void BoolIntersect(in Voxels voxOperand)
            => _BoolIntersect(m_hThis, voxOperand.m_hThis);

        /// <summary>
        /// Performs a smooth boolean union, with the objects merged
        /// into each other over the specified distance
        /// </summary>
        /// <param name="voxOperand">The voxels to add to ours</param>
        /// <param name="fDist">The distance in MM over which to merge</param>
        public void BoolAddSmoothVx(in Voxels voxOperand, float fDist)
            => _BoolAddSmooth(m_hThis, voxOperand.m_hThis, fDist);

        /// <summary>
        /// Performs a smooth boolean union, with the objects merged
        /// into each other over the specified distance
        /// </summary>
        /// <param name="voxOperand">The voxels to add to ours</param>
        /// <param name="fDistMM">The distance in millimeters over which to merge</param>
        public void BoolAddSmooth(in Voxels voxOperand, float fDistMM)
            => _BoolAddSmooth( m_hThis, voxOperand.m_hThis, fDistMM);

        /// <summary>
        /// Offsets the voxel field by the specified distance.
        /// The surface of the voxel field is moved outward or inward
        /// Outward is positive, inward is negative
        /// </summary>
        /// <param name="fDistMM">The distance to move the surface outward (positive) or inward (negative) in millimeters</param>
        public void Offset(float fDistMM)
            => _Offset(m_hThis, fDistMM);

        /// <summary>
        /// Offsets the voxel field twice, but the specified distances
        /// Outwards is positive, inwards is negative
        /// </summary>
        /// <param name="fDist1MM">First offset distance in mm</param>
        /// <param name="fDist2MM">Second distance in mm</param>
        public void DoubleOffset(   float fDist1MM,
                                    float fDist2MM)
            => _DoubleOffset(m_hThis, fDist1MM, fDist2MM);

        /// <summary>
        /// Offsets the voxel field three times by the specified distance.
        /// First it offsets inwards by the specified distance
        /// Then it offsets twice the distance outwards
        /// Then it offsets the distance inwards again
        /// This is useful to smoothen a voxel field. By offsetting inwards
        /// you eliminate all convex detail below a certain threshold
        /// by offsetting outwards, you eliminated concave detail below a threshold
        /// by offsetting inwards again, you are back to the size of the object
        /// that you started with, but without the detail
        /// Usually call this with a positive number, although you can reverse
        /// the operations by using a negative number
        /// </summary>
        /// <param name="fDistMM">Distance to move (in mm)</param>
        public void TripleOffset(  float fDistMM)
            => _TripleOffset(m_hThis, fDistMM);

        /// <summary>
        /// Renders a mesh into the voxel field, combining it with
        /// the existing content
        /// </summary>
        /// <param name="msh">The mesh to render (needs to be a closed surface)</param>
        public void RenderMesh(in Mesh msh)
            => _RenderMesh(m_hThis, msh.m_hThis);

        /// <summary>
        /// Render an implicit signed distance function into the voxels
        /// overwriting the existing content with the voxels where the implicit
        /// function returns <= 0
        /// You will often want to use IntersectImplicit instead
        /// </summary>
        /// <param name="xImp">Implicit object with signed distance function</param>
        /// <param name="oBounds">Bounding box in which to render the implicit</param>
        public void RenderImplicit( in IImplicit xImp,
                                    in BBox3 oBounds)
            => _RenderImplicit(m_hThis, in oBounds, xImp.fSignedDistance);

        /// <summary>
        /// Render an implicit signed distance function into the voxels
        /// but using the existing voxels as a mask.
        /// If the voxel field contains a voxel at a given position, the voxel
        /// will be set to true if the signed distance function returns <= 0
        /// and false if the signed distance function returns > 0
        /// So a voxel field, containting a filled sphere, will contain a
        /// Gyroid Sphere, if used with a Gyroid implict
        /// </summary>
        /// <param name="xImp">Implicit object with signed distance function<</param>
        public void IntersectImplicit(in IImplicit xImp)
            => _IntersectImplicit(m_hThis, xImp.fSignedDistance);

        /// <summary>
        /// Renders a lattice into the voxel field, combining it with
        /// the existing content
        /// </summary>
        /// <param name="lat">The lattice to render</param>
        public void RenderLattice(in Lattice lat)
            => _RenderLattice(m_hThis, lat.m_hThis);

        /// <summary>
        /// Projects the slices at the start Z position upwards or downwards,
        /// until it reaches the end Z position.
        /// </summary>
        /// <param name="nStartZMM">Start voxel slice in mm</param>
        /// <param name="nEndZMM">End voxel slice in mm</param>
        public void ProjectZSlice(  float fStartZMM,
                                    float fEndZMM)
            => _ProjectZSlice(  m_hThis, fStartZMM, fEndZMM);

        /// <summary>
        /// Returns true if the voxel fields contain the same content
        /// </summary>
        /// <param name="voxOther">Voxels to compare to</param>
        /// <returns></returns>
        public bool bIsEqual(in Voxels voxOther)
            => _bIsEqual(m_hThis, voxOther.m_hThis);

        /// <summary>
        /// This function evaluates the entire voxel field and returns
        /// the volume of all voxels in cubic millimeters and the Bounding Box
        /// in real world coordinates
        /// Note this function is potentially slow, as it needs to traverse the
        /// entire voxel field
        /// </summary>
        /// <param name="fVolumeCubicMM">Cubic MMs of volume filled with voxels</param>
        /// <param name="oBBox">The real world bounding box of the voxels</param>
        public void CalculateProperties(out float fVolumeCubicMM,
                                            out BBox3 oBBox)
        {
            oBBox = new();
           _CalculateProperties(m_hThis, out fVolumeCubicMM, ref oBBox);
        }

        /// <summary>
        /// Returns the closest point from the search point on the surface
        /// of the voxel field
        /// </summary>
        /// <param name="vecSearch">Search position</param>
        /// <param name="vecSurfacePoint">Point on the surface</param>
        /// <returns>True if point is found, false if field is empty</returns>
        public bool bClosestPointOnSurface( in  Vector3 vecSearch,
                                            out Vector3 vecSurfacePoint)
        {
            vecSurfacePoint     = new();
            return _bClosestPointOnSurface( m_hThis,
                                            in  vecSearch,
                                            ref vecSurfacePoint);
        }

        /// <summary>
        /// Casts a ray to the surface of a voxel field and finds the
        /// the point on the surface where the ray intersects
        /// </summary>
        /// <param name="vecSearch">Search point</param>
        /// <param name="vecDirection">Direction to search in</param>
        /// <param name="vecSurfacePoint">Point on the surface</param>
        /// <returns>True, point found. False, no surface in this direction</returns>
        public bool bRayCastToSurface(  in  Vector3 vecSearch,
                                        in  Vector3 vecDirection,
                                        out Vector3 vecSurfacePoint)
        {
            vecSurfacePoint     = new();
            return _bRayCastToSurface( m_hThis,
                                       in  vecSearch,
                                       in  vecDirection,
                                       ref vecSurfacePoint);
        }

        /// <summary>
        /// Returns the dimensions of the voxel field in discrete voxels
        /// </summary>
        /// <param name="nXSize">Size in x direction in voxels</param>
        /// <param name="nYSize">Size in y direction in voxels</param>
        /// <param name="nZSize">Size in z direction in voxels</param>
        public void GetVoxelDimensions( out int nXSize,
                                        out int nYSize,
                                        out int nZSize)
        {
            _GetVoxelDimensions(    m_hThis,
                                    out nXSize,
                                    out nYSize,
                                    out nZSize);
        }

        /// <summary>
        /// Returns a signed distance-field-encoded slice of the voxel field
        /// To use it, use GetVoxelDimensions to find out the size of the voxel
        /// field in voxel units. Then allocate a new grayscale image to copy
        /// the data into, and pass it as a reference. Since GetVoxelDimensions
        /// is potentially an "expensive" function, we are putting the burden
        /// on you to allocate an image and don't create it for you. You can
        /// also re-use the image if you want to save an entire image stack
        /// </summary>
        /// <param name="nZSlice">Slice to retrieve. 0 is at the bottom.</param>
        /// <param name="img">Pre-allocated grayscale image to receive the values</param>
        public void GetVoxelSlice(  in int nZSlice,
                                    ref ImageGrayScale img)
        {
            GCHandle oPinnedArray = GCHandle.Alloc(img.m_afValues, GCHandleType.Pinned);
            try
            {
                IntPtr afBufferPtr = oPinnedArray.AddrOfPinnedObject();
                _GetVoxelSlice(m_hThis, nZSlice, afBufferPtr);
            }
            finally
            {
                oPinnedArray.Free();
            }
        }
    }
}