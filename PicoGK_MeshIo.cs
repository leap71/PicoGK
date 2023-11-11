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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PicoGK
{
    public partial class Mesh
    {
        public enum EStlUnit { AUTO, MM, CM, M, FT, IN };

        /// <summary>
        /// Loads a mesh from an STL file
        /// By default, it tries to find a UNITS= info in the header and uses that
        /// to scale the mesh automatically to mm. If none is found mm is assumed
        /// You can override this behaviour and explicitly specify a unit
        /// To explicitly scale the STL, provide the fPostScale parameter
        /// vecPostOffsetMM is applied last, offsetting the mesh
        /// </summary>
        /// <param name="strFilePath">
        /// Path to the file. If not found or not able to open, an exception is thrown
        /// </param>
        /// <param name="bFuseVertices">Fuse vertices that are close to each other (make mesh watertight)</param>
        /// <param name="eLoadUnit">Units to load</param>
        /// <param name="fPostScale">Scale parameter to be applied before offset</param>
        /// <param name="vecPostOffsetMM">Offset parameter to be applied last</param>
        /// <returns>Returns a valid mesh in any case. If file was invalid, the mesh may be empty.</returns>
        public static Mesh mshFromStlFile(  string strFilePath,
                                            EStlUnit eLoadUnit = EStlUnit.AUTO, // use units from file, or mm when not spec'd
                                            float fPostScale = 1.0f,
                                            Vector3? vecPostOffsetMM = null)
        {
            Mesh oMesh;

            using (FileStream oFile = new FileStream(strFilePath, FileMode.Open, FileAccess.Read))
            {
                oMesh = mshFromStlFile( oFile,
                                        eLoadUnit,
                                        fPostScale,
                                        vecPostOffsetMM);
            }

            return oMesh;
        }


        public static Mesh mshFromStlFile(  FileStream  oFile,
                                            EStlUnit    eLoadUnit       = EStlUnit.AUTO, // use units from file, or mm when not spec'd
                                            float       fPostScale      = 1.0f,
                                            Vector3?    vecPostOffsetMM = null)
        {
            string strHeader = "";

            Vector3 vecOffset = new Vector3(0.0f);

            if (vecPostOffsetMM is not null)
                vecOffset = (Vector3)vecPostOffsetMM;
            
            using (BinaryReader oReader = new BinaryReader(oFile, Encoding.ASCII))
            {
                byte[] byHeader = new byte[80];
                oReader.Read(byHeader, 0, 80);
                strHeader = Encoding.ASCII.GetString(byHeader).Trim();
                string strOriginal = strHeader;

                bool bAscii = strHeader.StartsWith("solid");

                if (bAscii)
                    strHeader = strHeader.Substring("solid".Length);

                float fScale = 1.0f; // 1mm default

                if (eLoadUnit == EStlUnit.AUTO)
                {
                    // See if there is the inofficial "UNIT" tag inside the header
                    int iUnit = strHeader.IndexOf("UNITS=", StringComparison.OrdinalIgnoreCase);
                    if (iUnit != -1)
                    {
                        strHeader = strHeader.Substring(iUnit + "units=".Length);

                        if (strHeader.StartsWith(" m"))
                            eLoadUnit = EStlUnit.M;
                        else if (strHeader.StartsWith("mm"))
                            eLoadUnit = EStlUnit.MM;
                        else if (strHeader.StartsWith("cm"))
                            eLoadUnit = EStlUnit.CM;
                        else if (strHeader.StartsWith("ft"))
                            eLoadUnit = EStlUnit.FT;
                        else if (strHeader.StartsWith("in"))
                            eLoadUnit = EStlUnit.IN;

                        // We don't support lightyears...
                    }
                }

                Mesh oMesh = new Mesh();
                oMesh.m_strLoadHeaderData = strOriginal;
                oMesh.m_eLoadUnits = eLoadUnit;

                if (bAscii)
                {
                    oMesh.DoReadMeshFromAsciiStl(oReader,
                                                    eLoadUnit,
                                                    fPostScale,
                                                    vecOffset);
                }
                else
                {
                    oMesh.DoReadMeshFromBinaryStl(oReader,
                                                    eLoadUnit,
                                                    fScale,
                                                    vecOffset);
                }

                return oMesh;
            }
        }



        /// <summary>
        /// Saves a Mesh to STL file
        /// If eUnit is auto, then, if this mesh was loaded from an STL before
        /// the same units as before are being used (stored in public property
        /// m_eLoadUnits). If not loaded from an STL, defaults to mm.
        /// Internal mesh units are always mm, so if a unit other than mm is
        /// used, the mesh coordinates are scaled appropriately (divided by 10
        /// for CM units, for example). Our STL loaders use the UNIT= parameter
        /// in the header, so whatever units you use, you will always get an STL
        /// with the same dimensions if you load it again using our loader.
        /// However many STL importers out there, ignore the UNIT info in the
        /// header.
        /// You can explicitly offset the mesh before saving (offset in mm)
        /// after offset, a scale parameter can be applied.
        /// The last operation is always the conversion to the unit
        /// </summary>
        /// <param name="strFilePath">File path. If not writable, exceptions is thrown
        /// </param>
        /// <param name="eUnit">If loaded previously, defaults to original units.
        /// MM if not previously loaded. Can be overridden to save to a specifc
        /// unit.</param>
        /// <param name="vecOffsetMM">Offset applied while still in mm units</param>
        /// <param name="fScale">Scale applied after offset, while still in mm units</param>
        public void SaveToStlFile(  string strFilePath,
                                    EStlUnit eUnit = EStlUnit.AUTO,
                                    Vector3? vecOffsetMM = null,
                                    float fScale = 1.0f)
        {
            using (FileStream oFile = new FileStream(strFilePath, FileMode.Create, FileAccess.Write))
            {
                SaveToStlFile(oFile, eUnit, vecOffsetMM, fScale);
            }
        }

        public void SaveToStlFile(  FileStream oFile,
                                    EStlUnit eUnit = EStlUnit.AUTO,
                                    Vector3? vecOffsetMM = null,
                                    float fScale = 1.0f)
        {
            Vector3 vecOffset = new Vector3(0.0f);

            if (vecOffsetMM is not null)    
                vecOffset = (Vector3)vecOffsetMM;

            if (eUnit == EStlUnit.AUTO)
                eUnit = m_eLoadUnits;

  
            using (BinaryWriter oWriter = new BinaryWriter(oFile, Encoding.ASCII))
            {
                string strHeader = "PicoGK ";

                switch (eUnit)
                {
                    case EStlUnit.CM:
                        strHeader += "UNITS=cm";
                        break;
                    case EStlUnit.M:
                        strHeader += "UNITS= m";
                        break;
                    case EStlUnit.FT:
                        strHeader += "UNITS=ft";
                        break;
                    case EStlUnit.IN:
                        strHeader += "UNITS=in";
                        break;
                    default:
                        strHeader += "UNITS=mm";
                        break;
                }

                strHeader = strHeader.PadRight(80, ' ');

                oWriter.Write(Encoding.ASCII.GetBytes(strHeader), 0, 80);

                UInt32 n32Triangles = (uint)nTriangleCount();
                oWriter.Write(n32Triangles);

                SStlTriangle sTriangle = new SStlTriangle();
                Span<byte> abyMemory = MemoryMarshal.Cast<SStlTriangle, byte>(MemoryMarshal.CreateSpan(ref sTriangle, 1));

                for (int n = 0; n < nTriangleCount(); n++)
                {
                    GetTriangle(n,
                                    out Vector3 v1,
                                    out Vector3 v2,
                                    out Vector3 v3);


                    TransformToUnit(ref v1, vecOffset, fScale, eUnit);
                    TransformToUnit(ref v2, vecOffset, fScale, eUnit);
                    TransformToUnit(ref v3, vecOffset, fScale, eUnit);


                    Vector3 vecNormal = Vector3.Cross((v2 - v1), (v3 - v1));
                    vecNormal /= vecNormal.Length();

                    sTriangle.NormalX = vecNormal.X;
                    sTriangle.NormalY = vecNormal.Y;
                    sTriangle.NormalZ = vecNormal.Z;
                    sTriangle.V1X = v1.X;
                    sTriangle.V1Y = v1.Y;
                    sTriangle.V1Z = v1.Z;
                    sTriangle.V2X = v2.X;
                    sTriangle.V2Y = v2.Y;
                    sTriangle.V2Z = v2.Z;
                    sTriangle.V3X = v3.X;
                    sTriangle.V3Y = v3.Y;
                    sTriangle.V3Z = v3.Z;
                    sTriangle.AttributeByteCount = 0;

                    oWriter.Write(abyMemory);
                }
            }
        }

        void DoReadMeshFromAsciiStl(    BinaryReader oReader,
                                        EStlUnit eLoadUnit,
                                        float fPostScale,
                                        Vector3 vecPostOffsetMM)
        {

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SStlTriangle
        {
            public float NormalX;
            public float NormalY;
            public float NormalZ;
            public float V1X;
            public float V1Y;
            public float V1Z;
            public float V2X;
            public float V2Y;
            public float V2Z;
            public float V3X;
            public float V3Y;
            public float V3Z;
            public ushort AttributeByteCount;
        }

        void DoReadMeshFromBinaryStl(   BinaryReader oReader,
                                        EStlUnit eLoadUnit,
                                        float fPostScale,
                                        Vector3 vecPostOffsetMM)
        {
            UInt32 nNumberOfTriangles = oReader.ReadUInt32();

            SStlTriangle sTriangle = new SStlTriangle();
            var oTriangleSpan = MemoryMarshal.CreateSpan(ref sTriangle, 1);

            while (nNumberOfTriangles > 0)
            {
                oReader.Read(MemoryMarshal.AsBytes(oTriangleSpan));

                Vector3 v1 = new Vector3(sTriangle.V1X, sTriangle.V1Y, sTriangle.V1Z);
                Vector3 v2 = new Vector3(sTriangle.V2X, sTriangle.V2Y, sTriangle.V2Z);
                Vector3 v3 = new Vector3(sTriangle.V3X, sTriangle.V3Y, sTriangle.V3Z);

                TransformFromUnit(ref v1, eLoadUnit, fPostScale, vecPostOffsetMM);
                TransformFromUnit(ref v2, eLoadUnit, fPostScale, vecPostOffsetMM);
                TransformFromUnit(ref v3, eLoadUnit, fPostScale, vecPostOffsetMM);

                nAddTriangle(v1, v2, v3);

                nNumberOfTriangles--;
            }
        }

        void TransformToUnit(ref Vector3 v,
                                Vector3 vecPostOffsetMM,
                                float fMultiplier,
                                EStlUnit eUnit)
        {
            float fUnitDivider = fMultiplierFromUnit(eUnit);

            v += vecPostOffsetMM;
            v *= fMultiplier;
            v /= fUnitDivider;
        }

        void TransformFromUnit(ref Vector3 v,
                                EStlUnit eUnit,
                                float fMultiplier,
                                Vector3 vecPostOffsetMM)
        {
            float fUnitDivider = fMultiplierFromUnit(eUnit);

            v *= fUnitDivider;
            v *= fMultiplier;
            v += vecPostOffsetMM;
        }

        float fMultiplierFromUnit(EStlUnit eUnit)
        {
            switch (eUnit)
            {
                case EStlUnit.CM:
                    return 10.0f;
                case EStlUnit.M:
                    return 1000.0f;
                case EStlUnit.FT:
                    return 304.8f;
                case EStlUnit.IN:
                    return 25.4f;
            }

            // nothing to do
            Debug.Assert(eUnit == EStlUnit.AUTO ||
                            eUnit == EStlUnit.MM);
            return 1.0f;
        }

        public string m_strLoadHeaderData = "";
        public EStlUnit m_eLoadUnits = EStlUnit.AUTO;
    }
}
