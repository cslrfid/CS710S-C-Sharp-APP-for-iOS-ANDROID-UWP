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

using System.Reflection;


namespace CSLibrary
{
    using static RFIDDEVICE;
    using Constants;
    using Structures;
    using Events;
    using Tools;

    public partial class RFIDReader
    {
        private enum RFIDREADERCMDSTATUS
        {
            IDLE,           // Can send (SetRegister, GetRegister, ExecCmd, Abort), Can not receive data
            GETREGISTER,    // Can not send data, Can receive (GetRegister) 
            EXECCMD,        // Can send (Abort), Can receive (CMDBegin, CMDEnd, Inventory data, Abort)
            INVENTORY,      // Can send (Abort)
            ABORT,          // Can not send
        }

        // RFID event code
        internal class DOWNLINKCMD
        {
            public static readonly byte[] RFIDPOWERON = { 0x80, 0x00 };
            public static readonly byte[] RFIDPOWEROFF = { 0x80, 0x01 };
            public static readonly byte[] RFIDCMD = { 0x80, 0x02 };
        }

        private const int BYTES_PER_LEN_UNIT = 4;

        private const uint INVALID_POWER_VALUE = uint.MaxValue;
        private const uint INVALID_PROFILE_VALUE = uint.MaxValue;
        private const int DATA_FIELD_INDEX = 20;
        private const int RSSI_FIELD_INDEX = 12;
        private const int ANT_FIELD_INDEX = 14;
        private const int MS_FIELD_INDEX = 8;
        private const int RFID_PACKET_COMMON_SIZE = 8;

        private const ushort PC_START_OFFSET = 1;
        private const ushort PC_WORD_LENGTH = 1;
        private const ushort EPC_START_OFFSET = 2;
        private const ushort EPC_WORD_LENGTH = 6;
        private const ushort ACC_PWD_START_OFFSET = 2;
        private const ushort ACC_PWD_WORD_LENGTH = 2;
        private const ushort KILL_PWD_START_OFFSET = 0;
        private const ushort KILL_PWD_WORD_LENGTH = 2;
        private const ushort ONE_WORD_LEN = 1;
        private const ushort TWO_WORD_LEN = 2;

        private const ushort USER_WORD_LENGTH = 1;
        private const uint MAXFRECHANNEL = 50;

        private Result CurrentOperationResult;

        internal CSLibraryOperationParms m_rdr_opt_parms = new CSLibraryOperationParms();

        #region ====================== Fire Event ======================
        private void FireStateChangedEvent(RFState e)
        {
            TellThemOnStateChanged(this, new OnStateChangedEventArgs(e));
        }

        private void FireAccessCompletedEvent(OnAccessCompletedEventArgs args/*bool success, Bank bnk, TagAccess access, IBANK data*/)
        {
            TellThemOnAccessCompleted(this, args);
        }

        private void TellThemOnStateChanged(object sender, OnStateChangedEventArgs e)
        {
            if (OnStateChanged != null)
            {
                try
                {
                    OnStateChanged(sender, e);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                }
            }
        }

        private void TellThemOnAccessCompleted(object sender, OnAccessCompletedEventArgs e)
        {
            if (OnAccessCompleted != null)
            {
                try
                {
                    OnAccessCompleted(sender, e);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                }
            }
        }

        #endregion

        void Start18K6CRequest(uint tagStopCount, SelectFlags flags)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    Start18K6CRequest_CS108(tagStopCount, flags);
                    break;

                case MODEL.CS710S:
                    Start18K6CRequest_CS710S(tagStopCount, flags);
                    break;
            }
        }

        private void TagLockThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagLockThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagLockThreadProc_CS710S();
                    break;
            }
        }

        private void TagBlockLockThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagBlockLockThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagBlockLockThreadProc_CS710S();
                    break;
            }
        }

        private void TagKillThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagKillThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagKillThreadProc_CS710S();
                    break;
            }

            m_Result = Result.OK;
            return;
        }


        private void TagAuthenticateThreadProc()
        {
        }

        private void TagReadBufferThreadProc()
        {
        }

        private void TagUntraceableThreadProc()
        {
            return;
        }


        private bool FM13DTReadMemoryThreadProc()
        {
            return false;
        }

        private bool FM13DTWriteMemoryThreadProc()
        {
            return false;
        }

        private bool FM13DTReadRegThreadProc()
        {
            return true;
        }

        private bool FM13DTWriteRegThreadProc()
        {
            return true;
        }

        private bool FM13DTAuthThreadProc()
        {
//            FM13DT160_Auth(m_rdr_opt_parms.FM FM13DTWriteMemory.offset, m_rdr_opt_parms.FM13DTWriteMemory.count);
            return true;
        }

        private bool FM13DTGetTempThreadProc()
        {
            //FM13DT160_GetTemp(m_rdr_opt_parms.FM13DTWriteMemory.offset, m_rdr_opt_parms.FM13DTWriteMemory.count, m_rdr_opt_parms.FM13DTWriteMemory.data);
            return true;
        }
        private bool FM13DTStartLogThreadProc()
        {
            return true;
        }
        private bool FM13DTStopLogChkThreadProc()
        {
            return true;
        }
        private bool FM13DTDeepSleepThreadProc()
        {
            return true;
        }
        private bool FM13DTOpModeChkThreadProc()
        {
            return true;
        }
        private bool FM13DTInitialRegFileThreadProc()
        {
            return true;
        }

        private bool FM13DTLedCtrlThreadProc()
        {
            return true;
        }

        /// <summary>
        /// rfid reader packet
        /// </summary>
        /// <param name="RW"></param>
        /// <param name="add"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal byte[] PacketData(UInt16 add, UInt32? value = null)
        {
            byte[] CMDBuf = new byte[8];

            if (value == null)
            {
                CMDBuf[1] = 0x00;
                CMDBuf[4] = 0x00;
                CMDBuf[5] = 0x00;
                CMDBuf[6] = 0x00;
                CMDBuf[7] = 0x00;
            }
            else
            {
                CMDBuf[1] = 0x01;
                CMDBuf[4] = (byte)value;
                CMDBuf[5] = (byte)(value >> 8);
                CMDBuf[6] = (byte)(value >> 16);
                CMDBuf[7] = (byte)(value >> 24);
            }

            CMDBuf[0] = 0x70;
            CMDBuf[2] = (byte)add;
            CMDBuf[3] = (byte)((uint)add >> 8);

            return CMDBuf;
        }
    }
}
