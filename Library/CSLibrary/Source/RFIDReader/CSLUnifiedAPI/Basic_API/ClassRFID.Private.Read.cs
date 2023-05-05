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

using CSLibrary.Constants;
using CSLibrary.Structures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSLibrary.RFIDDEVICE;

namespace CSLibrary
{
    using static RFIDDEVICE;

    public partial class RFIDReader
    {
        void Setup18K6CReadRegisters(UInt32 bank, UInt32 offset, UInt32 count)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    Setup18K6CReadRegisters_CS108(bank, offset, count);
                    break;

                case MODEL.CS710S:
                    Setup18K6CReadRegisters_CS710S(bank, offset, count);
                    break;
            }
        }

        public int Start18K6CRead(uint bank, uint offset, uint count, UInt16[] data, uint accessPassword, uint retry, CSLibrary.Constants.SelectFlags flags)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return Start18K6CRead_CS108(bank, offset, count, data, accessPassword, retry, flags);

                case MODEL.CS710S:
                    return Start18K6CRead_CS710S(bank, offset, count, data, accessPassword, retry, flags);
            }

            return 0;
        } //  Start18K6CRead

        bool CUST_18K6CTagRead(CSLibrary.Constants.MemoryBank bank, int offset, int count, UInt16[] data, UInt32 password, /*UInt32 retry, */CSLibrary.Constants.SelectFlags flags)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return CUST_18K6CTagRead_CS108(bank, offset, count, data, password, flags);

                case MODEL.CS710S:
                    return CUST_18K6CTagRead_CS710S(bank, offset, count, data, password, flags);
            }

            return false;
        }

        private void ReadThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    ReadThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    ReadThreadProc_CS710S();
                    break;
            }
        }

        private void TagReadPCThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagReadPCThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagReadPCThreadProc_CS710S();
                    break;
            }
        }

        private void TagReadEPCThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagReadEPCThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagReadEPCThreadProc_CS710S();
                    break;
            }
        }

        private void TagReadAccPwdThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagReadAccPwdThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagReadAccPwdThreadProc_CS710S();
                    break;
            }
        }

        private void TagReadKillPwdThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagReadKillPwdThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagReadKillPwdThreadProc_CS710S();
                    break;
            }
        }

        private void TagReadTidThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagReadTidThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagReadTidThreadProc_CS710S();
                    break;
            }
        }

        private void TagReadUsrMemThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    TagReadUsrMemThreadProc_CS108();
                    break;

                case MODEL.CS710S:
                    TagReadUsrMemThreadProc_CS710S();
                    break;
            }
        }
    }
}
