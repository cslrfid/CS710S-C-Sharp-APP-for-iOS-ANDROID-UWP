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

    public partial class RFIDReader
    {
        const int MAX_WR_CNT = 0x20;

        void Setup18K6CWriteRegisters(CSLibrary.Constants.MemoryBank WriteBank, UInt32 WriteOffset, UInt32 WriteSize, UInt16[] WriteBuf, UInt32 BufOffset)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    Setup18K6CWriteRegisters_CS108(WriteBank, WriteOffset, WriteSize, WriteBuf, BufOffset);
                    break;

                case MODEL.CS710S:
                    Setup18K6CWriteRegisters_CS710S(WriteBank, WriteOffset, WriteSize, WriteBuf, BufOffset);
                    break;
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
        private CSLibrary.Constants.Result CUST_18K6CTagWrite(
            CSLibrary.Constants.MemoryBank bank,
            UInt32 offset,
            UInt32 count,
            UInt16[] data,
            UInt32 password,
            CSLibrary.Constants.SelectFlags flags
        )
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return CUST_18K6CTagWrite_CS108(bank, offset, count, data, password, flags);
                    break;

                case MODEL.CS710S:
                    return CUST_18K6CTagWrite_CS710S(bank, offset, count, data, password, flags);
                    break;
            }

            return Result.FAILURE;
        }

        private void WriteThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    WriteThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    WriteThreadProc_CS710S();
                    break;
            }
        }

        private void TagWritePCThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagWritePCThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagWritePCThreadProc_CS710S();
                    break;
            }
        }

        private void TagWriteEPCThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagWriteEPCThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagWriteEPCThreadProc_CS710S();
                    break;
            }
        }

        private void TagWriteAccPwdThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagWriteAccPwdThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagWriteAccPwdThreadProc_CS710S();
                    break;
            }
        }

        private void TagWriteKillPwdThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagWriteKillPwdThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagWriteKillPwdThreadProc_CS710S();
                    break;
            }
        }

        private void TagWriteUsrMemThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagWriteUsrMemThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagWriteUsrMemThreadProc_CS710S();
                    break;
            }
        }

        private void BlockWriteThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    BlockWriteThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    BlockWriteThreadProc_CS710S();
                    break;
            }
        }
    }
}
