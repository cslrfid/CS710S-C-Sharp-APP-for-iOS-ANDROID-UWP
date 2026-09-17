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
using CSLibrary.Structures;

namespace CSLibrary
{
    using static FrequencyBand;

    public partial class RFIDReader
    {
        private string m_save_countryname = "";                     // current selected country by name
        private int m_save_countryindex = -1;                       // current selected country by index
        private RegionCode m_save_region_code = RegionCode.UNKNOWN; // current selected country by RegionCode (just for old frequency set)
        private int m_save_freq_channel = -2;                       // current selected channel, start from 0 and -1 = hopping
        private int[] m_save_hoppingorder = null;                   // current hopping channel order (only for selected hopping channel)

        public Result SetDefaultChannel()
        {
            switch (_deviceType)
            {
                case RFIDDEVICE.MODEL.CS108:
                    return SetDefaultChannel_CS108();       // Set Default setting from hard code table

                case RFIDDEVICE.MODEL.CS710S:
                    return SetDefaultChannel_CS710S();      // Get Default setting from reader
            }

            return Result.FAILURE;
        }

    }
}