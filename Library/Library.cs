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


using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PicoGK
{
    /// <summary>
    /// The Library object encapsulates an instance of a PicoGK library configuration
    /// </summary>
    public partial class Library : IDisposable
    {
        /// <summary>
        /// Create a new Library instance, using the specified voxel size in MM
        /// </summary>
        /// <param name="fVoxelSizeMM">Voxel size in MM</param>
        public Library(float fVoxelSizeMM)
        {
            hThis               = _hCreateInstance(fVoxelSizeMM);
            fVoxelSize          = fVoxelSizeMM;
            m_oTimerMemCheck    = new(_ => MonitorMemory(), null, m_oInterval, m_oInterval);


            // Test a few assumptions
            // Built in data type Vector3 is implicit,
            // so should be compatible with our own
            // structs, but let's be sure

            Vector3     vec3    = new();
            Vector2     vec2    = new();
            Matrix4x4   mat4    = new();
            Coord       xyz     = new(0, 0, 0);
            Triangle    tri     = new(0, 0, 0);
            ColorFloat  clr     = new(0f);
            BBox2       oBB2    = new();
            BBox3       oBB3    = new();

            Debug.Assert(sizeof(bool)           == 1);                  // 8 bit for bool assumed
            Debug.Assert(Marshal.SizeOf(vec3)   == ((32 * 3) / 8));     // 3 x 32 bit float
            Debug.Assert(Marshal.SizeOf(vec2)   == ((32 * 2) / 8));     // 2 x 32 bit float
            Debug.Assert(Marshal.SizeOf(mat4)   == ((32 * 16) / 8));    // 4 x 4 x 32 bit float 
            Debug.Assert(Marshal.SizeOf(xyz)    == ((32 * 3) / 8));     // 3 x 32 bit integer
            Debug.Assert(Marshal.SizeOf(tri)    == ((32 * 3) / 8));     // 3 x 32 bit integer
            Debug.Assert(Marshal.SizeOf(clr)    == ((32 * 4) / 8));     // 4 x 32 bit float
            Debug.Assert(Marshal.SizeOf(oBB2)   == ((32 * 2 * 2) / 8)); // 2 x vec2
            Debug.Assert(Marshal.SizeOf(oBB3)   == ((32 * 3 * 2) / 8)); // 2 x vec3

            // If any of these assert, then something is wrong with the
            // memory layout, and the interface to compatible C libraries
            // will fail - this should never happen, as all these types
            // are well-defined
        }

        /// <summary>
        /// Return the total memory usage of all objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nTotalMemUsage()
        {
            return _nTotalMemUsage(hThis);
        }

        /// <summary>
        /// Returns the total memory usage of all Mesh objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nMeshesMemUsage()
        {
            return _nMeshesMemUsage(hThis);
        }

         /// <summary>
        /// Returns the total memory usage of all Lattice objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nLatticesMemUsage()
        {
            return _nLatticesMemUsage(hThis);
        }

         /// <summary>
        /// Returns the total memory usage of all PolyLine objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nPolyLinesMemUsage()
        {
            return _nPolyLinesMemUsage(hThis);
        }

         /// <summary>
        /// Returns the total memory usage of all Voxels objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nVoxelsMemUsage()
        {
            return _nVoxelsMemUsage(hThis);
        }

         /// <summary>
        /// Returns the total memory usage of all VdbFile objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nVdbFilesMemUsage()
        {
            return _nVdbFilesMemUsage(hThis);
        }

         /// <summary>
        /// Returns the total memory usage of all ScalarField objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nScalarFieldsMemUsage()
        {
            return _nScalarFieldsMemUsage(hThis);
        }

         /// <summary>
        /// Returns the total memory usage of all VectorField objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nVectorFieldsMemUsage()
        {
            return _nVectorFieldsMemUsage(hThis);
        }

         /// <summary>
        /// Returns the total memory usage of all VdbFile metadata objects created with this Library instance
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public long nVdbMetasMemUsage()
        {
            return _nVdbMetasMemUsage(hThis);
        }

        /// <summary>
        /// Returns the number of Mesh objects created with this Library instance
        /// </summary>
        /// <returns>Count of allocated objects</returns>
        public long nMeshesAllocated()
        {
            return _nMeshesAllocated(hThis);
        }

        /// <summary>
        /// Returns the number of Lattice objects created with this Library instance
        /// </summary>
        /// <returns>Count of allocated objects</returns>
        public long nLatticesAllocated()
        {
            return _nLatticesAllocated(hThis);
        }

        /// <summary>
        /// Returns the number of PolyLine objects created with this Library instance
        /// </summary>
        /// <returns>Count of allocated objects</returns>
        public long nPolyLinesAllocated()
        {
            return _nPolyLinesAllocated(hThis);
        }

        /// <summary>
        /// Returns the number of Voxels objects created with this Library instance
        /// </summary>
        /// <returns>Count of allocated objects</returns>
        public long nVoxelsAllocated()
        {
            return _nVoxelsAllocated(hThis);
        }

        /// <summary>
        /// Returns the number of VdbFile objects created with this Library instance
        /// </summary>
        /// <returns>Count of allocated objects</returns>
        public long nVdbFilesAllocated()
        {
            return _nVdbFilesAllocated(hThis);
        }

        /// <summary>
        /// Returns the number of ScalarField objects created with this Library instance
        /// </summary>
        /// <returns>Count of allocated objects</returns>
        public long nScalarFieldsAllocated()
        {
            return _nScalarFieldsAllocated(hThis);
        }

        /// <summary>
        /// Returns the number of VectorField objects created with this Library instance
        /// </summary>
        /// <returns>Count of allocated objects</returns>
        public long nVectorFieldsAllocated()
        {
            return _nVectorFieldsAllocated(hThis);
        }

        /// <summary>
        /// Returns the number of VdbFile metadata objects created with this Library instance
        /// </summary>
        /// <returns>Count of allocated objects</returns>
        public long nVdbMetasAllocated()
        {
            return _nVdbMetasAllocated(hThis);
        }

        /// <summary>
        /// Convert voxel index coordinates to world coordinates in millimeters
        /// </summary>
        /// <param name="x">x coordinate in voxel units</param>
        /// <param name="y">y coordinate in voxel units</param>
        /// <param name="z">z coordinate in voxel units</param>
        /// <returns>3D vector coordinate in millimeters (world units)</returns>
        public Vector3 vecVoxelsToMm(   int x,
                                        int y,
                                        int z)
        {
            Vector3 vecMm = new();
            Vector3 vecVoxels   = new Vector3(x, y, z);
            _VoxelsToMm(    hThis,
                            in vecVoxels,
                            ref vecMm);

            return vecMm;
        }

        /// <summary>
        /// Convert world (millimeter) units to voxel units
        /// </summary>
        /// <param name="vecMm">3D coordinate in world (millimeter) space</param>
        /// <param name="x">x coordinate in voxel units</param>
        /// <param name="y">y coordinate in voxel units</param>
        /// <param name="z">z coordinate in voxel units</param>
         public void MmToVoxels(    Vector3 vecMm,
                                    out int x,
                                    out int y,
                                    out int z)
        {
            Vector3 vecResult   = Vector3.Zero;

            _VoxelsToMm(    hThis,
                            vecMm,
                            ref vecResult);

            x = (int) (vecResult.X + 0.5f);
            y = (int) (vecResult.Y + 0.5f);
            z = (int) (vecResult.Z + 0.5f);
        }

        /// <summary>
        /// Voxel size in millimeters
        /// </summary>
        public readonly float fVoxelSize;

        /// <summary>
        /// Cleanup of library
        /// </summary>
        ~Library()
        {
            Dispose(false);
        }

        /// <summary>
        /// The Library implements the Dispose pattern, so you can use it with `using`
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal dispose function
        /// </summary>
        /// <param name="bDisposing">True if called from explict Dispose</param>
        protected virtual void Dispose(bool bDisposing)
        {   
            if (m_bDisposed)
            {
                return;
            }

            Console.WriteLine("Disposing Library");

            if (bDisposing)
            {
                m_oTimerMemCheck?.Dispose();
                _DestroyInstance(hThis);
            }

            Console.WriteLine("Done Disposing Library");
            m_bDisposed = true;
        }

        bool m_bDisposed = false;

        internal readonly LibHandle hThis;
        internal readonly Timer     m_oTimerMemCheck;
        static readonly TimeSpan    m_oInterval = TimeSpan.FromSeconds(10);

        long m_nUsedMemory = 0;

        void MonitorMemory()
        {
            // Monitor Library's memory use over time
            // and communicate to Garbage Collector
            // Without this, the Garbage Collector hardly
            // ever runs, because it is not aware of the
            // potentially gigabytes of memory allocated
            // by the PicoGK runtime (the C# objects are all tiny)

            long nNew = nTotalMemUsage();
            
            long nDiff = nNew - m_nUsedMemory;

            if (nDiff > 0)
            {
                GC.AddMemoryPressure(nDiff);
            }   
            else if (nDiff < 0)
            {
                GC.RemoveMemoryPressure(-nDiff);
            }

            m_nUsedMemory = nNew;
        }
    }
}