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

using CSLibrary;
using CSLibrary.Constants;
using CSLibrary.Structures;
using CSLibrary.Events;
using CSLibrary.Tools;


namespace CSLibrary
{
    public partial class RFIDReader
    {
        /// <summary>
        /// REGISTER NAME/ADDRESS CONSTANTS
        /// </summary>
        public enum MACREGISTER : UInt16
        {
            MAC_VER = 0x0000,
            MAC_INFO = 0x0001,
            MAC_RFTRANSINFO = 0x0002,
            MAC_DBG1 = 0x0003,
            MAC_DBG2 = 0x0004,
            MAC_ERROR = 0x0005,

            HST_ENGTST_ARG0 = 0x0100,
            HST_ENGTST_ARG1 = 0x0101,
            HST_DBG1 = 0x0102,
            HST_EMU = 0x0103,

            FM13DT160_CMDCFGPAR = 0x117,
            FM13DT160_REGADDRPAR = 0x118,
            FM13DT160_WRITEPAR = 0x119,
            FM13DT160_PWDPAR = 0x11a,
            FM13DT160_STOBLOCKADDPAR = 0x11b,
            FM13DT160_STARTADDRPAR = 0x11c,
            FM13DT160_READWRITELENPAR = 0x11d,
            FM13DT160_DATAPAR = 0x11e,

            EM4325_CFG = 0x11f,

            HST_PWRMGMT = 0x0200,
            HST_CMNDIAGS = 0x0201,
            MAC_BLK02RES1 = 0x0202,
            HST_IMPINJ_EXTENSIONS = 0x0203,
            HST_CTR1_CFG = 0x0204,
            MAC_CTR1_VAL = 0x0205,
            HST_CTR2_CFG = 0x0206,
            MAC_CTR2_VAL = 0x0207,
            HST_CTR3_CFG = 0x0208,
            MAC_CTR3_VAL = 0x0209,
            HST_CTR4_CFG = 0x020A,
            MAC_CTR4_VAL = 0x020B,

            HST_PROTSCH_SMIDX = 0x0300,
            HST_PROTSCH_SMCFG = 0x0301,
            HST_PROTSCH_FTIME_SEL = 0x0302,
            HST_PROTSCH_FTIME = 0x0303,
            HST_PROTSCH_SMCFG_SEL = 0x0304,
            HST_PROTSCH_TXTIME_SEL = 0x0305,
            HST_PROTSCH_TXTIME_ON = 0x0306,
            HST_PROTSCH_TXTIME_OFF = 0x0307,
            HST_PROTSCH_CYCCFG_SEL = 0x0308,
            HST_PROTSCH_CYCCFG_DESC_ADJ1 = 0x0309,
            HST_PROTSCH_ADJCW = 0x030A,

            HST_MBP_ADDR = 0x0400,
            HST_MBP_DATA = 0x0401,
            HST_MBP_RFU_0x0402 = 0x0402,
            HST_MBP_RFU_0x0403 = 0x0403,
            HST_MBP_RFU_0x0404 = 0x0404,
            HST_MBP_RFU_0x0405 = 0x0405,
            HST_MBP_RFU_0x0406 = 0x0406,
            HST_MBP_RFU_0x0407 = 0x0407,
            HST_LPROF_SEL = 0x0408,
            HST_LPROF_ADDR = 0x0409,
            HST_LPROF_DATA = 0x040A,

            HST_OEM_ADDR = 0x0500,
            HST_OEM_DATA = 0x0501,

            HST_GPIO_INMSK = 0x0600,
            HST_GPIO_OUTMSK = 0x0601,
            HST_GPIO_OUTVAL = 0x0602,
            HST_GPIO_CFG = 0x0603,

            HST_ANT_CYCLES = 0x0700,
            HST_ANT_DESC_SEL = 0x0701,
            HST_ANT_DESC_CFG = 0x0702,
            MAC_ANT_DESC_STAT = 0x0703,
            HST_ANT_DESC_PORTDEF = 0x0704,
            HST_ANT_DESC_DWELL = 0x0705,
            HST_ANT_DESC_RFPOWER = 0x0706,
            HST_ANT_DESC_INV_CNT = 0x0707,

            HST_TAGMSK_DESC_SEL = 0x0800,
            HST_TAGMSK_DESC_CFG = 0x0801,
            HST_TAGMSK_BANK = 0x0802,
            HST_TAGMSK_PTR = 0x0803,
            HST_TAGMSK_LEN = 0x0804,
            HST_TAGMSK_0_3 = 0x0805,
            HST_TAGMSK_4_7 = 0x0806,
            HST_TAGMSK_8_11 = 0x0807,
            HST_TAGMSK_12_15 = 0x0808,
            HST_TAGMSK_16_19 = 0x0809,
            HST_TAGMSK_20_23 = 0x080A,
            HST_TAGMSK_24_27 = 0x080B,
            HST_TAGMSK_28_31 = 0x080C,

            HST_QUERY_CFG = 0x0900,
            HST_INV_CFG = 0x0901,
            HST_INV_SEL = 0x0902,
            HST_INV_ALG_PARM_0 = 0x0903,
            HST_INV_ALG_PARM_1 = 0x0904,
            HST_INV_ALG_PARM_2 = 0x0905,
            HST_INV_ALG_PARM_3 = 0x0906,
            HST_INV_RSSI_FILTERING_CONFIG = 0x0907,
            HST_INV_RSSI_FILTERING_THRESHOLD = 0x0908,
            HST_INV_RFU_0x0909 = 0x0909,
            HST_INV_RFU_0x090A = 0x090A,
            HST_INV_RFU_0x090B = 0x090B,
            HST_INV_RFU_0x090C = 0x090C,
            HST_INV_RFU_0x090D = 0x090D,
            HST_INV_RFU_0x090E = 0x090E,
            HST_INV_RFU_0x090F = 0x090F,
            HST_INV_EPC_MATCH_SEL = 0x0910,
            HST_INV_EPC_MATCH_CFG = 0x0911,
            HST_INV_EPCDAT_0_3 = 0x0912,
            HST_INV_EPCDAT_4_7 = 0x0913,
            HST_INV_EPCDAT_8_11 = 0x0914,
            HST_INV_EPCDAT_12_15 = 0x0915,
            HST_INV_EPCDAT_16_19 = 0x0916,
            HST_INV_EPCDAT_20_23 = 0x0917,
            HST_INV_EPCDAT_24_27 = 0x0918,
            HST_INV_EPCDAT_28_31 = 0x0919,
            HST_INV_EPCDAT_32_35 = 0x091A,
            HST_INV_EPCDAT_36_39 = 0x091B,
            HST_INV_EPCDAT_40_43 = 0x091C,
            HST_INV_EPCDAT_44_47 = 0x091D,
            HST_INV_EPCDAT_48_51 = 0x091E,
            HST_INV_EPCDAT_52_55 = 0x091F,
            HST_INV_EPCDAT_56_59 = 0x0920,
            HST_INV_EPCDAT_60_63 = 0x0921,

