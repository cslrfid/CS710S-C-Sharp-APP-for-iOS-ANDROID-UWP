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
using System.Xml.Schema;
using CSLibrary.Barcode;
using CSLibrary.Barcode.Constants;
using CSLibrary.Barcode.Structures;

namespace CSLibrary
{
    public partial class SiliconLabIC
    {
        public event EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs> OnAccessCompleted;

        uint _firmwareVersion;
        public bool _firmwareOlderT108 = false;
        string _serailNumber = null;
        string _PcbVersion;
        string _Model;


        // RFID event code
        private class DOWNLINKCMD
		{
			public static readonly byte[] GETVERSION = { 0xB0, 0x00 };
            public static readonly byte[] GETSERIALNUMBER = { 0xB0, 0x04 };

            // for CS710S only
            public static readonly byte[] GETMODEL = { 0xB0, 0x06 };
            public static readonly byte[] RESETATMEL = { 0xB0, 0x0C };
        }

        private HighLevelInterface _deviceHandler;

        internal SiliconLabIC(HighLevelInterface handler)
		{
			_deviceHandler = handler;
        }

        internal void Connect (CSLibrary.RFIDDEVICE.MODEL model)
        {
            //internal void GetVersion()
            //{
            _deviceHandler.SendAsync(0, 3, DOWNLINKCMD.GETVERSION, null, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
            //}

            //internal void GetSerialNumber()
            //{
            _deviceHandler.SendAsync(0, 3, DOWNLINKCMD.GETSERIALNUMBER, new byte[1], HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
            //}

            if (model == RFIDDEVICE.MODEL.CS710S)
                _deviceHandler.SendAsync(0, 3, DOWNLINKCMD.GETMODEL, new byte[1], HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }

        internal HighLevelInterface.BTWAITCOMMANDRESPONSETYPE ProcessDataPacket (byte [] data)
        {
            uint pktType = (uint)(data[8] << 8 | data[9]);

            switch (pktType)
            {
                case 0xb000:    // version
                    if (data[2] == 0x03) // for CS463
                    {
                        _firmwareVersion = 0;
                        _firmwareOlderT108 = false;
                        return HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
                    }

                    _firmwareVersion = (uint)((data[10] << 16) | (data[11] << 8) | (data[12]));
                    if (_firmwareVersion < 0x00010008)
                        _firmwareOlderT108 = true;
                    return HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;

                case 0xb004:    // serial number
                                //_serailNumber = System.Text.Encoding.UTF8.GetString(data, 10, 13);
                                //_PcbVersion = (uint)(((data[23] - 0x30) << 16) | ((data[24] - 0x30) << 8) | (data[25] - 0x30));
                                //_PcbVersion = (uint)(((((data[23] & 0x0f) << 8) * 10) + ((data[24] &0x0f) << 8)) | (data[25] - 0x30));
                    try
                    {
                        _serailNumber = System.Text.Encoding.UTF8.GetString(data, 10, 16);
                    }
                    catch (Exception ex)
                    {
                        _serailNumber = "";

                    }

                    try
                    {
                        if (data[25] == 0x00)
                            data[25] = 0x30;
                        else if (data[23] == 0x30)
                        {
                            data[23] = data[24];
                            data[24] = data[25];
                            data[25] = 0x30;
                        }

                        _PcbVersion = System.Text.Encoding.UTF8.GetString(data, 23, 3);
                    }
                    catch (Exception ex)
                    {
                        _PcbVersion = "";
                    }
                    return HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;

                case 0xb006:    // get CS710S model
                    try
                    {
                        _Model = System.Text.Encoding.UTF8.GetString(data, 10, 32);
                    }
                    catch (Exception ex)
                    {
                        _Model = "";
                    }
                    return HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
            }

            return 0;
        }

        public UInt32 GetFirmwareVersion ()
        {
            return _firmwareVersion;
        }

        public void GetSerialNumber()
        {
            if (OnAccessCompleted != null)
            {
                try
                {
                    Events.OnAccessCompletedEventArgs args = new Events.OnAccessCompletedEventArgs(_serailNumber, Constants.AccessCompletedCallbackType.SERIALNUMBER);

                    OnAccessCompleted(this, args);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public string GetSerialNumberSync()
        {
            return _serailNumber;
        }

        public string GetPCBVersion ()
        {
            return _PcbVersion;
        }

        public void ClearEventHandler()
        {
            OnAccessCompleted = delegate { };
        }
    }
}
