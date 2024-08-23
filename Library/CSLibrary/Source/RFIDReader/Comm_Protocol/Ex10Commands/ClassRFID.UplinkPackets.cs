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

using CSLibrary;
using CSLibrary.Structures;
using CSLibrary.Constants;
using System.Net.Sockets;
using CSLibrary.Barcode.Constants;
using System.Linq.Expressions;


namespace CSLibrary
{
    public partial class RFIDReader
    {
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


        internal void UplinkPacketsProcess (byte[] data, int index)
        {
            UInt16 fixedHeader  = BitConverter.ToUInt16(data, index);
            RFIDPACKETCODE packetCode = (RFIDPACKETCODE)BitConverter.ToUInt16(data, index);
            byte sequenceNumber = data[index + 4];
            UInt16 payloadLen = BitConverter.ToUInt16(data, index + 5);

            if ((payloadLen + 7 - 1) > data.Length)
                return;

            switch (packetCode)
            {
                case RFIDPACKETCODE.csl_tag_read_epc_only_new:
                    csl_tag_read_epc_only_new_packet_proc(data, index);
                    break;

                case RFIDPACKETCODE.csl_tag_read_epc_only_recurrent:
                    TagReadEPCOnlyRecurrent(data, index);
                    break;

                case RFIDPACKETCODE.csl_tag_read_multibank_new:
                    TagReadMultiBankNew(data, index);
                    break;

                case RFIDPACKETCODE.csl_tag_read_multibank_recurrent_index_only:
                    TagReadMultiBankRecurrentIndexOnly(data, index);
                    break;

                case RFIDPACKETCODE.csl_tag_read_compact:
                    csl_tag_read_epc_only_new_packet_proc(data, 8);
                    break;

                case RFIDPACKETCODE.csl_miscellaneous_event:
                    MiscellaneousEvent(data, index);
                    break;

                case RFIDPACKETCODE.csl_operation_complete:
                    OperationComplete(data, index);
                    break;

                case RFIDPACKETCODE.csl_access_complete:
                    AccessComplete(data, index);
                    break;
            }
        }

        internal void TagReadEPCOnlyRecurrent(byte[] data, int index)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 RSSI = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_begin = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_end = BitConverter.ToUInt16(data, index);
            byte PortNumber = data[index];
            UInt16 TagIndex = BitConverter.ToUInt16(data, index);
        }

        internal void TagReadMultiBankNew(byte[] data, int index)
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

