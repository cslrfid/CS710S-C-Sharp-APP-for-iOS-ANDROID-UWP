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
        UInt32[] _0000 = null;             // 0X0000~0X0002
        UInt32[] _0100 = null;              // 0x0100 ~ 0x010d
        UInt32[] _0117 = null;              // 0x117 ~ 0x11f
        UInt32[] _0200 = null;
        UInt32[] _0300 = null;
        UInt32[] _0400 = null;
        UInt32[] _0500 = null;             // 0x0500 ~ 0x0501
        UInt32[] _0600 = null;             // 0x0600 ~ 0x0603
        UInt32[] _0700 = null;             // 0x0700
        UInt32[] _0701 = null;             // 0x0701
        UInt32[,] _0702_707 = null;     // 0x0702 ~ 0x0707
        UInt32[] _0800 = null;             // 0x0800
        UInt32[,] _0801_80c = null;    // 0x0800 ~ 0x080c
        UInt32[] _0900 = null;             // 0X0900 ~ 0X0901
        UInt32[] _0902 = null;             // 0X0902
        UInt32[,] _0903_906 = null;     // 0X0903 ~ 0X0906
        UInt32[] _0907_908 = null;        // 0X0907 ~ 0X0908
        UInt32[] _0910_921 = null;        // 0X0910 ~ 0X0921
        UInt32[] _0a00_a07 = null;            // 0X0a00 ~ 0x0a0f
        UInt32[] _0a08 = null;            // 0X0a08
        UInt32[,] _0a09_a18 = null;          // 0X0a09 ~ 0x0a18
        UInt32[] _0b00 = null;          // 0x0b00 ~ 0x0b84
        UInt32[] _0c01 = null;             // 0X0c01
        UInt32[,] _0c02_c07 = null;    // 0X0c02 ~ 0x0c07
        UInt32[] _0c08 = null;             // 0X0c08
        UInt32[] _0d00 = null;
        UInt32[] _0e00 = null;
        UInt32[] _0f00_f05 = null;
        UInt32[] _0f0f = null;

        public bool MacRegisterInitialize()
        {
            /*
            _0000 = new UInt32[3];             // 0X0000~0X0002
            _0100 = new UInt32[0];
            _0200 = new UInt32[0];
            _0300 = new UInt32[0];              // 302, 304, 308 (Selector?)
            _0400 = new UInt32[0];              // 408 (Selector?)
            _0500 = new UInt32[2];             // 0x0500 ~ 0x0501
            _0600 = new UInt32[4];             // 0x0600 ~ 0x0603
            _0700 = new UInt32[2];             // 0x0700 ~ 0x0701 (Selector)
            _0702_707 = new UInt32[16, 6];     // 0x0702 ~ 0x0707
            _0800 = new UInt32[1];             // 0x0800 (Selector)
            _0801_80c = new UInt32[8, 12];     // 0x0800 ~ 0x080c
            _0900 = new UInt32[3];             // 0X0900 ~ 0X0902 (Selector)
            _0903_906 = new UInt32[4, 4];      // 0X0903 ~ 0X0906
            _0910_921 = new UInt32[12];        // 0X0910 ~ 0X0921 (Selector)
            _0a00 = new UInt32[16];            // 0X0a00 ~ 0x0a0f (Selector)
            _0b00 = new UInt32[0x85];          // 0x0b00 ~ 0x0b84
            _0c01 = new UInt32[2];             // 0X0c01 (Selector)
            _0c02_c07 = new UInt32[50, 6];     // 0X0c02 ~ 0x0c07
            _0c08 = new UInt32[1];             // 0X0c08
            _0d00 = new UInt32[0];
            _0e00 = new UInt32[0];
            _0f00 = new UInt32[0];

            _0700[0x00] = 0xffff;
            //_0700[0x02] = 0x0001;
            //_0700[0x05] = 0x07d0;
            //_0700[0x06] = 0x012c;
            _0900[0x00] = 0x00c0;
            _0900[0x01] = 0x0003;
            _0900[0x02] = 0x0003;
            //_0900[0x03] = 0x40f4;
            //_0900[0x05] = 0x0001;
            _0a00[0x01] = 0x0006;
            _0a00[0x02] = 0x0001;
            _0a00[0x03] = 0x0002;
            _0a00[0x04] = 0x0001;

            ReadReaderRegister(0x0000); // Get RFID Reader Firmware version
            //ReadReaderRegister(0x0800);
            */



            _0000 = new UInt32[3];             // 0X0000~0X0002
            _0100 = new UInt32[14];             // 0x0100 ~ 0x010d
            _0117 = new UInt32[9];              // 117 ~ 11f
            _0200 = new UInt32[4];
            _0300 = new UInt32[0];

            //_0302 = new UInt32[2];              // (Selector)
            //_0303 = new UInt32[11];

            //_0304 = new UInt32[2];              // (Selector)

            //_0305 = new UInt32[2];              // (Selector)

            //_0308 = new UInt32[2];              // (Selector)

            //_0309 = new UInt32[8];              

            _0400 = new UInt32[11];              // 0x0400-0x040A

            //_0408 = new UInt32[2];              // (Selector)


            _0500 = new UInt32[2];             // 0x0500 ~ 0x0501
            _0600 = new UInt32[4];             // 0x0600 ~ 0x0603
            _0700 = new UInt32[1];             // 0x0700 

            _0701 = new UInt32[2];              // (Selector)
            _0702_707 = new UInt32[16, 6];     // 0x0702 ~ 0x0707

            _0800 = new UInt32[2];             // (Selector)
            _0801_80c = new UInt32[8, 12];     // 0x0800 ~ 0x080c

            _0900 = new UInt32[3];             // 0X0900 ~ 0X0901

            _0902 = new UInt32[2];              // Selector
            _0903_906 = new UInt32[4, 4];      // 0X0903 ~ 0X0906

            _0907_908 = new UInt32[2];        // 0X0907 ~ 0X0908
            _0910_921 = new UInt32[12];        // 0X0910 ~ 0X0921

            _0a00_a07 = new UInt32[8];            // 0X0a00 ~ 0x0a07

            _0a08 = new UInt32[2];              // Selector
            _0a09_a18 = new UInt32[8, 16];      // 0X0a09 ~ 0X0a18

            _0b00 = new UInt32[0x85];          // 0x0b00 ~ 0x0b84

            //_0b61 = new UInt32[2];              // Selector
            //_0b62_b

            _0c01 = new UInt32[2];             // 0X0c01 (Selector)
            _0c02_c07 = new UInt32[50, 6];     // 0X0c02 ~ 0x0c07

            _0c08 = new UInt32[1];             // 0X0c08

            _0d00 = new UInt32[0];
            _0e00 = new UInt32[0];

            _0f00_f05 = new UInt32[6];          // 0x0f00 ~ 0x0f05
            _0f0f = new UInt32[1];



            _0200[0x00] = 0x0000;
            _0200[0x01] = 0x0000;
            _0200[0x03] = 0x0000;
            _0700[0x00] = 0xffff;

            _0702_707[0, 0] = 0x01;
            _0702_707[1, 2] = 0x10001;
            _0702_707[2, 2] = 0x20002;
            _0702_707[3, 2] = 0x30003;
            for (int cnt = 0; cnt < 16; cnt++)
            {
                _0702_707[cnt, 3] = 0x7d0;
                _0702_707[cnt, 4] = 0x12c;
                _0702_707[cnt, 5] = 0x2000;
            }
            _0900[0x00] = 0x00c0;
            _0900[0x01] = 0x0001;
            _0900[0x02] = 0x0003;
            _0903_906[0x00, 0x00] = 0x04;
            _0903_906[0x03, 0x00] = 0x40F4;
            _0903_906[0x00, 0x02] = 0x03;
            _0903_906[0x0, 0x02] = 0x01;
            _0903_906[0x02, 0x02] = 0x01;
            _0903_906[0x03, 0x02] = 0x01;

            //_0900[0x03] = 0x40f4;
            //_0900[0x05] = 0x0001;
            _0a00_a07[0x01] = 0x0006;
            _0a00_a07[0x02] = 0x0001;
            _0a00_a07[0x03] = 0x0002;
            _0a00_a07[0x04] = 0x0001;

            for (int cnt = 0; cnt < 50; cnt++)
                for (int cnt1 = 0; cnt1 < 6; cnt1++)
                    _0c02_c07[cnt, cnt1] = 0xffffff;

            ReadReaderRegister(0x0000); // Get RFID Reader Firmware version
            //ReadReaderRegister(0x0800);
            WriteMacRegister(0x201, 0x010);

            return true;
        }

        /*            public bool MacRegisterInitialize (uint model, uint country, uint specialVersion, uint oemVersion)
                {
                    switch (model)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 4:
                            break;
                        case 7:
                            break;
                        case 8:
                            break;
                        case 9:
                            break;

                        default:
                            break;
                    }

                    ReadReaderRegister(0x000);                                          // Get RFID Reader Firmware version

                    //ReadReaderRegister((UInt16)MACREGISTER.HST_ANT_DESC_RFPOWER);      // Get Antenna 0 Power Level            
                    // ReadReaderRegister((UInt16)MACREGISTER.HST_ANT_CYCLES);
                    // MacWriteRegister(MACREGISTER.HST_ANT_DESC_SEL, 0);  // Set Antenna 0 
                    // ReadReaderRegister((UInt16)MACREGISTER.HST_ANT_DESC_RFPOWER);   // Get Antenna 0 Power Level
                    // ReadReaderRegister((UInt16)MACREGISTER.HST_ANT_DESC_DWELL);
                    // ReadReaderRegister((UInt16)MACREGISTER.HST_QUERY_CFG);
                    // ReadReaderRegister((UInt16)MACREGISTER.HST_INV_CFG);
                    // ReadReaderRegister((UInt16)MACREGISTER.HST_INV_EPC_MATCH_CFG);
                    // ReadReaderRegister((UInt16)MACREGISTER.HST_TAGACC_DESC_CFG);
                    // ReadReaderRegister(0x005); // reader mac error register

                    return false;
                }
        */

        private void ReadReaderRegister(UInt16 add)
        {
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(add), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1);
        }

        /// <summary>
        /// for compatible with old library
        /// </summary>
        /// <param name="add"></param>
        /// <param name="value"></param>
        internal void MacWriteRegister(MACREGISTER add, UInt32 value)
        {
            CSLibrary.Debug.WriteLine("MAC Write {0:X}:{1:X}", add, value);
            WriteMacRegister((UInt16)add, value);
            //MacWriteRegister((UInt16)add, value);
        }

        private bool MacReadRegister(MACREGISTER add, ref UInt32 data)
        {
            data = ReadMacRegister((UInt16)add);
            CSLibrary.Debug.WriteLine("MAC Read {0:X}:{1:X}", add, data);
            return true;
        }

        bool SendRegisterAsync (UInt16 address, UInt32 data)
        {
            return _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
        }

        internal UInt32 ReadMacRegister(UInt16 address)
        {
            UInt16 addressBench = (UInt16)(address & 0x0f00U);
            UInt16 addressoffset = (UInt16)(address & 0x00ffU);

            try
            {   switch (addressBench)
                {
                    case 0x0000:
                        if (addressoffset == 0x0000)
                        {
                            return _0000[addressoffset];
                        }
                        else
                        {
                            ReadReaderRegister(address);
                        }
                        break;

                    case 0x0100:
                        if (addressoffset >= 0x0017 && addressoffset  <= 0x001f)
                            return _0117[addressoffset - 0x17];

                        return _0100[addressoffset];
                        break;

                    case 0x0200:
                        return _0200[addressoffset];
/*                        if (addressoffset == 0x0001)
                            return _0201[0];
                        else if (addressoffset == 0x0003)
                            return _0203[0];*/
                        break;

                    case 0x0300:
                        return _0300[addressoffset];
                        break;

                    case 0x0400:
                        return _0400[addressoffset];
                        break;

                    case 0x0500:
                        return _0500[addressoffset];
                        break;

                    case 0x0600:
                        return _0600[addressoffset];
                        break;

                    case 0x0700:
                        if (addressoffset == 0x0000)
                            return _0700[0];
                        else if (addressoffset == 0x01)
                            return _0701[1];
                        else if (addressoffset >= 0x0002 && addressoffset <= 0x0007)
                            return _0702_707[_0701[1], addressoffset - 2];
                        break;

                    case 0x0800:
                        if (addressoffset == 0x0000)
                            return _0800[1];
                        else if (addressoffset >= 0x0001 && addressoffset <= 0x000c)
                            return _0801_80c[_0800[1], addressoffset - 1];
                        break;

                    case 0x0900:
                        if (addressoffset >= 0x0000 && addressoffset <= 0x0001)
                            return _0900[addressoffset];
                        else if (addressoffset == 0x02)
                            return _0902[1];
                        else if (addressoffset >= 0x0003 && addressoffset <= 0x006)
                            return _0903_906[_0902[1], addressoffset - 3];
                        else if (addressoffset >= 0x0007 && addressoffset <= 0x008)
                            return _0907_908[addressoffset - 0x07];
                        else if (addressoffset >= 0x0010 && addressoffset <= 0x021)
                            return _0910_921[addressoffset - 0x10];
                        break;

                    case 0x0a00:
                        if (addressoffset >= 0x0000 && addressoffset <= 0x0007)
                            return _0a00_a07[addressoffset];
                        else if (addressoffset == 0x08)
                            return _0a08[1];
                        else if (addressoffset >= 0x0009 && addressoffset <= 0x0018)
                            return _0a09_a18[_0a08[1], addressoffset - 9];
                        break;

                    case 0x0b00:
                        return _0b00[addressoffset];
                        break;

                    case 0x0c00:
                        if (addressoffset == 0x0001)
                            return _0c01[1];
                        else if (addressoffset >= 0x0002 && addressoffset <= 0x0007)
                            return _0c02_c07[_0c01[1], addressoffset - 2];
                        if (addressoffset == 0x0008)
                            return _0c08[0];
                        break;

                    case 0x0d00:
                        return _0d00[addressoffset];
                        break;

                    case 0x0e00:
                        return _0e00[addressoffset];
                        break;

                    case 0x0f00:
                        if (addressoffset == 0x000f)
                            return _0f0f[0];
                        else if (addressoffset >= 0x0000 && addressoffset <= 0x0005)
                            return _0f00_f05[addressoffset];
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine(ex.Message);
            }

            return 0;
        }

        public bool SaveMacRegister (UInt16 address, UInt32 data)
        {
            if (address > 0x0001)
                return false;

            UInt16 addressoffset = (UInt16)(address & 0x00ffU);

            _0000[addressoffset] = data;
            return true;
        }

        public void WriteMacRegister(UInt16 address, UInt32 data)
        {
            UInt16 addressBench = (UInt16)(address & 0x0f00U);
            UInt16 addressoffset = (UInt16)(address & 0x00ffU);

            try
            {
                switch (addressBench)
                {
                    case 0x0000:
                        if (data != _0000[addressoffset])
                        {
                            _0000[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0100:
                        if (addressoffset >= 0x0017 && addressoffset <= 0x001f)
                        {
                            int location = addressoffset - 0x17;
                            if (data != _0117[location])
                            {
                                _0117[location] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        else if (data != _0100[addressoffset])
                        {
                            _0100[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0200:
                        if (data != _0200[addressoffset])
                        {
                            _0200[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }

                        /*                        if (addressoffset == 0x01)
                                                {
                                                    if (data != _0201[0])
                                                    {
                                                        _0201[0] = data;
                                                        _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                                                    }
                                                }
                                                else if (addressoffset == 0x03)
                                                {
                                                    if (data != _0203[0])
                                                    {
                                                        _0203[0] = data;
                                                        _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                                                    }
                                                }*/
                        /*if (data != _0200[addressoffset])
                        {
                            _0200[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }*/
                        break;

                    case 0x0300:
                        if (data != _0300[addressoffset])
                        {
                            _0300[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0400:
                        if (data != _0400[addressoffset])
                        {
                            _0400[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0500:
                        if (data != _0500[addressoffset])
                        {
                            _0500[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0600:
                        if (data != _0600[addressoffset])
                        {
                            _0600[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0700:
                        if (addressoffset == 0x0000)
                        {
                            if (data != _0700[addressoffset])
                            {
                                _0700[addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        else if (addressoffset == 0x01)
                        {
                            _0701[1] = data;
                        }
                        else if (addressoffset >= 0x0002 && addressoffset <= 0x0007)
                        {
                            addressoffset -= 2;
                            if (data != _0702_707[_0701[0x0001], addressoffset])
                            {
                                if (_0701[0] != _0701[1])
                                {
                                    _0701[0] = _0701[1];
                                    _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0x701, _0701[0]), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                                }

                                _0702_707[_0701[1], addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        break;

                    case 0x0800:
                        if (addressoffset == 0x0000)
                        {
                            _0800[1] = data;
                        }
                        else if (addressoffset >= 0x0001 && addressoffset <= 0x000c)
                        {
                            addressoffset -= 1;
                            if (data != _0801_80c[_0800[1], addressoffset])
                            {
                                if (_0800[0] != _0800[1])
                                {
                                    _0800[0] = _0800[1];
                                    _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0x800, _0800[0]), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                                }

                                _0801_80c[_0800[1], addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        break;

                    case 0x0900:
                        if (addressoffset >= 0x0000 && addressoffset <= 0x0001)
                        {
                            if (data != _0900[addressoffset])
                            { 
                                _0900[addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        else if (addressoffset == 0x0002)
                        {
                            _0902[1] = data;
                        }
                        else if (addressoffset >= 0x0003 && addressoffset <= 0x0006)
                        {
                            addressoffset -= 3;
                            if (data != _0903_906[_0902[1], addressoffset])
                            {
                                if (_0902[0] != _0902[1])
                                {
                                    _0902[0] = _0902[1];
                                    _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0x902, _0902[0]), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                                }

                                _0903_906[_0902[1], addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        else if (addressoffset >= 0x0007 && addressoffset <= 0x0008)
                        {
                            addressoffset -= 0x0007;
                            if (data != _0907_908[addressoffset])
                            {
                                _0907_908[addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        else if (addressoffset >= 0x0010 && addressoffset <= 0x0021)
                        {
                            addressoffset -= 0x0010;
                            if (data != _0910_921[addressoffset])
                            {
                                _0910_921[addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        break;

                    case 0x0a00:
                        if (addressoffset >= 0x0000 && addressoffset <= 0x0007)
                        {
                            if (data != _0a00_a07[addressoffset])
                            {
                                _0a00_a07[addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        else if (addressoffset == 0x08)
                        {
                            _0a08[1] = data;
                        }
                        else if (addressoffset >= 0x0009 && addressoffset <= 0x0018)
                        {
                            addressoffset -= 0x0009;
                            if (data != _0a09_a18[_0a08[1], addressoffset])
                            {
                                if (_0a08[0] != _0a08[1])
                                {
                                    _0a08[0] = _0a08[1];
                                    _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0x0a08, _0a08[0]), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                                }

                                _0a09_a18[_0a08[1], addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }


                        }
                        break;

                    case 0x0b00:
                        if (data != _0b00[addressoffset])
                        {
                            _0b00[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0c00:
                        if (addressoffset == 0x0001)
                        {
                            _0c01[1] = data;
                        }
                        else if (addressoffset >= 0x0002 && addressoffset <= 0x0007)
                        {
                            addressoffset -= 2;
                            if (data != _0c02_c07[_0c01[1], addressoffset])
                            {
                                if (_0c01[0] != _0c01[1])
                                {
                                    _0c01[0] = _0c01[1];
                                    _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xc01, _0c01[0]), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                                }

                                _0c02_c07[_0c01[1], addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        if (addressoffset == 0x0008)
                        {
                            if (data != _0c08[0x00])
                            {
                                _0c08[0x00] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        break;

                    case 0x0d00:
                        if (data != _0d00[addressoffset])
                        {
                            _0d00[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0e00:
                        if (data != _0e00[addressoffset])
                        {
                            _0e00[addressoffset] = data;
                            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                        }
                        break;

                    case 0x0f00:
                        if (addressoffset == 0x000f)
                        {
                            if (data != _0f0f[0])
                            {
                                _0f0f[0] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        } else if (addressoffset >= 0x0000 && addressoffset <= 0x0005)
                        {
                            if (data != _0f00_f05[addressoffset])
                            {
                                _0f00_f05[addressoffset] = data;
                                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(address, data), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine(ex.Message);
            }
        }


    }
}
