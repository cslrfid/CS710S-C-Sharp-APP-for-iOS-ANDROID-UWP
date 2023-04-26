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
    public partial class HighLevelInterface
    {
        readonly byte[] destinationsID = { 0xc2, 0x6a, 0xd9, 0xe8, 0x5f };

        internal enum BTCOMMANDTYPE
        {
            Normal,         // Normanl command : send 5 times, and clear all command if send fail
            Validate,       // Validate command : for test hardhware, send 2 times
            None            // end of enum
        }

        internal enum DEVICEID : byte
        {
            RFID = 0xc2,
            Barcode = 0x6a,
            Notification = 0xd9,
            SiliconLabIC = 0xe8,
            NluetoothIC = 0x5f
        }

        [Flags]
        internal enum BTWAITCOMMANDRESPONSETYPE
        {
            NOWAIT = 0,
            BTAPIRESPONSE = 1,
            COMMANDENDRESPONSE = 2,
            DATA1 = 4,
            DATA2 = 8,
            REGISTERRETURN = 16,
            ABORTRESPONSE = 32,
            TAGACCESSPACKET = 64,
            ENDEVENTUPLINKPACKET = 128,
            CSL_OPERATION_COMPLETE = 256,
            CSL_ACCESS_COMPLETE = 512,

            WAIT_BTAPIRESPONSE = BTAPIRESPONSE,
            WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE = BTAPIRESPONSE | COMMANDENDRESPONSE,
            WAIT_BTAPIRESPONSE_DATA1 = BTAPIRESPONSE | DATA1,
            WAIT_BTAPIRESPONSE_DATA2 = BTAPIRESPONSE | DATA2,
            WAIT_BTAPIRESPONSE_DATA1_COMMANDENDRESPONSE = BTAPIRESPONSE | DATA1 | COMMANDENDRESPONSE,
            WAIT_BTAPIRESPONSE_DATA2_COMMANDENDRESPONSE = BTAPIRESPONSE | DATA2 | COMMANDENDRESPONSE,
        }

        internal class SENDBUFFER
        {
            public BTCOMMANDTYPE type = BTCOMMANDTYPE.None;     // command type
            public byte[] packetData;                           // send data packet
            public UInt32 cmdRemark;                            // reserver
            public BTWAITCOMMANDRESPONSETYPE dataRemark;        // BT command return data check
            public Action sendFailCallback = null;              // Send fail callback
        }

        private List<SENDBUFFER> _sendBuffer = new List<SENDBUFFER>();
        //private bool _PROTOCOL_HardwareDiagnosticsMode = true;
        private uint _PROTOCOL_RetryCount = 0;

        private object _bleEngineLock = new object();
        private DateTime _packetResponseTimeout;
        private DateTime _packetDelayTimeout;

        internal UInt32 _currentCmdRemark;
        private BTWAITCOMMANDRESPONSETYPE _NeedCommandResponseType;
        private BTWAITCOMMANDRESPONSETYPE _currentCommandResponse;

        #region ---- Internal function ----

        internal bool SendAsync(int connection, int destination, byte[] eventCode = null, byte[] payload = null, BTWAITCOMMANDRESPONSETYPE sendRemark = BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE, UInt32 cmdRemark = 0xffffffff)
        {
            if (eventCode == null)
                CSLibrary.Debug.WriteLine ("SendAsync : ") ;
            else
                CSLibrary.Debug.WriteLine("SendAsync : " + eventCode[0].ToString("X2") + eventCode[1].ToString("X2"));

            try
            {
                byte[] sendData;

                if (eventCode == null && payload == null)
                {
                    sendData = new byte[8];
                }
                else if (payload == null)
                {
                    if (eventCode.Length > (255 - 8))
                        return false;

                    sendData = new byte[8 + eventCode.Length];

                    Array.Copy(eventCode, 0, sendData, 8, eventCode.Length);

                    sendData[2] = (byte)eventCode.Length;
                }
                else
                {
                    if ((eventCode.Length + payload.Length) > (255 - 8))
                        return false;

                    sendData = new byte[8 + eventCode.Length + payload.Length];

                    Array.Copy(eventCode, 0, sendData, 8, eventCode.Length);
                    Array.Copy(payload, 0, sendData, 8 + eventCode.Length, payload.Length);

                    sendData[2] = (byte)(eventCode.Length + payload.Length);
                }

                sendData[0] = 0xa7;
                sendData[1] = (byte)((connection == 0) ? 0xb3 : 0xe6);
                sendData[3] = destinationsID[destination];
                sendData[4] = 0x82;
                sendData[5] = 0x37; // downlink
                sendData[6] = 0x00; // CRC Check
                sendData[7] = 0x00; // CRC Check

                SENDBUFFER sendItem = new SENDBUFFER();
                sendItem.packetData = sendData;
                sendItem.cmdRemark = cmdRemark;
                sendItem.dataRemark = sendRemark;

                _sendBuffer.Add(sendItem);
                BLERWEngineTimer();
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("SendAsync exception : " + ex.Message);
            }

            return true;
        }

        internal bool SendAsyncUrgent(int connection, int destination, byte[] eventCode = null, byte[] payload = null, BTWAITCOMMANDRESPONSETYPE sendRemark = BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE, UInt32 cmdRemark = 0xffffffff)
        {
            try
            {
                byte[] sendData;

                if (eventCode == null && payload == null)
                {
                    sendData = new byte[8];

                    sendData[6] = 0x00;
                    sendData[7] = 0x00;
                }
                else if (payload == null)
                {
                    if (eventCode.Length > (255 - 8))
                        return false;

                    sendData = new byte[8 + eventCode.Length];

                    Array.Copy(eventCode, 0, sendData, 8, eventCode.Length);

                    sendData[2] = (byte)eventCode.Length;
                }
                else
                {
                    if ((eventCode.Length + payload.Length) > (255 - 8))
                        return false;

                    sendData = new byte[8 + eventCode.Length + payload.Length];

                    Array.Copy(eventCode, 0, sendData, 8, eventCode.Length);
                    Array.Copy(payload, 0, sendData, 8 + eventCode.Length, payload.Length);

                    sendData[2] = (byte)(eventCode.Length + payload.Length);
                }

                sendData[0] = 0xa7;
                sendData[1] = (byte)((connection == 0) ? 0xb3 : 0xe6);
                sendData[3] = destinationsID[destination];
                sendData[4] = 0x82;
                sendData[5] = 0x37; // downlink
                sendData[6] = 0x00;
                sendData[7] = 0x00;

                SENDBUFFER sendItem = new SENDBUFFER();
                sendItem.packetData = sendData;
                sendItem.cmdRemark = cmdRemark;
                sendItem.dataRemark = sendRemark;

                _sendBuffer.Insert(1, sendItem);
                BLERWEngineTimer();
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("SendAsyncUrgent exception : " + ex.Message);
            }

            return true;
        }

        internal bool SendAsync(DEVICEID destination, 
                                    byte[] eventCode = null, 
                                    byte[] payload = null, 
                                    BTCOMMANDTYPE type = BTCOMMANDTYPE.Normal, 
                                    BTWAITCOMMANDRESPONSETYPE sendRemark = BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE, 
                                    UInt32 cmdRemark = 0xffffffff, 
                                    Action failCallback = null)
        {
            byte[] sendData;

            if (eventCode == null && payload == null)
            {
                sendData = new byte[8];

                sendData[6] = 0x00;
                sendData[7] = 0x00;
            }
            else 
            {
                if (eventCode.Length > (255 - 8))
                    return false;

                if (payload == null)
                {
                    sendData = new byte[8 + eventCode.Length];

                    Array.Copy(eventCode, 0, sendData, 8, eventCode.Length);

                    sendData[2] = (byte)eventCode.Length;
                }
                else
                {
                    if ((eventCode.Length + payload.Length) > (255 - 8))
                        return false;

                    sendData = new byte[8 + eventCode.Length + payload.Length];

                    Array.Copy(eventCode, 0, sendData, 8, eventCode.Length);
                    Array.Copy(payload, 0, sendData, 8 + eventCode.Length, payload.Length);

                    sendData[2] = (byte)(eventCode.Length + payload.Length);
                }
            }

            sendData[0] = 0xa7;
            sendData[1] = 0xb3; // (byte)((connection == 0) ? 0xb3 : 0xe6);
            sendData[3] = (byte)destination;
            sendData[4] = 0x82;
            sendData[5] = 0x37;
            sendData[6] = 0x00;
            sendData[7] = 0x00;

            SENDBUFFER sendItem = new SENDBUFFER();
            sendItem.type = type;
            sendItem.packetData = sendData;
            sendItem.cmdRemark = cmdRemark;
            sendItem.dataRemark = sendRemark;
            sendItem.sendFailCallback = failCallback;

            _sendBuffer.Add(sendItem);
            BLERWEngineTimer();

            return true;
        }

        static readonly byte[] RFIDCommand = new byte[] { 0xA7, 0xB3, 0x0A, 0xC2, 0x82, 0x37, 0x00, 0x00, 0x80, 0x02, 0x70, 0x01, 0x00, 0xF0 };
        bool CheckRFIDCommand()
        {
            if (_sendBuffer[0].packetData.Length != 18)
                return false;

            for (int index = 0; index < RFIDCommand.Length; index ++)
                if (_sendBuffer[0].packetData[index] != RFIDCommand[index])
                    return false;

            _handlerRFIDReader._readerMode = 1; // record reader static to command mode

            if (_sendBuffer[0].packetData[14] == 0x14 && _sendBuffer[0].packetData[15] == 0x00 && _sendBuffer[0].packetData[16] == 0x00 && _sendBuffer[0].packetData[17] == 0x00)
            {
                _handlerRFIDReader._SetRFIDToStandbyMode = false;
            }
            else
            {
                _handlerRFIDReader._SetRFIDToStandbyMode = true;
            }

            return true;
        }



        async void BLERWEngineTimer()
        {
            await Task.Delay(10);

            lock (_bleEngineLock)
            {
                try
                {
                    if (_readerState != READERSTATE.DISCONNECT)
                    {
                        if (_NeedCommandResponseType != BTWAITCOMMANDRESPONSETYPE.NOWAIT)
                        {
                            CSLibrary.Debug.WriteLine("wait response : " + _NeedCommandResponseType.ToString() + ":" + _currentCommandResponse);
                            if ((_currentCommandResponse & _NeedCommandResponseType) == _NeedCommandResponseType)
                            {
                                if (_sendBuffer.Count > 0)
                                    _sendBuffer.RemoveAt(0);

                                _NeedCommandResponseType = BTWAITCOMMANDRESPONSETYPE.NOWAIT;
                                _PROTOCOL_RetryCount = 0;
                            }
                            else if (DateTime.Now > _packetResponseTimeout)
                            {
                                switch (_sendBuffer[0].type)
                                {
                                    case BTCOMMANDTYPE.None:
                                    case BTCOMMANDTYPE.Normal:
                                        if (_PROTOCOL_RetryCount > 19) // retry 19 times (~40s)
                                        {
                                            // cancel all command and send error event

                                            CSLibrary.Debug.WriteLine("Communication retry fail!!");
                                            _NeedCommandResponseType = BTWAITCOMMANDRESPONSETYPE.NOWAIT;
                                            FireReaderStateChangedEvent(new Events.OnReaderStateChangedEventArgs(_sendBuffer[0], Constants.ReaderCallbackType.COMMUNICATION_ERROR));
                                            _sendBuffer.Clear();
                                            _PROTOCOL_RetryCount = 0;
                                        }
                                        else
                                        {
                                            _PROTOCOL_RetryCount++;
                                            CSLibrary.Debug.WriteLine("Command timeout");
                                            _NeedCommandResponseType = BTWAITCOMMANDRESPONSETYPE.NOWAIT;
                                        }
                                        break;

                                    case BTCOMMANDTYPE.Validate:
                                        if (_PROTOCOL_RetryCount > 0) // retry 1 times
                                        {
                                            // cancel all command and send error event

                                            CSLibrary.Debug.WriteLine("hardware fail!!");

                                            if (_sendBuffer[0].sendFailCallback != null)
                                                _sendBuffer[0].sendFailCallback();
                                            _NeedCommandResponseType = BTWAITCOMMANDRESPONSETYPE.NOWAIT;
                                            _sendBuffer.RemoveAt(0);
                                            _PROTOCOL_RetryCount = 0;
                                        }
                                        else
                                        {
                                            _PROTOCOL_RetryCount++;
                                            CSLibrary.Debug.WriteLine("Hardware vaildate command timeout");
                                            _NeedCommandResponseType = BTWAITCOMMANDRESPONSETYPE.NOWAIT;
                                        }
                                        break;

                                }

                            }
                        }

                        if (_NeedCommandResponseType == BTWAITCOMMANDRESPONSETYPE.NOWAIT)
                        {
                            if (_sendBuffer.Count > 0 && DateTime.Now > _packetDelayTimeout)
                            {
                                _currentCommandResponse = BTWAITCOMMANDRESPONSETYPE.NOWAIT;
                                _currentCmdRemark = _sendBuffer[0].cmdRemark;
                                _NeedCommandResponseType = _sendBuffer[0].dataRemark;
                                CheckRFIDCommand();
                                BLE_Send(_sendBuffer[0].packetData);

                                if (_NeedCommandResponseType == BTWAITCOMMANDRESPONSETYPE.NOWAIT)
                                {
                                    _sendBuffer.RemoveAt(0);
                                }
                                else
                                {
                                    _packetDelayTimeout = DateTime.Now;
                                    _packetResponseTimeout = DateTime.Now.AddSeconds(2);

                                    if (_currentCommandResponse == BTWAITCOMMANDRESPONSETYPE.NOWAIT)
                                    {
                                        if (_sendBuffer[0].packetData[2] == 0x02 && _sendBuffer[0].packetData[9] == 0x00)
                                        {
                                            switch (_sendBuffer[0].packetData[8])
                                            {
                                                case 0x80:
                                                    _packetDelayTimeout = DateTime.Now.AddSeconds(3);
                                                    break;

                                                case 0x90:
                                                    //_packetDelayTimeout = DateTime.Now.AddSeconds(1);
                                                    break;
                                            }
                                        } // barcode command delay
                                        else if (_sendBuffer[0].packetData[8] == 0x90 && _sendBuffer[0].packetData[9] == 0x03)
                                        {
                                            _packetDelayTimeout = DateTime.Now.AddMilliseconds(500);
                                        }
                                    }

                                    CSLibrary.Debug.WriteBytes("BT send data (" + _sendBuffer[0].dataRemark.ToString() + ")", _sendBuffer[0].packetData);
                                }

                            }
                        }
                    }
                    else
                    {
                        _sendBuffer.Clear();
                    }

                    if (_sendBuffer.Count == 0)
                        ExecuteFinishBLETask();

                    // battery routine
                    //_handleBattery.Timer();
                }
                catch (Exception ex)
                {
                    CSLibrary.Debug.WriteLine("BLERWEngineTimer exception : " + ex.Message);
                }
            }
        }

        #endregion

        #region ---- Public function ----

        public bool BLEBusy
        {
            get { return (_sendBuffer.Count != 0); }
        }

        #endregion

    }
}
