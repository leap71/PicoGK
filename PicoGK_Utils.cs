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

namespace PicoGK
{
    public class Utils
    {
        static string strStripQuotesFromPath(string strPath)
        {
            if (strPath.StartsWith("\"") && strPath.EndsWith("\""))
            {
                return strPath.Substring(1, strPath.Length - 2);
            }

            return strPath;
        }

        /// <summary>
        /// Returns the path to the home folder (cross platform compatible)
        /// </summary>
        /// <returns>Path to the home folder</returns>
        /// <exception cref="Exception">Excepts, if not found</exception>
        static public string strHomeFolder()
        {
            string? str = null;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                str = Environment.GetEnvironmentVariable("HOME");
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                str = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            }

            if (str is null)
            {
                throw new Exception("Could not find home folder for " + Environment.OSVersion.Platform.ToString());
            }

            return str;
        }

        /// <summary>
        /// Returns the path to the documents folder (cross platform compatible)
        /// </summary>
        /// <returns>The path th documents folder</returns>
        /// <exception cref="Exception">Excepts, if unable to find</exception>
        static public string strDocumentsFolder()
        {
            string? str = null;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                str = Environment.GetEnvironmentVariable("HOME");

                if (str is null)
                {
                    throw new Exception("Could not find home folder for " + Environment.OSVersion.Platform.ToString());
                }

                return Path.Combine(str, "Documents");
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                str = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            }

            if (str is null)
            {
                throw new Exception("Could not find home folder for " + Environment.OSVersion.Platform.ToString());
            }

