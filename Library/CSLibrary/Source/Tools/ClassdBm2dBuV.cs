using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.Tools
{
    public class dBConverion
    {
        // dBμV=dBm+90+20log(Z0−−√z0), z0 = 50
        // Tag RSSI(dBm) min -90 max -30, RSSI(dBuV) min 17 max 77

        public static float dBuV2dBm(float dBuV, int rounddec = -1)
        {
            return (float)dBuV2dBm((double)dBuV, rounddec);
        }

        public static float dBm2dBuV(float dBm, int rounddec = -1)
        {
            return (float)dBm2dBuV((double)dBm, rounddec);
        }

        public static double dBuV2dBm(double dBuV, int rounddec = -1)
        {
            double value = dBuV - 106.9897;
            if (rounddec < 0)
                return value;

            return Math.Round(value, rounddec);
        }

        public static double dBm2dBuV(double dBm, int rounddec = -1)
        {
            double value = dBm + 106.9897;
            if (rounddec < 0)
                return value;

            return Math.Round(value, rounddec);
        }
    }
}
