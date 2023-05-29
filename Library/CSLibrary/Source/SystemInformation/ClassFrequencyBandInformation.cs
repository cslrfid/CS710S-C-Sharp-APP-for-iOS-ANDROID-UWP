using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    using static RFIDDEVICE;
    using Constants;
    using System.Threading;
    using static CSLibrary.FrequencyBand;
    using static CSLibrary.RFIDDEVICE;

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
                this.code = code; // RegionCode
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
            foreach (var i in frequencySet)
                if (i.name == name)
                    return i.code;

            return RegionCode.UNKNOWN;
            //return ((frequencySet.Find(item => item.name == name)).code);
        }

        public static string GetRegionName(RegionCode code)
        {
            foreach (var i in frequencySet)
                if (i.code == code)
                    return i.name;

            return null;
            //return ((frequencySet.Find(item => item.code == code)).name);
        }

        public static bool HoppingAvalibable(RegionCode code)
        {
            foreach (var i in frequencySet)
                if (i.code == code)
                    return i.hopping;

            return false;
            //return ((frequencySet.Find(item => item.code == code)).hopping);
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
    
    public static class FrequencyBand_CS710S
    {
        public class FREQUENCYSET
        {
            public int index;                  // CSL E710 Country Enum
            public string name;                // CSL E710 Country Name
            public string modelCode;           // CSL Reader Model Code(Region Code)
            public int totalFrequencyChannel;  // Frequency Channel #
            public string hopping;             // Fixed or Hop
            public int onTime;                 // Hop Time or On Time
            public int offTime;                // Off Time
            public int channelSepatration;     // Channel separation
            public double firstChannel;        // First Channel
            public double lastChannel;         // Last Channel
            public string note;                // Note

            public FREQUENCYSET(int index, string name, string modelCode, int totalFrequencyChannel, string hopping, int onTime, int offTime, int channelSepatration, double firstChannel, double lastChannel, string note)
            {
                this.index = index;
                this.name = name;
                this.modelCode = modelCode;
                this.totalFrequencyChannel = totalFrequencyChannel;
                this.hopping = hopping;
                this.onTime = onTime;
                this.offTime = offTime;
                this.channelSepatration = channelSepatration;
                this.firstChannel = firstChannel;
                this.lastChannel = lastChannel;
                this.note = note;
            }
        }

        public static List<FREQUENCYSET> frequencySet;
        static List<RegionCode> m_save_country_list;

        static FrequencyBand_CS710S()
        {
            frequencySet = new List<FREQUENCYSET>();

            frequencySet.Add(new FREQUENCYSET(0, "UNKNOW", "", 0, "", 0, 0, 0, 0, 0, ""));
            frequencySet.Add(new FREQUENCYSET(1, "Albania1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(2, "Albania2", "-2 RW"       , 23, "Hop", 400, -1, 250, 915.25, 920.75, "915-921"));
            frequencySet.Add(new FREQUENCYSET(3, "Algeria1", "-1"          , 4, "Fixed", 3900, 100, 600, 871.6, 873.4, "870-876"));
            frequencySet.Add(new FREQUENCYSET(4, "Algeria2", "-1"          , 4, "Fixed", 3900, 100, 600, 881.6, 883.4, "880-885"));
            frequencySet.Add(new FREQUENCYSET(5, "Algeria3", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, "915-921"));
            frequencySet.Add(new FREQUENCYSET(6, "Algeria4", "-7"          , 2, "Fixed", 3900, 100, 500, 925.25, 925.75, "925-926"));
            frequencySet.Add(new FREQUENCYSET(7, "Argentina", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(8, "Armenia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(9, "Australia1", "-2 AS"        , 10, "Hop", 400, -1, 500, 920.75, 925.25, ""));
            frequencySet.Add(new FREQUENCYSET(10, "Australia2", "-2 AS"        , 14, "Hop", 400, -1, 500, 918.75, 925.25, ""));
            frequencySet.Add(new FREQUENCYSET(11, "Austria1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(12, "Austria2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(13, "Azerbaijan", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(14, "Bahrain", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(15, "Bangladesh", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(16, "Belarus", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(17, "Belgium1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(18, "Belgium2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(19, "Bolivia", "-2"          , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(20, "Bosnia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(21, "Botswana", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(22, "Brazil1", "-2 RW"       , 9, "Fixed", 3900, 100, 500, 902.75, 906.75, ""));
            frequencySet.Add(new FREQUENCYSET(23, "Brazil2", "-2 RW"       , 24, "Fixed", 3900, 100, 500, 915.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(24, "Brunei1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(25, "Brunei2", "-7"          , 7, "Fixed", 3900, 100, 250, 923.25, 924.75, "923 - 925"));
            frequencySet.Add(new FREQUENCYSET(26, "Bulgaria1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(27, "Bulgaria2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(28, "Cambodia", "-7"          , 16, "Hop", 400, -1, 250, 920.625, 924.375, ""));
            frequencySet.Add(new FREQUENCYSET(29, "Cameroon", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(30, "Canada", "-2"          , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(31, "Chile1", "-2 RW"       , 3, "Fixed", 3900, 100, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(32, "Chile2", "-2 RW"       , 24, "Hop", 400, -1, 500, 915.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(33, "Chile3", "-2 RW"       , 4, "Hop", 400, -1, 500, 925.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(34, "China", "-7"          , 16, "Hop", 2000, -1, 250, 920.625, 924.375, ""));
            frequencySet.Add(new FREQUENCYSET(35, "Colombia", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(36, "Congo", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(37, "CostaRica", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(38, "Cotedlvoire", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(39, "Croatia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(40, "Cuba", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(41, "Cyprus1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(42, "Cyprus2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(43, "Czech1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(44, "Czech2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(45, "Denmark1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(46, "Denmark2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(47, "Dominican", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(48, "Ecuador", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(49, "Egypt", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(50, "ElSalvador", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(51, "Estonia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(52, "Finland1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(53, "Finland2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(54, "France", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(55, "Georgia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(56, "Germany", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(57, "Ghana", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(58, "Greece", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(59, "Guatemala", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(60, "HongKong1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(61, "HongKong2", "-2 OFCA", 50, "Hop", 400, -1, 50,  921.25 , 923.7, ""));
            frequencySet.Add(new FREQUENCYSET(62, "Hungary1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(63, "Hungary2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(64, "Iceland", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(65, "India", "-1"          , 3, "Fixed", 3900, 100, 600, 865.7, 866.9, ""));
            frequencySet.Add(new FREQUENCYSET(66, "Indonesia", "-7"          , 4, "Hop", 400, -1, 500, 923.75, 924.25, ""));
            frequencySet.Add(new FREQUENCYSET(67, "Iran", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(68, "Ireland1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(69, "Ireland2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(70, "Israel", "-9"          , 3, "Fixed", 3900, -1, 500, 915.5, 916.5, ""));
            frequencySet.Add(new FREQUENCYSET(71, "Italy", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(72, "Jamaica", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(73, "Japan4", "-8 JP4", 4, "Fixed", 3900, -1, 1200, 916.8, 920.4, ""));
            frequencySet.Add(new FREQUENCYSET(74, "Japan6", "-8 JP6", 6, "Fixed", 3900, 100, 1200, 916.8, 920.8, "Channel separation 200 KHz Last 2, LBT carrier sense with Transmission Time Control"));
            frequencySet.Add(new FREQUENCYSET(75, "Jordan", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(76, "Kazakhstan", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(77, "Kenya", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(78, "Korea", "-6	", 6, "Hop", 400, -1, 600, 917.3, 920.3, ""));
            frequencySet.Add(new FREQUENCYSET(79, "KoreaDPR", "-7"          , 16, "Hop", 400, -1, 250, 920.625, 924.375, ""));
            frequencySet.Add(new FREQUENCYSET(80, "Kuwait", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(81, "Kyrgyz", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(82, "Latvia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(83, "Lebanon", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(84, "Libya", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(85, "Liechtenstein1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(86, "Liechtenstein2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(87, "Lithuania1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(88, "Lithuania2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(89, "Luxembourg1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(90, "Luxembourg2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(91, "Macao", "-7"          , 16, "Hop", 400, -1, 250, 920.625, 924.375, ""));
            frequencySet.Add(new FREQUENCYSET(92, "Macedonia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(93, "Malaysia", "-7"          , 6, "Hop", 400, -1, 500, 919.75, 922.25, ""));
            frequencySet.Add(new FREQUENCYSET(94, "Malta1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(95, "Malta2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(96, "Mauritius", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(97, "Mexico", "-2"          , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(98, "Moldova1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(99, "Moldova2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(100, "Mongolia", "-7"          , 16, "Hop", 400, -1, 250, 920.625, 924.375, ""));
            frequencySet.Add(new FREQUENCYSET(101, "Montenegro", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(102, "Morocco", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(103, "Netherlands", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(104, "NewZealand1", "-1"          , 4, "Hop", 400, -1, 500, 864.75, 867.25, ""));
            frequencySet.Add(new FREQUENCYSET(105, "NewZealand2", "-2 NZ"   , 14, "Hop", 400, -1, 500, 920.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(106, "Nicaragua", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(107, "Nigeria", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(108, "Norway1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(109, "Norway2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(110, "Oman", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(111, "Pakistan", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(112, "Panama", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(113, "Paraguay", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(114, "Peru", "-2 RW"       , 24, "Hop", 400, -1, 500, 915.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(115, "Philippines", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(116, "Poland", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(117, "Portugal", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(118, "Romania", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(119, "Russia1", "-1"          , 4, "Fixed", 3900, 100, 600, 866.3, 867.5, "2 W ERP"));
            frequencySet.Add(new FREQUENCYSET(120, "Russia3", "-9"          , 4, "Fixed", 3900, -1, 1200, 915.6, 919.2, "1 W ERP"));
            frequencySet.Add(new FREQUENCYSET(121, "Senegal", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(122, "Serbia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(123, "Singapore1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(124, "Singapore2", "-2 SG"    , 8, "Hop", 400, -1, 500, 920.75, 924.25, ""));
            frequencySet.Add(new FREQUENCYSET(125, "Slovak1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(126, "Slovak2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(127, "Slovenia1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(128, "Solvenia2", "-9"          , 3, "Fixed", 3900, 100, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(129, "SAfrica1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(130, "SAfrica2", "-9"          , 7, "Fixed", 400, -1, 500, 915.7, 918.7, "915.4-919"));
            frequencySet.Add(new FREQUENCYSET(131, "Spain", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(132, "SriLanka", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(133, "Sudan", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(134, "Sweden1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(135, "Sweden2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(136, "Switzerland1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(137, "Switzerland2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(138, "Syria", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(139, "Taiwan1", "-4"        , 12, "Hop", 400, -1, 375, 922.875, 927.000, "1 Watt ERP for Indoor"));
            frequencySet.Add(new FREQUENCYSET(140, "Taiwan2", "-4"        , 12, "Hop", 400, -1, 375, 922.875, 927.000, "0.5 Watt ERP for Outdoor"));
            frequencySet.Add(new FREQUENCYSET(141, "Tajikistan", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(142, "Tanzania", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(143, "Thailand", "-2 RW"       , 8, "Hop", 400, -1, 500, 920.75, 924.25, ""));
            frequencySet.Add(new FREQUENCYSET(144, "Trinidad", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(145, "Tunisia", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(146, "Turkey", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(147, "Turkmenistan", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(148, "Uganda", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(149, "Ukraine", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(150, "UAE", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(151, "UK1", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(152, "UK2", "-9"          , 3, "Fixed", 3900, -1, 1200, 916.3, 918.7, ""));
            frequencySet.Add(new FREQUENCYSET(153, "USA", "-2"          , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(154, "Uruguay", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(155, "Venezuela", "-2 RW"       , 50, "Hop", 400, -1, 500, 902.75, 927.25, ""));
            frequencySet.Add(new FREQUENCYSET(156, "Vietnam1", "-1"          , 4, "Fixed", 3900, 100, 600, 866.7, 868.5, "866-869"));
            frequencySet.Add(new FREQUENCYSET(157, "Vietnam2", "-7"          , 16, "Hop", 400, -1, 250, 920.625, 924.375, ""));
            frequencySet.Add(new FREQUENCYSET(158, "Yemen", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
            frequencySet.Add(new FREQUENCYSET(159, "Zimbabwe", "-1"          , 4, "Fixed", 3900, 100, 600, 865.7, 867.5, ""));
        }
    }
}
