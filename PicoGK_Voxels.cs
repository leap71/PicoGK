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

    /// <summary>
    /// Interface for a bounded implicit function. Like IImplicit, but
    /// allows querying the bounding box of the function. Use when the
    /// implicit defines a shape that has bounds (say, a sphere), vs.
    /// an unbounded function (a gyroid)
    /// </summary>
    public interface IBoundedImplicit : IImplicit
    {
        /// <summary>
        /// Access the bounding box of the implicit function
        /// </summary>
        BBox3 oBounds {get;}
    }

    public partial class Voxels
    {
        /// <summary>
        /// Create a voxels object from an existing handle
        /// (for internal use)
        /// </summary>
        internal Voxels(    Library libSet,
                            VoxHandle hVoxels)
        {
            lib     = libSet;
            hThis   = hVoxels;

            if (!_bIsValid(lib.hThis,hThis))
                throw new PicoGKAllocException();

            m_oMetadata = new(lib, FieldMetadata._hFromVoxels(lib.hThis, hThis));
            m_oMetadata._SetValue("PicoGK.Class", "Voxels");
        }

        /// <summary>
        /// Default constructor, builds a new empty voxel field
        /// </summary>
        public Voxels(Library libSet)
            : this( libSet, 
                    _hCreate(libSet.hThis))
        {
            if (!_bIsValid(lib.hThis,hThis))
                throw new PicoGKAllocException();
        }

        /// <summary>
        /// Copy constructor, create a duplicate
        /// of the supplied voxel field
        /// </summary>
        /// <param name="voxSource">Source to copy from</param>
        public Voxels(in Voxels voxSource)
            : this( voxSource.lib,
                    _hCreateCopy(   voxSource.lib.hThis,
                                    voxSource.hThis))
        {}

        /// <summary>
        /// Create a duplicate of the current voxel field
        /// </summary>
        public Voxels voxDuplicate()
        {
            return new Voxels(this);
        }

        /// <summary>
        /// Create a voxel field from a supplied scalar field
        /// the scalar field needs to contain a valid discretized
        /// signed distance field for this to work properly
        /// </summary>
        /// <param name="oSource">Source to copy from</param>
        public Voxels(in ScalarField oSource)
            : this(oSource.lib, oSource, oSource.oBoundingBox())
        {}

        /// <summary>
        /// Creates a new voxel field and renders it using the
        /// implicit function specified
        /// </summary>
        /// <param name="libSet">Library instance to use</param>
        /// <param name="xImplicit">Implicit function to render</param>
        /// <param name="oBounds">Boundary box to evaluate for implicit function</param>
        public Voxels(  Library libSet,
                        in IImplicit xImplicit,
                        in BBox3 oBounds) : this(libSet)
        {
            RenderImplicit(xImplicit, oBounds);
        }

        /// <summary>
        /// Creates a new voxel field and renders it using the
        /// bounded implicit function specified
        /// </summary>
        /// <param name="libSet">Library instance to use</param>
        /// <param name="xImplicit">Implicit function to render</param>
        public Voxels(  Library libSet,
                        in IBoundedImplicit xImplicit) : this(libSet)
        {
            RenderImplicit(xImplicit, xImplicit.oBounds);
        }

        /// <summary>
        /// Creates a new voxel field form a mesh
        /// </summary>
        /// <param name="msh">The mesh that is rendered into the voxels</param>
        public Voxels(in Mesh msh) : this(msh.lib)
        {
            RenderMesh(msh);
        }

        /// <summary>
        /// Creates a new voxel field from a lattice
        /// </summary>
        /// <param name="lat">The lattice used</param>
        public Voxels(in Lattice lat) : this(lat.lib)
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
        /// Create a voxel field with a sphere inside
        /// </summary>
        /// <param name="libSet">Library instance to use</param>
        /// <param name="vecCenter">Center of the sphere</param>
        /// <param name="fRadius">Radius of the Sphere</param>
        /// <returns></returns>
        public static Voxels voxSphere( Library libSet,
                                        Vector3 vecCenter, 
                                        float fRadius)
        {
            Lattice lat = new(libSet);
            lat.AddSphere(vecCenter, fRadius);
            return new(lat);
        }

        /// <summary>
        /// Returns the amount of memory in bytes used by this object
        /// </summary>
        /// <returns>Number of bytes allocated to this object</returns>
        public long nMemUsage()
        {
            return _nMemUsage(lib.hThis, hThis);
        }

        /// <summary>
        /// Returns the voxel size in millimeters used in the voxel field
        /// </summary>
        /// <returns>Voxel size in mm</returns>
        public float fVoxelSize => _fVoxelSize(lib.hThis, hThis);

        /// <summary>
        /// Performs a boolean union between two voxel fields
        /// Our voxelfield will have all the voxels set that the operands also has set
        /// </summary>
        /// <param name="voxOperand">Voxels to add to our field</param>
        public void BoolAdd(in Voxels voxOperand)
        {
            if (voxOperand.lib.hThis != lib.hThis)
                throw new PicoGKLibraryMismatchException();

            _BoolAdd(   lib.hThis,
                        hThis, 
                        voxOperand.hThis);
        }

        /// <summary>
        /// Performs a boolean union operation on a copy of the current 
        /// voxel field and the operand and returns the copy. 
        /// The current voxel field remains unchanged
        /// </summary>
        /// <param name="voxOperand">Voxels to add</param>
        /// <returns></returns>
        public Voxels voxBoolAdd(in Voxels voxOperand)
        {
            Voxels vox = new(this);
            vox.BoolAdd(voxOperand);
            return vox;
        }

        /// <summary>
        /// Performs a boolean union of all voxels supplied in the
        /// container (List, Array, etc.)
        /// </summary>
        /// <param name="avoxList">Container containing Voxels to be added</param>
        public void BoolAddAll(in IEnumerable<Voxels> avoxList)
        {
            foreach (Voxels vox in avoxList)
                BoolAdd(vox);
        }

        /// <summary>
        /// Performs a boolean union of all voxels supplied in the
        /// container (List, Array, etc.) on a copy of the current voxel field
        /// and returns that copy
        /// </summary>
        /// <param name="avoxList">Container containing Voxels to be added</param>
        public Voxels voxBoolAddAll(in IEnumerable<Voxels> avoxList)
        {
            Voxels vox = new(this);
            vox.BoolAddAll(avoxList);
            return vox;
        }

        /// <summary>
        /// Combines two voxel fields and returns the result using BoolAdd
        /// </summary>
        /// <param name="vox1">First field</param>
        /// <param name="vox2">Second field</param>
        /// <returns>Combination of the two fields as new field</returns>
        public static Voxels voxCombine(    in Voxels vox1, 
                                            in Voxels vox2)
        {
            return vox1.voxBoolAdd(vox2);
        }

        /// <summary>
        /// Combines all voxel fields in the container and returns the result
        /// </summary>
        /// <param name="libSet">Library instance to use</param>
        /// <param name="avoxList">Container with the voxel fields</param>
        /// <returns>All voxel fields combined</returns>
        public static Voxels voxCombineAll( Library libSet,
                                            in IEnumerable<Voxels> avoxList)
        {
            Voxels vox = new(libSet);
            vox.BoolAddAll(avoxList);
            return vox;
        }

        /// <summary>
        /// Performs a boolean difference between the two voxel fields
        /// Our voxel field's voxel will have all the matter removed
        /// that is set in the operand
        /// </summary>
        /// <param name="voxOperand">Voxels to remove from our field</param>
        public void BoolSubtract(in Voxels voxOperand)
        {
            if (voxOperand.lib.hThis != lib.hThis)
                throw new PicoGKLibraryMismatchException();

            _BoolSubtract(lib.hThis, hThis, voxOperand.hThis);
        }

        /// <summary>
        /// Performs a boolean difference operation on a copy of the current 
        /// voxel field and the operand and returns the copy. 
        /// The current voxel field remains unchanged
        /// </summary>
        /// <param name="voxOperand">Voxels to add</param>
        /// <returns></returns>
        public Voxels voxBoolSubtract(in Voxels voxOperand)
        {
            Voxels vox = new(this);
            vox.BoolSubtract(voxOperand);
            return vox;
        }

        /// <summary>
        /// Subtracts on all voxels supplied in the container (List, Array, etc.)
        /// from the current field
        /// </summary>
        /// <param name="avoxList">Container containing Voxels to be subtracted</param>
        public void BoolSubtractAll(in IEnumerable<Voxels> avoxList)
        {
            foreach (Voxels vox in avoxList)
                BoolSubtract(vox);
        }

        /// <summary>
        /// Subtracts on all voxels supplied in the container (List, Array, etc.)
        /// from a copy of the current field and returns the result
        /// </summary>
        /// <param name="avoxList">Container containing Voxels to be subtracted</param>
        public Voxels voxBoolSubtractAll(in IEnumerable<Voxels> avoxList)
        {
            Voxels vox = new(this);
            vox.BoolSubtractAll(avoxList);
            return vox;
        }

        /// <summary>
        /// Performs a boolean intersection between two voxel fields.
        /// Our fields will have all voxels removed, that are not
        /// inside the Operand's field
        /// </summary>
        /// <param name="voxOperand">Voxels masking our voxel field</param>
        public void BoolIntersect(in Voxels voxOperand)
        {
            if (voxOperand.lib.hThis != lib.hThis)
                throw new PicoGKLibraryMismatchException();

            _BoolIntersect(lib.hThis, hThis, voxOperand.hThis);
        }

        /// <summary>
        /// Performs a boolean intersection operation on a copy of the current 
        /// voxel field and the operand and returns the copy. 
        /// The current voxel field remains unchanged
        /// </summary>
        /// <param name="voxOperand">Voxels to intersect with</param>
        /// <returns></returns>
        public Voxels voxBoolIntersect(in Voxels voxOperand)
        {
            Voxels vox = new(this);
            vox.BoolIntersect(voxOperand);
            return vox;
        }

        /// <summary>
        /// Overloaded operators allow you to do things like
        /// vox = vox1 + vox2
        /// </summary>
        public static Voxels operator +(Voxels voxA, Voxels voxB)
        {
            return voxA.voxBoolAdd(voxB);
        }

        /// <summary>
        /// Overloaded operators allow you to do things like
        /// vox = vox1 - vox2
        /// </summary>
        public static Voxels operator -(Voxels voxA, Voxels voxB)
        {
            return voxA.voxBoolSubtract(voxB);
        }

        /// <summary>
        /// Overloaded operator for intersect  (boolean AND)
        /// vox = vox1 &amp; vox2
        /// </summary>
        public static Voxels operator &(Voxels voxA, Voxels voxB)
        {
            return voxA.voxBoolIntersect(voxB);
        }

        /// <summary>
        /// Intersects the voxel field with the specified bounding box
        /// so all voxels outside the box are trimmed away
        /// </summary>
        /// <param name="oBox"></param>
        public void Trim(BBox3 oBox)
        {
            Voxels voxTrim = new(Utils.mshCreateCube(lib, oBox));
            BoolIntersect(voxTrim);
        }

        /// <summary>
        /// Offsets the voxel field by the specified distance.
        /// The surface of the voxel field is moved outward or inward
        /// Outward is positive, inward is negative
        /// </summary>
        /// <param name="fDistMM">The distance to move the surface outward (positive) or inward (negative) in millimeters</param>
        public void Offset(float fDistMM)
            => _Offset(lib.hThis, hThis, fDistMM);

        /// <summary>
        /// Offsets a copy of the voxel field by the specified distance.
        /// The surface of the voxel field is moved outward or inward
        /// Outward is positive, inward is negative.
        /// </summary>
        /// <param name="fDistMM">The distance to move the surface outward (positive) or inward (negative) in millimeters</param>
        /// <returns>Resulting field</returns>
        public Voxels voxOffset(float fDistMM)
        {
            Voxels vox = new(this);
            vox.Offset(fDistMM);
            return vox;
        }

        /// <summary>
        /// Offsets the voxel field twice, by the specified distances
        /// Outwards is positive, inwards is negative
        /// </summary>
        /// <param name="fDist1MM">First offset distance in mm</param>
        /// <param name="fDist2MM">Second distance in mm</param>
        public void DoubleOffset(   float fDist1MM,
                                    float fDist2MM)
            => _DoubleOffset(lib.hThis, hThis, fDist1MM, fDist2MM);


        /// <summary>
        /// Offsets a copy of the voxel field twice, by the specified distances
        /// Outwards is positive, inwards is negative 
        /// </summary>
        /// <param name="fDist1MM">First distance to offset</param>
        /// <param name="fDist2MM">Second distance to offset</param>
       /// <returns>Returns the resulting field</returns>
        public Voxels voxDoubleOffset(  float fDist1MM,
                                        float fDist2MM)
        {
            Voxels vox = new(this);
            vox.DoubleOffset(fDist1MM, fDist2MM);
            return vox;
        }

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
        public void TripleOffset(float fDistMM)
            => _TripleOffset(lib.hThis, hThis, fDistMM);

        /// <summary>
        /// Offsets a copy of the voxel field three times by the specified distance.
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
        /// <returns>Returns the resulting field</returns>
        public Voxels voxTripleOffset(float fDistMM)
        {
            Voxels vox = new(this);
            vox.TripleOffset(fDistMM);
            return vox;
        }

        /// <summary>
        /// Same as TripleOffset
        /// </summary>
        /// <param name="fDistMM">Distance to move (in mm)</param>
        /// <returns>Returns the resulting field</returns>
        public void Smoothen(float fDistMM)
            => TripleOffset(fDistMM);

        /// <summary>
        /// Same as TripleOffset
        /// </summary>
        /// <param name="fDistMM">Distance to move (in mm)</param>
        /// <returns>Returns the resulting field</returns>
        public Voxels voxSmoothen(float fDistMM)
            => voxTripleOffset(fDistMM);

        /// <summary>
        /// Similar to DoubleOffset, but allows you to
        /// specify the offsetted distance to the original
        /// surface as the second parameter.
        /// The surface is first offset by fFirstOffsetMM
        /// Then the surface is offset so that the final
        /// offset from the surface is fFinalSurfaceDistInMM
        /// </summary>
        /// <param name="fFirstOffsetMM">Initial offset</param>
        /// <param name="fFinalSurfaceDistInMM">absolute final offset value</param>
        public void OverOffset( float fFirstOffsetMM, 
                                float fFinalSurfaceDistInMM = 0)
        {
            DoubleOffset(   fFirstOffsetMM,
                            -(fFirstOffsetMM - fFinalSurfaceDistInMM));
        }

        /// <summary>
        /// Similar to DoubleOffset, but allows you to
        /// specify the offsetted distance to the original
        /// surface as the second parameter.
        /// The surface is first offset by fFirstOffsetMM
        /// Then the surface is offset so that the final
        /// offset from the surface is fFinalSurfaceDistInMM.
        /// 
        /// If not specified, the surface is where it was before.
        /// 
        /// This function is used to eliminate detail in the voxel
        /// field. It generates fillet-like results when used
        /// with a positive first offset.
        /// </summary>
        /// <param name="fFirstOffsetMM">Initial offset</param>
        /// <param name="fFinalSurfaceDistInMM">Absolute final offset from initial surface</param>
        public Voxels voxOverOffset(    float fFirstOffsetMM, 
                                        float fFinalSurfaceDistInMM = 0)
        {
            Voxels vox = new(this);
            vox.DoubleOffset(   fFirstOffsetMM,
                                -fFirstOffsetMM + fFinalSurfaceDistInMM);

            return vox;
        }

        /// <summary>
        /// Creates a fillet-like effect.
        /// Same as OverOffset with second value 0
        /// Since the effect is similar to a fillet, this makes
        /// a lot of code more readable
        /// </summary>
        /// <param name="fRoundingMM"></param>
        /// <returns></returns>
        public void Fillet(float fRoundingMM)
            => OverOffset(fRoundingMM); 

        /// <summary>
        /// Creates a fillet-like effect.
        /// Same as OverOffset with second value 0
        /// Since the effect is similar to a fillet, this makes
        /// a lot of code more readable
        /// </summary>
        /// <param name="fRoundingMM"></param>
        /// <returns></returns>
        public Voxels voxFillet(float fRoundingMM)
            => voxOverOffset(fRoundingMM); 

        /// <summary>
        /// Creates a shell of a voxel field. The wall thickness is
        /// the size of the offset.
        /// 
        /// If a positive offset is supplied, the wall is outside the
        /// object, i.e. the void inside the shell has the shape and
        /// dimensions of the current object.
        /// 
        /// If the offset is negative, the object's dimensions remain
        /// the same, but a void is created that is created by negatively
        /// offsetting the object.
        /// </summary>
        /// <param name="fOffset"></param>
        /// <returns></returns>
        public Voxels voxShell(float fOffset)
        {
            if (fOffset < 0)
            {
                // Outside remains the same
                return voxBoolSubtract(voxOffset(fOffset));
            }
            
            return voxOffset(fOffset).voxBoolSubtract(this);
        }

        /// <summary>
        /// Creates a shell of a voxel field, by offsetting and subtracting
        /// copies of the field.
        /// One of the offsets can be zero, but if both are zero, an empty voxel
        /// field is the result
        /// </summary>
        /// <param name="fNegOffsetMM">Offset to be used to create the void</param>
        /// <param name="fPosOffsetMM">Offset to be used to create the outer shell</param>
        /// <param name="fSmoothInnerMM">Optional smoothing parameter that allows you to smoothen the internal void</param>
        /// <returns></returns>
        public Voxels voxShell( float fNegOffsetMM, 
                                float fPosOffsetMM, 
                                float fSmoothInnerMM = 0f)
        {
            if (fNegOffsetMM > fPosOffsetMM)
            {
                float fTemp     = fNegOffsetMM;
                fNegOffsetMM    = fPosOffsetMM;
                fPosOffsetMM    = fTemp;
            }

            Voxels voxInner = voxOffset(fNegOffsetMM);
            
            if (fSmoothInnerMM > 0)
                voxInner.voxTripleOffset(fSmoothInnerMM);
            
            Voxels voxOuter = voxOffset(fPosOffsetMM);
            voxOuter.voxBoolSubtract(voxInner);

            return voxOuter;
        }

        /// <summary>
        /// Renders a mesh into the voxel field, combining it with
        /// the existing content
        /// </summary>
        /// <param name="msh">The mesh to render (needs to be a closed surface)</param>
        public void RenderMesh(in Mesh msh)
        {
            if (msh.lib.hThis != lib.hThis)
                throw new PicoGKLibraryMismatchException();

            _RenderMesh(lib.hThis, hThis, msh.hThis);
        }

        /// <summary>
        /// Render an implicit signed distance function into the voxels
        /// overwriting the existing content with the voxels where the implicit
        /// function returns smaller or equal to 0
        /// You will often want to use IntersectImplicit instead
        /// </summary>
        /// <param name="xImp">Implicit object with signed distance function</param>
        /// <param name="oBounds">Bounding box in which to render the implicit</param>
        public void RenderImplicit( in IImplicit xImp,
                                    in BBox3 oBounds)
            => _RenderImplicit(lib.hThis, hThis, in oBounds, xImp.fSignedDistance);

        /// <summary>
        /// Render an implicit signed distance function into the voxels
        /// but using the existing voxels as a mask.
        /// If the voxel field contains a voxel at a given position, the voxel
        /// will be set to true if the signed distance function returns
        /// smaller or equal to 0
        /// and false if the signed distance function returns > 0
        /// So a voxel field, containting a filled sphere, will contain a
        /// Gyroid Sphere, if used with a Gyroid implict
        /// </summary>
        /// <param name="xImp">Implicit object with signed distance function</param>
        public void IntersectImplicit(in IImplicit xImp)
            => _IntersectImplicit(lib.hThis, hThis, xImp.fSignedDistance);


        /// <summary>
        /// Same as IntersectImplicit, but uses a copy of the current voxel field
        /// and returns the result.
        /// </summary>
        /// <param name="xImp">Implicit function to use</param>
        /// <returns></returns>
        public Voxels voxIntersectImplicit(in IImplicit xImp)
        {
            Voxels vox = new(this);
            vox.IntersectImplicit(xImp);
            return vox;
        }

        /// <summary>
        /// Renders a lattice into the voxel field, combining it with
        /// the existing content
        /// </summary>
        /// <param name="lat">The lattice to render</param>
        public void RenderLattice(in Lattice lat)
        {
            if (lat.lib.hThis != lib.hThis)
                throw new PicoGKLibraryMismatchException();

            _RenderLattice(lib.hThis, hThis, lat.hThis);
        }

        /// <summary>
        /// Projects the slices at the start Z position upwards or downwards,
        /// until it reaches the end Z position.
        /// </summary>
        /// <param name="fStartZMM">Start voxel slice in mm</param>
        /// <param name="fEndZMM">End voxel slice in mm</param>
        public void ProjectZSlice(  float fStartZMM,
                                    float fEndZMM)
            => _ProjectZSlice(lib.hThis, hThis, fStartZMM, fEndZMM);

        /// <summary>
        /// Makes a copy of the voxel field and applies
        /// the ProjectZSlice function to the copy.
        /// </summary>
        public Voxels voxProjectZSlice( float fStartZMM,
                                        float fEndZMM)
        {
            Voxels vox = new(this);
            vox.ProjectZSlice(fStartZMM, fEndZMM);
            return vox;    
        }

        /// <summary>
        /// Returns true if the voxel fields contain the same content
        /// </summary>
        /// <param name="voxOther">Voxels to compare to</param>
        /// <returns></returns>
        public bool bIsEqual(in Voxels voxOther)
        {
            if (voxOther.lib.hThis != lib.hThis)
                throw new PicoGKLibraryMismatchException();

            return _bIsEqual(lib.hThis, hThis, voxOther.hThis);
        }

        /// <summary>
        /// This function evaluates the entire voxel field and returns
        /// the volume of all voxels in cubic millimeters and the Bounding Box
        /// in real world coordinates
        /// Note this function is potentially slow, as it needs to traverse the
        /// entire voxel field
        /// </summary>
        /// <param name="fVolumeCubicMM">Cubic MMs of volume filled with voxels</param>
        /// <param name="oBBox">The real world bounding box of the voxels</param>
        public void CalculateProperties(    out float fVolumeCubicMM,
                                            out BBox3 oBBox)
        {
            oBBox           = new();
            fVolumeCubicMM  = 0f;

           _CalculateProperties(    lib.hThis,
                                    hThis,
                                    ref fVolumeCubicMM,
                                    ref oBBox);
        }

        /// <summary>
        /// Calculates the bounding box from an intermediate mesh generated from the voxels
        /// </summary>
        /// <returns>Bounding box of the voxels in real world coordinates</returns>
        public BBox3 oCalculateBoundingBox()
        {
            Mesh msh = new(this);
            return msh.oBoundingBox();  
        }

        /// <summary>
        /// Returns whether the location specified lies inside the solid
        /// domain of the voxel field (are you at or below the surface)
        /// </summary>
        /// <param name="vecTestPoint">Point to evaluate</param>
        /// <returns>True if inside or on the surface, False if outside</returns>
        public bool bIsInside(Vector3 vecTestPoint)
        {
            return _bIsInside(lib.hThis, hThis, vecTestPoint);
        }

        /// <summary>
        /// Returns the normal of the surface found at the specified point.
        /// Use after functions like bClosestPointOnSurface or bRayCastToSurface
        /// </summary>
        /// <param name="vecSurfacePoint">
        /// The point (on the surface of a voxel field, for which to return
        /// the normal
        /// </param>
        /// <returns>The normal vector of the surface at the point</returns>
        public Vector3 vecSurfaceNormal(in Vector3 vecSurfacePoint)
        {
            Vector3 vecNormal = Vector3.Zero;
            _GetSurfaceNormal(  lib.hThis, 
                                hThis, 
                                vecSurfacePoint, 
                                ref vecNormal);
            return vecNormal;
        }

        /// <summary>
        /// Returns the closest point from the search point on the surface
        /// of the voxel field
        /// </summary>
        /// <param name="vecSearch">Search position</param>
        /// <param name="vecSurfacePoint">Point on the surface of the voxel field which 
        /// is closest to the supplied point.</param>
        /// <returns>True if point is found, false if field is empty</returns>
        public bool bClosestPointOnSurface( in  Vector3 vecSearch,
                                            out Vector3 vecSurfacePoint)
        {
            vecSurfacePoint = new();
            return _bClosestPointOnSurface( lib.hThis,
                                            hThis,
                                            in  vecSearch,
                                            ref vecSurfacePoint);
        }

        /// <summary>
        /// Returns the closest point from the search point on the surface
        /// of the voxel field 
        /// </summary>
        /// <param name="vecSearch">Search position</param>
        /// <returns>Point on the surface of the voxel field which is closest
        /// to the supplied point.</returns>
        /// <exception cref="Exception">Throws an exception if no point found, 
        /// which means the voxel field is empty</exception>
        public Vector3 vecClosestPointOnSurface(in Vector3 vecSearch)
        {
            if (!bClosestPointOnSurface(vecSearch, out Vector3 vecSurfacePoint))
            {
                throw new Exception("Empty voxel field used in ClosesPointToSurface");
            }

            return vecSurfacePoint;
        }

        /// <summary>
        /// Casts a ray to the surface of a voxel field and finds
        /// the point on the surface where the ray intersects.
        /// </summary>
        /// <param name="vecSearch">Search point</param>
        /// <param name="vecDirection">Direction to search in</param>
        /// <param name="vecSurfacePoint">Point on the surface</param>
        /// <returns>True, point found. False, no surface intersection found</returns>
        public bool bRayCastToSurface(  in  Vector3 vecSearch,
                                        in  Vector3 vecDirection,
                                        out Vector3 vecSurfacePoint)
        {
            vecSurfacePoint     = new();
            return _bRayCastToSurface(  lib.hThis,
                                        hThis,
                                        in  vecSearch,
                                        in  vecDirection,
                                        ref vecSurfacePoint);
        }

        /// <summary>
        /// Casts a ray to the surface of a voxel field and finds
        /// the point on the surface where the ray intersects.
        /// </summary>
        /// <param name="vecSearch">Search point</param>
        /// <param name="vecDirection">Direction to search in</param>
        /// <returns>Point on surface</returns>
        /// <exception cref="Exception">Throws an exception of no intersection 
        /// with surface found.</exception>
        public Vector3 vecRayCastToSurface( in  Vector3 vecSearch,
                                            in  Vector3 vecDirection)
        {
            if (!bRayCastToSurface( in  vecSearch,
                                    in  vecDirection,
                                    out Vector3 vecSurfacePoint))
            {
                throw new Exception("No intersection with surface in RayCastToSurface");
            }

            return vecSurfacePoint;
        }

        /// <summary>
        /// Returns the dimensions of the voxel field in discrete voxels
        /// </summary>
        /// <param name="nXOrigin">X origin of the voxel field in voxels</param>
        /// <param name="nYOrigin">Y origin of the voxel field in voxels</param>
        /// <param name="nZOrigin">Z origin of the voxel field in voxels</param>
        /// <param name="nXSize">Size in x direction in voxels</param>
        /// <param name="nYSize">Size in y direction in voxels</param>
        /// <param name="nZSize">Size in z direction in voxels</param>
        public void GetVoxelDimensions( out int nXOrigin,
                                        out int nYOrigin,
                                        out int nZOrigin,
                                        out int nXSize,
                                        out int nYSize,
                                        out int nZSize)
        {
            nXOrigin    = 0;
            nYOrigin    = 0;
            nZOrigin    = 0;
            nXSize      = 0;
            nYSize      = 0;
            nZSize      = 0;

            _GetVoxelDimensions(    lib.hThis,
                                    hThis,
                                    ref nXOrigin,
                                    ref nYOrigin,
                                    ref nZOrigin,
                                    ref nXSize,
                                    ref nYSize,
                                    ref nZSize);
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
            int nXOrigin    = 0; // unused in this function
            int nYOrigin    = 0; // unused in this function
            int nZOrigin    = 0; // unused in this function
            nXSize          = 0;
            nYSize          = 0;
            nZSize          = 0;

            _GetVoxelDimensions(    lib.hThis,
                                    hThis,
                                    ref nXOrigin,
                                    ref nYOrigin,
                                    ref nZOrigin,
                                    ref nXSize,
                                    ref nYSize,
                                    ref nZSize);
        }

         /// <summary>
        /// Query the real world origin of a voxel slice, which is also
        /// the origin of the actual voxel field in space
        /// </summary>
        /// <param name="nZSlice">Slice you are looking for</param>
        /// <returns>Real world coordinates of the origin of the slice</returns>
        public Vector3 vecZSliceOrigin(int nZSlice=0)
        {
            GetVoxelDimensions( out int nXOrigin,
                                out int nYOrigin,
                                out int nZOrigin,
                                out _,
                                out _,
                                out _);

            return lib.vecVoxelsToMm(nXOrigin, nYOrigin, nZOrigin + nZSlice);
        }

        /// <summary>
        /// Return the number of slices in this voxel field
        /// </summary>
        /// <returns>Number of slices in the voxel field (equivalent to nZSice)</returns>
        public int nSliceCount()
        {
            GetVoxelDimensions(out _, out _, out int nCount);
            return nCount;
        }

        public enum ESliceMode
        {
            SignedDistance,
            BlackWhite,
            Antialiased
        }

        public enum ESliceAxis
        {
            X,
            Y,
            Z
        }

        /// <summary>
        /// Allocate a grayscale image that can hold a voxel slice
        /// </summary>
        /// <param name="nSliceCount">Number of slices in the specified axis</param>
        /// <param name="eAxis">Axis to use for the slice direction</param>
        /// <returns>Allocated grayscale image to be used in GetVoxelSlice</returns>
        public ImageGrayScale imgAllocateSlice( out int nSliceCount,
                                                ESliceAxis eAxis = ESliceAxis.Z)
        {
            GetVoxelDimensions( out int nXSize, 
                                out int nYSize, 
                                out int nZSize);

            switch (eAxis)
            {
            case ESliceAxis.Z:
                nSliceCount = nZSize;
                return new ImageGrayScale(nXSize, nYSize);

            case ESliceAxis.Y:
                nSliceCount = nYSize;
                return new ImageGrayScale(nXSize, nZSize);
                
            case ESliceAxis.X:
            default:
                nSliceCount = nXSize;
                return new ImageGrayScale(nYSize, nZSize);
            }
        }

        /// <summary>
        /// Returns a slice of the voxel field along the specified axis.
        /// To use it, use GetVoxelDimensions to find out the size of the voxel
        /// field in voxel units. Then allocate a new grayscale image to copy
        /// the data into, and pass it as a reference. Since GetVoxelDimensions
        /// is potentially an "expensive" function, we are putting the burden
        /// on you to allocate an image and don't create it for you. You can
        /// also re-use the image if you want to save an entire image stack
        /// </summary>
        /// <param name="nSlice">Slice to retrieve. 0 is at the origin.</param>
        /// <param name="img">Pre-allocated grayscale image to receive the values</param>
        /// <param name="eMode">Encoding mode of the image, defaults to signed distance, 
        /// which is the native narrow band distance encoded in the float image. You
        /// can also speciy black/white, which encodes the inside of the field as black
        /// pixels, or antialiased, which encodes the outside narrow band in grayscale</param>
        /// <param name="eAxis">Axis to slice along, defaults to Z, but you can also slice
        /// along X and Y</param>
        public void GetVoxelSlice(  in int nSlice,
                                    ref ImageGrayScale img,
                                    ESliceMode eMode = ESliceMode.SignedDistance,
                                    ESliceAxis eAxis = ESliceAxis.Z)
        {
            float fBackground = 0f;
            GCHandle oPinnedArray = GCHandle.Alloc(img.m_afValues, GCHandleType.Pinned);
            try
            {
                IntPtr afBufferPtr = oPinnedArray.AddrOfPinnedObject();

                switch (eAxis)
                {
                case ESliceAxis.Z:
                    _GetZSlice(lib.hThis, hThis, nSlice, afBufferPtr, ref fBackground);
                    break;

                case ESliceAxis.Y:
                    _GetYSlice(lib.hThis, hThis, nSlice, afBufferPtr, ref fBackground);
                    break;

                case ESliceAxis.X:
                default:
                    _GetXSlice(lib.hThis, hThis, nSlice, afBufferPtr, ref fBackground);
                    break;
                }
            }
            finally
            {
                oPinnedArray.Free();
            }

            switch (eMode)
            {
               case ESliceMode.Antialiased:
                    {
                        for (int x=0; x<img.nWidth; x++)
                        {
                            for (int y=0; y<img.nHeight; y++)
                            {
                                float fValue = img.fValue(x, y);
                                if (fValue <= 0)
                                    fValue = 0;
                                else if (fValue > fBackground)
                                    fValue = 1.0f;
                                else
                                    fValue = fValue / fBackground;

                                img.SetValue(x, y, fValue);
                            }
                        }

                        return;
                    }

                case ESliceMode.BlackWhite:
                    {
                        for (int x = 0; x < img.nWidth; x++)
                        {
                            for (int y = 0; y < img.nHeight; y++)
                            {
                                float fValue = img.fValue(x, y);
                                if (fValue <= 0)
                                    fValue = 0;
                                else
                                    fValue = 1.0f;

                                img.SetValue(x, y, fValue);
                            }
                        }
                        return;
                    }

                default:
                    return;
            }
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
        /// <param name="fZSlice">Slice to retrieve. 0 is at the bottom.</param>
        /// <param name="img">Pre-allocated grayscale image to receive the values</param>
        /// <param name="eMode">Encoding mode of the image, defaults to signed distance, 
        /// which is the native narrow band distance encoded in the float image. You
        /// can also speciy black/white, which encodes the inside of the field as black
        /// pixels, or antialiased, which encodes the outside narrow band in grayscale</param>
        public void GetInterpolatedVoxelSlice(  in float fZSlice,
                                                ref ImageGrayScale img,
                                                ESliceMode eMode = ESliceMode.SignedDistance)
        {
            float fBackground = 0f;
            GCHandle oPinnedArray = GCHandle.Alloc(img.m_afValues, GCHandleType.Pinned);
            try
            {
                IntPtr afBufferPtr = oPinnedArray.AddrOfPinnedObject();
                _GetInterpolatedZSlice(lib.hThis, hThis, fZSlice, afBufferPtr, ref fBackground);
            }
            finally
            {
                oPinnedArray.Free();
            }

            switch (eMode)
            {
               case ESliceMode.Antialiased:
                    {
                        for (int x=0; x<img.nWidth; x++)
                        {
                            for (int y=0; y<img.nHeight; y++)
                            {
                                float fValue = img.fValue(x, y);
                                if (fValue <= 0)
                                    fValue = 0;
                                else if (fValue > fBackground)
                                    fValue = 1.0f;
                                else
                                    fValue = fValue / fBackground;

                                img.SetValue(x, y, fValue);
                            }
                        }

                        return;
                    }

                case ESliceMode.BlackWhite:
                    {
                        for (int x = 0; x < img.nWidth; x++)
                        {
                            for (int y = 0; y < img.nHeight; y++)
                            {
                                float fValue = img.fValue(x, y);
                                if (fValue <= 0)
                                    fValue = 0;
                                else
                                    fValue = 1.0f;

                                img.SetValue(x, y, fValue);
                            }
                        }
                        return;
                    }

                default:
                    return;
            }
        }

        public FieldMetadata m_oMetadata;
    }
}