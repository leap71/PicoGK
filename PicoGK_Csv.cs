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
    public class CsvTable
    {
        public CsvTable(  string strFilePath,
                          string strDelimiters = ",")
        {
            using (StreamReader oReader = new StreamReader(strFilePath))
            {
                string? strLine = null;

                bool bFirst = true;

                // Read and process each line in the file
                while ((strLine = oReader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(strLine))
                        continue;

                    string[] astrParts = strLine.Split(strDelimiters);
                    
                    List<string> oColumns = new List<string>();

                    foreach (string str in astrParts)
                    {
                        oColumns.Add(str.Trim());
                    }

                    if (bFirst)
                    {
                        m_oColumnIDs = oColumns;
                        bFirst = false;
                    }
                    else
                    {
                        m_oRows.Add(oColumns);
                    }

                    m_nMaxColumnCount = Math.Max(m_nMaxColumnCount, oColumns.Count());
                }

                if (m_oColumnIDs is null)
                    throw new FileLoadException($"No content in CSV file {strFilePath}");
            }
        }

        public void Save(    string strFilePath,
                             string strDelimiter = ",")
        {
            using (StreamWriter oWriter = new StreamWriter(strFilePath))
            {
                // Write column headers
                {
                    string strLine = "";
                    foreach (string str in m_oColumnIDs)
                    {
                        strLine += str + strDelimiter;
                    }

                    strLine = strLine.Substring(0, strLine.Length - 1); // trim off trailing delimiter
                    oWriter.WriteLine(strLine);
                }

                foreach (List<string> oColumns in m_oRows)
                {
                    string strLine = "";
                    foreach (string str in oColumns)
                    {
                        strLine += str + strDelimiter;
                    }

                    strLine = strLine.Substring(0, strLine.Length - 1); // trim off trailing delimiter
                    oWriter.WriteLine(strLine);
                }
            }
        }

        public int nRowCount()
        {
            return m_oRows.Count;
        }

        public int nMaxColumnCount()
        {
            return m_nMaxColumnCount;
        }

        public string strGetAt( int nRow,
                                int nColumn)
        {
            if (nRow >= m_oRows.Count)
                return "";

            List<string> oColumns = m_oRows[nRow];
            if (nColumn >= oColumns.Count)
                return "";

            return oColumns[nColumn];
        }

        public void SetKeyColumn(int nColumn)
        {
            m_nKeyColumn = nColumn;
        }

        public bool bGetAt(in string strKey,
                            ref float fVal)
        {
            string str = "";

            if (!bGetAt(strKey, ref str))
                return false;

            float.TryParse(str, out fVal);
            return true;
        }

        public bool bGetAt( in string strKey,
                            ref string strVal)
        {
            string[] astr = strKey.Split(".");

            if (astr.Count() != 2)
            {
                // Expecting "RowName.ColumnName"
                return false;
            }

            int nColumn;
            if (!bFindColumn(   astr[1],
                                out nColumn))
            {
                return false;
            }

            
            foreach (List<string> oColumns in m_oRows)
            {
                if (oColumns.Count <= m_nKeyColumn)
                    continue; // no value in KeyColumn

                if (oColumns[m_nKeyColumn].Equals(astr[0], StringComparison.OrdinalIgnoreCase))
                {
                    if (nColumn < oColumns.Count)
                    {
                        strVal = oColumns[nColumn];
                    }
                   
                    return true;
                }
            }

            return false;
        }

        public bool bFindColumn(    string strColumnName,
                                    out int nColumn)
        {
            // find column
            int i = 0;
            foreach (string str in m_oColumnIDs)
            {
                if (str.Equals(strColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    nColumn = i;
                    return true;
                }
                i++;
            }

            nColumn = -1;
            return false;
        }

        List<string>        m_oColumnIDs;
        List<List<string>>  m_oRows             = new List<List<string>>();
        int                 m_nKeyColumn        = 0;
        int                 m_nMaxColumnCount   = 0;

    } // class PicoGKCsv
} // namespace

