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

namespace PicoGK
{
	public partial class Voxels
	{
		/// <summary>
		/// Create Voxels from a OpenVDB file (.vdb)
		/// This function searches for the first compatible field in the .vdb
		/// file (OpenVDB supports storing many fields of various types in one
		/// file). PicoGK Voxels need an openvdb::GRID_LEVEL_SET field.
		/// Incompatible fields and other compatible fields after the first one
		/// are ignored. For more sophisticated .vdb file handling, use
		/// PicoGK.OpenVdbFile
		/// Note: When you save your own VdbFiles with multiple fields in it
		/// you cannot rely on the fields being in the same order as you saved
		/// them. In other words, if you use PicoGK.OpenVdbFile to save multiple
		/// Voxel fields to a file, loading Voxels with voxFromVdbFile will
		/// load any one, not necessarily the one you saved first.
		/// </summary>
		/// <param name="strFileName">Path and file name of the VDB file</param>
		/// <returns>A voxel field, based on the .vdb data</returns>
		/// <exception cref="FileLoadException">If file is empty or no
		/// compatible field found, an exception is thrown.
		/// </exception>
		public static Voxels voxFromVdbFile(string strFileName)
		{
			OpenVdbFile oFile = new(strFileName);

            if (oFile.nFieldCount() == 0)
			{
				// Nothing in this file
				throw new FileLoadException($"No fields contained in OpenVDB file {strFileName}");
            }

            string strInfo = "Fields found:\n";

			// Find the first voxel field in this file
			for (int n=0; n<oFile.nFieldCount(); n++)
			{
				if (oFile.eFieldType(n) == OpenVdbFile.EFieldType.Voxels)
					return oFile.voxGet(n);

				strInfo += $"- {oFile.strFieldName(n)} ({oFile.strFieldType(n)})\n";
			}

			throw new FileLoadException($"No voxel field (openvdb::GRID_LEVEL_SET) found in VDB file {strFileName}\n{strInfo}");
		}

        /// <summary>
        /// Creates a new .vdb file and saves the voxel field to it
        /// </summary>
        /// <param name="strFileName">Path and filename of the file</param>
        /// <exception Throws an exception if unable to save
        /// </exception>
        public void SaveToVdbFile(string strFileName)
		{
			using OpenVdbFile oFile = new();
			oFile.nAdd(this);
			oFile.SaveToFile(strFileName);
		}
	}
}

