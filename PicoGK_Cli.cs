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

using System.Text;
using System.Numerics;
using System.Diagnostics;

namespace PicoGK
{
    /// <summary>
    /// ASCII CLI (Common Layer Interface) I/O based on
    /// https://www.hmilch.net/downloads/cli_format.html#:~:text=CLI%20is%20intended%20as%20a,data%20structure%20of%20the%20machine.
    /// Probably doesn't work if each $$ command is not in a separate line (\n) the spec is not clear whether this should be possible
    /// The parser could probably be written in a more concise way if it generalized the structure $$ .. / for commands.
    /// But this is generally not a well-though-out format.
    /// </summary>
	public class CliIo
    {
        public class Result
        {
            public PolySliceStack oSlices = new();
            public BBox3 oBBoxFile = new();
            public bool bBinary = false;
            public float fUnitsHeader = 0.0f;
            public bool b32BitAlign = false;
            public UInt32 nVersion = 0;
            public string strHeaderDate = "";
            public UInt32 nLayers = 0;
            public string strWarnings = "";
        }

        public static void WriteSlicesToCliFile(PolySliceStack oSlices,
                                                    string strFilePath,
                                                    string strDate = "",
                                                    float fUnitsInMM = 0.0f)
        {
            if (fUnitsInMM <= 0.0f)
                fUnitsInMM = 1.0f;

            if (strDate == "")
                strDate = DateTime.Now.ToString("yyyy-MM-dd");

            using (FileStream oFile = new(strFilePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter oTextWriter = new(oFile, Encoding.ASCII))
                {
                    oTextWriter.WriteLine("$$HEADERSTART");
                    oTextWriter.WriteLine("$$ASCII");
                    oTextWriter.WriteLine("$$UNITS/{0, 00000000}", fUnitsInMM);
                    oTextWriter.WriteLine("$$VERSION/200");
                    oTextWriter.WriteLine("$$LABEL/1,default");
                    oTextWriter.WriteLine("$$DATE/" + strDate);


                    string strDim = oSlices.oBBox().vecMin.X.ToString("00000000.00000") + "," +
                                    oSlices.oBBox().vecMin.Y.ToString("00000000.00000") + "," +
                                    oSlices.oBBox().vecMin.Z.ToString("00000000.00000") + "," +
                                    oSlices.oBBox().vecMax.X.ToString("00000000.00000") + "," +
                                    oSlices.oBBox().vecMax.Y.ToString("00000000.00000") + "," +
                                    oSlices.oBBox().vecMax.Z.ToString("00000000.00000");

                    oTextWriter.WriteLine("$$DIMENSION/{0}", strDim);
                    oTextWriter.WriteLine("$$LAYERS/{0}", (oSlices.nCount() + 1).ToString("00000"));
                    oTextWriter.WriteLine("$$HEADEREND");
                    oTextWriter.WriteLine("$$GEOMETRYSTART");

                    // Add the zero layer at the bottom
                    oTextWriter.WriteLine("$$LAYER/0.0");

                    // Now add all the actual layers
                    for (int nLayer = 0; nLayer < oSlices.nCount(); nLayer++)
                    {
                        PolySlice oSlice = oSlices.oSliceAt(nLayer);
                        oTextWriter.WriteLine("$$LAYER/{0}", (oSlice.fZPos() / fUnitsInMM).ToString("0.00000"));

                        for (int nPass = 0; nPass < 3; nPass++)
                        {
                            for (int nPolyline = 0; nPolyline < oSlice.nCountours(); nPolyline++)
                            {
                                PolyContour oPoly = oSlice.oCountourAt(nPolyline);

                                if ((nPass == 0) && (oPoly.eWinding() != PolyContour.EWinding.COUNTERCLOCKWISE))
                                {
                                    // Outside contours first
                                    continue;
                                }
                                else if ((nPass == 1) && (oPoly.eWinding() != PolyContour.EWinding.CLOCKWISE))
                                {
                                    // Inside contours second
                                    continue;
                                }
                                else if ((nPass == 2) && (oPoly.eWinding() != PolyContour.EWinding.UNKNOWN))
                                {
                                    // Unknown contours last
                                    continue;
                                }

                                int nWinding = 2; // open/unknown/degenerate

                                if (oPoly.eWinding() == PolyContour.EWinding.CLOCKWISE)
                                    nWinding = 0; // internal
                                else if (oPoly.eWinding() == PolyContour.EWinding.COUNTERCLOCKWISE)
                                    nWinding = 1; // external

                                // Create a StringBuilder instance
                                StringBuilder oPolyLine = new StringBuilder();

                                // Append the initial part
                                oPolyLine.Append("$$POLYLINE/1,");
                                oPolyLine.Append(nWinding.ToString());
                                oPolyLine.Append(",");
                                oPolyLine.Append(oPoly.nCount().ToString());

                                for (int nVertex = 0; nVertex < oPoly.nCount(); nVertex++)
                                {
                                    oPolyLine.Append(",");
                                    oPolyLine.Append((oPoly.vecVertex(nVertex).X / fUnitsInMM).ToString("0.00000"));
                                    oPolyLine.Append(",");
                                    oPolyLine.Append((oPoly.vecVertex(nVertex).Y / fUnitsInMM).ToString("0.00000"));
                                }

                                oTextWriter.WriteLine(oPolyLine.ToString());

                            }
                        }
                    }

                    oTextWriter.WriteLine("$$GEOMETRYEND");
                }
            }
        }

