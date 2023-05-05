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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSLibrary.Constants;

namespace CSLibrary
{
    using static FrequencyBand;
    using static FrequencyBand_CS710S;

    public partial class RFIDReader
    {
        internal string GetCurrentSubModulName()
        {
            /*
                        byte[] b = new byte[] { (byte)(m_oem_special_country_version >> 24),
                                              (byte)(m_oem_special_country_version >> 16),
                                              (byte)(m_oem_special_country_version >> 8),
                                              (byte)(m_oem_special_country_version) };

                        string subsubModel = System.Text.Encoding.UTF8.GetString(b);
            */
            string subsubModel = "";

            switch (m_oem_special_country_version)
            {
                case 0x2A2A5257:
                    subsubModel = " RW";
                    break;
                case 0x4f464341:
                    subsubModel = " OFCA";
                    break;
                case 0x2a2a4153:
                    subsubModel = " AS";
                    break;
                case 0x2a2a4e5a:
                    subsubModel = " NZ";
                    break;
                case 0x20937846:
                    subsubModel = " ZA";
                    break;
                case 0x2A2A5347:
                    subsubModel = " SG";
                    break;
                case 0x2A4A5036:
                    subsubModel = " JP6";
                    break;
            }

            string SubModel = string.Format("-{0}", m_save_country_code, subsubModel);

            return SubModel;
        }



        // Get Active Country Name List
        public List<string> GetActiveRegionNameList_CS710S()
        {
            string SubModel = GetCurrentSubModulName();

            List<string> ActiveCountryNameList = new List<string>();

            foreach (var i in FrequencyBand_CS710S.frequencySet.FindAll(item => item.modelCode.Equals(SubModel)))
                ActiveCountryNameList.Add(i.name);

            return ActiveCountryNameList;
        }

        public bool IsHopping_CS710S(string CountryName)
        {
            var item = FrequencyBand_CS710S.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item == null)
            {
                return false;
            }

            return (item.hopping == "Hop");
        }

        public bool IsHopping_CS710S(int index)
        {
            return (FrequencyBand_CS710S.frequencySet[index].hopping == "Hop");
        }

        public bool IsFixed_CS710S(string CountryName)
        {
            var item = FrequencyBand_CS710S.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item == null)
            {
                return false;
            }

            return (item.hopping == "Fixed");
        }

        public bool IsFixed_CS710S(int index)
        {
            return (FrequencyBand_CS710S.frequencySet[index].hopping == "Fixed");
        }

        internal Result SetRegion_CS710S(string CountryName, int Channel = 1)                                        // Select Country Frequency with channel if fixed
        {
            var item = FrequencyBand_CS710S.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item == null)
                return Result.FAILURE;

            RFIDRegister.CountryEnum.Set((UInt16)item.index);

            if (IsFixed_CS710S(item.index))
            {
                RFIDRegister.FrequencyChannelIndex.Set((byte)Channel);
                return Result.OK;
            }

            if (IsHopping_CS710S(item.index))
                return Result.OK;

            return Result.FAILURE;
        }

        public List<double> GetAvailableFrequencyTable_CS710S(string CountryName)									// Get Available frequency table with country code
        {
            var item = FrequencyBand_CS710S.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item == null)
                return null;

            return GetAvailableFrequencyTable_CS710S(item.index);
        }

        internal List<double> GetAvailableFrequencyTable_CS710S(int index)									// Get Available frequency table with country code
        {
            List<double> FreqTable = new List<double>();
            double firstChannel = FrequencyBand_CS710S.frequencySet[index].firstChannel;
            double lastChannel = FrequencyBand_CS710S.frequencySet[index].lastChannel + 0.1; // Fix double value compare error (C# bug)
            double totalFrequencyChannel = FrequencyBand_CS710S.frequencySet[index].totalFrequencyChannel;
            double channelSepatration = (double)(FrequencyBand_CS710S.frequencySet[index].channelSepatration) / 1000;
            double freq;

            for (freq = firstChannel; 
                freq <= lastChannel && FreqTable.Count <= totalFrequencyChannel; 
                freq += channelSepatration)
                FreqTable.Add(freq);

            return FreqTable;
        }

        public List<double> GetCurrentFrequencyTable_CS710S()														// Get frequency table on current selected region
        {
            //return GetAvailableFrequencyTable_CS710S(1m_save_region);

            return null;
        }

        public string GetCurrentCountry_CS710S()
        {
            return FrequencyBand_CS710S.frequencySet[RFIDRegister.CountryEnum.Get()].name;
        }

        public int GetCurrentCountryIndex_CS710S()
        {
            return RFIDRegister.CountryEnum.Get();
        }

        int GetCurrentFrequencyChannel_CS710S()
        {
            return RFIDRegister.FrequencyChannelIndex.Get();
        }
    }
}
