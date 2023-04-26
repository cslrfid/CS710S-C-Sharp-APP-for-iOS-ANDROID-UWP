using System;
using System.Collections.Generic;
using System.Text;

using CSLibrary.Barcode.Constants;

namespace CSLibrary.Barcode.Structures
{
    using OCR_T         =  SymCodeOCR;
    using AZTEC_T       =  SymFlagsRange;
    using CODABAR_T     =  SymFlagsRange;
    using CODE11_T      =  SymFlagsRange;
    using CODE128_T     =  SymFlagsRange;
    using CODE39_T      =  SymFlagsRange;
    using CODE49_T      =  SymFlagsRange;
    using CODE93_T      =  SymFlagsRange;
    using COMPOSITE_T   =  SymFlagsRange;
    using DATAMATRIX_T  =  SymFlagsRange;
    using INT25_T       =  SymFlagsRange;
    using MAXICODE_T    =  SymFlagsRange;
    using MICROPDF_T    =  SymFlagsRange;
    using PDF417_T      =  SymFlagsRange;
    using QR_T          =  SymFlagsRange;
    using RSS_T         =  SymFlagsRange;
    using IATA25_T      =  SymFlagsRange;
    using CODABLOCK_T   =  SymFlagsRange;
    using MSI_T        =   SymFlagsRange;
    using MATRIX25_T   =   SymFlagsRange;
    using KORPOST_T    =   SymFlagsRange;
    using CODE25_T     =   SymFlagsRange;
    using PLESSEY_T    =   SymFlagsRange;
    using CHINAPOST_T  =   SymFlagsRange;
    using TELEPEN_T    =   SymFlagsRange;
    using CODE16K_T   =    SymFlagsRange;
    using POSICODE_T  =    SymFlagsRange;
    using MESA_T        =  SymFlagsOnly;
    using EAN8_T        =  SymFlagsOnly;
    using EAN13_T       =  SymFlagsOnly;
    using POSTNET_T     =  SymFlagsOnly;
    using UPCA_T        =  SymFlagsOnly;
    using UPCE_T        =  SymFlagsOnly;
    using ISBT_T        =  SymFlagsOnly;
    using BPO_T         =  SymFlagsOnly;
    using CANPOST_T     =  SymFlagsOnly;
    using AUSPOST_T     =  SymFlagsOnly;
    using JAPOST_T      =  SymFlagsOnly;
    using PLANET_T      =  SymFlagsOnly;
    using DUTCHPOST_T   =  SymFlagsOnly;
    using TLCODE39_T   =   SymFlagsOnly;
    using TRIOPTIC_T   =   SymFlagsOnly;
    using CODE32_T     =   SymFlagsOnly;
    using COUPONCODE_T =    SymFlagsOnly;
    using UPUIDTAG_T   =   SymFlagsOnly;
    using CODE4CB_T     =  SymFlagsOnly;

    /// <summary>
    /// Image Size
    /// </summary>
    public struct SIZE
    {
        /// <summary>
        /// width
        /// </summary>
        public int width;
        /// <summary>
        /// height
        /// </summary>
        public int height;
    }

    /// <summary>
    /// Image Size
    /// </summary>
    public struct RECT
    {
        /// <summary>
        /// left position
        /// </summary>
        public Int32 left;
        /// <summary>
        /// top position
        /// </summary>
        public Int32 top;
        /// <summary>
        /// right position
        /// </summary>
        public Int32 right;
        /// <summary>
        /// bottom position
        /// </summary>
        public Int32 bottom;
    }

