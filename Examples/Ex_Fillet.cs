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
    ///////////////////////////////////////////////////////////////////////////
    // Below is a static class that implements a single static function
    // that can be called from Library::Go()

    /// <summary>
    /// This function tests the EXPERIMENTAL blurring and averaging functions
    /// introduced in PicoGK 1.5.
    ///
    /// We are still debating whether to keep them in, as they are potentially
    /// not suitable for engineering applications.
    /// 
    /// hey work reasonably well, but are mostly "visual" functions that
    /// require trial and error to get what you want.
    ///
    /// They are quite sensitive to voxel sizes.
    /// 
    /// They also seem to create artifacts at higher size parameters.
    ///
    /// DoubleOffset, TripleOffset are (slower) alternatives.
    /// 
    /// </summary>

    class Filetting
    {
        public static void Task()
        {
            try
            {
                Lattice lat = new();
                lat.AddSphere(Vector3.Zero, 10);
                lat.AddSphere(new Vector3(0,0,18), 12);
                
                Voxels vox = new(lat);

                Voxels voxFilleted = new(vox);
                voxFilleted.IterativeFillet((int) (50 / Library.fVoxelSizeMM));

                Library.oViewer().SetGroupMaterial(1, "0000FFEE", 0.5f, 0.6f);
                Library.oViewer().SetGroupMaterial(2, "FF0000", 0.5f, 0.6f);
                //Library.oViewer().Add(vox,1);
                Library.oViewer().Add(voxFilleted,2);
            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

