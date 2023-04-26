using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    using static RFIDDEVICE;
    using Constants;

    public static class FrequencyBand
    {
        /// <summary>
        /// Region Profile
        /// </summary>
        public enum RegionCode
        {
            /// <summary>
            /// USA
            /// </summary>
            FCC = 1,
            /// <summary>
            /// Europe
            /// </summary>
            ETSI,
            /// <summary>
            /// China all frequency
            /// </summary>
            CN,
            /// <summary>
            /// Taiwan
            /// </summary>
            TW,
            /// <summary>
            /// Korea
            /// </summary>
            KR,
            /// <summary>
            /// Hong Kong
            /// </summary>
            HK,
            /// <summary>
            /// Japan
            /// </summary>
            JP,
            /// <summary>
            /// Australia
            /// </summary>
            AU,
            /// <summary>
            /// Malaysia
            /// </summary>
            MY,
            /// <summary>
            /// Singapore
            /// </summary>
            SG,
            /// <summary>
            /// India
            /// </summary>
            IN,
            /// <summary>
            /// G800 same as India
            /// </summary>
            G800,
            /// <summary>
            /// South Africa
            /// </summary>
            ZA,
            //New added
            /// <summary>
            /// Brazil
            /// </summary>
            BR1,
            /// <summary>
            /// Brazil
            /// </summary>
            BR2,
            /// <summary>
            /// Brazil
            /// </summary>
            BR3,
            /// <summary>
            /// Brazil
            /// </summary>
            BR4,
            /// <summary>
            /// Brazil
            /// </summary>
            BR5,
            /// <summary>
            /// Indonesia 
            /// </summary>
            ID,
            /// <summary>
            /// Thailand
            /// </summary>
            TH,
            /// <summary>
            /// Israel
            /// </summary>
            JE,
            /// <summary>
            /// Philippine
            /// </summary>
            PH,
            /// <summary>
            /// ETSI Upper Band
            /// </summary>
            ETSIUPPERBAND,
            /// <summary>
            /// New Zealand
            /// </summary>
            NZ,
            /// <summary>
            /// UH1
            /// </summary>
            UH1,
            /// <summary>
            /// UH2
            /// </summary>
            UH2,
            /// <summary>
            /// LH
            /// </summary>
            LH,
            /// <summary>
            /// LH
            /// </summary>
            LH1,
            /// <summary>
            /// LH
            /// </summary>
            LH2,
            /// <summary>
            /// Venezuela
            /// </summary>
            VE,
            /// <summary>
            /// Argentina
            /// </summary>
            AR,
            /// Chile
            CL,
            /// <summary>
            /// Colombia
            /// </summary>
            CO,
            /// <summary>
            /// Costa Rica??? ????????
            /// </summary>
            CR,
            /// <summary>
            /// Dominican Republic
            /// </summary>
            DO,
            /// <summary>
            /// Panama
            /// </summary>
            PA,
            /// <summary>
            /// Peru
            /// </summary>
            PE,
            /// <summary>
            /// Uruguay
            /// </summary>
            UY,
            /// <summary>
            /// Bangladesh
            /// </summary>
            BA,
            /// <summary>
            /// Vietnam
            /// </summary>
            VI,
            /// <summary>
            /// Unknow Country
            /// </summary>
            UNKNOWN = 0,
            /// <summary>
            /// Current Country
            /// </summary>
            CURRENT = -1
        }

        public class FREQUENCYSET
        {
            public RegionCode code;
            public string name;
            public int year;
            public bool hopping;

            public FREQUENCYSET(RegionCode code, string name, int year, bool hopping)
            {
                this.code = code;
                this.name = name;
                this.year = year;
                this.hopping = hopping;
            }

        }

        public static List<FREQUENCYSET> frequencySet;
        static List<RegionCode> m_save_country_list;

        static FrequencyBand()
        {
            frequencySet = new List<FREQUENCYSET>();

            frequencySet.Add(new FREQUENCYSET(RegionCode.FCC, "USA/Canada", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.ETSI, "Europe", 0, false));
            frequencySet.Add(new FREQUENCYSET(RegionCode.CN, "China", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.TW, "Taiwan", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.KR, "Korea", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.HK, "Hong Kong", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.JP, "Japan", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.AU, "Australia", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.MY, "Malaysia", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.SG, "Singapore", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.IN, "India", 0, false));
            frequencySet.Add(new FREQUENCYSET(RegionCode.G800, "G800", 0, false));
            frequencySet.Add(new FREQUENCYSET(RegionCode.ZA, "South Africa", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.BR1, "Brazil 915-927", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.BR2, "Brazil 902-906, 915-927", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.BR3, "Brazil 902-906", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.BR4, "Brazil 902-904", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.BR5, "Brazil 917-924", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.ID, "Indonesia", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.TH, "Thailand", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.JE, "Israel", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.PH, "Philippine", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.ETSIUPPERBAND, "ETSI Upper Band", 0, false));
            frequencySet.Add(new FREQUENCYSET(RegionCode.NZ, "New Zealand", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.UH1, "UH1", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.UH2, "UH2", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.LH, "LH", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.LH1, "LH1", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.LH2, "LH2", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.VE, "Venezuela", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.AR, "Argentina", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.CL, "Chile", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.CO, "Colombia", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.CR, "Costa Rica", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.DO, "Dominican Republic", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.PA, "Panama", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.PE, "Peru", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.UY, "Uruguay", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.BA, "Bangladesh", 0, true));
            frequencySet.Add(new FREQUENCYSET(RegionCode.VI, "Vietnam", 0, true));
        }

        public static RegionCode GetRegionCode(string name)
        {
            return ((frequencySet.Find(item => item.name == name)).code);
        }

        public static string GetRegionName(RegionCode code)
        {
            return ((frequencySet.Find(item => item.code == code)).name);
        }

        public static bool HoppingAvalibable(RegionCode code)
        {
            return ((frequencySet.Find(item => item.code == code)).hopping);
        }

        public static RegionCode[] GetRegionCodeList()
        {
            RegionCode[] list = new RegionCode[frequencySet.Count];

            for (int cnt = 0; cnt < list.Length; cnt++)
                list[cnt] = frequencySet[cnt].code;

            return list;
        }

        public static string[] GetRegionNameList()
        {
            string[] list = new string[frequencySet.Count];

            for (int cnt = 0; cnt < frequencySet.Count; cnt++)
                list[cnt] = frequencySet[cnt].name;

            return list;
        }

        public static string[] GetRegionNameList(RegionCode[] code)
        {
            string[] list = new string[code.Length];

            for (int cnt = 0; cnt < code.Length; cnt++)
                list[cnt] = GetRegionName(code[cnt]);

            return list;
        }

        public static string[] GetRegionNameList(List <RegionCode> code)
        {
            string[] list = new string[code.Count];

            for (int cnt = 0; cnt < code.Count; cnt++)
                list[cnt] = GetRegionName(code[cnt]);

            return list;
        }
    }
}
