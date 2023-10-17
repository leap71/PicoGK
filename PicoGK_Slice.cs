//
// SPDX-License-Identifier: Apache-2.0
//
// PicoGK ("peacock") is a compact software kernel for computational geometry,
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://picogk.org
// 
// PicoGK is developed and maintained by LEAP 71 - © 2023 by LEAP 71
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

        public PolyContour(     List<Vector2> oVertices,
                                EWinding eWinding = EWinding.UNKNOWN)
        {
            Debug.Assert(oVertices.Count > 2,
                            "Polyline with less than 3 points makes no sense");

            m_oVertices = new List<Vector2>(oVertices);

            if (eWinding == EWinding.UNKNOWN)
            {
                m_eWinding = eDetectWinding(m_oVertices);
            }
            else
            {
                m_eWinding = eWinding;
                Debug.Assert(m_eWinding == eDetectWinding(m_oVertices),
                                "Detected Winding that is not correct");
            }

            m_oBBox = new();

            foreach (Vector2 vec in m_oVertices)
            {
                m_oBBox.Include(vec);
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

        public EWinding eWinding() { return m_eWinding; }
        public List<Vector2> oVertices() { return m_oVertices; }

        public void AsSvgPolyline(out string str)
        {
            str = "<polyline points='";
            foreach (Vector2 vec in m_oVertices)
            {
                str += " " + vec.X.ToString() + "," + vec.Y.ToString();
            }

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
                str += vec.X.ToString() + "," + vec.Y.ToString();
            }

            str += " Z";
        }

        List<Vector2> m_oVertices;
        EWinding m_eWinding;
        BBox2 m_oBBox;

        public BBox2 oBBox() => m_oBBox;
        public int nCount() => m_oVertices.Count;
        public Vector2 vecVertex(int n) => m_oVertices[n];
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
                        oBBoxView.vecMin.X.ToString() + " " + oBBoxView.vecMin.Y.ToString() + " " +
                        fSizeX.ToString() + " " +
                        fSizeY.ToString() + "' " +
                        "width='" + fSizeX.ToString() + "mm' " +
                        "height='" + fSizeY.ToString() + "mm' " +
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

        public float fZPos() => m_fZPos;
        public BBox2 oBBox() => m_oBBox;
        public int nCountours() => m_oContours.Count;
        public PolyContour oCountourAt(int i) => m_oContours[i];

        List<PolyContour>   m_oContours;
        float               m_fZPos;
        BBox2 m_oBBox;
    }

    public class PolySliceStack
    {
        public PolySliceStack()
        {
            m_oSlices = new();
            m_oBBox = new();
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
                for (int n = 0; n < oSlice.nCountours(); n++)
                {
                    PolyContour oContour = oSlice.oCountourAt(n);

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

        public int nCount() => m_oSlices.Count();
        public PolySlice oSliceAt(int n) => m_oSlices[n];
        public BBox3 oBBox() => m_oBBox;

        List<PolySlice> m_oSlices;
        BBox3 m_oBBox;
    }
}
