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
    class VdbToCLI
    {
        /// <summary>
        /// Converts a .VDB file to a .CLI file
        /// This example also demonstrates how to query a VDB file for the
        /// original voxel size
        /// </summary>
        /// <param name="strVdb">VDB File to read</param>
        /// <param name="fLayerHeight">Layer height - a typical layer height is 60 micron (0.06f mm) 
        /// or 30 micron for fine layers (0.03f mm)</param>
        /// <exception cref="Exception">Throws and exception if an error occurs</exception>
        public static void ConvertVdbToCli( string strVdbFile,
                                            float fLayerHeight=0.06f,
                                            bool bStartWithEmptyLayer=false)
        {
            float fVoxelSize = 0;

            {
                // First, let's try to find the voxel size of the VDB file
                using Library lib = new(1);
                
                OpenVdbFile oFile = new OpenVdbFile(strVdbFile);

                if (!oFile.bIsPicoGKCompatible())
                {
                    throw new Exception($"File {strVdbFile} was not created with PicoGK and is missing necessary metadata");
                }

                fVoxelSize = oFile.fPicoGKVoxelSizeMM();
            }

            string strCLIFile = strVdbFile + ".cli";

            {
                // Instantiate PicoGK library with correct voxel size
                using Library lib = new(fVoxelSize);

                // Now, let's load the first voxel field from the VDB file
                // if we find one, we save it as a CLI

                OpenVdbFile oFile = new OpenVdbFile(strVdbFile);

                bool bFound = false;

                for (int n=0; n<oFile.nFieldCount(); n++)
                {
                    Voxels vox;
                    try
                    {
                        vox = oFile.voxGet(n);
                    }

                    catch (Exception)
                    {
                        // continue until a compatible voxel field is found
                        continue;
                    }

                    // Save to CLI file
                    vox.SaveToCliFile(  strCLIFile, 
                                        fLayerHeight, 
                                        bStartWithEmptyLayer ? 
                                            CliIo.EFormat.UseEmptyFirstLayer : 
                                            CliIo.EFormat.FirstLayerWithContent);

                    bFound = true;
                    break;
                }

                if (!bFound)
                    throw new Exception($"No voxels found in VDB file {strVdbFile}");
            }

            // Lastly, let's visualize the CLI in the viewer, and output it to .SVG slices
            // we can use the ShowCLIFile example for that
            {
                ShowCLI oShow = new(strCLIFile);
                Library.Go(1, oShow.Task);
            }
        }
    }
}