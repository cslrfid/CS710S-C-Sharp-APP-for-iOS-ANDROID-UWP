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

using CSLibrary.Constants;
using CSLibrary.Structures;
using static CSLibrary.FrequencyBand;

namespace CSLibrary
{
    public partial class RFIDReader
    {

        /*
        /// <summary>
        /// Get Current Selected Frequency Channel
        /// </summary>
        public uint SelectedChannel
        {
            get { return m_save_selected_freq m_save_freq_channel; }
        }

        /// <summary>
        /// Get current seelcted Country
        /// </summary>
        public double SelectedFrequencyBand
        {
            get { return   m_save_selected_freq; }
        }
        */

        /// <summary>
        /// Set Fixed Frequency Channel
        /// All region can be used to set a fixed channel
        /// </summary>
        /// <param name="prof">Region Code</param>
        /// <param name="channel">Channel number start from zero, you can get the available channels 
        /// from CSLibrary.HighLevelInterface.AvailableFrequencyTable(CSLibrary.Constants.RegionCode)</param>
        public Result SetFixedChannel(RegionCode prof = RegionCode.CURRENT, uint channel = 0)
        {
            bool m_save_fixed_channel = !FrequencyBand.frequencySet.Find(i => i.code == prof).hopping;

            if (m_save_fixed_channel)
                return Result.INVALID_PARAMETER;

            if (m_save_fixed_channel == true && m_save_region_code == prof && m_save_freq_channel == channel)
            {
                if (currentInventoryFreqRevIndex == null)
                    currentInventoryFreqRevIndex = new uint[1] { channel };
                return Result.OK;
            }

            uint Reg0x700 = 0;

            //DEBUG_WriteLine(DEBUGLEVEL.API, "HighLevelInterface.SetFixedChannel(RegionCode prof, uint channel, LBT LBTcfg)");

            if (!GetActiveRegionCode().Contains(prof))
                return Result.INVALID_PARAMETER;

            // disable agile mode
            MacReadRegister(MACREGISTER.HST_ANT_CYCLES /*0x700*/, ref Reg0x700);
            Reg0x700 &= ~0x01000000U;
            MacWriteRegister(MACREGISTER.HST_ANT_CYCLES /*0x700*/, Reg0x700);

            //AGAIN:
            //try
            {
                //Result status = Result.OK;
                uint TotalCnt = FreqChnCnt(prof);
                uint[] freqTable = FreqTable(prof);
                uint i = 0;

                // Check Parameters
                if (!FreqChnWithinRange(channel, prof) || freqTable == null)
                    return Result.INVALID_PARAMETER;

                int Index = FreqSortedIdxTbls(prof, channel);
                if (Index < 0)
                    return Result.INVALID_PARAMETER;

                //Enable channel
                SetFrequencyBand(0, BandState.ENABLE, freqTable[Index], GetPllcc(prof));
                //ThrowException(SetFrequencyBand((uint)Index, BandState.ENABLE, freqTable[Index], GetPllcc(prof)));
                //ThrowException(SetFrequencyBand(0, BandState.ENABLE, freqTable[Index]));
                i = 1;

                //Disable channels
                for (uint j = i; j < MAXFRECHANNEL; j++)
                {
                    SetFrequencyBand(j, BandState.DISABLE, 0, 0);
                }

                SetRadioLBT(LBT.OFF);

                m_save_countryname = FrequencyBand.GetRegionName(prof);
                m_save_region_code = prof;
                m_save_freq_channel = (int)channel;

                //m_save_region_code = prof;
                //m_save_freq_channel = (int)channel;
                //m_save_fixed_channel = true;
                //m_save_agile_channel = false;
                //m_save_selected_freq = GetAvailableFrequencyTable(prof)[channel];

            }
#if nouse
            catch (ReaderException ex)
            {
                //if (FireIfReset(ex.ErrorCode) == Result.OK)
                //{
                //    goto AGAIN;
                //}
            }
            catch
            {
                m_Result = Result.SYSTEM_CATCH_EXCEPTION;
            }
#endif

            currentInventoryFreqRevIndex = new uint[1] { channel };

            return Result.OK;
            //return m_Result;
        }