            HST_TAGACC_DESC_SEL = 0x0A00,
            HST_TAGACC_DESC_CFG = 0x0A01,
            HST_TAGACC_BANK = 0x0A02,
            HST_TAGACC_PTR = 0x0A03,
            HST_TAGACC_CNT = 0x0A04,
            HST_TAGACC_LOCKCFG = 0x0A05,
            HST_TAGACC_ACCPWD = 0x0A06,
            HST_TAGACC_KILLPWD = 0x0A07,
            HST_TAGWRDAT_SEL = 0x0A08,
            HST_TAGWRDAT_0 = 0x0A09,
            HST_TAGWRDAT_1 = 0x0A0A,
            HST_TAGWRDAT_2 = 0x0A0B,
            HST_TAGWRDAT_3 = 0x0A0C,
            HST_TAGWRDAT_4 = 0x0A0D,
            HST_TAGWRDAT_5 = 0x0A0E,
            HST_TAGWRDAT_6 = 0x0A0F,
            HST_TAGWRDAT_7 = 0x0A10,
            HST_TAGWRDAT_8 = 0x0A11,
            HST_TAGWRDAT_9 = 0x0A12,
            HST_TAGWRDAT_10 = 0x0A13,
            HST_TAGWRDAT_11 = 0x0A14,
            HST_TAGWRDAT_12 = 0x0A15,
            HST_TAGWRDAT_13 = 0x0A16,
            HST_TAGWRDAT_14 = 0x0A17,
            HST_TAGWRDAT_15 = 0x0A18,

