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
using System.Reflection;

namespace CSLibrary
{
    using static RFIDDEVICE;
    using Constants;
    using Structures;
    using Events;
    using Tools;


    public partial class RFIDReader
    {
        #region public variable

        internal int _readerMode = 0;
        internal MODEL _deviceType = MODEL.UNKNOWN;

        #region ====================== Callback Event Handler ======================
        /// <summary>
        /// Reader Operation State Event
        /// </summary>
        public event EventHandler<CSLibrary.Events.OnStateChangedEventArgs> OnStateChanged;

        /// <summary>
        /// Tag Inventory(including Inventory and search) callback event
        /// </summary>
        public event EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs> OnAsyncCallback;

        /// <summary>
        /// Tag Access (including Tag read/write/kill/lock) completed event
        /// </summary>
        public event EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs> OnAccessCompleted;

        /// <summary>
        /// Tag Access (including Tag read/write/kill/lock) completed event
        /// </summary>
        public event EventHandler<CSLibrary.Events.OnInventoryTagRateCallbackEventArgs> OnInventoryTagRateCallback;
        #endregion
        #endregion

        /// <summary>
        /// CSLibrary Operation parameters
        /// Notes : you must config this parameters before perform any operation
        /// </summary>
        public CSLibraryOperationParms Options
        {
            get { return m_rdr_opt_parms; }
            set { m_rdr_opt_parms = value; }
        }
        //public UInt32 LastMacErrorCode;
        //public UInt16 LastResultCode = 0;

        internal HighLevelInterface _deviceHandler;
        private RFState m_state = RFState.IDLE;
        private Result m_Result;
        internal MODEL m_oem_machine;
        private string m_PCBAssemblyCode;

        /// <summary>
        /// Current Operation State
        /// </summary>
        public RFState State
        {
            get { { return m_state; } }
            private set { { m_state = value; } }
        }

        public void ClearEventHandler()
        {
            OnStateChanged = null;
            OnAsyncCallback = null;
            OnAccessCompleted = null;
            OnInventoryTagRateCallback = null;
            //OnFM13DTAccessCompleted = null;
        }

        public void ClearOnAsyncCallback()
        {
            OnAsyncCallback = null;
        }


        public MODEL GetModel()
        {
            return m_oem_machine;
        }

        public uint GetCountry()
        {
            return m_oem_country_code;
        }
        
        /*
                public string GetCountryCode()
                {
                    m_save_country_code = 2;
                    m_oem_freq_modification_flag = 0;
                    string country = "-" + m_save_country_code.ToString();

                    switch (m_save_country_code)
                    {
                        case 2:
                            if (m_oem_freq_modification_flag == 0)
                            {
                                country += " RW";
                            }
                            else
                            {
                                switch (m_oem_special_country_version)
                                {
                                    case 0x4f464341:
                                        country += " OFCA";
                                        break;
                                    case 0x2a2a4153:
                                        country += " AS";
                                        break;
                                    case 0x2a2a4e5a:
                                        country += " NZ";
                                        break;
                                    case 0x20937846: 
                                        country += " ZA";
                                        break;
                                    case 0x2A2A5347:
                                        country += " SG";
                                        break;
                                }
                            }
                            break;

                        case 8:
                            switch (m_oem_special_country_version)
                            {
                                case 0x2A4A5036:
                                    country += " JP6";
                                    break;
                            }
                            break;
                    }
                    return country;
                }
        */

        public string GetPCBAssemblyCode()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return m_PCBAssemblyCode;

                case MODEL.CS710S:
                    return RFIDRegister.EF9C.value;
            }

            return "";
        }

        internal static void ArrayCopy(byte[] src, int srcOffset, UInt16[] dest, int destOffset, int byteSize)
        {
            int len = byteSize / 2;

            if ((byteSize % 2) != 0 || (src.Length - srcOffset) < byteSize || (dest.Length - destOffset) < len)
            {
                return;
                //throw new ArgumentException();
            }

            for (int cnt = 0; cnt < len; cnt++)
                dest[destOffset + cnt] = (UInt16)(src[srcOffset + cnt * 2] << 8 | src[srcOffset + cnt * 2 + 1]);
        }

        private String uint32ArrayToString(UInt32[] source)
        {
            StringBuilder sb = new StringBuilder();

            // Byte at offset is total byte len, 2nd byte is always 3

            for (int index = 0; index < source.Length; index++)
            {
                sb.Append((Char)(source[index] >> 24 & 0x000000FF));
                sb.Append((Char)(source[index] >> 16 & 0x000000FF));
                sb.Append((Char)(source[index] >> 8 & 0x000000FF));
                sb.Append((Char)(source[index] >> 0 & 0x000000FF));
            }

            return sb.ToString();
        }

        internal CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE DeviceRecvData(byte[] recvData, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE currentCommandResponse)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return DeviceRecvData_CS108(recvData, currentCommandResponse);

                case MODEL.CS710S:
                    return DeviceRecvData_CS710S(recvData, currentCommandResponse);
            }

            return HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.ENDEVENTUPLINKPACKET;
        }

        public Result CancelAllSelectCriteria()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return CancelAllSelectCriteria_CS108();

                case MODEL.CS710S:
                    return CancelAllSelectCriteria_CS710S();
            }

            return Result.FAILURE;
        }


