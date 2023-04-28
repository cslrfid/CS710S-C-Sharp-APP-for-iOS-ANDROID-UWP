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
    using static RFIDDEVICE;
    using static FrequencyBand;

    public partial class RFIDReader
    {
        public uint[] GetActiveLinkProfile()
        {
            return GetActiveLinkProfile(m_save_region_code);
        }

        public string[] GetActiveLinkProfileName()
        {
            return GetActiveLinkProfileName(m_save_region_code);
        }

        public uint[] GetActiveLinkProfile(RegionCode region)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetActiveLinkProfile_CS108(region);

                case MODEL.CS710S:
                    return GetActiveLinkProfile_CS710S(region);
            }

            return null;
        }

        public string[] GetActiveLinkProfileName(RegionCode region)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetActiveLinkProfileName_CS108(region);

                case MODEL.CS710S:
                    return GetActiveLinkProfileName_CS710S(region);
            }

            return null;
        }

        /// <summary>
        /// Current selected frequency
        /// </summary>
        public uint SelectedLinkProfile
        {
            get
            {
                uint link = 0;
                GetCurrentLinkProfile(ref link);
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
        public Result SetCurrentLinkProfile(uint profile)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetCurrentLinkProfile_CS108(profile);

                case MODEL.CS710S:
                    return SetCurrentLinkProfile_CS710S(profile);
            }

            return Result.FAILURE;
        }

        /// <summary>
        ///  Allows the application to retrieve the current link profile for the 
        ///  radio module.  The current link profile cannot be retrieved while a 
        ///  radio module is executing a tag-protocol operation. 
        /// </summary>
        /// <returns></returns>
        public Result GetCurrentLinkProfile(ref uint link)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetCurrentLinkProfile_CS108(ref link);

                case MODEL.CS710S:
                    return GetCurrentLinkProfile_CS710S(ref link);
            }

            return Result.FAILURE;
        }

    }
}