            MAC_RFTC_PAPWRLEV = 0x0B00,
            HST_RFTC_PAPWRCTL_PGAIN = 0x0B01,
            HST_RFTC_PAPWRCTL_IGAIN = 0x0B02,
            HST_RFTC_PAPWRCTL_DGAIN = 0x0B03,
            MAC_RFTC_REVPWRLEV = 0x0B04,
            HST_RFTC_REVPWRTHRSH = 0x0B05,
            MAC_RFTC_AMBIENTTEMP = 0x0B06,
            HST_RFTC_AMBIENTTEMPTHRSH = 0x0B07,
            MAC_RFTC_XCVRTEMP = 0x0B08,
            HST_RFTC_XCVRTEMPTHRSH = 0x0B09,
            MAC_RFTC_PATEMP = 0x0B0A,
            HST_RFTC_PATEMPTHRSH = 0x0B0B,
            HST_RFTC_PADELTATEMPTHRSH = 0x0B0C,
            HST_RFTC_PAPWRCTL_AIWDELAY = 0x0B0D,
            MAC_RFTC_PAPWRCTL_STAT0 = 0x0B0E,
            MAC_RFTC_PAPWRCTL_STAT1 = 0x0B0F,
            MAC_RFTC_PAPWRCTL_STAT2 = 0x0B10,
            MAC_RFTC_PAPWRCTL_STAT3 = 0x0B11,
            HST_RFTC_ANTSENSRESTHRSH = 0x0B12,
            HST_RFTC_IFLNAAGCRANGE = 0x0B13,
            MAC_RFTC_LAST_ANACTRL1 = 0x0B14,
            HST_RFTC_OPENLOOPPWRCTRL = 0x0B15,
            HST_RFTC_RFU_0x0B16 = 0x0B16,
            HST_RFTC_RFU_0x0B17 = 0x0B17,
            HST_RFTC_RFU_0x0B18 = 0x0B18,
            HST_RFTC_RFU_0x0B19 = 0x0B19,
            HST_RFTC_PREDIST_COEFF0 = 0x0B1A,
            HST_RFTC_RFU_0x0B1B = 0x0B1B,
            HST_RFTC_RFU_0x0B1C = 0x0B1C,
            HST_RFTC_RFU_0x0B1D = 0x0B1D,
            HST_RFTC_RFU_0x0B1E = 0x0B1E,
            HST_RFTC_RFU_0x0B1F = 0x0B1F,
            HST_RFTC_CAL_GGNEG7 = 0x0B20,
            HST_RFTC_CAL_GGNEG5 = 0x0B21,
            HST_RFTC_CAL_GGNEG3 = 0x0B22,
            HST_RFTC_CAL_GGNEG1 = 0x0B23,
            HST_RFTC_CAL_GGPLUS1 = 0x0B24,
            HST_RFTC_CAL_GGPLUS3 = 0x0B25,
            HST_RFTC_CAL_GGPLUS5 = 0x0B26,
            HST_RFTC_CAL_GGPLUS7 = 0x0B27,
            HST_RFTC_CAL_MACADCREFV = 0x0B28,
            HST_RFTC_CAL_RFFWDPWR_C0 = 0x0B29,
            HST_RFTC_CAL_RFFWDPWR_C1 = 0x0B2A,
            HST_RFTC_CAL_RFFWDPWR_C2 = 0x0B2B,
            HST_RFTC_RFU_0x0B2C = 0x0B2C,
            HST_RFTC_RFU_0x0B2D = 0x0B2D,
            HST_RFTC_RFU_0x0B2E = 0x0B2E,
            HST_RFTC_RFU_0x0B2F = 0x0B2F,
            HST_RFTC_CLKDBLR_CFG = 0x0B30,
            HST_RFTC_CLKDBLR_SEL = 0x0B31,
            HST_RFTC_CLKDBLR_LUTENTRY = 0x0B32,
            HST_RFTC_RFU_0x0B33 = 0x0B33,
            HST_RFTC_RFU_0x0B34 = 0x0B34,
            HST_RFTC_RFU_0x0B35 = 0x0B35,
            HST_RFTC_RFU_0x0B36 = 0x0B36,
            HST_RFTC_RFU_0x0B37 = 0x0B37,
            HST_RFTC_RFU_0x0B38 = 0x0B38,
            HST_RFTC_RFU_0x0B39 = 0x0B39,
            HST_RFTC_RFU_0x0B3A = 0x0B3A,
            HST_RFTC_RFU_0x0B3B = 0x0B3B,
            HST_RFTC_RFU_0x0B3C = 0x0B3C,
            HST_RFTC_RFU_0x0B3D = 0x0B3D,
            HST_RFTC_RFU_0x0B3E = 0x0B3E,
            HST_RFTC_RFU_0x0B3F = 0x0B3F,
            HST_RFTC_FRQHOPMODE = 0x0B40,
            HST_RFTC_FRQHOPENTRYCNT = 0x0B41,
            HST_RFTC_FRQHOPTABLEINDEX = 0x0B42,
            MAC_RFTC_HOPCNT = 0x0B43,
            HST_RFTC_MINHOPDUR = 0x0B44,
            HST_RFTC_MAXHOPDUR = 0x0B45,
            HST_RFTC_FRQHOPRANDSEED = 0x0B46,
            MAC_RFTC_FRQHOPSHFTREGVAL = 0x0B47,
            MAC_RFTC_FRQHOPRANDNUMCNT = 0x0B48,
            HST_RFTC_FRQCHINDEX = 0x0B49,
            HST_RFTC_PLLLOCKTIMEOUT = 0x0B4A,
            HST_RFTC_PLLLOCK_DET_THRSH = 0x0B4B,
            HST_RFTC_PLLLOCK_DET_CNT = 0x0B4C,
            HST_RFTC_PLLLOCK_TO = 0x0B4D,
            HST_RFTC_BERREADDELAY = 0x0B4E,
            HST_RFTC_RFU_0x0B4F = 0x0B4F,
            MAC_RFTC_FWDRFPWRRAWADC = 0x0B50,
            MAC_RFTC_REVRFPWRRAWADC = 0x0B51,
            MAC_RFTC_ANTSENSERAWADC = 0x0B52,
            MAC_RFTC_AMBTEMPRAWADC = 0x0B53,
            MAC_RFTC_PATEMPRAWADC = 0x0B54,
            MAC_RFTC_XCVRTEMPRAWADC = 0x0B55,
            HST_RFTC_RFU_0x0B56 = 0x0B56,
            HST_RFTC_RFU_0x0B57 = 0x0B57,
            HST_RFTC_RFU_0x0B58 = 0x0B58,
            HST_RFTC_RFU_0x0B59 = 0x0B59,
            HST_RFTC_RFU_0x0B5A = 0x0B5A,
            HST_RFTC_RFU_0x0B5B = 0x0B5B,
            HST_RFTC_RFU_0x0B5C = 0x0B5C,
            HST_RFTC_RFU_0x0B5D = 0x0B5D,
            HST_RFTC_RFU_0x0B5E = 0x0B5E,
            HST_RFTC_RFU_0x0B5F = 0x0B5F,
            HST_RFTC_CURRENT_PROFILE = 0x0B60,
            HST_RFTC_PROF_SEL = 0x0B61,
            MAC_RFTC_PROF_CFG = 0x0B62,
            MAC_RFTC_PROF_ID_HIGH = 0x0B63,
            MAC_RFTC_PROF_ID_LOW = 0x0B64,
            MAC_RFTC_PROF_IDVER = 0x0B65,
            MAC_RFTC_PROF_PROTOCOL = 0x0B66,
            MAC_RFTC_PROF_R2TMODTYPE = 0x0B67,
            MAC_RFTC_PROF_TARI = 0x0B68,
            MAC_RFTC_PROF_X = 0x0B69,
            MAC_RFTC_PROF_PW = 0x0B6A,
            MAC_RFTC_PROF_RTCAL = 0x0B6B,
            MAC_RFTC_PROF_TRCAL = 0x0B6C,
            MAC_RFTC_PROF_DIVIDERATIO = 0x0B6D,
            MAC_RFTC_PROF_MILLERNUM = 0x0B6E,
            MAC_RFTC_PROF_T2RLINKFREQ = 0x0B6F,
            MAC_RFTC_PROF_VART2DELAY = 0x0B70,
            MAC_RFTC_PROF_RXDELAY = 0x0B71,
            MAC_RFTC_PROF_MINTOTT2DELAY = 0x0B72,
            MAC_RFTC_PROF_TXPROPDELAY = 0x0B73,
            MAC_RFTC_PROF_RSSIAVECFG = 0x0B74,
            MAC_RFTC_PROF_PREAMCMD = 0x0B75,
            MAC_RFTC_PROF_FSYNCCMD = 0x0B76,
            MAC_RFTC_PROF_T2WAITCMD = 0x0B77,
            HST_RFTC_RFU_0x0B78 = 0x0B78,
            HST_RFTC_RFU_0x0B79 = 0x0B79,
            HST_RFTC_RFU_0x0B7A = 0x0B7A,
            HST_RFTC_RFU_0x0B7B = 0x0B7B,
            HST_RFTC_RFU_0x0B7C = 0x0B7C,
            HST_RFTC_RFU_0x0B7D = 0x0B7D,
            HST_RFTC_RFU_0x0B7E = 0x0B7E,
            HST_RFTC_RFU_0x0B7F = 0x0B7F,
            HST_RFTC_RFU_0x0B80 = 0x0B80,
            HST_RFTC_RFU_0x0B81 = 0x0B81,
            HST_RFTC_RFU_0x0B82 = 0x0B82,
            HST_RFTC_RFU_0x0B83 = 0x0B83,
            HST_RFTC_RFU_0x0B84 = 0x0B84,

            HST_RFTC_FRQCH_ENTRYCNT = 0x0C00,
            HST_RFTC_FRQCH_SEL = 0x0C01,
            HST_RFTC_FRQCH_CFG = 0x0C02,
            HST_RFTC_FRQCH_DESC_PLLDIVMULT = 0x0C03,
            HST_RFTC_FRQCH_DESC_PLLDACCTL = 0x0C04,
            MAC_RFTC_FRQCH_DESC_PLLLOCKSTAT0 = 0x0C05,
            MAC_RFTC_FRQCH_DESC_PLLLOCKSTAT1 = 0x0C06,
            HST_RFTC_FRQCH_DESC_PARFU3 = 0x0C07,
            HST_RFTC_FRQCH_CMDSTART = 0x0C08,

            // for Ucode DNA Tag
            AUTHENTICATE_CFG = 0x0F00,
            AUTHENTICATE_MSG0 = 0x0F01,
            AUTHENTICATE_MSG1 = 0x0F02,
            AUTHENTICATE_MSG2 = 0x0F03,
            AUTHENTICATE_MSG3 = 0x0F04,
            READBUFFER_PTR = 0x0A03,
            READBUFFER_LEN = 0x0A04,
            UNTRACEABLE_CFG = 0x0F05,

            INV_CYCLE_DELAY = 0x0F0F,

            HST_CMD = 0xF000
        }