        public static Result oSlicesFromCliFile(string strFilePath)
        {
            Result oResult = new();

            using (FileStream oFile = new(strFilePath, FileMode.Open, FileAccess.Read))
            {
                long nFileSize = oFile.Length;

                // It's not entirely clear from the sta
                using (StreamReader oTextReader = new(oFile, Encoding.ASCII))
                {
                    string? strLine;
                    bool bInComment = false;

                    UInt32 nLabel = UInt32.MaxValue;
                    string strLabel = "";

                    UInt32 nHeaderLayerNumbers = UInt32.MaxValue;

                    bool bHeaderStarted = false;
                    bool bHeaderEnded = false;

                    int iCurrentFileLine = 0;

                    while ((!bHeaderEnded) && (strLine = oTextReader.ReadLine()) != null)
                    {
                        iCurrentFileLine++;

                        // It is not entirely clear from the documentation
                        // But it appears that the CLI files are organized with
                        // newlines separating the commands
                        // So, multiple commands in one line are likely not
                        // possible, and probably also not multi-line comments
                        // We are trying to ignore the newlines anyway and
                        // see how for we get
                        while (strLine != "")
                        {
                            strLine.Trim();
                            if (strLine.StartsWith("//"))
                            {
                                bInComment = true;
                                strLine = strLine.Substring(2);
                            }

                            if (bInComment)
                            {
                                int iEndComment = strLine.IndexOf("//");

                                if (iEndComment == -1)
                                {
                                    strLine = "";
                                    // multi line comment
                                    continue;
                                }

                                strLine = strLine.Substring(iEndComment + 2);
                                strLine.Trim();
                                bInComment = false;
                            }

                            if (!bHeaderStarted)
                            {
                                int iHeaderStarted = strLine.IndexOf("$$HEADERSTART");
                                if (iHeaderStarted == -1)
                                    continue; // Ignore everything until header starts

                                strLine = strLine.Substring(iHeaderStarted + "$$HEADERSTART".Length);
                                strLine.Trim();
                                bHeaderStarted = true;
                                continue;
                            }

                            if (!strLine.StartsWith("$$"))
                                continue; // Next round

                            if (strLine.StartsWith("$$BINARY"))
                            {
                                strLine = strLine.Substring("$$BINARY".Length);
                                oResult.bBinary = true;
                                continue;
                            }

                            if (strLine.StartsWith("$$ASCII"))
                            {
                                strLine = strLine.Substring("$$ASCII".Length);
                                oResult.bBinary = false;
                                continue;
                            }

                            if (strLine.StartsWith("$$ALIGN"))
                            {
                                strLine = strLine.Substring("$$ALIGN".Length);
                                oResult.b32BitAlign = true;
                                continue;
                            }

                            if (strLine.StartsWith("$$UNITS"))
                            {
                                strLine = strLine.Substring("$$UNITS".Length);

                                string strParam = "";
                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$UNITS");

                                if (!float.TryParse(strParam, out oResult.fUnitsHeader))
                                    throw new ArgumentException("Invalid parameter for $$UNITS: " + strParam);

                                if (oResult.fUnitsHeader <= 0.0f)
                                    throw new ArgumentException("Invalid parameter for $$UNITS: " + strParam);

                                continue;
                            }

                            if (strLine.StartsWith("$$VERSION"))
                            {
                                strLine = strLine.Substring("$$VERSION".Length);

                                string strParam = "";
                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$VERSION");

                                //if (!UInt32.TryParse(strParam, out oResult.nVersion))
                                //    throw new ArgumentException("Invalid parameter for $$VERSION: " + strParam);

                                continue;
                            }

                            if (strLine.StartsWith("$$LABEL"))
                            {
                                strLine = strLine.Substring("$$LABEL".Length);

                                string strParam = "";
                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$LABEL");

                                if (nLabel != UInt32.MaxValue)
                                    throw new NotSupportedException("Currently we do not support multiple labels and objects in one CLI file");

                                if (!UInt32.TryParse(strParam, out nLabel))
                                    throw new ArgumentException("Invalid parameter for $$LABEL: " + strParam);

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$LABEL (text)");

                                strLabel = strParam;
                                strLabel.Trim();

                                continue;
                            }

                            if (strLine.StartsWith("$$DATE"))
                            {
                                strLine = strLine.Substring("$$DATE".Length);

                                string strParam = "";
                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$DATE");

                                strParam.Trim();

                                oResult.strHeaderDate = strParam;
                                continue;
                            }

                            if (strLine.StartsWith("$$DIMENSION"))
                            {
                                strLine = strLine.Substring("$$DIMENSION".Length);

                                float f = 0.0f;

                                string strParam = "";

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter (xMin) after $$DIMENSION");

                                if (!float.TryParse(strParam, out f))
                                    throw new ArgumentException("Invalid parameter (xMin) for $$DIMENSION: " + strParam);

                                oResult.oBBoxFile.vecMin.X = f;

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter (yMin) after $$DIMENSION");

                                if (!float.TryParse(strParam, out f))
                                    throw new ArgumentException("Invalid parameter (yMin) for $$DIMENSION: " + strParam);

                                oResult.oBBoxFile.vecMin.Y = f;

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter (zMin) after $$DIMENSION");

                                if (!float.TryParse(strParam, out f))
                                    throw new ArgumentException("Invalid parameter (zMin) for $$DIMENSION: " + strParam);

                                oResult.oBBoxFile.vecMin.Z = f;

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter (xMax) after $$DIMENSION");

                                if (!float.TryParse(strParam, out f))
                                    throw new ArgumentException("Invalid parameter (xMax) for $$DIMENSION: " + strParam);

                                oResult.oBBoxFile.vecMax.X = f;

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter (yMax) after $$DIMENSION");

                                if (!float.TryParse(strParam, out f))
                                    throw new ArgumentException("Invalid parameter (yMax) for $$DIMENSION: " + strParam);

                                oResult.oBBoxFile.vecMax.Y = f;

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter (zMax) after $$DIMENSION");

                                if (!float.TryParse(strParam, out f))
                                    throw new ArgumentException("Invalid parameter (zMax) for $$DIMENSION: " + strParam);

                                oResult.oBBoxFile.vecMax.Z = f;

                                continue;
                            }

                            if (strLine.StartsWith("$$LAYERS"))
                            {
                                strLine = strLine.Substring("$$LAYERS".Length);

                                string strParam = "";
                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$LAYERS");

                                if (!UInt32.TryParse(strParam, out nHeaderLayerNumbers))
                                    throw new ArgumentException("Invalid parameter for $$LAYERS: " + strParam);

                                continue;
                            }

                            if (strLine.StartsWith("$$HEADEREND"))
                            {
                                bHeaderEnded = true;
                                break;
                            }
                        } // while not empty
                    } // while able to read file

                    if (!bHeaderEnded)
                        throw new ArgumentException("End of file while searching for valid header");

                    // Now read the actual geometry

                    if (oResult.bBinary)
                        throw new NotSupportedException("Binary CLI Files are not yet supported");

                    bool bGeometryStarted = false;
                    bool bGeometryEnded = false;

                    PolySlice? oCurrentSlice = null;

                    DateTime timePast = DateTime.Now;

                    List<PolySlice> oSlices = new();
                    float fPrevZPos = float.MinValue;

                    while ((!bGeometryEnded) && (strLine = oTextReader.ReadLine()) != null)
                    {
                        iCurrentFileLine++;

                        //if (iCurrentFileLine > 10000)
                        //    break;

                        while (strLine != "")
                        {
                            DateTime time = DateTime.Now;
                            TimeSpan span = time - timePast;

                            if (span.Seconds > 1)
                            {
                                float fPercentRead = 100.0f * oFile.Position / (float)nFileSize;
                                Console.WriteLine("Read: {0}% completed Line {1}: '{2}'...", Math.Floor(fPercentRead), iCurrentFileLine, Utils.strShorten(strLine, 20));
                                timePast = time;
                            }

                            strLine.Trim();
                            if (strLine.StartsWith("//"))
                            {
                                bInComment = true;
                                strLine = strLine.Substring(2);
                            }

                            if (bInComment)
                            {
                                int iEndComment = strLine.IndexOf("//");

                                if (iEndComment == -1)
                                {
                                    strLine = "";
                                    // multi line comment
                                    continue;
                                }

                                strLine = strLine.Substring(iEndComment + 2);
                                strLine.Trim();
                                bInComment = false;
                            }

                            if (!bGeometryStarted)
                            {
                                int iHeaderStarted = strLine.IndexOf("$$GEOMETRYSTART");
                                if (iHeaderStarted == -1)
                                    continue; // Ignore everything until header starts

                                strLine = strLine.Substring(iHeaderStarted + "$$GEOMETRYSTART".Length);
                                strLine.Trim();
                                bGeometryStarted = true;
                                continue;
                            }

                            if (strLine.StartsWith("$$LAYER"))
                            {
                                strLine = strLine.Substring("$$LAYER".Length);

                                string strParam = "";
                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$LAYER");

                                float fZPos = 0.0f;
                                if (!float.TryParse(strParam, out fZPos))
                                    throw new ArgumentException("Invalid parameter for $$LAYER: " + strParam);

                                fZPos *= oResult.fUnitsHeader;

                                if (fPrevZPos != float.MinValue) // no previous layer
                                {
                                    if (fZPos < fPrevZPos)
                                        throw new ArgumentException("Z position in current layer is smaller than in previous " + strParam);

                                    fPrevZPos = fZPos;
                                }
                                else
                                {
                                    fPrevZPos = 0.0f;
                                }

                                if (fZPos > 0.0f)
                                {
                                    if (oCurrentSlice != null)
                                    {
                                        oSlices.Add(oCurrentSlice);
                                    }

                                    oCurrentSlice = new(fZPos);
                                }

                                continue;
                            }

                            if (strLine.StartsWith("$$POLYLINE"))
                            {
                                if (oCurrentSlice == null)
                                    throw new ArgumentException("There should not be contours at z position 0");

                                Debug.Assert(oCurrentSlice != null);

                                strLine = strLine.Substring("$$POLYLINE".Length);

                                string strParam = "";

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$POLYLINE");

                                UInt32 nID = 0;
                                if (!UInt32.TryParse(strParam, out nID))
                                    throw new ArgumentException("Invalid parameter for $$POLYLINE: " + strParam);

                                if (nLabel == UInt32.MaxValue)
                                    nLabel = nID; // If no label, we label it with the first ID we encounter

                                if (nID != nLabel)
                                    throw new NotSupportedException("We do not support CLI labels and multiple models yet");

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter after $$POLYLINE");

                                UInt32 nWinding = 0;
                                if (!UInt32.TryParse(strParam, out nWinding))
                                    throw new ArgumentException("Invalid parameter for $$POLYLINE direction: " + strParam);

                                PolyContour.EWinding eWinding = PolyContour.EWinding.UNKNOWN;
                                if (nWinding == 0)
                                    eWinding = PolyContour.EWinding.CLOCKWISE;
                                else if (nWinding == 1)
                                    eWinding = PolyContour.EWinding.COUNTERCLOCKWISE;
                                else if (nWinding != 2)
                                    throw new ArgumentException("Invalid parameter for $$POLYLINE direction: " + strParam);

                                if (!bExtractParameter(ref strLine, ref strParam))
                                    throw new ArgumentException("Missing parameter polygon count after $$POLYLINE");

                                UInt32 nCount = 0;
                                if (!UInt32.TryParse(strParam, out nCount))
                                    throw new ArgumentException("Invalid parameter for $$POLYLINE polygon count: " + strParam);

                                List<Vector2> oVertices = new();

                                while (nCount > 0)
                                {
                                    time = DateTime.Now;
                                    span = time - timePast;

                                    if (span.Seconds > 10)
                                    {
                                        float fPercentRead = 100.0f * oFile.Position / (float)nFileSize;
                                        Console.WriteLine("Read: {0}% Vertex countdown: {1}", fPercentRead, nCount);
                                        timePast = time;
                                    }

                                    float fX = 0.0f;

                                    if (!bExtractParameter(ref strLine, ref strParam))
                                        throw new ArgumentException("Missing vertices in $$POLYLINE");

                                    if (!float.TryParse(strParam, out fX))
                                        throw new ArgumentException("Invalid parameter (X) for $$POLYLINE vertex: " + strParam);

                                    float fY = 0.0f;

                                    if (!bExtractParameter(ref strLine, ref strParam))
                                        throw new ArgumentException("Missing vertices in $$POLYLINE");

                                    if (!float.TryParse(strParam, out fY))
                                        throw new ArgumentException("Invalid parameter (Y) for $$POLYLINE vertex: " + strParam);

                                    fX *= oResult.fUnitsHeader;
                                    fY *= oResult.fUnitsHeader;

                                    oVertices.Add(new Vector2(fX, fY));
                                    nCount--;
                                }

                                if (oVertices.Count < 3)
                                {
                                    oResult.strWarnings += "Line: " + iCurrentFileLine.ToString() + " Discarding POLYLINE with " + oVertices.Count.ToString() + " vertices which is degenerate\n";
                                    continue;
                                }

                                PolyContour oPoly = new(oVertices);

                                if (oPoly.eWinding() == PolyContour.EWinding.UNKNOWN)
                                {
                                    oResult.strWarnings += "Line: " + iCurrentFileLine.ToString() + " Discarding POLYLINE with area 0 (degenerate) - defined with winding " + PolyContour.strWindingAsString(eWinding) + "\n";
                                    continue;
                                }

                                if (oPoly.eWinding() != eWinding)
                                {
                                    oResult.strWarnings += "Line: " + iCurrentFileLine.ToString() + " POLYLINE defined with winding " + PolyContour.strWindingAsString(eWinding) + " actual winding is " + PolyContour.strWindingAsString(oPoly.eWinding()) + " (using actual)\n";
                                }

                                oCurrentSlice.AddContour(oPoly);
                                continue;
                            }

                            if (strLine.StartsWith("$$GEOMETRYEND"))
                            {
                                bGeometryEnded = true;
                                break; // we are done
                            }

                            if (strLine.StartsWith("$$"))
                            {
                                oResult.strWarnings += "Line: " + iCurrentFileLine.ToString() + " Unsupported command " + Utils.strShorten(strLine, 20) + "\n";
                                // unknown command
                                strLine = "";
                                continue;
                            }

                            Console.WriteLine("Line: {0} Unknown command", iCurrentFileLine);
                            Console.WriteLine(strLine);
                            strLine = "";
                        }
                    }

                    oResult.oSlices.AddSlices(oSlices);
                }
            }

            return oResult;
        }

        public CliIo()
        {
        }

        private static bool bExtractParameter(ref string strLine,
                                                ref string strParam)
        {
            if ((strLine.StartsWith('/')) || (strLine.StartsWith(',')))
                strLine = strLine.Substring(1);
            else
                return false; // no parameter

            char[] achSep = { '$', '/', ',' };

            int iEnd = strLine.IndexOfAny(achSep);
            if (iEnd != -1)
            {
                strParam = strLine.Substring(0, iEnd);
                strLine = strLine.Substring(strParam.Length);
                return true;
            }

            strParam = strLine;
            strLine = "";
            return true;
        }
    }
}
