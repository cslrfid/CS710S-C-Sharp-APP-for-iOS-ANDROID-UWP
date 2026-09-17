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
    public partial class RFIDReader
    {
        const int MAX_WR_CNT_CS108 = 0x20;

        void Setup18K6CWriteRegisters_CS108(CSLibrary.Constants.MemoryBank WriteBank, uint WriteOffset, uint WriteSize, UInt16[] WriteBuf, uint BufOffset)
        {
            int offset;
            int pcnt = 0;

            // Set up the tag bank register (tells where to write the data)
            MacWriteRegister(MACREGISTER.HST_TAGACC_BANK, (uint)WriteBank);

            // Set the offset
            //MacWriteRegister(MACREGISTER.HST_TAGACC_PTR, WriteOffset);
            MacWriteRegister(MACREGISTER.HST_TAGACC_PTR, 0);

            // Set up the access count register (i.e., number of words to write)
            MacWriteRegister(MACREGISTER.HST_TAGACC_CNT, WriteSize);

            // Set up the HST_TAGWRDAT_N registers.  Fill up a bank at a time.
            for (UInt32 registerBank = 0; WriteSize > 0; registerBank++)
            {
                uint value = 0;

                // Indicate which bank of tag write registers we are going to fill
                MacWriteRegister(MACREGISTER.HST_TAGWRDAT_SEL, registerBank);

                /*
				MacReadRegister(MACREGISTER.MAC_ERROR, ref value);

				if (value == HOSTIF_ERR_SELECTORBNDS)
				{
					MacClearError();
					return;
				}
				*/

                // Write the values to the bank until either the bank is full or we run out of data
                UInt16 registerAddress = (UInt16)MACREGISTER.HST_TAGWRDAT_0;
                offset = 0;

                while ((WriteSize > 0) && (offset < 16 /*RFID_NUM_TAGWRDAT_REGS_PER_BANK*/))
                {
                    // Set up the register and then write it to the MAC
                    UInt32 registerValue = (uint)(WriteBuf[BufOffset + pcnt] | ((WriteOffset + pcnt) << 16));

                    MacWriteRegister((MACREGISTER)(registerAddress), registerValue);

                    pcnt++;
                    registerAddress++;
                    offset++;
                    WriteSize--;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bank"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="data"></param>
        /// <param name="password"></param>
        /// <param name="retry"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private CSLibrary.Constants.Result CUST_18K6CTagWrite_CS108(
            CSLibrary.Constants.MemoryBank bank,
            UInt32 offset,
            UInt32 count,
            UInt16[] data,
            UInt32 password,
            CSLibrary.Constants.SelectFlags flags
        )
        {
            if (count > MAX_WR_CNT)
                return CSLibrary.Constants.Result.DEVICE_NOT_SUPPORT;       // too many data


            MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, password);
            Start18K6CRequest(1, flags);
            MacWriteRegister(MACREGISTER.HST_TAGACC_DESC_CFG  /*0xA01*/, (31 << 1) | 0x01); // Enable write verify and set retry count
                                                                                            //MacWriteRegister(MACREGISTER.HST_TAGACC_DESC_CFG  /*0xA01*/, 0x1ff); // Enable write verify and set retry count
            Setup18K6CWriteRegisters(bank, offset, count, data, 0);
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.WRITE), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (UInt32)CurrentOperation);

            return CSLibrary.Constants.Result.OK;
        }

        private void WriteThreadProc_CS108()
        {
            try
            {
                FireStateChangedEvent(CSLibrary.Constants.RFState.BUSY);

                UInt16[] readData = new UInt16[m_rdr_opt_parms.TagWrite.count];
                UInt16[] writeData = m_rdr_opt_parms.TagWrite.pData;
                UInt16[] readCmp = new UInt16[MAX_WR_CNT];
                bool status;

                m_Result = CSLibrary.Constants.Result.OK;

                m_Result = CUST_18K6CTagWrite(
                    m_rdr_opt_parms.TagWrite.bank,
                    (uint)(m_rdr_opt_parms.TagWrite.offset),
                    m_rdr_opt_parms.TagWrite.count,
                    writeData,
                    m_rdr_opt_parms.TagWrite.accessPassword,
                    CSLibrary.Constants.SelectFlags.SELECT);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                //                CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.TagWriteUsrMemThreadProc()", ex);
#endif
            }
            finally
            {
            }
        }

        private void TagWritePCThreadProc_CS108()
        {
            UInt16[] readData = new UInt16[1];
            UInt16[] writeData = new UInt16[1];

            try
            {
                FireStateChangedEvent(CSLibrary.Constants.RFState.BUSY);

                //                m_Result = TagWritePC(m_rdr_opt_parms.TagWritePC);

                writeData[0] = m_rdr_opt_parms.TagWritePC.pc;
                                m_Result = CUST_18K6CTagWrite(
                                    CSLibrary.Constants.MemoryBank.EPC,
                                    PC_START_OFFSET,
                                    ONE_WORD_LEN,
                                    writeData,
                                    m_rdr_opt_parms.TagWritePC.accessPassword,
                                    //m_rdr_opt_parms.TagWritePC.retryCount,
                                    //m_rdr_opt_parms.TagWritePC.writeRetryCount,
                                    CSLibrary.Constants.SelectFlags.SELECT);
                }
                catch (System.Exception ex)
                {
    #if DEBUG
    //                CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.TagWritePCThreadProc()", ex);
    #endif
                }
                finally
                {
/*                            FireAccessCompletedEvent(
                        new OnAccessCompletedEventArgs(
                        m_Result == Result.OK,
                        Bank.PC,
                        TagAccess.WRITE,
                        new S_PC(m_rdr_opt_parms.TagWritePC.pc)));

                    FireStateChangedEvent(RFState.IDLE);
*/              }
        }

        private void TagWriteEPCThreadProc_CS108()
        {
            try
            {
                FireStateChangedEvent(CSLibrary.Constants.RFState.BUSY);

                UInt16[] readData = new UInt16[m_rdr_opt_parms.TagWriteEPC.count];
                UInt16[] writeData = m_rdr_opt_parms.TagWriteEPC.epc.ToUshorts();
                UInt16[] readCmp = new UInt16[MAX_WR_CNT];
                bool status;

                m_Result = CSLibrary.Constants.Result.OK;

                m_Result = CUST_18K6CTagWrite(
                    CSLibrary.Constants.MemoryBank.EPC,
                    (uint)(EPC_START_OFFSET + m_rdr_opt_parms.TagWriteEPC.offset),
                    m_rdr_opt_parms.TagWriteEPC.count,
                    writeData,
                    m_rdr_opt_parms.TagWriteEPC.accessPassword,
                    //m_rdr_opt_parms.TagWriteEPC.retryCount,
                    //m_rdr_opt_parms.TagWriteEPC.writeRetryCount,
                    CSLibrary.Constants.SelectFlags.SELECT);
            }
            catch (System.Exception ex)
            {
#if DEBUG
//                CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.TagWriteEPCThreadProc()", ex);
#endif
            }
            finally
            {
/*                FireAccessCompletedEvent(
                    new OnAccessCompletedEventArgs(
                    m_Result == Result.OK,
                    Bank.EPC,
                    TagAccess.WRITE,
                    m_rdr_opt_parms.TagWriteEPC.epc));

                FireStateChangedEvent(RFState.IDLE);
*/            }
        }

        private void TagWriteAccPwdThreadProc_CS108()
        {
            UInt16[] writeData = new UInt16[2];

            try
            {
                FireStateChangedEvent(CSLibrary.Constants.RFState.BUSY);

                //                m_Result = TagWriteAccPwd(m_rdr_opt_parms.TagWriteAccPwd);
                writeData[0] = (ushort)(m_rdr_opt_parms.TagWriteAccPwd.password >> 16);
                writeData[1] = (ushort)m_rdr_opt_parms.TagWriteAccPwd.password;

                m_Result = CUST_18K6CTagWrite(
                    CSLibrary.Constants.MemoryBank.RESERVED,
                    ACC_PWD_START_OFFSET,
                    TWO_WORD_LEN,
                    writeData,
                    m_rdr_opt_parms.TagWriteAccPwd.accessPassword,
                    //m_rdr_opt_parms.TagWriteAccPwd.retryCount,
                    //m_rdr_opt_parms.TagWriteAccPwd.writeRetryCount,
                    CSLibrary.Constants.SelectFlags.SELECT);
            }
            catch (System.Exception ex)
            {
#if DEBUG
//                CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.TagWriteAccPwdThreadProc()", ex);
#endif
            }
            finally
            {
                /*
                FireAccessCompletedEvent(
                    new OnAccessCompletedEventArgs(
                    m_Result == Result.OK,
                    Bank.ACC_PWD,
                    TagAccess.WRITE,
                    new S_PWD(m_rdr_opt_parms.TagWriteAccPwd.password)));

                FireStateChangedEvent(RFState.IDLE);*/
            }
        }

        private void TagWriteKillPwdThreadProc_CS108()
        {
            UInt16[] writeData = new UInt16[2];

            try
            {
                FireStateChangedEvent(CSLibrary.Constants.RFState.BUSY);

                //m_Result = TagWriteKillPwd(m_rdr_opt_parms.TagWriteKillPwd);

                writeData[0] = (UInt16)(m_rdr_opt_parms.TagWriteKillPwd.password >> 16);
                writeData[1] = (UInt16)(m_rdr_opt_parms.TagWriteKillPwd.password);

                m_Result = CUST_18K6CTagWrite(
                    CSLibrary.Constants.MemoryBank.RESERVED,
                    KILL_PWD_START_OFFSET,
                    TWO_WORD_LEN,
                    writeData,
                    m_rdr_opt_parms.TagWriteKillPwd.accessPassword,
                    //m_rdr_opt_parms.TagWriteKillPwd.retryCount,
                    //m_rdr_opt_parms.TagWriteKillPwd.writeRetryCount,
                    CSLibrary.Constants.SelectFlags.SELECT);
            }
            catch (System.Exception ex)
            {
#if DEBUG
//                CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.TagWriteKillPwdThreadProc()", ex);
#endif
            }
            finally
            {
/*                FireAccessCompletedEvent(
                    new OnAccessCompletedEventArgs(
                    m_Result == Result.OK,
                    Bank.KILL_PWD,
                    TagAccess.WRITE,
                    new S_PWD(m_rdr_opt_parms.TagWriteKillPwd.password)));

                FireStateChangedEvent(RFState.IDLE);
*/            }
        }

        private void TagWriteUsrMemThreadProc_CS108()
        {
            try
            {
                FireStateChangedEvent(CSLibrary.Constants.RFState.BUSY);

                UInt16[] readData = new UInt16[m_rdr_opt_parms.TagWriteUser.count];
                UInt16[] writeData = m_rdr_opt_parms.TagWriteUser.pData;
                UInt16[] readCmp = new UInt16[MAX_WR_CNT];
                bool status;

                m_Result = CSLibrary.Constants.Result.OK;

                m_Result = CUST_18K6CTagWrite(
                    CSLibrary.Constants.MemoryBank.USER,
                    (uint)(m_rdr_opt_parms.TagWriteUser.offset),
                    m_rdr_opt_parms.TagWriteUser.count,
                    writeData,
                    m_rdr_opt_parms.TagWriteUser.accessPassword,
                    //m_rdr_opt_parms.TagWriteUser.retryCount,
                    //m_rdr_opt_parms.TagWriteUser.writeRetryCount,
                    CSLibrary.Constants.SelectFlags.SELECT);
            }
            catch (System.Exception ex)
            {
#if DEBUG
//                CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.TagWriteUsrMemThreadProc()", ex);
#endif
            }
            finally
            {
/*                FireAccessCompletedEvent(
                    new OnAccessCompletedEventArgs(
                    m_Result == Result.OK,
                    Bank.USER,
                    TagAccess.WRITE,
                    new S_DATA(m_rdr_opt_parms.TagWriteUser.pData)));

                FireStateChangedEvent(RFState.IDLE);
*/            }
        }

        private void BlockWriteThreadProc_CS108()
        {
            if (m_rdr_opt_parms.TagBlockWrite.retryCount > 31)
            {
                m_Result = Constants.Result.INVALID_PARAMETER;
                return;
            }

            if (m_rdr_opt_parms.TagBlockWrite.count > 255)
            {
                m_Result = Constants.Result.INVALID_PARAMETER;
                return;
            }

            try
            {
                FireStateChangedEvent(CSLibrary.Constants.RFState.BUSY);

                m_Result = CSLibrary.Constants.Result.FAILURE;

                MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, m_rdr_opt_parms.TagBlockWrite.accessPassword);
                Start18K6CRequest(1, m_rdr_opt_parms.TagBlockWrite.flags);
                MacWriteRegister(MACREGISTER.HST_TAGACC_DESC_CFG, 0x01 | (m_rdr_opt_parms.TagBlockWrite.retryCount << 1)); // Enable write verify and set retry count

                // Set up the tag bank register (tells where to write the data)
                MacWriteRegister(MACREGISTER.HST_TAGACC_BANK, (uint)m_rdr_opt_parms.TagBlockWrite.bank);

                // Set the offset
                MacWriteRegister(MACREGISTER.HST_TAGACC_PTR, m_rdr_opt_parms.TagBlockWrite.offset);

                // Set up the access count register (i.e., number of words to write)
                MacWriteRegister(MACREGISTER.HST_TAGACC_CNT, m_rdr_opt_parms.TagBlockWrite.count);

                ushort DataSize = m_rdr_opt_parms.TagBlockWrite.count;
                ushort WriteSize = DataSize;
                // Write the values to the bank until either the bank is full or we run out of data
                ushort registerAddress = (UInt16)MACREGISTER.HST_TAGWRDAT_0;
                UInt32 registerBank = 0;
                int pcnt = 0;

                // Set up the HST_TAGWRDAT_N registers.  Fill up a bank at a time.
                for (registerBank = 0; DataSize > 0; registerBank++)
                {
                    // Indicate which bank of tag write registers we are going to fill
                    MacWriteRegister(MACREGISTER.HST_TAGWRDAT_SEL, registerBank);

                    // Write the values to the bank until either the bank is full or we run out of data
                    registerAddress = (UInt16)MACREGISTER.HST_TAGWRDAT_0;

                    Debug.WriteLine("1. datasize:" + DataSize + " writesieze:" + WriteSize);
                    if (DataSize >= 32)
                    {
                        WriteSize = 32;
                        DataSize -= 32;
                    }
                    else
                    {
                        WriteSize = DataSize;
                        DataSize = 0;
                    }
                    Debug.WriteLine("2. datasize:" + DataSize + " writesieze:" + WriteSize);

                    while (WriteSize > 1)
                    {
                        // Set up the register and then write it to the MAC
                        UInt32 registerValue = (UInt32)((UInt32)m_rdr_opt_parms.TagBlockWrite.data[pcnt + 1] | (((UInt32)m_rdr_opt_parms.TagBlockWrite.data[pcnt]) << 16));

                        MacWriteRegister((MACREGISTER)(registerAddress), registerValue);

                        pcnt += 2;
                        registerAddress++;
                        WriteSize -= 2;
                    }

                    if (WriteSize == 1)
                    {
                        // Set up the register and then write it to the MAC
                        UInt32 registerValue = (uint)((UInt32)m_rdr_opt_parms.TagBlockWrite.data[pcnt] << 16);

                        MacWriteRegister((MACREGISTER)(registerAddress), registerValue);
                    }
                }

                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.BLOCKWRITE), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (UInt32)CurrentOperation);

                m_Result = CSLibrary.Constants.Result.OK;
            }
            catch (System.Exception ex)
            {
            }
            finally
            {
            }
        }
    }
}
