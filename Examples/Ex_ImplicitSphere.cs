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

    class ImplicitSphereExample
    {
        // Let's derive a "Sphere" class from the Interface IImplict
        // The main function it needs to implement is "fSignedDistance"
        // This function returns the value for the signed distance field
        // at the specified coordinate
        // This simple sphere is always at the origin to illustrate the
        // simplicity of signed distance functions

        public class ImplicitSphere : IImplicit
        {
            public ImplicitSphere(float fRadius)
            {
                m_fRadius = fRadius;
            }

            public float fSignedDistance(in Vector3 vecPt)
            {
                return float.Sqrt(  vecPt.X * vecPt.X +
                                    vecPt.Y * vecPt.Y +
                                    vecPt.Z * vecPt.Z) - m_fRadius;
            }

            float m_fRadius;
        }

        public static void Task()
        {
            try
            {
                // let's use a transparent red color for group 0 (default)
                Library.oViewer().SetGroupMaterial(0, "EE000055", 0f, 1f);
                
                // Let's instantiate one implicit sphere, with the radius 50mm
                ImplicitSphere oSphere = new ImplicitSphere(50f);

                // Create a new voxel field, which renders the lattice
                // we are passing the bounding box of the sphere, so that
                // we know which area in the voxel field to evaluate

                Voxels vox = new Voxels(    oSphere,
                                            new BBox3(  new Vector3(-55f),
                                                        new Vector3(55f)));


                // Let's show what we got
                Library.oViewer().Add(vox);

                // To visualize how the voxels are stored internally
                // let's get a slice and show how it looks

                // Get the dimensions of the discrete voxel field
                vox.GetVoxelDimensions( out int nXSize,
                                        out int nYSize,
                                        out int nZSize);


                // Create a new grayscale image to store the signed distance
                // field in
                ImageGrayScale imgSDFSlice = new ImageGrayScale(nXSize, nZSize);

                // Get a slice in the middle of the stack
                vox.GetVoxelSlice(nZSize / 2, ref imgSDFSlice);

                // Let's get a fancy "color encoded" SDF, that shows the
                // bands
                ImageColor imgCoded = imgSDFSlice.imgGetColorCodedSDF(6.0f);

                // Save to your log folder for you to inspect
                TgaIo.SaveTga(  Path.Combine(Library.strLogFolder, "SDF.tga"),
                                imgCoded);

                TgaIo.SaveTga(  Path.Combine(Library.strLogFolder, "SDFBW.tga"),
                                imgSDFSlice);
            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

