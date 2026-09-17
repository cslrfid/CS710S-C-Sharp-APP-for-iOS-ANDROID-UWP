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

#if nouse


        internal enum RFIDPACKETCODE
        {
            csl_tag_read_epc_only_new = 0x3001,
            csl_tag_read_epc_only_recurrent = 0x3002,
            csl_tag_read_multibank_new = 0x3003,
            csl_tag_read_multibank_recurrent_index_only = 0x3004,
            csl_tag_read_compact = 0x3006,
            csl_miscellaneous_event = 0x3007,
            csl_operation_complete = 0x3008,
            csl_access_complete = 0x3009,
        }


        internal void UplinkPacketsProcess (byte[] data)
        {
            RFIDPACKETCODE packetCode = 0;
            int index = 0;


            switch (packetCode)
            {
                case RFIDPACKETCODE.csl_tag_read_epc_only_new:
                    TagReadEPCOnlyNew(index, data);
                    break;

                case RFIDPACKETCODE.csl_tag_read_epc_only_recurrent:
                    TagReadEPCOnlyRecurrent(index, data);
                    break;

                case RFIDPACKETCODE.csl_tag_read_multibank_new:
                    TagReadMultiBankNew(index, data);
                    break;

                case RFIDPACKETCODE.csl_tag_read_multibank_recurrent_index_only:
                    TagReadMultiBankRecurrentIndexOnly(index, data);
                    break;

                case RFIDPACKETCODE.csl_tag_read_compact:
                    TagReadCompact(index, data);
                    break;

                case RFIDPACKETCODE.csl_miscellaneous_event:
                    MiscellaneousEvent(index, data);
                    break;

                case RFIDPACKETCODE.csl_operation_complete:
                    OperationComplete(index, data);
                    break;

                case RFIDPACKETCODE.csl_access_complete:
                    AccessComplete(index, data);
                    break;
            }
        }

        internal void TagReadEPCOnlyNew(int index, byte[] data)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 RSSI = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_begin = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_end = BitConverter.ToUInt16(data, index);
            byte PortNumber = data[index];
            UInt16 TagIndex = BitConverter.ToUInt16(data, index);
            byte[] TagData;


        }

        internal void TagReadEPCOnlyRecurrent(int index, byte[] data)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 RSSI = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_begin = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_end = BitConverter.ToUInt16(data, index);
            byte PortNumber = data[index];
            UInt16 TagIndex = BitConverter.ToUInt16(data, index);
        }

        internal void TagReadMultiBankNew(int index, byte[] data)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 RSSI = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_begin = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_end = BitConverter.ToUInt16(data, index);
            byte PortNumber = data[index];
            UInt16 TagIndex = BitConverter.ToUInt16(data, index);
            byte[] EPCBankData;
            byte NumberofExtraBanks;
            byte[] ExtraBankIndex;
        }

        internal void TagReadMultiBankRecurrentIndexOnly(int index, byte[] data)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 RSSI = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_begin = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_end = BitConverter.ToUInt16(data, index);
            byte PortNumber = data[index];
            UInt16 TagIndex = BitConverter.ToUInt16(data, index);
        }

        internal void TagReadCompact(int index, byte[] data)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            byte [] TagData;
        }

        internal void MiscellaneousEvent(int index, byte[] data)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 EventCode = BitConverter.ToUInt32(data, index);
            byte[] Data;
        }

        internal void OperationComplete(int index, byte[] data)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 CommandCode = BitConverter.ToUInt32(data, index);
            UInt16 Status;
        }

        internal void AccessComplete(int index, byte[] data)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 AccessCommand = BitConverter.ToUInt16(data, index);
            byte TagErrorCode;
            byte MacErrorCode;
            UInt16 WriteWordCount;
            byte[] Data;
        }
#endif

    }
}