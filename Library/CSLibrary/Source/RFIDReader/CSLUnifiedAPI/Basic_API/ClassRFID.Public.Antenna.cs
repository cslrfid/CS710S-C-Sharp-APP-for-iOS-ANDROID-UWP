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
using System.Text;

namespace CSLibrary
{
    using static RFIDDEVICE;
    using Constants;
    using Structures;

    public partial class RFIDReader
    {
        //AntennaList m_AntennaList = null乙new AntennaList(AntennaList.DEFAULT_ANTENNA_LIST, true);
        AntennaList m_AntennaList = null;

        public AntennaList AntennaList
        {
            get { lock (m_AntennaList) return m_AntennaList; }
            set { lock (m_AntennaList) m_AntennaList = value; }
        }

        public Result SetDefaultAntennaList()
        {
            //DEBUG_WriteLine(DEBUGLEVEL.API, "HighLevelInterface.SetDefaultAntennaList()");
            
            m_AntennaList = new AntennaList();

            switch (m_oem_machine)
            {
                case MODEL.CS108:
                case MODEL.CS710S:
                    m_AntennaList.Add(new Antenna(0, AntennaPortState.ENABLED, 300, 0, 0x2000, false, false, SingulationAlgorithm.DYNAMICQ, 0, false, 0, false, 0, 1048575));
                    break;

                default:
                    m_AntennaList.Add(new Antenna(0, AntennaPortState.ENABLED, 300, 0, 0x2000, false, false, SingulationAlgorithm.DYNAMICQ, 0, false, 0, false, 0, 1048575));
                    m_AntennaList.Add(new Antenna(1, AntennaPortState.DISABLED, 300, 0, 0x2000, false, false, SingulationAlgorithm.DYNAMICQ, 0, false, 0, false, 0, 1048575));
                    m_AntennaList.Add(new Antenna(2, AntennaPortState.DISABLED, 300, 0, 0x2000, false, false, SingulationAlgorithm.DYNAMICQ, 0, false, 0, false, 0, 1048575));
                    m_AntennaList.Add(new Antenna(3, AntennaPortState.DISABLED, 300, 0, 0x2000, false, false, SingulationAlgorithm.DYNAMICQ, 0, false, 0, false, 0, 1048575));
                    break;
            }

            try
            {
                int i;
                //m_AntennaList = new AntennaList(AntennaList.DEFAULT_ANTENNA_LIST, true);
                for (i = 0; i < m_AntennaList.Count; i++)
                {
                    if (m_AntennaList[i].PowerLevel > GetSoftwareMaxPowerLevel(m_save_region_code))
                        m_AntennaList[i].PowerLevel = GetSoftwareMaxPowerLevel(m_save_region_code);

                    SetAntennaPortStatus((uint)i, m_AntennaList[i].AntennaStatus);
                    SetAntennaPortConfiguration((uint)i, m_AntennaList[i].AntennaConfig);
                }

                for (; i < 16; i++)
                {
                    AntennaPortSetState((uint)i, AntennaPortState.DISABLED);
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("HighLevelInterface.SetDefaultAntennaList() : " + ex.Message);
                //DEBUG_WriteLine(DEBUGLEVEL.API, "HighLevelInterface.SetDefaultAntennaList() : " + ex.Message);
                //CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.SetDefaultAntennaList()", ex);
            }

            return Result.OK;
        }

        /// <summary>
        /// Retrieves the status of the requested logical antenna port for a 
        /// particular radio module.  The antenna-port status cannot be 
        /// retrieved while a radio module is executing a tag-protocol 
        /// operation. 
        /// </summary>
        /// <param name="portStatus"></param>
        /// <returns></returns>
        public Result GetAntennaPortStatus(AntennaPortStatus portStatus)
        {
            return (m_Result = AntennaPortGetStatus(0, portStatus));
        }

        /// <summary>
        /// Retrieves the status of the requested logical antenna port for a 
        /// particular radio module.  The antenna-port status cannot be 
        /// retrieved while a radio module is executing a tag-protocol 
        /// operation. 
        /// </summary>
        /// <param name="port">antenna port</param>
        /// <param name="portStatus"></param>
        /// <returns></returns>
        public Result GetAntennaPortStatus(uint port, AntennaPortStatus portStatus)
        {
            return (m_Result = AntennaPortGetStatus(port, portStatus));
        }

        public Result AntennaPortGetStatus(uint port, AntennaPortStatus portStatus)
        {
            return Result.OK;
        }

        /// <summary>
        /// Retrieves the status of the requested logical antenna port for a 
        /// particular radio module.  The antenna-port status cannot be 
        /// retrieved while a radio module is executing a tag-protocol 
        /// operation. 
        /// </summary>
        /// <param name="portStatus"></param>
        /// <returns></returns>
        public Result SetAntennaPortStatus(AntennaPortStatus portStatus)
        {
            return (m_Result = AntennaPortSetStatus(0, portStatus));
        }

        /// <summary>
        /// Retrieves the status of the requested logical antenna port for a 
        /// particular radio module.  The antenna-port status cannot be 
        /// retrieved while a radio module is executing a tag-protocol 
        /// operation. 
        /// </summary>
        /// <param name="port">antenna port</param>
        /// <param name="portStatus"></param>
        /// <returns></returns>
        public Result SetAntennaPortStatus(uint port, AntennaPortStatus portStatus)
        {
            m_AntennaList[(int)port].AntennaStatus = portStatus;

            return (m_Result = AntennaPortSetStatus(port, portStatus));
        }

        public Result AntennaPortSetStatus(uint port, AntennaPortStatus portStatus)
        {
            return Result.OK;
        }

        /// <summary>
        /// Allows an application to specify whether or not a radio module's 
        /// logical antenna port is enabled for subsequent tag operations.  The 
        /// antenna-port state cannot be set while a radio module is executing 
        /// a tag-protocol operation. 
        /// </summary>
        /// <param name="portState">The new state of the logical antenna port. </param>
        /// <returns></returns>
        public Result SetAntennaPortState(AntennaPortState portState)
        {
            if (portState == AntennaPortState.UNKNOWN)
                return Result.INVALID_PARAMETER;

            return (m_Result = AntennaPortSetState(0, portState));
        }

        /// <summary>
        /// Allows an application to specify whether or not a radio module's 
        /// logical antenna port is enabled for subsequent tag operations.  The 
        /// antenna-port state cannot be set while a radio module is executing 
        /// a tag-protocol operation. 
        /// </summary>
        /// <param name="port">antenna port</param>
        /// <param name="portState">The new state of the logical antenna port.</param>
        /// <returns></returns>
        public Result SetAntennaPortState(uint port, AntennaPortState portState)
        {
            if (portState == AntennaPortState.UNKNOWN)
                return Result.INVALID_PARAMETER;

            m_AntennaList[(int)port].State = portState;

            return (m_Result = AntennaPortSetState(port, portState));
        }

        public Result AntennaPortSetState(UInt32 antennaPort, AntennaPortState state)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    AntennaPortSetState_CS108(antennaPort, state);
                    break;

                case MODEL.CS710S:
                    AntennaPortSetState_CS710S(antennaPort, state);
                    break;
            }