        internal void TagReadMultiBankRecurrentIndexOnly(byte[] data, int index)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 RSSI = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_begin = BitConverter.ToUInt16(data, index);
            UInt16 rf_phase_end = BitConverter.ToUInt16(data, index);
            byte PortNumber = data[index];
            UInt16 TagIndex = BitConverter.ToUInt16(data, index);
        }

        internal void MiscellaneousEvent(byte[] data, int index)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 EventCode = BitConverter.ToUInt16(data, index);
            byte[] Data;
        }

        internal void OperationComplete(byte[] data, int index)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 CommandCode = BitConverter.ToUInt16(data, index);
            UInt16 Status;
        }

        // 0x3001 packet
        void csl_tag_read_epc_only_new_packet_proc(byte[] data, int index)
        {
            try
            {

                UInt32 UTCTimeStamp = Tools.Hex.MSBToUInt32(data, index);
                UInt16 rf_phase_begin = Tools.Hex.MSBToUInt16(data, index + 6);
                UInt16 rf_phase_end = Tools.Hex.MSBToUInt16(data, index + 8);
                byte PortNumber = data[index + 10];
                UInt16 Tag_Index = Tools.Hex.MSBToUInt16(data, index + 13);

                int rssidBm100;
                {
                    byte hiByte = data[index + 4];
                    rssidBm100 = Tools.Hex.MSBToUInt16 (data, index + 4);

                    if (hiByte > 0x7f)
                        rssidBm100 -= 0x10000;
                }

                index += 15;
                //while (index < data.Length)
                {
                    CSLibrary.Constants.CallbackType type = CSLibrary.Constants.CallbackType.TAG_RANGING;
                    CSLibrary.Structures.TagCallbackInfo info = new CSLibrary.Structures.TagCallbackInfo();

                    info.pc = new S_PC(Tools.Hex.MSBToUInt16(data, index));
                    index += 2;

                    if (info.pc.XI) // Check XPC W1
                    {
                        info.xpc_w1 = new S_XPC_W1(Tools.Hex.MSBToUInt16(data, index));
                        index += 2;

                        if (info.xpc_w1.XEB) // Check XPC W2
                        {
                            info.xpc_w2 = new S_XPC_W2(Tools.Hex.MSBToUInt16(data, index));
                            index += 2;
                        }
                    }

                    int epcbytelen = ((info.pc >> 11) << 1);
                    byte[] epc = new byte[epcbytelen];
                    Array.Copy(data, index, epc, 0, epcbytelen);
                    info.epc = new S_EPC(epc);
                    index += epcbytelen;

                    info.antennaPort = PortNumber;
                    info.rssidBm = rssidBm100 / 100;
                    info.rssi = Tools.dBConverion.dBm2dBuV(info.rssidBm) ;

                    info.Bank1Data = new ushort[0];
                    info.Bank2Data = new ushort[0];
                    info.Bank3Data = new ushort[0];

                    if (OnAsyncCallback != null)
                    {
                        try
                        {
                            CSLibrary.Events.OnAsyncCallbackEventArgs callBackData = new Events.OnAsyncCallbackEventArgs(info, type);

                            OnAsyncCallback(_deviceHandler, callBackData);
                        }
                        catch (Exception ex)
                        {
                            CSLibrary.Debug.WriteLine("OnAsyncCallback Error : " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("csl_tag_read_epc_only_new_packet_proc Error : " + ex.Message);
            }
        }

        bool checkmultibankzero(UInt16[] multibankdata)
        {
            foreach (var data in multibankdata)
                if (data != 0)
                    return false;

            return true;
        }

        // 0x3003 packet
        void csl_tag_read_multibank_new_packet_proc(byte[] data, int index)
        {
            try
            {
                UInt32 UTCTimeStamp = Tools.Hex.MSBToUInt32(data, index);
                UInt16 rf_phase_begin = Tools.Hex.MSBToUInt16(data, index + 6);
                UInt16 rf_phase_end = Tools.Hex.MSBToUInt16(data, index + 8);
                byte PortNumber = data[index + 10];
                UInt16 Tag_Index = Tools.Hex.MSBToUInt16(data, index + 13);

                int rssidBm100;
                {
                    byte hiByte = data[index + 4];
                    rssidBm100 = Tools.Hex.MSBToUInt16(data, index + 4);

                    if (hiByte > 0x7f)
                        rssidBm100 -= 0x10000;
                }

                index += 15;
                //while (index < data.Length)
                {
                    CSLibrary.Constants.CallbackType type = CSLibrary.Constants.CallbackType.TAG_RANGING;
                    CSLibrary.Structures.TagCallbackInfo info = new CSLibrary.Structures.TagCallbackInfo();

                    info.pc = new S_PC(Tools.Hex.MSBToUInt16(data, index));
                    index += 2;

                    if (info.pc.XI) // Check XPC W1
                    {
                        info.xpc_w1 = new S_XPC_W1(Tools.Hex.MSBToUInt16(data, index));
                        index += 2;

                        if (info.xpc_w1.XEB) // Check XPC W2
                        {
                            info.xpc_w2 = new S_XPC_W2(Tools.Hex.MSBToUInt16(data, index));
                            index += 2;
                        }
                    }

                    int epcbytelen = ((info.pc >> 11) << 1);
                    byte[] epc = new byte[epcbytelen];
                    Array.Copy(data, index, epc, 0, epcbytelen);
                    info.epc = new S_EPC(epc);
                    index += epcbytelen;

                    info.antennaPort = PortNumber;
                    info.rssidBm = rssidBm100 / 100;
                    info.rssi = Tools.dBConverion.dBm2dBuV(info.rssidBm);

                    info.Bank1Data = new ushort[0];
                    info.Bank2Data = new ushort[0];
                    info.Bank3Data = new ushort[0];
                    if (data[index] == m_rdr_opt_parms.TagRanging.multibankswithreply)
                    {
                        index++;
                        if (m_rdr_opt_parms.TagRanging.multibanks > 0)
                            info.Bank1Data = Tools.Hex.MSBToUInt16Array(data, index, m_rdr_opt_parms.TagRanging.count1);
                        if (m_rdr_opt_parms.TagRanging.multibanks > 1)
                            info.Bank2Data = Tools.Hex.MSBToUInt16Array(data, index + (m_rdr_opt_parms.TagRanging.count1 * 2), m_rdr_opt_parms.TagRanging.count2);
                        if (m_rdr_opt_parms.TagRanging.multibanks > 2)
                            info.Bank3Data = Tools.Hex.MSBToUInt16Array(data, index + ((m_rdr_opt_parms.TagRanging.count1 + m_rdr_opt_parms.TagRanging.count2) * 2), m_rdr_opt_parms.TagRanging.count3);
                    }

                    if (_currentTagRanging.multibankswithreply >= 3)
                    {
                        if (_currentTagRanging.bank3 == MemoryBank.TID)
                            if (checkmultibankzero(info.Bank3Data))
                                return;
                    }

                    if (_currentTagRanging.multibankswithreply >= 2)
                    {
                        if (_currentTagRanging.bank2 == MemoryBank.TID)
                            if (checkmultibankzero(info.Bank2Data))
                                return;
                    }

                    if (_currentTagRanging.multibankswithreply >= 1)
                    {
                        if (_currentTagRanging.bank1 == MemoryBank.TID)
                            if (checkmultibankzero(info.Bank1Data))
                                return;
                    }

                    CSLibrary.Events.OnAsyncCallbackEventArgs callBackData = new Events.OnAsyncCallbackEventArgs(info, type);

                    if (OnAsyncCallback != null)
                        try
                        {
                            OnAsyncCallback(_deviceHandler, callBackData);
                        }
                        catch (Exception ex)
                        {
                            CSLibrary.Debug.WriteLine("OnAsyncCallback Error : " + ex.Message);
                        }
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("csl_tag_read_multibank_new_packet_proc Error : " + ex.Message);
            }
        }

        // 0x3006 packet
        internal void csl_tag_read_compact_packet_proc(byte[] data, int index)
        {
            try
            {
                UInt32 UTCTimeStamp = Tools.Hex.MSBToUInt32(data, index);

                index += 6;
                while (index < data.Length)
                {
                    CSLibrary.Constants.CallbackType type = CSLibrary.Constants.CallbackType.TAG_RANGING;
                    CSLibrary.Structures.TagCallbackInfo info = new CSLibrary.Structures.TagCallbackInfo();

                    //var PC = BitConverter.ToUInt16(data, index);
                    //info.pc = new S_PC(PC);
                    info.pc = new S_PC((UInt16)(data[index] << 8 | data[index + 1]));
                    index += 2;

                    if (info.pc.XI) // Check XPC W1
                    {
                        info.xpc_w1 = new S_XPC_W1((UInt16)(data[index] << 8 | data[index + 1]));
                        index += 2;

                        if (info.xpc_w1.XEB) // Check XPC W2
                        {
                            info.xpc_w2 = new S_XPC_W2((UInt16)(data[index] << 8 | data[index + 1]));
                            index += 2;
                        }
                    }

                    int epcbytelen = ((info.pc >> 11) << 1);
                    byte[] epc = new byte[epcbytelen];
                    Array.Copy(data, index, epc, 0, epcbytelen);
                    info.epc = new S_EPC(epc);
                    index += epcbytelen;


                    int rssidBm100;
                    {
                        byte hiByte = data[index];
                        rssidBm100 = ((int)(data[index] << 8 | data[index + 1]));

                        if (hiByte > 0x7f)
                            rssidBm100 -= 0x10000;
                    }
                    info.rssidBm = rssidBm100 / 100;
                    info.rssi = Tools.dBConverion.dBm2dBuV(info.rssidBm);

                    index += 2;

                    info.Bank1Data = new ushort[0];
                    info.Bank2Data = new ushort[0];
                    info.Bank3Data = new ushort[0];

                    /*
                                    if (RFIDRegister.AntennaPortConfig.FastId() && info.pc.EPCLength >= 6 && epc[epcbytelen - 12] == 0xe2 && epc[epcbytelen - 11] == 0x80 && epc[epcbytelen - 10] == 0x11)
                                    {
                                        byte[] newbyteEpc = new byte[epcbytelen - 12];
                                        UInt16[] newbyteTid = new UInt16[6];

                                        Array.Copy(epc, 0, newbyteEpc, 0, newbyteEpc.Length);
                                        ArrayCopy(epc, epcbytelen - 12, newbyteTid, 0, 12);

                                        info.FastTid = newbyteTid;
                                        epc = newbyteEpc;
                                    }
                                    else
                                        info.FastTid = new UInt16[0];
                    */

                    CSLibrary.Events.OnAsyncCallbackEventArgs callBackData = new Events.OnAsyncCallbackEventArgs(info, type);

                    if (OnAsyncCallback != null)
                        try
                        {
                            OnAsyncCallback(_deviceHandler, callBackData);
                        }
                        catch (Exception ex)
                        {
                            CSLibrary.Debug.WriteLine("OnAsyncCallback Error : " + ex.Message);
                        }
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("csl_tag_read_compact_packet_proc Error : " + ex.Message);
            }
        }

        // 0x3007 packet
        void csl_miscellaneous_event_packet_proc(byte[] data, int index, int len)
        {
            try
            {
            UInt32 UTCTimeStamp = Tools.Hex.MSBToUInt32(data, index);
            UInt16 eventCode = Tools.Hex.MSBToUInt16(data, index + 4);

            Debug.WriteLine("{0:X4} : 0x3007, {1:X2}", UTCTimeStamp, eventCode);

            switch (eventCode)
            {
                case 0x0001: // keep alive
                    break;

                case 0x0002: // inventory round end
                    break;

                case 0x0003: // CRC error rate (2 bytes Data)
                    break;

                case 0x0004: // tag rate value (2 bytes Data)
                    {
                        uint tagRate = Tools.Hex.MSBToUInt16(data, index + 6);

                        if (OnInventoryTagRateCallback != null)
                        {
                            try
                            {
                                OnInventoryTagRateCallback(this, new Events.OnInventoryTagRateCallbackEventArgs(tagRate));
                            }
                            catch (Exception ex)
                            {
                                CSLibrary.Debug.WriteLine("csl_miscellaneous_event call back error : " + ex.Message);
                            }
                        }
                    }
                    break;

                default:
                    CSLibrary.Debug.WriteLine("csl_miscellaneous_event Event Code : " + eventCode.ToString());
                    break;
            }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("csl_miscellaneous_event_packet_proc Error : " + ex.Message);
            }
        }

        // 0x3009 packet
        void csl_access_complete_packet_proc(byte[] data, int index, int size)
        {
            UInt32 UTCTimeStamp = Tools.Hex.MSBToUInt32(data, index);
            UInt16 accessCommand = Tools.Hex.MSBToUInt16(data, index + 4);
            byte tagErrorCode = data[index + 6];
            byte macErrorCode = data[index + 7];
            UInt16 writeWordCount = Tools.Hex.MSBToUInt16(data, index + 8);

            Operation RealCurrentOperation = (Operation)(_deviceHandler._currentCmdRemark);

            if (tagErrorCode != 0x10 || macErrorCode != 0x00)
                Debug.WriteLine("Tag Access Error, {0:2X}, {1:2X}", tagErrorCode, macErrorCode);

            switch (accessCommand)
            {
                case 0xC2: // Read
                    switch (RealCurrentOperation)
                    {
                        case CSLibrary.Constants.Operation.TAG_READ:
                            if (tagErrorCode == 0x10 && macErrorCode == 0x00)
                                ArrayCopy(data, index + 12, m_rdr_opt_parms.TagRead.m_pData, 0, m_rdr_opt_parms.TagRead.count * 2);
                            FireAccessCompletedEvent(
                            new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00),
                                CSLibrary.Constants.Bank.SPECIFIC,
                                TagAccess.READ,
                                m_rdr_opt_parms.TagRead.pData));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_PC:
                            if (tagErrorCode == 0x10 && macErrorCode == 0x00)
                                m_rdr_opt_parms.TagReadPC.m_pc = Tools.Hex.MSBToUInt16(data, index + 12);
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00),
                                CSLibrary.Constants.Bank.PC,
                                CSLibrary.Constants.TagAccess.READ,
                                m_rdr_opt_parms.TagReadPC.pc));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_EPC:
                            if (tagErrorCode == 0x10 && macErrorCode == 0x00)
                                ArrayCopy(data, index + 12, m_rdr_opt_parms.TagReadEPC.m_epc, 0, m_rdr_opt_parms.TagReadEPC.count * 2);
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00),
                                Bank.EPC,
                                TagAccess.READ,
                                m_rdr_opt_parms.TagReadEPC.epc));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_ACC_PWD:
                            if (tagErrorCode == 0x10 && macErrorCode == 0x00)
                                m_rdr_opt_parms.TagReadAccPwd.m_password = Tools.Hex.MSBToUInt32(data, index + 12);
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00),
                                CSLibrary.Constants.Bank.ACC_PWD,
                                CSLibrary.Constants.TagAccess.READ,
                                m_rdr_opt_parms.TagReadAccPwd.password));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_KILL_PWD:
                            if (tagErrorCode == 0x10 && macErrorCode == 0x00)
                                m_rdr_opt_parms.TagReadKillPwd.m_password = Tools.Hex.MSBToUInt32(data, index + 12);
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00),
                                CSLibrary.Constants.Bank.KILL_PWD,
                                CSLibrary.Constants.TagAccess.READ,
                                m_rdr_opt_parms.TagReadKillPwd.password));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_TID:
                            if (tagErrorCode == 0x10 && macErrorCode == 0x00)
                                ArrayCopy(data, index + 12, m_rdr_opt_parms.TagReadTid.pData, 0, m_rdr_opt_parms.TagReadTid.count * 2);
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00),
                                CSLibrary.Constants.Bank.TID,
                                CSLibrary.Constants.TagAccess.READ,
                                m_rdr_opt_parms.TagReadTid.tid));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_USER:
                            if (tagErrorCode == 0x10 && macErrorCode == 0x00)
                                ArrayCopy(data, index + 12, m_rdr_opt_parms.TagReadUser.m_pData, 0, m_rdr_opt_parms.TagReadUser.count * 2);
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00),
                                Bank.USER,
                                TagAccess.READ,
                                m_rdr_opt_parms.TagReadUser.pData));
                            break;

                        case CSLibrary.Constants.Operation.TAG_AUTHENTICATE:
