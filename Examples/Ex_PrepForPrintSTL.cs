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

namespace PicoGKExamples
{
    ///////////////////////////////////////////////////////////////////////////
    // Below is a static class that implements a single static function
    // that can be called from Library::Go()

    class PrepForPrint
    {
        public static void Task()
        {
            try
            {
                Library.Log($"This example prepares an STL for printing");
                Library.Log($"It retopologizes the object by loading it into a voxel field");
                Library.Log($"Make sure you choose a reasonable voxel size that represents the capabilities of your printer");
                Library.Log($"We are using voxel size {Library.fVoxelSizeMM}mm - you can change it in Program.cs");
                Library.Log($"Also, it makes sure the bottom of the object is flat, by projecting a few layers downwards and cutting the bottom flat");

                float fBaseHeight = 1.0f; // mm

                Library.Log($"We are projecting the last {fBaseHeight}mm of the bottom of the object downwards");
                Library.Log($"To allow you to inspect the bottom layer, we output the slice as PNG.");
                Library.Log($"If you see any large gray areas, you should increase the base height");

                string strSTL = Path.Combine(   Utils.strPicoGKSourceCodeFolder(),
                                                "Examples/Testfiles/Teapot.stl");

                Library.Log($"We are loading the STL file {strSTL}");

                string strFile = Path.GetFileNameWithoutExtension(strSTL);

                Mesh msh = Mesh.mshFromStlFile(strSTL);

                Library.oViewer().Add(msh);

                Library.Log($"We are now creating a voxel field from the mesh");
                Voxels vox = new Voxels(msh);

                Library.oViewer().RemoveAllObjects();
                Library.oViewer().SetGroupMaterial(0, "AABBCC", 0.2f, 0.5f);
                Library.oViewer().Add(vox);

                Library.Log($"To make sure the object sits tightly on the floor");
                Library.Log($"we project the last millimeters of the object downwards");
                Library.Log($"to below the object, and then cut it off cleanly");
                Library.Log($"with a boolean operation.");

                Library.Log($"Calculate the original bounding box");

                vox.CalculateProperties(    out float fVolume,
                                            out BBox3 oBox);

                Library.Log($"Project the last {fBaseHeight}mm downwards to 5mm below the bounding box");

                vox.ProjectZSlice(oBox.vecMin.Z + fBaseHeight, oBox.vecMin.Z-5f);

                Library.Log($"Cut off at bounding box min, to get a clean voxel field.");

                Library.Log($"First calculate the bounding box of the new (longer) voxel field.");

                vox.CalculateProperties(    out  fVolume,
                                            out BBox3 oBoxCut);

                Library.Log($"Old voxel field bounds are {oBox}");
                Library.Log($"New voxel field bounds are {oBoxCut}");

                Library.Log($"We are growing the new bounds by a bit.");

                oBoxCut.Grow(5);

                Library.Log($"And now we set the top of the new box to the bottom of the old one");
                oBoxCut.vecMax.Z = oBox.vecMin.Z;

                Library.Log($"Old voxel field bounds are {oBox}");
                Library.Log($"Box to cut off is {oBoxCut}");

                Library.Log($"Now we are creating a new mesh, and cutting it off from the existing voxels");

                Mesh mshCut = Utils.mshCreateCube(oBoxCut);
                vox.BoolSubtract(new Voxels(mshCut));

                string strTga = Path.Combine(Library.strLogFolder, strFile + ".tga");

                Library.Log($"Lets get the dimensions and output the bottom slice to {strTga}");

                {
                    vox.GetVoxelDimensions( out int nXSize,
                                            out int nYSize,
                                            out int nZSize);

                    Library.Log($"Voxel field dimensions are {nXSize} x {nYSize} x {nZSize}");

                    ImageGrayScale img = new(nXSize, nYSize);

                    Library.Log($"Let's find the first slice that contains active voxels");
                    Library.Log($"{nZSize} slices available");
                    Library.Log($"Normally the bottom 3 slices are empty, because our narrow band signed distance field internally has a distance of 3 voxels");

                    for (int Z=0; Z<=nZSize; Z++)
                    {
                        vox.GetVoxelSlice(Z, ref img, Voxels.ESliceMode.Antialiased);

                        if (img.bContainsActivePixels())
                        {
                            Library.Log($"Slice {Z} contains voxels");
                            break;
                        }
                        else
                        {
                            Library.Log($"Slice {Z} is empty");
                        }    
                    }

                    Library.Log($"Saving slice to {strTga}");
                    TgaIo.SaveTga(strTga, img);
                }

                Library.Log($"Now lets create a new mesh, add it to the viewer");

                Mesh mshFromVox = new Mesh(vox);

                Library.oViewer().RemoveAllObjects();
                Library.oViewer().SetGroupMaterial(0, "AA", 0.9f, 0.2f);
                Library.oViewer().Add(mshFromVox);

                Library.oViewer().SetGroupMaterial(1, "FF000022", 0, 1);
                Library.oViewer().Add(mshCut, 1);

                string strNewSTL = Path.Combine(Library.strLogFolder, strFile + "_Printable.stl");

                Library.Log($"Finally we are saving the new STL to {strNewSTL}");
                mshFromVox.SaveToStlFile(strNewSTL);

                // for the fun of it, let's also save it to the industry standard CLI data format
                // with a layer height of 60 micron, which is a typical value.

                vox.SaveToCliFile(Path.Combine(Library.strLogFolder, strFile + "_Printable.cli"), 0.06f);

                Thread.Sleep(1000);
                Library.oViewer().Remove(mshCut);

                Library.Log($"And we are done.");
            }

            catch (Exception e)
            {
                Library.Log($"Failed to run example: \n{e.Message}"); ;
            }
        }
    }
}

