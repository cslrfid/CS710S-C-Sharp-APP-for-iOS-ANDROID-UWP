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
using CSLibrary.Structures;

namespace CSLibrary
{
    public partial class RFIDReader
    {
        TagRangingParms _currentTagRanging;
        internal Result StartOperation_CS710S(Operation opertion)
        {
            CurrentOperation = opertion;

            InventoryDebug.Clear();
            switch (opertion)
            {
                case Operation.Kiloway_RANGING: // Spical Kiloway LED Inventory
                    _deviceHandler.battery.EnableAutoBatteryLevel();

                    _currentTagRanging = Options.TagRanging.Clone();

                    RFIDRegister.AntennaPortConfig.FastIdEnable(Options.TagRanging.fastid);
                    RFIDRegister.AntennaPortConfig.TagFocusEnable(Options.TagRanging.focus);
                    if (m_rdr_opt_parms.TagRanging.multibanks == 0)
                    {
                        if ((Options.TagRanging.flags & SelectFlags.FILTER) == 0X00 && (Options.TagRanging.flags & SelectFlags.SELECT) == 0x00)
                        {
                            //RFIDRegister.AntennaPortConfig.TagGroup(0, 3, 0);
                            RFIDStartCompactInventory();
                        }
                        else
                        {
                            RFIDRegister.AntennaPortConfig.TagGroup(0, 3, 0, 0xffff);
                            RFIDStartSelectCompactInventory();
                        }
                    }
                    else
                    {
                        if ((Options.TagRanging.flags & SelectFlags.FILTER) == 0X00 && (Options.TagRanging.flags & SelectFlags.SELECT) == 0x00)
                        {
                            //RFIDRegister.AntennaPortConfig.TagGroup(0, 3, 0);
                            RFIDStartKilowayMBInventory();
                        }
                        else
                        {
                            RFIDRegister.AntennaPortConfig.TagGroup(0, 3, 0, 0xffff);
                            RFIDStartKilowaySelectMBInventory();
                        }
                    }
                    break;

                case Operation.TAG_EXESEARCHING: // phase out for backward compatible
                case Operation.TAG_EXERANGING: // phase out for backward compatible
                case Operation.TAG_RANGING: // Enable get battery level by interrupt

                    //WriteRegister(0x3036, new byte[] { 0x66 });

                    _deviceHandler.battery.EnableAutoBatteryLevel();

                    _currentTagRanging = Options.TagRanging.Clone();

                    RFIDRegister.AntennaPortConfig.FastIdEnable(Options.TagRanging.fastid);
                    RFIDRegister.AntennaPortConfig.TagFocusEnable(Options.TagRanging.focus);
                    if (m_rdr_opt_parms.TagRanging.multibanks == 0)
                    {
                        if ((Options.TagRanging.flags & SelectFlags.FILTER) == 0X00 && (Options.TagRanging.flags & SelectFlags.SELECT) == 0x00)
                        {
                            //RFIDRegister.AntennaPortConfig.TagGroup(0, 3, 0);
                            RFIDStartCompactInventory();
                        }
                        else
                        {
                            RFIDRegister.AntennaPortConfig.TagGroup(0, 3, 0, 0xffff);
                            RFIDStartSelectCompactInventory();
                        }
                    }
                    else
                    {
                        if ((Options.TagRanging.flags & SelectFlags.FILTER) == 0X00 && (Options.TagRanging.flags & SelectFlags.SELECT) == 0x00)
                        {
                            //RFIDRegister.AntennaPortConfig.TagGroup(0, 3, 0);
                            RFIDStartMBInventory();
                        }
                        else
                        {
                            RFIDRegister.AntennaPortConfig.TagGroup(0, 3, 0, 0xffff);
                            RFIDStartSelectMBInventory();
                        }
                    }
                    break;

                case Operation.TAG_SELECTED:
                    // Set Q = 1
                    SetFixedQParms_CS710S(1, 1);
                    // Set 
                    TagSelected_CS710S();
                    break;

                case Operation.TAG_PREFILTER:
                    TagSelected_CS710S();
                    break;

                case Operation.TAG_READ:
                    ReadThreadProc_CS710S();
                    break;

                case Operation.TAG_READ_PC:
                    TagReadPCThreadProc_CS710S();
                    break;

                case Operation.TAG_READ_EPC:
                    TagReadEPCThreadProc_CS710S();
                    break;

                case Operation.TAG_READ_ACC_PWD:
                    TagReadAccPwdThreadProc_CS710S();
                    break;

                case Operation.TAG_READ_KILL_PWD:
                    TagReadKillPwdThreadProc_CS710S();
                    break;

                case Operation.TAG_READ_TID:
                    TagReadTidThreadProc_CS710S();
                    break;

                case Operation.TAG_READ_USER:
                    TagReadUsrMemThreadProc_CS710S();
                    break;

                case Operation.TAG_WRITE:
                    WriteThreadProc_CS710S();
                    break;

                case Operation.TAG_WRITE_PC:
                    TagWritePCThreadProc_CS710S();
                    break;

                case Operation.TAG_WRITE_EPC:
                    TagWriteEPCThreadProc_CS710S();
                    break;

                case Operation.TAG_WRITE_ACC_PWD:
                    TagWriteAccPwdThreadProc_CS710S();
                    break;

                case Operation.TAG_WRITE_KILL_PWD:
                    TagWriteKillPwdThreadProc_CS710S();
                    break;

                case Operation.TAG_WRITE_USER:
                    TagWriteUsrMemThreadProc_CS710S();
                    break;

                case Operation.TAG_LOCK:
                    TagLockThreadProc_CS710S();
                    break;

                case Operation.TAG_KILL:
                    TagKillThreadProc_CS710S();
                    break;

                case Operation.TAG_AUTHENTICATE:
                    TagAuthenticateThreadProc_CS710S();
                    break;


                //                case Operation.TAG_PREFILTER:
                //                    PreFilter();
                //                    break;







                /*
                                case Operation.TAG_SEARCHING: // Enable get battery level by interrupt
                                    _deviceHandler.battery.EnableAutoBatteryLevel();
                                    TagSearchOneTagThreadProc();
                                    break;

                                case Operation.TAG_PRESEARCHING:
                                    PreTagSearchOneTagThreadProc();
                                    break;

                                case Operation.TAG_EXESEARCHING: // Enable get battery level by interrupt
                                    CurrentOperation = Operation.TAG_SEARCHING;
                                    _deviceHandler.battery.EnableAutoBatteryLevel();
                                    ExeTagSearchOneTagThreadProc();
                                    break;

                case Operation.TAG_SELECTEDDYNQ:
                    TagSelectedDYNQ();
                    break;

                case Operation.TAG_FASTSELECTED:
                    FastTagSelected();
                    break;

                case Operation.TAG_GENERALSELECTED:
                    SetMaskThreadProc();
                    break;



                case Operation.TAG_BLOCK_WRITE:
                    BlockWriteThreadProc();
                    break;

                case Operation.TAG_BLOCK_PERMALOCK:
                    TagBlockLockThreadProc();
                    break;


                case Operation.TAG_AUTHENTICATE:
                    TagAuthenticateThreadProc();
                    break;

                case Operation.TAG_READBUFFER:
                    TagReadBufferThreadProc();
                    break;

                case Operation.TAG_UNTRACEABLE:
                    TagUntraceableThreadProc();
                    break;

                case Operation.FM13DT_READMEMORY:
                    FM13DTReadMemoryThreadProc();
                    break;

                case Operation.FM13DT_WRITEMEMORY:
                    FM13DTWriteMemoryThreadProc();
                    break;

                case Operation.FM13DT_READREGISTER:
                    FM13DTReadRegThreadProc();
                    break;

                case Operation.FM13DT_WRITEREGISTER:
                    FM13DTWriteRegThreadProc();
                    break;

               case Operation.FM13DT_AUTH:
                    FM13DTAuthThreadProc();
                    break;

                case Operation.FM13DT_GETTEMP:
                    FM13DTGetTempThreadProc();
                    break;

                case Operation.FM13DT_STARTLOG:
                    FM13DTStartLogThreadProc();
                    break;

                case Operation.FM13DT_STOPLOG:
                    FM13DTStopLogChkThreadProc();
                    break;

                case Operation.FM13DT_DEEPSLEEP:
                    FM13DTDeepSleepThreadProc();
                    break;

                case Operation.FM13DT_OPMODECHK:
                    FM13DTOpModeChkThreadProc();
                    break;

                case Operation.FM13DT_INITIALREGFILE:
                    FM13DTInitialRegFileThreadProc();
                    break;

                case Operation.FM13DT_LEDCTRL:
                    FM13DTLedCtrlThreadProc();
                    break;

                case Operation.QT_COMMAND:
                    QT_CommandProc();
                    break;
                */

                default:
                    return Result.NOT_SUPPORTED;
            }

            return Result.OK;
        }
    }
}
