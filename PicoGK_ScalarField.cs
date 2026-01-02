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

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PicoGK
{
    public interface ITraverseScalarField
    {
        public abstract void InformActiveValue( in Vector3  vecPosition,
                                                float       fValue);
    }

    public partial class ScalarField : IImplicit
    {
        /// <summary>
        /// Create a voxels object from an existing handle
        /// (for internal use)
        /// </summary>
        internal ScalarField(IntPtr hField)
        {
            m_hThis = hField;
            Debug.Assert(m_hThis != IntPtr.Zero);
            Debug.Assert(_bIsValid(m_hThis));

            m_oMetadata = new(FieldMetadata._hFromScalarField(m_hThis));
            m_oMetadata._SetValue("PicoGK.Class", "ScalarField");
        }

        /// <summary>
        /// Default constructor, builds a new empty field
        /// </summary>
        public ScalarField()
            : this(_hCreate())
        {}

        /// <summary>
        /// Copy constructor, create a duplicate
        /// of the supplied field
        /// </summary>
        /// <param name="oSource">Source to copy from</param>
        public ScalarField(in ScalarField oSource)
            : this(_hCreateCopy(oSource.m_hThis))
        {}

        /// <summary>
        /// Creates a scalar field from an existing voxel field
        /// setting the voxels inside the object to the specified value
        /// of the voxels
        /// </summary>
        /// <param name="oVoxels">Voxels to create SDF from</param>
        /// <param name="fValue">Value to set the scalar field to</param>"
        /// <param name="fSdThreshold">The threshold of the signed distance field
        /// to be used for the definition of "inside" - usually 0.5 is a good
        /// value - the surface is at exactly 0 and a value of
        /// 1.0 means you are 1 voxel outside from the surface.</param>
        public ScalarField(Voxels oVoxels)
            : this(_hCreateFromVoxels(oVoxels.m_hThis))
        { }

        public ScalarField( Voxels oVoxels,
                            float fValue,
                            float fSdThreshold = 0.5f)
            : this(_hBuildFromVoxels(oVoxels.m_hThis, fValue, fSdThreshold))
        {}

        /// <summary>
        /// Sets the value at the specified position in mm
        /// When you set a value, the position gets "activated"
        /// When no value is set, the position doesn't contain
        /// a value, and bGetValue returns false
        /// </summary>
        /// <param name="vecPosition">Position in mm</param>
        /// <param name="fValue">Value</param>
        public void SetValue(   Vector3 vecPosition,
                                float   fValue)
        {
            _SetValue(m_hThis, vecPosition, fValue);
        }

        /// <summary>
        /// Get the value at the specified position
        /// If the specified position doesn't contain a value
        /// the function returns false.
        /// </summary>
        /// <param name="vecPosition">Position in mm</param>
        /// <param name="fValue">Value at position</param>
        /// <returns>
        /// false:  the specified position doesn't contain a value
        /// true:   the specified position contains a value
        /// </returns>
        public bool bGetValue(  Vector3 vecPosition,
                                out float fValue)
        {
            fValue = 0.0f;
            return (_bGetValue(m_hThis, vecPosition, ref fValue));
        }

        /// <summary>
        /// Removes the value at the specified position
        /// </summary>
        /// <param name="vecPosition">Position of the value in space</param>
        public void RemoveValue(Vector3 vecPosition)
        {
            _RemoveValue(m_hThis, vecPosition);
        }

        /// <summary>
        /// Returns the dimensions of the field in discrete voxels
        /// </summary>
        /// <param name="nXOrigin">X origin of the field in voxels</param>
        /// <param name="nYOrigin">Y origin of the field in voxels</param>
        /// <param name="nZOrigin">Z origin of the field in voxels</param>
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

            _GetVoxelDimensions(    m_hThis,
                                    ref nXOrigin,
                                    ref nYOrigin,
                                    ref nZOrigin,
                                    ref nXSize,
                                    ref nYSize,
                                    ref nZSize);
        }

        /// <summary>
        /// Returns the dimensions of the field in discrete voxels
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

            _GetVoxelDimensions(    m_hThis,
                                    ref nXOrigin,
                                    ref nYOrigin,
                                    ref nZOrigin,
                                    ref nXSize,
                                    ref nYSize,
                                    ref nZSize);
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

        /// <summary>
        /// Visit each active value in the vector field and call the
        /// InformActiveValue methot of the ITraverseScalarField interface
        /// </summary>
        /// <param name="xTraverse">The interface containing the callback</param>
        public void TraverseActive(ITraverseScalarField xTraverse)
        {
            _TraverseActive(m_hThis, xTraverse.InformActiveValue);
        }

        /// <summary>
        /// Return the scalar value at the specified position as
        /// as signed distance value. This assumes you stored an signed
        /// distance field in the scalar field (for example by constructing it
        /// from a voxel field)
        /// </summary>
        /// <param name="vecPosition">Position to sample</param>
        /// <returns>
        /// Distance to the surface - negative values are inside the object
        /// positive values are outside the object, values of 0 are exactly
        /// on the surface
        /// </returns>
        public float fSignedDistance(in Vector3 vecPosition)
        {
            bGetValue(vecPosition, out float fValue);
            return fValue * Library.fVoxelSizeMM;
        }

        /// <summary>
        /// Returns the bounding box of all active voxels in mm coordinates
        /// </summary>
        /// <returns>Bounding box of all active voxels</returns>
        public BBox3 oBoundingBox()
        {
            GetVoxelDimensions( out int iXOrigin,
                                out int iYOrigin,
                                out int iZOrigin,
                                out int nXSize,
                                out int nYSize,
                                out int nZSize);

            return new( Library.vecVoxelsToMm(iXOrigin, iYOrigin, iZOrigin),
                        Library.vecVoxelsToMm(  iXOrigin + nXSize,
                                                iYOrigin + nYSize,
                                                iZOrigin + nZSize));
        }

        public FieldMetadata m_oMetadata;
    }
}