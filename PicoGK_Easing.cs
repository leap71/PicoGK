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

namespace PicoGK
{
    /// <summary>
    /// Easing functions — they take a float value from 0..1 and output
    /// an "eased" curve of the values, also from 0..1
    /// </summary>
    public class Easing
	{
        public static float fEaseSineIn(float x)
        {
            return 1.0f - float.Cos((x* float.Pi) / 2.0f);
        }

        public static float fEaseSineOut(float x)
        {
            return float.Sin((x * float.Pi) / 2.0f);
        }

        public static float fEaseSineInOut(float x)
        {
            return -(float.Cos(float.Pi * x) - 1.0f) / 2.0f;
        }

        public static float fEaseQuadIn(float x)
        {
            return x * x;
        }

        public static float fEaseQuadOut(float x)
        {
            return 1.0f - (1.0f - x) * (1.0f - x);
        }

        public static float fEaseQuadInOut(float x)
        {
            return x < 0.5f ?
                2.0f * x * x :
                1.0f - float.Pow(-2.0f * x + 2.0f, 2.0f) / 2.0f;
        }

        public static float fEaseCubicIn(float x)
        {
            return x * x * x;
        }

        public static float fEaseCubicOut(float x)
        {
            return 1 - float.Pow(1.0f - x, 3.0f);
        }

        public static float fEaseCubicInOut(float x)
		{
            return x < 0.5 ?
                4 * x * x * x :
                1 - float.Pow(-2 * x + 2, 3) / 2;
        }

        public enum EEasing {   LINEAR,
                                SINE_IN,
                                SINE_OUT,
                                SINE_INOUT,
                                QUAD_IN,
                                QUAD_OUT,
                                QUAD_INOUT,
                                CUBIC_IN,
                                CUBIC_OUT,
                                CUBIC_INOUT};

        public static float fEasingFunction(    float x,
                                                EEasing eEasing)
        {
            switch (eEasing)
            {
                case EEasing.LINEAR:
                    return x;
                case EEasing.SINE_IN:
                    return fEaseSineIn(x);
                case EEasing.SINE_OUT:
                    return fEaseSineOut(x);
                case EEasing.SINE_INOUT:
                    return fEaseSineInOut(x);
                case EEasing.QUAD_IN:
                    return fEaseQuadIn(x);
                case EEasing.QUAD_OUT:
                    return fEaseQuadOut(x);
                case EEasing.QUAD_INOUT:
                    return fEaseQuadInOut(x);
                case EEasing.CUBIC_IN:
                    return fEaseCubicIn(x);
                case EEasing.CUBIC_OUT:
                    return fEaseCubicOut(x);
                case EEasing.CUBIC_INOUT:
                    return fEaseCubicInOut(x);
            }

            throw new InvalidOperationException("Unknown easing function - forgot to implement?");
        }
	}
}

