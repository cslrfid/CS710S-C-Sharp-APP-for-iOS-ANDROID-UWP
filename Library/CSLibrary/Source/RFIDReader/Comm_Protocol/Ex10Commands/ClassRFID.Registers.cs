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
using static CSLibrary.BarcodeReader;

namespace CSLibrary
{
    using static RFIDDEVICE;
    using static FrequencyBand;
    using static CSLibrary.RFIDReader;

    public partial class RFIDReader
    {
        public enum REGPRIVATE
        {
            READWRITE,
            READONLY,
            WRITEONLY
        }


        internal class Regbyte
        {
            private RFIDReader _handler;
            public UInt16 regAdd;      // register address
            public byte value;      // value
            public REGPRIVATE Private;

            public Regbyte(RFIDReader handler, UInt16 add, REGPRIVATE Private)
            {
                this._handler = handler;
                this.regAdd = add;
                this.Private = Private;
            }

            public void Set(byte value)
            {
                if (this.value == value)
                    return;

                this.value = value;

                if (Private == REGPRIVATE.READONLY)
                    return;

                _handler.WriteRegister(this.regAdd, value);
            }

            public void Default(byte value)
            {
                this.value = value;
            }

            public byte Get()
            {
                return value;
            }
        }

        internal class RegUInt16
        {
            private RFIDReader _handler;
            public UInt16 regAdd;
            public UInt16 value;
            public REGPRIVATE Private;

            public RegUInt16(RFIDReader handler, UInt16 add, REGPRIVATE Private)
            {
                this._handler = handler;
                regAdd = add;
                this.Private = Private;
            }

            public void Set(UInt16 value)
            {
                if (this.value == value)
                    return;

                this.value = value;

                if (Private == REGPRIVATE.READONLY)
                    return;

                _handler.WriteRegister(this.regAdd, value);
            }

            public void Default(UInt16 value)
            {
                this.value = value;
            }

            public UInt16 Get()
            {
                return value;
            }
        }

        internal class RegUInt24
        {
            private RFIDReader _handler;
            public UInt16 regAdd;
            public UInt32 value;
            public REGPRIVATE Private;

            public RegUInt24(RFIDReader handler, UInt16 add, REGPRIVATE Private)
            {
                this._handler = handler;
                regAdd = add;
                this.Private = Private;
            }

            public void Set(UInt32 value)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (this.value == value)
                    return;

                this.value = value;

                byte[] byteValue = new byte[3];
                byteValue[2] = (byte)(value & 0xff);
                byteValue[1] = (byte)(value >> 8 & 0xff);
                byteValue[0] = (byte)(value >> 16 & 0xff);

                _handler.WriteRegister(this.regAdd, byteValue);
            }

            public void Default(UInt16 value)
            {
                this.value = value;
            }

            public UInt32 Get()
            {
                return value;
            }
        }

        internal class RegUInt32
        {
            private RFIDReader _handler;
            public UInt16 regAdd;
            public UInt32 value;
            public REGPRIVATE Private;

            public RegUInt32(RFIDReader handler, UInt16 add, REGPRIVATE Private)
            {
                this._handler = handler;
                regAdd = add;
                this.Private = Private;
            }

            public void Set(UInt32 value)
            {
                if (this.value == value)
                    return;

                this.value = value;

                if (Private == REGPRIVATE.READONLY)
                    return;

                _handler.WriteRegister(this.regAdd, value);
            }

            public void Default(UInt32 value)
            {
                this.value = value;
            }

            public UInt32 Get()
            {
                return value;
            }
        }

        internal class RegUInt64
        {
            private RFIDReader _handler;
            public UInt16 regAdd;
            public UInt64 value;
            public REGPRIVATE Private;

            public RegUInt64(RFIDReader handler, UInt16 add, REGPRIVATE Private)
            {
                this._handler = handler;
                regAdd = add;
                this.Private = Private;
            }

            public void Set(UInt64 value)
            {
                if (this.value == value)
                    return;

                this.value = value;

                if (Private == REGPRIVATE.READONLY)
                    return;

                _handler.WriteRegister(this.regAdd, value);
            }

            public void Default(UInt64 value)
            {
                this.value = value;
            }

            public UInt64 Get()
            {
                return value;
            }
        }

        internal class Regstring
        {
            private RFIDReader _handler;
            public UInt16 regAdd;
            public string value;
            public int maxlen;
            public REGPRIVATE Private;

            public Regstring(RFIDReader handler, UInt16 add, int len, REGPRIVATE Private)
            {
                this._handler = handler;
                regAdd = add;
                maxlen = len;
                this.Private = Private;
            }

            public void Set(string value)
            {
                if (this.value == value)
                    return;

                if (value.Length > maxlen)
                    return;

                this.value = value;
                
                if (Private == REGPRIVATE.READONLY)
                    return;

                byte[] bytes = Encoding.ASCII.GetBytes(this.value);
                _handler.WriteRegister(this.regAdd, bytes);
            }

            public void Default(string value)
            {
                this.value = value;
            }

            public string Get()
            {
                return value;
            }
        }

        internal class RegByteArray
        {
            private RFIDReader _handler;
            public UInt16 regAdd;
            public byte[] value;
            public REGPRIVATE Private;

            public RegByteArray(RFIDReader handler, UInt16 add, int len, REGPRIVATE Private)
            {
                this._handler = handler;
                regAdd = add;
                value = new byte[len];
                this.Private = Private;
            }

            public void Set(byte[] value)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (value.Length != this.value.Length)
                    return;

                for (int i = 0; i < this.value.Length; i++)
                {
                    if (value[i] != this.value[i])
                    {
                        value.CopyTo(this.value, 0);
                        _handler.WriteRegister(this.regAdd, this.value);
                        break;
                    }
                }
            }

