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

namespace CSLibrary
{
    using static FrequencyBand;

    public partial class RFIDReader
    {
        internal string[] _profileNameList_CS108 = {
            "  0. Multipath Interference Resistance",
            "  1. Range/Dense Reader",
            "  2. Range/Throughput/Dense Reader",
            "  3. Max Throughput"
        };


        public uint[] GetActiveLinkProfile_CS108(RegionCode region)
        {
            switch (region)
            {
                case RegionCode.KR:
                    return new uint[] { 0, 1, 2 };

                case RegionCode.JP:
                    return new uint[] { 1, 2 };

                default:
                    return new uint[] { 0, 1, 2, 3 };
            }
        }

        public string[] GetActiveLinkProfileName_CS108(RegionCode region)
        {
            switch (region)
            {
                case RegionCode.KR:
                    return new string[] { _profileNameList_CS108[0], _profileNameList_CS108[1], _profileNameList_CS108[2] };

                case RegionCode.JP:
                    return new string[] { _profileNameList_CS108[1], _profileNameList_CS108[2] };

                default:
                    return _profileNameList_CS108;
            }
        }

        /// <summary>
        /// Current selected frequency
        /// </summary>
        public uint SelectedLinkProfile_CS108
        {
            get
            {
                uint link = 0;
                GetCurrentLinkProfile_CS108(ref link);
                return link;
            }
        }

        /// <summary>
        /// Allows the application to set the current link profile for the radio 
        /// module.  A link profile will remain in effect until changed by a 
        /// subsequent call to RFID_RadioSetCurrentLinkProfile.  The 
        /// current link profile cannot be set while a radio module is executing 
        /// a tag-protocol operation. 
        /// </summary>
        /// <param name="profile">
        /// The link profile to make the current link profile.  If this 
        /// parameter does not represent a valid link profile, 
        /// RFID_ERROR_INVALID_PARAMETER is returned. </param>
        /// <returns></returns>
        public Result SetCurrentLinkProfile_CS108(uint profile)
        {
            //DEBUG_WriteLine(DEBUGLEVEL.API, "HighLevelInterface.SetCurrentLinkProfile(uint profile)");

            MacWriteRegister(MACREGISTER.HST_RFTC_CURRENT_PROFILE, profile);
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.UPDATELINKPROFILE), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE);
            SetLNA(m_save_rflna_high_comp, m_save_rflna_gain, m_save_iflna_gain, m_save_ifagc_gain);

            return Result.OK;
        }

        /// <summary>
        ///  Allows the application to retrieve the current link profile for the 
        ///  radio module.  The current link profile cannot be retrieved while a 
        ///  radio module is executing a tag-protocol operation. 
        /// </summary>
        /// <returns></returns>
        public Result GetCurrentLinkProfile_CS108(ref uint link)
        {
            MacReadRegister(MACREGISTER.HST_RFTC_CURRENT_PROFILE, ref link);

            return Result.OK;
        }

    }
}