            return Result.FAILURE;
        } // Radio::SetAntennaPortState


        /// <summary>
        /// Allows an application to retrieve a single logical antenna port's 
        /// configuration parameters  e.g., dwell time, power level, and 
        /// number of inventory cycles.  Even if the logical antenna port is 
        /// disabled, an application is allowed to retrieve these configuration 
        /// parameters.  Retrieving configuration parameters does not cause a 
        /// logical antenna port to be automatically enabled; the application 
        /// must still enable the logical antenna port via 
        /// RFID_AntennaPortSetState.  The antenna-port configuration 
        /// cannot be retrieved while a radio module is executing a tag-
        /// protocol operation. 
        /// </summary>
        /// <param name="antenna">A structure that upon return will 
        /// contain the antenna-port configuration 
        /// parameters. </param>
        /// <returns>
        /// </returns>
        public Result GetAntennaPortConfiguration(ref AntennaPortConfig antenna)
        {
            AntennaPortGetConfiguration(0, antenna);

            return Result.OK;
        }

        /// <summary>
        /// Allows an application to retrieve a single logical antenna port's 
        /// configuration parameters  e.g., dwell time, power level, and 
        /// number of inventory cycles.  Even if the logical antenna port is 
        /// disabled, an application is allowed to retrieve these configuration 
        /// parameters.  Retrieving configuration parameters does not cause a 
        /// logical antenna port to be automatically enabled; the application 
        /// must still enable the logical antenna port via 
        /// RFID_AntennaPortSetState.  The antenna-port configuration 
        /// cannot be retrieved while a radio module is executing a tag-
        /// protocol operation. 
        /// </summary>
        /// <param name="port">antenna-port</param>
        /// <param name="antenna">A structure that upon return will 
        /// contain the antenna-port configuration 
        /// parameters. </param>
        /// <returns>
        /// </returns>
        public Result GetAntennaPortConfiguration(uint port, ref AntennaPortConfig antenna)
        {
            AntennaPortGetConfiguration(port, antenna);

            return Result.OK;
        }

        Result AntennaPortGetConfiguration(uint port, AntennaPortConfig antenna)
        {
            return Result.OK;
        }