    #region Message
    ///<summary>
    /// Image information structure
    ///</summary>
    public class ImageMessage : MessageBase
    {
        /// <summary>
        /// Pointer Buffer for image
        /// </summary>
        public IntPtr puchBuffer;
        /// <summary>
        /// Size of buffer in bytes
        /// </summary>
        public Int32 nBufferSize;
        /// <summary>
        /// Image Format for returned data
        /// </summary>
        public ImgFormat imageFormat;
        /// <summary>
        /// JPEG Quality Factor, default 80%
        /// </summary>
        public uint dwJpegQFactor = 80;
        /// <summary>
        /// Number of bytes returned.
        /// </summary>
        public Int32 nBytesReturned;
        /// <summary>
        /// Size of image returned.
        /// </summary>
        public SIZE imgSize;
        /// <summary>
        /// Number of frames captured prior to this image
        /// </summary>
        public Int32 nCapturedFrames;
        /// <summary>
        /// Gain value used to capture this image
        /// </summary>
        public Int32 nGain;
        /// <summary>
        /// Exposure time used to capture this image
        /// </summary>
        public Int32 nExposureTime;
        /// <summary>
        /// Number of underexposed pixels in image
        /// </summary>
        public Int32 nUnderexposedPixels;
        /// <summary>
        /// Number of overexposed pixels in image
        /// </summary>
        public Int32 nOverexposedPixels;
        /// <summary>
        /// Constructor
        /// </summary>
        public ImageMessage()
        {
//            base.length = 52;
        }

    }
    /// <summary>
    /// error message structure
    /// </summary>
    public class ErrorMessage : MessageBase
    {
        /// <summary>
        /// decoded message data
        /// </summary>
        public String message;
        /// <summary>
        /// AIM Id of symbology
        /// </summary>
        public Result result;
    }
    /// <summary>
    /// decode structure
    /// </summary>
    public class DecodeMessage : MessageBase
    {
        /// <summary>
        /// decoded message data
        /// </summary>
        public String pchMessage;
        /// <summary>
        /// AIM Id of symbology
        /// </summary>
        //public AimID chCodeID;
        /// <summary>
        /// HHP Id of symbology
        /// </summary>
        //public SymID chSymLetter;
        /// <summary>
        /// Modifier characters.
        /// </summary>
        public Char chSymModifier;
        /// <summary>
        /// length of the decoded message
        /// </summary>
        public UInt32 nLength;
        /// <summary>
        /// Constructor
        /// </summary>
        public DecodeMessage()
        {
//            base.length = 8208;
        }
    }
    /// <summary>
    /// Raw decode structure
    /// </summary>
    public class RawDecodeMessage : MessageBase
    {
        /// <summary>
        /// decoded message data
        /// </summary>
        public byte[] pchMessage;
        /*/// <summary>
        /// AIM Id of symbology
        /// </summary>
        public byte chCodeID;
        /// <summary>
        /// Id of symbology
        /// </summary>
        public byte chSymLetter;
        /// <summary>
        /// Modifier characters.
        /// </summary>
        public byte chSymModifier;
        /// <summary>
        /// length of the decoded message
        /// </summary>
        public uint nLength;
        /// <summary>
        /// Constructor
        /// </summary>
        public RawDecodeMessage()
        {
            base.length = 4108;
        }*/
    }
    /// <summary>
    /// Barcode Message base class
    /// </summary>
    public class MessageBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MessageBase()
        {
            // NOP - length_ MUST be set by child classes
        }
        /*
        /// <summary>
        /// Structure size
        /// </summary>
        public UInt32 length
        {
            get { return this.m_length; }
            set { this.m_length = value; }
        }
        */
    }

    /// <summary>
    /// Text message type
    /// </summary>
    public class TextMessage
    {
        /// <summary>
        /// Structure size
        /// </summary>
        protected UInt32 dwStructSize = 8200;
        /// <summary>
        /// Text message with 4096 maximum length
        /// </summary>
        public UInt16[] tcTextMsg = new UInt16[Constants.Constants.MAX_MESAGE_LENGTH];
        /// <summary>
        /// Message length
        /// </summary>
        public UInt32 nLength;
    }