            return str;
        }

        /// <summary>
        /// Returns the path to the source folder of PicoGK, making the following
        /// Assumptions:
        /// - PicoGK is contained in a subfolder named "PicoGK" inside your
        ///   main project, so your path is MyProject/PicoGK
        /// - The executable .NET DLL is in its usual place
        /// This function is used mainly to load the viewer environment and
        /// test files used in the PicoGK examples
        /// </summary>
        /// <returns>The assumed path to the PicoGK source code</returns>
        static public string strPicoGKSourceCodeFolder()
        {
            string strPath = strStripQuotesFromPath(Environment.CommandLine);

            for (int n = 0; n < 4; n++)
            {
                strPath = Path.GetDirectoryName(strPath) ?? "";
            }

            strPath = Path.Combine(strPath, "PicoGK");

            return strPath;     
        }

        /// <summary>
        /// Returns the path in which your current executable resides
        /// </summary>
        /// <returns>The folder in which your executable resides</returns>
        static public string strExecutableFolder()
        {
            string strExePath = System.Reflection.Assembly.GetExecutingAssembly().Location ?? "";
            return System.IO.Path.GetDirectoryName(strExePath) ?? "";
        }

        /// <summary>
        /// Returns a file name in the form 20230930_134500 to be used in log files etc.
        /// </summary>
        /// <param name="strPrefix">Prepended before the date/time stamp</param>
        /// <param name="strPostfix">Appended after the date/time stamp</param>
        /// <returns></returns>
        static public string strDateTimeFilename(   in string strPrefix,
                                                    in string strPostfix)
        {
            return strPrefix + DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss") + strPostfix;
        }

        /// <summary>
        /// Shorted a string, IF it is too long. If it is not too long, nothing happens
        /// C# is lacking such a function (all other functions throw exceptions)
        /// </summary>
        /// <param name="str">String to shorten (if too long)</param>
        /// <param name="iMaxCharacters">Number of max characters in the string</param>
        /// <returns></returns>
        public static string strShorten(string str, int iMaxCharacters)
        {
            if (str.Length < iMaxCharacters)
                return str;

            // in their infinite wisdom, the coders of C# decided to
            // throw an exception, if str is shorter than iMaxCharacters
            // that's why this function is even necessary
            return str[..iMaxCharacters];
        }

        static public void SetMatrixRow(    ref Matrix4x4 mat, uint n,
                                            float f1, float f2, float f3, float f4)
        {
            // An insane person wrote Matrix4x4

            switch (n)
            {
                case 0:
                    mat.M11 = f1;
                    mat.M12 = f2;
                    mat.M13 = f3;
                    mat.M14 = f4;
                    break;
                case 1:
                    mat.M21 = f1;
                    mat.M22 = f2;
                    mat.M23 = f3;
                    mat.M24 = f4;
                    break;
                case 2:
                    mat.M31 = f1;
                    mat.M32 = f2;
                    mat.M33 = f3;
                    mat.M34 = f4;
                    break;
                case 3:
                    mat.M41 = f1;
                    mat.M42 = f2;
                    mat.M43 = f3;
                    mat.M44 = f4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Matrix 4x4 row index i 0..3");
            }
        }

        static public Matrix4x4 matLookAt(  Vector3 vecEye,
                                            Vector3 vecLookAt)
        {
            Vector3 vecZ = new(0.0f, 0.0f, 1.0f);

            Vector3 vecView = Vector3.Normalize(vecEye - vecLookAt);
            Vector3 vecRight = Vector3.Normalize(Vector3.Cross(vecZ, vecView));
            Vector3 vecUp = Vector3.Cross(vecView, vecRight);

            Matrix4x4 mat = new Matrix4x4();

            SetMatrixRow(ref mat, 0, vecRight.X, vecUp.X, vecView.X, 0f);
            SetMatrixRow(ref mat, 1, vecRight.Y, vecUp.Y, vecView.Y, 0f);
            SetMatrixRow(ref mat, 2, vecRight.Z, vecUp.Z, vecView.Z, 0f);

            SetMatrixRow(ref mat, 3, -Vector3.Dot(vecRight, vecEye),
                               -Vector3.Dot(vecUp, vecEye),
                               -Vector3.Dot(vecView, vecEye),
                               1.0f);

            return mat;
        }

        static public Mesh mshCreateCube(   Vector3? vecScale       = null,
                                            Vector3? vecOffsetMM    = null)
        {
            Vector3 vecS        = vecScale      ?? new Vector3(1.0f);
            Vector3 vecOffset   = vecOffsetMM   ?? new Vector3(0.0f);

            Mesh oMesh = new Mesh();

            Vector3[] cubeVertices =
            {
                new Vector3(-0.5f * vecS.X, -0.5f * vecS.Y, -0.5f * vecS.Z) + vecOffset,
                new Vector3(-0.5f * vecS.X, -0.5f * vecS.Y,  0.5f * vecS.Z) + vecOffset,
                new Vector3(-0.5f * vecS.X,  0.5f * vecS.Y, -0.5f * vecS.Z) + vecOffset,
                new Vector3(-0.5f * vecS.X,  0.5f * vecS.Y,  0.5f * vecS.Z) + vecOffset,
                new Vector3( 0.5f * vecS.X, -0.5f * vecS.Y, -0.5f * vecS.Z) + vecOffset,
                new Vector3( 0.5f * vecS.X, -0.5f * vecS.Y,  0.5f * vecS.Z) + vecOffset,
                new Vector3( 0.5f * vecS.X,  0.5f * vecS.Y, -0.5f * vecS.Z) + vecOffset,
                new Vector3( 0.5f * vecS.X,  0.5f * vecS.Y,  0.5f * vecS.Z) + vecOffset
            };

            // Front face
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[1], cubeVertices[3]);
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[3], cubeVertices[2]);

            // Back face
            oMesh.nAddTriangle(cubeVertices[4], cubeVertices[6], cubeVertices[7]);
            oMesh.nAddTriangle(cubeVertices[4], cubeVertices[7], cubeVertices[5]);

            // Left face
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[2], cubeVertices[6]);
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[6], cubeVertices[4]);

            // Right face
            oMesh.nAddTriangle(cubeVertices[1], cubeVertices[5], cubeVertices[7]);
            oMesh.nAddTriangle(cubeVertices[1], cubeVertices[7], cubeVertices[3]);

            // Top face
            oMesh.nAddTriangle(cubeVertices[2], cubeVertices[3], cubeVertices[7]);
            oMesh.nAddTriangle(cubeVertices[2], cubeVertices[7], cubeVertices[6]);

            // Bottom face
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[4], cubeVertices[5]);
            oMesh.nAddTriangle(cubeVertices[0], cubeVertices[5], cubeVertices[1]);

            return oMesh;
        }

        static public Mesh mshCreateCylinder( Vector3? vecScale     = null,
                                              Vector3? vecOffsetMM  = null,
                                              int iSides = 0)
        {
            Vector3 vecS        = vecScale ?? new Vector3(1.0f);
            Vector3 vecOffset   = vecOffsetMM ?? new Vector3(0.0f);

            float fA = vecS.X * 0.5f;
            float fB = vecS.Y * 0.5f;

            if (iSides <= 0)
            {
                //Ramanujan's ellipse perimeter
                //P ≈ π [ 3 (a + b) - √[(3a + b) (a + 3b) ]]
                //P ≈ π(a + b) [ 1 + (3h) / (10 + √(4 - 3h) ) ], where h = (a - b)2/(a + b)2

                float fP    = MathF.PI * (3.0f * (fA + fB) - MathF.Sqrt((3.0f * fA + fB) * (fA + 3.0f * fB)));
                iSides      = 2 * (int)MathF.Ceiling(fP);
            }

            if (iSides < 3)
            {
                iSides = 3;
            }

            Mesh oMesh = new Mesh();

            Vector3 vecBottomCenter = vecOffset;
            vecBottomCenter.Z      -= vecS.Z * 0.5f;
            Vector3 vecTopCenter    = vecBottomCenter;
            vecTopCenter.Z         += vecS.Z;
            Vector3 vecPrevBottom   = new Vector3(fA, 0, 0) + vecBottomCenter;
            Vector3 vecPrevTop      = vecPrevBottom;
            vecPrevTop.Z           += vecS.Z;

            float fStep             = MathF.PI * 2.0f / iSides;

            for (int i = 1; i <= iSides; ++i)
            {
                float fAngle = i * fStep;

                Vector3 vecThisBottom   = new Vector3(MathF.Cos(fAngle) * fA, MathF.Sin(fAngle) * fB, 0.0f) + vecBottomCenter;
                Vector3 vecThisTop      = vecThisBottom;
                vecThisTop.Z           += vecS.Z;

                //top cap
                oMesh.nAddTriangle(vecTopCenter, vecPrevTop, vecThisTop);

                //side
                oMesh.nAddTriangle(vecPrevBottom, vecThisBottom, vecPrevTop);
                oMesh.nAddTriangle(vecThisBottom, vecThisTop, vecPrevTop);

                //bottom cap
                oMesh.nAddTriangle(vecBottomCenter, vecThisBottom, vecPrevBottom);

                vecPrevBottom   = vecThisBottom;
                vecPrevTop      = vecThisTop;
            }

            return oMesh;
        }

        static public Mesh mshCreateCone(Vector3? vecScale = null,
                                         Vector3? vecOffsetMM = null,
                                         int iSides = 0)
        {
            Vector3 vecS        = vecScale ?? new Vector3(1.0f);
            Vector3 vecOffset   = vecOffsetMM ?? new Vector3(0.0f);

            float fA = vecS.X * 0.5f;
            float fB = vecS.Y * 0.5f;

            if (iSides <= 0)
            {
                float fP = MathF.PI * (3.0f * (fA + fB) - MathF.Sqrt((3.0f * fA + fB) * (fA + 3.0f * fB)));
                iSides = 2 * (int)MathF.Ceiling(fP);
            }

            if (iSides < 3)
            {
                iSides = 3;
            }

            Mesh oMesh = new Mesh();

            Vector3 vecBottomCenter = vecOffset;
            vecBottomCenter.Z      -= vecS.Z * 0.5f;
            Vector3 vecTop          = vecBottomCenter;
            vecTop.Z               += vecS.Z;
            Vector3 vecPrevBottom   = new Vector3(fA, 0, 0) + vecBottomCenter;

            float fStep = MathF.PI * 2.0f / iSides;

            for (int i = 1; i <= iSides; ++i)
            {
                float fAngle = i * fStep;

                Vector3 vecThisBottom = new Vector3(MathF.Cos(fAngle) * fA, MathF.Sin(fAngle) * fB, 0.0f) + vecBottomCenter;

                //side
                oMesh.nAddTriangle(vecPrevBottom, vecThisBottom, vecTop);
   
                //bottom cap
                oMesh.nAddTriangle(vecBottomCenter, vecThisBottom, vecPrevBottom);

                vecPrevBottom = vecThisBottom;
            }

            return oMesh;
        }

        static void GeoSphereTriangle(Vector3 vecA,
					                  Vector3 vecB,
					                  Vector3 vecC,
					                  Vector3 vecOffset,
					                  Vector3 vecRadii,
                                      int iRecursionDepth,
					                  Mesh oTarget)
        {
	        if (iRecursionDepth > 0)
	        {
		        Vector3 vecAB = vecOffset + ((vecA + vecB) * 0.5f - vecOffset);
                Vector3 vecBC = vecOffset + ((vecB + vecC) * 0.5f - vecOffset);
                Vector3 vecCA = vecOffset + ((vecC + vecA) * 0.5f - vecOffset);

                vecAB *= vecRadii / vecAB.Length();
                vecBC *= vecRadii / vecBC.Length();
                vecCA *= vecRadii / vecCA.Length();

                GeoSphereTriangle(vecA, vecAB, vecCA, vecOffset, vecRadii, iRecursionDepth - 1, oTarget);
                GeoSphereTriangle(vecAB, vecB, vecBC, vecOffset, vecRadii, iRecursionDepth - 1, oTarget);
                GeoSphereTriangle(vecAB, vecBC, vecCA, vecOffset, vecRadii, iRecursionDepth - 1, oTarget);
                GeoSphereTriangle(vecCA, vecBC, vecC, vecOffset, vecRadii, iRecursionDepth - 1, oTarget);
            }
	        else
	        {
                oTarget.nAddTriangle(vecA, vecB, vecC);
            }
        }

        static float fSquared(float fX)
        {
            return fX * fX;
        }

        static float fApproxEllipsoidSurfaceArea(Vector3 vecABC)
        {
            return 4.0f * MathF.PI * MathF.Pow((
                MathF.Pow(vecABC.X * vecABC.Y, 1.6f) +
                MathF.Pow(vecABC.Y * vecABC.Z, 1.6f) +
                MathF.Pow(vecABC.Z * vecABC.X, 1.6f)) / 3.0f, 1.0f / 1.6f);
        }

        static public Mesh mshCreateGeoSphere(Vector3? vecScale = null,
                                              Vector3? vecOffsetMM = null,
                                              int iSubdivisions = 0)
        {
            Vector3 vecS        = vecScale ?? new Vector3(1.0f);
            Vector3 vecOffset   = vecOffsetMM ?? new Vector3(0.0f);

            Mesh oMesh          = new Mesh();

            Vector3 vecRadii    = vecS * 0.5f;
            Vector3 vecRadii2   = vecRadii * vecRadii;

            float fCoeff        = fSquared(2.0f * MathF.Sin(MathF.PI * 0.2f));
            Vector3 vecPenta    = new Vector3(
                (2.0f * MathF.Sqrt(fCoeff * vecRadii2.X - vecRadii2.X)) / fCoeff,
                (2.0f * MathF.Sqrt(fCoeff * vecRadii2.Y - vecRadii2.Y)) / fCoeff,
                (2.0f * MathF.Sqrt(fCoeff * vecRadii2.Z - vecRadii2.Z)) / fCoeff);

            float fPentaDZ      = MathF.Sqrt(vecRadii2.Z - fSquared(vecPenta.Z));

            Vector3[] avecPOffs = new Vector3[5];

            for (int i = 0; i < 5; i++)
            {
                float fAngle = 0.4f * MathF.PI * i;
                avecPOffs[i] = new Vector3(vecPenta.X * MathF.Cos(fAngle), vecPenta.Y * MathF.Sin(fAngle), fPentaDZ);
            }

            //estimate the number of subdivisions based on the sphere or ellipsoid surface area
            
            if (iSubdivisions <= 0)
            {
                int iTargetTriangles = (int)MathF.Ceiling(fApproxEllipsoidSurfaceArea(vecRadii));

                iSubdivisions = 1;
                int iTriangles = 80;

                while (iSubdivisions < 8 && iTriangles < iTargetTriangles)
                {
                    ++iSubdivisions;
                    iTriangles = 20 * (1 << (2 * iSubdivisions));
                }
            }
            
            //top cap
            Vector3 vecCap = vecOffset;

            vecCap.Z      += vecRadii.Z;

            for (int i = 0; i < 5; i++)
            {
                GeoSphereTriangle(vecCap, vecOffset + avecPOffs[i], vecOffset + avecPOffs[(i + 1) % 5], vecOffset, vecRadii, iSubdivisions, oMesh);
            }

            //10 triangles around
            GeoSphereTriangle(vecOffset + avecPOffs[4], vecOffset - avecPOffs[2], vecOffset + avecPOffs[0], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[4], vecOffset - avecPOffs[1], vecOffset - avecPOffs[2], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[3], vecOffset - avecPOffs[1], vecOffset + avecPOffs[4], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[3], vecOffset - avecPOffs[0], vecOffset - avecPOffs[1], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[2], vecOffset - avecPOffs[0], vecOffset + avecPOffs[3], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[2], vecOffset - avecPOffs[4], vecOffset - avecPOffs[0], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[1], vecOffset - avecPOffs[4], vecOffset + avecPOffs[2], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[1], vecOffset - avecPOffs[3], vecOffset - avecPOffs[4], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[0], vecOffset - avecPOffs[3], vecOffset + avecPOffs[1], vecOffset, vecRadii, iSubdivisions, oMesh);
            GeoSphereTriangle(vecOffset + avecPOffs[0], vecOffset - avecPOffs[2], vecOffset - avecPOffs[3], vecOffset, vecRadii, iSubdivisions, oMesh);

            //bottom cap
            vecCap.Z = vecOffset.Z - vecRadii.Z;

            for (int i = 0; i < 5; i++)
            {
                GeoSphereTriangle(vecCap, vecOffset - avecPOffs[(i + 1) % 5], vecOffset - avecPOffs[i], vecOffset, vecRadii, iSubdivisions, oMesh);
            }

            return oMesh;
        }

    }

}