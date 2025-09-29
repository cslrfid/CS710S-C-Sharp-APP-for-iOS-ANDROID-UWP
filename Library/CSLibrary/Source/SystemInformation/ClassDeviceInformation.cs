/*
Copyright (c) 2025 Convergence Systems Limited

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

/// <summary>
/// Provides static device information for Convergence Systems Limited (CSL) RFID readers
/// Includes model numbers, antenna counts and basic specifications
/// </summary>

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
            //CS710S = 16,
            CS710S = 32,
            CS203XL = 33,
            UNKNOWN = 0xff
        }

/*

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
*/
    }
}
