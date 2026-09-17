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
    using static RFIDDEVICE;

    public partial class RFIDReader
    {
        uint m_oem_table_version;

        // 0x02 = Country Code
        // 0x04 = PCB assembly Code(0)
        // 0x05 = PCB assembly Code(1)
        // 0x06 = PCB assembly Code(2)
        // 0x07 = PCB assembly Code(3)
        // 0x0B = OEM table version
        // 0x8E = Special Country Version
        // 0x8F = Frequency Modification Flag
        // 0x9D = 0: Hopping, 1: non-Hopping
        // 0xA3 = 0: Low Power Mode, 1: High Power Mode (allow 32dBm)
        // 0xA4 = Model Code
        // 0xA5 = Max Output Power
        //        static readonly UInt32[] oemAddress = new UInt32[] { 0x02, 0x04, 0x05, 0x06, 0x07, 0x0B, 0x8E, 0x8F, 0x9D, 0xA3, 0xA4, 0xA5 };

        static readonly UInt32[] oemAddress = new UInt32[] { 0x02, 0x0B, 0x8E, 0x8F, 0x9D, 0xA3, 0xA4, 0x04, 0x05, 0x06, 0x07, 0xA5 };
        UInt32[] oemValue = new UInt32[oemAddress.Length];

        void ReadReaderOEMData()
        {
            for (int cnt = 0; cnt < oemAddress.Length; cnt++)
            {
                MacReadOemData(oemAddress[cnt], ref oemValue[cnt]);
            }
        }

        bool StoreOEMData(UInt32 address, UInt32 value)
        {
            int cnt;

            for (cnt = 0; cnt < oemAddress.Length; cnt++)
            {
                if (oemAddress[cnt] == address)
                {
                    oemValue[cnt] = value;
                    break;
                }
            }

            if (cnt == oemAddress.Length)
                return false;


            // All OEM data finish, then do frequence initialize
            if (cnt == oemAddress.Length - 1)
            {
                m_oem_country_code = oemValue[0];                  // 0x02
                m_oem_table_version = oemValue[1];                  // 0x0B
                m_oem_special_country_version = (uint)oemValue[2];  // 0x8e
                m_oem_freq_modification_flag = (uint)oemValue[3];    // 0x8f
                m_oem_machine = (oemValue[6] == 0) ? MODEL.CS108 : (MODEL)oemValue[6];               // 0xA4

                {
                    uint[] data = new uint[4];

                    data[0] = oemValue[7];
                    data[1] = oemValue[8];
                    data[2] = oemValue[9];
                    data[3] = oemValue[10];

                    m_PCBAssemblyCode = uint32ArrayToString(data).Replace("\0", "");
                }

                InitDefaultChannel_CS108();
                GenCountryList();
                SetDefaultAntennaList();
                FireStateChangedEvent(RFState.INITIALIZATION_COMPLETE);
                FireStateChangedEvent(RFState.IDLE);
            }

            return true;
        }

        bool GetOEMData(UInt32 address, ref UInt32 value)
        {
            int cnt;

            for (cnt = 0; cnt < oemAddress.Length; cnt++)
            {
                if (oemAddress[cnt] == address)
                {
                    value = oemValue[cnt];
                    return true;
                }
            }

            return false;
        }
    }
}
