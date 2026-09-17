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
    public partial class RFIDReader
    {
        internal void StopOperation_CS108()
        {
            //HighLevelInterface._debugBLEHold = false;
            byte[] cmd = { 0x40, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, cmd, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA2);
        }

        internal Result StartOperation_CS108(Operation opertion)
        {
            CurrentOperation = opertion;

            // Clear inventory compatmode
            if (opertion != Operation.TAG_EXERANGING)
            {
                UInt32 Value = 0;

                // Clear inventory compatmode
                MacReadRegister(MACREGISTER.HST_INV_CFG, ref Value);
                Value &= ~(1U << 26); // bit 26
                Value &= ~(3U << 16); // bit 16,17
                MacWriteRegister(MACREGISTER.INV_CYCLE_DELAY, 0);
                MacWriteRegister(MACREGISTER.HST_INV_CFG, Value);
            }

            InventoryDebug.Clear();
            switch (opertion)
            {
                case Operation.Kiloway_RANGING:
                case Operation.TAG_RANGING: // Enable get battery level by interrupt
                    _deviceHandler.battery.EnableAutoBatteryLevel();
                    TagRangingThreadProc_CS108();
                    break;

                case Operation.TAG_PRERANGING: // Enable get battery level by interrupt
                    PreTagRangingThreadProc_CS108();
                    break;

                case Operation.TAG_EXERANGING: // Enable get battery level by interrupt
                    CurrentOperation = Operation.TAG_RANGING;
                    _deviceHandler.battery.EnableAutoBatteryLevel();
                    PreTagRangingThreadProc_CS108(); // fix multibank can not work when selectd tag
                    ExeTagRangingThreadProc_CS108();
                    break;

                case Operation.TAG_SEARCHING: // Enable get battery level by interrupt
                    _deviceHandler.battery.EnableAutoBatteryLevel();
                    TagSearchOneTagThreadProc_CS108();
                    break;

                case Operation.TAG_PRESEARCHING:
                    PreTagSearchOneTagThreadProc_CS108();
                    break;

                case Operation.TAG_EXESEARCHING: // Enable get battery level by interrupt
                    CurrentOperation = Operation.TAG_SEARCHING;
                    _deviceHandler.battery.EnableAutoBatteryLevel();
                    ExeTagSearchOneTagThreadProc_CS108();
                    break;

                case Operation.TAG_SELECTED:
                    TagSelected_CS108();
                    break;

                case Operation.TAG_SELECTEDDYNQ:
                    TagSelectedDYNQ_CS108();
                    break;

                case Operation.TAG_FASTSELECTED:
                    FastTagSelected_CS108();
                    break;

                case Operation.TAG_GENERALSELECTED:
                    SetMaskThreadProc_CS108();
                    break;

                case Operation.TAG_PREFILTER:
                    PreFilter_CS108();
                    break;

                case Operation.TAG_READ:
                    ReadThreadProc_CS108();
                    break;

                case Operation.TAG_READ_PC:
                    TagReadPCThreadProc_CS108();
                    break;

                case Operation.TAG_READ_EPC:
                    TagReadEPCThreadProc_CS108();
                    break;

                case Operation.TAG_READ_ACC_PWD:
                    TagReadAccPwdThreadProc_CS108();
                    break;

                case Operation.TAG_READ_KILL_PWD:
                    TagReadKillPwdThreadProc_CS108();
                    break;

                case Operation.TAG_READ_TID:
                    TagReadTidThreadProc_CS108();
                    break;

                case Operation.TAG_READ_USER:
                    TagReadUsrMemThreadProc_CS108();
                    break;

                case Operation.TAG_WRITE:
                    WriteThreadProc_CS108();
                    break;

                case Operation.TAG_WRITE_PC:
                    TagWritePCThreadProc_CS108();
                    break;

                case Operation.TAG_WRITE_EPC:
                    TagWriteEPCThreadProc_CS108();
                    break;

                case Operation.TAG_WRITE_ACC_PWD:
                    TagWriteAccPwdThreadProc_CS108();
                    break;

                case Operation.TAG_WRITE_KILL_PWD:
                    TagWriteKillPwdThreadProc_CS108();
                    break;

                case Operation.TAG_WRITE_USER:
                    TagWriteUsrMemThreadProc_CS108();
                    break;

                case Operation.TAG_BLOCK_WRITE:
                    BlockWriteThreadProc_CS108();
                    break;

                case Operation.TAG_LOCK:
                    TagLockThreadProc_CS108();
                    break;

                case Operation.TAG_BLOCK_PERMALOCK:
                    TagBlockLockThreadProc_CS108();
                    break;

                case Operation.TAG_KILL:
                    TagKillThreadProc_CS108();
                    break;

                case Operation.TAG_AUTHENTICATE:
                    TagAuthenticateThreadProc_CS108();
                    break;

                case Operation.TAG_READBUFFER:
                    TagReadBufferThreadProc_CS108();
                    break;

                case Operation.TAG_UNTRACEABLE:
                    TagUntraceableThreadProc_CS108();
                    break;

                case Operation.FM13DT_READMEMORY:
                    FM13DTReadMemoryThreadProc_CS108();
                    break;

                case Operation.FM13DT_WRITEMEMORY:
                    FM13DTWriteMemoryThreadProc_CS108();
                    break;

                case Operation.FM13DT_READREGISTER:
                    FM13DTReadRegThreadProc_CS108();
                    break;

                case Operation.FM13DT_WRITEREGISTER:
                    FM13DTWriteRegThreadProc_CS108();
                    break;

               case Operation.FM13DT_AUTH:
                    FM13DTAuthThreadProc_CS108();
                    break;

                case Operation.FM13DT_GETTEMP:
                    FM13DTGetTempThreadProc_CS108();
                    break;

                case Operation.FM13DT_STARTLOG:
                    FM13DTStartLogThreadProc_CS108();
                    break;

                case Operation.FM13DT_STOPLOG:
                    FM13DTStopLogChkThreadProc_CS108();
                    break;

                case Operation.FM13DT_DEEPSLEEP:
                    FM13DTDeepSleepThreadProc_CS108();
                    break;

                case Operation.FM13DT_OPMODECHK:
                    FM13DTOpModeChkThreadProc_CS108();
                    break;

                case Operation.FM13DT_INITIALREGFILE:
                    FM13DTInitialRegFileThreadProc_CS108();
                    break;

                case Operation.FM13DT_LEDCTRL:
                    FM13DTLedCtrlThreadProc_CS108();
                    break;

                case Operation.QT_COMMAND:
                    //QT_CommandProc_CS108();
                    break;

                default:
                    return Result.NOT_SUPPORTED;
            }

            return Result.OK;
        }
    }
}
