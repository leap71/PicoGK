﻿//
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
using System.Runtime.InteropServices;
using System.Text;

namespace PicoGK
{
    /// <summary>
    /// OpenVdbFile handles the creation, loading and saving of openvdb .VDB
    /// files. These files are a general container for data generated by the
    /// OpenVDB library. As such, they have many data types and functionality
    /// that PicoGK doesn't support. PicoGK currently allows for the saving
    /// and storing of Voxels in VDB files.
    ///
    /// The following caveats are important:
    ///
    /// 1. Currently the voxel size in mm is not saved to the file.
    /// So voxels saved with global voxel size 1mm and loaded with a global
    /// voxel size of 0.1mm, be a 10th of the original size. There is no
    /// interpolation or resampling applied. One voxel is one voxel.
    ///
    /// 2. If you store more than one field to a .VDB file, you cannot rely on
    /// the first saved field to also be the first to be retrieved. OpenVDB
    /// reshuffles the order of the fields. The easiest way around that is to
    /// retrieve fields by name, if your .vdb file contains multiple
    ///
    /// 3. There are many data types that PicoGK currently doesn't support.
    /// PicoGK.Voxels are based on the OpenVDB type GRID_LEVEL_SET. Other
    /// types, such as GRID_FOG_VOLUME are not supported at the moment.
    ///
    /// 4. If you just want to save/store VDBs from PicoGK, and only have one
    /// field per VDB, you can use the helper functions in VoxelsIo which are
    /// easier to use.
    ///
    /// Workflow for creating .VDB files
    /// - Create an empty OpenVdbFile object
    /// - Add the fields you want to save to it
    /// - Save it to file
    ///
    /// Workflow for loading VDB files
    /// - Load .VDB file from disk into a OpenVdbFile object
    /// - Query the object for the fields stored, retrieving names and types
    /// - Get the Voxels from the field with the right name and type
    /// 
    /// </summary>
    public partial class OpenVdbFile
    {
        /// <summary>
        /// Types of fields in .VDB files
        /// </summary>
        public enum EFieldType
        {
            Unsupported = -1,
            Voxels      = 0,
            ScalarField = 1,
            VectorField = 2
        }

        /// <summary>
        /// Creates an empty .VDB file object, with no fields in it
        /// </summary>
        public OpenVdbFile()
        {
            m_hThis = _hCreate();
            Debug.Assert(m_hThis != IntPtr.Zero);
        }

        /// <summary>
        /// Creates a new .VDB file object by attempting to load it from disk
        /// Note, the order of the fields in the file is arbitrary
        /// </summary>
        /// <param name="strFileName">Path and filename of the .VDB</param>
        /// <exception cref="FileLoadException">
        /// If file cannot be read, exception is thrown.
        /// </exception>
        public OpenVdbFile(string strFileName)
        {
            m_hThis = _hCreateFromFile(strFileName);

            if (m_hThis == IntPtr.Zero)
            {
                throw new FileLoadException("Failed to load VDB file", strFileName);
            }
        }

        /// <summary>
        /// Saves the current object with all of its attached fields to a .VDB
        /// container. Note, the order of the fields inside the file is
        /// not the order in which you added the fields.
        /// </summary>
        /// <param name="strFileName">Path and filename to save to</param>
        /// <exception cref="IOException">
        /// Throws exception if unable to save
        /// </exception>
        public void SaveToFile(string strFileName)
        {
            if (!_bSaveToFile(m_hThis, strFileName))
            {
                throw new IOException($"Failed to save VDB file to {strFileName}");
            }
        }

