//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023-2026 by LEAP 71
// https://leap71.com
//
// Computational Engineering will profoundly change our physical world in the
// years ahead. Thank you for being part of the journey.
//
// We have developed this library to be used widely, for both commercial and
// non-commercial projects alike. Therefore, we have released it under a 
// permissive open-source license.
//
// The foundation of PicoGK is a thin layer on top of the powerful open-source
// OpenVDB project, which in turn uses many other Free and Open Source Software
// libraries. We are grateful to be able to stand on the shoulders of giants.
//
// LEAP 71 licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with the
// License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, THE SOFTWARE IS
// PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// See the License for the specific language governing permissions and
// limitations under the License.   
//

using System.Numerics;


// The extended constructors provided in this file allow PicoGK objects to be
// instatiated without providing a Library reference. This enables work inside
// a typical Library::Go task without passing library references around
// the global Library object needs to be registered using the
// Library.RegisterGlobalLibrary function. If you run your task using PicoGK.Go
// all of this is done for you automatically
// 

namespace PicoGK
{
    public partial class Voxels
    {
        /// <summary>
        /// Create a new empty voxels object, using the global library instance
        /// </summary>
        public Voxels()
        : this(Library.oLibrary())
        {

        }

        /// <summary>
        /// Create a new Voxels object using the global library instance, 
        /// rendering the implicit within the specified bounds
        /// </summary>
        /// <param name="xImplicit">Implicit to render</param>
        /// <param name="oBounds">Bounding box to evaluate the implicit in</param>
        public Voxels(  in IImplicit xImplicit,
                        in BBox3 oBounds)
        : this( Library.oLibrary(), 
                xImplicit, 
                oBounds)
        {

        }

        /// <summary>
        /// Create a new Voxels object using the global library instance, 
        /// rendering the specified bounded implicit 
        /// </summary>
        /// <param name="xImplicit">Bounded implicit to render</param>
        public Voxels(in IBoundedImplicit xImplicit) 
        : this(Library.oLibrary(), xImplicit)
        {

        }

        /// <summary>
        /// Create a new Voxels object using the global library instance, 
        /// rendering a sphere with the specified center and radius
        /// </summary>
        /// <param name="vecCenter">Center of the sphere</param>
        /// <param name="fRadius">Radius of the Sphere</param>
        /// <returns></returns>
        public static Voxels voxSphere( Vector3 vecCenter, 
                                        float fRadius)
        {
            return voxSphere(Library.oLibrary(), vecCenter, fRadius);
        }

        /// <summary>
        /// Returns a lattice beam with hemispherical ends
        /// internally uses an optimized internal function, so is faster
        /// than the general lattice functions
        /// </summary>
        /// <param name="vec1">Start point</param>
        /// <param name="fRadius1">Radius at start</param>
        /// <param name="vec2">End point</param>
        /// <param name="fRadius2">Radius at end</param>
        /// <returns>Voxels object</returns>
        public static Voxels voxLatticeBeam(    Vector3 vec1,
                                                float fRadius1,
                                                Vector3 vec2,
                                                float fRadius2)
        {
            return voxLatticeBeam(  Library.oLibrary(),
                                    vec1,
                                    fRadius1,
                                    vec2,
                                    fRadius2);
        }

        /// <summary>
        /// Creates a shelled (hollow) Voxels object
        /// from a mesh. The mesh object is offset in all
        /// directions by the radius. The mesh doesn't have
        /// to be closed (it could consist of just one triangle)
        /// </summary>
        /// <param name="msh">Mesh object</param>
        /// <param name="fRadius">Offset radius (in all directions)</param>
        /// <returns>Voxels object</returns>
        public static Voxels voxMeshShell(  Mesh msh,
                                            float fRadius)
        {
            return voxMeshShell(    Library.oLibrary(),
                                    msh,
                                    fRadius);
        }

        /// <summary>
        /// Create a new Voxels object using the global library instance, 
        /// and adds all voxel fields in the container, returning the result
        /// </summary>
        /// <param name="avoxList">Container with the voxel fields</param>
        /// <returns>All voxel fields combined</returns>
        public static Voxels voxCombineAll(in IEnumerable<Voxels> avoxList)
        {
            Voxels vox = new(Library.oLibrary());
            vox.BoolAddAll(avoxList);
            return vox;
        }

        /// <summary>
        /// Create Voxels from a OpenVDB file (.vdb) using the global library instance.
        /// This function searches for the first compatible field in the .vdb
        /// file (OpenVDB supports storing many fields of various types in one
        /// file). PicoGK Voxels need an openvdb::GRID_LEVEL_SET field.
        /// Incompatible fields and other compatible fields after the first one
        /// are ignored. For more sophisticated .vdb file handling, use
        /// PicoGK.OpenVdbFile
        /// Note: When you save your own VdbFiles with multiple fields in it
        /// you cannot rely on the fields being in the same order as you saved
        /// them. In other words, if you use PicoGK.OpenVdbFile to save multiple
        /// Voxel fields to a file, loading Voxels with voxFromVdbFile will
        /// load any one, not necessarily the one you saved first.
        /// </summary>
        /// <param name="strFileName">Path and file name of the VDB file</param>
        /// <returns>A voxel field, based on the .vdb data</returns>
        /// <exception cref="FileLoadException">If file is empty or no
        /// compatible field found, an exception is thrown.
        /// </exception>
        public static Voxels voxFromVdbFile(string strFileName)
            => voxFromVdbFile(Library.oLibrary(), strFileName);
    }   

    public partial class Mesh
    {
        /// <summary>
        /// Creates a new empty Mesh, using the global library instance
        /// </summary>
        public Mesh()
            : this(Library.oLibrary())
        {

        }
    }

    public partial class Lattice
    {
        /// <summary>
        /// Creates a new empty Lattice, using the global library instance
        /// </summary>
        public Lattice()
            : this(Library.oLibrary())
        {

        }
    }

    
    public partial class PolyLine
    {
        /// <summary>
        /// Creates a new empty PolyLine, using the global library instance
        /// </summary>
        public PolyLine(ColorFloat clr)
            : this(Library.oLibrary(), clr)
        {

        } 
    }

    public partial class OpenVdbFile
    {
        /// <summary>
        /// Create an empty openvdb file object
        /// </summary>
       public OpenVdbFile()
            : this(Library.oLibrary())
        {

        } 

        /// <summary>
        /// Create an OpenVDB file object by loading
        /// an existing file
        /// </summary>
        /// <param name="strFileName"></param>
        public OpenVdbFile(string strFileName)
            : this( Library.oLibrary(), 
                    strFileName)
        {
            
        }
    }

    public partial class ScalarField
    {
        /// <summary>
        /// Create an empty scalar field object
        /// </summary>
       public ScalarField()
            : this(Library.oLibrary())
        {

        } 
    }

    public partial class VectorField
    {
        /// <summary>
        /// Create an empty VectorField object
        /// </summary>
       public VectorField()
            : this(Library.oLibrary())
        {
            
        } 
    }
}