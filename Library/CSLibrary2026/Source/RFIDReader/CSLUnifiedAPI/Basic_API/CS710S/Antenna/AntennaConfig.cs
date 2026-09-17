//#if CS468
using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    using CSLibrary.Constants;
    using CSLibrary.Structures;
    class AntennaConfig
        :
        Object
    {
        private UInt32 port;
        private AntennaPortConfig antennaPortConfig;

        public AntennaPortConfig AntennaPortConfig
        {
            get { return antennaPortConfig; }
            set { antennaPortConfig = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port"></param>
        public AntennaConfig
        (
            UInt32 port
        )
            :
            base()
        {
            this.port = port;
            this.antennaPortConfig = new AntennaPortConfig();
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="source"></param>
        public AntennaConfig
        (
            AntennaConfig source
        )
            :
            base()
        {
            this.Copy(source);
        }

        /// <summary>
        /// Copy from AntennaConfig
        /// </summary>
        /// <param name="from"></param>
        public void Copy(AntennaConfig from)
        {
            this.port = from.port;

            this.antennaPortConfig.powerLevel = from.antennaPortConfig.powerLevel;
            this.antennaPortConfig.dwellTime = from.antennaPortConfig.dwellTime;
            this.antennaPortConfig.numberInventoryCycles = from.antennaPortConfig.numberInventoryCycles;
            this.antennaPortConfig.antennaSenseThreshold = from.antennaPortConfig.antennaSenseThreshold;
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

            AntennaConfig rhs = obj as AntennaConfig;

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
        public bool Equals(AntennaConfig rhs)
        {
            if (null == (System.Object)rhs)
            {
                return false;
            }

            return
                   this.port == rhs.port
                && this.antennaPortConfig.powerLevel == rhs.antennaPortConfig.powerLevel
                && this.antennaPortConfig.dwellTime == rhs.antennaPortConfig.dwellTime
                && this.antennaPortConfig.numberInventoryCycles == rhs.antennaPortConfig.numberInventoryCycles
                && this.antennaPortConfig.antennaSenseThreshold == rhs.antennaPortConfig.antennaSenseThreshold;
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
        /// Load Antenna config from MAC
        /// </summary>
        /// <param name="transport"></param>
        /// <returns></returns>
        public Result Load
        (
            HighLevelInterface transport
        )
        {
            if (transport == null)
                return Result.NOT_INITIALIZED;
            return transport.rfid.GetAntennaPortConfiguration
                (
                    port,
                    ref this.antennaPortConfig
                );

        }
        /// <summary>
        /// Store Antenna config to MAC
        /// </summary>
        /// <param name="transport"></param>
        /// <returns></returns>
        public Result Store
        (
            HighLevelInterface transport
        )
        {
            if (transport == null)
                return Result.NOT_INITIALIZED;
            return transport.rfid.SetAntennaPortConfiguration
                (
                    port,
                    this.antennaPortConfig
                );
        }
        /// <summary>
        /// Antenna port number
        /// </summary>
        public UInt32 Port
        {
            get
            {
                return this.port;
            }
        }
        /// <summary>
        /// The power level for the logical antenna port's physical 
        /// transmit antenna.  This value is specified in 0.1 (i.e., 
        /// 1/10th) dBm. Note that all radio modules may not support 
        /// setting an antenna port's power level at 1/10th dBm 
        /// resolutions.  The dBm rounding/truncation policy is left to 
        /// the radio module and is outside the scope of the RFID 
        /// Reader Library. 
        /// </summary>
        public UInt32 PowerLevel
        {
            get
            {
                return this.antennaPortConfig.powerLevel;
            }
            set
            {
                this.antennaPortConfig.powerLevel = value;
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
        public UInt32 DwellTime
        {
            get
            {
                return this.antennaPortConfig.dwellTime;
            }
            set
            {
                this.antennaPortConfig.dwellTime = value;
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
        public UInt32 NumberInventoryCycles
        {
            get
            {
                return this.antennaPortConfig.numberInventoryCycles;
            }
            set
            {
                this.antennaPortConfig.numberInventoryCycles = value;
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
        public UInt32 AntennaSenseThreshold
        {
            get
            {
                return this.antennaPortConfig.antennaSenseThreshold;
            }
            set
            {
                this.antennaPortConfig.antennaSenseThreshold = value;
            }
        }


    } // End class AntennaConfig


} // End namespace CSLibrary
//#endif