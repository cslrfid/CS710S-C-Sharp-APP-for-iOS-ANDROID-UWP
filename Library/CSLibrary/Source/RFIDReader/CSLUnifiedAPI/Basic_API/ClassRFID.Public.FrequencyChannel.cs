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

namespace CSLibrary
{
    using static FrequencyBand;

    public partial class RFIDReader
    {
        private RegionCode m_save_region_code = RegionCode.UNKNOWN;
        private bool m_save_fixed_channel = false;
        private bool m_save_agile_channel = false;
        private uint m_save_freq_channel = 0;
        private double m_save_selected_freq = 0;

        /// <summary>
        /// Get Current Selected Frequency Channel
        /// </summary>
        public uint SelectedChannel
        {
            get { return m_save_freq_channel; }
        }

        /// <summary>
        /// Get current frequency 
        /// </summary>
        public double SelectedFrequencyBand
        {
            get { return m_save_selected_freq; }
        }

        /// <summary>
        /// Set Fixed Frequency Channel
        /// All region can be used to set a fixed channel
        /// </summary>
        /// <param name="prof">Region Code</param>
        /// <param name="channel">Channel number start from zero, you can get the available channels 
        /// from CSLibrary.HighLevelInterface.AvailableFrequencyTable(CSLibrary.Constants.RegionCode)</param>
        public Result SetFixedChannel(RegionCode prof = RegionCode.CURRENT, uint channel = 0)
        {
            return Result.FAILURE;
        }

        /// <summary>
        /// Set to the specific frequency profile
        /// </summary>
        /// <param name="prof">Country Profile</param>
        /// <returns>Result</returns>
        public Result SetHoppingChannels(RegionCode prof)
        {
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
        public Result SetAgileChannels(RegionCode prof)
        {
            return Result.OK;
        }

        internal Result InitDefaultChannel()
        {
            switch (m_save_country_code)
            {
                case 1:     // ETSI
                    m_save_region_code = RegionCode.ETSI;
                    m_save_fixed_channel = true;
                    m_save_agile_channel = false;
                    m_save_freq_channel = 0;
                    break;

                case 2:     // FCC
                    if (m_oem_freq_modification_flag == 0x00)
                        m_save_region_code = RegionCode.FCC;
                    else
                    {
                        switch (m_oem_special_country_version)
                        {
                            default: // and case 0x2a555341
                                m_save_region_code = RegionCode.FCC;
                                break;
                            case 0x4f464341:
                                m_save_region_code = RegionCode.HK;
                                break;
                            case 0x2a2a4153:
                                m_save_region_code = RegionCode.AU;
                                break;
                            case 0x2a2a4e5a:
                                m_save_region_code = RegionCode.NZ;
                                break;
                            case 0x20937846:
                                m_save_region_code = RegionCode.ZA;
                                break;
                            case 0x2A2A5347:
                                m_save_region_code = RegionCode.SG;
                                break;
                        }
                    }
                    m_save_fixed_channel = false;
                    m_save_agile_channel = false;
                    break;

                case 4:     // 
                    m_save_region_code = RegionCode.TW;
                    m_save_fixed_channel = false;
                    m_save_agile_channel = false;
                    break;

                case 6:     // 
                    m_save_region_code = RegionCode.KR;
                    m_save_fixed_channel = false;
                    m_save_agile_channel = false;
                    break;

                case 7:     // 
                    m_save_region_code = RegionCode.CN;
                    m_save_fixed_channel = false;
                    m_save_agile_channel = false;
                    break;

                case 8:     // 
                    m_save_region_code = RegionCode.JP;
                    m_save_fixed_channel = true;
                    m_save_agile_channel = false;
                    m_save_freq_channel = 0;
                    break;

                case 9:     // 
                    m_save_region_code = RegionCode.ETSIUPPERBAND;
                    m_save_fixed_channel = true;
                    m_save_agile_channel = false;
                    m_save_freq_channel = 0;
                    break;

                default:
                    break;
            }

            return Result.OK;
        }

        public Result SetDefaultChannel()
        {
            switch (m_save_country_code)
            {
                case 1:     // ETSI
                    SetFixedChannel(RegionCode.ETSI, 0);
                    break;

                case 2:     // FCC
                    if (m_oem_freq_modification_flag == 0x00)
                        SetHoppingChannels(RegionCode.FCC);
                    else
                    {
                        switch (m_oem_special_country_version)
                        {
                            default: // and case 0x2a555341
                                SetHoppingChannels(RegionCode.FCC);
                                break;
                            case 0x4f464341:
                                SetHoppingChannels(RegionCode.HK);
                                break;
                            case 0x2a2a4153:
                                SetHoppingChannels(RegionCode.AU);
                                break;
                            case 0x2a2a4e5a:
                                SetHoppingChannels(RegionCode.NZ);
                                break;
                            case 0x20937846:
                                SetHoppingChannels(RegionCode.ZA);
                                break;
                            case 0x2A2A5347:
                                SetHoppingChannels(RegionCode.SG);
                                break;
                        }
                    }
                    break;

                case 4:     // 
                    SetHoppingChannels(RegionCode.TW);
                    break;

                case 6:     // 
                    SetHoppingChannels(RegionCode.KR);
                    break;

                case 7:     // 
                    SetHoppingChannels(RegionCode.CN);
                    break;

                case 8:     // 
                    SetFixedChannel(RegionCode.JP, 0);
                    break;

                case 9:     // 
                    SetFixedChannel(RegionCode.ETSIUPPERBAND, 0);
                    break;

                default:
                    break;
            }

            return Result.OK;
        }
    }
}