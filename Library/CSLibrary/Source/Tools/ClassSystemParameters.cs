using System;

namespace CSLibrary
{
    internal class SYSTEMPARAMETERS
    {
        // for connection
        public RFIDDEVICE.CONNECTIONMODE _ConnectionMode = RFIDDEVICE.CONNECTIONMODE.UNKNOWN;
        public string _deviceName;

        /* OEM Data
        PCB Manufacturing Date Code
        Country Code
        PCB Serial No.
        Special Country Version
        Frequency Modification Flag
        Model Code
        Max Target Power
        Enable/Disable Frequency Sequence
        */
        public UInt32 _PCBDateCode;
        public UInt32 _CountryCode;
        public byte[] _PCBSerialNo = new byte[16];
        public UInt32 _SpecialCountryVersion;
        public UInt32 _FrequencyModificationFlag;
        public UInt32 _ModelCode;
        public UInt32 _MaxTargetPower;
        public UInt32 _EnableFrequencySequence;

        public RFIDDEVICE.CONNECTIONMODE getConnectionMode()
        {
            return _ConnectionMode;
        }

        public string getDeviceName()
        {
            return _deviceName;
        }

        public DateTime getPCBDateCode()
        {
            Int32 datecode = (Int32)(_PCBDateCode);
            int year = (datecode >> 16) & 0xffff;
            int month = (datecode >> 8) & 0xff;
            int day = datecode & 0xff;
            return new DateTime(year, month, day);
        }

        public int getCountryCode()
        {
            return (int)_CountryCode;
        }

        public String getPCBSerialNo()
        {
            return System.Text.Encoding.ASCII.GetString(_PCBSerialNo).Trim('\0');
        }

        public UInt32 getSpecialCountryVersion()
        {
            return _SpecialCountryVersion;
        }

        public bool getFrequencyModificationFlag()
        {
            return (_FrequencyModificationFlag != 0);
        }

        public RFIDDEVICE.MODEL getModelCode()
        {
            return (RFIDDEVICE.MODEL)(_ModelCode);
        }

        public int getMaxTargetPower()
        {
            return (int)(_MaxTargetPower);
        }

        public bool getEnableFrequencySequence()
        {
            return (_EnableFrequencySequence != 0);
        }
    }
}