/*
                            {
                                int pkt_len = recvData[5] << 8 | recvData[4];
                                int flags = recvData[1];
                                int len = ((pkt_len - 3) * 4) - ((flags >> 6) & 3);
                                byte[] response = new byte[len];
                                Array.Copy(recvData, offset + 20, response, 0, len);
                                m_rdr_opt_parms.TagAuthenticate.pData = new S_DATA(response);
                                //Array.Copy(recvData, offset + 20, m_rdr_opt_parms.TagAuthenticate.pData, 0, len);
                            }
*/
                            break;
                    }
                    break;

                case 0xC3: // Write
                    switch (RealCurrentOperation)
                    {
                        case CSLibrary.Constants.Operation.TAG_WRITE:
                            if (m_rdr_opt_parms.TagWrite.bank == MemoryBank.USER)
                            {
                                FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                    (tagErrorCode == 0x10 && macErrorCode == 0x00 && writeWordCount != 0),
                                    CSLibrary.Constants.Bank.SPECIFIC,
                                    TagAccess.WRITE,
                                    m_rdr_opt_parms.TagRead.pData));
                            }
                            else
                            {
                                FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                    (tagErrorCode == 0x10 && macErrorCode == 0x00 && writeWordCount != 0),
                                    CSLibrary.Constants.Bank.SPECIFIC,
                                    TagAccess.WRITE,
                                    m_rdr_opt_parms.TagRead.pData));
                            }
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_PC:
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00 && writeWordCount != 0),
                                CSLibrary.Constants.Bank.PC,
                                CSLibrary.Constants.TagAccess.WRITE,
                                m_rdr_opt_parms.TagReadPC.pc));
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_EPC:
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00 && writeWordCount != 0),
                                Bank.EPC,
                                TagAccess.WRITE,
                                m_rdr_opt_parms.TagReadEPC.epc));
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_ACC_PWD:
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00 && writeWordCount != 0),
                                CSLibrary.Constants.Bank.ACC_PWD,
                                CSLibrary.Constants.TagAccess.WRITE,
                                m_rdr_opt_parms.TagReadAccPwd.password));
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_KILL_PWD:
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00 && writeWordCount != 0),
                                CSLibrary.Constants.Bank.KILL_PWD,
                                CSLibrary.Constants.TagAccess.WRITE,
                                m_rdr_opt_parms.TagReadKillPwd.password));
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_USER:
                            FireAccessCompletedEvent(
                                new CSLibrary.Events.OnAccessCompletedEventArgs(
                                (tagErrorCode == 0x10 && macErrorCode == 0x00 && writeWordCount != 0),
                                Bank.USER,
                                TagAccess.WRITE,
                                m_rdr_opt_parms.TagReadUser.pData));
                            break;
                    }
                    break;

                    /*
                case 0xC4: // Kill
                    {
                        FireAccessCompletedEvent(
                        new CSLibrary.Events.OnAccessCompletedEventArgs(
                        (tagErrorCode == 0x10 && macErrorCode == 0x00),
                            Bank.UNKNOWN,
                            TagAccess.KILL,
                            null));
                    }
                    break;
                    */
                case 0xC5: // Lock
                    {
                        FireAccessCompletedEvent(
                        new CSLibrary.Events.OnAccessCompletedEventArgs(
                        (tagErrorCode == 0x10 && macErrorCode == 0x00),
                            Bank.UNKNOWN,
                            TagAccess.LOCK,
                            null));
                    }
                    break;

                case 0xC6: // Access
                    break;

                case 0xC7: // Block Write
                    break;

                case 0xC9: // Block Permalock
                    break;

                case 0xD5: // Authenticate
                    {
                        byte[] response = new byte[size];
                        Array.Copy(data, index + 12, response, 0, size);
                        m_rdr_opt_parms.TagAuthenticate.pData = new S_DATA(response);

                        FireAccessCompletedEvent(
                        new CSLibrary.Events.OnAccessCompletedEventArgs(
                        (tagErrorCode == 0x10 && macErrorCode == 0x00),
                            Bank.UNKNOWN,
                            TagAccess.AUTHENTICATE,
                            null));
                    }
                    break;

                default:
                    Debug.WriteLine("Tag Access Command Error, {0:2X}, {1:2X}", accessCommand);
                    break;
            }

