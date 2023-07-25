using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client
{
    public static class ClassBattery
    {
        public enum BATTERYMODE
        {
            INVENTORY = 1,
            IDLE = 2,
        }

        public enum BATTERYLEVELSTATUS
        {
            NORMAL = 0,
            LOW = 1,
        }



        // for inventory mode
        readonly static double[] voltageTable1 = new double[] { 4.106, 4.017, 3.98, 3.937, 3.895, 3.853, 3.816, 3.779, 3.742, 3.711, 3.679, 3.658, 3.637, 3.626, 3.61, 3.584, 3.547, 3.515, 3.484, 3.457, 3.431, 3.399, 3.362, 3.32, 3.251, 3.135 };
        readonly static double[] capacityTable1 = new double[] {  100,    96,   92,    88,    84,    80,    76,    72,    67,    63,    59,    55,    51,    47,   43,    39,    35,    31,    27,    23,    19,    15,    11,    7,     2,     0 };
        //readonly static double[] voltageTable1 = new double[] { 3.921, 3.890, 3.863, 3.826, 3.795, 3.768, 3.742, 3.721, 3.700, 3.679, 3.652, 3.642, 3.621, 3.605, 3.589, 3.573, 3.563, 3.557, 3.552, 3.536, 3.526, 3.520, 3.499, 3.478, 3.457, 3.415, 3.241, 2.612 };
        //readonly static double[] capacityTable1 = new double[] { 100, 99, 98, 97, 96, 94, 92, 89, 85, 80, 75, 70, 65, 60, 55, 50, 45, 40, 35, 30, 24, 20, 16, 13, 9, 6, 2, 0 };
        readonly static double[] voltageSlope1 = new double[voltageTable1.Length - 1];

        // for non-inventory mode
        readonly static double[] voltageTable2 = new double[] { 4.212, 4.175, 4.154, 4.133, 4.112, 4.085, 4.069, 4.054, 4.032, 4.011, 3.99, 3.969, 3.953, 3.937, 3.922, 3.901, 3.885, 3.869, 3.853, 3.837, 3.821, 3.806, 3.79, 3.774, 3.769, 3.763, 3.758, 3.753, 3.747, 3.742, 3.732, 3.721, 3.705, 3.684, 3.668, 3.652, 3.642, 3.626, 3.615, 3.605, 3.594, 3.584, 3.568, 3.557, 3.542, 3.531, 3.510, 3.494, 3.473, 3.457, 3.436, 3.41, 3.362, 3.235, 2.987, 2.982 };
        readonly static double[] capacityTable2 = new double[] {  100,    98,    96,    95,    93,    91,    89,    87,    85,    84,   82,    80,    78,    76,    75,    73,    71,    69,    67,    65,    64,    62,   60,    58,    56,    55,    53,    51,    49,    47,    45,    44,    42,    40,    38,    36,    35,    33,    31,    29,    27,    25,    24,    22,    20,    18,    16,    15,    13,    11,     9,    7,     5,     4,     2,     0 };
        //readonly static double[] voltageTable2 = new double[] { 4.048, 4.032, 4.011, 3.995, 3.974, 3.964, 3.948, 3.932, 3.911, 3.895, 3.879, 3.863, 3.853, 3.842, 3.826, 3.811, 3.800, 3.784, 3.774, 3.758, 3.747, 3.737, 3.726, 3.721, 3.710, 3.705, 3.695, 3.689, 3.684, 3.679, 3.673, 3.668, 3.663, 3.658, 3.658, 3.652, 3.647, 3.642, 3.636, 3.631, 3.626, 3.615, 3.605, 3.594, 3.578, 3.573, 3.563, 3.552, 3.504, 3.394, 3.124, 2.517 };
        //readonly static double[] capacityTable2 = new double[] { 100, 99, 98, 97, 95, 93, 90, 87, 84, 81, 78, 75, 73, 71, 69, 68, 66, 64, 62, 60, 58, 56, 54, 52, 50, 48, 47, 45, 43, 41, 39, 37, 35, 33, 31, 29, 27, 26, 24, 22, 20, 18, 16, 14, 12, 10, 8, 6, 5, 3, 1, 0 };
        readonly static double[] voltageSlope2 = new double[voltageTable2.Length - 1];

        static double[] voltageTable;
        static double[] capacityTable;
        static double[] voltageSlope;

        static BATTERYMODE _currentInventoryMode;

        static ClassBattery()
        {
            int cnt;

            for (cnt = 0; cnt < voltageTable1.Length - 2; cnt++)
                voltageSlope1[cnt] = (capacityTable1[cnt] - capacityTable1[cnt + 1]) / (voltageTable1[cnt] - voltageTable1[cnt + 1]);

            for (cnt = 0; cnt < voltageTable2.Length - 2; cnt++)
                voltageSlope2[cnt] = (capacityTable2[cnt] - capacityTable2[cnt + 1]) / (voltageTable2[cnt] - voltageTable2[cnt + 1]);

            SetBatteryMode(BATTERYMODE.IDLE);
        }

        public static void SetBatteryMode(BATTERYMODE bm)
        {
            _currentInventoryMode = bm;

            if (bm == BATTERYMODE.INVENTORY)
            {
                voltageTable = voltageTable1;
                capacityTable = capacityTable1;
                voltageSlope = voltageSlope1;
            }
            else
            {
                voltageTable = voltageTable2;
                capacityTable = capacityTable2;
                voltageSlope = voltageSlope2;
            }
        }

        public static BATTERYLEVELSTATUS BatteryLow(double voltage)
        {
            if (Voltage2Percent(voltage) <= 20.0)
                return BATTERYLEVELSTATUS.LOW;

#if old
            if (_currentInventoryMode == BATTERYMODE.INVENTORY)
            {
                if (voltage <= 3.515)
                {
                    return BATTERYLEVELSTATUS.LOW;
                }
            }
            else
            {
                if (voltage <= 3.652)
                {
                    return BATTERYLEVELSTATUS.LOW;
                }
            }
#endif

            return BATTERYLEVELSTATUS.NORMAL;
        }

        public static double Voltage2Percent(double voltage)
        {
            int cnt;

            if (voltage > voltageTable[0])
                return 100;

            if (voltage <= voltageTable[voltageTable.Length - 1])
                return 0;

            for (cnt = voltageTable.Length - 2; cnt >= 0; cnt--)
            {
                if (voltage > voltageTable[cnt])
                    continue;

                if (voltage == voltageTable[cnt])
                    return capacityTable[cnt];

                double percent = 0;

                percent = (voltage - voltageTable[cnt + 1]) * voltageSlope[cnt] + capacityTable[cnt + 1];

                return percent;
            }

            return 0;
        }
    }






    /* for old formula    

    #if oldMode
        14% Battery Life Left, Please Recharge RFID Reader or Replace with Freshly Charged Battery
    #else


        public static class ClassBattery
        {
            public enum BATTERYMODE
            {
                INVENTORY = 1,
                IDLE = 2,
            }

            public enum BATTERYLEVELSTATUS
            {
                NORMAL = 0,
                LOW = 1,
                LOW_17 = 2
            }

            // battery table for PCB version below of 1.7
            readonly static double voltageFirstOffset0 = 100.0 / 90 * 5;
            readonly static double[] voltageTable0 = new double[] { 3.4, 3.457, 3.468, 3.489, 3.494, 3.515, 3.541, 3.566, 3.578, 3.610, 3.615, 3.668, 3.7, 3.731, 3.753, 3.790, 3.842, 3.879, 4.0 };
            readonly static double[] voltageSlope0 = new double[voltageTable0.Length - 1];
            readonly static double voltagestep0 = (100.0 - voltageFirstOffset0) / (voltageTable0.Length - 2);

            // battery table for PCB version of 1.7 or above and inventory mode
            readonly static double voltageFirstOffset1 = 100.0 / 134 * 4;
            readonly static double[] voltageTable1 = new double[] { 2.789, 3.304, 3.452, 3.489, 3.515, 3.534, 3.554, 3.563, 3.578, 3.584, 3.594, 3.61, 3.625, 3.652, 3.652, 3.673, 3.7, 3.725, 3.747, 3.769, 3.8, 3.826, 3.858, 3.89, 3.972, 3.964, 4.001, 4.069 };
            readonly static double[] voltageSlope1 = new double[voltageTable1.Length - 1];
            readonly static double voltagestep1 = (100.0 - voltageFirstOffset1) / (voltageTable1.Length - 2);

            // battery table for PCB version of 1.7 or above and idle mode
            readonly static double voltageFirstOffset2 = 100.0 / 534 * 4;
            readonly static double[] voltageTable2 = new double[] { 2.322, 3.156, 3.452, 3.563, 3.605, 3.626, 3.631, 3.642, 3.652, 3.668, 3.679, 3.689, 3.700, 3.705, 3.710, 3.716, 3.721, 3.724, 3.726, 3.731, 3.737, 3.742, 3.747, 3.753, 3.758, 3.763, 3.774, 3.779, 3.784, 3.798, 3.805, 3.816, 3.826, 3.842, 3.853, 3.863, 3.879, 3.895, 3.906, 3.921, 3.937, 3.948, 3.964, 3.980, 4.001, 4.018, 4.032, 4.048, 4.064, 4.085, 4.097, 4.117, 4.138, 4.185, 4.190 };
            readonly static double[] voltageSlope2 = new double[voltageTable2.Length - 1];
            readonly static double voltagestep2 = (100.0 - voltageFirstOffset2) / (voltageTable2.Length - 2);

            static double voltageFirstOffset;
            static double[] voltageTable;
            static double[] voltageSlope;
            static double voltagestep;

            static BATTERYMODE _currentInventoryMode;

            static ClassBattery()
            {
                int cnt;

                for (cnt = 0; cnt < voltageTable0.Length - 1; cnt++)
                    voltageSlope0[cnt] = voltagestep0 / (voltageTable0[cnt + 1] - voltageTable0[cnt]);

                for (cnt = 0; cnt < voltageTable1.Length - 1; cnt++)
                    voltageSlope1[cnt] = voltagestep1 / (voltageTable1[cnt + 1] - voltageTable1[cnt]);

                for (cnt = 0; cnt < voltageTable2.Length - 1; cnt++)
                    voltageSlope2[cnt] = voltagestep2 / (voltageTable2[cnt + 1] - voltageTable2[cnt]);

                SetBatteryMode(BATTERYMODE.IDLE);
            }

            public static void SetBatteryMode(BATTERYMODE bm)
            {
                _currentInventoryMode = bm;

                if (string.Compare (BleMvxApplication._reader.siliconlabIC.GetPCBVersion(), "180") < 0 )
                {
                    voltageFirstOffset = voltageFirstOffset0;
                    voltageTable = voltageTable0;
                    voltageSlope = voltageSlope0;
                    voltagestep = voltagestep0;
                }
                else
                {
                    if (bm == BATTERYMODE.INVENTORY)
                    {
                        voltageFirstOffset = voltageFirstOffset1;
                        voltageTable = voltageTable1;
                        voltageSlope = voltageSlope1;
                        voltagestep = voltagestep1;
                    }
                    else
                    {
                        voltageFirstOffset = voltageFirstOffset2;
                        voltageTable = voltageTable2;
                        voltageSlope = voltageSlope2;
                        voltagestep = voltagestep2;
                    }
                }
            }

            public static BATTERYLEVELSTATUS BatteryLow (double voltage)
            {
                return BATTERYLEVELSTATUS.NORMAL;

                if (string.Compare(BleMvxApplication._reader.siliconlabIC.GetPCBVersion(), "180") < 0)
                {
                    if (_currentInventoryMode == BATTERYMODE.INVENTORY)
                    {
                        if (voltage <= 3.45)
                        {
                            return BATTERYLEVELSTATUS.LOW_17;
                        }
                    }
                    else
                    {
                        if (voltage <= 3.6)
                        {
                            return BATTERYLEVELSTATUS.LOW_17;
                        }
                    }
                }
                else
                {
                    if (_currentInventoryMode == BATTERYMODE.INVENTORY)
                    {
                        if (voltage <= 3.515)
                        {
                            return BATTERYLEVELSTATUS.LOW;
                        }
                    }
                    else
                    {
                        if (voltage <= 3.652)
                        {
                            return BATTERYLEVELSTATUS.LOW;
                        }
                    }
                }

                return BATTERYLEVELSTATUS.NORMAL;
            }

            public static double Voltage2Percent(double voltage)
            {
                int cnt;

                if (voltage > voltageTable[0])
                {
                    if (voltage > voltageTable[1])
                    {
                        for (cnt = voltageTable.Length - 1; cnt >= 0; cnt--)
                        {
                            if (voltage > voltageTable[cnt])
                            {
                                if (cnt == voltageTable.Length - 1)
                                    return 100;

                                double percent = 0;

                                percent = (voltagestep * (cnt - 1) + voltageFirstOffset) + ((voltage - voltageTable[cnt]) * voltageSlope[cnt]);

                                return percent;
                            }
                        }
                    }
                    else
                    {
                        double percent = ((voltage - voltageTable[0]) * voltageSlope[0]);
                        return percent;
                    }
                }

                return 0;
            }
        }
    #endif

    */
}
