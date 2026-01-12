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

using System.Numerics;

namespace PicoGK
{
    /// <summary>
    /// Helper class to save a voxel field contained in a VDB file to a CLI slice file
    /// </summary>
    public class Vdb2Cli
    {
        /// <summary>
        /// Convert a voxel field to a CLI slice file
        /// </summary>
        /// <param name="strVdbFilePath">Path to the VDB file</param>
        /// <param name="fCliLayerHeight">Layer height in millimeters</param>
        /// <param name="strCliFilePath">Path to the CLI file. If empty, same file/path as VDB with .cli</param>
        /// <param name="strVoxelFieldName">Name of the voxel field, or the first voxel field in the file</param>
        /// <param name="xProgress">Progress reporting interface</param>
        /// <exception cref="Exception">Throws exceptions when file(s) cannot be accessed orcreated, or voxel field cannot be found</exception>
        public static void Convert( string strVdbFilePath,
                                    float fCliLayerHeight,
                                    string strCliFilePath = "",
                                    string strVoxelFieldName = "",
                                    IProgress? xProgress = null)
        {
            if (strCliFilePath == "")
            {
                strCliFilePath = Utils.strStripExtension(strVdbFilePath) + ".cli";
            }

            using Library oLib = OpenVdbFile.libCreateCompatibleLibraryFor(strVdbFilePath);

            using OpenVdbFile oFile = new(oLib, strVdbFilePath);

            Voxels? vox = null;

            if (strVoxelFieldName == "")
            {
                for (int n=0; n<oFile.nFieldCount(); n++)
                {
                    if (oFile.eFieldType(n) == OpenVdbFile.EFieldType.Voxels)
                    {
                        vox = oFile.voxGet(n);
                        break;
                    }
                }
            }
            else
            {
                vox = oFile.voxGet(strVoxelFieldName);
            }

            if (vox == null)
                throw new Exception($"VDB file does not contain any voxel fields: {strVdbFilePath}");
            
            vox.SaveToCliFile(strCliFilePath, fCliLayerHeight, xProgress: xProgress);
        }

    }   
}