        /// <summary>
        /// Set to the specific frequency profile
        /// </summary>
        /// <param name="prof">Country Profile</param>
        /// <returns>Result</returns>
        public Result SetHoppingChannels(RegionCode prof)
        {
            bool m_save_fixed_channel = !FrequencyBand.frequencySet.Find(i => i.code == prof).hopping;

            if (m_save_fixed_channel || !GetActiveRegionCode().Contains(prof))
                return Result.INVALID_PARAMETER;

            if (!m_save_fixed_channel && m_save_region_code == prof)
            {
                if (currentInventoryFreqRevIndex == null)
                    currentInventoryFreqRevIndex = FreqIndex(m_save_region_code);
                return Result.OK;
            }

            uint TotalCnt = FreqChnCnt(prof);
            uint[] freqTable = FreqTable(prof);

            //Enable channels
            for (uint i = 0; i < TotalCnt; i++)
            {
                SetFrequencyBand(i, BandState.ENABLE, freqTable[i], GetPllcc(prof));
            }

            //Disable channels
            for (uint i = TotalCnt; i < 50; i++)
            {
                SetFrequencyBand(i, BandState.DISABLE, 0, 0);
            }

            SetRadioLBT(LBT.OFF);

            m_save_countryname = FrequencyBand.GetRegionName(prof);
            m_save_region_code = prof;
            m_save_freq_channel = -1;

            //m_save_region_code = prof;
            //m_save_freq_channel = -1;
            //m_save_agile_channel = false;
            m_Result = Result.OK;

            currentInventoryFreqRevIndex = FreqIndex(m_save_region_code);

            return Result.OK;
        }

        /// <summary>
        /// Reset current frequency profile
        /// </summary>
        /// <returns></returns>
        public Result SetHoppingChannels()
        {
            return SetHoppingChannels(m_save_region_code);
        }

        /// <summary>
        /// Set to frequency agile mode
        /// </summary>
        /// <param name="prof">Country Profile</param>
        /// <returns>Result</returns>
        private Result SetAgileChannels(RegionCode prof)
        {
            bool m_save_fixed_channel = !FrequencyBand.frequencySet.Find(i => i.code == prof).hopping;

            if (!m_save_fixed_channel && m_save_region_code == prof)
            {
                if (currentInventoryFreqRevIndex == null)
                    currentInventoryFreqRevIndex = FreqIndex(m_save_region_code);
                return Result.OK;
            }

            uint Reg0x700 = 0;

            if (!GetActiveRegionCode().Contains(prof) || (prof != RegionCode.ETSI && prof != RegionCode.JP))
                return Result.INVALID_PARAMETER;

            uint TotalCnt = FreqChnCnt(prof);
            uint[] freqTable = FreqTable(prof);

            //Enable channels
            for (uint i = 0; i < TotalCnt; i++)
            {
                SetFrequencyBand(i, BandState.ENABLE, freqTable[i], GetPllcc(prof));
            }
            //Disable channels
            for (uint i = TotalCnt; i < 50; i++)
            {
                SetFrequencyBand(i, BandState.DISABLE, 0, 0);
            }

            SetRadioLBT(LBT.OFF);

            m_save_region_code = prof;
            m_save_fixed_channel = false;
            //m_save_agile_channel = true;

            MacReadRegister(MACREGISTER.HST_ANT_CYCLES /*0x700*/, ref Reg0x700);
            Reg0x700 |= 0x01000000U;
            MacWriteRegister(MACREGISTER.HST_ANT_CYCLES /*0x700*/, Reg0x700);

            currentInventoryFreqRevIndex = FreqIndex(m_save_region_code);
            return Result.OK;
        }

        internal Result InitDefaultChannel_CS108()
        {
            var defaulRegion = CSLibrary.DEVICE.GetDefauleRegion((int)m_oem_country_code, (int)m_oem_special_country_version);
            var hopping = CSLibrary.FrequencyBand.HoppingAvalibable(defaulRegion);

            if (hopping)
                m_save_freq_channel = -1;
            else
                m_save_freq_channel = 0;

            m_save_countryname = FrequencyBand.GetRegionName(defaulRegion);
            m_save_region_code = defaulRegion;

            return Result.OK;
        }

        internal Result SetDefaultChannel_CS108()
        {
            //m_save_freq_channel = 0;
            var defaulRegion = CSLibrary.DEVICE.GetDefauleRegion((int)m_oem_country_code, (int)m_oem_special_country_version);
            var hopping = CSLibrary.FrequencyBand.HoppingAvalibable(defaulRegion);

            if (hopping)
            {
                SetHoppingChannels(defaulRegion);
            }
            else
            {
                SetFixedChannel(defaulRegion, 0);
            }

            return Result.OK;
        }
    }
}