            public byte[] Get()
            {
                return value;
            }
        }

        internal class RegAntennaPortConfig
        {
            internal class AntennaPortConfig
            {
                public bool enable = false;
                public UInt16 dwell = 2000;
                public UInt16 power = 3000;
                public UInt32 inventoryRoundControl;
                public UInt32 inventoryRoundControl2;
                public bool toggle = true;
                public UInt16 rfMode = 5;
            }

            internal RFIDReader _handler;
            internal UInt16 regAdd;
            internal AntennaPortConfig [] data = new AntennaPortConfig[16];
            internal REGPRIVATE Private;

            internal RegAntennaPortConfig(RFIDReader handler)
            {
                this._handler = handler;
                regAdd = 0x3030;
                //data = new AntennaPortConfig[7];
                this.Private = REGPRIVATE.READWRITE;
                for (int cnt = 0; cnt < 16; cnt++)
                    data[cnt] = new AntennaPortConfig();
                data[0].enable = true;
            }

            internal void Enable(bool enable, byte port = 0)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (data[port].enable == enable)
                    return;

                byte[] sendData = new byte[1];
                int dataAdd = (port * 16);

                if (data[port].enable == enable)
                    return;

                sendData[0] = (byte)(enable ? 1 : 0);
                data[port].enable = enable;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
            }

            internal void Enable(bool enable, int startport, int endport)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                List<UInt16> address = new List<UInt16>();
                List<byte> sendBuffer = new List<byte>();
                byte newValue = (byte)(enable ? 1 : 0);

                for (int i = startport; i < endport; i++)
                {
                    if (data[i].enable == enable)
                        continue;

                    data[i].enable = enable;
                    address.Add((UInt16)(regAdd + (i * 16)));
                    sendBuffer.Add(newValue);
                }

                if (address.Count > 0)
                    _handler.WriteRegister(address.ToArray(), sendBuffer.ToArray());
            }

            internal void SetDwell(UInt16 ms, byte port = 0)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (data[port].dwell == ms)
                    return;

                int dataAdd = 1 + (port * 16);

                data[port].dwell = ms;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), ms);
            }

            internal void SetPower(UInt16 power, byte port = 0)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (data[port].power == power)
                    return;

                int dataAdd = 3 + (port * 16);

                data[port].power = power;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), power);
            }

            internal UInt16 GetPower(byte port = 0)
            {
                if (Private == REGPRIVATE.WRITEONLY)
                    return 0;

                return (UInt16)data[port].power;
            }

            internal void SetTargetToggle(bool enable, byte port = 0)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (data[port].toggle == enable)
                    return;

                byte[] sendData = new byte[1];
                int dataAdd = 13 + (port * 16);

                data[port].toggle = enable;

                sendData[0] = (byte)(enable ? 1 : 0);
                _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
            }

            // if == 0 DynamicQ, != 0 FixedQ 
            internal int GetCurrentAlgorithm(int port = 0)
            {
                if (Private == REGPRIVATE.WRITEONLY)
                    return -1;

                int dataAdd = 5 + (port * 16);

                return (int)(data[port].inventoryRoundControl & (1U << 16));
            }

            internal void EnableFixedQ(int port = 0)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                uint sendData = data[port].inventoryRoundControl | (1U << 16);
                if (data[port].inventoryRoundControl == sendData)
                    return;

                int dataAdd = 5 + (port * 16);
                data[port].inventoryRoundControl = sendData;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
            }

            internal void EnableFixedQ(uint InitialQ, uint QueryTarget, int port = 0)
            {
                if (InitialQ > 15 || QueryTarget > 1)
                    return;

                uint sendData = data[port].inventoryRoundControl & 0xff7ffff0;
                sendData |= InitialQ;
                sendData |= (1U << 16);
                sendData |= QueryTarget;

                if (data[port].inventoryRoundControl == sendData)
                    return;

                int dataAdd = 5 + (port * 16);

                data[port].inventoryRoundControl = sendData;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), data[port].inventoryRoundControl);
            }

            internal void EnableDynamicQ(int port = 0)
            {
                uint sendData = data[port].inventoryRoundControl & ~(1U << 16);

                if (data[port].inventoryRoundControl == sendData)
                    return;

                int dataAdd = 5 + (port * 16);

                data[port].inventoryRoundControl = sendData;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
            }

            /// <summary>
            /// Set Dynamic Q parameters and enabled
            /// </summary>
            /// <param name="MinQ"></param>
            /// <param name="MaxQ"></param>
            /// <param name="InitialQ"></param>
            /// <param name="NumMinQCycles"></param>
            /// <param name="QDecreaseUseQuery"></param>
            /// <param name="QIncreaseUseQuery"></param>
            /// <param name="Session"></param>
            /// <param name="QueryTarget"></param>
            /// <returns></returns>
            internal int EnableDynamicQ(uint MinQ, uint MaxQ, uint InitialQ, uint NumMinQCycles, bool QDecreaseUseQuery, bool QIncreaseUseQuery, uint QueryTarget, int port = 0)
            {
                CSLibrary.Debug.WriteLine("EnableDynamicQ");

                if (MinQ > 15 || MaxQ > 15 || InitialQ > 15 || NumMinQCycles > 153 || QueryTarget > 1)
                    return -1;

                uint sendData = data[port].inventoryRoundControl & 0xff780000;
                sendData |= InitialQ << 0;
                sendData |= MaxQ << 4;
                sendData |= MinQ << 8;
                sendData |= NumMinQCycles << 12;
                sendData |= QIncreaseUseQuery ? (1U << 17) : 0;
                sendData |= QDecreaseUseQuery ? (1U << 18) : 0;
                sendData |= QueryTarget;

                if (data[port].inventoryRoundControl == sendData)
                    return 0;

                int dataAdd = 5 + (port * 16);

                data[port].inventoryRoundControl = sendData;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
                return 0;
            }

            internal void TagGroup(uint session, uint select, uint target, int port = 0)
            {
                if (port == 0xffff)
                {
                    List<UInt16> address = new List<UInt16>();
                    List<uint> sendData = new List<uint>();

                    for (int i = 0; i < 16; i++)
                    {
                        if (!data[i].enable)
                            continue;

                        uint newvalue = (data[i].inventoryRoundControl & 0xff07ffff);
                        newvalue |= (session & 0x03) << 19;
                        newvalue |= (select & 0x03) << 21;
                        newvalue |= (target & 0x01) << 23;

                        if (data[i].inventoryRoundControl == newvalue)
                            continue;

                        data[i].inventoryRoundControl = newvalue;
                        address.Add((UInt16)(regAdd + 5 + (i * 16)));
                        sendData.Add(newvalue);
                    }

                    _handler.WriteRegister(address.ToArray(), sendData.ToArray());
                }
                else
                {
                    uint sendData = data[port].inventoryRoundControl & 0xff07ffff;
                    sendData |= (session & 0x03) << 19;
                    sendData |= (select & 0x03) << 21;
                    sendData |= (target & 0x01) << 23;

                    if (data[port].inventoryRoundControl == sendData)
                        return;

                    int dataAdd = 5 + (port * 16);

                    data[port].inventoryRoundControl = sendData;

                    _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
                }
            }

            internal void Select(uint select, int port = 0)
            {
                if (port == 0xffff)
                {
                    List<UInt16> address = new List<UInt16>();
                    List<uint> sendData = new List<uint>();

                    for (int i = 0; i < 16; i++)
                    {
                        if (!data[i].enable)
                            continue;

                        uint newvalue = data[i].inventoryRoundControl & 0xff9fffff;
                        newvalue |= (select & 0x03) << 21;

                        if (data[i].inventoryRoundControl == newvalue)
                            continue;

                        data[i].inventoryRoundControl = newvalue;
                        address.Add((UInt16)(regAdd + 5 + (i * 16)));
                        sendData.Add(newvalue);
                    }

                    _handler.WriteRegister(address.ToArray(), sendData.ToArray());
                }
                else
                {
                    uint sendData = data[port].inventoryRoundControl & 0xff9fffff;
                    sendData |= (select & 0x03) << 21;

                    if (data[port].inventoryRoundControl == sendData)
                        return;

                    int dataAdd = 5 + (port * 16);

                    data[port].inventoryRoundControl = sendData;

                    _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
                }
            }

            internal void FastIdEnable(bool enable, int port = 0)
            {
                UInt32 newValue = data[port].inventoryRoundControl & 0xfdffffff;

                if (enable)
                    newValue |= 1 << 25;

                if (data[port].inventoryRoundControl == newValue)
                    return;

                int dataAdd = 5 + (port * 16);

                data[port].inventoryRoundControl = newValue;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), newValue);
                return;
            }

            internal bool FastId(int port = 0)
            {
                UInt32 Value = data[port].inventoryRoundControl | ~(0xfdffffff);

                if (Value == 0)
                    return false;

                return true;
            }

            internal void TagFocusEnable(bool enable, int port = 0)
            {
                UInt32 newValue = data[port].inventoryRoundControl & 0xfbffffff;

                if (enable)
                    newValue |= 1 << 26;

                if (data[port].inventoryRoundControl == newValue)
                    return;

                int dataAdd = 5 + (port * 16);

                data[port].inventoryRoundControl = newValue;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), newValue);
                return;
            }

            internal void MaxQSinceValidEpc(UInt32 Q, int port = 0)
            {
                if (data[port].inventoryRoundControl2 == Q)
                    return;

                int dataAdd = 9 + (port * 16);

                data[port].inventoryRoundControl2 = Q;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), data[port].inventoryRoundControl2);
            }

            internal void RfMode(UInt16 mode, int port = 0)
            {
                if (data[port].rfMode == mode)
                    return;

                int dataAdd = 14 + (port * 16);
                data[port].rfMode = mode;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), data[port].rfMode);
            }

            internal void SetInventoryRoundControl(int port, UInt32 value)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (data[port].inventoryRoundControl == value)
                    return;

                int dataAdd = 5 + (port * 16);
                data[port].inventoryRoundControl = value;

                _handler.WriteRegister((UInt16)(regAdd + dataAdd), value);
            }

            internal bool CloneAntenna0Setting()
            {
                List<UInt16> address = new List<UInt16>();
                List<byte[]> sendData = new List<byte[]>();
                byte[] newValue = new byte[11];

                Tools.Hex.MSBArrayCopy(data[0].inventoryRoundControl, newValue, 0);
                Tools.Hex.MSBArrayCopy(data[0].inventoryRoundControl2, newValue, 4);
                newValue[8] = (byte)(data[0].toggle ? 1 : 0);
                Tools.Hex.MSBArrayCopy(data[0].rfMode, newValue, 9);

                for (int i = 1; i < 16; i++)
                {
                    if (!data[i].enable)
                        continue;

                    if (data[i].inventoryRoundControl == data[0].inventoryRoundControl &&
                        data[i].inventoryRoundControl2 == data[0].inventoryRoundControl2 &&
                        data[i].toggle == data[0].toggle &&
                        data[i].rfMode == data[0].rfMode)
                        continue;

                    data[i].inventoryRoundControl = data[0].inventoryRoundControl;
                    data[i].inventoryRoundControl2 = data[0].inventoryRoundControl2;
                    data[i].toggle = data[0].toggle;
                    data[i].rfMode = data[0].rfMode;
                    address.Add((UInt16)(regAdd + 5 + (i * 16)));
                    sendData.Add(newValue);
                }

                _handler.WriteRegister(address.ToArray(), sendData.ToArray());
                return true;
            }
        }

        /*
                internal class RegAntennaPortConfig
                {
                    internal RFIDReader _handler;
                    internal UInt16 regAdd;
                    internal byte[] data;
                    internal REGPRIVATE Private;

                    internal RegAntennaPortConfig(RFIDReader handler)
                    {
                        this._handler = handler;
                        regAdd = 0x3030;
                        data = new byte[256];
                        this.Private = REGPRIVATE.READWRITE;
                    }

                    internal void Enable(bool enable, byte port = 0)
                    {
                        if (Private == REGPRIVATE.READONLY)
                            return;

                        byte[] sendData = new byte[1];
                        int dataAdd = (port * 16);

                        if (enable)
                            data[dataAdd] = 1;
                        else
                            data[dataAdd] = 0;

                        sendData[0] = data[dataAdd];
                        _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
                    }

                    internal void SetDwell(UInt16 ms, byte port = 0)
                    {
                        if (Private == REGPRIVATE.READONLY)
                            return;

                        byte[] sendData = new byte[2];
                        int dataAdd = 1 + (port * 16);

                        data[dataAdd] = (byte)(ms >> 8);
                        data[dataAdd + 1] = (byte)(ms);

                        Array.Copy(data, dataAdd, sendData, 0, 2);
                        _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
                    }

                    internal void SetPower(UInt16 power, byte port = 0)
                    {
                        power *= 10;

                        if (Private == REGPRIVATE.READONLY)
                            return;

                        byte[] sendData = new byte[2];
                        int dataAdd = 3 + (port * 16);

                        data[dataAdd] = (byte)(power >> 8);
                        data[dataAdd + 1] = (byte)(power);

                        Array.Copy(data, dataAdd, sendData, 0, 2);
                        _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
                    }

                    internal UInt16 GetPower(byte port = 0)
                    {
                        if (Private == REGPRIVATE.WRITEONLY)
                            return 0;

                        byte[] sendData = new byte[2];
                        int dataAdd = 3 + (port * 16);
                        int power;

                        power = (data[dataAdd] << 8 | data[dataAdd + 1]);

                        return (UInt16)power;
                    }

                    internal void SetTargetToggle(bool enable, byte port = 0)
                    {
                        if (Private == REGPRIVATE.READONLY)
                            return;

                        byte[] sendData = new byte[1];
                        int dataAdd = 13 + (port * 16);

                        if (enable)
                            data[dataAdd] = 1;
                        else
                            data[dataAdd] = 0;

                        sendData[0] = data[dataAdd];
                        _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
                    }

                    // if == 0 DynamicQ, != 0 FixedQ 
                    internal int GetCurrentAlgorithm(int port = 0)
                    {
                        if (Private == REGPRIVATE.READONLY)
                            return -1;

                        int dataAdd = 5 + (port * 16);

                        return (int)(data[dataAdd] & (1U << 16));
                    }

                    internal void EnableFixedQ(int port = 0)
                    {
                        if (Private == REGPRIVATE.READONLY)
                            return;

                        int dataAdd = 5 + (port * 16);

                        data[dataAdd] = data[dataAdd] | (1U << 16);

                        Set(data);
                    }

                    internal void EnableFixedQ(uint InitialQ, uint QueryTarget)
                    {
                        if (InitialQ > 15 || QueryTarget > 1)
                            return;

                        UInt32 data = _data & 0xff7ffff0;

                        data |= InitialQ;
                        data |= (1U << 16);
                        data |= QueryTarget;

                        Set(data);
                    }

                    internal void EnableDynamicQ()
                    {
                        var data = _data & ~(1U << 16);

                        Set(data);
                    }

                    /// <summary>
                    /// Set Dynamic Q parameters and enabled
                    /// </summary>
                    /// <param name="MinQ"></param>
                    /// <param name="MaxQ"></param>
                    /// <param name="InitialQ"></param>
                    /// <param name="NumMinQCycles"></param>
                    /// <param name="QDecreaseUseQuery"></param>
                    /// <param name="QIncreaseUseQuery"></param>
                    /// <param name="Session"></param>
                    /// <param name="QueryTarget"></param>
                    /// <returns></returns>
                    internal int EnableDynamicQ(uint MinQ, uint MaxQ, uint InitialQ, uint NumMinQCycles, bool QDecreaseUseQuery, bool QIncreaseUseQuery, uint QueryTarget)
                    {
                        if (MinQ > 15 || MaxQ > 15 || InitialQ > 15 || NumMinQCycles > 153 || QueryTarget > 1)
                            return -1;

                        UInt32 data = _data & 0xff780000;

                        data |= InitialQ << 0;
                        data |= MaxQ << 4;
                        data |= MinQ << 8;
                        data |= NumMinQCycles << 12;
                        data |= QIncreaseUseQuery ? (1U << 17) : 0;
                        data |= QDecreaseUseQuery ? (1U << 18) : 0;
                        data |= QueryTarget;

                        Set(data);

                        return 0;
                    }

                    internal void SetFastID(bool enable)
                    {
                        var data = _data;

                        if (enable)
                            data |= (1U << 25);
                        else
                            data &= ~(1U << 25);

                        Set(data);
                    }

                    internal void SetTagFocus(bool enable)
                    {
                        var data = _data;

                        if (enable)
                            data |= (1U << 26);
                        else
                            data &= ~(1U << 26);

                        Set(data);
                    }
                }
        */

        internal class RegSelectConfiguration
        {
            internal RFIDReader _handler;
            internal UInt16 regAdd;
            internal byte[] data;
            internal REGPRIVATE Private;

            internal RegSelectConfiguration(RFIDReader handler)
            {
                this._handler = handler;
                regAdd = 0x3140;
                data = new byte[294];
                this.Private = REGPRIVATE.READWRITE;
            }

            internal void Enable(uint index, bool enable)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (index >= 7)
                    return;

                byte sendData;
                uint dataAdd = (index * 42);

                sendData = (byte)(enable ? 1 : 0);

                if (data[dataAdd] == sendData)
                    return;

                data[dataAdd] = sendData;
                _handler.WriteRegister((UInt16)(regAdd + dataAdd), sendData);
            }

            internal void Enable(int startIndex, int endIndex, bool enable)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (startIndex < 0 || startIndex > endIndex || endIndex >= 7)
                    return;

                List<UInt16> address = new List<UInt16>();
                List<byte> sendbuffer = new List<byte>();
                byte newvalue = (byte)(enable ? 1 : 0);

                for (int i = startIndex; i <= endIndex; i++)
                {
                    uint dataAdd = (uint)(i * 42);

                    if (data[dataAdd] == newvalue)
                        continue;

                    data[dataAdd] = newvalue;
                    address.Add((UInt16)(regAdd + dataAdd));
                    sendbuffer.Add(newvalue);
                }

                if (address.Count > 0)
                    _handler.WriteRegister(address.ToArray(), sendbuffer.ToArray());
            }

            internal bool IsEnable(uint index)
            {
                if (index >= 7)
                    return false;

                uint dataAdd = (index * 42);

                return (data[dataAdd] != 0);
            }

            internal void SetPostConfigurationDelay(uint index, int ms)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (index >= 7 || ms > 255)
                    return;

                byte[] sendData = new byte[1];
                uint add = 41 + (index * 42);

                sendData[0] = (byte)ms;

                if (data[add] == sendData[0])
                    return;

                data[add] = sendData[0];
                _handler.WriteRegister((UInt16)(regAdd + add), sendData);
            }

            internal void Set(int index, bool enable, byte bank, UInt32 offset, byte len, byte[] mask, byte target, byte action, byte delay)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                if (index >= 7)
                    return;

                byte[] sendData = new byte[42];
                int add = (index * 42);
                int maskbytelen = (len + 7) / 8;

                if (mask.Length < maskbytelen)
                {
                    Debug.WriteLine("Set selected mask length error");
                    return;
                }

                if (enable)
                    sendData[0] = 1;
                else
                    sendData[0] = 0;

                sendData[1] = bank;
                sendData[2] = (byte)(offset >> 24);
                sendData[3] = (byte)(offset >> 16);
                sendData[4] = (byte)(offset >> 8);
                sendData[5] = (byte)(offset);
                sendData[6] = len;
                Array.Copy(mask, 0, sendData, 7, maskbytelen);
                sendData[39] = target;
                sendData[40] = action;
                sendData[41] = delay;

                for (int i = 0; i < sendData.Length; i++)
                {
                    if (data[add + i] != sendData[i])
                    {
                        Array.Copy(sendData, 0, data, add, sendData.Length);
                        _handler.WriteRegister((UInt16)(regAdd + add), sendData);
                        break;
                    }
                }
            }
        }

        internal class RegMultibankReadConfig
        {
            internal RFIDReader _handler;
            internal UInt16 regAdd;
            internal byte[] data;
            internal REGPRIVATE Private;

            internal RegMultibankReadConfig(RFIDReader handler)
            {
                this._handler = handler;
                regAdd = 0x3270;
                data = new byte[21];
                this.Private = REGPRIVATE.READWRITE;
            }

            internal void Enable(byte index, bool enable)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                byte[] sendData = new byte[1];
                int add = (index * 7);

                byte newValue = enable ? (byte)1 : (byte)0;

                if (data[add] == newValue)
                    return;

                data[add] = newValue;

                sendData[0] = data[add];
                _handler.WriteRegister((UInt16)(regAdd + add), sendData);
            }

            internal void Set(int index, bool enable, int bank, int address, int len)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                byte[] sendData = new byte[7];
                int add = (index * 7);


                sendData[0] = enable ? (byte)1 : (byte)0;
                sendData[1] = (byte)bank;
                sendData[2] = (byte)(address >> 24);
                sendData[3] = (byte)(address >> 16);
                sendData[4] = (byte)(address >> 8);
                sendData[5] = (byte)(address);
                sendData[6] = (byte)len;


                for (int i = 0; i < sendData.Length; i++)
                {
                    if (data[add + i] != sendData[i])
                    {
                        Array.Copy(sendData, 0, data, add, sendData.Length);
                        _handler.WriteRegister((UInt16)(regAdd + add), sendData);
                        break;
                    }
                }

            }

            internal void SetNoReply(byte index)
            {
                if (Private == REGPRIVATE.READONLY)
                    return;

                byte[] sendData = new byte[1];
                int add = (index * 7);

                byte newValue = 2;

                if (data[add] == newValue)
                    return;

                data[add] = newValue;

                sendData[0] = data[add];
                _handler.WriteRegister((UInt16)(regAdd + add), sendData);
            }
        }
        internal class RegMultibankWriteConfig
        {
            internal RFIDReader _RFIDhandler;
            internal UInt16 _address;
            internal byte[] _data;
            internal REGPRIVATE _private;

            internal RegMultibankWriteConfig(RFIDReader handler)
            {
                this._RFIDhandler = handler;
                _address = 0x3290;
                _data = new byte[1557];
                this._private = REGPRIVATE.READWRITE;
            }

            internal void Enable(byte index, bool enable)
            {
                if (_private == REGPRIVATE.READONLY)
                    return;

                byte[] sendData = new byte[1];
                int add = (index * 519);

                if (enable)
                    _data[add] = 1;
                else
                    _data[add] = 0;

                sendData[0] = _data[add];
                _RFIDhandler.WriteRegister((UInt16)(_address + add), sendData);

            }

            internal void Set(int index, bool enable, byte bank, UInt32 offset, byte len, byte[] data)
            {
                if (_private == REGPRIVATE.READONLY)
                    return;

                byte[] sendData = new byte[7 + data.Length];
                int add = (index * 519);

                if (enable)
                    _data[add] = 1;
                else
                    _data[add] = 0;

                _data[add + 1] = (byte)bank;
                _data[add + 2] = (byte)(offset >> 24);
                _data[add + 3] = (byte)(offset >> 16);
                _data[add + 4] = (byte)(offset >> 8);
                _data[add + 5] = (byte)(offset);
                _data[add + 6] = (byte)len;
                Array.Copy(data, 0, _data, add + 7, data.Length);

                Array.Copy(_data, add, sendData, 0, 7 + data.Length);
                _RFIDhandler.WriteRegister((UInt16)(_address + add), sendData);
            }
        }

        internal class RegInventoryRoundControl
        {
            internal RFIDReader _RFIDhandler;
            internal UInt16 _address;
            internal UInt32 _data;
            internal REGPRIVATE _private;

            internal RegInventoryRoundControl(RFIDReader handler)
            {
                this._RFIDhandler = handler;
                _address = 0x1000;
                _data = 0;
                this._private = REGPRIVATE.READWRITE;
            }

            internal void Set(UInt32 value)
            {
                if (value == _data)
                    return;

                _data = value;
                _RFIDhandler.WriteRegister((UInt16)_address, _data);
            }

            internal UInt32 Get()
            {
                return _data;
            }

            // if == 0 DynamicQ, != 0 FixedQ 
            internal uint GetCurrentAlgorithm()
            {
                return _data & (1U << 16);
            }

            internal void EnableFixedQ()
            {
                var data = _data | (1U << 16);
                Set(data);
            }

            internal void EnableFixedQ(uint InitialQ, uint QueryTarget)
            {
                if (InitialQ > 15 || QueryTarget > 1)
                    return;

                UInt32 data = _data & 0xff7ffff0;

                data |= InitialQ;
                data |= (1U << 16);
                data |= QueryTarget;

                Set(data);
            }

            internal void EnableDynamicQ()
            {
                var data = _data & ~(1U << 16);

                Set(data);
            }

            /// <summary>
            /// Set Dynamic Q parameters and enabled
            /// </summary>
            /// <param name="MinQ"></param>
            /// <param name="MaxQ"></param>
            /// <param name="InitialQ"></param>
            /// <param name="NumMinQCycles"></param>
            /// <param name="QDecreaseUseQuery"></param>
            /// <param name="QIncreaseUseQuery"></param>
            /// <param name="Session"></param>
            /// <param name="QueryTarget"></param>
            /// <returns></returns>
            internal int EnableDynamicQ(uint MinQ, uint MaxQ, uint InitialQ, uint NumMinQCycles, bool QDecreaseUseQuery, bool QIncreaseUseQuery, uint QueryTarget)
            {
                if (MinQ > 15 || MaxQ > 15 || InitialQ > 15 || NumMinQCycles > 153 || QueryTarget > 1)
                    return -1;

                UInt32 data = _data & 0xff780000;

                data |= InitialQ << 0;
                data |= MaxQ << 4;
                data |= MinQ << 8;
                data |= NumMinQCycles << 12;
                data |= QIncreaseUseQuery ? (1U << 17) : 0;
                data |= QDecreaseUseQuery ? (1U << 18) : 0;
                data |= QueryTarget;

                Set(data);

                return 0;
            }

            internal void SetFastID(bool enable)
            {
                var data = _data;

                if (enable)
                    data |= (1U << 25);
                else
                    data &= ~(1U << 25);

                Set(data);
            }

            internal void SetTagFocus(bool enable)
            {
                var data = _data;

                if (enable)
                    data |= (1U << 26);
                else
                    data &= ~(1U << 26);

                Set(data);
            }
        }

        internal class CSLRFIDREGISTER
        {
            internal RFIDReader _handler;

            //internal RegUInt32 CommandResult;
            //internal RegUInt16 ResetCause;
            //internal RegUInt16 Status;
            internal Regstring VersionString;
            internal Regstring BuildNumber;
            //internal RegUInt32 Githash;
            //internal Regstring SerialNumber;
            //internal RegUInt32 FrefFreq;
            //internal RegUInt64 ProductSku;
            //internal RegUInt32 DeviceInfo;
            //internal RegUInt32 DeviceBuild;
            //internal RegUInt32 RtlRevision;
            //internal Regbyte InterruptMask;
            //internal RegUInt32 InterruptMaskSet;
            //internal RegUInt32 InterruptMaskClear;
            //internal Regbyte InterruptStatus;
            //internal RegUInt16 EventFifoNumBytes;
            //internal RegUInt16 EventFifoIntLevel;
            //internal RegUInt32 GpioOutputEnable;
            //internal RegUInt32 GpioOutputLevel;
            //internal Regbyte OpsControl;
            //internal RegUInt32 OpsStatus;
            //internal RegUInt32 HaltedControl;
            //internal RegUInt32 HaltedStatus;
            //internal RegUInt32 LogTestPeriod;
            //internal RegUInt32 LogTestWordRepeat;
            //internal RegUInt32 EventFifoTestPeriod;
            //internal RegUInt32 EventFifoTestPayloadNumWords;
            //internal RegUInt16 LogSpeed;
            //internal RegUInt16 LogEnables;
            //internal RegUInt32 BerControl;
            //internal Regbyte BerMode;
            //internal RegUInt16 AuxAdcControl;
            //internal RegUInt16 AuxAdcResults;
            //internal RegUInt32 RfSynthesizerControl;
            //internal RegUInt32 TxFineGain;
            //internal RegUInt32 RxGainControl;
            //internal RegUInt32 TxCoarseGain;
            //internal RegUInt32 RfMode;
            //internal RegUInt32 DcOffset;
            //internal RegUInt32 CwOffTime;
            //internal Regbyte SjcControl;
            //internal RegUInt32 SjcGainControl;
            //internal RegUInt32 SjcSettlingTime;
            //internal RegUInt32 SjcCdacI;
            //internal RegUInt32 SjcCdacQ;
            //internal RegUInt32 SjcResultI;
            //internal RegUInt32 SjcResultQ;
            //internal RegUInt32 AnalogEnable;
            //internal RegInventoryRoundControl InventoryRoundControl;
            //internal RegUInt32 InventoryRoundControl_2;
            //internal RegUInt16 NominalStopTime;
            //internal RegUInt16 ExtendedStopTime;
            //internal RegUInt16 RegulatoryStopTime;
            //internal RegUInt16 Gen2SelectEnable;
            //internal RegUInt16 Gen2AccessEnable;
            //internal Regbyte Gen2Offsets;
            //internal RegUInt16 Gen2Lengths;
            //internal Regbyte Gen2TransactionIds;
            //internal RegUInt32 Gen2TxnControls;
            //internal RegByteArray Gen2TxBuffer;
            internal RegUInt32 LoopStyle;
            internal Regbyte HopStyle;
            internal RegUInt16 RegulatoryNoEmissionTime;
            internal RegUInt16 CountryEnum;
            internal Regbyte FrequencyChannelIndex;
            internal RegAntennaPortConfig AntennaPortConfig;
            internal RegSelectConfiguration SelectConfiguration;
            internal RegMultibankReadConfig MultibankReadConfig;
            internal RegMultibankWriteConfig MultibankWriteConfig;
            internal RegUInt32 AccessPassword;
            internal RegUInt32 KillPassword;
            internal RegUInt16 LockMask;
            internal RegUInt16 LockAction;
            internal Regbyte DuplicateEliminationRollingWindow;
            internal RegUInt16 TagCacheTableCurrentSize;
            internal Regbyte TagCacheStatus;
            internal RegUInt16 EventPacketUplinkEnable;
            internal Regbyte IntraPacketDelay;
            internal Regbyte RssiFilteringConfig;
            internal RegUInt16 RssiThreshold;
            internal RegUInt24 AuthenticateConfig;
            internal RegByteArray AuthenticateMessage;
            internal RegUInt16 AuthenticateResponseLen;
            internal RegUInt16 ReversePowerADC;
            internal Regbyte CurrentPort;
            internal Regstring ModelName;
            internal Regstring SerialNumber_1;
            internal RegUInt16 CountryEnum_1;
            internal RegUInt32 EF98;
            internal Regstring EF9C;
            internal RegUInt32 EFAC;
            internal RegUInt32 EFB0;

            public CSLRFIDREGISTER(RFIDReader _deviceHandler)
            {

                this._handler = _deviceHandler;

                //CommandResult = new RegUInt32(_deviceHandler, 0x0000, REGPRIVATE.READONLY);
                //ResetCause = new RegUInt16(_deviceHandler, 0x0004, REGPRIVATE.READONLY);
                //Status = new RegUInt16(_deviceHandler, 0x0006, REGPRIVATE.READONLY);
                VersionString = new Regstring(_deviceHandler, 0x0008, 32, REGPRIVATE.READONLY);
                BuildNumber = new Regstring(_deviceHandler, 00028, 4, REGPRIVATE.READONLY);
                //Githash = new RegUInt32(_deviceHandler, 0x002c, REGPRIVATE.READONLY);
                //SerialNumber = new Regstring(_deviceHandler, 0x0070, 32, REGPRIVATE.READONLY);
                //FrefFreq = new RegUInt32(_deviceHandler, 0x0034, REGPRIVATE.READONLY);
                //ProductSku = new RegUInt64(_deviceHandler, 0x0068, REGPRIVATE.READONLY);
                //DeviceInfo = new RegUInt32(_deviceHandler, 0x0090, REGPRIVATE.READONLY);
                //DeviceBuild = new RegUInt32(_deviceHandler, 0x0094, REGPRIVATE.READONLY);
                //RtlRevision = new RegUInt32(_deviceHandler, 0x0098, REGPRIVATE.READONLY);
                //InterruptMask = new Regbyte(_deviceHandler, 0x00a0, REGPRIVATE.READWRITE);
                //InterruptMaskSet = new RegUInt32(_deviceHandler, 0x00a4, REGPRIVATE.WRITEONLY);
                //InterruptMaskClear = new RegUInt32(_deviceHandler, 0x00a8, REGPRIVATE.WRITEONLY);
                //InterruptStatus = new Regbyte(_deviceHandler, 0x00ac, REGPRIVATE.READONLY);
                //EventFifoNumBytes = new RegUInt16(_deviceHandler, 0x00b0, REGPRIVATE.READONLY);
                //EventFifoIntLevel = new RegUInt16(_deviceHandler, 0x00b2, REGPRIVATE.READWRITE);
                //GpioOutputEnable = new RegUInt32(_deviceHandler, 0x00b4, REGPRIVATE.READWRITE);
                //GpioOutputLevel = new RegUInt32(_deviceHandler, 0x00b8, REGPRIVATE.READWRITE);
                //OpsControl = new Regbyte(_deviceHandler, 0x0300, REGPRIVATE.READWRITE);
                //OpsStatus = new RegUInt32(_deviceHandler, 0x0304, REGPRIVATE.READONLY);
                //HaltedControl = new RegUInt32(_deviceHandler, 0x0308, REGPRIVATE.READWRITE);
                //HaltedStatus = new RegUInt32(_deviceHandler, 0x030c, REGPRIVATE.READONLY);
                //LogTestPeriod = new RegUInt32(_deviceHandler, 0x0320, REGPRIVATE.READWRITE);
                //LogTestWordRepeat = new RegUInt32(_deviceHandler, 0x0324, REGPRIVATE.READWRITE);
                //EventFifoTestPeriod = new RegUInt32(_deviceHandler, 0x0328, REGPRIVATE.READWRITE);
                //EventFifoTestPayloadNumWords = new RegUInt32(_deviceHandler, 0x032c, REGPRIVATE.READWRITE);
                //LogSpeed = new RegUInt16(_deviceHandler, 0x0330, REGPRIVATE.READWRITE);
                //LogEnables = new RegUInt16(_deviceHandler, 0x0334, REGPRIVATE.READWRITE);
                //BerControl = new RegUInt32(_deviceHandler, 0x0338, REGPRIVATE.READWRITE);
                //BerMode = new Regbyte(_deviceHandler, 0x033c, REGPRIVATE.READWRITE);
                //AuxAdcControl = new RegUInt16(_deviceHandler, 0x0400, REGPRIVATE.READWRITE);
                //AuxAdcResults = new RegUInt16(_deviceHandler, 0x0404, REGPRIVATE.READONLY);
                //RfSynthesizerControl = new RegUInt32(_deviceHandler, 0x0500, REGPRIVATE.READWRITE);
                //TxFineGain = new RegUInt32(_deviceHandler, 0x504, REGPRIVATE.READWRITE);
                //RxGainControl = new RegUInt32(_deviceHandler, 0x0508, REGPRIVATE.READWRITE);
                //TxCoarseGain = new RegUInt32(_deviceHandler, 0x050c, REGPRIVATE.READWRITE);
                //RfMode = new RegUInt32(_deviceHandler, 0x0514, REGPRIVATE.READWRITE);
                //DcOffset = new RegUInt32(_deviceHandler, 0x0518, REGPRIVATE.READWRITE);
                //CwOffTime = new RegUInt32(_deviceHandler, 0x051c, REGPRIVATE.READWRITE);
                //SjcControl = new Regbyte(_deviceHandler, 0x0600, REGPRIVATE.READWRITE);
                //SjcGainControl = new RegUInt32(_deviceHandler, 0x0604, REGPRIVATE.READWRITE);
                //SjcSettlingTime = new RegUInt32(_deviceHandler, 0x0608, REGPRIVATE.READWRITE);
                //SjcCdacI = new RegUInt32(_deviceHandler, 0x060c, REGPRIVATE.READWRITE);
                //SjcCdacQ = new RegUInt32(_deviceHandler, 0x0610, REGPRIVATE.READWRITE);
                //SjcResultI = new RegUInt32(_deviceHandler, 0x0614, REGPRIVATE.READONLY);
                //SjcResultQ = new RegUInt32(_deviceHandler, 0x0618, REGPRIVATE.READONLY);
                //AnalogEnable = new RegUInt32(_deviceHandler, 0x0700, REGPRIVATE.READWRITE);
                //InventoryRoundControl = new RegInventoryRoundControl(_deviceHandler); // for E710 only
                //InventoryRoundControl_2 = new RegUInt32(_deviceHandler, 0x1004, REGPRIVATE.READWRITE); // for E710 only
                //NominalStopTime = new RegUInt16(_deviceHandler, 0x1008, REGPRIVATE.READWRITE);
                //ExtendedStopTime = new RegUInt16(_deviceHandler, 0x100c, REGPRIVATE.READWRITE);
                //RegulatoryStopTime = new RegUInt16(_deviceHandler, 0x1010, REGPRIVATE.READWRITE);
                //Gen2SelectEnable = new RegUInt16(_deviceHandler, 0x1014, REGPRIVATE.READWRITE);
                //Gen2AccessEnable = new RegUInt16(_deviceHandler, 0x1018, REGPRIVATE.READWRITE);
                //Gen2Offsets = new Regbyte(_deviceHandler, 0x1020, REGPRIVATE.READWRITE);
                //Gen2Lengths = new RegUInt16(_deviceHandler, 0x1030, REGPRIVATE.READWRITE);
                //Gen2TransactionIds = new Regbyte(_deviceHandler, 0x1050, REGPRIVATE.READWRITE);
                //Gen2TxnControls = new RegUInt32(_deviceHandler, 0x1060, REGPRIVATE.READWRITE);
                //Gen2TxBuffer = new RegByteArray(_deviceHandler, 0x1100, 128, REGPRIVATE.READWRITE);
                LoopStyle = new RegUInt32(_deviceHandler, 0x3000, REGPRIVATE.READWRITE);
                HopStyle = new Regbyte(_deviceHandler, 0x3008, REGPRIVATE.READWRITE);
                RegulatoryNoEmissionTime = new RegUInt16(_deviceHandler, 0x3010, REGPRIVATE.READWRITE);
                CountryEnum = new RegUInt16(_deviceHandler, 0x3014, REGPRIVATE.READWRITE);
                FrequencyChannelIndex = new Regbyte(_deviceHandler, 0x3018, REGPRIVATE.READWRITE);
                AntennaPortConfig = new RegAntennaPortConfig(_deviceHandler);
                SelectConfiguration = new RegSelectConfiguration(_deviceHandler);
                MultibankReadConfig = new RegMultibankReadConfig(_deviceHandler);
                MultibankWriteConfig = new RegMultibankWriteConfig(_deviceHandler);
                AccessPassword = new RegUInt32(_deviceHandler, 0x38a6, REGPRIVATE.READWRITE);
                KillPassword = new RegUInt32(_deviceHandler, 0x38aa, REGPRIVATE.READWRITE);
                LockMask = new RegUInt16(_deviceHandler, 0x38ae, REGPRIVATE.READWRITE);
                LockAction = new RegUInt16(_deviceHandler, 0x38b0, REGPRIVATE.READWRITE);
                DuplicateEliminationRollingWindow = new Regbyte(_deviceHandler, 0x3900, REGPRIVATE.READWRITE);
                TagCacheTableCurrentSize = new RegUInt16(_deviceHandler, 0x3902, REGPRIVATE.READWRITE);
                TagCacheStatus = new Regbyte(_deviceHandler, 0x3904, REGPRIVATE.READWRITE);
                EventPacketUplinkEnable = new RegUInt16(_deviceHandler, 0x3906, REGPRIVATE.READWRITE);
                IntraPacketDelay = new Regbyte(_deviceHandler, 0x3908, REGPRIVATE.READWRITE);
                RssiFilteringConfig = new Regbyte(_deviceHandler, 0x390a, REGPRIVATE.READWRITE);
                RssiThreshold = new RegUInt16(_deviceHandler, 0x390c, REGPRIVATE.READWRITE);
                AuthenticateConfig = new RegUInt24(_deviceHandler, 0x390e, REGPRIVATE.READWRITE);
                AuthenticateMessage = new RegByteArray(_deviceHandler, 0x3912, 32, REGPRIVATE.READWRITE);
                AuthenticateResponseLen = new RegUInt16(_deviceHandler, 0x3944, REGPRIVATE.READWRITE);
                ReversePowerADC = new RegUInt16(_deviceHandler, 0x3946, REGPRIVATE.READWRITE);
                CurrentPort = new Regbyte(_deviceHandler, 0x3948, REGPRIVATE.READWRITE);
                ModelName = new Regstring(_deviceHandler, 0x5000, 32, REGPRIVATE.READONLY);
                SerialNumber_1 = new Regstring(_deviceHandler, 0x5020, 32, REGPRIVATE.READONLY);
                CountryEnum_1 = new RegUInt16(_deviceHandler, 0x5040, REGPRIVATE.READONLY);
                EF98 = new RegUInt32(_deviceHandler, 0xef98, REGPRIVATE.READONLY);
                EF9C = new Regstring(_deviceHandler, 0xef9c, 16, REGPRIVATE.READONLY);
                EFAC = new RegUInt32(_deviceHandler, 0xefac, REGPRIVATE.READONLY);
                EFB0 = new RegUInt32(_deviceHandler, 0xefb0, REGPRIVATE.READONLY);
            }
        }

        CSLRFIDREGISTER RFIDRegister;
        /*
                VersionString 8, 32
                BuildNumber 28, 4
                Githash 2c, 4
                FrefFreq 34, 4
                ProductSku 68, 8
                SerialNumber 70, 32
                DeviceInfo 90, 4
                DeviceBuild 94, 4
                RtlRevision 98, 4
                Model Name 5000, 32
                Serial Number 5020, 32
                Country Enum 5040, 2
        */
        /*
                int[,] ReaderOEMDDataAddress = new int[,] { { 0x0008, 32 },
                                                            { 0x0028, 4 },
        //                                                    { 0x002c, 4 },
        //                                                    { 0x0034, 4 },
        //                                                    { 0x0068, 8 },
        //                                                    { 0x0070, 32 },
        //                                                    { 0x0090, 4 },
        //                                                    { 0x0094, 4 },
        //                                                    { 0x0098, 4 },
                                                            { 0x5000, 32 },
                                                            { 0x5020, 32 },
                                                            { 0x5040, 2 },
        //                                                    { 0xef98, 4 },
        //                                                    { 0xefac, 4 },
        //                                                    { 0xefb0, 4 },
                                                            { 0x3014, 2 },
                                                            { 0x3018, 1 }
        */
        int[,] ReaderOEMDDataAddress = new int[,] { { 0x0008, 32 },
                                                    { 0x0028, 4 },
                                                    { 0x3014, 2 },
                                                    { 0x3018, 1 },
                                                    { 0x5000, 32 },
                                                    { 0x5040, 2 },
                                                    { 0xef98, 4 },
                                                    { 0xef9c, 16 },
                                                    { 0xefac, 4 },
                                                    { 0xefb0, 4 }
    };

        public bool RegisterInitialize_CS710S()
        {
            RFIDRegister = new CSLRFIDREGISTER(this);

            READREGISTERSET[] OEMDataAddress = new READREGISTERSET[ReaderOEMDDataAddress.GetLength(0)]; 

            for (int cnt = 0; cnt < ReaderOEMDDataAddress.GetLength(0); cnt++)
                OEMDataAddress[cnt] = new READREGISTERSET(ReaderOEMDDataAddress[cnt, 0], ReaderOEMDDataAddress[cnt, 1]);

            ReadRegister(OEMDataAddress);
            return true;
		}

        internal class READREGISTERSET
        {
            public int address;
            public int size;

            public READREGISTERSET (int address, int size)
            {
                this.address = address;
                this.size = size;
            }
            public READREGISTERSET(Regbyte reg)
            {
                this.address = reg.regAdd;
                this.size = 1;
            }
            public READREGISTERSET(RegUInt16 reg)
            {
                this.address = reg.regAdd;
                this.size = 2;
            }
            public READREGISTERSET(RegUInt32 reg)
            {
                this.address = reg.regAdd;
                this.size = 4;
            }
            public READREGISTERSET(RegUInt64 reg)
            {
                this.address = reg.regAdd;
                this.size = 8;
            }
            public READREGISTERSET(Regstring reg)
            {
                this.address = reg.regAdd;
                this.size = reg.maxlen;
            }
            public READREGISTERSET(RegByteArray reg)
            {
                this.address = reg.regAdd;
                this.size = reg.value.Length;
            }
        }

        byte[] RfidCmdpack(SCSLRFIDCMD cmd, byte[] payload)
		{
            byte[] datapacket = new byte[7 + payload.Length];

            datapacket[0] = 0x80;
            datapacket[1] = 0xb3;
            datapacket[2] = (byte)((int)cmd >> 8);
            datapacket[3] = (byte)cmd;
            datapacket[4] = 0x00;
            datapacket[5] = (byte)(payload.Length >> 8);
            datapacket[6] = (byte)payload.Length;

            if (payload.Length > 0)
                Array.Copy(payload, 0, datapacket, 7, payload.Length);

            return datapacket;
        }

        void SaveInitRegisters(int index, byte[]data, int size)
        {
            /*
                        RFIDRegister.VersionString.Set(System.Text.Encoding.Default.GetString(data, index, RFIDRegister.VersionString.maxlen));
                        RFIDRegister.BuildNumber.Set(System.Text.Encoding.Default.GetString(data, index + 32, RFIDRegister.BuildNumber.maxlen));
                        RFIDRegister.Githash.Set(Tools.Hex.MSBToUInt32(data, index + 36));
                        RFIDRegister.FrefFreq.Set(Tools.Hex.MSBToUInt32(data, index + 40));
                        RFIDRegister.ProductSku.Set(Tools.Hex.MSBToUInt64(data, index + 44));
                        RFIDRegister.SerialNumber.Set(System.Text.Encoding.Default.GetString(data, index + 52, RFIDRegister.BuildNumber.maxlen));
                        RFIDRegister.DeviceInfo.Set(Tools.Hex.MSBToUInt32(data, index + 84));
                        RFIDRegister.DeviceBuild.Set(Tools.Hex.MSBToUInt32(data, index + 88));
                        RFIDRegister.RtlRevision.Set(Tools.Hex.MSBToUInt32(data, index + 92));
                        RFIDRegister.ModelName.Set(System.Text.Encoding.Default.GetString(data, index + 96, RFIDRegister.ModelName.maxlen));
                        RFIDRegister.SerialNumber_1.Set(System.Text.Encoding.Default.GetString(data, index + 128, RFIDRegister.SerialNumber_1.maxlen));
                        RFIDRegister.CountryEnum_1.Set(Tools.Hex.MSBToUInt16(data, index + 160));
                        RFIDRegister.EF98.Set(Tools.Hex.MSBToUInt32(data, index + 162));
                        RFIDRegister.EFAC.Set(Tools.Hex.MSBToUInt32(data, index + 166));
                        RFIDRegister.EFB0.Set(Tools.Hex.MSBToUInt32(data, index + 170));
                        RFIDRegister.CountryEnum.Set(Tools.Hex.MSBToUInt16(data, index + 174));
                        RFIDRegister.FrequencyChannelIndex.Set(data[index + 176]);
            */

            RFIDRegister.VersionString.Set(System.Text.Encoding.Default.GetString(data, index, RFIDRegister.VersionString.maxlen));
            RFIDRegister.BuildNumber.Set(System.Text.Encoding.Default.GetString(data, index + 32, RFIDRegister.BuildNumber.maxlen));
            RFIDRegister.CountryEnum.Set(Tools.Hex.MSBToUInt16(data, index + 36));
            RFIDRegister.FrequencyChannelIndex.Set(data[index + 38]);
            RFIDRegister.ModelName.Set(System.Text.Encoding.Default.GetString(data, index + 39, RFIDRegister.ModelName.maxlen));
            RFIDRegister.CountryEnum_1.Set(Tools.Hex.MSBToUInt16(data, index + 71));
            RFIDRegister.EF98.Set(Tools.Hex.MSBToUInt32(data, index + 73));
            RFIDRegister.EF9C.Set(System.Text.Encoding.Default.GetString(data, index + 77, RFIDRegister.EF9C.maxlen));
            RFIDRegister.EFAC.Set(Tools.Hex.MSBToUInt32(data, index + 93));
            RFIDRegister.EFB0.Set(Tools.Hex.MSBToUInt32(data, index + 97));

            m_oem_machine = MODEL.CS710S;
            m_oem_country_code = RFIDRegister.EF98.Get();
            m_oem_special_country_version = RFIDRegister.EFAC.Get();
            m_oem_freq_modification_flag = (uint)RFIDRegister.EFB0.Get();
            
            switch (m_oem_country_code)
            {
                case 0x01:
                    m_save_region_code = RegionCode.ETSI;
                    break;

                case 0x02:
                    m_save_region_code = RegionCode.FCC;
                    break;
            }
        }


        /// <summary>
        /// return : data length
        /// </summary>
        /// <param name="add"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <returns></returns>
/*
        int SaveRegister(int add, byte[] data, int index)
        {
            switch (add)
            {
                case 0x0008:
                    RFIDRegister.VersionString.value = System.Text.Encoding.Default.GetString(data, index, 32);
                    return 32;

                case 0x0028:
                    RFIDRegister.BuildNumber.value = System.Text.Encoding.Default.GetString(data, index, 32);
                    return 32;

                case 0x002c:
                    RFIDRegister.Githash.value = BitConverter.ToUInt32(data, index);
                    return 1;

                case 0x5000:
                    RFIDRegister.ModelName.value = System.Text.Encoding.Default.GetString(data, index, 32);
                    return 32;

                case 0x5020:
                    RFIDRegister.SerialNumber_1.value = System.Text.Encoding.Default.GetString(data, index, 32);
                    return 32;

                case 0x5040:
                    RFIDRegister.CountryEnum_1.value = BitConverter.ToUInt16(data, index);
                    return 2;
            }

            return 0;
        }
*/
    }
}
