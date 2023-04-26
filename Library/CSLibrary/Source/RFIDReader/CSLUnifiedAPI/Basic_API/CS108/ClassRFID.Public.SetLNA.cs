using System;
using System.Collections.Generic;
using System.Text;

using CSLibrary.Constants;

namespace CSLibrary
{
    public partial class RFIDReader
    {
        private int m_save_rflna_high_comp = 1;
        private int m_save_rflna_gain = 1;
        private int m_save_iflna_gain = 24;
        private int m_save_ifagc_gain = -6;

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
        public Result SetLNA_CS108(int rflna_high_comp, int rflna_gain, int iflna_gain, int ifagc_gain)
        {
            if (rflna_high_comp != 00 && rflna_high_comp != 1)
                return Result.INVALID_PARAMETER;

            if (rflna_gain != 1 && rflna_gain != 7 && rflna_gain != 13)
                return Result.INVALID_PARAMETER;

            if (iflna_gain != 6 && iflna_gain != 12 && iflna_gain != 18 && iflna_gain != 24)
                return Result.INVALID_PARAMETER;

            if (ifagc_gain != -12 && ifagc_gain != -6 && ifagc_gain != 0 && ifagc_gain != 6)
                return Result.INVALID_PARAMETER;

            m_save_rflna_high_comp = rflna_high_comp;
            m_save_rflna_gain = rflna_gain;
            m_save_iflna_gain = iflna_gain;
            m_save_ifagc_gain = ifagc_gain;

            int rflna_high_comp_norm = rflna_high_comp;
            int rflna_gain_norm = 0;
            int iflna_gain_norm = 0;
            int ifagc_gain_norm = 0;

            switch (rflna_gain)
            {
                case 1:
                    rflna_gain_norm = 0;
                    break;
                case 7:
                    rflna_gain_norm = 2;
                    break;
                case 13:
                    rflna_gain_norm = 3;
                    break;
            }

            switch (iflna_gain)
            {
                case 24:
                    iflna_gain_norm = 0;
                    break;
                case 18:
                    iflna_gain_norm = 1;
                    break;
                case 12:
                    iflna_gain_norm = 3;
                    break;
                case 6:
                    iflna_gain_norm = 7;
                    break;
            }

            switch (ifagc_gain)
            {
                case -12:
                    ifagc_gain_norm = 0;
                    break;
                case -6:
                    ifagc_gain_norm = 4;
                    break;
                case 0:
                    ifagc_gain_norm = 6;
                    break;
                case 6:
                    ifagc_gain_norm = 7;
                    break;
            }

            int value = rflna_high_comp_norm << 8 |
                rflna_gain_norm << 6 |
                iflna_gain_norm << 3 |
                ifagc_gain_norm;

            return MacBypassWriteRegister(0x450, (UInt16)(value));
        }
    }
}
