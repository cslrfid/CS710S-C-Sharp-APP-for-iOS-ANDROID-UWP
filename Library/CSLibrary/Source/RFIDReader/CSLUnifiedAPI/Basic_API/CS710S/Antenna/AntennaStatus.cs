//#if CS468
using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    using CSLibrary.Constants;
    using CSLibrary.Structures;
    /// <summary>
    /// Antenna Status
    /// </summary>
    class AntennaStatus
        :
        Object
    {
        private UInt32 port;
        private AntennaPortStatus antennaPortStatus;


        /// <summary>
        /// Constructor
        /// designed to init for loading from radio
        /// </summary>
        /// <param name="port">Valid port from 0 - 15</param>
        public AntennaStatus
        (
            UInt32 port
        )
            :
            base()
        {
            this.port = port;
            this.antennaPortStatus = new AntennaPortStatus();
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="source"></param>
        public AntennaStatus
        (
            AntennaStatus source
        )
            :
            base()
        {
            this.Copy(source);
        }

        /// <summary>
        /// Copy from AntennaStatus
        /// </summary>
        /// <param name="from"></param>
        public void Copy(AntennaStatus from)
        {
            this.port = from.Port;
            this.antennaPortStatus.state = from.State;
            this.antennaPortStatus.antennaSenseValue = from.AntennaSenseValue;
        }

        /// <summary>
        /// check equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object obj)
        {
            if (null == obj)
            {
                return false;
            }

            AntennaStatus rhs = obj as AntennaStatus;

            if (null == (System.Object)rhs)
            {
                return false;
            }

            return this.Equals(rhs);
        }

        /// <summary>
        /// check equal
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public bool Equals(AntennaStatus rhs)
        {
            if (null == (System.Object)rhs)
            {
                return false;
            }

            return
                   this.port == rhs.port
                && this.antennaPortStatus.state == rhs.antennaPortStatus.state
//#if CS468
                && this.antennaPortStatus.enableLocalFreq == rhs.antennaPortStatus.enableLocalFreq
                && this.antennaPortStatus.enableLocalInv == rhs.antennaPortStatus.enableLocalInv
                && this.antennaPortStatus.enableLocalProfile == rhs.antennaPortStatus.enableLocalProfile
                && this.antennaPortStatus.freqChn == rhs.antennaPortStatus.freqChn
                && this.antennaPortStatus.inv_algo == rhs.antennaPortStatus.inv_algo
                && this.antennaPortStatus.profile == rhs.antennaPortStatus.profile
                && this.antennaPortStatus.startQ == rhs.antennaPortStatus.startQ
//#endif
                && this.antennaPortStatus.antennaSenseValue == rhs.antennaPortStatus.antennaSenseValue;
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
        /// Load Antenna Status from MAC
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
            return transport.rfid.GetAntennaPortStatus
                (
                    this.port,
                    this.antennaPortStatus
                );
        }

        /// <summary>
        /// Store Antenna Status to MAC
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
            // There is no save for the AntennaPortStatus in lower library but
            // there is a set state so use that here!

            return transport.rfid.SetAntennaPortState
                (
                    this.port,
                    this.antennaPortStatus.state
                );
        }
        /// <summary>
        /// Get current port number
        /// </summary>
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
        public AntennaPortState State
        {
            get
            {
                return this.antennaPortStatus.state;
            }
            set
            {
                this.antennaPortStatus.state = value;
            }
        }

//#if CS468
        public Boolean EnableLocalInv
        {
            get { return this.antennaPortStatus.enableLocalInv; }
            set { this.antennaPortStatus.enableLocalInv = value; }
        }
        public Boolean EnableLocalProfile
        {
            get { return this.antennaPortStatus.enableLocalProfile; }
            set { this.antennaPortStatus.enableLocalProfile = value; }
        }
        public Boolean EnableLocalFreq
        {
            get { return this.antennaPortStatus.enableLocalFreq; }
            set { this.antennaPortStatus.enableLocalFreq = value; }
        }
        public SingulationAlgorithm InventoryAlgorithm
        {
            get { return this.antennaPortStatus.inv_algo; }
            set { this.antennaPortStatus.inv_algo = value; }
        }
        public UInt32 StartQ
        {
            get { return this.antennaPortStatus.startQ; }
            set { this.antennaPortStatus.startQ = value; }
        }
        public UInt32 LinkProfile
        {
            get { return this.antennaPortStatus.profile; }
            set { this.antennaPortStatus.profile = value; }
        }
        public UInt32 FreqChannel
        {
            get { return this.antennaPortStatus.freqChn; }
            set { this.antennaPortStatus.freqChn = value; }
        }
//#endif

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
        public UInt32 AntennaSenseValue
        {
            get
            {
                return this.antennaPortStatus.antennaSenseValue;
            }
        }


    } // End class AntennaStatus


} // End namespace CSLibrary
//#endif