        /// <summary>
        /// Get the Voxels at the index specified. 0 is the first field.
        /// If the field at the specified position is incompatible with Voxels
        /// an exception is thrown.
        /// </summary>
        /// <param name="nIndex">Index of the field</param>
        /// <returns>Voxels described by the VDB field</returns>
        /// <exception>Throws exception if no Voxels found at index</exception>
        public Voxels voxGet(int nIndex)
        {
            if (nIndex >= nFieldCount())
                throw new ArgumentOutOfRangeException();

            IntPtr hVoxels = _hGetVoxels(m_hThis, nIndex);
            if (hVoxels == IntPtr.Zero)
                throw new Exception($"No voxel field found at index {nIndex}");

            return new Voxels(hVoxels);
        }

        /// <summary>
        /// Gets the Voxels with the field name specified
        /// </summary>
        /// <param name="strName">Name of the field in the .VDB file</param>
        /// <returns>Voxels described by the VDB field</returns>
        /// <exception cref="Exception">Throws exception if not found
        /// </exception>
        public Voxels voxGet(string strName)
        {
            for (int n=0; n<nFieldCount(); n++)
            {
                if (String.Compare(strFieldName(n), strName, true) == 0)
                {
                    return voxGet(n);
                }
            }

            throw new Exception($"No voxel field with name {strName} found");
        }

        /// <summary>
        /// Adds a copy of the specified Voxels to the VdbFile object
        /// </summary>
        /// <param name="vox">Voxels to add</param>
        /// <param name="strFieldName">Field name (if not specified,
        /// autogenerates a unique one
        /// </param>
        /// <returns>Index of the field inside the VdbFile object</returns>
        public int nAdd(Voxels vox, string strFieldName = "")
        {
            if (strFieldName == "")
            {
                // Use a unique name if none specified
                strFieldName = $"PicoGK.Voxels.{nFieldCount()}";
            }

            return _nAddVoxels(m_hThis, strFieldName, vox.m_hThis);
        }

        /// <summary>
        /// Get the ScalarField at the index specified. 0 is the first field.
        /// If the field at the specified position is incompatible with the
        /// field type, an exception is thrown.
        /// </summary>
        /// <param name="nIndex">Index of the field</param>
        /// <returns>ScalarField described by the VDB field</returns>
        /// <exception>Throws exception if no ScalarField found at index</exception>
        public ScalarField oGetScalarField(int nIndex)
        {
            if (nIndex >= nFieldCount())
                throw new ArgumentOutOfRangeException();

            IntPtr hField = _hGetScalarField(m_hThis, nIndex);
            if (hField == IntPtr.Zero)
                throw new Exception($"No scalar field found at index {nIndex}");

            return new ScalarField(hField);
        }

        /// <summary>
        /// Gets the ScalarField with the field name specified
        /// </summary>
        /// <param name="strName">Name of the field in the .VDB file</param>
        /// <returns>ScalarField described by the VDB field</returns>
        /// <exception cref="Exception">Throws exception if not found
        /// </exception>
        public ScalarField oGetScalarField(string strName)
        {
            for (int n=0; n<nFieldCount(); n++)
            {
                if (String.Compare(strFieldName(n), strName, true) == 0)
                {
                    return oGetScalarField(n);
                }
            }

            throw new Exception($"No scalar field with name {strName} found");
        }

        /// <summary>
        /// Adds a copy of the specified Scalar Field to the VdbFile object
        /// </summary>
        /// <param name="oField">Field to add</param>
        /// <param name="strFieldName">Field name (if not specified,
        /// autogenerates a unique one
        /// </param>
        /// <returns>Index of the field inside the VdbFile object</returns>
        public int nAdd(ScalarField oField, string strFieldName = "")
        {
            if (strFieldName == "")
            {
                // Use a unique name if none specified
                strFieldName = $"PicoGK.ScalarField.{nFieldCount()}";
            }

            return _nAddScalarField(m_hThis, strFieldName, oField.m_hThis);
        }

