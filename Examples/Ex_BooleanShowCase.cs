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

    class BooleanShowCase
    {
        public class Sphere : IImplicit
        {
            public Sphere(  float fRadius,
                            Vector3 vecCenter)
            {
                m_fRadius   = fRadius;
                m_vecC      = vecCenter;
                oBB = new BBox3(    vecCenter - new Vector3(fRadius),
                                    vecCenter + new Vector3(fRadius));
            }

            public float fSignedDistance(in Vector3 vecSample)
            {
                Vector3 vecPt = vecSample - m_vecC;
                // Move sample point to origin by subtracting center

                return float.Sqrt(  vecPt.X * vecPt.X +
                                    vecPt.Y * vecPt.Y +
                                    vecPt.Z * vecPt.Z) - m_fRadius;
            }

            public readonly BBox3   oBB;
            float   m_fRadius;
            Vector3 m_vecC;
        }

        public static void Task()
        {
            try
            {
                Library.oViewer().SetGroupMaterial(0, "EE", 0f, 1f);

                Library.oViewer().SetGroupMaterial(1, "FF000033", 0f, 1f);
                // Transparent Red

                Library.oViewer().SetGroupMaterial(2, "00FF0033", 0f, 1f);
                // Transparent Green

                Sphere oSphere1A = new Sphere(20f, new Vector3(-5f, 0, 0));
                Sphere oSphere1B = new Sphere(20f, new Vector3(5f, 0, 0));

                // Boolean Add showcase

                Voxels vox1 = new Voxels(oSphere1A, oSphere1A.oBB);
                vox1.BoolAdd(new Voxels(oSphere1B, oSphere1B.oBB));
                Library.oViewer().Add(vox1);

                Sphere oSphere2A = new Sphere(20f, new Vector3(-5f + 90, 0, 0));
                Sphere oSphere2B = new Sphere(20f, new Vector3(5f + 90, 0, 0));

                // Boolean Subtract showcase

                Voxels vox2A    = new Voxels(oSphere2A, oSphere2A.oBB);
                Voxels vox2B    = new Voxels(oSphere2B, oSphere2B.oBB);

                vox2A.BoolSubtract(vox2B);

                Library.oViewer().Add(vox2A);
                Library.oViewer().Add(vox2B, 1);

                // Boolean Intersect showcase

                Sphere oSphere3A = new Sphere(20f, new Vector3(-5f + 180, 0, 0));
                Sphere oSphere3B = new Sphere(20f, new Vector3(5f + 180, 0, 0));

                Voxels vox3A    = new Voxels(oSphere3A, oSphere3A.oBB);
                Voxels vox3B    = new Voxels(oSphere3B, oSphere3B.oBB);
                Voxels vox3     = new Voxels(vox3A);

                vox3.BoolIntersect(vox3B);

                Library.oViewer().Add(vox3);
                Library.oViewer().Add(vox3A, 2);
                Library.oViewer().Add(vox3B, 2);

                Voxels vox = new Voxels();

                vox.BoolAdd(vox3);
                vox.BoolAdd(vox2A);
                vox.BoolAdd(vox1);

                Mesh msh = new Mesh(vox);
                msh.SaveToStlFile(  Path.Combine(Library.strLogFolder,
                                    "Booleans.stl"));
            }

            catch (Exception e)
            {
                Library.Log($"Failed run example: \n{e.Message}"); ;
            }
        }
    }
}