        internal enum HST_CMD : uint
        {
            NV_MEM_UPDATE = 0x00000001, // Enter NV MEMORY UPDATE mode
            WROEM = 0x00000002, // Write OEM Configuration Area
            RDOEM = 0x00000003, // Read OEM Configuration Area
            ENGTST1 = 0x00000004, // Engineering Test Command #1
            MBPRDREG = 0x00000005, // R1000 firmware by-pass Read Register
            MBPWRREG = 0x00000006, // R1000 firmware by-pass Write Register
            RDGPIO = 0x0000000C, // Read GPIO
            WRGPIO = 0x0000000D, // Write GPIO
            CFGGPIO = 0x0000000E, // Configure GPIO
            INV = 0x0000000F, // ISO 18000-6C Inventory
            READ = 0x00000010, // ISO 18000-6C Read
            WRITE = 0x00000011, // ISO 18000-6C Write
            LOCK = 0x00000012, // ISO 18000-6C Lock
            KILL = 0x00000013, // ISO 18000-6C Kill
            SETPWRMGMTCFG = 0x00000014, // Set Power Management Configuration
            CLRERR = 0x00000015, // Clear Error
            CWON = 0x00000017, // Engineering CMD: Powers up CW
            CWOFF = 0x00000018, // Engineering CMD: Powers down CW
            UPDATELINKPROFILE = 0x00000019, // Changes the Link Profile
            CALIBRATE_GG = 0x0000001B, // Calibrate gross-gain settings
            LPROF_RDXCVRREG = 0x0000001C, // Read R1000 reg associated with given link profile
            LPROF_WRXCVRREG = 0x0000001D, // Write R1000 reg associated with given link profile
            BLOCKERASE = 0x0000001e, // ISO 18000-6C block erase
            BLOCKWRITE = 0x0000001f, // ISO 18000-6C block write
            POPULATE_SPURWATABLE = 0x00000020, // populate a local copy of the spur workaround table
            POPRFTCSENSLUTS = 0x00000021, // map the ADC readings to sensor-appropriate units
            BLOCKPERMALOCK,
            CUSTOMM4QT,
            CUSTOMG2XREADPROTECT,
            CUSTOMG2XRESETREADPROTECT,
            CUSTOMG2XCHANGEEAS,
            CUSTOMG2XEASALARM,
            CUSTOMG2XCHANGECONFIG,
            CUSTOMSLSETPASSWORD,
            CUSTOMSLSETLOGMODE,
            CUSTOMSLSETLOGLIMITS,
            CUSTOMSLGETMEASUREMENTSETUP,
            CUSTOMSLSETSFEPARA,
            CUSTOMSLSETCALDATA,
            CUSTOMSLENDLOG,
            CUSTOMSLSTARTLOG,
            CUSTOMSLGETLOGSTATE,
            CUSTOMSLGETCALDATA,
            CUSTOMSLGETBATLV,
            CUSTOMSLSETSHELFLIFE,
            CUSTOMSLINIT,
            CUSTOMSLGETSENSORVALUE,
            CUSTOMSLOPENAREA,
            CUSTOMSLACCESSFIFO,
            CUSTOMEM4324GETUID,
            CUSTOMEM4325GETUID,
            CUSTOMEMGETSENSORDATA = 0x3B,
            CUSTOMEMRESETALARMS,
            CUSTOMEMSENDSPI,
            AUTHENTICATE = 0x50,
            READBUFFER = 0x51,
            UNTRACEABLE = 0x52,
            CUSTOMMFM13DTREADMEMORY = 0x00000053,
	        CUSTOMMFM13DTWRITEMEMORY = 0x00000054,
	        CUSTOMMFM13DTAUTH = 0x00000055,
	        CUSTOMMFM13DTGETTEMP = 0x00000056,
	        CUSTOMMFM13DTSTARTLOG = 0x00000057,
	        CUSTOMMFM13DTSTOPLOG = 0x00000058,
	        CUSTOMMFM13DTWRITEREG = 0x00000059,
	        CUSTOMMFM13DTREADREG = 0x0000005A,
	        CUSTOMMFM13DTDEEPSLEEP = 0x0000005B,
	        CUSTOMMFM13DTOPMODECHK = 0x0000005C,
	        CUSTOMMFM13DTINITIALREGFILE = 0x0000005D,
	        CUSTOMMFM13DTLEDCTRL = 0x0000005E,
            CMD_END
        }

        /*
        private const int BYTES_PER_LEN_UNIT = 4;

        private const uint INVALID_POWER_VALUE = uint.MaxValue;
        private const uint INVALID_PROFILE_VALUE = uint.MaxValue;
        private const int DATA_FIELD_INDEX = 20;
        private const int RSSI_FIELD_INDEX = 12;
        private const int ANT_FIELD_INDEX = 14;
        private const int MS_FIELD_INDEX = 8;
        private const int RFID_PACKET_COMMON_SIZE = 8;

        private const ushort PC_START_OFFSET = 1;
        private const ushort PC_WORD_LENGTH = 1;
        private const ushort EPC_START_OFFSET = 2;
        private const ushort EPC_WORD_LENGTH = 6;
        private const ushort ACC_PWD_START_OFFSET = 2;
        private const ushort ACC_PWD_WORD_LENGTH = 2;
        private const ushort KILL_PWD_START_OFFSET = 0;
        private const ushort KILL_PWD_WORD_LENGTH = 2;
        private const ushort ONE_WORD_LEN = 1;
        private const ushort TWO_WORD_LEN = 2;

        private const ushort USER_WORD_LENGTH = 1;
        private const uint MAXFRECHANNEL = 50;

        private Result CurrentOperationResult;

        internal CSLibraryOperationParms m_rdr_opt_parms = new CSLibraryOperationParms();
        */

        void Start18K6CRequest_CS108(uint tagStopCount, SelectFlags flags)
        {
            // Set up the rest of the HST_INV_CFG register.  First, we have to read its
            // current value
            UInt32 registerValue = 0;

            MacReadRegister(MACREGISTER.HST_INV_CFG, ref registerValue);
            registerValue &= ~0x0000FFC0U;  // reserver bit 0:5 ~ 16:31

            // TBD - an optimization could be to only write back the register if
            // the value changes

            // Set the tag stop count and enabled flags and then write the register
            // back
            if ((flags & SelectFlags.SELECT) != 0)
            {
                registerValue |= (1 << 14);
            }
            if ((flags & SelectFlags.DISABLE_INVENTORY) != 0)
            {
                registerValue |= (1 << 15);
            }
            registerValue |= tagStopCount << 6;
            MacWriteRegister(MACREGISTER.HST_INV_CFG, registerValue);

            // Set the enabled flag in the HST_INV_EPC_MATCH_CFG register properly.  To
            // do so, have to read the register value first.  Then set the bit properly
            // and then write the register value back to the MAC.
            MacReadRegister(MACREGISTER.HST_INV_EPC_MATCH_CFG, ref registerValue);
            if ((flags & SelectFlags.POST_MATCH) != 0)
            {
                registerValue |= 0x01;
            }
            else
            {
                registerValue &= ~(uint)0x01; ;
            }

            MacWriteRegister(MACREGISTER.HST_INV_EPC_MATCH_CFG, registerValue);
        } // Radio::Start18K6CRequest

