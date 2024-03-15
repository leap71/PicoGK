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

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace PicoGK
{
    public partial class FieldMetadata
    {
        public enum EType
        {
            UNKNOWN = -1,
            STRING  = 0,
            FLOAT,
            VECTOR
        };

        public int nCount()
        {
            return _nCount(m_hThis);
        }

        public bool bGetNameAt(int nIndex, out string strValueName)
        {
            int nLength = _nNameLengthAt(m_hThis, nIndex) + 1;

            StringBuilder oBuilder = new StringBuilder(nLength);
            if (!_bGetNameAt(m_hThis, nIndex, oBuilder, nLength))
            {
                strValueName = "";
                return false;
            }
                
            strValueName = oBuilder.ToString();
            return true;
        }

        public EType eTypeAt(string strName)
        {
            int iType = _nTypeAt(m_hThis, strName);
            if (iType > (int) EType.VECTOR)
            {
                Debug.Assert(false, "Invalid metadata type returned by PicoGKRuntime");
                return EType.UNKNOWN;
            }

            return (EType) iType;
        }

        public string strTypeAt(string strName)
        {
            return strTypeName(eTypeAt(strName));
        }

        public string strTypeName(EType eType)
        {
            switch (eType)
            {
                case EType.UNKNOWN:
                    return "unknown";
                case EType.STRING:
                    return "string";
                case EType.FLOAT:
                    return "float";
                case EType.VECTOR:
                    return "vector";
            }

            Debug.Assert(false, "Unknown type");
            return "undefined";
        }

        public bool bGetValueAt(    string strFieldName,
                                    out string strValue)
        {
            int nLength = _nStringLengthAt(m_hThis, strFieldName) + 1;

            StringBuilder oBuilder = new StringBuilder(nLength);
            if (!_bGetStringAt(m_hThis, strFieldName, oBuilder, nLength))
            {
                strValue = "";
                return false;
            }
                
            strValue = oBuilder.ToString();
            return true;
        }

        
        public bool bGetValueAt(string strFieldName, out float fValue)
        {
            fValue = 0f;
            return _bGetFloatAt(m_hThis, strFieldName, ref fValue);
        }

        
        public bool bGetValueAt(string strFieldName, out Vector3 vecValue)
        {
            vecValue = Vector3.Zero;
            return _bGetVectorAt(m_hThis, strFieldName, ref vecValue);
        }

        
        public void SetValue(string strFieldName, string strValue)
        {
            _SetStringValue(m_hThis, strFieldName, strValue);
        }
        
        public void SetValue(string strFieldName, float fValue)
        {
            _SetFloatValue(m_hThis, strFieldName, fValue);
        }
        
        public void SetValue(string strFieldName, Vector3 vecValue)
        {
            _SetVectorValue(m_hThis, strFieldName, vecValue);
        }

        
        public void RemoveValue(string strFieldName)
        {
            _RemoveValue(m_hThis, strFieldName);
        }

        public override string? ToString()
        {
            string str = $"Metadata table with {nCount()} items\n";
            for (int nMeta=0; nMeta < nCount(); nMeta++)
            {
                bGetNameAt(nMeta, out string strName);
                str += $"  {strName} ({strTypeAt(strName)}): ";

                switch (eTypeAt(strName))
                {
                    case EType.UNKNOWN:
                        break;
                    case EType.STRING:
                        bGetValueAt(strName, out string strValue);
                        str += "'" + strValue + "'";
                        break;
                    case EType.FLOAT:
                        bGetValueAt(strName, out float fValue);
                        str += fValue.ToString();
                        break;
                    case EType.VECTOR:
                        bGetValueAt(strName, out Vector3 vecValue);
                        str += vecValue.ToString();
                        break;
                }
                str += "\n";
            }

            return str;
        }
    
        /// <summary>
        /// Internal constructor used by the Voxels, ScalarField and VectorField
        /// accessor function. Do not construct FieldMetadata objects by
        /// yourself
        /// </summary>
        /// <param name="hSource">This pointer</param>
        public FieldMetadata(IntPtr hSource)
        {
            m_hThis = hSource;
        }
    }
}