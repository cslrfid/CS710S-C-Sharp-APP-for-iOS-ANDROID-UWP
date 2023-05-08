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
    using static FrequencyBand_CS710S;

    public partial class RFIDReader
    {
        /// <summary>
        /// If true, it can only set to hopping channel.
        /// </summary>
        internal bool IsHoppingChannelOnly
        {
            get { return m_oem_freq_modification_flag != 0x00; }
        }

        /// <summary>
        /// If true, it can only set to fixed channel.
        /// Otherwise, both fixed and hopping can be set.
        /// </summary>
        internal bool IsFixedChannelOnly_CS108
        {
            get { return (m_save_country_code == 1 | m_save_country_code == 3 | m_save_country_code == 8 | m_save_country_code == 9); }
        }

        /// <summary>
        /// Get Fixed frequency channel
        /// </summary>
        internal bool IsFixedChannel_CS108
        {
            get { { return m_save_fixed_channel; } }
        }

        /// <summary>
        /// GetCountryCode
        /// </summary>
        /// <returns>Result</returns>
        public Result GetCountryCode(ref uint code)
        {
            code = m_save_country_code;

            if (code < 0 || code > 8)
                return Result.INVALID_OEM_COUNTRY_CODE;

            return Result.OK;
        }

        /// <summary>
        /// Available region you can use
        /// </summary>
        public List<RegionCode> GetActiveRegionCode()
        {
            //DEBUG_WriteLine(DEBUGLEVEL.API, "HighLevelInterface.GetActiveRegionCode()");

            return m_save_country_list;
        }

        /// <summary>
        /// Get Region Profile
        /// </summary>
        public RegionCode SelectedRegionCode
        {
            get { return m_save_region_code; }
        }

        // Get Active Country Name List
        public List<string> GetActiveRegionNameList_CS108()
        {
            return m_save_country_list_name;
        }

        public bool IsHopping_CS108(string CountryName)
        {
            var item = FrequencyBand_CS710S.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item == null)
            {
                return false;
            }

            return (item.hopping == "Hop");
        }


        public bool IsFixed_CS108(string CountryName)
        {
            var item = FrequencyBand_CS710S.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item == null)
            {
                return false;
            }

            return (item.hopping == "Fixed");
        }

        public Result SetRegion_CS108(string CountryName, int Channel = -1)                                        // Select Country Frequency with channel if fixed
        {
            var item = FrequencyBand.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item != null)
            {
                if (item.hopping)
                    return SetHoppingChannels(item.code);

                return SetFixedChannel(item.code, (uint)Channel);
            }

            return Result.FAILURE;
        }

        public List<double> GetAvailableFrequencyTable_CS108(string CountryName)									// Get Available frequency table with country code
        {
            var item = FrequencyBand.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item == null)
                return null;

            return (new List<double>(GetAvailableFrequencyTable_CS108(item.code)));
        }

        public List<double> GetCurrentFrequencyTable_CS108()														// Get frequency table on current selected region
        {
            return (new List<double>(GetAvailableFrequencyTable_CS108(m_save_region_code)));
        }

        public string GetCurrentCountry_CS108()
        {
            return FrequencyBand.frequencySet.Find(item => item.code == m_save_region_code).name;
        }
    }
}