        /*
				public int CUST_18K6CTagWrite(uint bank, uint offset, uint count, UInt16[] data, uint accessPassword, uint retry, SelectFlags flags)
				{
					// Perform the common 18K6C tag operation setup
					Start18K6CRequest(retry, flags);

					Setup18K6CReadRegisters(bank, offset, count);

					// Set up the access password register
					MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, accessPassword);

					// Issue the read command
					_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.WRITE), (UInt32)SENDREMARK.EXECCMD);

					return 0;
				}
		*/

        private void TagLockThreadProc_CS108()
        {
            const uint HST_TAGACC_LOCKCFG_MASK_USE_PWD_ACTION = 0x1;
            const uint HST_TAGACC_LOCKCFG_MASK_USE_PERMA_ACTION = 0x2;

            /* HST_TAGACC_LOCKCFG register helper macros                                */
            /* The size of the bit fields in the HST_TAGACC_LOCKCFG register.           */
            const byte HST_TAGACC_LOCKCFG_ACTION_USER_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_ACTION_TID_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_ACTION_EPC_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_ACTION_ACC_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_ACTION_KILL_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_MASK_USER_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_MASK_TID_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_MASK_EPC_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_MASK_ACC_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_MASK_KILL_SIZE = 2;
            const byte HST_TAGACC_LOCKCFG_RFU1_SIZE = 12;

            const byte HST_TAGACC_LOCKCFG_ACTION_USER_SHIFT = 0;
            const byte HST_TAGACC_LOCKCFG_ACTION_TID_SHIFT = (HST_TAGACC_LOCKCFG_ACTION_USER_SHIFT + HST_TAGACC_LOCKCFG_ACTION_USER_SIZE);
            const byte HST_TAGACC_LOCKCFG_ACTION_EPC_SHIFT = (HST_TAGACC_LOCKCFG_ACTION_TID_SHIFT + HST_TAGACC_LOCKCFG_ACTION_TID_SIZE);
            const byte HST_TAGACC_LOCKCFG_ACTION_ACC_SHIFT = (HST_TAGACC_LOCKCFG_ACTION_EPC_SHIFT + HST_TAGACC_LOCKCFG_ACTION_EPC_SIZE);
            const byte HST_TAGACC_LOCKCFG_ACTION_KILL_SHIFT = (HST_TAGACC_LOCKCFG_ACTION_ACC_SHIFT + HST_TAGACC_LOCKCFG_ACTION_ACC_SIZE);
            const byte HST_TAGACC_LOCKCFG_MASK_USER_SHIFT = (HST_TAGACC_LOCKCFG_ACTION_KILL_SHIFT + HST_TAGACC_LOCKCFG_ACTION_KILL_SIZE);
            const byte HST_TAGACC_LOCKCFG_MASK_TID_SHIFT = (HST_TAGACC_LOCKCFG_MASK_USER_SHIFT + HST_TAGACC_LOCKCFG_MASK_USER_SIZE);
            const byte HST_TAGACC_LOCKCFG_MASK_EPC_SHIFT = (HST_TAGACC_LOCKCFG_MASK_TID_SHIFT + HST_TAGACC_LOCKCFG_MASK_TID_SIZE);
            const byte HST_TAGACC_LOCKCFG_MASK_ACC_SHIFT = (HST_TAGACC_LOCKCFG_MASK_EPC_SHIFT + HST_TAGACC_LOCKCFG_MASK_EPC_SIZE);
            const byte HST_TAGACC_LOCKCFG_MASK_KILL_SHIFT = (HST_TAGACC_LOCKCFG_MASK_ACC_SHIFT + HST_TAGACC_LOCKCFG_MASK_ACC_SIZE);
            const byte HST_TAGACC_LOCKCFG_RFU1_SHIFT = (HST_TAGACC_LOCKCFG_MASK_KILL_SHIFT + HST_TAGACC_LOCKCFG_MASK_KILL_SIZE);

            /* Constants for HST_TAGACC_LOCKCFG register bit fields (note that the      */
            /* values are already shifted into the low-order bits of the constant.      */
            const uint HST_TAGACC_LOCKCFG_ACTION_MEM_WRITE = 0x0;
            const uint HST_TAGACC_LOCKCFG_ACTION_MEM_PERM_WRITE = 0x1;
            const uint HST_TAGACC_LOCKCFG_ACTION_MEM_SEC_WRITE = 0x2;
            const uint HST_TAGACC_LOCKCFG_ACTION_MEM_NO_WRITE = 0x3;
            const uint HST_TAGACC_LOCKCFG_ACTION_PWD_ACCESS = 0x0;
            const uint HST_TAGACC_LOCKCFG_ACTION_PWD_PERM_ACCESS = 0x1;
            const uint HST_TAGACC_LOCKCFG_ACTION_PWD_SEC_ACCESS = 0x2;
            const uint HST_TAGACC_LOCKCFG_ACTION_PWD_NO_ACCESS = 0x3;
            const uint HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION = 0x0;

            const uint HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION = (HST_TAGACC_LOCKCFG_MASK_USE_PWD_ACTION | HST_TAGACC_LOCKCFG_MASK_USE_PERMA_ACTION);

            const uint RFID_18K6C_TAG_PWD_PERM_ACCESSIBLE = 0x0;
            const uint RFID_18K6C_TAG_PWD_PERM_ALWAYS_NOT_ACCESSIBLE = 0x1;
            const uint RFID_18K6C_TAG_PWD_PERM_ALWAYS_ACCESSIBLE = 0x2;
            const uint RFID_18K6C_TAG_PWD_PERM_SECURED_ACCESSIBLE = 0x3;
            const uint RFID_18K6C_TAG_PWD_PERM_NO_CHANGE = 0x4;

            const uint RFID_18K6C_TAG_MEM_PERM_WRITEABLE = 0x0;             //unlock		00
            const uint RFID_18K6C_TAG_MEM_PERM_ALWAYS_NOT_WRITEABLE = 0x1;  //permlock		01
            const uint RFID_18K6C_TAG_MEM_PERM_ALWAYS_WRITEABLE = 0x2;      //permunlock	10
            const uint RFID_18K6C_TAG_MEM_PERM_SECURED_WRITEABLE = 0x3;     //lock			11
            const uint RFID_18K6C_TAG_MEM_PERM_NO_CHANGE = 0x4;


            m_Result = Result.FAILURE;

            UInt32 registerValue = 0;

            Start18K6CRequest(1, m_rdr_opt_parms.TagLock.flags);

            if (m_rdr_opt_parms.TagLock.permanentLock)
            {
                registerValue = 0xfffff;
            }
            else
            {

                if (RFID_18K6C_TAG_PWD_PERM_NO_CHANGE == (uint)m_rdr_opt_parms.TagLock.killPasswordPermissions)
                {
                    registerValue |= (HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_KILL_SHIFT);
                }
                // Otherwise, indicate to look at the kill password bits and set the
                // persmission for it
                else
                {
                    registerValue |= (HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_KILL_SHIFT);
                    registerValue |= ((uint)m_rdr_opt_parms.TagLock.killPasswordPermissions << HST_TAGACC_LOCKCFG_ACTION_KILL_SHIFT);
                }

                // If the access password access permissions are not to change, then
                // indicate to ignore those bits.
                if (RFID_18K6C_TAG_PWD_PERM_NO_CHANGE == (uint)m_rdr_opt_parms.TagLock.accessPasswordPermissions)
                {
                    registerValue |= HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_ACC_SHIFT;
                }
                // Otherwise, indicate to look at the access password bits and set the
                // persmission for it
                else
                {
                    registerValue |= HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_ACC_SHIFT;
                    registerValue |= (uint)m_rdr_opt_parms.TagLock.accessPasswordPermissions << HST_TAGACC_LOCKCFG_ACTION_ACC_SHIFT;
                }

                // If the EPC memory access permissions are not to change, then indicate
                // to ignore those bits.
                if (RFID_18K6C_TAG_MEM_PERM_NO_CHANGE == (uint)m_rdr_opt_parms.TagLock.epcMemoryBankPermissions)
                {
                    registerValue |= HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_EPC_SHIFT;
                }
                // Otherwise, indicate to look at the EPC memory bits and set the
                // persmission for it
                else
                {
                    registerValue |= HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_EPC_SHIFT;
                    registerValue |= (uint)m_rdr_opt_parms.TagLock.epcMemoryBankPermissions << HST_TAGACC_LOCKCFG_ACTION_EPC_SHIFT;
                }

                // If the TID memory access permissions are not to change, then indicate
                // to ignore those bits.
                if (RFID_18K6C_TAG_MEM_PERM_NO_CHANGE == (uint)m_rdr_opt_parms.TagLock.tidMemoryBankPermissions)
                {
                    registerValue |= HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_TID_SHIFT;
                }
                // Otherwise, indicate to look at the TID memory bits and set the
                // persmission for it
                else
                {
                    registerValue |= HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_TID_SHIFT;
                    registerValue |= (uint)m_rdr_opt_parms.TagLock.tidMemoryBankPermissions << HST_TAGACC_LOCKCFG_ACTION_TID_SHIFT;
                }

                // If the user memory access permissions are not to change, then indicate
                // to ignore those bits.
                if (RFID_18K6C_TAG_MEM_PERM_NO_CHANGE == (uint)m_rdr_opt_parms.TagLock.userMemoryBankPermissions)
                {
                    registerValue |= HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_USER_SHIFT;
                }
                // Otherwise, indicate to look at the user memory bits and set the
                // persmission for it
                else
                {
                    registerValue |= HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_USER_SHIFT;
                    registerValue |= (uint)m_rdr_opt_parms.TagLock.userMemoryBankPermissions << HST_TAGACC_LOCKCFG_ACTION_USER_SHIFT;
                }
            }

            // Set up the lock configuration register
            MacWriteRegister(MACREGISTER.HST_TAGACC_LOCKCFG, registerValue);

            // Set up the access password register
            MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, m_rdr_opt_parms.TagLock.accessPassword);

