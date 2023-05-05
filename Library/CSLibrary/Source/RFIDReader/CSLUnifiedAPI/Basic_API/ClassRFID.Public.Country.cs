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
        // Get Active Country Name List
        public List<string> GetActiveCountryNameList()
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

        public List<double> GetAvailableFrequencyTable()
        {
            return GetAvailableFrequencyTable(m_save_countryname);
        }

        public List<double> GetAvailableFrequencyTable(string CountryName)									// Get Available frequency table with country code
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

    }
}
