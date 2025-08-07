//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023-2025 by LEAP 71
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


/// The extended constructors provided in this file allow PicoGK objects to be
/// instatiated without providing a Library reference. This enables work inside
/// a typical Library::Go task without passing library references around
/// the global Library object needs to be registered using the
/// Library.RegisterGlobalLibrary function. If you run your task using PicoGK.Go
/// all of this is done for you automatically
/// 

namespace PicoGK
{
    public partial class Voxels
    {
        public Voxels()
        : this(Library.oLibrary())
        {

        }

        public Voxels(  in IImplicit xImplicit,
                    in BBox3 oBounds)
        : this( Library.oLibrary(), 
                xImplicit, 
                oBounds)
        {

        }

        public Voxels(in IBoundedImplicit xImplicit) 
        : this(Library.oLibrary(), xImplicit)
        {

        }

        /// <summary>
        /// Create a voxel field with a sphere inside
        /// </summary>
        /// <param name="vecCenter">Center of the sphere</param>
        /// <param name="fRadius">Radius of the Sphere</param>
        /// <returns></returns>
        public static Voxels voxSphere( Vector3 vecCenter, 
                                        float fRadius)
        {
            Lattice lat = new(Library.oLibrary());
            lat.AddSphere(vecCenter, fRadius);
            return new(lat);
        }

        /// <summary>
        /// Combines all voxel fields in the container and returns the result
        /// </summary>
        /// <param name="avoxList">Container with the voxel fields</param>
        /// <returns>All voxel fields combined</returns>
        public static Voxels voxCombineAll(in IEnumerable<Voxels> avoxList)
        {
            Voxels vox = new(Library.oLibrary());
            vox.BoolAddAll(avoxList);
            return vox;
        }

        public static Voxels voxFromVdbFile(string strFileName)
            => voxFromVdbFile(Library.oLibrary(), strFileName);
    }   

    public partial class Mesh
    {
        public Mesh()
            : this(Library.oLibrary())
        {

        }
    }

    public partial class Lattice
    {
        public Lattice()
            : this(Library.oLibrary())
        {

        }
    }

    public partial class PolyLine
    {
       public PolyLine(ColorFloat clr)
            : this(Library.oLibrary(), clr)
        {

        } 
    }

    public partial class OpenVdbFile
    {
       public OpenVdbFile()
            : this(Library.oLibrary())
        {

        } 

        public OpenVdbFile(string strFileName)
            : this( Library.oLibrary(), 
                    strFileName)
        {
            
        }
    }

    public partial class ScalarField
    {
       public ScalarField()
            : this(Library.oLibrary())
        {

        } 
    }

    public partial class VectorField
    {
       public VectorField()
            : this(Library.oLibrary())
        {
            
        } 
    }
}