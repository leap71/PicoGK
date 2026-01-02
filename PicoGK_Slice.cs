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
using System.Diagnostics;
using System.Globalization;

namespace PicoGK
{
    public class PolyContour
    {
        public enum EWinding
        {
            UNKNOWN,
            CLOCKWISE,
            COUNTERCLOCKWISE
        }

        public static string strWindingAsString(EWinding eWinding)
        {
            if (eWinding == EWinding.COUNTERCLOCKWISE)
                return "[counter-clockwise]";

            if (eWinding == EWinding.CLOCKWISE)
                return "[clockwise]";

            return "[unknown/degenerate]";
        }

        public static EWinding eDetectWinding(List<Vector2> oVertices)
        {
            int nVertices = oVertices.Count;

            if (nVertices < 3)
                return EWinding.UNKNOWN;

            float fArea = 0f;

            for (int i = 0; i < nVertices; i++)
            {
                int j = (i + 1) % nVertices;
                fArea += (oVertices[j].X - oVertices[i].X) *
                            (oVertices[j].Y + oVertices[i].Y);
            }

            if (fArea > 0.0f)
                return EWinding.CLOCKWISE;

            if (fArea < 0.0f)
                return EWinding.COUNTERCLOCKWISE;

            return EWinding.UNKNOWN; // Degenerate case
        }

        public PolyContour(     IEnumerable<Vector2> oVertices,
                                EWinding eWinding = EWinding.UNKNOWN)
        {
            m_oVertices = new();
            m_oBBox     = new();
            int nCount  = 0;

            foreach (Vector2 vec in oVertices)
            {
                m_oBBox.Include(vec);
                m_oVertices.Add(vec);
                nCount++;
            }

            Debug.Assert(   nCount > 2,
                            "Polyline with less than 3 points makes no sense");

            if (eWinding == EWinding.UNKNOWN)
            {
                m_eWinding = eDetectWinding(m_oVertices);
            }
            else
            {
                m_eWinding = eWinding;
                Debug.Assert(   m_eWinding == eDetectWinding(m_oVertices),
                                "Detected Winding that is not correct");
            }
        }

        public void AddVertex(Vector2 vec)
        {
            m_oVertices.Add(vec);
        }

        public void DetectWinding()
        {
            m_eWinding = eDetectWinding(m_oVertices);
        }

        public EWinding eWinding()          { return m_eWinding; }

        public List<Vector2> oVertices()    { return m_oVertices; }

        /// <summary>
        /// Makes sure that the last coordinate is identical to the first
        /// coordinate, to close the loop
        /// </summary>
        public void Close()
        {
            if (m_oVertices.Count() == 0)
                return;

            Vector2 vecDist = m_oVertices.First() - m_oVertices.Last();
            if (vecDist.Length() > float.Epsilon)
                m_oVertices.Add(m_oVertices.First());
        }

        public void AsSvgPolyline(out string str)
        {
            str = "<polyline points='";
            foreach (Vector2 vec in m_oVertices)
            {
                str += " " + vec.X.ToString(CultureInfo.InvariantCulture) + "," + vec.Y.ToString(CultureInfo.InvariantCulture);
            }

            str += " " + m_oVertices[0].X.ToString(CultureInfo.InvariantCulture) + "," +
                    m_oVertices[0].Y.ToString(CultureInfo.InvariantCulture);

            str += "' ";

            if (m_eWinding == EWinding.CLOCKWISE)
                str += "stroke='blue' fill='none'";
            else if (m_eWinding == EWinding.COUNTERCLOCKWISE)
                str += "stroke='black' fill='none'";
            else
                str += "stroke='red' fill='none'";

            str += " stroke-width='0.1' />\n";
        }

        public void AsSvgPath(out string str)
        {
            str = "";
            foreach (Vector2 vec in m_oVertices)
            {
                if (str == "")
                {
                    // first, move to position using M
                    str = " M";
                }
                else
                {
                    str += " L";
                }
                str += vec.X.ToString(CultureInfo.InvariantCulture) + "," + vec.Y.ToString(CultureInfo.InvariantCulture);
            }

            str += " Z";
        }

        List<Vector2>   m_oVertices;
        EWinding        m_eWinding;
        BBox2           m_oBBox;

        public BBox2 oBBox()                => m_oBBox;
        public int nCount()                 => m_oVertices.Count;
        public Vector2 vecVertex(int n)     => m_oVertices[n];
    }

    public class PolySlice
    {
        public PolySlice(float fZPos)
        {
            m_fZPos = fZPos;
            m_oContours = new();
            m_oBBox = new();
        }

