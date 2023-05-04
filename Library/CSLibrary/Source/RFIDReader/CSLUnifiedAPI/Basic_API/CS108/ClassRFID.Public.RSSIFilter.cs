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
    public partial class RFIDReader
    {
        public Result SetRSSIFilter_CS108(RSSIFILTERTYPE type)
        {
            UInt32 value = 0;
            UInt32 setting = 0;

            switch (type)
            {
                case RSSIFILTERTYPE.DISABLE:
                    setting = 0;
                    break;

                case RSSIFILTERTYPE.RSSI:
                    setting = 1;
                    break;

                /*case RSSIFILTERTYPE.NB_RSSI:
                    setting = 1;
                    break;*/
            }

            MacReadRegister(MACREGISTER.HST_INV_RSSI_FILTERING_CONFIG, ref value);
            value &= 0xfffffff0;
            value |= (uint)(setting);
            MacWriteRegister(MACREGISTER.HST_INV_RSSI_FILTERING_CONFIG, value);

            return Result.OK;
        }

        public Result SetRSSIFilter_CS108(RSSIFILTERTYPE type, RSSIFILTEROPTION option, double threshold_dbV)
        {
            UInt32 value;
            UInt32 typeSettingValue = 0;
            UInt32 optionSettingValue = 0;

            switch (type)
            {
                case RSSIFILTERTYPE.DISABLE:
                    typeSettingValue = 0;
                    break;

                case RSSIFILTERTYPE.RSSI:
                    typeSettingValue = 1;
                    break;

                    /*                case RSSIFILTERTYPE.RSSI:
                                        typeSettingValue = 2;
                                        break;

                                    case RSSIFILTERTYPE.NB_RSSI:
                                        typeSettingValue = 1;
                                        break;
                    */
            }

            switch (option)
            {
                case RSSIFILTEROPTION.DISABLE:
                    typeSettingValue = 0;
                    optionSettingValue = 0;
                    break;

                case RSSIFILTEROPTION.LESSOREQUAL:
                    optionSettingValue = 0;
                    break;

                case RSSIFILTEROPTION.GREATEROREQUAL:
                    optionSettingValue = 1;
                    break;
            }

            value = (uint)(typeSettingValue) | ((uint)(optionSettingValue) << 4);
            MacWriteRegister(MACREGISTER.HST_INV_RSSI_FILTERING_CONFIG, value);

            value = (uint)encodeNarrowBandRSSI (threshold_dbV);
            MacWriteRegister(MACREGISTER.HST_INV_RSSI_FILTERING_THRESHOLD, value);

            return Result.OK;
        }

        private int encodeNarrowBandRSSI(double dRSSI)
        {
            double dValue = dRSSI / 20;
            dValue = Math.Pow(10, dValue);
            int exponent = 0;

            //if (false) appendToLog("exponent = " + exponent + ", dValue = " + dValue);

            while ((dValue + 0.5) >= 2)
            {
                dValue /= 2; exponent++;
                //if (false) appendToLog("exponent = " + exponent + ", dValue = " + dValue);
            }

            dValue--;

            int mantissa = (int)((dValue * 8) + 0.5);
            int iValue = ((exponent & 0x1F) << 3) | (mantissa & 0x7);
            return iValue;
        }
    }
}
