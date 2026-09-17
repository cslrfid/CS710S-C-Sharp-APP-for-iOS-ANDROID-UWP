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
using System.Text;

namespace CSLibrary
{
    public partial class RFIDReader
    {
        internal void ReadRegister(READREGISTERSET[] readset)
        {
            int payloadlen = readset.Length * 3 + 1;
            byte[] sendpacket = new byte[7 + payloadlen];

            sendpacket[0] = 0x80;
            sendpacket[1] = 0xb3;
            sendpacket[2] = 0x14;
            sendpacket[3] = 0x71;
            sendpacket[4] = 0x00;
            sendpacket[5] = (byte)(payloadlen >> 8);
            sendpacket[6] = (byte)(payloadlen);

            int index = 8;
            sendpacket[7] = (byte)readset.Length;
            for (int cnt = 0; cnt < readset.Length; cnt++)
            {
                sendpacket[index++] = (byte)(readset[cnt].address >> 8);
                sendpacket[index++] = (byte)(readset[cnt].address);
                sendpacket[index++] = (byte)(readset[cnt].size);
            }

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, sendpacket, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1);
        }

        internal bool ReadRegisterCommandReply(int index, byte[] data)
        {
            if (data.Length < index + 7)
                return false;

            UInt16 Header = BitConverter.ToUInt16(data, index);
            if (Header != 0x51e2)
                return false;

            UInt16 EcgoCommandCode = BitConverter.ToUInt16(data, index + 2);
            if (EcgoCommandCode != 0x1471)
                return false;

            byte EchoSequenceNumber = data[index + 4];

            UInt16 PayloadLength = BitConverter.ToUInt16(data, index + 5);
            if (data.Length < index + 7 + PayloadLength)
                return false;

            index += 7;
            while (data.Length < index)
            {
                
                //                reg_addr_1_data
                // pass data to Register

                index++;
            }

            return true;

        }
    }
}