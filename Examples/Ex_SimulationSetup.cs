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
    class OpenVdbSimulationExchange
    {
        /// <summary>
        /// his function comprehensively explains how to deal with the setup
        /// of a fluid simulation using the OpenVDB file format for the exchange
        /// of information.
        ///
        /// The example creates a simple fluid manifold, which consists of
        /// a fluid volume, a vector field which defines the initial flow speed
        /// condition at the inlet, and a voxel field, which defines the actual
        /// geometry of the manifold.
        ///
        /// The fluid volume and the outside boundary do not intersect, and
        /// the vector field at the inlet has the same voxel coordinates and
        /// as the voxels of the surface of the fluid domain.
        /// 
        /// Note, for the exchange of information, we always SI units
        /// https://en.wikipedia.org/wiki/International_System_of_Units
        ///
        /// So the vector field uses m/s as the flow speed.
        /// 
        /// </summary>
        public static void Task()
        {
            Library.Log("We will create a fluid volume of a very simple inverted T-shaped manifold.");
            Library.Log("We are only using PicoGK functions. You will usually use the Shapekernel.");

            Lattice lat = new();

            Library.Log("A horizontal beam of 100mm length and 10mm diameter");
            lat.AddBeam(new Vector3(-50,0,0), new Vector3(50,0,0), 10f, 10f, false);

            Library.Log("A vertical beam of 50mm, connected to the horizontal beam");
            lat.AddBeam(new Vector3(0,0,0), new Vector3(0,0,50), 10f, 10f, false);

            Library.Log("Now we have a simple inverted T which represents where our fluid will be");
            Voxels voxFluid = new(lat);

            Library.Log("Add to the viewer with a 'blue liquid' color");
            Library.oViewer().SetGroupMaterial(0, "11D0E411", 0.1f, 0.1f);
            Library.oViewer().Add(voxFluid);

            Library.Log("Now we will cut off a portion of the liquid volume to define the inlet at the top");

            Library.Log("We will just use a smaller lattice with a 3mm height.");
            Library.Log("We will make it a bit oversize to avoid any overlap problems.");
            Library.Log("It doesn't really matter how you cut off the part that defines your inlet/outlets");
            Library.Log("as long as you can later uniquely identify the voxels that you want your vectors");
            Library.Log("to be associated with.");

            Lattice latInlet = new();
            latInlet.AddBeam(new Vector3(0,0,47), new Vector3(0,0,51), 60, 60, false);
            Voxels voxInlet = new(latInlet);

            Library.Log("We will now intersect with the fluid volume to make sure we only have the voxels actually in that volume");
            voxInlet.BoolIntersect(voxFluid);

            Library.Log("Add inlet area to the viewer with a green transparent color");
            Library.oViewer().SetGroupMaterial(1, "00FF0011", 0.0f, 0.8f);
            Library.oViewer().Add(voxInlet, 1);

            Library.Log("Turn off fluid volume to make inlet/outlet area visible");
            Thread.Sleep(1000);
            Library.oViewer().SetGroupVisible(0, false);

            Library.Log("Now we have the general areas of the inlets");
            Library.Log("We now need a general direction that defines the inlet");

            Vector3 vecInlet = -Vector3.UnitZ;
            Library.Log($"We will use the minus Z direction {vecInlet}");

            // At this point, we have three things that matter.
            //
            // 1. We have the fluid volume as a voxel field
            //
            // 2. We have a voxel slice that defines the general region of
            //    the inlet.
            //
            // 3. We have the direction of the fluid of the inlet.
            //
            // We have a slice which represent the rough inlet region.
            // 
            // Nothing else matters. Note that specifically it doesn't matter
            // how big the inlet voxel region is, as long as it is not outside
            // the fluid domain. We will use only the filtered surface voxels
            // to create our vector field.

            float fFluidSpeed = -4.5f; // m/s

            Library.Log("We will define the fluid speed the inlet (in meters/second).");
            Library.Log($"Since the surface normal is outwards, we use a negative number: {fFluidSpeed}m,/s");
            
            Library.Log("We will now extract all the surface voxels of the inlet voxel field");
            Library.Log("If the surface normal of that voxel is aligned with the minus inlet direction");
            Library.Log("we will add it to the VectorField object");

            // We are using the minus inlet direction, because the surface normal
            // will point outward and we want the normals at the top inlet surface
            // We are multiplying each normal by Vector3.One * fFluidSpeed to scale/flip around

            VectorField oInletField
                = SurfaceNormalFieldExtractor.oExtract( voxInlet,       // voxel field for the surface voxels
                                                        0.5f,           // max distance to the surface in voxels 
                                                        -vecInlet,      // direction filter
                                                        0.0f,           // direction tolerance
                                                        Vector3.One * fFluidSpeed);

            AddVectorFieldToViewer.AddToViewer( Library.oViewer(),
                                                oInletField,
                                                "00FF00",
                                                1,
                                                0.2f,
                                                10);

            Library.Log("Turn off inlet/outlet volumes to show vectors");
            Thread.Sleep(1000);
            Library.oViewer().SetGroupVisible(1, false);

            Library.Log("Now let's create an actual manifold");
            Library.Log("We do this by offsetting the fluid volume");

            Voxels voxManifold = new(voxFluid);
            voxManifold.Offset(3); // 3mm wall thickness

            Library.Log("Cut away the fluid to create a pipe");
            voxManifold.BoolSubtract(voxFluid);

            BBox3 oBox = new(-50,-13,-13,50,13,50);
            voxManifold.BoolIntersect(new Voxels(Utils.mshCreateCube(oBox)));

            Library.oViewer().SetGroupMaterial(20, "AA11", 0.8f, 0.3f);
            Library.oViewer().Add(voxManifold, 20);

            // Create a vector field which has the value 0,0,0 at each position
            // of the fluid volume. This represents the initial flow speed
            VectorField oFlowSpeed = new VectorField(voxFluid, Vector3.Zero);

            // We will now merge the inlet vector field into the flow speed
            // vector field. This will overwrite the 0,0,0 vectors currently
            // in the area of the inlet with the actual flow speed at the inlet.
            //
            // The result is a field which represents the initial condition
            // of the simulation. The inlet has a flow speed of our defined
            // speed, all other flow speed vectors are zero, and will be
            // set as the simulation is executed

            VectorFieldMerge.Merge(oInletField, oFlowSpeed);

            AddVectorFieldToViewer.AddToViewer(
                Library.oViewer(),  // Which viewer
                oFlowSpeed,         // What field
                "EE",               // Color
                1,                  // 1=add every vector, increase for better performance
                0.05f,              // length of arrow
                100);               // Viewer group

            Library.oViewer().SetGroupVisible(10, false);

            // Create a new scalar field, which contains the density of water
            // at all voxels which are in the fluid domain (997 kg/m³)
            ScalarField oFluidDensity = new(voxFluid, 997.0f);

            OpenVdbFile oFile = new();

            voxFluid.m_oMetadata.SetValue("Simulation.Material.Name", "Water");
            voxFluid.m_oMetadata.SetValue("Simulation.Material.Type", "Liquid");
            oFile.nAdd(voxFluid, "Simulation.FluidDomain");

            oFile.nAdd(voxManifold,     "Simulation.SolidDomain");
            oFile.nAdd(oFlowSpeed,      "Simulation.FlowSpeed");
            oFile.nAdd(oFluidDensity,   "Simulation.FluidDensity");

            string strVdbFile = Path.Combine(Utils.strDocumentsFolder(), "Sim.vdb");
            oFile.SaveToFile(strVdbFile);

            OpenVdbFile vdbfileRead = new(strVdbFile);

            Library.Log($"Loaded VdbFile {strVdbFile}");
            Library.Log($"VdbFile contains {vdbfileRead.nFieldCount()} fields");

            for (int nField = 0; nField <vdbfileRead.nFieldCount(); nField++)
            {
                Library.Log($"-  Field {nField} has type {vdbfileRead.strFieldType(nField)} and name '{vdbfileRead.strFieldName(nField)}'");
            }
            
        }
    }
}

