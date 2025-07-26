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


using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PicoGK
{
    public partial class Library : IDisposable
    {
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

        public long nTotalMemUsage()
        {
            return _nTotalMemUsage(hThis);
        }

        public long nMeshesMemUsage()
        {
            return _nMeshesMemUsage(hThis);
        }

        public long nLatticesMemUsage()
        {
            return _nLatticesMemUsage(hThis);
        }

        public long nPolyLinesMemUsage()
        {
            return _nPolyLinesMemUsage(hThis);
        }

        public long nVoxelsMemUsage()
        {
            return _nVoxelsMemUsage(hThis);
        }

        public long nVdbFilesMemUsage()
        {
            return _nVdbFilesMemUsage(hThis);
        }

        public long nScalarFieldsMemUsage()
        {
            return _nScalarFieldsMemUsage(hThis);
        }

        public long nVectorFieldsMemUsage()
        {
            return _nVectorFieldsMemUsage(hThis);
        }

        public long nVdbMetasMemUsage()
        {
            return _nVdbMetasMemUsage(hThis);
        }

        public long nMeshesAllocated()
        {
            return _nMeshesAllocated(hThis);
        }

        public long nLatticesAllocated()
        {
            return _nLatticesAllocated(hThis);
        }

        public long nPolyLinesAllocated()
        {
            return _nPolyLinesAllocated(hThis);
        }

        public long nVoxelsAllocated()
        {
            return _nVoxelsAllocated(hThis);
        }

        public long nVdbFilesAllocated()
        {
            return _nVdbFilesAllocated(hThis);
        }

        public long nScalarFieldsAllocated()
        {
            return _nScalarFieldsAllocated(hThis);
        }

        public long nVectorFieldsAllocated()
        {
            return _nVectorFieldsAllocated(hThis);
        }

        public long nVdbMetasAllocated()
        {
            return _nVdbMetasAllocated(hThis);
        }

        public Vector3 vecVoxelsToMm(   int x,
                                        int y,
                                        int z)
        {
            Vector3 vecMm = new();
            Vector3 vecVoxels   = new Vector3(x, y, z);
            _VoxelsToMm(    hThis,
                            vecVoxels,
                            ref vecMm);

            return vecMm;
        }

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

        public readonly float fVoxelSize;

        ~Library()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

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
            Console.WriteLine("Monitor Memory");
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

            Console.WriteLine("Monitor Memory - Done");
        }
    }
}