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

    class ImplicitsExample
    {
        // Let's derive a "Gyroid" class from the Interface IImplict
        // The main function it needs to implement is "fSignedDistance"
        // This function returns the value for the signed distance field
        // at the specified coordinate

        public class ImplicitGyroid : IImplicit
        {
            public ImplicitGyroid(  float fUnitSize,
                                    float fWallThickness)
            {
                m_fScale = MathF.PI / 0.5f;
                m_fUnitSize = fUnitSize;
                m_fWallThickness = fWallThickness;
            }

            public float fSignedDistance(in Vector3 vecPt)
            {
                // Calculate the normalized coordinates within the gyroid space
                double nx = vecPt.X / m_fUnitSize;
                double ny = vecPt.Y / m_fUnitSize;
                double nz = vecPt.Z / m_fUnitSize;

                // Calculate the gyroid surface equation
                double fDist
                    =  Math.Sin(m_fScale * nx) * Math.Cos(m_fScale * ny) +
                       Math.Sin(m_fScale * ny) * Math.Cos(m_fScale * nz) +
                       Math.Sin(m_fScale * nz) * Math.Cos(m_fScale * nx);

                // Apply thickness to the gyroid surface
                return (float)(Math.Abs(fDist) - 0.5f * m_fWallThickness);
            }

            float m_fUnitSize;
            float m_fScale;
            float m_fWallThickness;
        }

        // Let's also derive a implicit "Lattice" class, that renders a lattice

        class ImplicitLattice : IImplicit
        {
            public ImplicitLattice( Vector3 vecA,
                                    Vector3 vecB,
                                    float fA,
                                    float fB,
                                    bool bRoundCap)
            {
                m_vecA = vecA;
                m_vecB = vecB;
                m_fA = fA;
                m_fB = fB;
                m_bRoundCap = bRoundCap;

                oBB.Include(vecA);
                oBB.Include(vecB);
                oBB.Grow(float.Max(fA, fB));
            }

            public float fSignedDistance(in Vector3 vecP)
            {
                Vector3 vecAB = m_vecB - m_vecA;
                Vector3 vecAP = vecP - m_vecA;
                float t = Vector3.Dot(vecAP, vecAB) / Vector3.Dot(vecAB, vecAB);
                t = Math.Clamp(t, 0f, 1f); // Ensure t is within the line segment

                Vector3 vecNearest = m_vecA + t * vecAB;
                float fDistToLine = Vector3.Distance(vecP, vecNearest);

                if (t <= 0f)
                {
                    // Point is before the line segment
                    return fDistToLine - (m_bRoundCap ? m_fA : 0f);
                }
                else if (t >= 1f)
                {
                    // Point is after the line segment
                    return fDistToLine - (m_bRoundCap ? m_fB : 0f);
                }
                else
                {
                    // Point is inside the line segment
                    float fR = m_fA + t * (m_fB - m_fA);
                    return fDistToLine - fR;
                }
            }

            public BBox3 oBB;

            Vector3 m_vecA;
            Vector3 m_vecB;
            float m_fA;
            float m_fB;
            bool m_bRoundCap;
        };

        public static void Task()
        {
            try
            {
                Library.oViewer().SetGroupMaterial(0, "EE0000", 0f, 1f);

                // Let's instantiate one implicit lattice, and store the
                // parameters which will be used in the signed distance function
                ImplicitLattice oLattice
                    = new ImplicitLattice( new Vector3(0f, 0f, 0f),
                                           new Vector3(0f, 0f, 50f),
                                           5.0f,
                                           10.0f,
                                           true);

                // Create a new voxel field, which renders the lattice
                // we are passing the bounding box of the lattice, so that
                // we know which area in the voxel field to evaluate

                Voxels voxL = new(  oLattice,
                                    oLattice.oBB);

                // Let's show what we got
                Library.oViewer().Add(voxL);

                // Let's also instantiate a gyroid, storing the parameters
                // which will be used in the signed distance function
                ImplicitGyroid oGyroid = new ImplicitGyroid(25f, 1.5f);

                // Let's create a little box 50x50x50mm at position x 150
                // that is filled with a gyroid pattern
                // The first argument is the gyroid object that is queried for
                // the signed distance
                // the second argument is again the bounding box which is
                // evaluated
                Voxels voxG = new(  oGyroid,
                                    new BBox3(   150, 0, 0,
                                                 200, 50, 50));

                // Extract a slice from the voxel field and save it to
                // your log folder

                voxG.GetVoxelDimensions(out int nXSize,
                                        out int nYSize,
                                        out int nZSize);

                ImageGrayScale imgSDFSlice = new ImageGrayScale(nXSize, nZSize);

                voxG.GetVoxelSlice(nZSize / 2, ref imgSDFSlice);

                ImageColor imgCoded = imgSDFSlice.imgGetColorCodedSDF(6.0f);

                TgaIo.SaveTga(
                    Path.Combine(Library.strLogFolder, "SDF.tga"),
                    imgCoded);

                TgaIo.SaveTga(
                    Path.Combine(Library.strLogFolder, "SDFBW.tga"),
                    imgSDFSlice);

                // Let's show it
                Library.oViewer().Add(voxG);

                // Now, for the fun of it, let's create another lattice
                // and fill it with a gyroid
                ImplicitLattice oLattice2
                    = new ImplicitLattice(    new Vector3(50f, 0f, 0f),
                                              new Vector3(50f, 0f, 50f),
                                              10.0f,
                                              15.0f,
                                              true);

                // Again, let's evaluate the lattice signed distance function
                // in the bounding box of the lattice
                Voxels voxI = new Voxels(   oLattice2,
                                            oLattice2.oBB);

                // The voxel field now contains a solid lattice
                // Now let's evaluate the Gyroid signed distance function
                // for every voxel that is already set in the voxel field
                voxI.IntersectImplicit(oGyroid);

                // Now we have a lattice beam that is filled with a gyroid
                // Let's show it
                Library.oViewer().Add(voxI);
            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

