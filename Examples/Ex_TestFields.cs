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

    class TestFields
    {
        public static void Task()
        {
            try
            {
                Library.Log("Testing fundamental field functions");

                Library.Log("Create an empty scalar field and set a value at a point in space");
                ScalarField oScalarField = new();
                oScalarField.SetValue(Vector3.One, 10f);

                Library.Log("Test if the value exists at the point and read back if this is the case");
                
                if (oScalarField.bGetValue(Vector3.One, out float fValue))
                {
                    Library.Log($"Successfully read value back ({fValue})");
                }
                else
                {
                    Library.Log($"Failed to read value back (this should not happen)");
                }

                Library.Log("Remove the value at the specified point");

                oScalarField.RemoveValue(Vector3.One);

                if (oScalarField.bGetValue(Vector3.One, out fValue))
                {
                    Library.Log($"Error value still there: ({fValue})");
                }
                else
                {
                    Library.Log($"Successfully removed the value");
                }

                Library.Log("Create an empty vector field, set a value and read it back");
                VectorField oVectorField = new();

                oVectorField.SetValue(Vector3.One, Vector3.UnitZ);
                if (oVectorField.bGetValue(Vector3.One, out Vector3 vecValue))
                {
                    Library.Log($"Successfully read value back ({vecValue})");
                }
                else
                {
                    Library.Log($"Failed to read value back (this should not happen)");
                }

                oVectorField.RemoveValue(Vector3.One);

                if (oVectorField.bGetValue(Vector3.One, out vecValue))
                {
                    Library.Log($"Error value still there: ({vecValue})");
                }
                else
                {
                    Library.Log($"Successfully removed the value");
                }

                Library.oViewer().SetGroupMaterial(0, "AA33", 0f, 1f);

                Library.Log("Let's create both a scalar signed distance field and a vector gradient field from an existing mesh");
               
                Library.Log("Load mesh from STL file");
                
                // Create a mesh from an existing teapot file
                Mesh msh = Mesh.mshFromStlFile(
                    Path.Combine(   Utils.strPicoGKSourceCodeFolder(),
                                    "Examples/Testfiles/Teapot.stl"));

                Library.Log("Create voxels from mesh");
                Voxels vox = new(msh);
                Library.oViewer().Add(vox, 0);
               
                Library.Log("Create gradient field from mesh");
                VectorField oGradientField  = new(vox);

                Library.Log("Create signed distance scalar field from mesh");
                ScalarField oSDField        = new(vox);

                Library.Log("Calculate voxel field properties");
                vox.CalculateProperties(    out float fVolume,
                                            out BBox3 oBox);

                Library.Log("Grow Bounding Box by 1mm");
                oBox.Grow(1f);
                Vector3 vecSize = oBox.vecSize();

                Library.Log("Calculate step distance between samples");
                float fStep = float.Max(    Library.fVoxelSizeMM * 4,
                                            // to avoid cluttering the view, we use 4 voxels minimum spacing
                                            float.Min(vecSize.X, float.Min(vecSize.Y, vecSize.Z)) / 20f);

                Library.Log($"Starting plot with {fStep}mm grid raster");

                ColorFloat clrMinus = "FF0000";
                ColorFloat clrPlus  = "0000FF";
                ColorFloat clrGrid  = "0011";

                for (float x = 0f; x < vecSize.X; x += fStep)
                {
                    for (float y = 0f; y < vecSize.Y; y += fStep)
                    {
                        for (float z = 0f; z < vecSize.Z; z += fStep)
                        {
                            Vector3 vecPos = oBox.vecMin + new Vector3(x,y,z);
                            Vector3 vecVal = Vector3.Zero;

                            if (oGradientField.bGetValue(vecPos, out vecVal))
                            {
                                if (oSDField.bGetValue(vecPos, out float fSDVal))
                                {
                                    ColorFloat clr;

                                    // Transform to real world values
                                    // the SD value is in voxels
                                    fSDVal *= Library.fVoxelSizeMM;

                                    if (fSDVal < 0) 
                                    {
                                        clr = clrMinus;
                                    }
                                    else
                                    {
                                        clr = clrPlus;
                                    }

                                    // Multiply the vector with the SD value
                                    // Exagerate the arrow by factor of 4:
                                    vecVal *= fSDVal * 4f;

                                    PolyLine oPoly = new(clr);
                                    oPoly.nAddVertex(vecPos);
                                    oPoly.nAddVertex(vecPos + vecVal);
                                    oPoly.AddArrow(fStep / 5);

                                    Library.oViewer().Add(oPoly);
                                }
                                else
                                {
                                    Library.Log("Inconsistent active voxels between SDF and VectorField");
                                }
                            }
                            else
                            {
                                PolyLine oPoly = new(clrGrid);
                                oPoly.nAddVertex(vecPos);
                                oPoly.AddCross(fStep / 15);

                                Library.oViewer().Add(oPoly);
                            }
                        }
                    }
                }
                
                Library.Log($"Done adding stuff to field");
                Library.oViewer().LogStatistics();
            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

