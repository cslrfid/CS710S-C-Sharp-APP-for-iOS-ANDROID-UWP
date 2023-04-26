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

namespace CSLibrary
{
    using static RFIDDEVICE;
    using Constants;

    namespace Constants
    {
        public enum RSSIFILTERTYPE
        {
            DISABLE,
            RSSI        // for CS108 = NB_RSSI, for CS710S = DBM_RSSI
        }

        public enum RSSIFILTEROPTION
		{
            DISABLE,
            LESSOREQUAL,
			GREATEROREQUAL,
        }
    }

    public partial class RFIDReader
    {
        public Result SetRSSIFilter(RSSIFILTERTYPE type)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetRSSIFilter_CS108(type);

                case MODEL.CS710S:
                    return SetRSSIFilter_CS710S(type);
            }

            return Result.FAILURE;
        }

        public Result SetRSSIFilter(RSSIFILTERTYPE type, RSSIFILTEROPTION option, UInt16 threshold)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetRSSIFilter_CS108(type, option, threshold);

                //case MODEL.CS710S:
                //    return SetRSSIFilter_CS710S(type, option, threshold);
            }

            return Result.FAILURE;
        }

        /// <summary>
        /// RSSI dBm Filter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="option"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public Result SetRSSIFilter(RSSIFILTERTYPE type, RSSIFILTEROPTION option, double threshold)
        {
            switch(_deviceType)
            {
                case MODEL.CS108:
                    return SetRSSIFilter_CS108(type, option, threshold);

                case MODEL.CS710S:
                    return SetRSSIFilter_CS710S(type, option, threshold);
            }

            return Result.FAILURE;
        }

        /// <summary>
        /// RSSI dBm Filter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="option"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public Result SetRSSIFilter(RSSIFILTEROPTION option, Int16 threshold)
        {
            switch (_deviceType)
            {
                //case MODEL.CS108:
                //    return SetRSSIFilter_CS108(option, threshold);

                case MODEL.CS710S:
                    return SetRSSIFilter_CS710S(option, threshold);
            }

            return Result.FAILURE;
        }

    }
}
