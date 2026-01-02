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

namespace PicoGK
{
    /// <summary>
    /// Visualizes the result of a voxel filed cut along an axis slice.
    /// Useful for showing the inside of a voxel field.
    /// You can set up two slice planes along the X,Y,Z axes. The class 
    /// uses two SliceViz objects to show the contents of the signed distance
    /// field, and then uses a boolean operation to cut the object along these
    /// slices. It then shows the result in the viewer.
    /// </summary>
    public class VoxCutViz : IDisposable
    {
        /// <summary>
        /// Initializes a new VoxCutViz object with the specified
        /// Viewer and VoxelField.
        /// To use the object, encapsulate it into using VoxCutViz oViz = new (...)
        /// this will make the visualization disappear, once it goes out of scope.
        /// </summary>
        /// <param name="oViewer">Viewer to use to visualize</param>
        /// <param name="vox">Voxel field to slice and cut</param>
        /// <param name="nViewerGroup">Viewer group to use for the sliced object</param>
        /// <param name="eAxis">Axis along which to slice</param>
        public VoxCutViz(   Viewer oViewer,
                            Voxels vox,
                            int nViewerGroup = 0,
                            Voxels.ESliceAxis eAxis = Voxels.ESliceAxis.Z)
        {
            m_oViewer   = oViewer;
            m_vox       = vox;
            m_nGroup    = nViewerGroup;
            m_eAxis     = eAxis;

            m_oViz1     = new(oViewer, vox, eAxis);
            m_oViz2     = new(oViewer, vox, eAxis);

            m_oBounds   = vox.oCalculateBoundingBox();

            m_cts = new CancellationTokenSource();
            m_evChanged = new AutoResetEvent(false);

            m_oTask = Task.Factory.StartNew(
                () => CutterTask(m_cts.Token),
                m_cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            // Initialize with full range
            Cut(0, nSliceCount - 1);
        }

        /// <summary>
        /// Number of slices in the voxel field
        /// </summary>
        public int nSliceCount => m_oViz1.nSliceCount;

        /// <summary>
        /// Cut the voxel field along the two normalized values (0 is the first slice 1 is the last)
        /// </summary>
        /// <param name="fNormalizedPos1">First slice position (0..1)</param>
        /// <param name="fNormalizedPos2">Second slice position (0..1)</param>
        public void Cut(    float fNormalizedPos1 = 0.0f,
                            float fNormalizedPos2 = 0.0f)
        {
            int nSlice1 = (int) (nSliceCount * float.Clamp(fNormalizedPos1, 0,1) + 0.5f);
            int nSlice2 = (int) (nSliceCount * float.Clamp(fNormalizedPos2, 0,1) + 0.5f);
            Cut(nSlice1, nSlice2);
        }

        /// <summary>
        /// Cut the voxel field along the two absolute slice values (0..nSliceCount-1)
        /// </summary>
        /// <param name="nSlice1">First slice position</param>
        /// <param name="nSlice2">Second slice position</param>
        public void Cut(int nSlice1, int nSlice2)
        {
            // Clamp to valid index range
            nSlice1 = Math.Clamp(nSlice1, 0, nSliceCount - 1);
            nSlice2 = Math.Clamp(nSlice2, 0, nSliceCount - 1);

            m_oViz1.Visualize(nSlice1);
            m_oViz2.Visualize(nSlice2);

            int nSliceMin = Math.Min(nSlice1, nSlice2);
            int nSliceMax = Math.Max(nSlice1, nSlice2);

            // Publish latest cut positions (in meters) to the worker atomically
            float fMin = m_vox.fVoxelSize * nSliceMin;
            float fMax = m_vox.fVoxelSize * nSliceMax;

            // Use Volatile.Write so the worker sees these in order
            Volatile.Write(ref m_fMin, fMin);
            Volatile.Write(ref m_fMax, fMax);

            // Signal the worker there is new work
            m_evChanged.Set();
        }

        /// <summary>
        /// Call to stop the visualization (or let the object go out of scope, if you used using)
        /// </summary>
        public void Dispose()
        {
            // Stop worker
            m_cts.Cancel();
            m_evChanged.Set(); // wake if waiting
            try 
            { 
                m_oTask?.Wait(); 
            } 
            catch (AggregateException) 
            { 
                // swallow cancellations
            }

            // Cleanup viewer visuals
            m_oViz1.Dispose();
            m_oViz2.Dispose();

            m_evChanged.Dispose();
            m_cts.Dispose();
        }

        void CutterTask(CancellationToken tok)
        {
            // Keep last applied range to avoid redundant recompute
            float fLastMin = float.NaN;
            float fLastMax = float.NaN;

            Voxels voxCut = m_vox; // current cut vox in viewer, replaced on update

            while (!tok.IsCancellationRequested)
            {
                // Wait for a signal or cancellation; also coalesce rapid updates
                m_evChanged.WaitOne();
                if (tok.IsCancellationRequested) 
                    break;

                // Drain extra signals quickly to coalesce multiple Cut() calls
                while (m_evChanged.WaitOne(0)) 
                { /* no-op */ }

                // Read latest published values
                float fCurMin = Volatile.Read(ref m_fMin);
                float fCurMax = Volatile.Read(ref m_fMax);

                if (fCurMin == fLastMin && fCurMax == fLastMax)
                    continue;

                fLastMin = fCurMin; 
                fLastMax = fCurMax;

                // Compute new trim bounds
                var oTrimBounds = m_oBounds;
                switch (m_eAxis)
                {
                    case Voxels.ESliceAxis.X:
                        oTrimBounds.vecMin.X = m_oBounds.vecMin.X + fCurMin;
                        oTrimBounds.vecMax.X = m_oBounds.vecMin.X + fCurMax;
                        break;
                    case Voxels.ESliceAxis.Y:
                        oTrimBounds.vecMin.Y = m_oBounds.vecMin.Y + fCurMin;
                        oTrimBounds.vecMax.Y = m_oBounds.vecMin.Y + fCurMax;
                        break;
                    case Voxels.ESliceAxis.Z:
                        oTrimBounds.vecMin.Z = m_oBounds.vecMin.Z + fCurMin;
                        oTrimBounds.vecMax.Z = m_oBounds.vecMin.Z + fCurMax;
                        break;
                }

                Voxels vox = m_vox.voxTrim(oTrimBounds);
                m_oViewer.Add(vox, m_nGroup);
                m_oViewer.Remove(voxCut);
                voxCut = vox;
            }

            m_oViewer.Remove(voxCut);
        }

        readonly Viewer                     m_oViewer;
        readonly Voxels                     m_vox;
        readonly int                        m_nGroup;
        readonly Voxels.ESliceAxis          m_eAxis;
        readonly SliceViz                   m_oViz1;
        readonly SliceViz                   m_oViz2;
        readonly BBox3                      m_oBounds;

        // Signaled state from producer (Cut) to worker
        float                               m_fMin;
        float                               m_fMax;

        readonly AutoResetEvent             m_evChanged;
        readonly CancellationTokenSource    m_cts;
        readonly Task                       m_oTask;
    }
}