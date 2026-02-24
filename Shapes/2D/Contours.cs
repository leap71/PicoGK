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
using PicoGK.Numerics;

namespace PicoGK.Shapes
{
    // <summary>
    /// Class to represent an circle as a normalized path/contour
    /// </summary>
    public readonly struct Circle : IContour2d
    {
        /// <summary>
        /// Create a Circle contour with radius fR
        /// </summary>
        public Circle(float fR)
        {
            m_fR = fR;
        }

        /// <summary>
        /// Radius of the circle
        /// </summary>
        public float    fR      => m_fR;

        public float fLength => float.Pi * 2 * m_fR;

        public Vector2 vecPtAtT(float t)
        {
            float fAngle = float.Clamp(t, 0, 1) * float.Pi * 2;
            
            return  new (   float.Cos(fAngle) * fR,
                            float.Sin(fAngle) * fR);
        }

        public void PtAtT(  in float t, 
                            out Vector2 vecPt, 
                            out Vector2 vecNormal)
        {
            
            vecPt       = vecPtAtT(t);
            vecNormal   = Vector2.Normalize(vecPt);
        }

        readonly float  m_fR;
    }

    /// <summary>
    /// Class to represent an ellipse as a normalized path/contour
    /// </summary>
    public sealed class Ellipse : IContour2d, ContourSampler2d.ISampleable
    {
        /// <summary>
        /// Constructor using axis A vector and axis B length
        /// </summary>
        /// <param name="vecAxisA">Direction and length of axis A</param>
        /// <param name="fLengthB">Length of axis B (perpendicular to A)</param>
        /// <exception cref="ArgumentException"></exception>
        public Ellipse( Vector2 vecAxisA, 
                        float fLengthB)
        {
            float a = vecAxisA.Length();
            
            if (a == 0) 
                throw new ArgumentException("Major axis cannot be zero.");
            
            m_fPhi = float.Atan2(vecAxisA.Y, vecAxisA.X);
            m_fA = a;
            m_fB = fLengthB;
            m_fCosPhi = float.Cos(m_fPhi);
            m_fSinPhi = float.Sin(m_fPhi);

            m_oSampler = new(this);
        }

        /// <summary>
        /// Constructor using semi-major axis length, semi-minor axis length, and rotation angle
        /// </summary>
        /// <param name="a">Length of axis A</param>
        /// <param name="b">Length of axis B</param>
        /// <param name="fAngle">Rotation angle in radians</param>
        public Ellipse( float a, 
                        float b, 
                        float fAngle=0)
        {
            m_fA = a;
            m_fB = b;
            m_fPhi      = fAngle;
            m_fCosPhi   = float.Cos(fAngle);
            m_fSinPhi   = float.Sin(fAngle);

            m_oSampler = new(this);
        }

        /// <summary>
        /// Rotation angle of the ellipse
        /// </summary>
        public float fPhi => m_fPhi;

        /// <summary>
        /// Half-length of the ellipse in A
        /// </summary>
        public float fA => m_fA;

        /// <summary>
        /// Half-length of the ellipse in B
        /// </summary>
        public float fB => m_fB;

        public float fLength => m_oSampler.fTotalLength;

        public Vector2 vecPtAtTLinear(float t)
        {
            float theta = float.Clamp(t, 0f, 1f) * 2f * float.Pi;
            float x = m_fA * float.Cos(theta);
            float y = m_fB * float.Sin(theta);
            
            return new Vector2( x * m_fCosPhi - y * m_fSinPhi, 
                                x * m_fSinPhi + y * m_fCosPhi);
        }

        public Vector2 vecPtAtT(float t)
        {
            return vecPtAtTLinear(m_oSampler.fArcTFromLinearT(t));
        }

        readonly ContourSampler2d   m_oSampler;
        readonly float              m_fPhi;
        readonly float              m_fA;
        readonly float              m_fB;
        readonly float              m_fCosPhi;
        readonly float              m_fSinPhi;
    }

    /// <summary>
    /// Implements the supershape formula for interesting 2D contours
    /// </summary>
    public sealed class Supershape : IContour2d, ContourSampler2d.ISampleable
    {

