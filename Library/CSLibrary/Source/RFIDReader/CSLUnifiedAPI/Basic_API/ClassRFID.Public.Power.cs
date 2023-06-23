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
    using static RFIDDEVICE;
    using static FrequencyBand;
    using Constants;

    public partial class RFIDReader
    {
        //private uint m_oem_hipower = 0;

        /// <summary>
        /// Available Maximum Power you can set on specific region
        /// </summary>
        public uint GetActiveMaxPowerLevel()
        {
           return 320;
/*
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetActiveMaxPowerLevel_CS108();
                    break;

                case MODEL.CS710S:
                    return GetActiveMaxPowerLevel_CS710S();
                    break;
            }

            return 0;
*/
        }

        /// <summary>
        /// Get current power level
        /// </summary>
        public uint SelectedPowerLevel
        {
            get
            {
                uint pwrlvl = 0;
                switch (_deviceType)
                {
                    case MODEL.CS108:
                        GetPowerLevel_CS108(ref pwrlvl);
                        break;

                    case MODEL.CS710S:
                        GetPowerLevel_CS710S(ref pwrlvl);
                        break;
                }
                return pwrlvl;
            }
        }

        /// <summary>
        /// Get Power Level
        /// </summary>
        public Result GetPowerLevel(ref uint pwrlvl)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetPowerLevel_CS108(ref pwrlvl);

                case MODEL.CS710S:
                    return GetPowerLevel_CS710S(ref pwrlvl);
            }

            return Result.FAILURE;
        }

        public Result SetPowerLevel(uint pwrlevel, uint port = 0)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetPowerLevel_CS108(pwrlevel, port = 0);

                case MODEL.CS710S:
                    return SetPowerLevel_CS710S(pwrlevel, port = 0);
            }

            return Result.FAILURE;
        }

        public Result SetPowerLevel(int pwrlevel, uint port = 0)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetPowerLevel_CS108((uint)pwrlevel, port);

                case MODEL.CS710S:
                    return SetPowerLevel_CS710S((uint)pwrlevel, port);
            }

            return Result.FAILURE;
        }

        public Result SetPowerLevel(UInt32 [] pwrlevel)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetPowerLevel_CS108(pwrlevel);

                case MODEL.CS710S:
                    return SetPowerLevel_CS710S(pwrlevel);
            }

            return Result.FAILURE;
        }

        /// <summary>
        /// Available Maximum Power you can set on specific region
        /// </summary>
        private uint GetSoftwareMaxPowerLevel(RegionCode region)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetSoftwareMaxPowerLevel_CS108(region);

                case MODEL.CS710S:
                    return GetSoftwareMaxPowerLevel_CS710S(region);
            }

            return 0;
        }

        /// <summary>
        /// Set Power Sequencing (only for CS108)
        /// </summary>
        /// <param name="numberofPower"></param>
        /// <param name="power"></param>
        /// <param name="dwell"></param>
        /// <returns></returns>
        public Result SetPowerSequencing(int numberofPower, uint[] power = null, uint[] dwell = null, bool CloneAntenna0Setting = true)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetPowerSequencing_CS108(numberofPower, power, dwell);

                case MODEL.CS710S:
                    return SetPowerSequencing_CS710S(numberofPower, power, dwell, CloneAntenna0Setting);
            }

            return Result.FAILURE;
        }
    }
}
