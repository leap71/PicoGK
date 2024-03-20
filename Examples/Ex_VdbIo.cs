//
// SPDX-License-Identifier: CC0-1.0
//
// This example code file is released to the public under Creative Commons CC0.
// See https://creativecommons.org/publicdomain/zero/1.0/legalcode
//
// To the extent possible under law, LEAP 71 has waived all copyright and
// related or neighboring rights to this PicoGK example code file.
//
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using PicoGK;
using System.Numerics;

namespace PicoGKExamples
{
    class OpenVdbExample
    {
        /// <summary>
        /// his function comprehensively explains how to deal with OpenVDB
        /// (.vdb) files. These files can contain many different fields and
        /// each field be of a number of types.
        ///
        /// PicoGK Voxels are stored in GRID_LEVEL_SET data structures
        /// PicoGK doesn't support any other data types at the moment.
        ///
        /// If you want to make sure you deal with all of these cases
        /// this detailed function is for you
        ///
        /// If you have control over the files and just want to load/save
        /// voxel data in PicoGK, you can use the helper functions
        /// implemented in PicoGK_VoxelsIo.cs
        ///
        /// Basically you do:
        ///
        /// vox.SaveToVdbFile("Hello.vdb")
        ///
        /// and
        ///
        /// Voxels vox = Voxels.voxFromVdbFile("Hello.vdb")
        ///
        /// Check out the end of the function below, for an example
        ///
        /// </summary>
        public static void Task()
        {
            // Create a mesh from an existing STL file
            Mesh msh = Mesh.mshFromStlFile(
                Path.Combine(   Utils.strPicoGKSourceCodeFolder(),
                                "Examples/Testfiles/Teapot.stl"));

            // Create Voxels from the mesh
            Voxels vox = new Voxels(msh);

            // Create an empty VDB file object
            OpenVdbFile oFileCreated = new OpenVdbFile();

            // Add the voxels to the VDB file object
            // You can add multiple fields to an object
            // The name is optional
            oFileCreated.nAdd(vox, "Teapot");

            // Let's add another field, Vdb supports multiple
            oFileCreated.nAdd(vox, "Another One");

            // Let's add a third field
            oFileCreated.nAdd(vox, "Three");

            // Let's add a fourth field with an auto generated name
            oFileCreated.nAdd(vox);

            // Note, internally copies are made of the voxel fields, so
            // after you add a voxel field, any changes made to it after adding
            // are not reflected in the saved file

            VectorField oVectorField = new(vox);
            oFileCreated.nAdd(oVectorField, "Vector");

            ScalarField oScalarField = new(vox);
            oFileCreated.nAdd(oScalarField, "Scalar");

            // Filename to use
            string strVdbFileName = Path.Combine(   Library.strLogFolder,
                                                    "Teapot.vdb");

            Library.Log($"In memory VdbFile object contains {oFileCreated.nFieldCount()} fields");

            for (int nField = 0; nField < oFileCreated.nFieldCount(); nField++)
            {
                Library.Log($"-  Field {nField} has type {oFileCreated.strFieldType(nField)} and name '{oFileCreated.strFieldName(nField)}'");
            }

            // Save the VdbFile object to an actual file on disk
            oFileCreated.SaveToFile(strVdbFileName);

            // Load the saved VdbFile from disk
            // If this fails, an exception is thrown
            OpenVdbFile oFileLoad = new OpenVdbFile(strVdbFileName);

            Library.Log($"Loaded VdbFile {strVdbFileName}");
            Library.Log($"VdbFile contains {oFileLoad.nFieldCount()} fields");

            for (int nField = 0; nField <oFileLoad.nFieldCount(); nField++)
            {
                Library.Log($"-  Field {nField} has type {oFileLoad.strFieldType(nField)} and name '{oFileLoad.strFieldName(nField)}'");
            }

            VectorField oReadVectorField = oFileLoad.oGetVectorField("Vector");
            Library.Log($"VectorField metadata after reading {oReadVectorField.m_oMetadata}");

            ScalarField oReadScalarField = oFileLoad.oGetScalarField("Scalar");
            Library.Log($"ScalarField metadata after reading {oReadScalarField.m_oMetadata}");

            // Extract field number with the name Teapot (case insensitive)
            // .vdb files support several fields of various types, which
            // are unsupported in PicoGK.
            // Also be aware that the order of which these fields are
            // written to the file may often not be the same order that we
            // added it to the object, so don't rely on the third added
            // field to be the third field in the file
            // The safest way to get exactly what you want is to search by
            // name.
            
            Voxels voxRead = oFileLoad.voxGet("Teapot");

            // Show what we loaded
            Library.oViewer().Add(voxRead);

            // Lastly, let's demonstrate the simple helper functions
            // which you can use when you have full control over
            // the origin VDB files (no surprises)

            // Create a Voxel field from the first compatible field in a VDB
            // file. If there are multiple fields, all others are ignored
            // If no compatible field is found, an exception is thrown
            Voxels voxReadSimple = Voxels.voxFromVdbFile(strVdbFileName);

            // Let's deal with field meta data

            voxReadSimple.m_oMetadata.SetValue("StringValue", "Hello PicoGK");

            if (!voxReadSimple.m_oMetadata.bGetValueAt("StringValue", out string strValue))
            {
                Library.Log($"Did not find the metadata value previously saved");
            }
            else
            {
                Library.Log($"Found metadata value {strValue}");
            }

            voxReadSimple.m_oMetadata.SetValue("FloatValue", 12345.67f);

            if (!voxReadSimple.m_oMetadata.bGetValueAt("FloatValue", out float fValue))
            {
                Library.Log($"Did not find the metadata value previously saved");
            }
            else
            {
                Library.Log($"Found metadata value {fValue}");
            }

            voxReadSimple.m_oMetadata.SetValue("VectorValue", new Vector3(1,2,3));

            if (!voxReadSimple.m_oMetadata.bGetValueAt("VectorValue", out Vector3 vecValue))
            {
                Library.Log($"Did not find the metadata value previously saved");
            }
            else
            {
                Library.Log($"Found metadata value {vecValue}");
            }

            Library.Log($"Metadata state: {voxReadSimple.m_oMetadata}");

            Library.Log($"Removing data item 'StringValue'");
            voxReadSimple.m_oMetadata.RemoveValue("StringValue");

            Library.Log($"After removal: {voxReadSimple.m_oMetadata}");

            Library.Log($"Now lets save the Voxels to a new file");
            string strSimple = Path.Combine(Library.strLogFolder, "Simple.vdb");
            voxReadSimple.SaveToVdbFile(strSimple);

            Library.Log($"This file now contains exactly one field,and uses an auto-generated field name");

            Voxels voxReadAgain = Voxels.voxFromVdbFile(strSimple);
            Library.Log($"After reading: {voxReadAgain.m_oMetadata}");

            try
            {
                Library.Log($"Try setting a metadata item that is internal");
                Library.Log($"this will result in an exception, because we guard against this");

                voxReadSimple.m_oMetadata.SetValue("file_compression", "Hello");

                Library.Log($"Error - you should not be able to set this metadata");
            }

            catch (Exception e)
            {
                 Library.Log($"Correctly caught exception, as you are not allowed to set this value");
                 Library.Log($"{e.Message}");
            }
        }
    }
}

