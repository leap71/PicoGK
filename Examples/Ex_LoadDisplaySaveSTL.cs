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
using System.Diagnostics;
using System.Numerics;

namespace PicoGKExamples
{
    ///////////////////////////////////////////////////////////////////////////
    // Below is a static class that implements a single static function
    // that can be called from Library::Go()

    class LoadDisplaySaveSTL
    {
        public static void Task()
        {
            try
            {
                // Create a mesh from an existing STL file
                Mesh msh = Mesh.mshFromStlFile(
                    Path.Combine(   Utils.strPicoGKSourceCodeFolder(),
                                    "Examples/Testfiles/Teapot.stl"));

                // Add it to the viewer
                Library.oViewer().Add(msh);

                // Save it 
                msh.SaveToStlFile(
                    Path.Combine(   Library.strLogFolder,
                                    "Saved.stl"));

                // Create a voxel field from it
                Voxels vox = new Voxels(msh);

                // Create a mesh from the voxels again
                // (it now has a new topology, based on the voxels)
                Mesh mshFromVox = new Mesh(vox);

                // Let's save that mesh as well, so we can compare
                mshFromVox.SaveToStlFile(
                    Path.Combine(   Library.strLogFolder,
                                    "SavedFromVox.stl"));

                Library.oViewer().SetGroupMaterial(0, "22", 0.05f, 0.8f);

                Thread.Sleep(2000);

                // Remove the previous object and show voxels instead
                Library.oViewer().RemoveAllObjects();
                Library.oViewer().Add(vox);

                Thread.Sleep(2000);

                Library.oViewer().SetGroupMaterial(0, "AA", 0.5f, 0.9f);

                // Remove the previous object and show mesh from voxels instead
                Library.oViewer().RemoveAllObjects();
                Library.oViewer().Add(mshFromVox);
            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

