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
    using static FrequencyBand;
    using Constants;

    public partial class RFIDReader
    {
        public uint[] GetActiveLinkProfile_CS710S(RegionCode region)
        {
            switch (region)
            {
                default:
                    return new uint[] { 103, 120, 345, 302, 323, 344, 223, 222, 241, 244, 285 };
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
        public Result SetCurrentLinkProfile_CS710S(uint profile)
        {
            RFIDRegister.AntennaPortConfig.RfMode((UInt16)profile);
            return Result.OK;
        }

        /// <summary>
        ///  Allows the application to retrieve the current link profile for the 
        ///  radio module.  The current link profile cannot be retrieved while a 
        ///  radio module is executing a tag-protocol operation. 
        /// </summary>
        /// <returns></returns>
        public Result GetCurrentLinkProfile_CS710S(ref uint link)
        {
            return Result.FAILURE;
        }

    }
}
