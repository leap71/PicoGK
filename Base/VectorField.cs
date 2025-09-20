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
    /// <summary>
    /// An interface to allow traversal of all active values in a VectorField
    /// </summary>
    public interface ITraverseVectorField
    {
        /// <summary>
        /// Called for every active value in the VectorField object
        /// </summary>
        /// <param name="vecPosition">Position in the VectorField</param>
        /// <param name="vecValue">Value at position</param>
        public abstract void InformActiveValue( in Vector3 vecPosition,
                                                in Vector3 vecValue);
    }

    /// <summary>
    /// A Field of 3D floating point vectors
    /// </summary>
    public partial class VectorField
    {
        /// <summary>
        /// Create a VectorField object from an existing handle
        /// (for internal use)
        /// </summary>
        internal VectorField(   Library libSet,
                                VectorFieldHandle hField)
        {
            lib     = libSet;
            hThis   = hField;

            if (!_bIsValid(lib.hThis, hThis))
                throw new PicoGKAllocException();

            m_oMetadata = new(  lib, 
                                FieldMetadata._hFromVectorField(    lib.hThis, 
                                                                    hThis));

            m_oMetadata._SetValue("PicoGK.Class", "VectorField");
        }

        /// <summary>
        /// Default constructor, builds a new empty field
        /// </summary>
        public VectorField(Library libSet)
            : this( libSet, 
                    _hCreate(libSet.hThis))
        {
            if (!_bIsValid(lib.hThis, hThis))
                throw new PicoGKAllocException();
        }

        /// <summary>
        /// Copy constructor, create a duplicate
        /// of the supplied field
        /// </summary>
        /// <param name="oSource">Source to copy from</param>
        public VectorField(in VectorField oSource)
            : this(  oSource.lib, 
                    _hCreateCopy(   oSource.lib.hThis, 
                                    oSource.hThis))
        {
            if (!_bIsValid(lib.hThis, hThis))
                throw new PicoGKAllocException();
        }

        /// <summary>
        /// Creates a gradient field from an existing voxel field
        /// </summary>
        /// <param name="vox">Voxels to create gradients from</param>
        public VectorField(Voxels vox)
            : this( vox.lib, 
                    _hCreateFromVoxels( vox.lib.hThis, 
                                        vox.hThis))
        {
            if (!_bIsValid(lib.hThis, hThis))
                throw new PicoGKAllocException();
        }

        /// <summary>
        /// Creates a vector field from an existing voxel field
        /// setting the value of each voxel to the specified value
        /// </summary>
        /// <param name="vox">Voxel field that defines which values to set</param>
        /// <param name="vecValue">Value to set in the vector field</param>
        /// <param name="fSdThreshold">The threshold of the signed distance field
        /// to be used for the definition of "inside" - usually 0.5 is a good
        /// value - the surface is at exactly 0 and a value of
        /// 1.0 means you are 1 voxel outside from the surface.</param>
        public VectorField( Voxels vox,
                            Vector3 vecValue,
                            float fSdThreshold = 0.5f)
            : this( vox.lib, 
                    _hBuildFromVoxels(  vox.lib.hThis, 
                                        vox.hThis, 
                                        vecValue, 
                                        fSdThreshold))
        {
            if (!_bIsValid(lib.hThis, hThis))
                throw new PicoGKAllocException();
        }

        /// <summary>
        /// Sets the value at the specified position in mm
        /// When you set a value, the position gets "activated"
        /// When no value is set, the position doesn't contain
        /// a value, and bGetValue returns false
        /// </summary>
        /// <param name="vecPosition">Position in mm</param>
        /// <param name="vecValue">Value</param>
        public void SetValue(   Vector3 vecPosition,
                                Vector3 vecValue)
        {
            _SetValue(  lib.hThis, 
                        hThis, 
                        vecPosition, 
                        vecValue);
        }

        /// <summary>
        /// Get the value at the specified position
        /// If the specified position doesn't contain a value
        /// the function returns false.
        /// </summary>
        /// <param name="vecPosition">Position in mm</param>
        /// <param name="vecValue">Value at position</param>
        /// <returns>
        /// false:  the specified position doesn't contain a value
        /// true:   the specified position contains a value
        /// </returns>
        public bool bGetValue(  Vector3 vecPosition,
                                out Vector3 vecValue)
        {
            vecValue = Vector3.Zero;
            return _bGetValue(  lib.hThis, 
                                hThis, 
                                vecPosition, 
                                ref vecValue);
        }

        /// <summary>
        /// Removes the value at the specified position
        /// </summary>
        /// <param name="vecPosition">Position of the value in space</param>
        public void RemoveValue(Vector3 vecPosition)
        {
            _RemoveValue(   lib.hThis, 
                            hThis, 
                            vecPosition);
        }

        /// <summary>
        /// Visit each active value in the vector field and call the
        /// InformActiveValue methot of the ITraverseVectorField interface
        /// </summary>
        /// <param name="xTraverse">The interface containing the callback</param>
        public void TraverseActive(ITraverseVectorField xTraverse)
        {
            _TraverseActive(    lib.hThis, 
                                hThis, 
                                xTraverse.InformActiveValue);
        }

        /// <summary>
        /// VectorField metadata
        /// </summary>
        public FieldMetadata m_oMetadata;
    }
}