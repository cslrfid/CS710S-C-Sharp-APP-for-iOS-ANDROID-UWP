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
        /// <summary>
        /// Get frequency table on specific region
        /// </summary>
        /// <param name="region">Region Code</param>
        /// <returns></returns>
        internal double[] GetAvailableFrequencyTable_CS108(RegionCode region)
        {
            switch (region)
            {
                case RegionCode.AU:
                    return AUSTableOfFreq;
                case RegionCode.CN:
                    return CHNTableOfFreq;
                case RegionCode.ETSI:
                case RegionCode.G800:
                    return ETSITableOfFreq;
                case RegionCode.IN:
                    return IDATableOfFreq;
                case RegionCode.AR:
                case RegionCode.CL:
                case RegionCode.CO:
                case RegionCode.CR:
                case RegionCode.DO:
                case RegionCode.PA:
                case RegionCode.UY:
                case RegionCode.FCC:
                    return FCCTableOfFreq;
                case RegionCode.HK:
                    return OFCATableOfFreq;
                case RegionCode.SG:
                case RegionCode.TH:
                case RegionCode.VI:
                    return HKTableOfFreq;
                case RegionCode.JP:
                    if (m_oem_special_country_version == 0x2A4A5036)
                        return JPN2012TableOfFreq;
                    else
                        return JPN2019TableOfFreq;
                case RegionCode.KR:
                    return KRTableOfFreq;
                case RegionCode.MY:
                    return MYSTableOfFreq;
                case RegionCode.TW:
                    return TWTableOfFreq;
                case RegionCode.ZA:
                    return ZATableOfFreq;
                case RegionCode.BR1:
                    return BR1TableOfFreq;
                case RegionCode.PE:
                case RegionCode.BR2:
                    return BR2TableOfFreq;
                case RegionCode.BR3:
                    return BR3TableOfFreq;
                case RegionCode.BR4:
                    return BR4TableOfFreq;
                case RegionCode.BR5:
                    return BR5TableOfFreq;
                case RegionCode.ID:
                    return IDTableOfFreq;
                case RegionCode.JE:
                    return JETableOfFreq;
                case RegionCode.PH:
                    return PHTableOfFreq;
                case RegionCode.ETSIUPPERBAND:
                    return ETSIUPPERBANDTableOfFreq;
                case RegionCode.NZ:
                    return NZTableOfFreq;
                case RegionCode.UH1:
                    return UH1TableOfFreq;
                case RegionCode.UH2:
                    return UH2TableOfFreq;
                case RegionCode.LH:
                    return LHTableOfFreq;
                case RegionCode.LH1:
                    return LH1TableOfFreq;
                case RegionCode.LH2:
                    return LH2TableOfFreq;
                case RegionCode.VE:
                    return VETableOfFreq;
                case RegionCode.BA:
                    return BATableOfFreq;
                default:
                    return new double[0];
            }
        }


    }
}