        /// <summary>
        /// Helper function to create simple rounded polygons based on the Supershape
        /// https://en.wikipedia.org/wiki/Superformula
        /// </summary>
        /// <param name="fRadius">Radius</param>
        /// <param name="fPolySymmery">Number of polygonal sides, for example 4 for a square</param>
        /// <param name="fOutwardCurve">Higher values curve the sides outwards, smaller values create lobes</param>
        /// <param name="fRoundness">Roundness factor, higher values approach a circle</param>
        /// <returns></returns>
        static public Supershape oRoundedPolygon(   float fRadius, 
                                                    float fPolySymmery, 
                                                    float fOutwardCurve =5f,
                                                    float fRoundness=5f)
        {
            return new Supershape(  fRadius, fRadius, 
                                    fPolySymmery, 
                                    fOutwardCurve, 
                                    fRoundness,fRoundness);
        }
        
         /// <summary>
        /// Constructor for a supershape with superformula parameters and rotation.
        /// </summary>
        /// <param name="a">Scaling factor along x (similar to semi-major axis)</param>
        /// <param name="b">Scaling factor along y (similar to semi-minor axis)</param>
        /// <param name="m">Symmetry parameter (number of lobes)</param>
        /// <param name="n1">Exponent for overall sharpness</param>
        /// <param name="n2">Exponent for cosine term</param>
        /// <param name="n3">Exponent for sine term</param>
        /// <param name="fAngle">Rotation angle in radians</param>
        public Supershape(  float a, 
                            float b, 
                            float m, 
                            float n1, 
                            float n2, 
                            float n3, 
                            float fAngle = 0)
        {
            if (a <= 0 || b <= 0) 
                throw new ArgumentException("Scaling factors must be positive.");

            if (n1 <= 0) 
                throw new ArgumentException("n1 must be positive to avoid singularities.");

            m_fA = a;
            m_fB = b;
            m_fM = m;
            m_fN1 = n1;
            m_fN2 = n2;
            m_fN3 = n3;
            m_fCosPhi = float.Cos(fAngle);
            m_fSinPhi = float.Sin(fAngle);

             m_oSampler = new(this);
        }

        public Vector2 vecPtAtT(float t)
        {
            return vecPtAtTLinear(m_oSampler.fArcTFromLinearT(t));
        }

        public Vector2 vecPtAtTLinear(float t)
        {
            t = float.Clamp(t, 0f, 1f);
            float fTheta = t * 2f * float.Pi;
            
            float r = fSuperformula(fTheta);
            Vector2 pt = vecPolarToCartesian(r, fTheta);
            
            return new Vector2( pt.X * m_fCosPhi - pt.Y * m_fSinPhi,
                                pt.X * m_fSinPhi + pt.Y * m_fCosPhi);
        }

        public float fLength => m_oSampler.fTotalLength;

        Vector2 vecPolarToCartesian(float r, float theta) =>
            new Vector2(m_fA * r * float.Cos(theta), m_fB * r * float.Sin(theta));

        float fSuperformula(float fTheta)
        {
            float fAngleM   = m_fM * fTheta / 4f;
            float fCosTerm  = float.Abs(float.Cos(fAngleM));
            float fSinTerm  = float.Abs(float.Sin(fAngleM));
            float fDenom    = float.Pow(fCosTerm, m_fN2) + float.Pow(fSinTerm, m_fN3);
           
            return float.Pow(fDenom, -1f / m_fN1);
        }

        readonly ContourSampler2d   m_oSampler;
        readonly float              m_fA;
        readonly float              m_fB;
        readonly float              m_fM;
        readonly float              m_fN1;
        readonly float              m_fN2;
        readonly float              m_fN3;
        readonly float              m_fCosPhi;
        readonly float              m_fSinPhi;
    }

    /// <summary>
    /// This class allows you to use a closed path as a contour
    /// </summary>
    public sealed class ContourFromPath : IContour2d, ContourSampler2d.ISampleable
    {
        /// <summary>
        /// Create a IContour2d-compatible contour from an existing closed path
        /// The first point in the path and the last point in the path need to
        /// be identical
        /// </summary>
        public ContourFromPath(IPath2d xPath)
        {
            // Ensure the path was closed
            if (!xPath.vecPtAtT(0).bAlmostEqual(xPath.vecPtAtT(1)))
                throw new Exception("Path is not closed, cannot create contour");

            m_xPath     = xPath;
            m_oSampler  = new(this);
        }

        public float fLength => m_xPath.fLength;

        public Vector2 vecPtAtT(float t)
        {
            return vecPtAtTLinear(m_oSampler.fArcTFromLinearT(t));
        }

        public Vector2 vecPtAtTLinear(float t)
        {
            return m_xPath.vecPtAtT(t);
        }

        readonly IPath2d            m_xPath;
        readonly ContourSampler2d   m_oSampler;
    }
}