#region ====================== Set Tag Group ======================
        /// <summary>
        /// Get Tag Group
        /// </summary>
        /// <param name="gpSelect"></param>
        /// <returns></returns>
        public Result SetTagGroup(Selected gpSelect)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetTagGroup_CS108 (gpSelect);

                case MODEL.CS710S:
                    return SetTagGroup_CS710S(gpSelect);
            }

            return (m_Result = Result.FAILURE);
        }

        /// <summary>
        /// Once the tag population has been partitioned into disjoint groups, a subsequent 
        /// tag-protocol operation (i.e., an inventory operation or access command) is then 
        /// applied to one of the tag groups. 
        /// </summary>
        /// <param name="gpSelect">Specifies the state of the selected (SL) flag for tags that will have 
        /// the operation applied to them. </param>
        /// <param name="gpSession">Specifies which inventory session flag (i.e., S0, S1, S2, or S3) 
        /// will be matched against the inventory state specified by target. </param>
        /// <param name="gpSessionTarget">Specifies the state of the inventory session flag (i.e., A or B),
        /// specified by session, for tags that will have the operation 
        /// applied to them. </param>
        public Result SetTagGroup(Selected gpSelect, Session gpSession, SessionTarget gpSessionTarget)
		{
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetTagGroup_CS108(gpSelect, gpSession, gpSessionTarget);

                case MODEL.CS710S:
                    return SetTagGroup_CS710S(gpSelect, gpSession, gpSessionTarget);
            }

            return (m_Result = Result.FAILURE);
        }

        /// <summary>
        /// Once the tag population has been partitioned into disjoint groups, a subsequent 
        /// tag-protocol operation (i.e., an inventory operation or access command) is then 
        /// applied to one of the tag groups. 
        /// </summary>
        /// <param name="tagGroup"><see cref="TagGroup"/></param>
        /// <returns></returns>
        public Result SetTagGroup(TagGroup tagGroup)
		{
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetTagGroup_CS108(tagGroup);

                case MODEL.CS710S:
                    return SetTagGroup_CS710S(tagGroup);
            }

            return (m_Result = Result.FAILURE);
        }
        /// <summary>
        /// Get Tag Group
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public Result GetTagGroup(ref TagGroup tagGroup)
		{
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetTagGroup_CS108(ref tagGroup);

                case MODEL.CS710S:
                    return GetTagGroup_CS710S(ref tagGroup);
            }

            return (m_Result = Result.FAILURE);
        }
        #endregion


        // public RFID function
        internal void PowerOn()
		{
			CSLibrary.Debug.WriteLine("DateTime {0}", DateTime.Now);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDPOWERON);
		}

		internal void PowerOff()
		{
			CSLibrary.Debug.WriteLine("DateTime {0}", DateTime.Now);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDPOWEROFF);
		}

        internal UInt32 _InventoryCycleDelay = 0x00;


        /// <summary>
        /// Set Compact Inventory Delay Time (for CS108 only) 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public bool SetTagDelayTime(UInt32 ms)
        {
            switch(_deviceType)
            {
                case MODEL.CS108:
                    return SetTagDelayTime_CS108(ms);
            }

            return false;
        }


        /// <summary>
        /// Set Intra Packet Delay Time (for CS710S only)
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>        
        public bool SetIntraPacketDelayTime(UInt32 ms)
        {
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    return SetTagDelayTime_CS710S(ms);
            }

            return false;
        }



        #region Public Functions

        public ClassEM4325 EM4325 = null;
        public ClassFM13DT160 FM13DT160 = null;

        internal RFIDReader(HighLevelInterface deviceHandler)
		{
			_deviceHandler = deviceHandler;

            // Special Module
            EM4325 = new ClassEM4325(deviceHandler);
            FM13DT160 = new ClassFM13DT160(deviceHandler);
        }

        ~RFIDReader()
		{
		}


        internal void Connect(MODEL deviceType)
		{
            this._deviceType = deviceType;

            switch (_deviceType)
            {
                case MODEL.CS108:
                    Connect_CS108();
                    break;

                case MODEL.CS710S:
                    Connect_CS710S();
                    break;
            }

            return;
        }

        internal void Reconnect()
		{
            switch (_deviceType)
            {
                case MODEL.CS108:
                    Reconnect_CS108();
                    break;

                case MODEL.CS710S:
                    //Reconnect_CS710S();
                    break;
            }

            return;
        }

        public string GetFirmwareVersionString()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetFirmwareVersionString_CS108();

                case MODEL.CS710S:
                    return GetFirmwareVersionString_CS710S();
            }

            return "";
        }

        /// <summary>
        /// Retrieves the operation mode for the RFID radio module.  The 
        /// operation mode cannot be retrieved while a radio module is 
        /// executing a tag-protocol operation. 
        /// </summary>
        /// <param name="cycles">The number of antenna cycles to be completed for command execution.
        /// <para>0x0001 = once cycle through</para>
        /// <para>0xFFFF = cycle forever until a CANCEL is received.</para></param>
        /// <param name="mode">Antenna Sequence mode.</param>
        /// <param name="sequenceSize">Sequence size. Maximum value is 48</param>
        /// <returns></returns>
        public Result GetOperationMode(ref ushort cycles, ref AntennaSequenceMode mode, ref uint sequenceSize)
		{
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    return GetOperationMode(ref cycles, ref mode, ref sequenceSize);
            }

            return Result.FAILURE;
        }

        /// <summary>
        /// Retrieves the operation mode for the RFID radio module.  The 
        /// operation mode cannot be retrieved while a radio module is 
        /// executing a tag-protocol operation. 
        /// </summary>
        /// <param name="mode"> return will receive the current operation mode.</param>
        /// <returns></returns>
        public void GetOperationMode(ref RadioOperationMode mode)
		{
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    break;
            }

            return;
		}

		/// <summary>
		/// Sets the operation mode of RFID radio module.  By default, when 
		/// an application opens a radio, the RFID Reader Library sets the 
		/// reporting mode to non-continuous.  An RFID radio module's 
		/// operation mode will remain in effect until it is explicitly changed 
		/// via RFID_RadioSetOperationMode, or the radio is closed and re-
		/// opened (at which point it will be set to non-continuous mode).  
		/// The operation mode may not be changed while a radio module is 
		/// executing a tag-protocol operation. 
		/// </summary>
		/// <param name="mode">The operation mode for the radio module.</param>
		/// <returns></returns>
		public Result SetOperationMode(RadioOperationMode mode)
		{
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    return SetOperationMode_CS710S(mode);
            }

            return Result.FAILURE;
        }

        /// <summary>
        /// This is used to set inventory duration
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public Result SetInventoryDuration(uint duration, uint antennaPort = 0)
		{
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetInventoryDuration_CS108(duration, antennaPort);

                case MODEL.CS710S:
                    return SetInventoryDuration_CS710S(duration, antennaPort);
            }

            return Result.FAILURE;
		}

        public Result SetInventoryDuration(uint [] duration)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetInventoryDuration_CS108(duration);

                case MODEL.CS710S:
                    return SetInventoryDuration_CS710S(duration);
            }

            return Result.FAILURE;
        }

        /// <summary>
        /// Configures the tag-selection criteria for the ISO 18000-6C select 
        /// command.  The supplied tag-selection criteria will be used for any 
        /// tag-protocol operations (i.e., Inventory, etc.) in 
        /// which the application specifies that an ISO 18000-6C select 
        /// command should be issued prior to executing the tag-protocol 
        /// operation (i.e., the SelectFlags.SELECT flag is provided to 
        /// the appropriate RFID_18K6CTag* function).  The tag-selection 
        /// criteria will stay in effect until the next call to 
        /// SetSelectCriteria.  Tag-selection criteria may not 
        /// be changed while a radio module is executing a tag-protocol 
        /// operation. 
        /// </summary>
        /// <param name="critlist">
        /// SelectCriteria array, containing countCriteria entries, of selection 
        /// criterion structures that are to be applied sequentially, beginning with 
        /// pCriteria[0], to the tag population.  If this field is NULL, 
        /// countCriteria must be zero. 
        ///</param>
        /// <returns></returns>
        public Result SetSelectCriteria(SelectCriterion[] critlist)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetSelectCriteria_CS108(critlist);

                case MODEL.CS710S:
                    return SetSelectCriteria_CS710S(critlist);
            }

            return Result.FAILURE;
        }

        public Result SetSelectCriteria(uint index, SelectCriterion crit)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetSelectCriteria_CS108(index, crit);

                case MODEL.CS710S:
                    return SetSelectCriteria_CS710S(index, crit);
            }

            return Result.FAILURE;
        }

        public Result SetPostMatchCriteria(SingulationCriterion[] postmatch)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetPostMatchCriteria_CS108(postmatch);

                    //                case MODEL.CS710S:
                    //                    return SetSelectCriteria_CS710S(critlist);
            }

            return Result.FAILURE;
        }


        #endregion

    }
}