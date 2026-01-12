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

namespace PicoGK.Diagnostics
{
    public static class TestCliOutput
    {
        /// <summary>
        /// Test function, generates a unique voxel object and tests vectorization and CLI output
        /// </summary>
        public static void Run()
        {
            Lattice lat = new();

            lat.AddBeam(Vector3.Zero, 5, Vector3.One * 50, .1f);
            lat.AddBeam(Vector3.Zero, 5, Vector3.UnitX * 20, 1f);
            lat.AddBeam(Vector3.Zero, 5, Vector3.UnitY * 30, 2f);
            lat.AddBeam(Vector3.Zero, 5, Vector3.UnitZ * 30, 3f);

            Library.oViewer().SetBackgroundColor("#3ccd9f");
            
            Voxels vox = new(lat);

            Library.oViewer().SetBackgroundColor("#c13ccd");
            Library.oViewer().Add(vox);

            using LogProgress oLP = new(Library.xLog(), "Vectorize");
            PolySliceStack oStack = vox.oVectorize(xProgress: oLP);

            //oStack.AddToViewer(Library.oLibrary(), Library.oViewer());

            {
                using Mesh msh = new(vox);
                msh.SaveToStlFile(Path.Combine(Utils.strDocumentsFolder(), "Object.stl"));
            }
            
            ImageGrayScale img = vox.imgAllocateSlice(out int nSliceCount);
            vox.GetVoxelSlice(5, ref img, Voxels.ESliceMode.Antialiased);

            img.SaveJpg(Path.Combine(Utils.strDocumentsFolder(), "Slice.jpg"));
            img.SaveTga(Path.Combine(Utils.strDocumentsFolder(), "Slice.tga"));
            img.SavePng(Path.Combine(Utils.strDocumentsFolder(), "Slice.png"));

            string strCLI = Path.Combine(Utils.strDocumentsFolder(), "Object.cli");

            {
                using LogProgress oProgress = new(Library.xLog(), "Save to CLI file", 0.1f);
                vox.SaveToCliFile(strCLI, xProgress: oProgress);
            }            

            CliIo.Result oResult = CliIo.oSlicesFromCliFile(strCLI);

            PolySliceStack oCliStack = oResult.oSlices;
            oCliStack.AddToViewer(Library.oLibrary(), Library.oViewer(), new("#ff0000"), new("#00BB00"));

            string strVDB = Path.Combine(Utils.strDocumentsFolder(), "ObjectVDB.vdb");

            vox.SaveToVdbFile(strVDB);

            {
                using LogProgress oProg = new(Library.xLog(), "VDB to CLI");
                Vdb2Cli.Convert(strVDB, 0.01f, xProgress: oProg);
            }
        }
    }
}