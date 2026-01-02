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

using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace PicoGK
{
    public interface IFieldWithMetadata
    {
        public FieldMetadata oMetaData();
    }

    public partial class Voxels : IFieldWithMetadata
    {
        public FieldMetadata oMetaData() => m_oMetadata;
    }

    public partial class VectorField : IFieldWithMetadata
    {
        public FieldMetadata oMetaData() => m_oMetadata;
    }

    public partial class ScalarField : IFieldWithMetadata
    {
        public FieldMetadata oMetaData() => m_oMetadata;
    }

    /// <summary>
    /// Metadata table containing parameters associated with field types
    /// like Voxels, ScalarFields, VectorFields. The metadata contained in here
    /// is saved and loaded from VDB files.
    /// </summary>
    public partial class FieldMetadata
    {
        /// <summary>
        /// Type of the data items in the metadata table
        /// </summary>
        public enum EType
        {
            UNKNOWN = -1,
            STRING  = 0,
            FLOAT,
            VECTOR
        };

        /// <summary>
        /// Number of items in the metadata table
        /// </summary>
        public int nCount()
        {
            return _nCount(m_hThis);
        }

        /// <summary>
        /// Attempts to retrieve the name of the parameter at
        /// the index supplied
        /// </summary>
        /// <param name="nIndex">Index value of the parameter</param>
        /// <param name="strValueName">Name of the parameter at this
        /// position.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the type of the value with the specified name
        /// </summary>
        /// <param name="strName">Name of the parameter to retrieve</param>
        /// <returns>Type of the parameter</returns>
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

        /// <summary>
        /// Returns the human readable type of the parameter with the
        /// specified name
        /// </summary>
        /// <param name="strName">Name of the parameter</param>
        /// <returns>The human readable name of the type</returns>
        public string strTypeAt(string strName)
        {
            return strTypeName(eTypeAt(strName));
        }

        /// <summary>
        /// Translate the type enum to a string
        /// </summary>
        /// <param name="eType">Type to translate</param>
        /// <returns>Human readable string</returns>
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

        /// <summary>
        /// Try to get the value of a parameter
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="strValue">Value returned</param>
        /// <returns>Returns false if value doesn't exist or has the
        /// wrong type</returns>
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

        /// <summary>
        /// Try to get the value of a parameter
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="fValue">Value returned</param>
        /// <returns>Returns false if value doesn't exist or has the
        /// wrong type</returns>
        public bool bGetValueAt(string strFieldName, out float fValue)
        {
            fValue = 0f;
            return _bGetFloatAt(m_hThis, strFieldName, ref fValue);
        }

        /// <summary>
        /// Try to get the value of a parameter
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="vecValue">Value returned</param>
        /// <returns>Returns false if value doesn't exist or has the
        /// wrong type</returns>
        public bool bGetValueAt(string strFieldName, out Vector3 vecValue)
        {
            vecValue = Vector3.Zero;
            return _bGetVectorAt(m_hThis, strFieldName, ref vecValue);
        }

        /// <summary>
        /// Set string value in the metadata table
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="strValue">Value to set</param>
        public void SetValue(string strFieldName, string strValue)
        {
            GuardInternalFields(strFieldName);
            _SetValue(strFieldName, strValue);
        }

        /// <summary>
        /// Set floating point value in the metadata table
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="fValue">Value to set</param>
        public void SetValue(string strFieldName, float fValue)
        {
            GuardInternalFields(strFieldName);
            _SetValue(strFieldName, fValue);
        }

        /// <summary>
        /// Set a vector value in the metadata table
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="vecValue">Value to set</param>
        public void SetValue(string strFieldName, Vector3 vecValue)
        {
            GuardInternalFields(strFieldName);
            _SetValue(strFieldName, vecValue);
        }

        /// <summary>
        /// Remove a value from the metadata table
        /// </summary>
        /// <param name="strFieldName">Name of the value</param>
        public void RemoveValue(string strFieldName)
        {
            GuardInternalFields(strFieldName);
            _RemoveValue(m_hThis, strFieldName);
        }

        /// <summary>
        /// Converts the contents of the metadata table to a string
        /// </summary>
        /// <returns>String containing info about metadata</returns>
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

        /// <summary>
        /// Set string value in the metadata table (internal, do not use)
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="strValue">Value to set</param>
        internal void _SetValue(string strFieldName, string strValue)
        {
            _SetStringValue(m_hThis, strFieldName, strValue);
        }

        /// <summary>
        /// Set floating point value in the metadata table (internal, do not use)
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="fValue">Value to set</param>
        internal void _SetValue(string strFieldName, float fValue)
        {
            _SetFloatValue(m_hThis, strFieldName, fValue);
        }

        /// <summary>
        /// Set a vector value in the metadata table (internal, do not use)
        /// </summary>
        /// <param name="strFieldName">Name of the parameter</param>
        /// <param name="vecValue">Value to set</param>
        internal void _SetValue(string strFieldName, Vector3 vecValue)
        {
            _SetVectorValue(m_hThis, strFieldName, vecValue);
        }

        /// <summary>
        /// Remove a value from the metadata table (internal, do not use)
        /// </summary>
        /// <param name="strFieldName">Name of the value</param>
        internal void _RemoveValue(string strFieldName)
        {
            _RemoveValue(m_hThis, strFieldName);
        }

        /// <summary>
        /// This function tests whether you are attempting to set internal
        /// metadata fields from your code — this can mess up openvdb and
        /// internal PicoGK functionality
        /// </summary>
        /// <param name="strFieldName">Field name you are trying to set</param>
        /// <exception cref="FieldAccessException"></exception>
        protected void GuardInternalFields(string strFieldName)
        {
            if (strFieldName.StartsWith("PicoGK.", StringComparison.InvariantCultureIgnoreCase))
                throw new FieldAccessException($"Fields starting with 'PicoGK.' are internal - do not set them from your code ('{strFieldName}').");

            if (    strFieldName.Equals("class",   StringComparison.InvariantCultureIgnoreCase) ||
                    strFieldName.Equals("name",    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new FieldAccessException($"Do not set openvdb-internal fields from your code ('{strFieldName}')");
            }

            if (strFieldName.StartsWith("file_", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new FieldAccessException($"Field names starting with file_ are openvdb-internal - do not set from your code ('{strFieldName}')");
            }   
        }
    }
}