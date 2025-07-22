﻿//
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


using System.Text;

namespace PicoGK
{
    public partial class Library : IDisposable
    {
        /// <summary>
        /// Returns the library name (from the C++ side)
        /// </summary>
        /// <returns>The name of the dynamically loaded C++ library</returns>
        public static string strName()
        {
            StringBuilder oBuilder = new StringBuilder(Library.nStringLength);
            _GetName(oBuilder);
            return oBuilder.ToString();
        }

        /// <summary>
        /// Returns the library version (from the C++ side)
        /// </summary>
        /// <returns>The library version of the C++ library</returns>
        public static string strVersion()
        {
            StringBuilder oBuilder = new StringBuilder(Library.nStringLength);
            _GetVersion(oBuilder);
            return oBuilder.ToString();
        }

        /// <summary>
        /// Returns internal build info, such as build date/time
        /// of the C++ library
        /// </summary>
        /// <returns>Internal build info of the C++ library</returns>
        public static string strBuildInfo()
        {
            StringBuilder oBuilder = new StringBuilder(Library.nStringLength);
            _GetBuildInfo(oBuilder);
            return oBuilder.ToString();
        }
    }
}