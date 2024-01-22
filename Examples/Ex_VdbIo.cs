using PicoGK;

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
            // after you add a voxel field, any changes made to it are not
            // reflected when you save it

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

            // Now lets save the Voxels to a new file
            voxReadSimple.SaveToVdbFile(Path.Combine(   Library.strLogFolder,
                                                        "Simple.vdb"));

            // This file now contains exactly one field,
            // and uses an auto-generated field name
        }
    }
}

