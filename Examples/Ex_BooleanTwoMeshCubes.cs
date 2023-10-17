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

    class BooleanTwoMeshCubes
    {
        public static void Task()
        {
            try
            {
                // Create a 100mm cube at the origin
                Mesh mshCube1 = Utils.mshCreateCube(new Vector3(100.0f));

                // Create a 50mm cube offset 30mm diagonally
                Mesh mshCube2 = Utils.mshCreateCube(    new Vector3(50.0f),
                                                        new Vector3(30f));

                // Render both objects into voxel fields
                Voxels voxCube1 = new Voxels(mshCube1);
                Voxels voxCube2 = new Voxels(mshCube2);

                // Subtract (cut away), voxCube2 from voxCube1
                voxCube1.BoolSubtract(voxCube2);

                // Make a copy of the resulting object
                Voxels voxShell = new(voxCube1);

                // Offset by 2mm
                voxShell.Offset(2f);

                // Cut out voxCube1 to create a shell
                voxShell.BoolSubtract(voxCube1);

                // Subtract Cube2 again to expose the inside of the shell
                voxShell.BoolSubtract(voxCube2); 

                // Add result to viewer
                Library.oViewer().Add(voxShell);

                voxShell.mshAsMesh().SaveToStlFile(
                    Path.Combine(Library.strLogFolder, "Shell.stl"));
            }

            catch (Exception e)
            {
                Library.Log($"Failed run example: \n{e.Message}"); ;
            }
        }
    }
}
