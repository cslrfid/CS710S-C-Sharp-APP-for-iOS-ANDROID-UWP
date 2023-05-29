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
        /// is reader can change country frequency
        /// </summary>
        /// <returns></returns>
        public bool IsCountryChangeable()
        {
            return (m_oem_freq_modification_flag != 0xaa);
        }

        /// <summary>
        /// Get Active Country Name List 
        /// </summary>
        /// <returns></returns>
        public string[] GetActiveCountryNameList()
        {
            switch (_deviceType)
            {
                case RFIDDEVICE.MODEL.CS108:
                    return GetActiveRegionNameList_CS108();

                case RFIDDEVICE.MODEL.CS710S:
                    return GetActiveRegionNameList_CS710S();
            }

            return null;
        }

        /// <summary>
        /// Get Current Country Name
        /// </summary>
        /// <returns></returns>
        public string GetCurrentCountry()
        {
            return m_save_countryname;
            // return FrequencyBand.frequencySet[m_oem_country_code].name;
        }

        /// <summary>
        /// Get CurrentCountry Index (A.3.4 CSL Ex10 Country Enum Table)
        /// </summary>
        /// <returns></returns>
        public int GetCurrentCountryIndex()
        {
            return (int)m_save_countryindex;
        }

        /// <summary>
        /// Get current selected frequency channel 
        /// </summary>
        /// <returns>channel number, first channel = 0 and hopping = -1</returns>
        public int GetCurrentFrequencyChannel()
        {
            return m_save_freq_channel;
        }

        /// <summary>
        // Get Available frequency table with country code
        /// </summary>
        /// <param name="CountryName"></param>
        /// <returns>Country index, if not found = -1</returns>
        public int GetCountryIndex(string CountryName)
        {
            var item = FrequencyBand_CS710S.frequencySet.Find(i => i.name.Equals(CountryName));

            if (item == null)
                return -1;

            return (item.index);
        }

        /// <summary>
        /// Get Current Country Available Frequency Table
        /// </summary>
        /// <returns></returns>
        public double[] GetAvailableFrequencyTable()
        {
            if (_deviceType == RFIDDEVICE.MODEL.CS108)
                if (m_save_region_code != RegionCode.UNKNOWN)
                    return GetAvailableFrequencyTable_CS108(m_save_region_code);

            return GetAvailableFrequencyTable(m_save_countryindex);
        }

        /// <summary>
        /// Get Available frequency table with country code
        /// </summary>
        /// <returns></returns>
        public double[] GetAvailableFrequencyTable(string CountryName)
        {
            if (_deviceType == RFIDDEVICE.MODEL.CS108)
            {
                var region = CSLibrary.FrequencyBand.GetRegionCode(CountryName);

                if (region != RegionCode.UNKNOWN)
                    return GetAvailableFrequencyTable_CS108(region);
            }

            int CountryIndex = GetCountryIndex(CountryName);

            if (CountryIndex == -1)
                return null;

            return GetAvailableFrequencyTable_CS710S(CountryIndex);
        }

        /// <summary>
        /// Get frequency table with country index
        /// </summary>
        /// <param name="CountryIndex"></param>
        /// <returns></returns>
        public double[] GetAvailableFrequencyTable(int CountryIndex)
        {
            return GetAvailableFrequencyTable_CS710S(CountryIndex);
        }

        /// <summary>
        /// Is Current Country Hopping Channel
        /// </summary>
        /// <returns></returns>
        public bool IsHoppingChannel()
        {
            if (m_save_region_code != RegionCode.UNKNOWN)
                return HoppingAvalibable(m_save_region_code);

            return IsHoppingChannel((int)m_save_countryindex);
        }

        public bool IsHoppingChannel(string CountryName)
        {
            if (_deviceType == RFIDDEVICE.MODEL.CS108)
            {
                var region = CSLibrary.FrequencyBand.GetRegionCode(CountryName);

                if (region != RegionCode.UNKNOWN)
                    return HoppingAvalibable(region);
            }

            int CountryIndex = GetCountryIndex(CountryName);

            if (CountryIndex == -1)
                return false;

            return IsHoppingChannel(CountryIndex);
        }

        public bool IsHoppingChannel(int CountryIndex)
        {
            return IsHopping_CS710S(CountryIndex);
        }

        /// <summary>
        /// Is Current Country Fixed Channel
        /// </summary>
        /// <returns></returns>
        public bool IsFixedChannel()
        {
            if (m_save_region_code != RegionCode.UNKNOWN)
                return !HoppingAvalibable(m_save_region_code);

            return IsFixedChannel((int)m_save_countryindex);
        }

        public bool IsFixedChannel(string CountryName)
        {
            if (_deviceType == RFIDDEVICE.MODEL.CS108)
            {
                var region = CSLibrary.FrequencyBand.GetRegionCode(CountryName);

                if (region != RegionCode.UNKNOWN)
                    return !HoppingAvalibable(region);
            }

            int CountryIndex = GetCountryIndex(CountryName);

            if (CountryIndex == -1)
                return false;

            return IsFixedChannel(CountryIndex);
        }

        public bool IsFixedChannel(int CountryIndex)
        {
            return IsFixed_CS710S(CountryIndex);
        }

        /// <summary>
        /// Select Country Frequency with channel if fixed
        /// </summary>
        /// <param name="CountryIndex"></param>
        /// <param name="Channel"></param>
        /// <returns></returns>
        public Result SetCountry(string CountryName, int Channel = 0)                                        // Select Country Frequency with channel if fixed
        {
            if (_deviceType == RFIDDEVICE.MODEL.CS108)
            {
                var region = CSLibrary.FrequencyBand.GetRegionCode(CountryName);

                if (region != RegionCode.UNKNOWN)
                {
                    if (IsHopping_CS108(region))
                        return SetHoppingChannels(region);
                    else
                        return SetFixedChannel(region, (uint)Channel);
                }
            }



            int CountryIndex = GetCountryIndex(CountryName);

            if (CountryIndex == -1)
                return Result.FAILURE;

            return SetCountry(CountryIndex, Channel);
        }

        /// <summary>
        /// Select Country Frequency with channel if fixed
        /// </summary>
        /// <param name="CountryIndex"></param>
        /// <param name="Channel"></param>
        /// <returns></returns>
        public Result SetCountry(int CountryIndex, int Channel = 0)
        {
            m_save_countryindex = CountryIndex;
            m_save_freq_channel = Channel;

            switch (_deviceType)
            {
                case RFIDDEVICE.MODEL.CS108:
                    return SetCountry_CS108(CountryIndex, Channel);

                case RFIDDEVICE.MODEL.CS710S:
                    return SetCountry_CS710S(CountryIndex, Channel);
            }

            return Result.FAILURE;
        }





        /*
                // Get Active Country Name List
                public string[] GetActiveCountryNameList()
                {
                    switch (_deviceType)
                    {
                        case RFIDDEVICE.MODEL.CS108:
                            return GetActiveRegionNameList_CS108();

                        case RFIDDEVICE.MODEL.CS710S:
                            return GetActiveRegionNameList_CS710S();
                    }

                    return null;
                }

                public bool IsHoppingChannel()
                {
                    return IsHoppingChannel(m_save_countryname);
                }
                public bool IsFixedChannel()
                {
                    return IsFixedChannel(m_save_countryname);
                }

                public bool IsHoppingChannel(string CountryName)
                {
                    switch (_deviceType)
                    {
                        case RFIDDEVICE.MODEL.CS108:
                            return IsHopping_CS108(CountryName);

                        case RFIDDEVICE.MODEL.CS710S:
                            return IsHopping_CS710S(CountryName);
                    }

                    return false;
                }

                public bool IsFixedChannel(string CountryName)
                {
                    switch (_deviceType)
                    {
                        case RFIDDEVICE.MODEL.CS108:
                            return IsFixed_CS108(CountryName);

                        case RFIDDEVICE.MODEL.CS710S:
                            return IsFixed_CS710S(CountryName);
                    }

                    return false;
                }

                public Result SetCountry(string CountryName, int Channel = 1)                                        // Select Country Frequency with channel if fixed
                {
                    switch (_deviceType)
                    {
                        case RFIDDEVICE.MODEL.CS108:
                            return SetRegion_CS108(CountryName, Channel);

                        case RFIDDEVICE.MODEL.CS710S:
                            return SetRegion_CS710S(CountryName, Channel);
                    }

                    return Result.FAILURE;
                }

                public double[] GetAvailableFrequencyTable()
                {
                    return GetAvailableFrequencyTable(m_save_countryname);
                }

                public double[] GetAvailableFrequencyTable(string CountryName)									// Get Available frequency table with country code
                {
                    switch (_deviceType)
                    {
                        case RFIDDEVICE.MODEL.CS108:
                            return GetAvailableFrequencyTable_CS108(CountryName);

                        case RFIDDEVICE.MODEL.CS710S:
                            return GetAvailableFrequencyTable_CS710S(CountryName);
                    }

                    return null;
                }

                /// <summary>
                /// Get frequency table with country index
                /// </summary>
                /// <param name="CountryIndex"></param>
                /// <returns></returns>
                public double[] GetAvailableFrequencyTable(int CountryIndex)
                {
                    return GetAvailableFrequencyTable_CS710S(CountryIndex);
                }

                public List<double> GetCurrentFrequencyTable()														// Get frequency table on current selected region
                {
                    switch (_deviceType)
                    {
                        case RFIDDEVICE.MODEL.CS108:
                            return GetCurrentFrequencyTable_CS108();

                        case RFIDDEVICE.MODEL.CS710S:
                            return GetCurrentFrequencyTable_CS710S();
                    }

                    return null;
                }
                public string GetCurrentCountry()
                {
                    switch (_deviceType)
                    {
                        case RFIDDEVICE.MODEL.CS108:
                            return GetCurrentCountry_CS108();

                        case RFIDDEVICE.MODEL.CS710S:
                            return GetCurrentCountry_CS710S();
                    }

                    return null;
                }

                public int GetCurrentCountryIndex()
                {
                    switch (_deviceType)
                    {
                        //case RFIDDEVICE.MODEL.CS108:
                        //    return GetCurrentCountryIndex_CS108();

                        case RFIDDEVICE.MODEL.CS710S:
                            return GetCurrentCountryIndex_CS710S();
                    }

                    return -1;
                }

                public int GetCurrentFrequencyChannel()
                {
                    switch (_deviceType)
                    {
                        //case RFIDDEVICE.MODEL.CS108:
                        //    return GetCurrentFrequencyChannel_CS108();

                        case RFIDDEVICE.MODEL.CS710S:
                            return GetCurrentFrequencyChannel_CS710S();
                    }

                    return -1;
                }

                /// <summary>
                /// Select Country Frequency with channel if fixed
                /// </summary>
                /// <param name="CountryIndex"></param>
                /// <param name="Channel"></param>
                /// <returns></returns>
                public Result SetCountry(int CountryIndex, int Channel = 1)                                        
                {
                    switch (_deviceType)
                    {
                        case RFIDDEVICE.MODEL.CS108:
                            return SetCountry_CS108(CountryIndex, Channel);

                        case RFIDDEVICE.MODEL.CS710S:
                            return SetCountry_CS710S(CountryIndex, Channel);
                    }

                    return Result.FAILURE;
                }




        */



    }
}
