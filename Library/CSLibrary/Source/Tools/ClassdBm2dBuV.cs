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
