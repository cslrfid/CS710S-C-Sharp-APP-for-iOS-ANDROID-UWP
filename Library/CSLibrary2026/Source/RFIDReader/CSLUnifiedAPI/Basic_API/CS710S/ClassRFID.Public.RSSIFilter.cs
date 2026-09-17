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
    using Constants;
    using CSLibrary.Structures;
    using System.Runtime.CompilerServices;

    public partial class RFIDReader
    {
        private RSSIFILTEROPTION _saveOption;

        public Result SetRSSIFilter_CS710S(RSSIFILTERTYPE type)
        {
            if (type == RSSIFILTERTYPE.DISABLE)
            {
                RFIDRegister.RssiFilteringConfig.Set(0x00);
            }
            else
            {
                RFIDRegister.RssiFilteringConfig.Set((byte)_saveOption);
            }

            return Result.OK;
        }

        /// <summary>
        /// RSSI dBm Filter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="option"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        internal Result SetRSSIFilter_CS710S(RSSIFILTERTYPE type, RSSIFILTEROPTION option, double threshold)
        {
            if (type == RSSIFILTERTYPE.DISABLE || option == RSSIFILTEROPTION.DISABLE)
            {
                RFIDRegister.RssiFilteringConfig.Set(0x00);
            }
            else
            {
                Int16 Intvalue = (Int16)(threshold * 100f);
                UInt16 UIntvalue = (UInt16)(Intvalue);

                _saveOption = option;
                RFIDRegister.RssiFilteringConfig.Set((byte)option);
                RFIDRegister.RssiThreshold.Set(UIntvalue);
            }
            return Result.OK;
        }

        internal Result SetAuthenticateConfig_CS710S(bool SenRep, bool IncRepLen, uint CSI, uint MessbitLen)
        {
            // Bit 0: SenRep
            // Bit 1: IncRepLen
            // Bit 9:2: CSI
            // Bit 21:10: Length of message in bits

            UInt32 newvalue = 0x00;

            if (SenRep)
                newvalue |= 0x01;

            if (IncRepLen)
                newvalue |= 0x02;

            CSI = CSI & 0xff;
            newvalue |= (CSI << 2);

            MessbitLen = MessbitLen & 0xfff;
            newvalue |= (MessbitLen << 10);

            if (newvalue == RFIDRegister.AuthenticateConfig.Get())
                return Result.OK;

            RFIDRegister.AuthenticateConfig.Set(newvalue);
            return Result.OK;
        }

        internal Result SetAuthenticateMessage(byte [] value)
        {
            if (value.Length > 32)
                return Result.INVALID_PARAMETER;

            byte [] oldValue = RFIDRegister.AuthenticateMessage.Get();
            byte [] newValue;

            if (value.Length == 32)
                newValue = value;
            else
            {
                int i = 0;
                newValue = new byte[32];

                for (; i < value.Length; i++)
                    newValue[i] = value[i];
                for (; i < 32; i++)
                    newValue[i] = 0x00;
            }

            RFIDRegister.AuthenticateMessage.Set(newValue);

            return Result.OK;
        }

        internal Result SetAuthenticateResponseLen(UInt16 value)
        {
            RFIDRegister.AuthenticateResponseLen.Set(value);

            return Result.OK;
        }

    }
}
