//#if CS468
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
namespace CSLibrary
{
    using CSLibrary.Constants;
    using CSLibrary.Structures;

    /// <summary>
    /// Antenna
    /// </summary>
#if NETCFDESIGNTIME
    [System.ComponentModel.TypeConverter(typeof(AntennaTypeConverter))]
#endif
    public class Antenna
        :
        Object
    {

        // According to MAC EDS ~ phy port number allocated 2 bits only

        //public static readonly UInt32 RX_PHY_MINIMUM = 0;
        //public static readonly UInt32 TX_PHY_MINIMUM = RX_PHY_MINIMUM;

        //public static readonly UInt32 RX_PHY_MAXIMUM = 3;
        //public static readonly UInt32 TX_PHY_MAXIMUM = RX_PHY_MAXIMUM;

        // According to MAC EDS ~ caveat RX logical must == TX logical
        // for the current version...
        /// <summary>
        /// RX_LOGICAL_MINIMUM
        /// </summary>
        public static readonly UInt32 RX_LOGICAL_MINIMUM = 0;
        /// <summary>
        /// TX_LOGICAL_MINIMUM
        /// </summary>
        public static readonly UInt32 TX_LOGICAL_MINIMUM = 0;
        /// <summary>
        /// RX_LOGICAL_MAXIMUM
        /// </summary>
        public static readonly UInt32 RX_LOGICAL_MAXIMUM = 15;
        /// <summary>
        /// TX_LOGICAL_MAXIMUM
        /// </summary>
        public static readonly UInt32 TX_LOGICAL_MAXIMUM = 15;

        // According to previous app default settings
        /// <summary>
        /// POWER_MINIMUM
        /// </summary>
        public static readonly UInt32 POWER_MINIMUM = 1;
        /// <summary>
        /// POWER_MAXIMUM
        /// </summary>
        public static readonly UInt32 POWER_MAXIMUM = 300;


        public UInt32 port = 0;
        AntennaPortStatus antennaStatus;

        public AntennaPortStatus AntennaStatus
        {
            get { return antennaStatus; }
            set { antennaStatus = value; }
        }
        AntennaPortConfig antennaConfig;

        public AntennaPortConfig AntennaConfig
        {
            get { return antennaConfig; }
            set { antennaConfig = value; }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public Antenna() : this(0) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port"></param>
        public Antenna
        (
            UInt32 port
        )
            :
            base()
        {
            this.port = port;
            this.antennaStatus = new AntennaPortStatus();
            this.antennaConfig = new AntennaPortConfig();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port"></param>
        /// <param name="state"></param>
        /// <param name="powerLevel"></param>
        /// <param name="dwellTime"></param>
        /// <param name="numberInventoryCycles"></param>
        /// <param name="antennaSenseThreshold"></param>
        public Antenna
        (
            UInt32 port,
            AntennaPortState state,
            UInt32 powerLevel,
            UInt32 dwellTime,
            UInt32 numberInventoryCycles,

            Boolean easAlarm,
            Boolean enableLocalInv,
            SingulationAlgorithm invAlgo,
            UInt32 startQ,
            Boolean enableLocalProfile,
            UInt32 linkProfile,
            Boolean enableLocalFreq,
            UInt32 freqChannel,

            UInt32 antennaSenseThreshold
        )
            :
            base()
        {
            this.port = port;
            this.antennaStatus = new AntennaPortStatus();
            this.antennaConfig = new AntennaPortConfig();

            this.State = state;
            this.EnableLocalFreq = enableLocalFreq;
            this.EASAlarm = easAlarm;
            this.EnableLocalInv = enableLocalInv;
            this.EnableLocalProfile = enableLocalProfile;
            this.InventoryAlgorithm = invAlgo;
            this.StartQ = startQ;
            this.LinkProfile = linkProfile;
            this.FreqChannel = freqChannel;
            this.PowerLevel = powerLevel;
            this.DwellTime = dwellTime;
            this.NumberInventoryCycles = numberInventoryCycles;
            this.AntennaSenseThreshold = antennaSenseThreshold;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="antenna"></param>
        public Antenna
        (
            Antenna antenna
        )
            :
            this(0)
        {
            this.Copy(antenna);
        }


        /// <summary>
        /// Copy from Antenna
        /// </summary>
        /// <param name="from"></param>
        public void Copy(Antenna from)
        {
            this.port = from.Port;

            this.antennaStatus = (from.antennaStatus);
            this.antennaConfig = (from.antennaConfig);
        }

        /// <summary>
        /// Check equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object obj)
        {
            if (null == obj)
            {
                return false;
            }

            Antenna rhs = obj as Antenna;

            if (null == (System.Object)rhs)
            {
                return false;
            }

            return this.Equals(rhs);
        }

        /// <summary>
        /// Check equal
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public bool Equals(Antenna rhs)
        {
            if (null == (System.Object)rhs)
            {
                return false;
            }

            return
                   this.port == rhs.port
                && this.antennaStatus.Equals(rhs.antennaStatus)
                && this.antennaConfig.Equals(rhs.antennaStatus);
        }

        /// <summary>
        /// TODO: provide real hash return value
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Antenna port number
        /// </summary>
        [XmlElement()]
        public UInt32 Port
        {
            get
            {
                return this.port;
            }
        }
        /// <summary>
        /// Get the state of the logical antenna port. 
        /// </summary>
        [XmlIgnore]
        public AntennaPortState State
        {
            get
            {
                return this.antennaStatus.state;
            }
            set
            {
                this.antennaStatus.state = value;
            }
        }
//#if CS468
        /// <summary>
        /// EAS Alarm
        /// </summary>
        [XmlIgnore]
        public Boolean EASAlarm
        {
            get { return this.antennaStatus.easAlarm; }
            set { this.antennaStatus.easAlarm = value; }
        }

        /// <summary>
        /// Enable local inventory parameter
        /// </summary>
        [XmlIgnore]
        public Boolean EnableLocalInv
        {
            get { return this.antennaStatus.enableLocalInv; }
            set { this.antennaStatus.enableLocalInv = value; }
        }
        /// <summary>
        /// Enable local link profile
        /// </summary>
        [XmlIgnore]
        public Boolean EnableLocalProfile
        {
            get { return this.antennaStatus.enableLocalProfile; }
            set { this.antennaStatus.enableLocalProfile = value; }
        }
        /// <summary>
        /// Enable local frequency
        /// </summary>
        [XmlIgnore]
        public Boolean EnableLocalFreq
        {
            get { return this.antennaStatus.enableLocalFreq; }
            set { this.antennaStatus.enableLocalFreq = value; }
        }
        /// <summary>
        /// Inventory algorithm
        /// </summary>
        [XmlIgnore]
        public SingulationAlgorithm InventoryAlgorithm
        {
            get { return this.antennaStatus.inv_algo; }
            set { this.antennaStatus.inv_algo = value; }
        }
        /// <summary>
        /// Inventory StartQ
        /// </summary>
        [XmlIgnore]
        public UInt32 StartQ
        {
            get { return this.antennaStatus.startQ; }
            set { this.antennaStatus.startQ = value; }
        }
        /// <summary>
        /// LinkProfile
        /// </summary>
        [XmlIgnore]
        public UInt32 LinkProfile
        {
            get { return this.antennaStatus.profile; }
            set { this.antennaStatus.profile = value; }
        }
        /// <summary>
        /// Frequency channel
        /// </summary>
        [XmlIgnore]
        public UInt32 FreqChannel
        {
            get { return this.antennaStatus.freqChn; }
            set { this.antennaStatus.freqChn = value; }
        }
//#endif
        /// <summary>
        /// The power level for the logical antenna port's physical 
        /// transmit antenna.  This value is specified in 0.1 (i.e., 
        /// 1/10th) dBm. Note that all radio modules may not support 
        /// setting an antenna port's power level at 1/10th dBm 
        /// resolutions.  The dBm rounding/truncation policy is left to 
        /// the radio module and is outside the scope of the RFID 
        /// Reader Library. 
        /// </summary>
        [XmlIgnore]
        public UInt32 PowerLevel
        {
            get
            {
                return this.antennaConfig.powerLevel;
            }
            set
            {
                this.antennaConfig.powerLevel = value;
            }
        }
        /// <summary>
        /// Specifies the maximum amount of time, in milliseconds, 
        /// that may be spent on the logical antenna port during a 
        /// tag-protocol-operation cycle before switching to the next 
        /// enabled antenna port.  A value of zero indicates that there 
        /// is no maximum dwell time for this antenna port.  If this 
        /// parameter is zero, then numberInventoryCycles may 
        /// not be zero. 
        /// See  for the effect of antenna-port dwell time and number 
        /// of inventory cycles on the amount of time spent on an 
        /// antenna port during a single tag-protocol-operation cycle. 
        /// NOTE:  when performing any non-inventory ISO 18000-6C tag
        /// access operation (i.e., read, write, kill, or lock), the
        /// radio module ignores the dwell time for the antenna port 
        /// which is used for the tag-protocol operation. 
        /// </summary>
        [XmlIgnore]
        public UInt32 DwellTime
        {
            get
            {
                return this.antennaConfig.dwellTime;
            }
            set
            {
                this.antennaConfig.dwellTime = value;
            }
        }
        /// <summary>
        /// Specifies the maximum number of inventory cycles to 
        /// attempt on the antenna port during a tag-protocol-
        /// operation cycle before switching to the next enabled 
        /// antenna port.  An inventory cycle consists of one or more 
        /// executions of the singulation algorithm for a particular 
        /// inventory-session target (i.e., A or B).  If the singulation 
        /// algorithm [SING-ALG] is configured to toggle the 
        /// inventory-session, executing the singulation algorithm for 
        /// inventory session A and inventory session B counts as 
        /// two inventory cycles.  A value of zero indicates that there 
        /// is no maximum number of inventory cycles for this 
        /// antenna port.  If this parameter is zero, then dwellTime 
        /// may not be zero. 
        /// See  for the effect of antenna-port dwell time and number 
        /// of inventory cycles on the amount of time spent on an 
        /// antenna port during a single tag-protocol-operation cycle. 
        /// NOTE:  when performing any non-inventory ISO 18000-
        /// 6C tag access operation (i.e., read, write, kill, or lock), the 
        /// radio module ignores the number of inventory cycles for 
        /// the antenna port which is used for the tag-protocol 
        /// operation. 
        /// </summary>
        [XmlIgnore]
        public UInt32 NumberInventoryCycles
        {
            get
            {
                return this.antennaConfig.numberInventoryCycles;
            }
            set
            {
                this.antennaConfig.numberInventoryCycles = value;
            }
        }

        /// <summary>
        /// The measured resistance, specified in ohms, above which 
        /// the antenna-sense resistance should be considered to be 
        /// an open circuit (i.e., a disconnected antenna).  If it is 
        /// detected that the antenna-sense resistance is above the 
        /// threshold, the carrier wave will not be turned on in order 
        /// to protect the circuit. 
        /// NOTE:  This value, while appearing in the per-antenna 
        /// configuration is actually a system-wide setting in the 
        /// current release.  Changing it will Result in the value being 
        /// changed for all antennas.  To prevent unintentionally 
        /// changing this value for all antennas, it is best to first 
        /// retrieve the antenna configuration for the antenna for 
        /// which configuration will be changed, update the fields that 
        /// should be changed, and then set the configuration. 
        /// </summary>
        [XmlIgnore]
        public UInt32 AntennaSenseThreshold
        {
            get
            {
                return this.antennaConfig.antennaSenseThreshold;
            }
            set
            {
                this.antennaConfig.antennaSenseThreshold = value;
            }
        }
        /// <summary>
        /// The stored value from the last measurement of the 
        /// antenna-sense resistor for the logical antenna port's 
        /// physical transmit antenna port.  The last measurement 
        /// taken occurred the last time that the carrier wave was 
        /// turned on for this antenna port ¨C note that this means that 
        /// when retrieving the logical antenna port's status, this does 
        /// not Result in an active measurement of the antenna-sense 
        /// resistor.  This value is specified in ohms. 
        /// </summary>
        [XmlIgnore]
        public UInt32 AntennaSenseValue
        {
            get
            {
                return this.antennaStatus.antennaSenseValue;
            }
        }
    } // End class Antenna


} // End namespace RFID.RFIDInterface
//#endif