        /// <summary>
        /// Allows an application to configure several parameters for a single 
        /// logical antenna port e.g.,  dwell time, power level, and number 
        /// of inventory cycles.  Even if the logical antenna port is disabled, 
        /// an application is allowed to set these configuration parameters.  
        /// Setting configuration parameters does not cause a logical antenna 
        /// port to be automatically enabled; the application must still enable 
        /// the logical antenna port via RFID_AntennaPortSetState.  The 
        /// antenna-port configuration cannot be set while a radio module is 
        /// executing a tag-protocol operation. 
        /// NOTE:  Since RFID_AntennaPortSetConfiguration sets all of the 
        /// configuration parameters that are present in the 
        /// RFID_ANTENNA_PORT_CONFIG structure, if an application wishes to 
        /// leave some parameters unchanged, the application should first call 
        /// RFID_AntennaPortGetConfiguration to retrieve the current 
        /// settings, update the values in the structure that are to be 
        /// changed, and then call RFID_AntennaPortSetConfiguration. 
        /// </summary>
        /// <param name="antenna">A structure that contains the 
        /// antenna-port configuration parameters.  This 
        /// parameter must not be NULL.  In version 1.1, 
        /// the physicalRxPort and physicalTxPort 
        /// fields must be the same. </param>
        /// <returns></returns>
        public Result SetAntennaPortConfiguration(AntennaPortConfig antenna)
        {
            if (antenna == null)
                return Result.INVALID_PARAMETER;

            return (m_Result = AntennaPortSetConfiguration(0, antenna));
        }

        /// <summary>
        /// Allows an application to configure several parameters for a single 
        /// logical antenna port e.g.,  dwell time, power level, and number 
        /// of inventory cycles.  Even if the logical antenna port is disabled, 
        /// an application is allowed to set these configuration parameters.  
        /// Setting configuration parameters does not cause a logical antenna 
        /// port to be automatically enabled; the application must still enable 
        /// the logical antenna port via RFID_AntennaPortSetState.  The 
        /// antenna-port configuration cannot be set while a radio module is 
        /// executing a tag-protocol operation. 
        /// NOTE:  Since RFID_AntennaPortSetConfiguration sets all of the 
        /// configuration parameters that are present in the 
        /// RFID_ANTENNA_PORT_CONFIG structure, if an application wishes to 
        /// leave some parameters unchanged, the application should first call 
        /// RFID_AntennaPortGetConfiguration to retrieve the current 
        /// settings, update the values in the structure that are to be 
        /// changed, and then call RFID_AntennaPortSetConfiguration. 
        /// </summary>
        /// <param name="port">antenna-port</param>
        /// <param name="antenna">A structure that contains the 
        /// antenna-port configuration parameters.  This 
        /// parameter must not be NULL.  In version 1.1, 
        /// the physicalRxPort and physicalTxPort 
        /// fields must be the same. </param>
        /// <returns></returns>
        public Result SetAntennaPortConfiguration(uint port, AntennaPortConfig antenna)
        {
            if (antenna == null)
                return Result.INVALID_PARAMETER;

            if (antenna.powerLevel > GetSoftwareMaxPowerLevel(m_save_region_code))
                return (m_Result = Result.INVALID_PARAMETER);

            m_AntennaList[(int)port].AntennaConfig = antenna;

            return (m_Result = AntennaPortSetConfiguration(port, antenna));
        }

        private Result SetAntennaPortConfiguration(uint virtual_port, uint physical_port)
        {
            AntennaPortConfig antenna = new AntennaPortConfig();

            if ((m_Result = AntennaPortGetConfiguration(virtual_port, antenna)) != Result.OK)
                return m_Result;

            return (m_Result = AntennaPortSetConfiguration(virtual_port, antenna));
        }

        Result AntennaPortSetConfiguration(uint port, AntennaPortConfig antenna)
        {
            SetPowerLevel((UInt16)(antenna.powerLevel * 10), (byte)port);
            SetInventoryDuration  ((UInt16)antenna.dwellTime, (byte)port);


            //RFIDRegister.AntennaPortConfig.SetPower((UInt16)(antenna.powerLevel * 10), (byte)port);
            //RFIDRegister.AntennaPortConfig.SetDwell((UInt16)antenna.dwellTime, (byte)port);

            //MacWriteRegister(MACREGISTER.HST_ANT_DESC_INV_CNT, antenna.numberInventoryCycles);





#if forreference

            UInt32 registerValue = 0;

            // First, tell the MAC which antenna descriptors we'll be reading and
            // verify that it was a valid selector
            MacWriteRegister( MACREGISTER.HST_ANT_DESC_SEL, port);

            /*
            MacReadRegister(MACREGISTER.MAC_ERROR, ref registerValue);

            if (registerValue == HOSTIF_ERR_SELECTORBNDS)
            {
                MacClearError();
                return Result.INVALID_PARAMETER;
            }
            */

            // Write the antenna dwell, RF power, inventory cycle count, and sense
            // resistor threshold registers
            MacWriteRegister(MACREGISTER.HST_ANT_DESC_DWELL, antenna.dwellTime);

            MacWriteRegister(MACREGISTER.HST_ANT_DESC_RFPOWER, antenna.powerLevel);

            MacWriteRegister(MACREGISTER.HST_ANT_DESC_INV_CNT, antenna.numberInventoryCycles);
#endif

            return Result.OK;
        }

        public uint GetAntennaPort()
        {
            if (m_AntennaList == null)
                return 1;

            return (uint)m_AntennaList.Count;
		}

    }
}
