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
        byte _sequencedNumber = 0;

        public void WriteRegister(UInt16 address, byte value)
        {
            byte[] data = new byte[1];

            data[0] = (byte)(value);

            WriteRegister(address, data);
        }

        public void WriteRegister(UInt16 [] address, byte [] value)
        {
            byte[][] data = new byte[value.Length][];

            for (int i = 0; i < address.Length; i++)
            {
                data[i] = new byte[1];
                data[i][0] = (byte)(value[i]);
            }

            WriteRegister(address, data);
        }

        public void WriteRegister(UInt16 address, UInt16 value)
        {
            byte[] data = new byte[2];

            data[0] = (byte)(value >> 8);
            data[1] = (byte)(value);

            WriteRegister (address, data);
        }

        public void WriteRegister(UInt16 address, UInt32 value)
        {
            byte[] data = new byte[4];

            data[0] = (byte)(value >> 24);
            data[1] = (byte)(value >> 16);
            data[2] = (byte)(value >> 8);
            data[3] = (byte)(value);

            WriteRegister(address, data);
        }

        public void WriteRegister(UInt16[] address, UInt32[] value)
        {
            byte[][] data = new byte[value.Length][];

            for (int i = 0; i < address.Length; i++)
            {
                data[i] = new byte[4];
                data[i][0] = (byte)(value[i] >> 24);
                data[i][1] = (byte)(value[i] >> 16);
                data[i][2] = (byte)(value[i] >> 8);
                data[i][3] = (byte)(value[i]);
            }

            WriteRegister(address, data);
        }

        public void WriteRegister(UInt16 address, UInt64 value)
        {
            byte[] data = new byte[8];

            data[0] = (byte)(value >> 56);
            data[1] = (byte)(value >> 48);
            data[2] = (byte)(value >> 40);
            data[3] = (byte)(value >> 32);
            data[4] = (byte)(value >> 24);
            data[5] = (byte)(value >> 16);
            data[6] = (byte)(value >> 8);
            data[7] = (byte)(value);

            WriteRegister(address, data);
        }

        byte _SequencedNumber = 0;

        public void WriteRegister(UInt16 address, byte[] data)
        {
            if (data == null)
                return;

            SCSLRFIDCMD cmd = SCSLRFIDCMD.SCSLWriteRegister;
            byte[] payload = new byte[11 + data.Length];
            int datapayloadlen = 4 + data.Length;

            payload[0] = 0x80;
            payload[1] = 0xb3;
            payload[2] = (byte)((int)cmd >> 8);
            payload[3] = (byte)cmd;
            payload[4] = _SequencedNumber++;  // Sequenced number
            payload[5] = (byte)(datapayloadlen >> 8);
            payload[6] = (byte)datapayloadlen;
            payload[7] = 1;
            payload[8] = (byte)(address >> 8);
            payload[9] = (byte)address;
            payload[10] = (byte)data.Length;
            Array.Copy(data, 0, payload, 11, data.Length);

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, payload, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.COMMANDENDRESPONSE);
        }

        public void WriteRegister(UInt16 [] address, byte[][] data)
        {
            if (address == null || data == null || address.Length > data.Length)
                return;

            SCSLRFIDCMD cmd = SCSLRFIDCMD.SCSLWriteRegister;
            int totallen = 0;
            for (int i = 0; i< address.Length; i++)
                totallen += data[i].Length;
            totallen += (address.Length * 3);

            if (totallen > 247)
                throw new OutOfMemoryException("out of BLE buffer");

            byte[] payload = new byte[8 + totallen];
            int datapayloadlen = 1 + totallen;

            payload[0] = 0x80;
            payload[1] = 0xb3;
            payload[2] = (byte)((int)cmd >> 8);
            payload[3] = (byte)cmd;
            payload[4] = _SequencedNumber++;  // Sequenced number
            payload[5] = (byte)(datapayloadlen >> 8);
            payload[6] = (byte)datapayloadlen;
            payload[7] = (byte)address.Length;

            int index = 8;
            for (int i = 0; i < address.Length; i++)
            {
                payload[index++] = (byte)(address[i] >> 8);
                payload[index++] = (byte)address[i];
                payload[index++] = (byte)data[i].Length;
                Array.Copy(data[i], 0, payload, index, data[i].Length);
                index += data[i].Length;
            }

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, payload, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.COMMANDENDRESPONSE);
        }

        internal bool WriteRegisterCommandReply(int index, byte[] data)
        {
            if (data == null)
                return false;

            if (data.Length < index + 7)
                return false;

            UInt16 Header = BitConverter.ToUInt16(data, index);
            if (Header != 0x51e2)
                return false;

            UInt16 EcgoCommandCode = BitConverter.ToUInt16(data, index + 2);
            if (EcgoCommandCode != 0x9a06)
                return false;

            byte EchoSequenceNumber = data[index + 4];

            UInt16 PayloadLength = BitConverter.ToUInt16(data, index + 5);
            if (PayloadLength != 1)
                return false;

            byte WriteStatus = data[index + 7];

            return true;
        }
    }
}