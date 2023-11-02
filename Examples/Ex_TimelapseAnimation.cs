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

                // Set start view point for animation
                Library.oViewer().SetViewAngles(0, 30);

                // Sleep a bit
                Thread.Sleep(2000);

                // Create an action for the animation
                // here, we rotate the viewport from 30º down to -30º

                Animation.IAction xAction1
                    = new AnimViewRotate(   Library.oViewer(),
                                            new Vector2(0,30),
                                            new Vector2(0, -30));

                // Let's create an animation — length is 5 seconds
                // it's executed just once and it eases in and out

                Animation oAnim1
                    = new Animation(    xAction1, 5.0f,
                                        Animation.EType.Once,
                                        Animation.EEasing.EaseOut);

                // let's start a timelapse with 40ms interval (=25fps — 1000ms/40ms = 25 frames per second)
                Library.oViewer().StartTimeLapse(40, Library.strLogFolder);

                // Let's start the animation
                Library.oViewer().AddAnimation(oAnim1);

                Thread.Sleep(5000);

                // Pause the timelapse 
                Library.oViewer().PauseTimeLapse();

                // You could do something lengthy here that is not recorded
                Thread.Sleep(2000);

                Animation.IAction xAction2
                    = new AnimViewRotate(Library.oViewer(),
                                            new Vector2(0, -30),
                                            new Vector2(0, 80));

                Animation oAnim2
                    = new Animation(xAction2, 10.0f,
                                        Animation.EType.Once,
                                        Animation.EEasing.EaseOut);

                // Resume the timelapse 
                //Library.oViewer().ResumeTimeLapse();

                // Let's start the second animation
                Library.oViewer().AddAnimation(oAnim2);

                Thread.Sleep(10000);

                Library.oViewer().StopTimeLapse();
            }

            catch (Exception e)
            {
                Library.Log($"Failed run example: \n{e.Message}"); ;
            }
        }
    }
}

