/*
Copyright (c) 2018 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSLibrary.Constants;

namespace CSLibrary
{
    using static FrequencyBand;

    public partial class RFIDReader
    {
        private uint m_oem_country_code = 0;
        private uint m_oem_special_country_version;
        private uint m_oem_freq_modification_flag;
        private List<RegionCode> m_save_country_list = new List<RegionCode>();  // for CS108 API only
        private List<string> m_save_country_list_name = new List<string>();

        private uint GetOEMCountryCode
        {
            get
            {
                uint dataBuf = 0xff;

                //m_Result = MacReadOemData(0x2, ref dataBuf);
                if (m_Result != Result.OK)
                    return 0;

                return dataBuf;
            }
        }


        private void GenCountryList()
        {
            m_save_country_list.Clear();
            m_save_country_list_name.Clear();

            string Model = GetModelCountry();

            switch (Model)
            {
                case "-1":
                    m_save_country_list.Add(RegionCode.ETSI);
                    m_save_country_list.Add(RegionCode.IN);
                    m_save_country_list.Add(RegionCode.G800);
                    break;

                case "-2":
                    m_save_country_list.Add(RegionCode.FCC);
                    break;

                case "-2 RW":
                    m_save_country_list.Add(RegionCode.AR);
                    m_save_country_list.Add(RegionCode.AU);
                    m_save_country_list.Add(RegionCode.BA);
                    m_save_country_list.Add(RegionCode.BR1);
                    m_save_country_list.Add(RegionCode.BR2);
                    m_save_country_list.Add(RegionCode.BR3);
                    m_save_country_list.Add(RegionCode.BR4);
                    m_save_country_list.Add(RegionCode.BR5);
                    m_save_country_list.Add(RegionCode.CL);
                    m_save_country_list.Add(RegionCode.CO);
                    m_save_country_list.Add(RegionCode.CR);
                    m_save_country_list.Add(RegionCode.DO);
                    m_save_country_list.Add(RegionCode.FCC);
                    m_save_country_list.Add(RegionCode.HK);
                    m_save_country_list.Add(RegionCode.ID);
                    m_save_country_list.Add(RegionCode.JE);  // 915-917 MHz
                    m_save_country_list.Add(RegionCode.KR);
                    m_save_country_list.Add(RegionCode.MY);
                    m_save_country_list.Add(RegionCode.PA);
                    m_save_country_list.Add(RegionCode.PE);
                    m_save_country_list.Add(RegionCode.PH);  // 918-920 MHz
                    m_save_country_list.Add(RegionCode.SG);
                    m_save_country_list.Add(RegionCode.TH);
                    m_save_country_list.Add(RegionCode.UY);
                    m_save_country_list.Add(RegionCode.VE);
                    m_save_country_list.Add(RegionCode.VI);
                    m_save_country_list.Add(RegionCode.ZA);
                    m_save_country_list.Add(RegionCode.LH1);  // 
                    m_save_country_list.Add(RegionCode.LH2);  // 
                    m_save_country_list.Add(RegionCode.UH1); // 915-920 MHz
                    m_save_country_list.Add(RegionCode.UH2); // 920-928 MHz
                    break;

                case "-2 OFCA":
                    m_save_country_list.Add(RegionCode.HK);
                    break;

                case "-2 AS":
                    m_save_country_list.Add(RegionCode.AU);
                    break;

                case "-2 NZ":
                    m_save_country_list.Add(RegionCode.NZ);
                    break;

                case "-2 ZA":
                    m_save_country_list.Add(RegionCode.ZA);
                    break;

                case "-2 SG":
                    m_save_country_list.Add(RegionCode.SG);
                    break;

                case "-2 TH":
                    m_save_country_list.Add(RegionCode.TH);
                    break;

                case "-4":
                    m_save_country_list.Add(RegionCode.AU);
                    m_save_country_list.Add(RegionCode.CN);
                    m_save_country_list.Add(RegionCode.HK);
                    m_save_country_list.Add(RegionCode.ID);
                    m_save_country_list.Add(RegionCode.MY);
                    m_save_country_list.Add(RegionCode.SG);
                    m_save_country_list.Add(RegionCode.TW);
                    break;

                case "-6":
                    m_save_country_list.Add(RegionCode.KR);
                    break;

                case "-7":
                    m_save_country_list.Add(RegionCode.AU);
                    m_save_country_list.Add(RegionCode.CN);
                    m_save_country_list.Add(RegionCode.HK);
                    m_save_country_list.Add(RegionCode.ID);
                    m_save_country_list.Add(RegionCode.MY);
                    m_save_country_list.Add(RegionCode.SG);
                    m_save_country_list.Add(RegionCode.TH);
                    break;

                case "-8 JP4":
                    m_save_country_list.Add(RegionCode.JP);
                    break;

                case "-8 JP6":
                    m_save_country_list.Add(RegionCode.JP);
                    break;

                case "-9":
                    m_save_country_list.Add(RegionCode.ETSIUPPERBAND);
                    m_save_country_list.Add(RegionCode.ZA);
                    break;

                //default:
                    //throw new ReaderException(Result.INVALID_PARAMETER);
            }

            foreach (RegionCode code in m_save_country_list)
                m_save_country_list_name.Add(CSLibrary.FrequencyBand.GetRegionName(code));

            if (m_oem_freq_modification_flag == 0xaa)
                return;

            // add E710 frequency set
            foreach (var i in FrequencyBand_CS710S.frequencySet)
                if (i.modelCode.Equals(Model))
                {
                    var find = m_save_country_list_name.Find(item => item.Equals(i.name));

                    if (find == null)
                        m_save_country_list_name.Add(i.name);
                }
        }
    }
}