#endregion

    /// <summary>
    /// Intelligent Image Capture
    /// </summary>
    public class IntelligentImage
    {
        /// <summary>
        /// Structure size
        /// </summary>
        protected UInt32 dwStructSize = 36;
        /// <summary>
        /// ratio of barcode height to narrow element width
        /// </summary>
        public UInt32 dwAspectRatio;  
        /// <summary>
        /// offset in X direction, relative to barcode center
        /// </summary>
        public Int64 nOffsetX;       
        /// <summary>
        /// offset in Y direction
        /// </summary>
        public Int64 nOffsetY;       
        /// <summary>
        /// width of image in IntellBarcodeUnits
        /// </summary>
        public UInt32 dwWidth;       
        /// <summary>
        /// height of image
        /// </summary>
        public UInt32 dwHeight;       
        /// <summary>
        /// Maximum width and height for intell image
        /// </summary>
        public SIZE maxImgSize;     
        /// <summary>
        /// Have reader binarize data before transfer.
        /// </summary>
        public bool bSendBinary;     

    }

    #region Config Structs

    ///<summary>
    /// Image Acquisition structure
    ///</summary>
    public class ImageAcquisitionParms : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public ImgAcquisitionMask dwMask = ImgAcquisitionMask.CAPTURE_MASK_ALL;
        /// <summary>
        /// Target "white pixel" value.
        /// </summary>
        public uint dwWhiteValue;
        /// <summary>
        /// Acceptable delta from white value.
        /// </summary>
        public uint dwWhiteWindow;
        /// <summary>
        /// Max frame capture tries for white value.
        /// </summary>
        public uint dwMaxNumExposures;
        /// <summary>
        /// Illumination duty cycle (never on, on during imaging).
        /// </summary>
        public DutyCycle illuminatCycle;
        /// <summary>
        /// Aimer duty cycle (never on, on during imaging).
        /// </summary>
        public DutyCycle aimerCycle;
        /// <summary>
        /// If manual capture mode, gain value for capture.
        /// </summary>
        public Gain fixedGain;
        /// <summary>
        /// If manual capture mode, exposure time for capture.(1-7874)
        /// </summary>
        public uint dwFixedExposure;
        /// <summary>
        /// If manual capture mode, frame rate for capture.
        /// </summary>
        public FrameRate frameRate;
        /// <summary>
        /// Autoexposure (AGC) Capture mode: barcode, photo or manual.
        /// </summary>
        public AutoExposure captureMode;
        /// <summary>
        /// 640x480 Max Image Size.
        /// </summary>
        public bool bVgaCompatibleImage; 
        /// <summary>
        /// Wait for hardware or software trigger before capturing image.
        /// </summary>
        public bool bWaitForTrigger;
        /// <summary>
        /// Capture a preview image. These are subsample 3, full window, JPEG transfer images.
        /// </summary>
        public bool bPreviewImage;
        /// <summary>
        /// Constructor
        /// </summary>
        public ImageAcquisitionParms()
        {
            base.m_length = 56;
        }
    }

    ///<summary>
    /// Image Transfer structure
    ///</summary>
    public class ImageTransferParms : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public ImgTransferMask dwMask = ImgTransferMask.TRANSFER_MASK_ALL;
        /// <summary>
        /// Bits per pixel for transferred image (1 or 8 bits only).
        /// </summary>
        public uint dwBitsPerPixel;
        /// <summary>
        /// Subsample value. This means take every dwSubSample pixels 
        /// of every dwSubSample row.  The default is 1.(1-10)
        /// </summary>
        public uint dwSubSample;
        /// <summary>
        /// Rectangle describing a window within the image. 
        /// All pixels outside this rectangle are omitted from the transferred image. 
        /// </summary>
        public RECT boundingRect;
        /// <summary>
        /// Scaled frequency of total image pixels.
        /// </summary>
        public bool bHistogramStretch;
        /// <summary>
        /// How image is compressed on transfer.  Compression reduces 
        /// the amount of data to transfer but can reduce image quality 
        /// (Lossy compression) and does take a finite length of time.
        /// </summary>
        public Compression compressionMode;
        /// <summary>
        /// Lossy compression is JPEG lossy.  If lossy compression, this 
        /// value specifies the image quality percentage from 100 (virtually 
        /// no loss) to 1 (very poor).  Image size drops with decrease in 
        /// image quality.
        /// </summary>
        public uint dwCompressionFactor;
        /// <summary>
        /// A sharpening filter used to sharpen light/dark edges within the 
        /// image.  The valid range of values is 0 (no edge enhancement) 
        /// to 23 (maximum edge enhancement).
        /// </summary>
        public uint dwEdgeEhancement;
        /// <summary>
        /// Applies gamma correction to the image. The valid range is 0 (no 
        /// gamma correction) to 1000 (maximum correction).
        /// </summary>
        public uint dwGammaCorrection;
        /// <summary>
        /// This filter is an edge sharpening filter optimized for text. The 
        /// valid range is 0 (no text enhancement) to 255 (maximum 
        /// enhancement).
        /// </summary>
        public uint dwTextEnhancement;
        /// <summary>
        /// This is a boolean flag (TRUE or FALSE) that applies a filter to 
        /// the image that sharpens objects beyond the normal focal dis-
        /// tance of the imager.
        /// </summary>
        public bool bInfinityFilter;
        /// <summary>
        /// This is a boolean flag (TRUE or FALSE) that flips the image 180бу 
        /// </summary>
        public bool bFlipImage;
        /// <summary>
        /// This is a boolean flag (TRUE or FALSE) that enables or dis-
        /// ables the imager smoothing filter.
        /// </summary>
        public bool bNoiseFilter;
        /// <summary>
        /// The user-defined window message. 
        /// WM_ID.WM_SDK_PROGRESS_HWND_MSG (wParam is bytes so far, 
        /// lParam is bytes to send) will be sent if this member mask speci-
        /// fied and its value is a valid windows handle.
        /// </summary>
        public IntPtr hTransferNotifyHwnd;
        /// <summary>
        /// If non-NULL and specified in MASK, the percent complete of 
        /// the transfer is placed here.  It is up to the caller to check the 
        /// value in a thread or timer callback.
        /// </summary>
        public IntPtr pdwTransferPercent;
        /// <summary>
        /// Constructor
        /// </summary>
        public ImageTransferParms()
        {
            base.m_length = 76;
        }
    }

    ///<summary>
    /// Beeper structure
    ///</summary>
    // Beeper structure
    public class BeeperParms : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public BeeperMask dwMask = BeeperMask.ALL;
        /// <summary>
        /// Sound beeper on successful decode.
        /// </summary>
        public bool bBeepOnDecode;
        /// <summary>
        /// Sound beeper whenever imager resets.
        /// </summary>
        public bool bShortBeep;
        /// <summary>
        /// Sound beeper whenever a menu command is received.
        /// </summary>
        public bool bMenuCmdBeep;
        /// <summary>
        /// Set the beeper volume.
        /// </summary>
        public BeeperVolume beepVolume;
        /// <summary>
        /// Constructor
        /// </summary>
        public BeeperParms()
        {
            base.m_length = 24;
        }
    }

    /// <summary>
    /// Triggering control
    /// </summary>
    public class TriggerParms : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public TriggerMask dwMask = TriggerMask.ALL;
        /// <summary>
        /// Trigger Mode
        /// </summary>
        public TriggerMode TriggerMode;
        /// <summary>
        /// 0->300000 (milliseconds)
        /// </summary>
        public uint dwTriggerTimeout;
        /// <summary>
        /// Constructor
        /// </summary>
        public TriggerParms()
        {
            base.m_length = 16;
        }
    }

    /// <summary>
    ///  Sequence mode barcode descriptor
    /// </summary>
    public class SequenceBarcodeParms
    {
        /// <summary>
        /// Symbology Identifier SYM_xxxx
        /// </summary>
        //public Symbol nSymId;
        /// <summary>
        ///  Match length or 9999 to match any length.
        /// </summary>
        public Int32 nLength;
        /// <summary>
        /// Match string (from start)
        /// </summary>
        public String tcStartChars;

    }
    /// <summary>
    /// Sequence Mode control
    /// </summary>
    public class SequenceModeParms : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public SequenceMask dwMask = SequenceMask.ALL;
        /// <summary>
        /// Disabled/Enabled/Enabled and Required.
        /// </summary>
        public SequenceMode sequenceMode;
        /// <summary>
        /// This MUST be sent if sending seqBarCodes.
        /// </summary>
        public uint dwNumBarCodes;
        /// <summary>
        /// Barcodes to sequence in order they are to be sent
        /// </summary>
        public SequenceBarcodeParms[] seqBarCodes = new SequenceBarcodeParms[Constants.Constants.MAX_SEQ_BARCODES];
        /// <summary>
        /// Constructor
        /// </summary>
        public SequenceModeParms()
        {
            base.m_length = 928;
        }

    }

    /// <summary>
    /// Decoder functionality settings.
    /// </summary>
    public class DecoderParms : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public DecoderMask dwMask = DecoderMask.ALL;
        /// <summary>
        /// Maximum length for any returned barcode string.  This is a read only value.
        /// </summary>
        public uint dwMaxMsgSize;
        /// <summary>
        /// Decode and send all symbols decoded with first frame where at least 1 symbol is found.
        /// </summary>
        public bool bDecodeMultiple;
        /// <summary>
        /// Turn on aimers during barcode capture.
        /// </summary>
        public bool bUseAimers;
        /// <summary>
        /// How dark the barcode elements are relative to the background (1-7).
        /// </summary>
        public uint dwPrintWeight;
        /// <summary>
        /// Normal decoder, linear codes only, fast normal decoder, which 
        /// omits checking at the image margins as well as some bad bar-
        /// code correction.
        /// </summary>
        public DecoderMode decodeMethod;
        /// <summary>
        /// Does symbol have to intersect center decode window to be valid.
        /// </summary>
        public bool bCenterDecodeEnable;
        /// <summary>
        /// Bounding coords or window that decoded symbol must intersect
        /// </summary>
        public RECT centerWindow;
        /// <summary>
        /// Illumination LED color to use.
        /// </summary>
        public IllumLedColor illumLedColor;
        /// <summary>
        /// 10->500  Delay before reading UPC if expecting addenda.
        /// </summary>
        public uint dwUpcAddendaDelay;
        /// <summary>
        /// 10->1000 Delay before reading code if expecting composite.
        /// </summary>
        public uint dwCompositeDelay;
        /// <summary>
        /// 10->1000 Delay to wait before reading code if expecting concatenated code.
        /// </summary>
        public uint dwConcatenateDelay;
        /// <summary>
        /// Constructor
        /// </summary>
        public DecoderParms()
        {
            base.m_length = 64;
        }

    }

    /// <summary>
    /// Matrix Products Power management structure
    /// </summary>
    public class PowerParms : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public PowerMask dwMask = PowerMask.ALL;
        /// <summary>
        /// Trigger mode (same as per trigger struct)
        /// </summary>
        public TriggerMode TriggerMode;           
        /// <summary>
        /// 0 -> 300000 (milliseconds)
        /// </summary>
        public uint dwTriggerTimeout;       
        /// <summary>
        /// 0 -> 300    (seconds)
        /// </summary>
        public uint dwLowPowerTimeout;     
        /// <summary>
        /// 0 -> 100%
        /// </summary>
        public uint dwLEDIntensityPercent; 
        /// <summary>
        /// Clock speed for reset of system (except RS232)
        /// </summary>
        public SystemSpeed systemClockSpeed;       
        /// <summary>
        /// Aimer Mode
        /// </summary>
        public AimerModes AimerMode;              
        /// <summary>
        /// 0 -> 240000 (milliseconds)
        /// </summary>
        public uint dwAimerDuration;        
        /// <summary>
        ///  0 -> 4000   (milliseconds)
        /// </summary>
        public uint dwAimerDelay;          
        /// <summary>
        /// 0 -> 30000  (milliseconds)
        /// </summary>
        public uint dwAutoAimerTimeout;     
        /// <summary>
        /// 0 -> 999999 (milliseconds)
        /// </summary>
        public uint dwImagerIdleTimeout;    
        /// <summary>
        /// 0 -> 300    (seconds)RS232 inactivity timeout used to enter sleep mode.
        /// </summary>
        public uint dwRS232LowPwrTimeout;   
        /// <summary>
        /// These are used to notify on suspend (WinCE Suspend)
        /// </summary>
        public IntPtr hPowerOffHandle;
        /// <summary>
        /// 
        /// </summary>
        public IntPtr hPowerOffHwnd;
        /// <summary>
        /// Constructor
        /// </summary>
        public PowerParms()
        {
            base.m_length = 60;
        }
    }

    /// <summary>
    /// Revision information
    /// </summary>
    public class VersionParms : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public VersionMask dwMask = VersionMask.ALL;
        /// <summary>
        /// SDK API version string
        /// </summary>
        public String tcAPIRev;
        /// <summary>
        /// Imager firmware version
        /// </summary>
        public String tcFirmwareRev;      
        /// <summary>
        /// Engine PartNumber.
        /// </summary>
        public String tcFirmwarePartNumber;
        /// <summary>
        /// Imager boot code version.
        /// </summary>
        public String tcBootCodeRev;
        /// <summary>
        /// Imager device string.
        /// </summary>
        public String tcDeviceType;
        /// <summary>
        /// Engine Report string.
        /// </summary>
        public String tcManufacturersId;
        /// <summary>
        /// Decoder Revision
        /// </summary>
        public String tcDecoderRev;
        /// <summary>
        /// Scan Driver Revision
        /// </summary>
        public String tcScanDriverRev;
        /// <summary>
        /// Constructor
        /// </summary>
        public VersionParms()
        {
            base.m_length = 1704;
        }

    }
    /// <summary>
    /// Engine Information Structure (5000K engine with PSOC only)
    /// </summary>
    public class EngineInfoParms
    {
        /// <summary>
        /// Structure size
        /// </summary>
        protected uint dwStructSize = 224;                                   // Structure size
        /// <summary>
        /// 4 digit ASCII Hex.
        /// </summary>
        public UInt16[] tcEngId = new UInt16[Constants.Constants.ENGINE_ID_DIGITS];                    
        /// <summary>
        /// ASCII Decimal
        /// </summary>
        public UInt16[] tcHHPSerialNumber = new UInt16[Constants.Constants.MAX_SERIAL_NUMBER_LEN];     
        /// <summary>
        /// ASCII Decimal
        /// </summary>
        public UInt16[] tcCustomSerialNumber = new UInt16[Constants.Constants.MAX_SERIAL_NUMBER_LEN];  
        /// <summary>
        /// Aimer X
        /// </summary>
        public long nAimerX;                                        
        /// <summary>
        /// Aimer Y
        /// </summary>
        public long nAimerY;                                        
        /// <summary>
        /// Laser power in mW
        /// </summary>
        public long nLaserPower;                                    
        /// <summary>
        /// Firmware Checksum (ASCII Decimal)
        /// </summary>
        public UInt16[] tcFirmwareChecksum = new UInt16[Constants.Constants.MAX_CHECKSUM_LEN];         
        /// <summary>
        /// Firmware Revision Number
        /// </summary>
        public UInt16[] tcFirmwareRev = new UInt16[Constants.Constants.MAX_SHORT_VERSION_LEN];      
        /// <summary>
        /// How LEDs are controled.
        /// </summary>
        public long nLedCtrlMode;                                   
        /// <summary>
        /// LED color (Red or Green LEDs)
        /// </summary>
        public long nLedClr;                                        
        /// <summary>
        /// PWM base frequence.
        /// </summary>
        public long nPwmFreq;                                      
        /// <summary>
        /// Red LED current (mA).
        /// </summary>
        public long nRedLedCurrent;                                 
        /// <summary>
        /// Red LED max current (mA).
        /// </summary>
        public long nRedLedMaxCurrent;                              
        /// <summary>
        /// Green LED current (mA).
        /// </summary>
        public long nGreenLedCurrent;                              
        /// <summary>
        ///  Green LED max current (mA).
        /// </summary>
        public long nGreenLedMaxCurrent;                            
        /// <summary>
        /// Aimer current (mA).
        /// </summary>
        public long nAimerCurrent;                                  
        /// <summary>
        /// Aimer max current (mA).
        /// </summary>
        public long nAimerMaxCurrent;                               
        /// <summary>
        /// Pixel clock frequency (MHz)
        /// </summary>
        public long nPixelClockFreq;                               
        /// <summary>
        /// Register Checksum.
        /// </summary>
        public UInt16[] tcRegisterChecksum = new UInt16[Constants.Constants.MAX_CHECKSUM_LEN];      

    }

    ///<summary>
    /// Big mother of them all configuration structure.
    ///</summary>
    public class AllConfigParms : ConfigBase
    {
        /// <summary>
        /// Beeper config
        /// </summary>
        public BeeperParms beeperCfg = new BeeperParms();
        /// <summary>
        /// Trigger config
        /// </summary>
        public TriggerParms triggerCfg = new TriggerParms();
        /// <summary>
        /// Decoder config
        /// </summary>
        public DecoderParms decoderCfg = new DecoderParms();
        /// <summary>
        /// Power setting config
        /// </summary>
        public PowerParms powerCfg = new PowerParms();
        /// <summary>
        /// Version information
        /// </summary>
        public VersionParms versionInfo = new VersionParms();
        /// <summary>
        /// Symbology
        /// </summary>
        public SymbologyParms symbolCfg = new SymbologyParms();
        /// <summary>
        /// Image acquisition config
        /// </summary>
        public ImageAcquisitionParms imgAcqu = new ImageAcquisitionParms();
        /// <summary>
        /// Image Transfer config
        /// </summary>
        public ImageTransferParms imgTrans = new ImageTransferParms();
        /// <summary>
        /// Sequence mode config
        /// </summary>
        public SequenceModeParms sequenceCfg = new SequenceModeParms();
#if ADD_PREFIX_SUFFIX_DATA_FORMATTER
        public CFG_DATA_EDITING      dataEditCfg;
#endif
        /// <summary>
        /// 
        /// </summary>
        public AllConfigParms()
        {
            base.m_length = 5364;
        }

    }
    /// <summary>
    /// Configuration base class
    /// </summary>
    public class ConfigBase
    {
        /// <summary>
        /// Structure size
        /// </summary>
        protected Int32 m_length = 4;
        /// <summary>
        /// 
        /// </summary>
        public ConfigBase()
        {
            // NOP - length_ MUST be set by child classes
        }
        /// <summary>
        /// Structure size
        /// </summary>
        public Int32 length
        {
            get { return this.m_length; }
            protected set { m_length = value; }
        }
    }


    #endregion

    #region Symbo
    /// <summary>
    /// Structure for symbologies with no specified min or max length.
    /// </summary>
    public class SymFlagsOnly : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public SymbolMask dwMask = SymbolMask.ALL;
        /// <summary>
        /// OR of valid flags for the given symbology.
        /// </summary>
        public SymbolFlags dwFlags;
        /// <summary>
        /// Constructor
        /// </summary>
        public SymFlagsOnly()
        {
            base.m_length = 12;
        }
    }
    /// <summary>
    /// Structure for symbologies with min or max length.
    /// </summary>
    public class SymFlagsRange : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public SymbolMask dwMask = SymbolMask.ALL;
        /// <summary>
        /// OR of valid flags for the given symbology.
        /// </summary>
        public SymbolFlags dwFlags;
        /// <summary>
        /// Minimum length for valid barcode string for this symbology.
        /// </summary>
        public uint dwMinLen;
        /// <summary>
        /// Maximum length for valid barcode string for this symbology.
        /// </summary>
        public uint dwMaxLen;
        /// <summary>
        /// Constructor
        /// </summary>
        public SymFlagsRange()
        {
            base.m_length = 20;
        }
    }
    /// <summary>
    /// Structure for unusual OCR
    /// </summary>
    public class SymCodeOCR : ConfigBase
    {
        /// <summary>
        /// Mask of active items. Notes: You can use OR to enable multiple items
        /// </summary>
        public SymbolMask dwMask = SymbolMask.ALL;
        /// <summary>
        /// OCR Enable/Mode structure.
        /// </summary>
        public OCR ocrMode;
        /// <summary>
        /// OCR direction structure.
        /// </summary>
        public OCRDirection ocrDirection;
        /// <summary>
        /// Template for decoded data ('d' - decimal, 'a' - ASCII, 'l' - letter, 'e' - extended).
        /// Maximum len is 256
        /// </summary>
        public String tcTemplate;
        /// <summary>
        /// Group G character string.
        /// Maximum len is 256
        /// </summary>
        public String tcGroupG;
        /// <summary>
        /// Group H character string.
        /// Maximum len is 256
        /// </summary>
        public String tcGroupH;
        /// <summary>
        ///  Check character string.
        /// Maximum len is 64
        /// </summary>
        public String tcCheckChar;
        /// <summary>
        /// Constructor
        /// </summary>
        public SymCodeOCR()
        {
            base.m_length = 1680;
        }
    }

    ///<summary>
    /// Structure of structures, one for each symbology.
    ///</summary>
    public class SymbologyParms : ConfigBase
    {
        // Linear Codes                 // Flags supported for this code
        //---------------------------------------------------------------
        /// <summary>
        /// CODABAR Configuration
        /// </summary>
        public CODABAR_T codabar = new CODABAR_T();        // Enable,Check,CheckSend,StartStop,Concatenate
        /// <summary>
        /// CODE11 Configuration
        /// </summary>
        public CODE11_T code11 = new CODE11_T();         // Enable,Check,CheckSend
        /// <summary>
        /// CODE11 Configuration
        /// </summary>
        /// <summary>
        /// CODE128 Configuration
        /// </summary>
        public CODE128_T code128 = new CODE128_T();        // Enable
        /// <summary>
        /// CODE39 Configuration
        /// </summary>
        public CODE39_T code39 = new CODE39_T();         // Enable,Check,CheckSend,StartStop,Append,FullAscii
        /// <summary>
        /// CODE49 Configuration
        /// </summary>
        public CODE49_T code49 = new CODE49_T();         // Enable
        /// <summary>
        /// CODE93 Configuration
        /// </summary>
        public CODE93_T code93 = new CODE93_T();         // Enable
        /// <summary>
        /// COMPOSITE Configuration
        /// </summary>
        public COMPOSITE_T composite = new COMPOSITE_T();      // Enable,CompositeUPC
        /// <summary>
        /// DATAMATRIX Configuration
        /// </summary>
        public DATAMATRIX_T datamatrix = new DATAMATRIX_T();     // Enable
        /// <summary>
        /// EAN8 Configuration
        /// </summary>
        public EAN8_T ean8 = new EAN8_T();           // Enable,Check,Addenda2,Addenda5,AddendaReq,AddendaSep
        /// <summary>
        /// EAN13 Configuration
        /// </summary>
        public EAN13_T ean13 = new EAN13_T();          // Enable,Check,Addenda2,Addenda5,AddendaReq,AddendaSep
        /// <summary>
        /// IATA25 Configuration
        /// </summary>
        public IATA25_T iata25 = new IATA25_T();         // Enable
        /// <summary>
        /// INT25 Configuration
        /// </summary>
        public INT25_T int2of5 = new INT25_T();        // Enable,Check,CheckSend
        /// <summary>
        /// ISBT Configuration
        /// </summary>
        public ISBT_T isbt = new ISBT_T();           // Enable
        /// <summary>
        /// MSI Configuration
        /// </summary>
        public MSI_T msi = new MSI_T();            // Enable,Check
        /// <summary>
        /// UPCA Configuration
        /// </summary>
        public UPCA_T upcA = new UPCA_T();           // Enable,check,NumSysTrans,Addenda2,Addenda5,AddendaReq,AddendaSep
        /// <summary>
        /// UPCE Configuration
        /// </summary>
        public UPCE_T upcE = new UPCE_T();           // Enable,check,NumSysTrans,Addenda2,Addenda5,AddendaReq,AddendaSep,ExpandedE,EnableE1
         // Postal Codes
        /// <summary>
        /// AUSPOST Configuration
        /// </summary>
        public AUSPOST_T australiaPost = new AUSPOST_T();  // Enable,AustralianBar
        /// <summary>
        /// BPO Configuration
        /// </summary>
        public BPO_T britishPost = new BPO_T();    // Enable
        /// <summary>
        /// CANPOST Configuration
        /// </summary>
        public CANPOST_T canadaPost = new CANPOST_T();     // Enable
        /// <summary>
        /// DUTCHPOST Configuration
        /// </summary>
        public DUTCHPOST_T dutchPost = new DUTCHPOST_T();      // Enable
        /// <summary>
        /// JAPOST Configuration
        /// </summary>
        public JAPOST_T japanPost = new JAPOST_T();      // Enable
        /// <summary>
        /// PLANET Configuration
        /// </summary>
        public PLANET_T usPlanet = new PLANET_T();       // Enable,Check
        /// <summary>
        /// POSTNET Configuration
        /// </summary>
        public POSTNET_T usPostnet = new POSTNET_T();      // Enable,Check
        // 2D Codes
        /// <summary>
        /// AZTEC Configuration
        /// </summary>
        public AZTEC_T aztec = new AZTEC_T();          // Enable,AztecRune
        /// <summary>
        /// MESA Configuration
        /// </summary>
        public MESA_T aztecMesa = new MESA_T();      // EnableIMS,Enable1MS,Enable3MS,Enable9MS,EnableUMS,EnableEMS
        /// <summary>
        /// CODABLOCK Configuration
        /// </summary>
        public CODABLOCK_T codablock = new CODABLOCK_T();      // Enable
        /// <summary>
        /// MAXICODE Configuration
        /// </summary>
        public MAXICODE_T maxicode = new MAXICODE_T();       // Enable,SCMOnly
        /// <summary>
        /// MICROPDF Configuration
        /// </summary>
        public MICROPDF_T microPDF = new MICROPDF_T();       // Enable
        /// <summary>
        /// PDF417 Configuration
        /// </summary>
        public PDF417_T pdf417 = new PDF417_T();         // Enable
        /// <summary>
        /// QR Configuration
        /// </summary>
        public QR_T qr = new QR_T();             // Enable
        /// <summary>
        /// RSS Configuration
        /// </summary>
        public RSS_T rss = new RSS_T();            // Enable (RSS,RSL,RSE)
        /// <summary>
        /// TLCODE39 Configuration
        /// </summary>
        public TLCODE39_T tlCode39 = new TLCODE39_T();       // Enable
        // Special OCR "code"
        /// <summary>
        /// OCR Configuration
        /// </summary>
        public OCR_T ocr = new OCR_T();            // None (See SymCodeOCR_t)
        // New codes
        /// <summary>
        /// MATRIX25 Configuration
        /// </summary>
        public MATRIX25_T matrix25 = new MATRIX25_T();       // Enable,
        /// <summary>
        /// KORPOST Configuration
        /// </summary>
        public KORPOST_T koreaPost = new KORPOST_T();      // Enable
        /// <summary>
        /// TRIOPTIC Configuration
        /// </summary>
        public TRIOPTIC_T triopticCode = new TRIOPTIC_T();   // Enable
        /// <summary>
        /// CODE32 Configuration
        /// </summary>
        public CODE32_T code32 = new CODE32_T();         // Enable
        /// <summary>
        /// CODE25 Configuration
        /// </summary>
        public CODE25_T code2of5 = new CODE25_T();       // Enable
        /// <summary>
        /// PLESSEY Configuration
        /// </summary>
        public PLESSEY_T plesseyCode = new PLESSEY_T();    // Enable
        /// <summary>
        /// CHINAPOST Configuration
        /// </summary>
        public CHINAPOST_T chinaPost = new CHINAPOST_T();      // Enable
        /// <summary>
        /// TELEPEN Configuration
        /// </summary>
        public TELEPEN_T telepen = new TELEPEN_T();        // Enable,OldStyle?
        /// <summary>
        /// CODE16K Configuration
        /// </summary>
        public CODE16K_T code16k = new CODE16K_T();        // Enable
        /// <summary>
        /// POSICODE Configuration
        /// </summary>
        public POSICODE_T posiCode = new POSICODE_T();       // Enable,Limited 1 and 2
        /// <summary>
        /// COUPONCODE Configuration
        /// </summary>
        public COUPONCODE_T couponCode = new COUPONCODE_T();     // Enable
        // 5000 engine only codes?
        /// <summary>
        /// UPUIDTAG Configuration
        /// </summary>
        public UPUIDTAG_T upuIdTag = new UPUIDTAG_T();       // Enable
        /// <summary>
        /// CODE4CB Configuration
        /// </summary>
        public CODE4CB_T code4CB = new CODE4CB_T();        // Enable
        /// <summary>
        /// Constructor
        /// </summary>
        public SymbologyParms()
        {
            base.m_length = 2432;
        }
    }
    #endregion

}