        /// <summary>
        /// Get the VectorField at the index specified. 0 is the first field.
        /// If the field at the specified position is incompatible with the
        /// field type, an exception is thrown.
        /// </summary>
        /// <param name="nIndex">Index of the field</param>
        /// <returns>VectorField described by the VDB field</returns>
        /// <exception>Throws exception if no ScalarField found at index</exception>
        public VectorField oGetVectorField(int nIndex)
        {
            if (nIndex >= nFieldCount())
                throw new ArgumentOutOfRangeException();

            IntPtr hField = _hGetVectorField(m_hThis, nIndex);
            if (hField == IntPtr.Zero)
                throw new Exception($"No vector field found at index {nIndex}");

            return new VectorField(hField);
        }

        /// <summary>
        /// Gets the VectorField with the field name specified
        /// </summary>
        /// <param name="strName">Name of the field in the .VDB file</param>
        /// <returns>VectorField described by the VDB field</returns>
        /// <exception cref="Exception">Throws exception if not found
        /// </exception>
        public VectorField oGetVectorField(string strName)
        {
            for (int n=0; n<nFieldCount(); n++)
            {
                if (String.Compare(strFieldName(n), strName, true) == 0)
                {
                    return oGetVectorField(n);
                }
            }

            throw new Exception($"No vector field with name {strName} found");
        }

        /// <summary>
        /// Adds a copy of the specified VectorField to the VdbFile object
        /// </summary>
        /// <param name="oField">Field to add</param>
        /// <param name="strFieldName">Field name (if not specified,
        /// autogenerates a unique one
        /// </param>
        /// <returns>Index of the field inside the VdbFile object</returns>
        public int nAdd(VectorField oField, string strFieldName = "")
        {
            if (strFieldName == "")
            {
                // Use a unique name if none specified
                strFieldName = $"PicoGK.VectorField.{nFieldCount()}";
            }

            return _nAddVectorField(m_hThis, strFieldName, oField.m_hThis);
        }

        /// <summary>
        /// Number of fields stored in the VdbFile container
        /// </summary>
        /// <returns>The number of fields in the VdbFile</returns>
        public int nFieldCount()
        {
            return _nFieldCount(m_hThis);
        }

        /// <summary>
        /// Returns the name of the field (if specified) at the given field
        /// index
        /// </summary>
        /// <param name="nIndex">Index of the field</param>
        /// <returns>The name as string</returns>
        /// /// <exception cref="Exception">Throws exception if out of range
        /// </exception>
        public string strFieldName(int nIndex)
        {
            if (nIndex >= nFieldCount())
                throw new ArgumentOutOfRangeException();

            StringBuilder oBuilder = new StringBuilder(Library.nStringLength);
            _GetFieldName(m_hThis, nIndex, oBuilder);
            return oBuilder.ToString();
        }

        /// <summary>
        /// Returns the type of the field at the given field index
        /// </summary>
        /// <param name="nIndex">Index of the field</param>
        /// <returns>The type of the field</returns>
        /// /// <exception cref="Exception">Throws exception if out of range
        /// </exception>
        public EFieldType eFieldType(int nIndex)
        {
            if (nIndex >= nFieldCount())
                throw new ArgumentOutOfRangeException();

            return (EFieldType)_nFieldType(m_hThis, nIndex);
        }

        /// <summary>
        /// Returns the field type at the given index as string
        /// </summary>
        /// <param name="nIndex">Index of the field</param>
        /// <returns>A string describing the field type</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the field index is out of range, an exception is thrown
        /// </exception>
        public string strFieldType(int nIndex)
        {
            if (nIndex >= nFieldCount())
                throw new ArgumentOutOfRangeException();

            // We intentionally use the runtime function, so we can catch
            // values not represented by the enum (default:)
            switch (_nFieldType(m_hThis, nIndex))
            {
                case -1:
                    return "Unsupported";

                case 0:
                    return "Voxels";

                 case 1:
                    return "ScalarField";

                 case 2:
                    return "VectorField";

                default:
                    // Incompatible value returned from runtime
                    Debug.Assert(false);
                    return $"Unknown #{_nFieldType(m_hThis, nIndex)}";
            }
        }
    }
}