            // Set up the HST_TAGACC_DESC_CFG register (controls the verify and retry
            // count) and write it to the MAC
            //m_pMac->WriteRegister(HST_TAGACC_DESC_CFG, HST_TAGACC_DESC_CFG_RETRY(0));

            // Issue the lock command
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.LOCK), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (UInt32)CurrentOperation);
        }

        private void TagBlockLockThreadProc_CS108()
        {
            const UInt32 RFID_NUM_TAGWRDAT_REGS_PER_BANK = 16;

            // Perform the common 18K6C tag operation setup
            //this->Start18K6CRequest(&pBWParms->common, flags);
            Start18K6CRequest(1, Options.TagBlockLock.flags);

            // Set the tag access descriptor to the first one just to be safe
            MacWriteRegister(MACREGISTER.HST_TAGACC_DESC_SEL, 0);

            // Set the tag write data select register to zero
            MacWriteRegister(MACREGISTER.HST_TAGWRDAT_SEL, 0x0000);

            // Set up the HST_TAGACC_DESC_CFG register (controls the verify and retry
            // count) and write it to the MAC
            /*INT32U  registerValue = 
            (pBWParms->verify ? HST_TAGACC_DESC_CFG_VERIFY_ENABLED :
            HST_TAGACC_DESC_CFG_VERIFY_DISABLED)  |
            HST_TAGACC_DESC_CFG_RETRY(pBWParms->verifyRetryCount)     | 
            HST_TAGACC_DESC_CFG_RFU1(0);
            m_pMac->WriteRegister(HST_TAGACC_DESC_CFG, registerValue);*/

            //INT16U count = pBWParms->permalockCmdParms.count;
            //INT16U offset = pBWParms->permalockCmdParms.offset;
            //BOOL32 readOrLock = pBWParms->permalockCmdParms.readOrLock;
            //const INT16U* pData = pBWParms->permalockCmdParms.pData;

            // Set up the tag bank register (tells where to write the data)
            MacWriteRegister(MACREGISTER.HST_TAGACC_BANK, 0x03);

            //Set up the access offset register (i.e., number of words to lock)
            MacWriteRegister(MACREGISTER.HST_TAGACC_PTR, Options.TagBlockLock.offset);

            // Set up the access count register (i.e., number of words to lock)
            MacWriteRegister(MACREGISTER.HST_TAGACC_CNT, Options.TagBlockLock.count);

            // Set up the tag access password
            MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, Options.TagBlockLock.accessPassword);

            MacWriteRegister(MACREGISTER.HST_TAGACC_LOCKCFG, (Options.TagBlockLock.setPermalock ? (1U << 20) : 0x0000U));

            UInt16 count = 0;
            UInt16 offset = Options.TagBlockLock.offset;

            if (Options.TagBlockLock.setPermalock)
            {
                // Set up the HST_TAGWRDAT_N registers.  Fill up a bank at a time.
                for (UInt32 registerBank = 0; count < Options.TagBlockLock.count; ++registerBank)
                {
                    // Indicate which bank of tag write registers we are going to fill
                    MacWriteRegister(MACREGISTER.HST_TAGWRDAT_SEL, registerBank);

                    /*
                    if (HOSTIF_ERR_SELECTORBNDS == MacReadRegister(MAC_ERROR))
                    {
                        this->ClearMacError();
                        throw RfidErrorException(RFID_ERROR_INVALID_PARAMETER);
                    }
                    */

                    // Write the values to the bank until either the bank is full or we get to
                    // a point where we cannot fill a register (i.e., we have 0 or 1 words left)
                    offset = 0;
                    for (; (offset < RFID_NUM_TAGWRDAT_REGS_PER_BANK) && (count < (Options.TagBlockLock.count - 1)); ++offset)
                    {
                        MacWriteRegister((MACREGISTER)((int)MACREGISTER.HST_TAGWRDAT_0 + offset), (uint)((Options.TagBlockLock.mask[count] << 16) | Options.TagBlockLock.mask[count + 1]));
                        count += 2;
                    }

                    // If we didn't use all registers in the bank and count is non-zero, it means
                    // that the request was for an odd number of words to be written.  Make sure
                    // that the last word is written.
                    if ((offset < RFID_NUM_TAGWRDAT_REGS_PER_BANK) && (count < Options.TagBlockLock.count))
                    {
                        MacWriteRegister((MACREGISTER)((int)MACREGISTER.HST_TAGWRDAT_0 + offset), (uint)((Options.TagBlockLock.mask[count] << 16)));
                        //MacWriteRegister(MacRegister.HST_TAGWRDAT_0 + offset, HST_TAGWRDAT_N_DATA0(*pData) | HST_TAGWRDAT_N_DATA1(0));
                        break;
                    }
                }
            }

            // Issue the write command to the MAC
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.BLOCKPERMALOCK), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE);
            //m_Result = COMM_HostCommand(HST_CMD.BLOCKPERMALOCK);


            /*			if (m_Result == Result.OK && !Options.TagBlockLock.setPermalock)
						{
							Options.TagBlockLock.mask = new ushort[Options.TagBlockLock.count];
							Array.Copy(tagreadbuf, Options.TagBlockLock.mask, Options.TagBlockLock.count);
						}
			*/
        }

        ////////////////////////////////////////////////////////////////////////////////
        // Name: RFID_18K6CTagKill
        //
        // Description:
        //   Executes a tag kill for the tags of interest.  If the
        //   RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        //   partitioned (i.e., ISO 18000-6C select) prior to the tag-kill operation.
        //   If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        //   match mask is applied to a singulated tag's EPC to determine if the tag
        //   will be killed.  The operation-response packets will be returned to the
        //   application via the application-supplied callback function.  Each tag-kill
        //   record is grouped with its corresponding tag-inventory record.  An
        //   application may prematurely stop a kill operation by calling
        //   RFID_Radio{Cancel|Aobrt}Operation on another thread or by returning a non-
        //   zero value from the callback function.
        ////////////////////////////////////////////////////////////////////////////////
        private bool RFID_18K6CTagKill_CS108()
        {
            // Perform the common 18K6C tag operation setup
            Start18K6CRequest(1, m_rdr_opt_parms.TagKill.flags);

            // Set up the access password register
            MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, m_rdr_opt_parms.TagKill.accessPassword);

            // Set up the kill password register
            MacWriteRegister(MACREGISTER.HST_TAGACC_KILLPWD, m_rdr_opt_parms.TagKill.killPassword);

            // Set up the kill extended register
            MacWriteRegister(MACREGISTER.HST_TAGACC_LOCKCFG, (0x7U & (uint)m_rdr_opt_parms.TagKill.extCommand) << 21);

            // Set up the HST_TAGACC_DESC_CFG register (controls the verify and retry
            // count) and write it to the MAC
            //m_pMac->WriteRegister(HST_TAGACC_DESC_CFG, HST_TAGACC_DESC_CFG_RETRY(7));

            // Issue the kill command
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.KILL), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.TAG_KILL);
            //if (COMM_HostCommand(HST_CMD.KILL) != Result.OK || CurrentOperationResult != Result.OK)
            //	return false;

            return true;
        } // RFID_18K6CTagKill

        private void TagKillThreadProc_CS108()
        {
            ushort[] tmp = new ushort[1];

            RFID_18K6CTagKill_CS108();

#if oldcode
            if (RFID_18K6CTagKill())
            {
                if (CUST_18K6CTagRead(
                    MemoryBank.EPC,
                    EPC_START_OFFSET,
                    1,
                    tmp,
                    m_rdr_opt_parms.TagKill.accessPassword,
                    //m_rdr_opt_parms.TagKill.retryCount,
                    SelectFlags.SELECT) != true)
                {
                    //can't read mean killed
                    m_Result = Result.OK;
                    return;
                }
            }
            //FireAccessCompletedEvent(new OnAccessCompletedEventArgs(m_Result == Result.OK, Bank.UNKNOWN, TagAccess.KILL, null));
            //FireStateChangedEvent(RFState.IDLE);
#endif
        }

        private void TagAuthenticateThreadProc_CS108()
        {
            UInt32 value = 0;
            UInt32[] m_data;

            if (m_rdr_opt_parms.TagAuthenticate.Message.Length > 32)
            {
                m_Result = Result.INVALID_PARAMETER;
                return;
            }


            // Set up the access password register
            MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, m_rdr_opt_parms.TagAuthenticate.password);
            value |= (UInt32)m_rdr_opt_parms.TagAuthenticate.SenRep & 0x01;
            value |= ((UInt32)m_rdr_opt_parms.TagAuthenticate.IncRepLen & 0x01) << 1;
            value |= ((UInt32)m_rdr_opt_parms.TagAuthenticate.CSI & 0xff) << 2;
            value |= ((UInt32)m_rdr_opt_parms.TagAuthenticate.Length & 0xFFF) << 10;

            string NewMessage = m_rdr_opt_parms.TagAuthenticate.Message + new String('0', 32 - m_rdr_opt_parms.TagAuthenticate.Message.Length);

            m_data = CSLibrary.Tools.Hex.ToUInt32s(NewMessage);

            MacWriteRegister(MACREGISTER.AUTHENTICATE_CFG, value);
            MacWriteRegister(MACREGISTER.AUTHENTICATE_MSG0, m_data[0]);
            MacWriteRegister(MACREGISTER.AUTHENTICATE_MSG1, m_data[1]);
            MacWriteRegister(MACREGISTER.AUTHENTICATE_MSG2, m_data[2]);
            MacWriteRegister(MACREGISTER.AUTHENTICATE_MSG3, m_data[3]);

            Start18K6CRequest(1, CSLibrary.Constants.SelectFlags.SELECT);

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.AUTHENTICATE), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (UInt32)CurrentOperation);
            m_Result = Result.OK;
            return;
        }

        private void TagReadBufferThreadProc_CS108()
        {
            MacWriteRegister(MACREGISTER.READBUFFER_PTR, m_rdr_opt_parms.TagReadBuffer.Offset);
            MacWriteRegister(MACREGISTER.READBUFFER_LEN, (UInt32)(m_rdr_opt_parms.TagReadBuffer.Length & 0xfff));

            Start18K6CRequest(1, CSLibrary.Constants.SelectFlags.SELECT);

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.READBUFFER), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE);
            m_Result = Result.OK;
            return;
        }

        private void TagUntraceableThreadProc_CS108()
        {
            UInt32 value = 0;

            if (m_rdr_opt_parms.TagUntraceable.EPCLength >= 32)
            {
                m_Result = Result.INVALID_PARAMETER;
                return;
            }

            value |= (UInt32)m_rdr_opt_parms.TagUntraceable.Range;
            value |= ((UInt32)m_rdr_opt_parms.TagUntraceable.User << 2);
            value |= ((UInt32)m_rdr_opt_parms.TagUntraceable.TID << 3);
            value |= ((UInt32)m_rdr_opt_parms.TagUntraceable.EPC << 10);
            value |= ((UInt32)m_rdr_opt_parms.TagUntraceable.EPCLength << 5);
            value |= ((UInt32)m_rdr_opt_parms.TagUntraceable.U << 11);

            MacWriteRegister( MACREGISTER.UNTRACEABLE_CFG, value);

            Start18K6CRequest(1, CSLibrary.Constants.SelectFlags.SELECT);

            // Issue the untraceable command
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.UNTRACEABLE), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (UInt32)CurrentOperation);
            m_Result = Result.OK;
            return;
        }


        private bool FM13DTReadMemoryThreadProc_CS108()
        {
            if (m_rdr_opt_parms.FM13DTReadMemory.offset > 0xffff)
                return false;

            FM13DT160_ReadMemory(m_rdr_opt_parms.FM13DTReadMemory.offset, m_rdr_opt_parms.FM13DTReadMemory.count);
            return true;
        }

        private bool FM13DTWriteMemoryThreadProc_CS108()
        {
            if (m_rdr_opt_parms.FM13DTWriteMemory.offset > 0xffff)
                return false;

            FM13DT160_WriteMemory(m_rdr_opt_parms.FM13DTWriteMemory.offset, m_rdr_opt_parms.FM13DTWriteMemory.data);
            return true;
        }

        private bool FM13DTReadRegThreadProc_CS108()
        {
            if (m_rdr_opt_parms.FM13DTReadMemory.offset > 0xffff)
                return false;

            FM13DT160_ReadMemory(m_rdr_opt_parms.FM13DTReadMemory.offset, m_rdr_opt_parms.FM13DTReadMemory.count);
            return true;
        }

        private bool FM13DTWriteRegThreadProc_CS108()
        {
            if (m_rdr_opt_parms.FM13DTWriteMemory.offset > 0xffff)
                return false;

            FM13DT160_WriteMemory(m_rdr_opt_parms.FM13DTWriteMemory.offset, m_rdr_opt_parms.FM13DTWriteMemory.data);
            return true;
        }

        private bool FM13DTAuthThreadProc_CS108()
        {
//            FM13DT160_Auth(m_rdr_opt_parms.FM FM13DTWriteMemory.offset, m_rdr_opt_parms.FM13DTWriteMemory.count);
            return true;
        }

        private bool FM13DTGetTempThreadProc_CS108()
        {
            //FM13DT160_GetTemp(m_rdr_opt_parms.FM13DTWriteMemory.offset, m_rdr_opt_parms.FM13DTWriteMemory.count, m_rdr_opt_parms.FM13DTWriteMemory.data);
            return true;
        }
        private bool FM13DTStartLogThreadProc_CS108()
        {
            FM13DT160_StartLog();
            return true;
        }
        private bool FM13DTStopLogChkThreadProc_CS108()
        {
            FM13DT160_StopLog(m_rdr_opt_parms.FM13DTWriteMemory.offset);
            return true;
        }
        private bool FM13DTDeepSleepThreadProc_CS108()
        {
            FM13DT160_DeepSleep(true);
            return true;
        }
        private bool FM13DTOpModeChkThreadProc_CS108()
        {
            FM13DT160_OpModeChk(m_rdr_opt_parms.FM13DTOpModeChk.enable);
            return true;
        }
        private bool FM13DTInitialRegFileThreadProc_CS108()
        {
            FM13DT160_InitialRegFile();
            return true;
        }

        private bool FM13DTLedCtrlThreadProc_CS108()
        {
            FM13DT160_LedCtrl(m_rdr_opt_parms.FM13DTLedCtrl.enable);
            return true;
        }

        /// <summary>
        /// rfid reader packet
        /// </summary>
        /// <param name="RW"></param>
        /// <param name="add"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal byte[] PacketData_CS108(UInt16 add, UInt32? value = null)
        {
            byte[] CMDBuf = new byte[8];

            if (value == null)
            {
                CMDBuf[1] = 0x00;
                CMDBuf[4] = 0x00;
                CMDBuf[5] = 0x00;
                CMDBuf[6] = 0x00;
                CMDBuf[7] = 0x00;
            }
            else
            {
                CMDBuf[1] = 0x01;
                CMDBuf[4] = (byte)value;
                CMDBuf[5] = (byte)(value >> 8);
                CMDBuf[6] = (byte)(value >> 16);
                CMDBuf[7] = (byte)(value >> 24);
            }

            CMDBuf[0] = 0x70;
            CMDBuf[2] = (byte)add;
            CMDBuf[3] = (byte)((uint)add >> 8);

            return CMDBuf;
        }

        private Result MacReadOemData(UInt32 address, ref UInt32 value)
        {
            MacWriteRegister(MACREGISTER.HST_OEM_ADDR, address);

            // Issue read OEM command
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.RDOEM), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1_COMMANDENDRESPONSE, (UInt32)0xffffffff);

            return Result.OK;
        }

        private Result MacWriteOemData(uint address, uint value)
        {
            MacWriteRegister(MACREGISTER.HST_OEM_ADDR, address);
            MacWriteRegister(MACREGISTER.HST_OEM_DATA, value);

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.WROEM), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (UInt32)0xffffffff);

            return Result.OK;
        }

        private Result MacWriteOemData(uint address, uint [] value)
        {
            return Result.CURRENTLY_NOT_ALLOWED;
        }
    }
}
