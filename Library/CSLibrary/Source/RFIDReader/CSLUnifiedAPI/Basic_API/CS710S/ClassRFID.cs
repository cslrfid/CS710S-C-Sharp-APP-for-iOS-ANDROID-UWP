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
    using Constants;
    using Structures;
    using Events;
    using Tools;

    public partial class RFIDReader
    {
        // 0x3002 packet
        void csl_tag_read_epc_only_recurrent_packet_proc(byte[] data, int index)
        {

		}

        // 0x3004 packet
        void csl_tag_read_multibank_recurrent_index_only_packet_proc(byte[] data, int index)
        {

		}

        // 0x3008 packet
        void csl_operation_complete_packet_proc(byte[] data, int index)
        {
            UInt32 UTCTimeStamp = Tools.Hex.MSBToUInt32(data, index);
            UInt16 commandCode = Tools.Hex.MSBToUInt16(data, index + 4);
            UInt16 status = Tools.Hex.MSBToUInt16(data, index + 6);

            Debug.WriteLine("{0:X4} : 0x3008, {1:X2}, {2:X2}", UTCTimeStamp, commandCode, status);

            LastMacErrorCode = status;

            switch (commandCode)
            {
                case 0x10b8:
                    FireAccessCompletedEvent(
                    new CSLibrary.Events.OnAccessCompletedEventArgs(
                    (status == 0x00),
                        Bank.UNKNOWN,
                        TagAccess.KILL,
                        null));
                    break;
            }

            FireStateChangedEvent(RFState.IDLE);
        }

        // 0x1471
        void ReadRegister_packet_proc(byte[] data, int index)
        {
            // 33:A7 B3 19 C2 82 9E 02 9B 81 00     51 E2 14 71    00   00 10     01 07 D0 0B B8 00 06 30 F7 00 00 00 08 01 05 00
            //    A7 B3 73 C2 00 9E B3 F4 81 00     51 E2 14 71    00   00 6A     31 2E 31 2E 30 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 2F 00 00 00 A2 20 1A 06 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF


            int size = (data[15] << 8) + (data[16]);

            if (size == 101) // OEM packet data len
                SaveInitRegisters(index, data, size);
            //else
            //    SaveRegister(17, data, size);


            /*
                        int seq = data[index];
                        index++;

                        do
                        {
                            int len = data[index] << 8 | data[index + 1];

                            if (index + len > data.Length)
                                break;

                            index += 2;
            //                Array.Copy(data, index, buffer, 0, len);
                            index += len;
                        } while (index < data.Length);
            */

            // Init setting after read registers
            InitDefaultChannel_CS710S();
            GenCountryList_CS710S();
            SetDefaultAntennaList();
            FireStateChangedEvent(RFState.INITIALIZATION_COMPLETE);
            FireStateChangedEvent(RFState.IDLE);
        }


        internal CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE DeviceRecvData_CS710S(byte[] recvData, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE currentCommandResponse)
        {
            CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE result = HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1;

            CSLibrary.Debug.WriteLine("Routine : DeviceRecvData");
            int index = 10; // header size

            do
            {
                int header = Tools.Hex.MSBToUInt16(recvData, index);
                int packetcode = Tools.Hex.MSBToUInt16(recvData, index + 2);
                int seqnum = recvData[index + 4];
                int payloadlen = Tools.Hex.MSBToUInt16(recvData, index + 5);

                index += 7;
                switch (header)
                {
                    case 0x49dc:
                        {
                            switch (packetcode)
                            {
                                case 0x3001:
                                    csl_tag_read_epc_only_new_packet_proc(recvData, index);
                                    break;

                                case 0x3002:
                                    csl_tag_read_epc_only_recurrent_packet_proc(recvData, index);
                                    break;

                                case 0x3003:
                                    csl_tag_read_multibank_new_packet_proc(recvData, index);
                                    break;

                                case 0x3004:
                                    csl_tag_read_multibank_recurrent_index_only_packet_proc(recvData, index);
                                    break;

                                case 0x3006:
                                    csl_tag_read_compact_packet_proc(recvData, index);
                                    break;

                                case 0x3007:
                                    csl_miscellaneous_event_packet_proc(recvData, index, payloadlen - 6);
                                    break;

                                case 0x3008:
                                    csl_operation_complete_packet_proc(recvData, index);
                                    result = HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.CSL_OPERATION_COMPLETE;
                                    break;

                                case 0x3009:
                                    csl_access_complete_packet_proc(recvData, index, payloadlen - 12);
                                    break;

                                default:
                                    break;
                            }
                        }
                        break;


                    case 0x51e2:
                        {
                            switch (packetcode)
                            {
                                case 0x1471:
                                    ReadRegister_packet_proc(recvData, index);
                                    break;

                                case 0x10b1: // read tag
                                    result = HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.ENDEVENTUPLINKPACKET;
                                    break;

                                case 0x10b7: // read tag
                                    result = HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.ENDEVENTUPLINKPACKET;
                                    break;

                                case 0x9a06: // write registers
                                    result = HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.COMMANDENDRESPONSE;
                                    break;
                            }
                        }
                        break;
                }

                index += payloadlen;
            } while (index < recvData.Length);

            return result;
        }

        public Result CancelAllSelectCriteria_CS710S()
        {
            if (RFIDRegister == null)
                return Result.FAILURE;

            RFIDRegister.SelectConfiguration.Enable(0, 6, false);

            return Result.OK;
        }

#region ====================== Set Tag Group ======================
        /// <summary>
        /// Get Tag Group
        /// </summary>
        /// <param name="gpSelect"></param>
        /// <returns></returns>
        public Result SetTagGroup_CS710S(Selected gpSelect)
        {
            RFIDRegister.AntennaPortConfig.Select((uint)gpSelect);

            return (m_Result = Result.OK);
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
        public Result SetTagGroup_CS710S(Selected gpSelect, Session gpSession, SessionTarget gpSessionTarget)
		{
            RFIDRegister.AntennaPortConfig.TagGroup((uint)gpSession, (uint)gpSelect, (uint)gpSessionTarget);

            return (m_Result = Result.OK);
        }

        /// <summary>
        /// Once the tag population has been partitioned into disjoint groups, a subsequent 
        /// tag-protocol operation (i.e., an inventory operation or access command) is then 
        /// applied to one of the tag groups. 
        /// </summary>
        /// <param name="tagGroup"><see cref="TagGroup"/></param>
        /// <returns></returns>
        public Result SetTagGroup_CS710S(TagGroup tagGroup)
		{
            RFIDRegister.AntennaPortConfig.TagGroup((uint)tagGroup.session, (uint)tagGroup.selected, (uint)tagGroup.target);

            return (m_Result = Result.OK);
        }
        /// <summary>
        /// Get Tag Group
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        internal Result GetTagGroup_CS710S(ref TagGroup tagGroup)
		{
            //RFIDRegister.AntennaPortConfig.TagGroup((uint)tagGroup.session, (uint)tagGroup.selected, (uint)tagGroup.target);
        
            return (m_Result = Result.NOT_SUPPORTED);
		}
#endregion

        public bool SetTagDelayTime_CS710S(UInt32 ms)
        {
            if (ms > 0xff)
                return false;

            //_INVENTORYDELAYTIME = (ms << 20);

            RFIDRegister.IntraPacketDelay.Set((byte)ms);

            return true;
        }

        internal Result CancelAllSelectCriteria_CS701S()
        {
            for (uint cnt = 0; cnt < 7; cnt++)
            {
                SetSelectCriteria_CS710S(cnt, null);
            }

            return Result.OK;
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

        internal Result SetSelectCriteria_CS710S(SelectCriterion[] critlist)
        {
            uint index;
            uint registerValue;

            if (critlist == null || critlist.Length == 0)
                return Result.INVALID_PARAMETER;

            try
            {
                for (index = 0; index < critlist.Length; index++)
                    SetSelectCriteria_CS710S(index, critlist[index]);


/*
                SelectCriteria SC = new SelectCriteria();
                SC.countCriteria = (uint)critlist.Length;
                SC.pCriteria = (SelectCriterion[])critlist.Clone();

                index = 0;
                {
                    SelectCriterion pCriterion = SC.pCriteria[index];
                    SelectMask pMask = pCriterion.mask;
                    SelectAction pAction = pCriterion.action;

                    // Instruct the MAC as to which select mask we want to work with
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_SEL, index);

                    // Create the HST_TAGMSK_DESC_CFG register value and write it to the MAC
                    registerValue = (0x01 |
                        (((uint)(pAction.target) & 0x07) << 1) |
                        (((uint)(pAction.action) & 0x07) << 4) |
                        (pAction.enableTruncate != 0x00 ? (uint)(1 << 7) : 0)) |
                        (((uint)(pAction.delay) & 0xFF) << 8);
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_CFG, registerValue);

                    // Create the HST_TAGMSK_BANK register value and write it to the MAC
                    registerValue = (uint)pMask.bank;
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_BANK, registerValue);

                    // Write the mask offset to the HST_TAGMSK_PTR register
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_PTR, (uint)pMask.offset);

                    // Create the HST_TAGMSK_LEN register and write it to the MAC
                    registerValue = (uint)(pMask.count);
                    MacWriteRegister(MACREGISTER.HST_TAGMSK_LEN, registerValue);

                    // Now write the MAC's mask registers
                    WriteMacMaskRegisters((ushort)MACREGISTER.HST_TAGMSK_0_3, pMask.count, pMask.mask);
                    // Set up the selection criteria
                }
*/
            }
            catch (System.Exception ex)
            {
#if DEBUG
                //				CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.SetSelectCriteria()", ex);
#endif
                return Result.SYSTEM_CATCH_EXCEPTION;
            }
            return m_Result;
        }

        internal Result SetSelectCriteria_CS710S(uint index, SelectCriterion crit)
        {
            try
            {
                if (crit == null)
                {
                    RFIDRegister.SelectConfiguration.Enable(index, false);
                    return Result.OK;
                }

                byte[] a = crit.mask.mask;

                RFIDRegister.SelectConfiguration.Set((int)index, true, (byte)crit.mask.bank, crit.mask.offset, (byte)crit.mask.count, crit.mask.mask, (byte)crit.action.target, (byte)crit.action.action, (byte)crit.action.delay);

                return Result.OK;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                //CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.TagSelected()", ex);
#endif
                m_Result = CSLibrary.Constants.Result.SYSTEM_CATCH_EXCEPTION;
            }

            return Result.FAILURE;






            /*
                        uint registerValue;

                        // Instruct the MAC as to which select mask we want to work with
                        MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_SEL, index);

                        if (crit == null)
                        {
                            MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_CFG, 0x0000);
                            //MacWriteRegister(MACREGISTER.HST_TAGMSK_BANK, 0x0000);
                            //MacWriteRegister(MACREGISTER.HST_TAGMSK_PTR, 0x0000);
                            //MacWriteRegister(MACREGISTER.HST_TAGMSK_LEN, 0x0000);
                            return Result.OK;
                        }

                        try
                        {
                            {
                                SelectCriterion pCriterion = crit;
                                SelectMask pMask = pCriterion.mask;
                                SelectAction pAction = pCriterion.action;

                                // Create the HST_TAGMSK_DESC_CFG register value and write it to the MAC
                                registerValue = (0x01 |
                                    (((uint)(pAction.target) & 0x07) << 1) |
                                    (((uint)(pAction.action) & 0x07) << 4) |
                                    (pAction.enableTruncate != 0x00 ? (uint)(1 << 7) : 0)) |
                                    (((uint)(pAction.delay) & 0xFF) << 8);
                                MacWriteRegister(MACREGISTER.HST_TAGMSK_DESC_CFG, registerValue);

                                // Create the HST_TAGMSK_BANK register value and write it to the MAC
                                registerValue = (uint)pMask.bank;
                                MacWriteRegister(MACREGISTER.HST_TAGMSK_BANK, registerValue);

                                // Write the mask offset to the HST_TAGMSK_PTR register
                                MacWriteRegister(MACREGISTER.HST_TAGMSK_PTR, (uint)pMask.offset);

                                // Create the HST_TAGMSK_LEN register and write it to the MAC
                                registerValue = (uint)(pMask.count);
                                MacWriteRegister(MACREGISTER.HST_TAGMSK_LEN, registerValue);

                                // Now write the MAC's mask registers
                                WriteMacMaskRegisters((ushort)MACREGISTER.HST_TAGMSK_0_3, pMask.count, pMask.mask);
                                // Set up the selection criteria
                            }
                        }
                        catch (System.Exception ex)
                        {
            #if DEBUG
                            //				CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.SetSelectCriteria()", ex);
            #endif
                            return Result.SYSTEM_CATCH_EXCEPTION;
                        }
                        return m_Result;
            */
        }

        #region Public Functions

        internal void Connect_CS710S()
		{
            RegisterInitialize_CS710S();

            //RFIDRegister.EventPacketUplinkEnable.Set(0x0D);
            RFIDRegister.EventPacketUplinkEnable.Set(0x09);

            //ReadReaderOEMData();
        }
        public string GetFirmwareVersionString_CS710S()
        {
            return RFIDRegister.VersionString.Get();
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
        public Result SetOperationMode_CS710S(RadioOperationMode mode)
		{
            return Result.NOT_SUPPORTED; // CS710S 
		}

        /// <summary>
        /// This is used to set inventory duration
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public Result SetInventoryDuration_CS710S(uint duration, uint antennaPort = 0)
		{
            RFIDRegister.AntennaPortConfig.SetDwell((UInt16)duration, (byte)antennaPort);

			return Result.OK;
		}

        public Result SetInventoryDuration_CS710S(uint [] duration)
        {
            Result r;

			for (uint cnt = 0; cnt < duration.Length; cnt++)
                if ((r = SetInventoryDuration_CS710S(duration[cnt], cnt)) != Result.OK)
                    return r;

            return Result.OK;
        }

#endregion

    }
}