#if oldcode


                                        switch (RealCurrentOperation)
                                        {
                                            case CSLibrary.Constants.Operation.TAG_READ:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        CSLibrary.Constants.Bank.SPECIFIC,
                                                        TagAccess.READ,
                                                        m_rdr_opt_parms.TagRead.pData));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_READ_PC:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        CSLibrary.Constants.Bank.PC,
                                                        CSLibrary.Constants.TagAccess.READ,
                                                        m_rdr_opt_parms.TagReadPC.pc));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_READ_EPC:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.EPC,
                                                        TagAccess.READ,
                                                        m_rdr_opt_parms.TagReadEPC.epc));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_READ_ACC_PWD:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        CSLibrary.Constants.Bank.ACC_PWD,
                                                        CSLibrary.Constants.TagAccess.READ,
                                                        m_rdr_opt_parms.TagReadAccPwd.password));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_READ_KILL_PWD:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        CSLibrary.Constants.Bank.KILL_PWD,
                                                        CSLibrary.Constants.TagAccess.READ,
                                                        m_rdr_opt_parms.TagReadKillPwd.password));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_READ_TID:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        CSLibrary.Constants.Bank.TID,
                                                        CSLibrary.Constants.TagAccess.READ,
                                                        m_rdr_opt_parms.TagReadTid.tid));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_READ_USER:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.USER,
                                                        TagAccess.READ,
                                                        m_rdr_opt_parms.TagReadUser.pData));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_AUTHENTICATE:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.UNKNOWN,
                                                        TagAccess.AUTHENTICATE,
                                                        m_rdr_opt_parms.TagAuthenticate.pData));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_UNTRACEABLE:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.UNKNOWN,
                                                        TagAccess.UNTRACEABLE,
                                                        m_rdr_opt_parms.TagAuthenticate.pData));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_WRITE:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.SPECIFIC,
                                                        CSLibrary.Constants.TagAccess.WRITE,
                                                        m_rdr_opt_parms.TagReadUser.pData));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_WRITE_PC:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        CSLibrary.Constants.Bank.PC,
                                                        CSLibrary.Constants.TagAccess.WRITE,
                                                        m_rdr_opt_parms.TagReadPC.pc));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_WRITE_EPC:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.EPC,
                                                        CSLibrary.Constants.TagAccess.WRITE,
                                                        m_rdr_opt_parms.TagReadEPC.epc));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_WRITE_ACC_PWD:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        CSLibrary.Constants.Bank.ACC_PWD,
                                                        CSLibrary.Constants.TagAccess.WRITE,
                                                        m_rdr_opt_parms.TagReadAccPwd.password));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_WRITE_KILL_PWD:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        CSLibrary.Constants.Bank.KILL_PWD,
                                                        CSLibrary.Constants.TagAccess.WRITE,
                                                        m_rdr_opt_parms.TagReadKillPwd.password));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_WRITE_USER:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.USER,
                                                        CSLibrary.Constants.TagAccess.WRITE,
                                                        m_rdr_opt_parms.TagReadUser.pData));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_BLOCK_WRITE:
                                                {
                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.SPECIFIC,
                                                        CSLibrary.Constants.TagAccess.WRITE,
                                                        m_rdr_opt_parms.TagReadUser.pData));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_LOCK:
                                                {
                                                    CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.UNKNOWN,
                                                        TagAccess.LOCK,
                                                        null));
                                                }
                                                break;

                                            case CSLibrary.Constants.Operation.TAG_KILL:
                                                {
                                                    CSLibrary.Debug.WriteLine("Tag Kill end {0}", currentCommandResponse);

                                                    FireAccessCompletedEvent(
                                                        new OnAccessCompletedEventArgs(
                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                        Bank.UNKNOWN,
                                                        TagAccess.KILL,
                                                        null));
                                                }
                                                break;

                                            default:
                                                if (_deviceHandler.rfid.EM4325.CommandEndProc(RealCurrentOperation, ((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0))
                                                    break;

                                                if (_deviceHandler.rfid.FM13DT160.CommandEndPProc(RealCurrentOperation, ((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0))
                                                    break;

                                                break;

                                                /*
                                                                                            case CSLibrary.Constants.Operation.TAG_UNTRACEABLE:
                                                                                                {
                                                                                                    CSLibrary.Debug.WriteLine("Tag untraceable end {0}", currentCommandResponse);

                                                                                                    FireAccessCompletedEvent(
                                                                                                        new OnAccessCompletedEventArgs(
                                                                                                        (((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
                                                                                                        Bank.UNTRACEABLE,
                                                                                                        TagAccess.WRITE,
                                                                                                        null));
                                                                                                }
                                                                                                break;
                                                                                                */

                                        }
                                    }

                                    FireStateChangedEvent(CSLibrary.Constants.RFState.IDLE);







            switch (accessCommand)
            {
                case 0xc2:  // Read
                    switch (RealCurrentOperation)
                    {
                        case CSLibrary.Constants.Operation.TAG_READ:
                            //ArrayCopy(recvData, offset + 20, m_rdr_opt_parms.TagRead.m_pData, 0, m_rdr_opt_parms.TagRead.count * 2);
                            if (size > 12)
                            {
                                int datalen = size - 12;
                                recData = new byte[datalen];
                                Array.Copy(data, 12, recData, 0, datalen);
                            }
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_PC:
                            m_rdr_opt_parms.TagReadPC.m_pc = (ushort)((recvData[offset + 20] << 8) | (recvData[offset + 21]));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_EPC:
                            ArrayCopy(recvData, offset + 20, m_rdr_opt_parms.TagReadEPC.m_epc, 0, m_rdr_opt_parms.TagReadEPC.count * 2);
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_ACC_PWD:
                            m_rdr_opt_parms.TagReadAccPwd.m_password = (UInt32)((recvData[offset + 20] << 24) | (recvData[offset + 21] << 16) | (recvData[offset + 22] << 8) | (recvData[offset + 23]));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_KILL_PWD:
                            m_rdr_opt_parms.TagReadKillPwd.m_password = (UInt32)((recvData[offset + 20] << 24) | (recvData[offset + 21] << 16) | (recvData[offset + 22] << 8) | (recvData[offset + 23]));
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_TID:
                            ArrayCopy(recvData, offset + 20, m_rdr_opt_parms.TagReadTid.pData, 0, m_rdr_opt_parms.TagReadTid.count * 2);
                            break;

                        case CSLibrary.Constants.Operation.TAG_READ_USER:
                            ArrayCopy(recvData, offset + 20, m_rdr_opt_parms.TagReadUser.m_pData, 0, m_rdr_opt_parms.TagReadUser.count * 2);
                            break;

                        case CSLibrary.Constants.Operation.TAG_AUTHENTICATE:
                            {
                                int pkt_len = recvData[5] << 8 | recvData[4];
                                int flags = recvData[1];
                                int len = ((pkt_len - 3) * 4) - ((flags >> 6) & 3);
                                byte[] response = new byte[len];
                                Array.Copy(recvData, offset + 20, response, 0, len);
                                m_rdr_opt_parms.TagAuthenticate.pData = new S_DATA(response);
                                //Array.Copy(recvData, offset + 20, m_rdr_opt_parms.TagAuthenticate.pData, 0, len);
                            }
                            break;
                    }
                    break;

                case 0xc3:  // Write
                    switch (RealCurrentOperation)
                    {
                        case CSLibrary.Constants.Operation.TAG_WRITE:
                            //ArrayCopy(recvData, offset + 20, m_rdr_opt_parms.TagReadUser.m_pData, 0, m_rdr_opt_parms.TagReadEPC.count * 2);
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_PC:
                            //m_rdr_opt_parms.TagReadPC.m_pc = (ushort)((recvData[offset + 20] << 8) | (recvData[offset + 21]));
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_EPC:
                            //ArrayCopy(recvData, offset + 20, m_rdr_opt_parms.TagReadEPC.m_epc, 0, m_rdr_opt_parms.TagReadEPC.count * 2);
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_ACC_PWD:
                            //m_rdr_opt_parms.TagReadAccPwd.m_password = (UInt32)((recvData[offset + 20] << 24) | (recvData[offset + 21] << 16) | (recvData[offset + 22] << 8) | (recvData[offset + 23]));
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_KILL_PWD:
                            //m_rdr_opt_parms.TagReadKillPwd.m_password = (UInt32)((recvData[offset + 20] << 24) | (recvData[offset + 21] << 16) | (recvData[offset + 22] << 8) | (recvData[offset + 23]));
                            break;

                        case CSLibrary.Constants.Operation.TAG_WRITE_USER:
                            //ArrayCopy(recvData, offset + 20, m_rdr_opt_parms.TagReadUser.m_pData, 0, m_rdr_opt_parms.TagReadEPC.count * 2);
                            break;
                    }
                    break;

                case 0xc4:  // Kill
                    break;

                case 0xc5:  // Lock
                    break;

                case 0xc7:  // Block Write
                    break;

                case 0x04:  // EAS
                    break;

                case 0xe2: // Untraceable
                    break;

                case 0xd5: // Authenticate
                    break;

                case 0xe0: // FM13DT160 & EM4325
                    return false;

                default:
                    return false;
            }
#endif


        }

        internal void AccessComplete(byte[] data, int index)
        {
            UInt32 UTCTimpStamp = BitConverter.ToUInt32(data, index);
            UInt16 AccessCommand = BitConverter.ToUInt16(data, index);
            byte TagErrorCode;
            byte MacErrorCode;
            UInt16 WriteWordCount;
            byte[] Data;
        }
    }
}