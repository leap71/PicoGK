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

    class BlurringAndAveraging
    {
        public static void Task()
        {
            try
            {
                // Create a mesh from an existing STL file
                Mesh msh = Mesh.mshFromStlFile(
                    Path.Combine(   Utils.strPicoGKSourceCodeFolder(),
                                    "Examples/Testfiles/Teapot.stl"));

                // Add it to the viewer (moving it 20mm to the side)
                Library.oViewer().Add(new Voxels(msh));

                float fStart    = 0.1f;
                float fEnd      = 1.0f;
                float fSteps    = 5;
                float fDiff     = (fEnd - fStart)/fSteps;

                Library.oViewer().SetGroupMaterial(0, "AAAAAA", 0.2f, 0.5f);
                Library.oViewer().SetGroupMaterial(1, "FF0000", 0.2f, 0.5f);
                Library.oViewer().SetGroupMaterial(2, "00FF00", 0.2f, 0.5f);
                Library.oViewer().SetGroupMaterial(3, "0000FF", 0.2f, 0.5f);

                {
                    float fOffset = 0f;
                    for (float f=fStart; f<=fEnd; f+=fDiff)
                    {
                        Voxels vox = new(msh);
                        vox.Gaussian(f);

                        Mesh mshVoxelized = new(vox);
                        Library.oViewer().Add(mshVoxelized.mshCreateTransformed(    Vector3.One,
                                                                                    new Vector3(fOffset, 20, 0)),
                                                                                    1);
                        fOffset += 20f;
                    }
                }

                {
                    float fOffset = 0f;
                    for (float f=fStart; f<=fEnd; f+=fDiff)
                    {
                        Voxels vox = new(msh);
                        vox.Median(f);

                        Mesh mshVoxelized = new(vox);
                        Library.oViewer().Add(mshVoxelized.mshCreateTransformed(    Vector3.One,
                                                                                    new Vector3(fOffset, 40, 0)),
                                                                                    2);
                        fOffset += 20f;
                    }
                }

                {
                    float fOffset = 0f;
                    for (float f=fStart; f<=fEnd; f+=fDiff)
                    {
                        Voxels vox = new(msh);
                        vox.Mean(f);

                        Mesh mshVoxelized = new(vox);
                        Library.oViewer().Add(mshVoxelized.mshCreateTransformed(    Vector3.One,
                                                                                    new Vector3(fOffset, 60, 0)),
                                                                                    3);
                        fOffset += 20f;
                    }
                }
                

            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

