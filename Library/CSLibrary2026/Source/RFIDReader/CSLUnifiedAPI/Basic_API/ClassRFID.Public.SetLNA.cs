using System;
using System.Collections.Generic;
using System.Text;

using CSLibrary.Constants;

namespace CSLibrary
{
    using static RFIDDEVICE;

    public partial class RFIDReader
    {
        /// <summary>
        /// RF LNA compression mode = 0, 1
        /// RF LNA Gain = 1, 7, 13
        /// IF LNA Gain = 6, 12, 18, 24
        /// AGC Gain = -12, -6, 0, 6
        /// </summary>
        /// <param name="rflna_high_comp_norm"></param>
        /// <param name="rflna_gain_norm"></param>
        /// <param name="iflna_gain_norm"></param>
        /// <param name="ifagc_gain_norm"></param>
        /// <param name="ifagc_gain_norm"></param>
        /// <returns></returns>
        public Result SetLNA(int rflna_high_comp, int rflna_gain, int iflna_gain, int ifagc_gain)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetLNA_CS108(rflna_high_comp, rflna_gain, iflna_gain, ifagc_gain);

                //case MODEL.CS710S:
                //    break;
            }

            return Result.FAILURE;
        }
    }
}
