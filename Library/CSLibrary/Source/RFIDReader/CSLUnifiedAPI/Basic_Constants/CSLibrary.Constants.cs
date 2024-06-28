using CSLibrary.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.Constants
{
    /// <summary>
    /// reader Callback Type
    /// </summary>
    public enum ReaderCallbackType
    {
        /// <summary>
        /// Reader connect sucess
        /// </summary>
        CONNECT_SUCESS,

        /// <summary>
        /// RFID Command Error
        /// </summary>
        COMMUNICATION_ERROR,
        
        /// <summary>
        /// BT connection disconnected
        /// </summary>
        CONNECTION_LOST,
        
        /// <summary>
        /// Specific-Tag Searching, Only PC , EPC and RSSI will backscatter
        /// </summary>
        UNKNOWN
    }

    /// <summary>
    /// Callback Type
    /// </summary>
    public enum CallbackType
    {
        /// <summary>
        /// Tag Inventory, Only PC and EPC will backscatter
        /// </summary>
        //TAG_INVENTORY,
        /// <summary>
        /// Specific-Tag Searching, Only PC , EPC and RSSI will backscatter
        /// </summary>
        TAG_SEARCHING,
        /// <summary>
        /// Ranging all tag, Only PC , EPC and RSSI will backscatter
        /// </summary>
        TAG_RANGING,
        /// <summary>
        /// Unknown Type will not happen
        /// </summary>
        UNKNOWN
    }
 
    /// <summary>
    /// RFID device status
    /// </summary>
    public enum RFState
    {
        /// <summary>
        /// Idle mode, ready for any operation.
        /// </summary>
        IDLE,
        /// <summary>
        /// Operation is running, please stop it before do any other operation.
        /// </summary>
        BUSY,
        /// <summary>
        /// Operation is stopping, please wait until back to Idle mode.
        /// </summary>
        ABORT,
        /// <summary>
        /// Reader is required to reset.
        /// </summary>
        RESET,
        /// <summary>
        /// buffer full
        /// </summary>
        SHUTDOWN,
        /// <summary>
        /// Reader is in error stage, please restart reader.
        /// </summary>
        ERROR,
        /// <summary>
        /// Anntenna Cycle End Notification.
        /// </summary>
        ANT_CYCLE_END,
        /// <summary>
        /// Channel hit
        /// </summary>
        CH_BUSY,
        /// <summary>
        /// Channel clear
        /// </summary>
        CH_CLEAR,
        /// <summary>
        /// EAS Alert
        /// </summary>
        EAS_ALARM,
        /// <summary>
        /// Receive Buffer Full
        /// </summary>
        BUFFER_FULL,

        /// <summary>
        /// Inventory MAC Error
        /// </summary>
        INVENTORY_MAC_ERROR,

        /// <summary>
        /// Received Carrier Info
        /// </summary>
        CARRIER_INFO,

        /// <summary>
        /// Inventory Cycle Begin
        /// </summary>
        INVENTORY_CYCLE_BEGIN,

        /// <summary>
        /// Reader initialization complete
        /// </summary>
        INITIALIZATION_COMPLETE,

        /// <summary>
        /// Unknown operation
        /// </summary>
        UNKNOWN
    }

    /// <summary>
    /// Error Type
    /// </summary>
    
    public enum ErrorType : int
    {
        /// <summary>
        /// Unknow
        /// </summary>
        UNKNOWN = -1,
        /// <summary>
        /// General Error
        /// </summary>
        COMMON = 0,
        /// <summary>
        /// Inventory or search error
        /// </summary>
        INVENTORY,
        /// <summary>
        /// Write error
        /// </summary>
        WRITE,
        /// <summary>
        /// Read error
        /// </summary>
        READ,
        /// <summary>
        /// Lock error
        /// </summary>
        LOCK,
        /// <summary>
        /// Kill error
        /// Notes : Kill always reports an error even  kill a tag successfully
        /// </summary>
        KILL,
        /// <summary>
        /// Mac Error
        /// </summary>
        MAC,
    }
    /// <summary>
    /// Operation Error Code
    /// </summary>
    
    public enum ErrorCode : int
    {
        /// <summary>
        /// Unknow error
        /// </summary>
        UNKNOWN = -1,
        /// <summary>
        /// MacError Occurs
        /// </summary>
        MAC_ERROR = 0,
        /// <summary>
        /// Max retry is over
        /// </summary>
        MAX_RETRY_OVER,
        /// <summary>
        /// Written data to target tag is invalid
        /// </summary>
        WRITTEN_DATA_INVALID,
        /// <summary>
        /// function return failure
        /// </summary>
        FUNC_RETURN_FAILED,
        /// <summary>
        /// CRC Invalid
        /// </summary>
        CRC_INVALID,
        /// <summary>
        /// fail to parse packet
        /// </summary>
        PARSE_PKT_ERROR,
        /// <summary>
        /// System.Exception catch
        /// </summary>
        SYSTEM_ERROR,
        /// <summary>
        /// Invalid tag found, ie not Gen2 class-1
        /// </summary>
        INVALID_TAG,
        /// <summary>
        /// can't find specific tag
        /// </summary>
        TAG_NOT_FOUND,
    }

    /// <summary>
    /// Extended Kill command for UHF class 1 gen-2 version 1.2
    /// </summary>
    [Flags]
    public enum ExtendedKillCommand
    {
        /// <summary>
        /// Perform normall Tag Kill command
        /// </summary>
        NORMAL = 0,
        /// <summary>
        /// The Tag shall diable block permalocking and unlock any block of User memory that
        /// were previously permalocked. The Tag shall disable support for the BlockPermalock
        /// command. If the Tag did not implement block permalocking prior to recommissioning
        /// then block permalocking shall remains disabled. The lock status of User memory shall
        /// be determined solely by the lock bits.
        /// </summary>
        DISABLE_PERMALOCK = 1,
        /// <summary>
        /// The Tag shall render its User memory inaccessible, causing the entire memory bank to
        /// become unreadable, unwriteable, unselectable(ie. the Tag functions as though its User memory
        /// bank no longer exits).
        /// </summary>
        DISABLE_USER_MEMORY = 2,
        /// <summary>
        /// The Tag shall unlock its EPC, TID, and User memory banks, regardless of whether these
        /// banks were locked or permalocked. Portions of User memory that were block permalock shall
        /// remain block permalocked, and vice versa, unless the DISABLE_PERMALOCK is also asserted, in which
        /// case the Tag shall unlock its permalocked blocks. The Tag shall write-unlock its kill and
        /// access passwords, and shall render the kill and access passwords permanently unreadable regardless
        /// of the values of the Tag's lock bits. If an Interrogator subsequently attempts to read
        /// the Tag's kill or access passwords the Tag shall backscatter an error code
        /// </summary>
        UNLOCK_ALL_BANKS = 4,
        /// <summary>
        /// Unknown command
        /// </summary>
        UNKNOWN = 0xff
    }

    /// <summary>
    /// Frequency Band State
    /// </summary>
    
    public enum BandState
    {
        /// <summary>
        /// Disable
        /// </summary>
        DISABLE = 0,
        /// <summary>
        /// Enable
        /// </summary>
        ENABLE = 1,
        /// <summary>
        /// Unknow
        /// </summary>
        UNKNOWN = 2
    }
    /// <summary>
    /// LBT Config
    /// </summary>
    
    internal enum LBT : uint
    {
        /// <summary>
        /// LBT OFF
        /// </summary>
        OFF = 0,
        /// <summary>
        /// LBT ON
        /// </summary>
        ON = 1,
        /// <summary>
        /// LBT SCAN MODE
        /// </summary>
        SCAN = 3
    }

/*
    /// <summary>
    /// Region Profile
    /// </summary>

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
*/

	//AC	    Tag mask designer identifier    tag model number    Serial number
	//8 bits	12 bits	                        12 bits	            16 bits
	//E2        001                             093
	/*public enum ISO
    {*/
	//INCITS 256	64	000x xxxx	 	 
	//INCITS 256, 18000-7	32	000x xxxx	000x xxxx	 
	//ISO 18000-7	48	000x xxxx	xxxx xxxx	xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx 
	/*ISO18K7 = 0x11,
	ISO18K2 = 0xE0,
	ISO18K3 = 0xE2,
	ISO18K3 = 0xE2,

ISO 18000-7 (Savi)	48	0001 0001	0000 0100	xxxx xxxx xxxx xxxx xxxx xxxx  xxxx xxxx 
ISO 18000-2, Type A (7816-6)	56 + AC	1110 0000	xxxx xxxx    (per 7816-6)	xxxx xxxx xxxx xxxx xxxx xxxx  xxxx xxxx xxxx xxxx xxxx xxxx 
ISO 18000-3m3 (non-EPC)	16 - 496	1110 0000	xxxx xxxx xxxx - xxxx xxxx xxxx	Maximum 496 bits
ISO 18000-3m3 (EPC)	16 - 496 	1110 0010	xxxx xxxx xxxx -xxxx xxxx xxxx	Maximum 496 bits
ISO 18000-4 (Intermec ¨C 7816-6)	56	1110 0000	xxxx xxxx    (per 7816-6) 	xxxx xxxx xxxx xxxx xxxx xxxx  xxxx xxxx xxxx xxxx xxxx xxxx  
ISO 18000-4 (Intermec ¨C 256)	56	0001 0010	xxxx xxxx  (Unassigned)	xxxx xxxx xxxx xxxx xxxx xxxx  xxxx xxxx xxxx xxxx xxxx xxxx 
ISO 18000-4 (Siemens/Nedap 7816-6) (see 18000-4 for details)	32	N/A	xxxx xxxx    (per 7816-6)	xxxx xxxx xxxx xxxx xxxx xxxx
xxxx xxxx xxxx xxxx xxxx xxxx
ISO 18000-6A	64	1110 0010	xxxx xxxx xxxx - xxxx xxxx xxxx	xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx
ISO 18000-6B	64	0001 0011	xxxx xxxx xxxx - xxxx xxxx xxxx	xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx
ISO 18000-6C (7816)	64	1110 0000	(per 7816-6)	xxxx xxxx xxxx xxxx xxxx xxxx  xxxx xxxx xxxx xxxx xxxx xxxx  
ISO 18000-6C (EPC)	16 - 496 	1110 0010	xxxx xxxx xxxx - xxxx xxxx xxxx	TBD - 18000-6C, Table 1 states a maximum of 496 bits
ISO/IEC 24730	48	0000 0000	xxxx - xxxx	xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx
ISO/IEC 24730 (WhereNet)	48	0000 0000	0000 0000	xxxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx
}*/
	/// <summary>
	/// Allocation Class Identifier
	/// </summary>
	public enum ACID
    {
        /// <summary>
        /// ISO/IEC 7816-6 registration authority
        /// </summary>
        APACS       = 0xE0,
        /// <summary>
        /// ISO/TS 14816 registration authority
        /// </summary>
        NEN         = 0xE1,
        /// <summary>
        /// EPCglobal
        /// </summary>
        GS1         = 0xE2,
        /// <summary>
        /// ISO/IEC 7816-6 registration authority(includes memory size and XTID Header)
        /// </summary>
        APACSX      = 0xE3,
        /// <summary>
        /// Unknown authority
        /// </summary>
        UNKNOWN     = 0xFF
    }
    /// <summary>
    /// EPCglobal Tag Mask Designer Identifier
    /// </summary>
    public enum EpcMDID
    {
        /// <summary>
        /// Impinj
        /// </summary>
        Impinj = 0x1,
        /// <summary>
        /// Texas Instruments
        /// </summary>
        Texas_Instruments = 0x2,
        /// <summary>
        /// Alien Technology
        /// </summary>
        Alien_Technology = 0x3,
        /// <summary>
        /// Intelleflex
        /// </summary>
        Intelleflex = 0x4,
        /// <summary>
        /// Atmel
        /// </summary>
        Atmel = 0x5,
        /// <summary>
        /// NXP
        /// </summary>
        NXP = 0x6,
        /// <summary>
        /// ST Microelectronics
        /// </summary>
        ST_Microelectronics = 0x7,
        /// <summary>
        /// EP Microelectronics
        /// </summary>
        EP_Microelectronics = 0x8,
        /// <summary>
        /// Motorola
        /// </summary>
        Motorola = 0x9,
        /// <summary>
        /// Sentech Snd Bhd
        /// </summary>
        Sentech_Snd_Bhd = 0xA,
        /// <summary>
        /// EM Microelectronics
        /// </summary>
        EM_Microelectronics = 0xB,
        /// <summary>
        /// Renesas Technology Corp
        /// </summary>
        Renesas_Technology_Corp = 0xC,
        /// <summary>
        /// Mstar
        /// </summary>
        Mstar = 0xD,
        /// <summary>
        /// Tyco International
        /// </summary>
        Tyco_International = 0xE,
        /// <summary>
        /// Quanray Electronics
        /// </summary>
        Quanray_Electronics = 0xF,
        /// <summary>
        /// Fujitsu
        /// </summary>
        Fujitsu = 0x10,
        /// <summary>
        /// LSIS
        /// </summary>
        LSIS = 0x11,
        /// <summary>
        /// CAEN RFID srl
        /// </summary>
        CAEN_RFID_srl = 0x12,
        /// <summary>
        /// CAEN Productivity Engineering Gesellschaft fuer IC Design mbH
        /// </summary>
        CAEN_Productivity_Engineering_Gesellschaft_fuer_IC_Design_mbH = 0x13,
        /// <summary>
        /// Impinj with xTid
        /// </summary>
        Impinj_with_xTid = 0x801,
        /// <summary>
        /// Texas Instruments with xTid
        /// </summary>
        Texas_Instruments_with_xTid = 0x802,
        /// <summary>
        /// Alien Technology with xTid
        /// </summary>
        Alien_Technology_with_xTid = 0x803,
        /// <summary>
        /// Intelleflex with xTid
        /// </summary>
        Intelleflex_with_xTid = 0x804,
        /// <summary>
        /// Atmel with xTid
        /// </summary>
        Atmel_with_xTid = 0x805,
        /// <summary>
        /// NXP with xTid
        /// </summary>
        NXP_with_xTid = 0x806,
        /// <summary>
        /// ST Microelectronics with xTid
        /// </summary>
        ST_Microelectronics_with_xTid = 0x807,
        /// <summary>
        /// EP Microelectronics with xTid
        /// </summary>
        EP_Microelectronics_with_xTid = 0x808,
        /// <summary>
        /// Motorola with xTid
        /// </summary>
        Motorola_with_xTid = 0x809,
        /// <summary>
        /// Sentech Snd Bhd with xTid
        /// </summary>
        Sentech_Snd_Bhd_with_xTid = 0x80A,
        /// <summary>
        /// EM Microelectronics with xTid
        /// </summary>
        EM_Microelectronics_with_xTid = 0x80B,
        /// <summary>
        /// Renesas Technology Corp with xTid
        /// </summary>
        Renesas_Technology_Corp_with_xTid = 0x80C,
        /// <summary>
        /// Mstar with xTid
        /// </summary>
        Mstar_with_xTid = 0x80D,
        /// <summary>
        /// Tyco International with xTid
        /// </summary>
        Tyco_International_with_xTid = 0x80E,
        /// <summary>
        /// Quanray Electronics with xTid
        /// </summary>
        Quanray_Electronics_with_xTid = 0x80F,
        /// <summary>
        /// Fujitsu with xTid
        /// </summary>
        Fujitsu_with_xTid = 0x810,
        /// <summary>
        /// LSIS with xTid
        /// </summary>
        LSIS_with_xTid = 0x811,
        /// <summary>
        /// CAEN RFID srl with xTid
        /// </summary>
        CAEN_RFID_srl_with_xTid = 0x812,
        /// <summary>
        /// CAEN Productivity Engineering Gesellschaft fuer IC Design mbH with xTid
        /// </summary>
        CAEN_Productivity_Engineering_Gesellschaft_fuer_IC_Design_mbH_with_xTid = 0x813,
        /// <summary>
        /// UNKNOWN
        /// </summary>
        UNKNOWN = 0xFFFF
    }
    /// <summary>
    /// ISO/IEC 7816-6 registration authority
    /// </summary>
    public enum IsoMDID
    {
        /// <summary>
        /// Motorola
        /// </summary>
        Motorola = 0x1,
        /// <summary>
        /// STMicroelectronics SA
        /// </summary>
        STM,
        /// <summary>
        /// Hitachi, Ltd 
        /// </summary>
        Hitachi,
        /// <summary>
        /// Philips Semiconductors 
        /// </summary>
        Philips,
        /// <summary>
        /// Infineon Technologies AG 
        /// </summary>
        Infineon,
        /// <summary>
        /// Cylink
        /// </summary>
        Cylink,
        /// <summary>
        /// Texas Instrument 
        /// </summary>
        Texas,
        /// <summary>
        /// Fujitsu Limited 
        /// </summary>
        Fujitsu,
        /// <summary>
        /// Matsushita Electronics Corporation, Semiconductor Co.
        /// </summary>
        Matsushita,
        /// <summary>
        /// NEC 
        /// </summary>
        NEC,
        /// <summary>
        /// Oki Electric Industry Co. Ltd 
        /// </summary>
        Oki,
        /// <summary>
        /// Toshiba Corp. 
        /// </summary>
        Toshiba,
        /// <summary>
        /// Mitsubishi Electric Corp. 
        /// </summary>
        Mitsubishi,
        /// <summary>
        /// Samsung Electronics Co. Ltd 
        /// </summary>
        Samsung,
        /// <summary>
        /// Hynix
        /// </summary>
        Hynix,
        /// <summary>
        /// LG-Semiconductors Co. Ltd 
        /// </summary>
        LG,
        /// <summary>
        /// Emosyn-EM Microelectronics 
        /// </summary>
        EmosynEM,
        /// <summary>
        /// INSIDE Technology 
        /// </summary>
        INSIDE,
        /// <summary>
        /// ORGA Kartensysteme GmbH 
        /// </summary>
        ORGA,
        /// <summary>
        /// SHARP Corporation 
        /// </summary>
        SHARP,
        /// <summary>
        /// ATMEL 
        /// </summary>
        ATMEL,
        /// <summary>
        /// EM Microelectronic-Marin SA 
        /// </summary>
        EM,
        /// <summary>
        /// KSW Microtec GmbH 
        /// </summary>
        KSW,
        /// <summary>
        /// ZMD AG 
        /// </summary>
        ZMD,
        /// <summary>
        /// XICOR, Inc. 
        /// </summary>
        XICOR,
        /// <summary>
        /// Sony Corporation 
        /// </summary>
        Sony,
        /// <summary>
        /// Malaysia Microelectronic Solutions Sdn. Bhd 
        /// </summary>
        Malaysia,
        /// <summary>
        /// Emosyn 
        /// </summary>
        Emosyn,
        /// <summary>
        /// Shanghai Fudan Microelectronics Co. Ltd. 
        /// </summary>
        Fudan,
        /// <summary>
        /// Magellan Technology Pty Limited 
        /// </summary>
        Magellan,
        /// <summary>
        /// Melexis NV BO 
        /// </summary>
        Melexis,
        /// <summary>
        /// Renesas Technology Corp. 
        /// </summary>
        Renesas,
        /// <summary>
        /// TAGSYS 
        /// </summary>
        TAGSYS,
        /// <summary>
        /// Transcore
        /// </summary>
        Transcore,
        /// <summary>
        /// Shanghai Belling Corp., Ltd. 
        /// </summary>
        Belling,
        /// <summary>
        /// Masktech Germany Gmbh 
        /// </summary>
        Masktech,
        /// <summary>
        /// Innovision Research and Techology Plc 
        /// </summary>
        Innovision,
        /// <summary>
        /// Hitachi ULSI Systems Co., Ltd. 
        /// </summary>
        HitachiULSI,
        /// <summary>
        /// Cypak AB 
        /// </summary>
        Cypak,
        /// <summary>
        /// Ricoh
        /// </summary>
        Ricoh,
        /// <summary>
        /// ASK 
        /// </summary>
        ASK,
        /// <summary>
        /// Unicore Microsystems, LLC 
        /// </summary>
        Unicore,
        /// <summary>
        /// Dallas Semiconductor/Maxim 
        /// </summary>
        Dallas,
        /// <summary>
        /// Impinj, Inc. 
        /// </summary>
        Impinj,
        /// <summary>
        /// RightPlug Alliance 
        /// </summary>
        Alliance,
        /// <summary>
        /// Broadcom Corporation 
        /// </summary>
        Broadcom,
        /// <summary>
        /// MStar Semiconductor, Inc 
        /// </summary>
        MStar,
        /// <summary>
        /// eeDar Technology Inc. 
        /// </summary>
        eeDar,
        /// <summary>
        /// RFIDsec
        /// </summary>
        RFIDsec,
        /// <summary>
        /// Schweizer Electronic AG 
        /// </summary>
        Schweizer,
        /// <summary>
        /// AMIC Technology Corp 
        /// </summary>
        AMIC,
        /// <summary>
        /// Unknown company
        /// </summary>
        UNKNOWN = 0xFFFF
    }

    /// <summary>
    /// RFID Operation Mode
    /// </summary>
    public enum Operation : int
    {
        /// <summary>
        /// Unknow operation
        /// </summary>
        UNKNOWN = -1,
        /// <summary>
        /// perform 18K6C tag read to target tag
        /// </summary>
        TAG_READ,
        /// <summary>
        /// perform 18K6C tag write to target tag
        /// </summary>
        TAG_WRITE,
        /// <summary>
        /// perform 18K6C BlockPermalock to target tag
        /// </summary>
        TAG_BLOCK_PERMALOCK,
        /// <summary>
        /// perform 18K6C tag lock to target tag
        /// </summary>
        TAG_LOCK,
        /// <summary>
        /// perform 18K6C tag kill to target tag
        /// </summary>
        TAG_KILL,
        /*/// <summary>
        /// perform 18K6C block write to target tag
        /// </summary>
        TAG_BLOCK_WRITE,*/
        /// <summary>
        /// perform custom read on kill password
        /// </summary>
        TAG_READ_KILL_PWD,
        /// <summary>
        /// perform custom read on access password
        /// </summary>
        TAG_READ_ACC_PWD,
        /// <summary>
        /// perform custom read on pc
        /// </summary>
        TAG_READ_PC,
        /// <summary>
        /// perform custom read on epc
        /// </summary>
        TAG_READ_EPC,
        /// <summary>
        /// perform custom read on tid
        /// </summary>
        TAG_READ_TID,
        /// <summary>
        /// perform custom read on user bank
        /// </summary>
        TAG_READ_USER,
        /// <summary>
        /// perform custom write on kill password
        /// </summary>
        TAG_WRITE_KILL_PWD,
        /// <summary>
        /// perform custom write on access password
        /// </summary>
        TAG_WRITE_ACC_PWD,
        /// <summary>
        /// perform custom write on pc
        /// </summary>
        TAG_WRITE_PC,
        /// <summary>
        /// perform custom write on epc
        /// </summary>
        TAG_WRITE_EPC,
        /// <summary>
        /// perform custom write on user bank 
        /// </summary>
        TAG_WRITE_USER,
        /// <summary>
        /// perform 18K6C block write to target tag
        /// </summary>
        TAG_BLOCK_WRITE,
        /// <summary>
        /// perform custom ranging on any tags
        /// </summary>
        TAG_RANGING,
        /// <summary>
        /// perform custom ranging on any tags
        /// </summary>
        TAG_PRERANGING,
        /// <summary>
        /// perform custom ranging on any tags
        /// </summary>
        TAG_EXERANGING,
        /// <summary>
        /// perform custom search specific tags
        /// </summary>
        TAG_SEARCHING,
        /// <summary>
        /// perform custom search specific tags
        /// </summary>
        TAG_PRESEARCHING,
        /// <summary>
        /// perform custom search specific tags
        /// </summary>
        TAG_EXESEARCHING,
        /*
        /// <summary>
        /// perform custom lock access password
        /// </summary>
        TAG_LOCK_ACC_PWD,
        /// <summary>
        /// perform custom lock kill password
        /// </summary>
        TAG_LOCK_KILL_PWD,
        /// <summary>
        /// perform custom lock EPC bank
        /// </summary>
        TAG_LOCK_EPC,
        /// <summary>
        /// perform custom lock TID bank
        /// </summary>
        TAG_LOCK_TID,
        /// <summary>
        /// perform custom lock USER bank
        /// </summary>
        TAG_LOCK_USER,*/
        /// <summary>
        /// Select a tag for operation
        /// </summary>
        TAG_SELECTED,
        /// <summary>
        /// Select a tag using dynamic Q for operation
        /// </summary>
        TAG_SELECTEDDYNQ,
        /// <summary>
        /// perform read protect custom command on specific tags
        /// </summary>
        TAG_READ_PROTECT,
        /// <summary>
        /// perform reset read protect custom search on specific tags
        /// </summary>
        TAG_RESET_READ_PROTECT,
        /// <summary>
        /// configure EAS
        /// </summary>
        EAS_CONFIG,
        /// <summary>
        /// EAS Alarm
        /// </summary>
        EAS_ALARM,
        /// <summary>
        /// Set password
        /// </summary>
        CL_SET_PASSWORD,
        /// <summary>
        /// Set log mode
        /// </summary>
        CL_SET_LOG_MODE,
        /// <summary>
        /// Set log limits
        /// </summary>
        CL_SET_LOG_LIMITS,
        /// <summary>
        /// Get measurement setup
        /// </summary>
        CL_GET_MEASUREMENT_SETUP,
        /// <summary>
        /// Set DFE parameter
        /// </summary>
        CL_SET_SFE_PARA,
        /// <summary>
        /// Set cal data
        /// </summary>
        CL_SET_CAL_DATA,
        /// <summary>
        /// End log
        /// </summary>
        CL_END_LOG,
        /// <summary>
        /// Start log
        /// </summary>
        CL_START_LOG,
        /// <summary>
        /// get log state
        /// </summary>
        CL_GET_LOG_STATE,
        /// <summary>
        /// Get cal data
        /// </summary>
        CL_GET_CAL_DATA,
        /// <summary>
        /// Get battery level
        /// </summary>
        CL_GET_BAT_LV,
        /// <summary>
        /// Set shelf life
        /// </summary>
        CL_SET_SHELF_LIFE,
        /// <summary>
        /// Init
        /// </summary>
        CL_INIT,
        /// <summary>
        /// Get sensor value
        /// </summary>
        CL_GET_SENSOR_VALUE,
        /// <summary>
        /// Open area
        /// </summary>
        CL_OPEN_AREA,
        /// <summary>
        /// Set access FIFO
        /// </summary>
        CL_ACCESS_FIFO,
        /// <summary>
        /// G2X ReadProtect
        /// </summary>
        G2_READ_PROTECT,
        /// <summary>
        /// G2X Reset ReadProtect
        /// </summary>
        G2_RESET_READ_PROTECT,
        /// <summary>
        /// G2X ChangeEAS
        /// </summary>
        G2_CHANGE_EAS,
        /// <summary>
        /// G2X EAS Alarm
        /// </summary>
        G2_EAS_ALARM,
        /// <summary>
        /// G2iL Change Config
        /// </summary>
        G2_CHANGE_CONFIG,
        /// <summary>
        /// QT Command
        /// </summary>
        QT_COMMAND,
        /// <summary>
        /// Custom EM Get Sensor Data
        /// </summary>
        EM4324_GetUid,
        /// <summary>
        /// Custom EM Get Sensor Data
        /// </summary>
        EM4325_GetUid,
        /// <summary>
        /// Custom EM Get Sensor Data
        /// </summary>
        EM_GetSensorData,
        /// <summary>
        /// Custom EM Get Sensor Data
        /// </summary>
        EM_ResetAlarms,
        /// <summary>
        /// SPI Command
        /// </summary>
        EM_SPI,
        /// <summary>
        /// SPI Command
        /// </summary>
        EM_SPIRequestStatus,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIBoot,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPITransponder,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIGetSensorData,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPISetFlags,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIReadWord,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIWriteWord,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIReadPage,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIWritePage,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPISetClock,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIAlarm,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIReadRegisterFileWord,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIWriteRegisterFileWord,
        /// <summary>
        /// EM4325 Command
        /// </summary>
        EM_SPIReqRN,

        TAG_GENERALSELECTED,

        TAG_FASTSELECTED,

        TAG_PREFILTER,

        TAG_POSTFILTER,

        TAG_AUTHENTICATE,

        TAG_READBUFFER,

        TAG_UNTRACEABLE,

        // for FM13DT160 
        FM13DT_READMEMORY,
        FM13DT_WRITEMEMORY,
        FM13DT_AUTH,
        FM13DT_GETTEMP,
        FM13DT_STARTLOG,
        FM13DT_STOPLOG,
        FM13DT_WRITEREGISTER,
        FM13DT_READREGISTER,
        FM13DT_DEEPSLEEP,
        FM13DT_OPMODECHK,
        FM13DT_INITIALREGFILE,
        FM13DT_LEDCTRL,

        /// <summary>
        /// for Kiloway Tag
        /// </summary>
        Kiloway_RANGING,
    }
    /// <summary>
    /// Memory bank
    /// </summary>
    public enum Bank
    {
        /// <summary>
        /// Access password
        /// </summary>
        ACC_PWD,
        /// <summary>
        /// Kill password
        /// </summary>
        KILL_PWD,
        /// <summary>
        /// Protocol Control
        /// </summary>
        PC,
        /// <summary>
        /// Electronic Product Code
        /// </summary>
        EPC,
        /// <summary>
        /// Transponder ID
        /// </summary>
        TID,
        /// <summary>
        /// User memory
        /// </summary>
        USER,
        /// <summary>
        /// Any bank memory
        /// </summary>
        SPECIFIC,
        /// <summary>
        /// Untraceable
        /// </summary>
        UNTRACEABLE,
        /// <summary>
        /// Unknown bank
        /// </summary>
        UNKNOWN
    }

    /// <summary>
    /// The values that can be found in the command field for tag access packets
    /// </summary>
    public enum TagAccess : byte
    {
        /// <summary>
        /// ISO 18000-6C Read
        /// </summary>
        READ = 0xC2,
        /// <summary>
        /// ISO 18000-6C Write
        /// </summary>
        WRITE = 0xC3,
        /// <summary>
        /// ISO 18000-6C Kill
        /// </summary>
        KILL = 0xC4,
        /// <summary>
        /// ISO 18000-6C Lock
        /// </summary>
        LOCK = 0xC5,
        /// <summary>
        /// ISO 18000-6C Erase
        /// </summary>
        ERASE = 0xC6,
        /// <summary>
        /// UCODE DNA Authenticate
        /// </summary>
        AUTHENTICATE = 0x01,
        /// <summary>
        /// UCODE DNA Untracable
        /// </summary>
        UNTRACEABLE = 0x02,
        /// <summary>
        /// Unknown
        /// </summary>
        UNKNOWN = 0xff
    }

    public enum FM13DTAccess
    {
        READMEMORY,
        WRITEMEMORY,
        READREGISTER,
        WRITEREGISTER,
        AUTH,
        GETTEMP,
        STARTLOG,
        STOPLOG,
        DEEPSLEEP,
        OPMODECHK,
        INITIALREGFILE,
        LEDCTRL,
        UNKNOWN
    }

    /// <summary>
    /// Machine Type (phase out), please use CSLibrary.DEVICEINFORMATION.MODEL
    /// </summary>
    /// 
/*
    public enum Machine
    {
        CS101 = 0,
        CS108,
        CS203,
        CS333,
        CS468,
        CS206_OLD,
        CS468INT,
        CS463,
        CS469,
        CS208,
        CS209,
        CS103,
        CS206,
        CS468X,
        CS203X,
        CS468XJ,
        CS710S,
        MACHINE_CODE_END,
        UNKNOWN = 0xff
    }
*/

    public enum ChipSetID
    {
        R1000 = 0,
        R2000 = 1,
        E710 = 2,
        UNKNOWN = 0xff
    }

    public enum ApiMode
    {
        HIGHLEVEL,
        LOWLEVEL,
        UNKNOWN
    }

}
