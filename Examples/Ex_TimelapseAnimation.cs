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
using static PicoGK.Viewer;

namespace PicoGKExamples
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    // Below is a static class that implements a single static function
    // that can be called from Library::Go()

    class TimeLapseAnimation
    {
       public static void Task()
        {
            try
            {
                Lattice lat = new();
                lat.AddBeam(new Vector3(0f), 10f, new Vector3(50f), 15f);

                Voxels vox = new(lat);

                Library.oViewer().Add(vox);

                // let's start a timelapse with 100ms interval =10fps (1000ms/100ms = 10 frames per second)
                Library.oViewer().StartTimeLapse(   100,
                                                    Library.strLogFolder,
                                                    "tl_",
                                                    0,
                                                    true); // paused


                Thread.Sleep(1000);

                foreach (Easing.EEasing eEasing in Enum.GetValues(typeof(Easing.EEasing)))
                {
                    if (!Library.bContinueTask())
                        return; // Make sure we exit, when the viewer is closed

                    // Set start view point for animation
                    Library.oViewer().SetViewAngles(90, 20);
                    Library.oViewer().SetGroupMaterial(0, "AA", 0.0f, 1.0f);
                    Thread.Sleep(1000);

                    Library.oViewer().SetGroupMaterial(0, ColorFloat.clrRandom(), 0.0f, 1.0f);

                    Animation.IAction xAction1
                    = new AnimViewRotate(   Library.oViewer(),
                                            new Vector2(90, 20),
                                            new Vector2(30, -20));

                    // Let's create an animation — length is 5 seconds
                    // it's executed just once and it eases in and out

                    Animation oAnim
                        = new Animation(    xAction1, 5.0f,
                                            Animation.EType.Once,
                                            eEasing);

                    Library.oViewer().ResumeTimeLapse();
                    Library.oViewer().AddAnimation(oAnim);

                    Thread.Sleep(5200);

                    Library.oViewer().PauseTimeLapse();

                    Library.oViewer().SetGroupMaterial(0, "22", 0.0f, 1.0f);
                    Thread.Sleep(1000); // this is not recorded
                }

                Library.oViewer().StopTimeLapse();
                Library.oViewer().Remove(vox);
            }

            catch (Exception e)
            {
                Library.Log($"Failed run example: \n{e.Message}"); ;
            }
        }
    }
}

