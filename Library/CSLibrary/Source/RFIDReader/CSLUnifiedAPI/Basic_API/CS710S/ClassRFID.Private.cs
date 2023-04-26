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
        private enum RFIDREADERCMDSTATUS
        {
            IDLE,           // Can send (SetRegister, GetRegister, ExecCmd, Abort), Can not receive data
            GETREGISTER,    // Can not send data, Can receive (GetRegister) 
            EXECCMD,        // Can send (Abort), Can receive (CMDBegin, CMDEnd, Inventory data, Abort)
            INVENTORY,      // Can send (Abort)
            ABORT,          // Can not send
        }

        void Start18K6CRequest_CS710S(uint tagStopCount, SelectFlags flags)
        {
            if ((flags & SelectFlags.SELECT) != 0)
            {
                RFIDRegister.AntennaPortConfig.Select(3);
            }
        }

        private void TagLockThreadProc_CS710S()
        {
            const UInt16 HST_TAGACC_LOCKCFG_MASK_USE_PWD_ACTION = 0x1;
            const UInt16 HST_TAGACC_LOCKCFG_MASK_USE_PERMA_ACTION = 0x2;

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

            const byte HST_TAGACC_LOCKCFG_MASK_USER_SHIFT = 0;
            const byte HST_TAGACC_LOCKCFG_MASK_TID_SHIFT = (HST_TAGACC_LOCKCFG_MASK_USER_SHIFT + HST_TAGACC_LOCKCFG_MASK_USER_SIZE);
            const byte HST_TAGACC_LOCKCFG_MASK_EPC_SHIFT = (HST_TAGACC_LOCKCFG_MASK_TID_SHIFT + HST_TAGACC_LOCKCFG_MASK_TID_SIZE);
            const byte HST_TAGACC_LOCKCFG_MASK_ACC_SHIFT = (HST_TAGACC_LOCKCFG_MASK_EPC_SHIFT + HST_TAGACC_LOCKCFG_MASK_EPC_SIZE);
            const byte HST_TAGACC_LOCKCFG_MASK_KILL_SHIFT = (HST_TAGACC_LOCKCFG_MASK_ACC_SHIFT + HST_TAGACC_LOCKCFG_MASK_ACC_SIZE);

            const byte HST_TAGACC_LOCKCFG_RFU1_SHIFT = (HST_TAGACC_LOCKCFG_MASK_KILL_SHIFT + HST_TAGACC_LOCKCFG_MASK_KILL_SIZE);

            /* Constants for HST_TAGACC_LOCKCFG register bit fields (note that the      */
            /* values are already shifted into the low-order bits of the constant.      */
            const UInt16 HST_TAGACC_LOCKCFG_ACTION_MEM_WRITE = 0x0;
            const UInt16 HST_TAGACC_LOCKCFG_ACTION_MEM_PERM_WRITE = 0x1;
            const UInt16 HST_TAGACC_LOCKCFG_ACTION_MEM_SEC_WRITE = 0x2;
            const UInt16 HST_TAGACC_LOCKCFG_ACTION_MEM_NO_WRITE = 0x3;
            const UInt16 HST_TAGACC_LOCKCFG_ACTION_PWD_ACCESS = 0x0;
            const UInt16 HST_TAGACC_LOCKCFG_ACTION_PWD_PERM_ACCESS = 0x1;
            const UInt16 HST_TAGACC_LOCKCFG_ACTION_PWD_SEC_ACCESS = 0x2;
            const UInt16 HST_TAGACC_LOCKCFG_ACTION_PWD_NO_ACCESS = 0x3;
            const UInt16 HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION = 0x0;

            const UInt16 HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION = (HST_TAGACC_LOCKCFG_MASK_USE_PWD_ACTION | HST_TAGACC_LOCKCFG_MASK_USE_PERMA_ACTION);

            const UInt16 RFID_18K6C_TAG_PWD_PERM_ACCESSIBLE = 0x0;
            const UInt16 RFID_18K6C_TAG_PWD_PERM_ALWAYS_NOT_ACCESSIBLE = 0x1;
            const UInt16 RFID_18K6C_TAG_PWD_PERM_ALWAYS_ACCESSIBLE = 0x2;
            const UInt16 RFID_18K6C_TAG_PWD_PERM_SECURED_ACCESSIBLE = 0x3;
            const UInt16 RFID_18K6C_TAG_PWD_PERM_NO_CHANGE = 0x4;

            const UInt16 RFID_18K6C_TAG_MEM_PERM_WRITEABLE = 0x0;             //unlock		00
            const UInt16 RFID_18K6C_TAG_MEM_PERM_ALWAYS_NOT_WRITEABLE = 0x1;  //permlock		01
            const UInt16 RFID_18K6C_TAG_MEM_PERM_ALWAYS_WRITEABLE = 0x2;      //permunlock	10
            const UInt16 RFID_18K6C_TAG_MEM_PERM_SECURED_WRITEABLE = 0x3;     //lock			11
            const UInt16 RFID_18K6C_TAG_MEM_PERM_NO_CHANGE = 0x4;

            UInt16 maskValue = 0;
            UInt16 actionValue = 0;

            if (m_rdr_opt_parms.TagLock.permanentLock)
            {
                actionValue = 0x3ff;
            }
            else
            {
                if (RFID_18K6C_TAG_PWD_PERM_NO_CHANGE == (UInt16)m_rdr_opt_parms.TagLock.killPasswordPermissions)
                {
                    maskValue |= (HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_KILL_SHIFT);
                }
                // Otherwise, indicate to look at the kill password bits and set the
                // persmission for it
                else
                {
                    maskValue |= (HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_KILL_SHIFT);
                    actionValue |= (UInt16)((UInt16)m_rdr_opt_parms.TagLock.killPasswordPermissions << HST_TAGACC_LOCKCFG_ACTION_KILL_SHIFT);
                }

                // If the access password access permissions are not to change, then
                // indicate to ignore those bits.
                if (RFID_18K6C_TAG_PWD_PERM_NO_CHANGE == (UInt16)m_rdr_opt_parms.TagLock.accessPasswordPermissions)
                {
                    maskValue |= HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_ACC_SHIFT;
                }
                // Otherwise, indicate to look at the access password bits and set the
                // persmission for it
                else
                {
                    maskValue |= HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_ACC_SHIFT;
                    actionValue |= (UInt16)((UInt16)m_rdr_opt_parms.TagLock.accessPasswordPermissions << HST_TAGACC_LOCKCFG_ACTION_ACC_SHIFT);
                }

                // If the EPC memory access permissions are not to change, then indicate
                // to ignore those bits.
                if (RFID_18K6C_TAG_MEM_PERM_NO_CHANGE == (UInt16)m_rdr_opt_parms.TagLock.epcMemoryBankPermissions)
                {
                    maskValue |= HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_EPC_SHIFT;
                }
                // Otherwise, indicate to look at the EPC memory bits and set the
                // persmission for it
                else
                {
                    maskValue |= HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_EPC_SHIFT;
                    actionValue |= (UInt16)((UInt16)m_rdr_opt_parms.TagLock.epcMemoryBankPermissions << HST_TAGACC_LOCKCFG_ACTION_EPC_SHIFT);
                }

                // If the TID memory access permissions are not to change, then indicate
                // to ignore those bits.
                if (RFID_18K6C_TAG_MEM_PERM_NO_CHANGE == (UInt16)m_rdr_opt_parms.TagLock.tidMemoryBankPermissions)
                {
                    maskValue |= HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_TID_SHIFT;
                }
                // Otherwise, indicate to look at the TID memory bits and set the
                // persmission for it
                else
                {
                    maskValue |= HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_TID_SHIFT;
                    actionValue |= (UInt16)((UInt16)m_rdr_opt_parms.TagLock.tidMemoryBankPermissions << HST_TAGACC_LOCKCFG_ACTION_TID_SHIFT);
                }

                // If the user memory access permissions are not to change, then indicate
                // to ignore those bits.
                if (RFID_18K6C_TAG_MEM_PERM_NO_CHANGE == (UInt16)m_rdr_opt_parms.TagLock.userMemoryBankPermissions)
                {
                    maskValue |= HST_TAGACC_LOCKCFG_MASK_USE_NO_ACTION << HST_TAGACC_LOCKCFG_MASK_USER_SHIFT;
                }
                // Otherwise, indicate to look at the user memory bits and set the
                // persmission for it
                else
                {
                    maskValue |= HST_TAGACC_LOCKCFG_MASK_USE_BOTH_ACTION << HST_TAGACC_LOCKCFG_MASK_USER_SHIFT;
                    actionValue |= (UInt16)((UInt16)m_rdr_opt_parms.TagLock.userMemoryBankPermissions << HST_TAGACC_LOCKCFG_ACTION_USER_SHIFT);
                }

                // Set LockMask
                RFIDRegister.LockMask.Set(maskValue);

                // Set LockAction
                RFIDRegister.LockAction.Set(actionValue);

                // run 
                RFIDLockTag();
            }

            // Write to Tag


#if oldcode


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
#endif

        }

        private void TagBlockLockThreadProc_CS710S()
        {
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
/*        private bool RFID_18K6CTagKill()
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
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.KILL), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE);
            //if (COMM_HostCommand(HST_CMD.KILL) != Result.OK || CurrentOperationResult != Result.OK)
            //	return false;

            return true;
        } // RFID_18K6CTagKill
*/

        private void TagKillThreadProc_CS710S()
        {
            RFIDKillTag();

            m_Result = Result.OK;
            return;

/*            ushort[] tmp = new ushort[1];

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
            //FireStateChangedEvent(RFState.IDLE);*/
        }


        private void TagAuthenticateThreadProc_CS710S()
        {
        }

        private void TagReadBufferThreadProc_CS710S()
        {
        }

        private void TagUntraceableThreadProc_CS710S()
        {
            return;
        }


        private bool FM13DTReadMemoryThreadProc_CS710S()
        {
            return false;
        }

        private bool FM13DTWriteMemoryThreadProc_CS710S()
        {
            return false;
        }

        private bool FM13DTReadRegThreadProc_CS710S()
        {
            return true;
        }

        private bool FM13DTWriteRegThreadProc_CS710S()
        {
            return true;
        }

        private bool FM13DTAuthThreadProc_CS710S()
        {
//            FM13DT160_Auth(m_rdr_opt_parms.FM FM13DTWriteMemory.offset, m_rdr_opt_parms.FM13DTWriteMemory.count);
            return true;
        }

        private bool FM13DTGetTempThreadProc_CS710S()
        {
            //FM13DT160_GetTemp(m_rdr_opt_parms.FM13DTWriteMemory.offset, m_rdr_opt_parms.FM13DTWriteMemory.count, m_rdr_opt_parms.FM13DTWriteMemory.data);
            return true;
        }
        private bool FM13DTStartLogThreadProc_CS710S()
        {
            return true;
        }
        private bool FM13DTStopLogChkThreadProc_CS710S()
        {
            return true;
        }
        private bool FM13DTDeepSleepThreadProc_CS710S()
        {
            return true;
        }
        private bool FM13DTOpModeChkThreadProc_CS710S()
        {
            return true;
        }
        private bool FM13DTInitialRegFileThreadProc_CS710S()
        {
            return true;
        }

        private bool FM13DTLedCtrlThreadProc_CS710S()
        {
            return true;
        }

        /// <summary>
        /// rfid reader packet
        /// </summary>
        /// <param name="RW"></param>
        /// <param name="add"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal byte[] PacketData_CS710S(UInt16 add, UInt32? value = null)
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
    }
}