        public void AddContour(PolyContour oPoly)
        {
            m_oBBox.Include(oPoly.oBBox());
            m_oContours.Add(oPoly);
        }

        public bool bIsEmpty()
        {
            return m_oContours.Count() == 0;
        }

        public void Close()
        {
            foreach (PolyContour oContour in m_oContours)
            {
                oContour.Close();
            }
        }

        public void SaveToSvgFile(  string strPath,
                                    bool bSolid,
                                    BBox2? oBBoxToUse = null)
        {

            BBox2 oBBoxView = oBBoxToUse ?? m_oBBox;

            using (StreamWriter writer = new(strPath))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                writer.WriteLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">\n");

                float fSizeX = oBBoxView.vecMax.X - oBBoxView.vecMin.X;
                float fSizeY = oBBoxView.vecMax.Y - oBBoxView.vecMin.Y;

                writer.WriteLine("<svg xmlns='http://www.w3.org/2000/svg' version='1.1' " +
                    "viewBox='" +
                        oBBoxView.vecMin.X.ToString(CultureInfo.InvariantCulture) + " " +
                        oBBoxView.vecMin.Y.ToString(CultureInfo.InvariantCulture) + " " +
                        fSizeX.ToString(CultureInfo.InvariantCulture) + " " +
                        fSizeY.ToString(CultureInfo.InvariantCulture) + "' " +
                        "width='" + fSizeX.ToString(CultureInfo.InvariantCulture) + "mm' " +
                        "height='" + fSizeY.ToString(CultureInfo.InvariantCulture) + "mm' " +
                        ">");

                writer.WriteLine("<g>");

                if (!bSolid)
                {
                    foreach (PolyContour oPoly in m_oContours)
                    {
                        string str = "";
                        oPoly.AsSvgPolyline(out str);
                        writer.WriteLine(str);
                    }
                }
                else
                {
                    string strContour = "<path d='";
                    for (int iPass = 0; iPass < 2; iPass++)
                    {
                        foreach (PolyContour oPoly in m_oContours)
                        {
                            if (iPass == 0)
                            {
                                if (oPoly.eWinding() != PolyContour.EWinding.COUNTERCLOCKWISE)
                                    continue; // only outside contours in first pass
                            }
                            else
                            {
                                if (oPoly.eWinding() == PolyContour.EWinding.COUNTERCLOCKWISE)
                                    continue; // skip outside contours in second pass
                            }

                            string str = "";
                            oPoly.AsSvgPath(out str);
                            strContour += str;
                        }
                    }

                    strContour += "' fill='black'/> ";
                    writer.WriteLine(strContour);
                }

