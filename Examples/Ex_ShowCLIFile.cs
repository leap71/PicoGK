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
    class ShowCLI
    {
        public ShowCLI(string strCLIFile)
        {
            m_strCLIFile = strCLIFile;
        }

        public void Task()
        {
            // Load all CLI slices
            CliIo.Result oResult = CliIo.oSlicesFromCliFile(m_strCLIFile);

            // Add all slices to the viewer
            oResult.oSlices.AddToViewer(Library.oViewer());

            // Write out all slice as SVGs
            // Create a subdirectory, to not spam the CLI file folder

            string strDir       = Path.GetDirectoryName(m_strCLIFile) ?? Library.strLogFolder;
            string strName      = Path.GetFileNameWithoutExtension(m_strCLIFile);
            string strSubDir    = Path.Combine(strDir, strName + "_SVGs");
            Directory.CreateDirectory(strSubDir);

            // Iterate throug all slices and save as SVGs
            for (int n=0; n<oResult.oSlices.nCount(); n++)
            {
                PolySlice oSlice = oResult.oSlices.oSliceAt(n);
                oSlice.SaveToSvgFile(Path.Combine(strSubDir, strName + $"_{n:00000}.svg"), true);
            }
        }

        string m_strCLIFile;
    }   
}