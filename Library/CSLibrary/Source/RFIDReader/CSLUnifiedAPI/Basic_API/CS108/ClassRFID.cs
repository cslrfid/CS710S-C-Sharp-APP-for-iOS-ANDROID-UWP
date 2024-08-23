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
    using Structures;
    using Events;
    using Tools;

    public partial class RFIDReader
    {
        public UInt32 LastMacErrorCode = 0;
        public UInt16 LastResultCode = 0;

        private CSLibrary.Tools.Queue _dataBuffer = new Tools.Queue(16 * 1024 * 1024);

        public ChipSetID OEMChipSetID
        {
            get
            {
                return ChipSetID.R2000;

                switch (m_oem_machine)
                {
                    case MODEL.CS103:
                    case MODEL.CS108:
                    case MODEL.CS209:
                    case MODEL.CS333:
                    case MODEL.CS463:
                        return ChipSetID.R2000;
                }

                return ChipSetID.R1000;
            }
        }

        int TwoByteArraryToInt(byte[] data, int startIndex)
        {
            int a = data[startIndex];
            a |= (data[startIndex + 1] << 8);

            return (a);
        }

        int FourByteArraryToInt(byte[] data, int startIndex)
        {
            int a = data[startIndex];
            a |= (data[startIndex + 1] << 8);
            a |= (data[startIndex + 2] << 16);
            a |= (data[startIndex + 3] << 24);

            return (a);
        }

        Single R2000_RssiTranslation(byte rawValue)
        {
            int iMantissa = rawValue & 0x07;
            int iExponent = (rawValue >> 3) & 0x1F;

            double dRSSI = 20.0 * Math.Log10(Math.Pow(2.0, (double)iExponent) * (1.0 + ((double)iMantissa / 8.0)));

            CSLibrary.Debug.WriteLine("Routine : R2000_RssiTranslation, 0x" + rawValue.ToString("x2") + "=" + dRSSI.ToString());

            return (Single) dRSSI;
        }

        bool R2000Packet_NewInventory(byte[] recvData, int offset = 0)
        {
            CSLibrary.Debug.WriteLine("Routine : R2000Packet_NewInventory");

            if (OnAsyncCallback != null)
            {
                uint newInventoryPacketOffset = 8;
                Byte AntennaPort = recvData[6];

                while (newInventoryPacketOffset < (recvData.Length - 1))
                {
                    CSLibrary.Structures.TagCallbackInfo info = new CSLibrary.Structures.TagCallbackInfo();

                    info.antennaPort = AntennaPort;

                    info.pc = (UInt16)(recvData[newInventoryPacketOffset] << 8 | recvData[newInventoryPacketOffset + 1]);
                    int epcbytelen = ((info.pc & 0xf800) >> 11) * 2;

                    if (epcbytelen == 0)
                        return false;

                    if ((newInventoryPacketOffset + epcbytelen + 1) >= recvData.Length)
                        return false;

                    info.rssi = R2000_RssiTranslation(recvData[newInventoryPacketOffset + epcbytelen + 2]);
                    info.rssidBm = Tools.dBConverion.dBuV2dBm(info.rssi);

                    int xpcoffset = 0;

                    if (info.pc.XI) // Check XPC W1
                    {
                        info.xpc_w1 = new S_XPC_W1((UInt16)(recvData[newInventoryPacketOffset + 2] << 8 | recvData[newInventoryPacketOffset + 3]));
                        xpcoffset+=2;
                        epcbytelen -= 2;

                        if (info.xpc_w1.XEB) // Check XPC W2
                        {
                            info.xpc_w2 = new S_XPC_W2((UInt16)(recvData[newInventoryPacketOffset + 4] << 8 | recvData[newInventoryPacketOffset + 5]));
                            xpcoffset+=2;
                            epcbytelen -= 2;
                        }
                    }

                    byte[] byteEpc = new byte[epcbytelen];
                    Array.Copy(recvData, (int)(newInventoryPacketOffset + 2 + xpcoffset), byteEpc, 0, epcbytelen);

/*
                    if (Options.TagRanging.fastid && info.pc.EPCLength >= 6 && byteEpc[byteEpc.Length - 12] == 0xe2 && byteEpc[byteEpc.Length - 11] == 0x80 && byteEpc[byteEpc.Length - 10] == 0x11)
                    {
                        byte[] newbyteEpc = new byte[byteEpc.Length - 12];
                        UInt16[] newbyteTid = new UInt16[6];

                        Array.Copy(byteEpc, 0, newbyteEpc, 0, newbyteEpc.Length);
                        ArrayCopy (byteEpc, byteEpc.Length - 12, newbyteTid, 0, 12);

                        info.FastTid = newbyteTid;
                        byteEpc = newbyteEpc;
                    }
*/

                    info.epc = new S_EPC(byteEpc);

                    newInventoryPacketOffset += (uint)(2 + xpcoffset + epcbytelen + 1);

                    switch (CurrentOperation)
                    {
                        case Operation.TAG_RANGING:
                            {
                                CSLibrary.Constants.CallbackType type = CSLibrary.Constants.CallbackType.TAG_RANGING;
                                CSLibrary.Events.OnAsyncCallbackEventArgs callBackData = new Events.OnAsyncCallbackEventArgs(info, type);
                                if (OnAsyncCallback != null)
                                    OnAsyncCallback(_deviceHandler, callBackData);
                            }
                            break;
                    }
                }
            }

            return true;
        }

        bool R2000Packet_Inventory (byte [] recvData, int offset = 0)
        {
            try
            {
            if (OnAsyncCallback != null)
            {
                CSLibrary.Structures.TagCallbackInfo info = new CSLibrary.Structures.TagCallbackInfo();

                var flags = recvData[offset + 1];
                var pkt_len = (recvData[offset + 4] | (recvData[offset + 5] << 8));
                var ms_ctr = (recvData[offset + 8] | recvData[offset + 9] << 8 | recvData[offset + 10] << 16 | recvData[offset + 11] << 24);
                var wb_rssi = recvData[offset + 12];
                var nb_rssi = recvData[offset + 13];
                var phase = recvData[offset + 14];
                var chidx = recvData[offset + 15];
                var data1count = recvData[offset + 16];
                var data2count = recvData[offset + 17];
                var antennaPort = (recvData[offset + 18] | recvData[offset + 19] << 8);
                var pc = (recvData[offset + 20] << 8 | recvData[offset + 21]);
                var totaldatalen = ((((pkt_len - 3) * 4) - ((flags >> 6) & 3)));
                var crc16 = (recvData[offset + 18 + totaldatalen] << 8 | recvData[offset + 19 + totaldatalen]);

                info.ms_ctr = (UInt32)ms_ctr;
                switch (OEMChipSetID)
                {
                    default:
                        info.rssi = (Single)(nb_rssi * 0.8);
                        break;

                    case ChipSetID.R2000:
                        info.rssi = R2000_RssiTranslation(nb_rssi);
                        break;
                }
                info.rssidBm = Tools.dBConverion.dBuV2dBm(info.rssi);

                if (currentInventoryFreqRevIndex != null)
                {
                    info.freqChannel = (chidx < currentInventoryFreqRevIndex.Length) ? currentInventoryFreqRevIndex[chidx] : (uint)0xff;
                }

                info.antennaPort = (UInt16)antennaPort;
                info.pc = new S_PC((UInt16)pc);
                info.crc16 = (UInt16)crc16;

                {
                    info.phase = (Int16)(phase & 0x3f);
                    if ((phase & 0x40) != 0x00)
                    {
                        UInt16 pvalue = (UInt16)info.phase;
                        pvalue |= 0xffc0;
                        info.phase = (Int16)pvalue;
                    }
                }

                if (data2count > 0)
                {
                    UInt16[] data = new UInt16[data2count];
                    int data2offset = (int)(offset + 20 + totaldatalen - 2 - (data2count * 2));

                    ArrayCopy(recvData, data2offset, data, 0, data2count * 2);
                    info.Bank2Data = data;

                    CSLibrary.Debug.WriteLine("data 2:" + Tools.Hex.ToString(data));
                }

                if (data1count > 0)
                {
                    UInt16[] data = new UInt16[data1count];
                    int data1offset = (int)(offset + 20 + totaldatalen - 2 - ((data2count + data1count) * 2));

                    ArrayCopy(recvData, data1offset, data, 0, data1count * 2);
                    info.Bank1Data = data;

                    CSLibrary.Debug.WriteLine("data 1:" + Tools.Hex.ToString(data));
                }

                int xpcoffset = 0;

                if (info.pc.XI) // Check XPC W1
                {
                    info.xpc_w1 = new S_XPC_W1((UInt16)(recvData[offset + 8 + 12] << 8 | recvData[offset + 8 + 13]));
                    xpcoffset += 2;

                    if (info.xpc_w1.XEB) // Check XPC W2
                    {
                        info.xpc_w2 = new S_XPC_W2((UInt16)(recvData[offset + 8 + 14] << 8 | recvData[offset + 8 + 15]));
                        xpcoffset += 2;
                    }
                }

                
                /*                if (CurrentOperation == Operation.TAG_RANGING)
                {
                    if (_tagRangingParms.multibanks == 2)
                    {
                        UInt16[] data = new UInt16[_tagRangingParms.count2];
                        int data2length = (int)(_tagRangingParms.count2 * 2);
                        int data2offset = (int)(offset + 8 + 14 + info.epcstrlen - data2length);

                        ArrayCopy(recvData, data2offset, data, 0, data2length);
                        info.Bank2Data = data;

                        CSLibrary.Debug.WriteLine("data 1:" + Tools.Hex.ToString(data));

                        info.epcstrlen -= (uint)data2length;
                    }

                    if (_tagRangingParms.multibanks > 0)
                    {
                        UInt16[] data = new UInt16[_tagRangingParms.count1];
                        int data1length = (int)(_tagRangingParms.count1 * 2);
                        int data1offset = (int)(offset + 8 + 14 + info.epcstrlen - data1length);

                        ArrayCopy(recvData, data1offset, data, 0, data1length);
                        info.Bank1Data = data;

                        CSLibrary.Debug.WriteLine("data 2:" + Tools.Hex.ToString(data));

                        info.epcstrlen -= (uint)data1length;
                    }
                }
*/

                byte[] byteEpc = new byte[info.pc.EPCLength * 2];
                Array.Copy(recvData, (int)(offset + 22 + xpcoffset), byteEpc, 0, (int)info.pc.EPCLength * 2);

/*
                if (Options.TagRanging.fastid && info.pc.EPCLength >= 6 && byteEpc[byteEpc.Length - 12] == 0xe2 && byteEpc[byteEpc.Length - 11] == 0x80 && byteEpc[byteEpc.Length - 10] == 0x11)
                {
                    byte[] newbyteEpc = new byte[byteEpc.Length - 12];
                    UInt16[] newbyteTid = new UInt16[6];

                    Array.Copy(byteEpc, 0, newbyteEpc, 0, newbyteEpc.Length);
                    ArrayCopy(byteEpc, byteEpc.Length - 12, newbyteTid, 0, 12);

                    info.FastTid = newbyteTid;
                    byteEpc = newbyteEpc;
                }
*/

                info.epc = new S_EPC(byteEpc);

                switch (CurrentOperation)
                {
                    case Operation.Kiloway_RANGING:
                    case Operation.TAG_RANGING:
                        {
                            CSLibrary.Constants.CallbackType type = CSLibrary.Constants.CallbackType.TAG_RANGING;
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
                        break;

                    case Operation.TAG_SEARCHING:
                        {
                            bool match = true;
                            // mach tag selected
                            if (_tagSelectedParms.bank == MemoryBank.EPC && ((_tagSelectedParms.flags & SelectMaskFlags.ENABLE_PC_MASK) == 0) && _tagSelectedParms.epcMaskLength == 96)
                            {
                                byte [] data = _tagSelectedParms.epcMask.ToBytes();

                                if (data.Length != byteEpc.Length)
                                    break;

                                for (int cnt= 0; cnt < data.Length; cnt++)
                                    if (data[cnt] != byteEpc[cnt])
                                    {
                                        match = false;
                                        break;
                                    }
                            }

                            if (match)
                            {
                                CSLibrary.Constants.CallbackType type = CSLibrary.Constants.CallbackType.TAG_SEARCHING;
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
                            else
                            {
                                CSLibrary.Debug.WriteLine("Found a non-match Tag");
                            }
                        }
                        break;

                    default:
                        return false;
                }
            }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("R2000Packet_Inventory Error : " + ex.Message);
                return false;
            }
            return true;
        }

        bool R2000Packet_TagAccess (byte[] recvData, int offset = 0)
        {
            if ((recvData[offset + 1] & 0x0f) != 0)
                return false;

			Operation RealCurrentOperation = (Operation)(_deviceHandler._currentCmdRemark);

            int packetType = recvData[offset + 12];

            switch (packetType)
            {
                case 0xc2:  // Read
                    switch (RealCurrentOperation)
                    {
                        case CSLibrary.Constants.Operation.TAG_READ:
                            ArrayCopy(recvData, offset + 20, m_rdr_opt_parms.TagRead.m_pData, 0, m_rdr_opt_parms.TagRead.count * 2);
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
							/*
							 *
							 * 
								Win32.memcpy(tagreadbuf, TagAccessPacket, 20, (uint)len);

 * 							 * if (m_Result == Result.OK && !Options.TagBlockLock.setPermalock)
										{
											Options.TagBlockLock.mask = new ushort[Options.TagBlockLock.count];
											Array.Copy(tagreadbuf, Options.TagBlockLock.mask, Options.TagBlockLock.count);
										}
							*/
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
                    {
                        var a = 10;
					}

                    if (_deviceHandler.rfid.FM13DT160.TagAccessProc(RealCurrentOperation, recvData))
                        break;

                    if (_deviceHandler.rfid.EM4325.TagAccessProc(RealCurrentOperation, recvData))            
                        break;

                    return false;

                default:
                    return false;
            }

            return true;
        }

        public void ClearBuffer()
        {
            _dataBuffer.Clear();
        }

        public bool CheckValidRFIDPacket(ref byte [] packetHeader)
        {
            byte pkt_ver = packetHeader[0];
            byte flags = packetHeader[1];
            UInt16 pkt_type = (UInt16)((packetHeader[3] << 8 | packetHeader[2]) & 0x7fff);
            UInt16 pkt_len = (UInt16)(packetHeader[5] << 8 | packetHeader[4]);
            UInt16 reserved = (UInt16)(packetHeader[7] << 8 | packetHeader[6]);

            // Abort Command
            if ((pkt_ver == 0x40) && (flags == 0x03) && (pkt_type == 0x7cbf) && (pkt_len == 0xfcbf) && (reserved == 0xfcbf))
                return true;

            if (_readerMode == 0)
            {
                // Idle mode

                // Read Mac Register packet
                if ((pkt_ver == 0x70 || pkt_ver == 0x00) && (flags == 0x00))
                    return true;
            }
            else
            {
                // Command mode

                // Command-Begin Packet
                if ((pkt_ver == 0x01 || pkt_ver == 0x02) && (pkt_type == 0x0000) && (pkt_len == 0x02) && (reserved == 0x0000))
                    return true;

                // Command-End Packet 
                if ((pkt_ver == 0x01 || pkt_ver == 0x02) && (flags == 0x00) && (pkt_type == 0x0001) && (pkt_len == 0x02) && (reserved == 0x0000))
                    return true;

                // OEM Configuration Read Packet
                if ((pkt_ver == 0x01) && (flags == 0x00) && (pkt_type == 0x3007) && (pkt_len == 0x0002))
                    return true;

                // Inventory-Response Packet 
                if ((pkt_ver == 0x03) && (pkt_type == 0x0005) && (reserved == 0x0000))
                    return true;

                // Inventory-Response Packet (Compact mode)
                //if ((pkt_ver == 0x04) && (pkt_type == 0x0005) && (reserved == 0x0000))
                if ((pkt_ver == 0x04) && (pkt_type == 0x0005))
                    return true;

                // Tag-Access Packet 
                if ((pkt_ver == 0x01) && (pkt_type == 0x0006) && (reserved == 0x0000))
                    return true;

                // Antenna-Cycle-End Packet
                if ((pkt_ver == 0x01 || pkt_ver == 0x02) && (flags == 0x00) && (pkt_type == 0x0007) && (pkt_len == 0x0000) && (reserved == 0x0000))
                    return true;

                // LBT-Jam-Detected Packet 
                if ((pkt_ver == 0x01 || pkt_ver == 0x02) && (flags == 0x00) && (pkt_type == 0x0008) && (pkt_len == 0x0001))
                    return true;
            }

            //CSLibrary.Debug.WriteLine("Packet Check Fail : pkt_ver:" +); 
            return false;
        }


        /// <summary>
        /// Transfer BT API packet to R2000 packet
        /// </summary>
        /// <param name="recvData"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        internal CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE DeviceRecvData_CS108(byte[] recvData1, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE currentCommandResponse)
        {
            CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE result = HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.NOWAIT;

            try
            {
            CSLibrary.Debug.WriteLine("Routine : DeviceRecvData");

            if (!_dataBuffer.DataIn(recvData1, 10, recvData1[2] - 2))
                CSLibrary.Debug.WriteLine("RFID ring buffer FULL!!!!");

            while (_dataBuffer.length >= 8)
            {
                byte[] recvData = _dataBuffer.DataPreOut(8);

                if (!CheckValidRFIDPacket(ref recvData))
                {
                    // drop packet if not is valid RFID packet
                    _dataBuffer.TrimRFIDPacket(_readerMode);
                    continue;
                }

                // first packet
                byte header = recvData[0];

                CSLibrary.Debug.WriteLine("packet type :0x{0}", header.ToString("X2"));
                switch (header)
                {
                    default:
                        _dataBuffer.Clear();
                        CSLibrary.Debug.WriteLine("Can not handle R2000 packet type :0x{0}", header.ToString("X2"));
                        break;
                    case 0x40:  // Abort packet
                        {
                            _deviceHandler.battery.DisbleAutoBatteryLevel();
                            _readerMode = 0; // record reader status to idle mode

                            //result |= HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA2 | HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
                            result |= HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA2;
                            result |= HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
                            result = result | HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA2 | HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
                            _dataBuffer.DataDel(8);
                            LastMacErrorCode = 0x0000;
                            FireStateChangedEvent(CSLibrary.Constants.RFState.IDLE);
//                            _deviceHandler.rfid.SetToStandbyMode();
                        }
                        break;

                    case 0x00:
                    case 0x70:  // register read packet
                        {
                            UInt16 add = (UInt16)(recvData[3] << 8 | recvData[2]);
                            UInt32 data = (UInt32)(recvData[7] << 24 | recvData[6] << 16 | recvData[5] << 8 | recvData[4]);

                            SaveMacRegister(add, data);

                            result |= HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1;

                            _dataBuffer.DataDel(8);
                        }
                        break;

                    case 0x04:  // New Inventory Packet

                        //CSLibrary.HighLevelInterface._debugBLEHold = true;

                        // only valid on inventory compact mode
                        //if ((Operation)(_deviceHandler._currentCmdRemark) == Operation.TAG_RANGING)

                        {
                            int pkt_type = BitConverter.ToUInt16(recvData, 2) & 0x7fff;
                            int packetLen = BitConverter.ToUInt16(recvData, 4) + 8;

                            if (packetLen > _dataBuffer.length)
                                return result;

                            switch (pkt_type)
                            {
                                case 0x0005:    /// inventory packet
									{
                                        InventoryDebug.InventoryPackerCountInc();

                                        byte[] packetData = _dataBuffer.DataOut(packetLen);

                                        R2000Packet_NewInventory(packetData);
                                    }
                                    break;
                            }
                        }
                        break;

                    case 0x01:
                    case 0x02:
                    case 0x03:
                        {
                            int pkt_type = BitConverter.ToUInt16(recvData, 2) & 0x7fff;
                            int packetLen = BitConverter.ToUInt16(recvData, 4) * 4 + 8;

                            if (packetLen > _dataBuffer.length)
                                return result;

                            switch (pkt_type)
                            {
                                default:
                                    _dataBuffer.DataDel(packetLen);
                                    break;

                                case 0x0000:    // Command begin Packet
                                    _dataBuffer.DataDel(packetLen);
                                    break;

                                case 0x0001:    // Command end Packet
                                    _readerMode = 0; // record reader status to idle mode

                                    result |= HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.COMMANDENDRESPONSE;
                                    //_dataBuffer.DataDel(packetLen);
                                    {
                                        byte[] packetData = _dataBuffer.DataOut(packetLen);
                                        LastMacErrorCode = BitConverter.ToUInt32(packetData, 12);
                                    }

                                    // Check Tag access packet
                                    // if ((currentCommandResponse | HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0)
                                    {
                                        Operation RealCurrentOperation = (Operation)(_deviceHandler._currentCmdRemark);

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
//                                    _deviceHandler.rfid.SetToStandbyMode();

                                    break;

                                case 0x0005:    /// inventory packet
									{
                                        InventoryDebug.InventoryPackerCountInc();

                                        byte[] packetData = _dataBuffer.DataOut(packetLen);

                                        R2000Packet_Inventory(packetData);
                                    }
                                    break;

                                case 0x0006:    // Tag access Packet
                                    {
                                        byte[] packetData = _dataBuffer.DataOut(packetLen);

                                        if (R2000Packet_TagAccess(packetData))
                                            result |= HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1;
                                    }
                                    break;

                                case 0x0007:    // Antenna-Cycle-End Packet
                                    {
                                        byte[] packetData = _dataBuffer.DataOut(packetLen);
                                    }
                                    break;

                                case 0x000A:    // INVENTORY_CYCLE_BEGIN
                                    {
                                        byte[] packetData = _dataBuffer.DataOut(packetLen);
                                    }
                                    break;

                                case 0x3007:    // RFID_PACKET_OEMCFG_READ
                                    {
                                        byte[] packetData = _dataBuffer.DataOut(packetLen);

                                        var m_OEMReadAdd = (UInt32)(packetData[8] | packetData[9] << 8 | packetData[10] << 16 | packetData[11] << 24);
                                        var m_OEMReadData = (UInt32)(packetData[12] | packetData[13] << 8 | packetData[14] << 16 | packetData[15] << 24);

                                        StoreOEMData(m_OEMReadAdd, m_OEMReadData);

                                        result |= HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1;
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("DeviceRecvData_CS108 Error : " + ex.Message);
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////
        // Name:        Radio::WriteMacMaskRegisters
        // Description: Writes the MAC mask registers (select or post-singulation).
        ////////////////////////////////////////////////////////////////////////////////
        void WriteMacMaskRegisters(UInt16 registerAddress, UInt32 bitCount, byte[] pMask)
        {
			const int BITS_PER_BYTE = 8;
            const int BYTES_PER_REGISTER = 4;
            const int BITS_PER_REGISTER = BITS_PER_BYTE * BYTES_PER_REGISTER;
            int pcnt = 0;

            // Figure out how many bytes are in the mask
            UInt32 byteCount = (bitCount + 7) / 8;

            // Now write each MAC mask register
            while (byteCount > 0)
            {
                    UInt32 registerValue = 0;
                    int leftShift = 0;
                    UInt32 loopCount = (byteCount > BYTES_PER_REGISTER ? BYTES_PER_REGISTER : byteCount);

                    // Decrement the byte count by the number of bytes put into the register
                    byteCount -= loopCount;

                    // Build up the register value
                    for (int cnt = 0; cnt < loopCount; cnt++)
                    {
                        registerValue |= ((uint)pMask[pcnt++] << leftShift);
                        leftShift += BITS_PER_BYTE;
                    }

                    // If it is the last byte of the mask, then we are going to zero out
                    // any bits not in the mask
                    if (byteCount == 0 && (bitCount % BITS_PER_BYTE) != 0)
                    {
                        UInt32 mask = 0xFFFFFFFF;
                        mask <<= (int)(BITS_PER_REGISTER - (BITS_PER_BYTE - (bitCount % BITS_PER_BYTE)));
                        mask >>= (int)(BITS_PER_REGISTER - (leftShift - (bitCount % BITS_PER_BYTE)));
                        registerValue &= ~mask;
                    }

                // Write the register
                MacWriteRegister((MACREGISTER)(registerAddress++), registerValue);
           }
       } // Radio::WriteMacMaskRegisters

        /// <summary>
        /// Configures the tag-selection criteria for the ISO 18000-6C select 
        /// command.  The supplied tag-selection criteria will be used for any 
        /// tag-protocol operations (i.e., Inventory, etc.) in 
        /// which the application specifies that an ISO 18000-6C select 
        /// command should be issued prior to executing the tag-protocol 
        /// operation (i.e., the SelectFlags.SELECT flag is provided to 
        /// the appropriate RFID_18K6CTag* function).  The tag-selection 
        /// criteria will stay in effect until the next call to 
        /// SetSelectCriteria.  Tag-selection criteria may not 
        /// be changed while a radio module is executing a tag-protocol 
        /// operation. 
        /// </summary>
        /// <param name="critlist">
        /// SelectCriteria array, containing countCriteria entries, of selection 
        /// criterion structures that are to be applied sequentially, beginning with 
        /// pCriteria[0], to the tag population.  If this field is NULL, 
        /// countCriteria must be zero. 
        ///</param>
        /// <returns></returns>

        internal Result SetSelectCriteria_CS108(SelectCriterion[] critlist)
        {
            uint index;
            uint registerValue;

            if (critlist == null || critlist.Length == 0)
                return Result.INVALID_PARAMETER;

            try
            {
                SelectCriteria SC = new SelectCriteria();
                SC.countCriteria = (uint)critlist.Length;
                SC.pCriteria = (SelectCriterion[])critlist.Clone();

                for (index = 0; index < critlist.Length; index++)
                {
                    SelectCriterion pCriterion = SC.pCriteria[index];
                    SelectMask pMask = pCriterion.mask;
                    SelectAction pAction = pCriterion.action;

                    // Instruct the MAC as to which select mask we want to work with
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_SEL, index);

                    // Create the HST_TAGMSK_DESC_CFG register value and write it to the MAC
                    registerValue = (0x01 |
                        (((uint)(pAction.target) & 0x07) << 1) |
                        (((uint)(pAction.action) & 0x07) << 4) |
                        (pAction.enableTruncate != 0x00 ? (uint)(1 << 7) : 0)) |
                        (((uint)(pAction.delay) & 0xFF) << 8) ;
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_CFG, registerValue);

                    // Create the HST_TAGMSK_BANK register value and write it to the MAC
                    registerValue = (uint)pMask.bank;
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_BANK, registerValue);

                    // Write the mask offset to the HST_TAGMSK_PTR register
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_PTR, (uint)pMask.offset);

                    // Create the HST_TAGMSK_LEN register and write it to the MAC
                    registerValue = (uint)(pMask.count);
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_LEN, registerValue);

                    // Now write the MAC's mask registers
                    WriteMacMaskRegisters((ushort)MACREGISTER.HST_TAGMSK_0_3, pMask.count, pMask.mask);
                    // Set up the selection criteria
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                //				CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.SetSelectCriteria()", ex);
#endif
                return Result.SYSTEM_CATCH_EXCEPTION;
            }
            return m_Result;
        }

        internal Result CancelAllSelectCriteria_CS108()
        {
            for (uint cnt = 0; cnt < 7; cnt++)
            {
                SetSelectCriteria_CS108(cnt, null);
            }

            return Result.OK;
        }

        internal Result SetSelectCriteria_CS108(uint index, SelectCriterion crit)
        {
            uint registerValue;

            // Instruct the MAC as to which select mask we want to work with
            MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_SEL, index);

            if (crit == null)
            {
                MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_CFG, 0x0000);
                //MacWriteRegister(MACREGISTER.HST_TAGMSK_BANK, 0x0000);
                //MacWriteRegister(MACREGISTER.HST_TAGMSK_PTR, 0x0000);
                //MacWriteRegister(MACREGISTER.HST_TAGMSK_LEN, 0x0000);
                return Result.OK;
            }

            try
            {
                {
                    SelectCriterion pCriterion = crit;
                    SelectMask pMask = pCriterion.mask;
                    SelectAction pAction = pCriterion.action;

                    // Create the HST_TAGMSK_DESC_CFG register value and write it to the MAC
                    registerValue = (0x01 |
                        (((uint)(pAction.target) & 0x07) << 1) |
                        (((uint)(pAction.action) & 0x07) << 4) |
                        (pAction.enableTruncate != 0x00 ? (uint)(1 << 7) : 0)) |
                        (((uint)(pAction.delay) & 0xFF) << 8);
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_CFG, registerValue);

                    // Create the HST_TAGMSK_BANK register value and write it to the MAC
                    registerValue = (uint)pMask.bank;
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_BANK, registerValue);

                    // Write the mask offset to the HST_TAGMSK_PTR register
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_PTR, (uint)pMask.offset);

                    // Create the HST_TAGMSK_LEN register and write it to the MAC
                    registerValue = (uint)(pMask.count);
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_LEN, registerValue);

                    // Now write the MAC's mask registers
                    WriteMacMaskRegisters((ushort)MACREGISTER.HST_TAGMSK_0_3, pMask.count, pMask.mask);
                    // Set up the selection criteria
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                //				CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.SetSelectCriteria()", ex);
#endif
                return Result.SYSTEM_CATCH_EXCEPTION;
            }
            return m_Result;
        }

        /// <summary>
        /// Configures the post-singulation match criteria to be used by the 
        /// RFID radio module.  The supplied post-singulation match criteria 
        /// will be used for any tag-protocol operations (i.e., 
        /// Inventory, etc.) in which the application specifies 
        /// that a post-singulation match should be performed on the tags 
        /// that are singulated by the tag-protocol operation (i.e., the 
        /// SelectFlags.POST_MATCH flag is provided to the 
        /// appropriate RFID_18K6CTag* function).  The post-singulation 
        /// match criteria will stay in effect until the next call to 
        /// SetPostMatchCriteria.  Post-singulation match 
        /// criteria may not be changed while a radio module is executing a 
        /// tag-protocol operation. 
        /// </summary>
        /// <param name="postmatch"> An array that specifies the post-
        /// singulation match criteria that are to be 
        /// applied to the tag's Electronic Product Code 
        /// after it is singulated to determine if it is to 
        /// have the tag-protocol operation applied to it.  
        /// If the countCriteria field is zero, all post-
        /// singulation criteria will be disabled.  This 
        /// parameter must not be NULL. </param>
        /// <returns></returns>
        internal Result SetPostMatchCriteria_CS108(SingulationCriterion[] postmatch)
		{
			UInt32 registerValue;

			try
			{
				if (postmatch.Length != 0)
				{
					// Set up the post-singulation match criteria
					//                    pCriterion = pParms->pCriteria;
					//                    const RFID_18K6C_SINGULATION_MASK* pMask = &pCriterion->mask;

					SingulationMask pMask = postmatch[0].mask;

					// Set up the HST_INV_EPC_MATCH_CFG register and write it to the MAC.
					// For now, we are going to assume that the singulation match should be
					// enabled (if the application so desires, we can turn it off when we
					// actually do the tag-protocol operation).
					registerValue =
                        (uint)(postmatch[0].match != 0 ? 0 : 2) |
						(uint)(postmatch[0].mask.count << 2) |
						(uint)(postmatch[0].mask.offset << 11);

                    MacWriteRegister(MACREGISTER.HST_INV_EPC_MATCH_SEL, 0X00);
					MacWriteRegister(MACREGISTER.HST_INV_EPC_MATCH_CFG, registerValue);

					// Now write the MAC's mask registers
					WriteMacMaskRegisters((UInt16)MACREGISTER.HST_INV_EPCDAT_0_3, pMask.count, pMask.mask);
				}
				else // must be calling to disable criteria
				{
					MacWriteRegister(MACREGISTER.HST_INV_EPC_MATCH_CFG, 0);
				}
			}
			catch (System.Exception ex)
			{
#if DEBUG
//				CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.SetSelectCriteria()", ex);
#endif
				return Result.SYSTEM_CATCH_EXCEPTION;
			}
			return m_Result;
		}

        #region ====================== Set Tag Group ======================
        /// <summary>
        /// Get Tag Group
        /// </summary>
        /// <param name="gpSelect"></param>
        /// <returns></returns>
        public Result SetTagGroup_CS108(Selected gpSelect)
        {
            UInt32 value = 0;

            MacReadRegister(MACREGISTER.HST_QUERY_CFG, ref value);

            value &= ~0x0180U;
            value |= ((uint)gpSelect << 7);

            MacWriteRegister(MACREGISTER.HST_QUERY_CFG, value);

            return Result.OK;
        }

        /// <summary>
        /// Once the tag population has been partitioned into disjoint groups, a subsequent 
        /// tag-protocol operation (i.e., an inventory operation or access command) is then 
        /// applied to one of the tag groups. 
        /// </summary>
        /// <param name="gpSelect">Specifies the state of the selected (SL) flag for tags that will have 
        /// the operation applied to them. </param>
        /// <param name="gpSession">Specifies which inventory session flag (i.e., S0, S1, S2, or S3) 
        /// will be matched against the inventory state specified by target. </param>
        /// <param name="gpSessionTarget">Specifies the state of the inventory session flag (i.e., A or B),
        /// specified by session, for tags that will have the operation 
        /// applied to them. </param>
        public Result SetTagGroup_CS108(Selected gpSelect, Session gpSession, SessionTarget gpSessionTarget)
		{
			UInt32 value = 0;

			//DEBUG_WriteLine(DEBUGLEVEL.API, "HighLevelInterface.SetTagGroup(Selected gpSelect, Session gpSession, SessionTarget gpSessionTarget)");

			MacReadRegister(MACREGISTER.HST_QUERY_CFG, ref value);

			value &= ~0x01f0U;
			value |= ((uint)gpSessionTarget << 4) | ((uint)gpSession << 5) | ((uint)gpSelect << 7);

			MacWriteRegister(MACREGISTER.HST_QUERY_CFG, value);

			return Result.OK;

			/*            return (m_Result = MacWriteRegister(MACREGISTER.HST_QUERY_CFG,
							(uint)gpSessionTarget << 4 |
							(uint)gpSession << 5 |
							(uint)gpSelect << 7));*/
		}

        /// <summary>
        /// Once the tag population has been partitioned into disjoint groups, a subsequent 
        /// tag-protocol operation (i.e., an inventory operation or access command) is then 
        /// applied to one of the tag groups. 
        /// </summary>
        /// <param name="tagGroup"><see cref="TagGroup"/></param>
        /// <returns></returns>
        public Result SetTagGroup_CS108(TagGroup tagGroup)
		{
			UInt32 value = 0;

			//DEBUG_WriteLine(DEBUGLEVEL.API, "HighLevelInterface.SetTagGroup(TagGroup tagGroup)");

			MacReadRegister(MACREGISTER.HST_QUERY_CFG, ref value);

			value &= ~0x01f0U;
			value |= ((uint)tagGroup.target << 4) | ((uint)tagGroup.session << 5) | ((uint)tagGroup.selected << 7);

			MacWriteRegister(MACREGISTER.HST_QUERY_CFG, value);

			return Result.OK;

			/*            return (m_Result = MacWriteRegister(MACREGISTER.HST_QUERY_CFG,
							(uint)tagGroup.target << 4 |
							(uint)tagGroup.session << 5 |
							(uint)tagGroup.selected << 7));
			*/
		}
		/// <summary>
		/// Get Tag Group
		/// </summary>
		/// <param name="tagGroup"></param>
		/// <returns></returns>
		internal Result GetTagGroup_CS108(ref TagGroup tagGroup)
		{
			//UInt16 HST_QUERY_CFG = 0x0900;

			UInt32 registerValue = 0;

			//DEBUG_WriteLine(DEBUGLEVEL.API, "HighLevelInterface.GetTagGroup(TagGroup tagGroup)");

			MacReadRegister(MACREGISTER.HST_QUERY_CFG, ref registerValue);

			tagGroup.selected = (Selected)((registerValue >> 7) & 0x03);
			tagGroup.session = (Session)((registerValue >> 5) & 0x03);
			tagGroup.target = (SessionTarget)((registerValue >> 4) & 0x01);

			return (m_Result = Result.OK);
		}
#endregion


        internal UInt32 _INVENTORYDELAYTIME = (7 << 20);

        internal bool SetTagDelayTime_CS108(UInt32 ms)
        {
            if (ms > 0x3f)
                return false;

            _INVENTORYDELAYTIME = (ms << 20);

            UInt32 value = 0;

            MacReadRegister(MACREGISTER.HST_INV_CFG, ref value);

            value &= ~(0x03f00000U);
            value |= (ms << 20);

            MacWriteRegister(MACREGISTER.HST_INV_CFG, value);

            return true;
        }

        #region Public Functions

		internal void Connect_CS108()
		{
            MacRegisterInitialize();
            ReadReaderOEMData();
        }

        internal void Reconnect_CS108()
		{
		}

        internal UInt32 GetFirmwareVersion_CS108()
        {
            UInt32 value = 0, value1;

            MacReadRegister(MACREGISTER.MAC_VER, ref value);

            value1 = (0xfff & value) | ((0xfff & (value >> 12)) << 8) | ((0xff & (value >> 24)) << 16);

            return value1;
        }

        internal string GetFirmwareVersionString_CS108()
        {
            var ver = GetFirmwareVersion_CS108();
            var verString = string.Format("{0}.{1}.{2}", (ver >> 16) & 0xff, (ver >> 8) & 0xff, ver & 0xff);

            return verString;
        }



        /// <summary>
        /// Retrieves the operation mode for the RFID radio module.  The 
        /// operation mode cannot be retrieved while a radio module is 
        /// executing a tag-protocol operation. 
        /// </summary>
        /// <param name="cycles">The number of antenna cycles to be completed for command execution.
        /// <para>0x0001 = once cycle through</para>
        /// <para>0xFFFF = cycle forever until a CANCEL is received.</para></param>
        /// <param name="mode">Antenna Sequence mode.</param>
        /// <param name="sequenceSize">Sequence size. Maximum value is 48</param>
        /// <returns></returns>
        public Result GetOperationMode_CS108(ref ushort cycles, ref AntennaSequenceMode mode, ref uint sequenceSize)
		{
			uint value = 0;

			MacReadRegister(MACREGISTER.HST_ANT_CYCLES /*0x700*/, ref value);

			cycles = (ushort)(0xffff & value);
			mode = (AntennaSequenceMode)((value >> 16) & 0x3);
			sequenceSize = (value >> 18) & 0x3F;

			return Result.OK;
		}

		/// <summary>
		/// Retrieves the operation mode for the RFID radio module.  The 
		/// operation mode cannot be retrieved while a radio module is 
		/// executing a tag-protocol operation. 
		/// </summary>
		/// <param name="mode"> return will receive the current operation mode.</param>
		/// <returns></returns>
		public void GetOperationMode_CS108(ref RadioOperationMode mode)
		{
			UInt32 value = 0;

			MacReadRegister(MACREGISTER.HST_ANT_CYCLES /*0x700 HST_ANT_CYCLES*/, ref value);

			if ((value & 0xffff) == 0xffff)
				mode = RadioOperationMode.CONTINUOUS;
			else
				mode = RadioOperationMode.NONCONTINUOUS;
		}

		/// <summary>
		/// Sets the operation mode of RFID radio module.  By default, when 
		/// an application opens a radio, the RFID Reader Library sets the 
		/// reporting mode to non-continuous.  An RFID radio module's 
		/// operation mode will remain in effect until it is explicitly changed 
		/// via RFID_RadioSetOperationMode, or the radio is closed and re-
		/// opened (at which point it will be set to non-continuous mode).  
		/// The operation mode may not be changed while a radio module is 
		/// executing a tag-protocol operation. 
		/// </summary>
		/// <param name="mode">The operation mode for the radio module.</param>
		/// <returns></returns>
		public Result SetOperationMode_CS108(RadioOperationMode mode)
		{
			AntennaSequenceMode smode = AntennaSequenceMode.NORMAL;
			uint sequenceSize = 0;

			if (RadioOperationMode.UNKNOWN == mode)
				return Result.INVALID_PARAMETER;

			SetOperationMode_CS108((ushort)(mode == RadioOperationMode.CONTINUOUS ? 0xFFFF : 1), smode, sequenceSize);

			return Result.OK;
		}

		/// <summary>
		/// Sets the operation mode of RFID radio module.  By default, when 
		/// an application opens a radio, the RFID Reader Library sets the 
		/// reporting mode to non-continuous.  An RFID radio module's 
		/// operation mode will remain in effect until it is explicitly changed 
		/// via RFID_RadioSetOperationMode, or the radio is closed and re-
		/// opened (at which point it will be set to non-continuous mode).  
		/// The operation mode may not be changed while a radio module is 
		/// executing a tag-protocol operation. 
		/// </summary>
		/// <param name="cycles">The number of antenna cycles to be completed for command execution.
		/// <para>0x0001 = once cycle through</para>
		/// <para>0xFFFF = cycle forever until a CANCEL is received.</para></param>
		/// <returns></returns>
		public Result SetOperationMode_CS108(UInt16 cycles)
		{
			bool result = false;
			uint value = 0, value1 = 0;

			if (!MacReadRegister(MACREGISTER.HST_ANT_CYCLES/*0x700*/, ref value))
				return Result.FAILURE;

			value1 = cycles;
			if (((value >> 24) & 0x01) != 0)
				value1 |= (0x01 << 24);

			MacWriteRegister(MACREGISTER.HST_ANT_CYCLES/*0x700*/, value1);

			return Constants.Result.OK;
		}

		/// <summary>
		/// Sets the operation mode of RFID radio module.  By default, when 
		/// an application opens a radio, the RFID Reader Library sets the 
		/// reporting mode to non-continuous.  An RFID radio module's 
		/// operation mode will remain in effect until it is explicitly changed 
		/// via RFID_RadioSetOperationMode, or the radio is closed and re-
		/// opened (at which point it will be set to non-continuous mode).  
		/// The operation mode may not be changed while a radio module is 
		/// executing a tag-protocol operation. 
		/// </summary>
		/// <param name="cycles">The number of antenna cycles to be completed for command execution.
		/// <para>0x0001 = once cycle through</para>
		/// <para>0xFFFF = cycle forever until a CANCEL is received.</para></param>
		/// <param name="mode">Antenna Sequence mode.</param>
		/// <param name="sequenceSize">Sequence size. Maximum value is 48</param>
		/// <returns></returns>
		public Result SetOperationMode_CS108(ushort cycles, AntennaSequenceMode mode = AntennaSequenceMode.NORMAL, uint sequenceSize = 0)
		{
			uint value = 0;

			if (sequenceSize > 48)
				return Result.INVALID_PARAMETER;

			value = (cycles | ((uint)mode & 0x3) << 16 | (sequenceSize & 0x3F) << 18);
			MacWriteRegister(MACREGISTER.HST_ANT_CYCLES, value);
            return Result.OK;
		}

        /// <summary>
        /// Sets the radio's operation mode.  An RFID radio module operation mode
        /// will remain in effect until it is explicitly changed via RadioSetOperationMode
        /// </summary>
        /// <param name="operationMode">The operation mode for the radio module.</param>
        /// <param name="sequenceMode">The antenna sequence mode for the radio module.</param>
        /// <param name="sequenceSize">The antenna sequence size for the radio module. This must be between 0 to 48.</param>
        /// <returns></returns>
        public Result SetOperationMode_CS108(RadioOperationMode operationMode, AntennaSequenceMode sequenceMode, int sequenceSize)
        {
            String __FUNCTION__ = "SetOperationMode";

            if (m_state == RFState.BUSY)
                return Result.RADIO_BUSY;

            try
            {
                // Validate the operation mode
                switch (operationMode)
                {
                    // Valid operation modes
                    case RadioOperationMode.CONTINUOUS:
                    case RadioOperationMode.NONCONTINUOUS:
                        {
                            break;
                        }
                    // Invalid operation modes
                    default:
                        {
                            return Result.INVALID_PARAMETER;
                        }
                } // switch (mode)

                switch (sequenceMode)
                {
                    case AntennaSequenceMode.NORMAL:
                    case AntennaSequenceMode.SEQUENCE:
                    case AntennaSequenceMode.SMART_CHECK:
                    case AntennaSequenceMode.SEQUENCE_SMART_CHECK:
                        break;
                    default:
                        {
                            return Result.INVALID_PARAMETER;
                        }
                }

                if (sequenceSize < 0 || sequenceSize > 48)
                {
                    return Result.INVALID_PARAMETER;
                }

                // Let the radio object set the operation mode
                uint old_data = 0;
                FreqAgileMode AgileEnable;

                MacReadRegister(MACREGISTER.HST_ANT_CYCLES, ref old_data);

                if ((old_data & (1 << 24)) == 0)
                    AgileEnable = FreqAgileMode.DISABLE;
                else
                    AgileEnable = FreqAgileMode.ENABLE;

                // Set the antenna cycles register to either perform a single cycle or to
                // cycle until a cancel
                UInt32 registerValue = (UInt32)(((operationMode == RadioOperationMode.CONTINUOUS) ? 0xffff : 0x00001) |
                                                ((int)sequenceMode << 16) |
                                                ((int)sequenceSize << 18) |
                                                ((AgileEnable == FreqAgileMode.ENABLE) ? 1 << 24 : 0x0000));

                MacWriteRegister(MACREGISTER.HST_ANT_CYCLES, registerValue);

                //m_save_antenna_cycle = operationMode;
                //m_save_antenna_cycle_sequence_mode = sequenceMode;
                //m_save_antenna_cycle_sequence_size = sequenceSize;
            }
            catch (Exception ex)
            {
            }

            return Result.OK;
        }

        /// <summary>
        /// This is used to set inventory duration
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public Result SetInventoryDuration_CS108(uint duration, uint antennaPort = 0)
		{
			MacWriteRegister(MACREGISTER.HST_ANT_DESC_SEL, antennaPort);
			MacWriteRegister(MACREGISTER.HST_ANT_DESC_DWELL, duration);

			return Result.OK;
		}

        public Result SetInventoryDuration_CS108(uint [] duration)
        {
            Result r;

			for (uint cnt = 0; cnt < duration.Length; cnt++)
                if ((r = SetInventoryDuration(duration[cnt], cnt)) != Result.OK)
                    return r;

            return Result.OK;
        }

        #endregion

    }
}
