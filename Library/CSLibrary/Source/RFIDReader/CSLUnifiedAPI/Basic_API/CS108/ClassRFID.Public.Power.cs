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
        /// <summary>
        /// Get Power Level
        /// </summary>
        public Result GetPowerLevel_CS108(ref uint pwrlvl)
        {
            MacWriteRegister(MACREGISTER.HST_ANT_DESC_SEL, 0);
            MacReadRegister(MACREGISTER.HST_ANT_DESC_RFPOWER, ref pwrlvl);

            return Result.OK;
        }

        public Result SetPowerLevel_CS108(uint pwrlevel, uint port = 0)
        {
            if (pwrlevel > 330)
                pwrlevel = 330;

            MacWriteRegister(MACREGISTER.HST_ANT_DESC_SEL, port);         // select antenna
            MacWriteRegister(MACREGISTER.HST_ANT_DESC_RFPOWER, pwrlevel);

            return Result.OK;
        }

        public Result SetPowerLevel_CS108(int pwrlevel, uint port = 0)
        {
            if (pwrlevel < 0)
                return Result.INVALID_PARAMETER;

            return SetPowerLevel_CS108((uint)pwrlevel, port);
        }

        public Result SetPowerLevel_CS108(UInt32 [] pwrlevel)
        {
            Result r;

            for (uint cnt = 0; cnt < pwrlevel.Length; cnt++)
                if ((r = SetPowerLevel_CS108(pwrlevel[cnt], cnt)) != Result.OK)
                    return r;

            return Result.OK;
        }

        /// <summary>
        /// Available Maximum Power you can set on specific region
        /// </summary>
        private uint GetSoftwareMaxPowerLevel_CS108(RegionCode region)
        {
            // MAX Power 32dB
            if ((m_oem_hipower == 1) ||
                (m_oem_machine == MODEL.CS468INT) ||
                (m_oem_machine == MODEL.CS463) ||
                (m_oem_machine == MODEL.CS469) ||
                (region == RegionCode.IN) ||
                (region == RegionCode.G800) ||
                (m_oem_machine == MODEL.CS209) ||
                (m_oem_machine == MODEL.CS103) ||
                (m_oem_machine == MODEL.CS108)
                )
                return 320;

            // MAX Power 27.5dB
            if ((m_oem_machine == MODEL.CS101 && region == RegionCode.ETSI) ||
                (m_oem_machine == MODEL.CS203 && region == RegionCode.JP))
                return 275;

            return 300;
        }

        /// <summary>
        /// Set Power Sequencing (only for CS108)
        /// </summary>
        /// <param name="numberofPower"></param>
        /// <param name="power"></param>
        /// <param name="dwell"></param>
        /// <returns></returns>
        public Result SetPowerSequencing_CS108(int numberofPower, uint[] power = null, uint[] dwell = null)
        {
            if (numberofPower == 0)
            {
                try
                {
                    int i;
                    for (i = 0; i < m_AntennaList.Count; i++)
                    {
                        if (m_AntennaList[i].PowerLevel > GetSoftwareMaxPowerLevel(m_save_region_code))
                            m_AntennaList[i].PowerLevel = GetSoftwareMaxPowerLevel(m_save_region_code);

                        SetAntennaPortStatus((uint)i, m_AntennaList[i].AntennaStatus);
                        SetAntennaPortConfiguration((uint)i, m_AntennaList[i].AntennaConfig);
                    }

                    for (; i < 16; i++)
                    {
                        AntennaPortSetState((uint)i, AntennaPortState.DISABLED);
                    }
                }
                catch (Exception ex)
                {
                    CSLibrary.Debug.WriteLine("Set Antenna Configuration Fail : " + ex.Message);
                }
                return Result.OK;
            }

            if (power == null || dwell == null || power.Length < numberofPower || dwell.Length < numberofPower)
            {
                return Result.INVALID_PARAMETER;
            }

            {
                uint i;

                for (i = 0; i < numberofPower; i++)
                {
                    AntennaPortSetState((uint)i, AntennaPortState.ENABLED);
                    SetPowerLevel_CS108(power[i], i);
                    SetInventoryDuration_CS108(dwell[i], i);
                }

                for (; i < 16; i++)
                {
                    AntennaPortSetState((uint)i, AntennaPortState.DISABLED);
                }
            }

            return Result.OK;
        }

    }
}
