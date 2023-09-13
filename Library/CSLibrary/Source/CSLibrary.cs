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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        #region Constant
        // CS108 State
        public enum READERSTATE
        {
            DISCONNECT,
            IDLE,
            BUSY,
            READYFORDISCONNECT
        }

        // CS108 Command
        private class DOWNLINKCMD
        {
            // RFID event code
            public static readonly byte[] RFIDPOWERON = { 0x80, 0x00 };
            public static readonly byte[] RFIDPOWEROFF = { 0x80, 0x01 };
            public static readonly byte[] RFIDCMD = { 0x80, 0x02 };

            // Barcode event code
            public static readonly byte[] BARCODEPOWERON = { 0x90, 0x00 };
            public static readonly byte[] BARCODEPOWEROFF = { 0x90, 0x01 };
            public static readonly byte[] BARCODESCANTRIGGER = { 0x90, 0x02 };
            public static readonly byte[] BARCODERAWDATA = { 0x90, 0x03 };

            // CS108 status event code
            public static readonly byte[] GETVOLTAGE = { 0xA0, 0x00 };
            public static readonly byte[] GETTRIGGERSTATE = { 0xA0, 0x01 };
            public static readonly byte[] STARTAUTOREPORTING = { 0xA0, 0x02 };
            public static readonly byte[] STOPAUTOREPORTING = { 0xA0, 0x03 };
            public static readonly byte[] RFIDABORT = { 0xA0, 0x04 };
            public static readonly byte[] GETTRIGGER = { 0xA0, 0x05 };
            public static readonly byte[] SETFASTMODE = { 0xA0, 0x06 };
            public static readonly byte[] GETFASTMODE = { 0xA0, 0x07 };
            public static readonly byte[] STARTAUTOREIGGERREPORTING = { 0xA0, 0x08 };
            public static readonly byte[] STOPAUTOREIGGERREPORTING = { 0xA0, 0x09 };

            // Silicon Lab IC event code
            public static readonly byte[] GETSILICONVER = { 0xB0, 0x00 };
            public static readonly byte[] SLIMAGERAWDATA = { 0xB0, 0x01 };
            public static readonly byte[] SLBOOTLOADERRAWDATA = { 0xB0, 0x02 };

            // BlueTooth event code
            public static readonly byte[] GETVLUETOOTHVER = { 0xB0, 0x00 };
            public static readonly byte[] BTIMAGERAWDATA = { 0xB0, 0x01 };
            public static readonly byte[] BTBOOTLOADERRAWDATA = { 0xB0, 0x02 };
            public static readonly byte[] SETDEVICENAME = { 0xB0, 0x03 };
            public static readonly byte[] GETDEVICENAME = { 0xB0, 0x04 };
        }

        private class UPLINKCMD
        {
            public static readonly byte[] RFIDDATA = { 0x81, 0x00 };

            public static readonly byte[] BARCODEDATA = { 0x90, 0x00 };
            public static readonly byte[] BARCODEVALID = { 0x90, 0x01 };

            public static readonly byte[] BATTERYFAILED = { 0xA1, 0x00 };
            public static readonly byte[] ERRORCODE = { 0xA1, 0x01 };
            public static readonly byte[] TRIGGERPUSHED = { 0xA1, 0x02 };
            public static readonly byte[] TRIGGERRELEASED = { 0xA1, 0x03 };
        }

        // Constant
        #endregion

        #region internal variable

        private READERSTATE _readerState = READERSTATE.DISCONNECT;
        public READERSTATE Status { get { return _readerState; } }

        /// <summary>
        /// Reader Operation State Event
        /// </summary>
        public event EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs> OnReaderStateChanged;

        #endregion 

        #region public variable

        private SiliconLabIC _handleSiliconLabIC = null;
        private RFIDReader _handlerRFIDReader = null;
        private BarcodeReader _handleBarCodeReader = null;
        private Notification _handleNotification = null;
        private BluetoothIC _handleBluetoothIC = null;
        internal Battery _handleBattery = null;

        public SiliconLabIC siliconlabIC
        {
            get { return _handleSiliconLabIC; }
        }

        public RFIDReader rfid
        {
            get { return _handlerRFIDReader; }
            //set { _handlerRFIDReader = value; }
        }

        public BarcodeReader barcode
        {
            get { return _handleBarCodeReader; }
            //set { _handleBarCodeReader = value; }
        }

        public Notification notification
        {
            get { return _handleNotification; }
            //set { _handleNotification = value; }
        }

        public BluetoothIC bluetoothIC
        {
            get { return _handleBluetoothIC; }
            //set { _handleBluetoothIC = value; }
        }

        public Battery battery
        {
            get { return _handleBattery; }
        }

        public string ReaderName
        {
            get
            {
                var value = _handleBluetoothIC.GetDeviceName();

                if (value == null)
                    return _device.Name;
                else
                    return value;
            }
        }

        internal string _MacAdd = null;
        public string GetMacAddress()
        {
            return _MacAdd;
        }


        #endregion

        public HighLevelInterface()
        {
            // Basic Module
            _handleSiliconLabIC = new SiliconLabIC(this);
            _handlerRFIDReader = new RFIDReader(this);
            _handleBarCodeReader = new BarcodeReader(this);
            _handleNotification = new Notification(this);
            _handleBluetoothIC = new BluetoothIC(this);
            _handleBattery = new Battery(this);

            BLE_Init();
        }

        ~HighLevelInterface()
        {
            DisconnectAsync();
        }

        public void CancelEventOnReaderStateChanged()
        {
            OnReaderStateChanged = null;
        }


        public Version GetVersion ()
        {
            Version ver = new Version(2, 0, 10, 1);
            return ver;
        }

        CSLibrary.Timer BTTimer;
        void HardwareInit()
        {
            BARCODEPowerOn();
            RFIDPowerOff();
            RFIDPowerOn();

            _handleBluetoothIC.Connect();
            _handleSiliconLabIC.Connect(_deviceType);
            _handleBarCodeReader.CheckHWValid();

            //rfid.StopOperation();
            //rfid.Connect(MODEL.CS710S);
            rfid.Connect(_deviceType);

            //rfid.ResetToDefaultPowerMode();
            //rfid.SetReaderPowerMode(false);
            //rfid.SetToStandbyMode();

            notification.SetAutoReport(true);
            //notification.SetAutoTriggerReport(0); // off auto trigger report // cancel SetAutoTriggerReport commnad to compatible with old BT firmware

            FireReaderStateChangedEvent(new Events.OnReaderStateChangedEventArgs(_sendBuffer[0], Constants.ReaderCallbackType.CONNECT_SUCESS));
        }

        Func <Task> _afterFinishBLETask = null;
        void WhenBLEFinish(Func<Task> nextTask)
        {
            _afterFinishBLETask = nextTask;
        }

        void ExecuteFinishBLETask()
        {
            if (_afterFinishBLETask == null)
                return;

            _afterFinishBLETask();
            _afterFinishBLETask = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packetData"></param>
        public bool ProcessAPIPacket (byte [] recData)
        {
            byte [] data = (byte [])recData.Clone();

            //CSLibrary.Debug.WriteLine("Routine : ProcessAPIPacket");

            switch (data[3])
            {
                case 0xc2:
                    // check packet running number
                    if (data[5] == 0x9e && data[8] == 0x81 && data[9] == 0x00)
                    {
                        _blePacketRunningNumber++;
                        if (data[4] != _blePacketRunningNumber)
                        {
                            if (data[4] > _blePacketRunningNumber)
                                InventoryDebug.InventorySkipPackerAdd((uint)(data[4] - _blePacketRunningNumber));
                            else 
                                InventoryDebug.InventorySkipPackerAdd((uint)((256U + data[4]) - _blePacketRunningNumber));

                            _blePacketRunningNumber = data[4];
                        }
                    }

                    _currentCommandResponse |= RecvRFIDPacket(data);
                    break;

                case 0x6a:
                    switch (RecvBarcodePacket(data))
                    {
                        case BARCODECOMMANDRESPONSETYPE.CONTROLCOMMAND:
                            _currentCommandResponse |= BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
                            break;

                        case BARCODECOMMANDRESPONSETYPE.DATA:
                            _currentCommandResponse |= BTWAITCOMMANDRESPONSETYPE.DATA1;
                            break;

                        case BARCODECOMMANDRESPONSETYPE.NOTIFICATION:
                        case BARCODECOMMANDRESPONSETYPE.ERROR:
                        default:
                            return true;
                    }
                    break;

                case 0xd9:
                    if (RecvNofigicationPacket (data))
						_currentCommandResponse |= BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
					break;
                    //return false;

                case 0xe8:
                    _currentCommandResponse |= _handleSiliconLabIC.ProcessDataPacket(data);
                    break;

                case 0x5f:
                    if (_handleBluetoothIC.BluetoothICPacket(data))
                        _currentCommandResponse |= BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
                    break;
            }

            BLERWEngineTimer(); // send next packet immediately
            return true;
        }

        void TimerFunc(object o)
        {
            if (_readerState == READERSTATE.READYFORDISCONNECT || _readerState == READERSTATE.DISCONNECT)
            {
                BTTimer.Cancel();
                _sendBuffer.Clear();
                _NeedCommandResponseType = BTWAITCOMMANDRESPONSETYPE.NOWAIT;
                _readerState = READERSTATE.DISCONNECT;
                return;
            }

            BLERWEngineTimer();
            return;
        }

		internal CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE RecvRFIDPacket(byte[] recvData)
        {
            CSLibrary.Debug.WriteLine("Routine : RecvRFIDPacket");

            UInt16 eventCode = (UInt16)((UInt16)recvData[8] << 8 | (UInt16)recvData[9]);

            switch (eventCode)
            {
                case 0x8000:
                    {
                        // power on
                        var result = recvData[10];
                    }
                    break;

                case 0x8001:
                    {
                        // power off
                        var result = recvData[10];
                    }
                    break;

                case 0x8002:
                    {
                        var result = recvData[10];
                    }
                    break;

                case 0x8100:
                    //return _handlerRFIDReader.DeviceRecvData (recvData, _currentCommandResponse);
                    return rfid.DeviceRecvData(recvData, _currentCommandResponse);

                default:
                    // packet data error
                    return BTWAITCOMMANDRESPONSETYPE.NOWAIT;
            }

            return  BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
        }

        enum BARCODECOMMANDRESPONSETYPE
        {
            CONTROLCOMMAND,
            DATA,
            NOTIFICATION,
            ERROR
        }

		BARCODECOMMANDRESPONSETYPE RecvBarcodePacket (byte[] recvData)
        {
            UInt16 eventCode = (UInt16)((UInt16)recvData[8] << 8 | (UInt16)recvData[9]);

            switch (eventCode)
            {
                case 0x9000:
                    {
                        // power on
                        //_handleBarCodeReader.DeviceRecvState(1);
                        return BARCODECOMMANDRESPONSETYPE.CONTROLCOMMAND;
                    }

                case 0x9001:
                    {
                        // power off
                        //_handleBarCodeReader.DeviceRecvState(0);
                        return BARCODECOMMANDRESPONSETYPE.CONTROLCOMMAND;
                    }

                case 0x9002:
					//_handlerRFIDReader.DeviceRecvData(recvData, 10, recvData.Length - 10);
					return BARCODECOMMANDRESPONSETYPE.CONTROLCOMMAND;

				case 0x9003:
					//_handlerRFIDReader.DeviceRecvData(recvData, 10, recvData.Length - 10);
					return BARCODECOMMANDRESPONSETYPE.CONTROLCOMMAND;

                case 0x9004:
                    return BARCODECOMMANDRESPONSETYPE.CONTROLCOMMAND;

                case 0x9005:
                    return BARCODECOMMANDRESPONSETYPE.CONTROLCOMMAND;

                case 0x9100:    // (barcode reader data)
                    if (_handleBarCodeReader.DeviceRecvData(recvData))
                        return BARCODECOMMANDRESPONSETYPE.DATA;
                    break;

                case 0x9101:	// Good read (ignore)
                    _handleBarCodeReader.DeviceRecvGoodRead();
                    return BARCODECOMMANDRESPONSETYPE.NOTIFICATION;
            }

            return BARCODECOMMANDRESPONSETYPE.ERROR;
        }

        internal bool RecvNofigicationPacket(byte[] recvData)
        {
			UInt16 eventCode = (UInt16)((UInt16)recvData[8] << 8 | (UInt16)recvData[9]);

			switch (eventCode)
			{
				case 0xa000:    // Current battery voltage
                    _handleNotification.DeviceRecvVoltage((UInt16)((UInt16)recvData[10] << 8 | (UInt16)recvData[11]));
                    return false;
					break;

				case 0xa001:    // Button Status
					switch (recvData[10])
					{
						case 0x00:
							_handleNotification.DeviceRecvState(1);              // Send event to application
							break;

						case 0x01:
							_handleNotification.DeviceRecvState(0);              // Send event to application
							break;
					}
                    return false;
                    break;

                case 0xa002:
                    break;

                case 0xa006:
                    break;

                case 0xa101:        // Error Code
					CSLibrary.Debug.WriteLine("Error : CS108 Error Code : {0}", (UInt16)((UInt16)recvData[10] << 8 | (UInt16)recvData[11]));
                    return false;
                    break;

				case 0xa102:      // Button On
					_handleNotification.DeviceRecvState(0);              // Send event to application
					return false;
					break;

				case 0xa103:      // Button Off
					_handleNotification.DeviceRecvState(1);              // Send event to application
					return false;
					break;
			}

			return true;
        }

        // public RFID function
        internal void RFIDPowerOn()
        {
            SendAsync(0, 0, DOWNLINKCMD.RFIDPOWERON, null, BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }

		internal void RFIDPowerOff()
        {
            SendAsync(0, 0, DOWNLINKCMD.RFIDPOWEROFF, null, BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }

        // public barcode function
        internal void BARCODEPowerOn()
        {
			SendAsync(0, 1, DOWNLINKCMD.BARCODEPOWERON, null, BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }

        internal void BARCODEPowerOff()
        {
            SendAsync(0, 1, DOWNLINKCMD.BARCODEPOWEROFF, null, BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }

    }
}
