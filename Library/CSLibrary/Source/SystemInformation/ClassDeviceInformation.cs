using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    using Constants;

    public static class RFIDDEVICE
    {
        /// <summary>
        /// CSL RFID Reader model (OEM model code)
        /// </summary>
        public enum MODEL
        {
            CS101 = 0,
            CS203 = 1,
            CS333 = 2,
            CS468 = 3,
            CS468INT = 5,
            CS463 = 6,
            CS469 = 7,
            CS208 = 8,
            CS209 = 9,
            CS103 = 10,
            CS108 = 11,
            CS206 = 12,
            CS468X = 13,
            CS203X = 14,
            CS468XJ = 15,
            CS710S = 16,
            UNKNOWN = 0xff
        }

        public class HARDWARECONFIGURATION
        {
            public MODEL model;
            public int totalAntenna;
            internal int firstAntenna;

            public HARDWARECONFIGURATION (MODEL model, int totalAntenna, int firstAntenna)
            {
                this.model = model;
                this.totalAntenna = totalAntenna;
                this.firstAntenna = firstAntenna;
            }
        }

        public static List<HARDWARECONFIGURATION> info;

        static RFIDDEVICE()
        {
            info = new List<HARDWARECONFIGURATION>();

            info.Add(new HARDWARECONFIGURATION(MODEL.CS108, 1, 0));
            info.Add(new HARDWARECONFIGURATION(MODEL.CS203, 2, 3));
            info.Add(new HARDWARECONFIGURATION(MODEL.CS463, 4, 0));
            info.Add(new HARDWARECONFIGURATION(MODEL.CS710S, 1, 0));
        }

        public static int GetTotalAntenna(MODEL model)
        {
            return ((info.Find(item => item.model == model)).totalAntenna);
        }

        public static int GetfirstAntenna(MODEL model)
        {
            return ((info.Find(item => item.model == model)).firstAntenna);
        }
    }
}
