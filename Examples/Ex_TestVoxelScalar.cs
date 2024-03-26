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

namespace PicoGKExamples
{
    ///////////////////////////////////////////////////////////////////////////
    // Below is a static class that implements a single static function
    // that can be called from Library::Go()

    class TestVoxelScalar
    {
        public static void Task()
        {
            try
            {
                Library.Log("Testing conversions of voxels to scalar and back");

                Library.Log("Load mesh from STL file");
                
                // Create a mesh from an existing teapot file
                Mesh msh = Mesh.mshFromStlFile(
                    Path.Combine(   Utils.strPicoGKSourceCodeFolder(),
                                    "Examples/Testfiles/Teapot.stl"));

                Library.Log("Create voxels from mesh");
                Voxels vox = new(msh);

                vox.mshAsMesh().SaveToStlFile(Path.Combine(Library.strLogFolder, "Voxelized.stl"));

                Library.Log("Create scalar signed distance field from voxels");
                Library.Log("The field will contain the signed distances in voxel measurements");
                ScalarField oField = new(vox);

                oField.GetVoxelDimensions(out int nXS, out int nYS, out int nZS);

                int nZSlice = nZS * 2 / 3;

                Library.Log("Visualize the signed distances in the scalar field and save");
                ImageColor imgSDF = SdfVisualizer.imgEncodeFromSdf(oField, 3f, nZSlice);
                TgaIo.SaveTga(Path.Combine(Library.strLogFolder, "SDFBefore.tga"), imgSDF);

                Library.Log("Lets create a new voxel field from the scalar field containing the original signed distances");
                Library.Log("This uses the scalar field as an implicit function and returns the values of each voxel");
                Voxels voxAfter = new(oField);

                Library.Log("Create another scalar field from the resulting voxel field and save");
                Library.Log("The fields must be identical");
                ScalarField oFieldAfter = new(voxAfter);
                ImageColor imgSDFAfter = SdfVisualizer.imgEncodeFromSdf(oFieldAfter, 3f, nZSlice);
                TgaIo.SaveTga(Path.Combine(Library.strLogFolder, "SDFAfter.tga"), imgSDFAfter);

                Library.Log("Save the resulting voxels as STL");
                voxAfter.mshAsMesh().SaveToStlFile(Path.Combine(Library.strLogFolder, "ObjectAfter.stl"));

                Library.oViewer().Add(voxAfter);
            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

