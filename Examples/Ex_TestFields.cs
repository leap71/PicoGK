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
using System.Runtime.ConstrainedExecution;

namespace PicoGKExamples
{
    ///////////////////////////////////////////////////////////////////////////
    // Below is a static class that implements a single static function
    // that can be called from Library::Go()

    class TestFields
    {
        public static void Task()
        {
            try
            {
                ScalarField oScalarField = new();

                oScalarField.SetValue(Vector3.One, 10f);
                if (oScalarField.bGetValue(Vector3.One, out float fValue))
                {
                    Library.Log($"Successfully read value back ({fValue})");
                }
                else
                {
                    Library.Log($"Failed to read value back.");
                }

                VectorField oVectorField = new();

                oVectorField.SetValue(Vector3.One, Vector3.UnitZ);
                if (oVectorField.bGetValue(Vector3.One, out Vector3 vecValue))
                {
                    Library.Log($"Successfully read value back ({vecValue})");
                }
                else
                {
                    Library.Log($"Failed to read value back.");
                }

                // Set up viewer materials (transparent green for mesh outline)
                Library.oViewer().SetGroupMaterial(0, "FF0000", 0f, 1f);
                Library.oViewer().SetGroupMaterial(1, "00FF0033", 0f, 1f);

                // Create a mesh from an existing teapot file
                Mesh msh = Mesh.mshFromStlFile(
                    Path.Combine(   Utils.strPicoGKSourceCodeFolder(),
                                    "Examples/Testfiles/Teapot.stl"));

                Lattice lat = new();
                lat.AddSphere(Vector3.Zero, 1.3f);
                lat.AddSphere(Vector3.One, 0.6f);

                // Create Voxels from teapot mesh
                Voxels vox = new(lat);
                Library.oViewer().Add(vox, 1);

                // Create a gradient and signed distance field from voxels
                VectorField oGradientField  = new(vox);
                ScalarField oSDField        = new(vox);

                Library.Log("Calculate properties");
                vox.CalculateProperties(out float fVolume, out BBox3 oBox);

                Library.Log("Grow Bounding Box by .5mm");
                oBox.Grow(.2f);

                Vector3 vecSize = oBox.vecSize();
                float fStep = float.Min(vecSize.X, float.Min(vecSize.Y, vecSize.Z)) / 20f;

                Library.Log($"Starting plot with {fStep}mm grid raster");

                for (float x = 0f; x < vecSize.X; x += fStep)
                {
                    for (float y = 0f; y < vecSize.Y; y += fStep)
                    {
                        for (float z = 0f; z < vecSize.Z; z += fStep)
                        {
                            Vector3 vecPos = oBox.vecMin + new Vector3(x,y,z);
                            Vector3 vecVal = Vector3.Zero;

                            ColorFloat clrMinus = "AA0000";
                            ColorFloat clrPlus  = "0000CC";

                            if (oGradientField.bGetValue(vecPos, out vecVal))
                            {
                                if (oSDField.bGetValue(vecPos, out float fSDVal))
                                {
                                    vecVal = Vector3.Normalize(vecVal);
                                    fSDVal = float.Abs(fSDVal / 3f);

                                    ColorFloat clr;

                                    if (fSDVal < 0)
                                    {
                                        clr = clrMinus;
                                        fSDVal = -fSDVal;
                                    }
                                    else
                                    {
                                        clr = clrPlus;
                                    }

                                    vecVal *= fSDVal;
                                    PolyLine oPoly = new("AA");
                                    oPoly.nAddVertex(vecPos);
                                    oPoly.nAddVertex(vecPos + vecVal);
                                    oPoly.AddArrow(0.1f);

                                    Library.oViewer().Add(oPoly);
                                }
                                else
                                {
                                    Library.Log("Inconsistency between SDF and VectorField");
                                }
                            }
                            else
                            {
                                PolyLine oPoly = new("AA");
                                oPoly.nAddVertex(vecPos);
                                oPoly.nAddVertex(vecPos + Vector3.UnitZ * .1f);

                                Library.oViewer().Add(oPoly);
                            }
                        }
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