                writer.WriteLine("</g>");
                writer.WriteLine("</svg>");
            }
        }

        public static PolySlice oFromSdf(   Image img,
                                            float fZPos,
                                            Vector2 vecOffset,
                                            float fScale)
        {
            PolySlice oSlice = new PolySlice(fZPos);

            if (img.nWidth < 2 || img.nHeight < 2)
                return oSlice;

            List<CSegment> oSegments = new List<CSegment>();

            for (int nY = 0; nY < img.nHeight - 1; ++nY)
            {
                for (int nX = 0; nX < img.nWidth - 1; ++nX)
                {
                    float[] afCorners =
                    {
                        img.fValue(nX, nY),
                        img.fValue(nX + 1, nY),
                        img.fValue(nX + 1, nY + 1),
                        img.fValue(nX, nY + 1),
                    };

                    int nLutIndex = 0;

                    if (afCorners[0] < 0.0) nLutIndex |= 1;
                    if (afCorners[1] < 0.0) nLutIndex |= 2;
                    if (afCorners[2] < 0.0) nLutIndex |= 4;
                    if (afCorners[3] < 0.0) nLutIndex |= 8;

                    int nEdgesCrossed = m_aanEdgeLut[nLutIndex, 0];

                    if (nEdgesCrossed == 0)
                        continue;

                    Vector2[] avecCrossings = new Vector2[4];

                    if ((nEdgesCrossed & 1) != 0)
                    {
                        avecCrossings[0] = new Vector2(nX + fZeroCrossing(afCorners[0], afCorners[1]), nY);
                    }
                    if ((nEdgesCrossed & 2) != 0)
                    {
                        avecCrossings[1] = new Vector2(nX + 1, nY + fZeroCrossing(afCorners[1], afCorners[2]));
                    }
                    if ((nEdgesCrossed & 4) != 0)
                    {
                        avecCrossings[2] = new Vector2(nX + fZeroCrossing(afCorners[3], afCorners[2]), nY + 1);
                    }
                    if ((nEdgesCrossed & 8) != 0)
                    {
                        avecCrossings[3] = new Vector2(nX, nY + fZeroCrossing(afCorners[0], afCorners[3]));
                    }

                    for (int nSegment = 1; nSegment < 5; nSegment += 2)
                    {
                        if (m_aanEdgeLut[nLutIndex, nSegment] >= 0)
                        {
                            oSegments.Add(new CSegment(
                                vecOffset + (fScale * avecCrossings[m_aanEdgeLut[nLutIndex, nSegment]]),
                                vecOffset + (fScale * avecCrossings[m_aanEdgeLut[nLutIndex, nSegment + 1]])));
                        }
                        else
                            break;
                    }
                }
            }

            //create contours from (partially sorted) segment soup

            int nSegmentsLeft = oSegments.Count;

            int nCurrStart  = -1;
            int nCurrEnd    = -1;
            int nUnused     = 0;

            LinkedList<Vector2> oNewContour = new();

            while (nSegmentsLeft > 0)
            {
                if (nCurrStart < 0)
                {
                    for (int n = nUnused; n < oSegments.Count; ++n)
                    {
                        if (!oSegments[n].m_bUsed)
                        {
                            nCurrStart = n;
                            break;
                        }
                    }

                    Debug.Assert(nCurrStart >= 0);

                    nCurrEnd    = nCurrStart;
                    nUnused     = nCurrStart + 1;

                    oNewContour.AddLast(oSegments[nCurrStart].m_vecStart);
                    oNewContour.AddLast(oSegments[nCurrStart].m_vecEnd);
                    oSegments[nCurrStart].m_bUsed = true;
                    nSegmentsLeft--;
                }

                int nBestStart      = -1;
                int nBestEnd        = -1;
                float fBestSqrStart = 1.0f;
                float fBestSqrEnd   = 1.0f;

                if (nCurrEnd != nCurrStart)
                {
                    float fSqrDist = (oSegments[nCurrStart].m_vecStart - oSegments[nCurrEnd].m_vecEnd).LengthSquared();
                    if (fSqrDist < 1.0f)
                    {
                        fBestSqrEnd = fBestSqrStart = fSqrDist;
                        nBestStart  = nCurrEnd;
                        nBestEnd    = nCurrStart;
                    }
                }

                float fMin = MathF.Floor(Math.Min(oSegments[nCurrStart].m_fMinY, oSegments[nCurrEnd].m_fMinY)) - 1.0f;
                float fMax = MathF.Ceiling(Math.Max(oSegments[nCurrStart].m_fMaxY, oSegments[nCurrEnd].m_fMaxY)) + 1.0f;

                int nSearchFrom = Math.Min(nCurrStart, nCurrEnd);
                while (nSearchFrom > 0)
                {
                    if (oSegments[nSearchFrom - 1].m_fMaxY >= fMin)
                        nSearchFrom--;
                    else
                        break;
                }

                for (int n = nSearchFrom; n < oSegments.Count; ++n)
                {
                    if (oSegments[n].m_bUsed)
                        continue;
                    if (oSegments[n].m_fMinY > fMax)
                        break;

                    float fSqrDist = (oSegments[nCurrStart].m_vecStart - oSegments[n].m_vecEnd).LengthSquared();
                    if (fSqrDist < fBestSqrStart)
                    {
                        fBestSqrStart   = fSqrDist;
                        nBestStart      = n;
                    }

                    fSqrDist = (oSegments[nCurrEnd].m_vecEnd - oSegments[n].m_vecStart).LengthSquared();
                    if (fSqrDist < fBestSqrEnd)
                    {
                        fBestSqrEnd = fSqrDist;
                        nBestEnd    = n;
                    }                        
                }
 
                if (nBestEnd < 0 && nBestStart < 0)
                {
                    Debug.Assert(oNewContour.Count() < 3);
                    oNewContour.Clear();
                    nCurrStart = nCurrEnd = -1;
                }
                else if (nBestStart == nBestEnd || nBestStart == nCurrEnd || nBestEnd == nCurrStart)
                {
                    if (nBestStart == nBestEnd)
                    {
                        oSegments[nBestEnd].m_bUsed = true;
                        nSegmentsLeft--;
                    }

                    if (oNewContour.Count > 2)
                        oSlice.AddContour(new PolyContour(oNewContour));

                    oNewContour.Clear();
                    nCurrStart = nCurrEnd = -1;
                }
                else
                {
                    if (nBestEnd >= 0)
                    {
                        oNewContour.AddLast(oSegments[nBestEnd].m_vecEnd);
                        oSegments[nBestEnd].m_bUsed = true;
                        nSegmentsLeft--;
                        nCurrEnd = nBestEnd;
                    }
                    if (nBestStart >= 0)
                    {
                        oNewContour.AddFirst(oSegments[nBestStart].m_vecStart);
                        oSegments[nBestStart].m_bUsed = true;
                        nSegmentsLeft--;
                        nCurrStart = nBestStart;
                    }
                }
            }

            return oSlice;
        }

        public float fZPos()                    => m_fZPos;
        public BBox2 oBBox()                    => m_oBBox;
        public int nContours()                 => m_oContours.Count;
        public PolyContour oContourAt(int i)   => m_oContours[i];

        List<PolyContour>   m_oContours;
        float               m_fZPos;
        BBox2               m_oBBox;
        
        static readonly int[,] m_aanEdgeLut =
        {
            { 0,-1, -1, -1, -1},
            { 9, 0,  3, -1, -1},
            { 3, 1,  0, -1, -1},
            {10, 1,  3, -1, -1},
            { 6, 2,  1, -1, -1},
            {15, 0,  1,  2,  3},
            { 5, 2,  0, -1, -1},
            {12, 2,  3, -1, -1},
            {12, 3,  2, -1, -1},
            { 5, 0,  2, -1, -1},
            {15, 3,  0,  1,  2},
            { 6, 1,  2, -1, -1},
            {10, 3,  1, -1, -1},
            { 3, 0,  1, -1, -1},
            { 9, 3,  0, -1, -1},
            { 0,-1, -1, -1, -1}
        };
        
        class CSegment
        {
            public CSegment(Vector2 vecStart, Vector2 vecEnd)
            {
                m_vecStart  = vecStart;
                m_vecEnd    = vecEnd;
                m_fMinY     = Math.Min(vecStart.Y, vecEnd.Y);
                m_fMaxY     = Math.Max(vecStart.Y, vecEnd.Y);
            }

            public Vector2  m_vecStart;
            public Vector2  m_vecEnd;
            public float    m_fMinY;
            public float    m_fMaxY;
            public bool     m_bUsed = false;
        };

        static float fZeroCrossing(float fA, float fB)
        {
            // add small number to avoid division by zero
            return Math.Abs(fA) / (Math.Abs(fA) + Math.Abs(fB)) + 1e-6f;
        }
    }

    public class PolySliceStack
    {
        public PolySliceStack()
        {
            m_oSlices   = new();
            m_oBBox     = new();
        }

        public PolySliceStack(List<PolySlice> oSlices) : this()
        {
            AddSlices(oSlices);
        }

        public void AddSlices(List<PolySlice> oSlices)
        {
            foreach (PolySlice oSlice in oSlices)
            {
                m_oBBox.Include(oSlice.oBBox(), oSlice.fZPos());
                m_oSlices.Add(oSlice);
            }
        }

        public void AddToViewer(    Viewer oViewer,
                                    ColorFloat? clrOutside      = null,
                                    ColorFloat? clrInside       = null,
                                    ColorFloat? clrDegenerate   = null,
                                    int nGroup = 0)
        {
            if (clrDegenerate is null)
                clrDegenerate   = "#AAAAAAAA";

            if (clrInside is null)
                clrInside       = "#AAAAAAAA";

            if (clrOutside is null)
                clrOutside      = "#FF0000AA";

            foreach (PolySlice oSlice in m_oSlices)
            {  
                for (int n = 0; n < oSlice.nContours(); n++)
                {
                    PolyContour oContour = oSlice.oContourAt(n);

                    ColorFloat? clr = clrDegenerate;

                    if (oContour.eWinding() == PolyContour.EWinding.CLOCKWISE)
                        clr = clrInside;
                    else if (oContour.eWinding() == PolyContour.EWinding.COUNTERCLOCKWISE)
                        clr = clrOutside;

                    PolyLine oPolyLine = new PolyLine((ColorFloat)clr);

                    foreach (Vector2 vec in oContour.oVertices())
                    {
                        oPolyLine.nAddVertex(new Vector3(vec.X, vec.Y, oSlice.fZPos()));
                    }

                    oViewer.Add(oPolyLine, nGroup);
                }
            }
        }

        public int nCount()                 => m_oSlices.Count();
        public PolySlice oSliceAt(int n)    => m_oSlices[n];
        public BBox3 oBBox()                => m_oBBox;

        List<PolySlice> m_oSlices;
        BBox3           m_oBBox;
    }
}
