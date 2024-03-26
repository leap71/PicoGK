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

    class RayCasting
    {
        public static void Task()
        {
            try
            {
                // Set up viewer materials (transparent green for mesh outline)
                Library.oViewer().SetGroupMaterial(0, "FF0000", 0f, 1f);
                Library.oViewer().SetGroupMaterial(1, "00FF0033", 0f, 1f);

                // Create a mesh from an existing teapot file
                Mesh msh = Mesh.mshFromStlFile(
                    Path.Combine(   Utils.strPicoGKSourceCodeFolder(),
                                    "Examples/Testfiles/Teapot.stl"));

                Library.oViewer().Add(msh, 1);
                
                // Create Voxels from teapot mesh
                Voxels vox = new(msh);

                // Add it to the viewer
                Library.oViewer().Add(vox);

                // Create a box 10mm larger than the teapot's bounds
                BBox3 oBounds = msh.oBoundingBox();
                oBounds.Grow(10);

                // Generate 1000 rays and try to randomly
                // intersect with teapot

                Random rnd = new Random();

                // Add a log entry, so we can benchmark
                Library.Log("Starting Raycasting");

                for (int n=0; n<300; n++)
                {
                    // random point from 0 .. 1 in x/y/z
                    Vector3 vecPos = new(   rnd.NextSingle(),
                                            rnd.NextSingle(),
                                            rnd.NextSingle());


                    // Multiple by size and offset by bounding box origin
                    // to create a random point inside the bounding box
                    vecPos *= oBounds.vecSize();
                    vecPos += oBounds.vecMin;

                    // Create a random direction vector for the ray
                    Vector3 vecDir = Vector3.Normalize(
                                     new(   rnd.NextSingle() - 0.5f,
                                            rnd.NextSingle() - 0.5f,
                                            rnd.NextSingle() - 0.5f));


                    // Create a point in the direction of our ray.
                    // We will replace it with the point on the surface
                    // if the ray hits
                    Vector3 vecSurfacePt = vecPos + vecDir;

                    // The third point will show us the surface normal if the
                    // ray hits
                    Vector3 vecNormalPt = vecSurfacePt;

                    // Default the color to transparent gray (= ray did not hit)
                    ColorFloat clr = new("00AA");

                    if (vox.bRayCastToSurface(vecPos, vecDir, out Vector3 vecHit))
                    {
                        // We found a surface point
                        vecSurfacePt = vecHit;

                        // indicate we have hit by coloring the line in red
                        clr = new("FF0000");
                        vecNormalPt = vecSurfacePt + vox.vecSurfaceNormal(vecSurfacePt) * 5f;
                    }

                    // Show the result
                    PolyLine oPoly = new(clr);
                    oPoly.nAddVertex(vecPos);
                    oPoly.nAddVertex(vecSurfacePt);

                    if (vecSurfacePt != vecNormalPt)
                        oPoly.nAddVertex(vecNormalPt);

                    oPoly.AddArrow(0.5f);
                    Library.oViewer().Add(oPoly);
                }

                // Finish log entry (allows us to benchmark)
                Library.Log("Done Raycasting");
            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

