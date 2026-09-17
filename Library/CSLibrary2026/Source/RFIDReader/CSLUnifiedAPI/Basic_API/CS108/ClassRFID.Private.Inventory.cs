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

namespace CSLibrary
{
    public partial class RFIDReader
    {
        //private void StartInventory()
        //{
        //    _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.INV), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);

            //_deviceHandler.rfid._dataBuffer.Clear();
            /*
            // Create a timer that waits one second, then invokes every second.
            Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(2000), () => {
                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0, 0xf000, 0x000f), (UInt32)SENDREMARK.INVENTORY);
                return true;
            });
            */
        //}

		private void TagRangingThreadProc_CS108()
		{
            _tagRangingParms = m_rdr_opt_parms.TagRanging.Clone();

            uint Value = 0;

			CSLibrary.Structures.InternalTagRangingParms parms = new CSLibrary.Structures.InternalTagRangingParms();
			parms.flags = m_rdr_opt_parms.TagRanging.flags;
			parms.tagStopCount = m_rdr_opt_parms.TagRanging.tagStopCount;

			// Set MultiBanks Info
			MacReadRegister(MACREGISTER.HST_INV_CFG, ref Value);

			Value &= 0xfff5fcff;
			if (m_rdr_opt_parms.TagRanging.multibanks != 0)
				Value |= (m_rdr_opt_parms.TagRanging.multibanks & (uint)0x03) << 16;

			if (m_rdr_opt_parms.TagRanging.QTMode == true)
				Value |= 0x00080000; // bit 19

            Value &= ~(0x03f00000U); // Set delay time to 0

            if (m_rdr_opt_parms.TagRanging.compactmode)
            {
                Value |= _INVENTORYDELAYTIME;
                Value |= (1 << 26); // bit 26
                MacWriteRegister(MACREGISTER.INV_CYCLE_DELAY, _InventoryCycleDelay);
            }
            else
            {
                Value |= (30 << 20);
                Value &= ~(1U << 26); // bit 26
                MacWriteRegister(MACREGISTER.INV_CYCLE_DELAY, 0);
            }

            MacWriteRegister(MACREGISTER.HST_INV_CFG, Value);

            Value = 0;
            if (m_rdr_opt_parms.TagRanging.focus)
                Value |= 0x10;
            if (m_rdr_opt_parms.TagRanging.fastid)
                Value |= 0x20;
            MacWriteRegister(MACREGISTER.HST_IMPINJ_EXTENSIONS, Value);

            // Set up the access bank register
            Value = (UInt32)(m_rdr_opt_parms.TagRanging.bank1) | (UInt32)(((int)m_rdr_opt_parms.TagRanging.bank2) << 2);
			MacWriteRegister(MACREGISTER.HST_TAGACC_BANK, Value);

			// Set up the access pointer register (tells the offset)
			Value = (UInt32)((m_rdr_opt_parms.TagRanging.offset1 & 0xffff) | ((m_rdr_opt_parms.TagRanging.offset2 & 0xffff) << 16));
			MacWriteRegister(MACREGISTER.HST_TAGACC_PTR, Value);

			// Set up the access count register (i.e., number values to read)
			Value = (UInt32)((0xFF & m_rdr_opt_parms.TagRanging.count1) | ((0xFF & m_rdr_opt_parms.TagRanging.count2) << 8));
			MacWriteRegister(MACREGISTER.HST_TAGACC_CNT, Value);

			// Set up the access password
			Value = (UInt32)(m_rdr_opt_parms.TagRanging.accessPassword);
			MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, Value);

			// Set Toggle off, if QT Mode. 
			if (m_rdr_opt_parms.TagRanging.QTMode == true)
			{
				uint RegValue = 0;

				for (uint cnt = 0; cnt < 4; cnt++)
				{
					MacWriteRegister(MACREGISTER.HST_INV_SEL, cnt);
					MacReadRegister(MACREGISTER.HST_INV_ALG_PARM_2, ref RegValue);
					Value &= 0xfffffffe;
					MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_2, Value);
				}
			}

			Start18K6CRequest(m_rdr_opt_parms.TagRanging.tagStopCount, parms.flags);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.INV), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
			//m_Result = COMM_HostCommand(HST_CMD.INV);
		}

        private void PreTagRangingThreadProc_CS108()
        {
            _tagRangingParms = m_rdr_opt_parms.TagRanging.Clone();

            uint Value = 0;

            CSLibrary.Structures.InternalTagRangingParms parms = new CSLibrary.Structures.InternalTagRangingParms();
            parms.flags = m_rdr_opt_parms.TagRanging.flags;
            parms.tagStopCount = m_rdr_opt_parms.TagRanging.tagStopCount;

            // Set MultiBanks Info
            MacReadRegister(MACREGISTER.HST_INV_CFG, ref Value);

            Value &= 0xfff0fcff;

            Value |= (1 << 18); // enable CRC checking

            if (m_rdr_opt_parms.TagRanging.multibanks != 0)
                Value |= (m_rdr_opt_parms.TagRanging.multibanks & (uint)0x03) << 16;

            if (m_rdr_opt_parms.TagRanging.QTMode == true)
                Value |= (1 << 19); // bit 19

            Value &= ~(0x03f00000U); // Set delay time to 0

            if (m_rdr_opt_parms.TagRanging.compactmode)
            {
                Value |= _INVENTORYDELAYTIME;
                Value |= (1 << 26); // bit 26
                MacWriteRegister(MACREGISTER.INV_CYCLE_DELAY, _InventoryCycleDelay);
            }
            else
            {
                Value |= (30 << 20);
                Value &= ~(1U << 26); // bit 26
                MacWriteRegister(MACREGISTER.INV_CYCLE_DELAY, 0);
            }

            MacWriteRegister(MACREGISTER.HST_INV_CFG, Value);

            Value = 0;
            if (m_rdr_opt_parms.TagRanging.focus)
                Value |= 0x10;
            if (m_rdr_opt_parms.TagRanging.fastid)
                Value |= 0x20;
            MacWriteRegister(MACREGISTER.HST_IMPINJ_EXTENSIONS, Value);

            // Set up the access bank register
            Value = (UInt32)(m_rdr_opt_parms.TagRanging.bank1) | (UInt32)(((int)m_rdr_opt_parms.TagRanging.bank2) << 2);
            MacWriteRegister(MACREGISTER.HST_TAGACC_BANK, Value);

            // Set up the access pointer register (tells the offset)
            Value = (UInt32)((m_rdr_opt_parms.TagRanging.offset1 & 0xffff) | ((m_rdr_opt_parms.TagRanging.offset2 & 0xffff) << 16));
            MacWriteRegister(MACREGISTER.HST_TAGACC_PTR, Value);

            // Set up the access count register (i.e., number values to read)
            Value = (UInt32)((0xFF & m_rdr_opt_parms.TagRanging.count1) | ((0xFF & m_rdr_opt_parms.TagRanging.count2) << 8));
            MacWriteRegister(MACREGISTER.HST_TAGACC_CNT, Value);

            // Set up the access password
            Value = (UInt32)(m_rdr_opt_parms.TagRanging.accessPassword);
            MacWriteRegister(MACREGISTER.HST_TAGACC_ACCPWD, Value);

            // Set Toggle off, if QT Mode. 
            if (m_rdr_opt_parms.TagRanging.QTMode == true)
            {
                uint RegValue = 0;

                for (uint cnt = 0; cnt < 4; cnt++)
                {
                    MacWriteRegister(MACREGISTER.HST_INV_SEL, cnt);
                    MacReadRegister(MACREGISTER.HST_INV_ALG_PARM_2, ref RegValue);
                    Value &= 0xfffffffe;
                    MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_2, Value);
                }
            }

            Start18K6CRequest(m_rdr_opt_parms.TagRanging.tagStopCount, parms.flags);
        }

        private void ExeTagRangingThreadProc_CS108()
        {
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.INV), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }

        private void TagSearchOneTagThreadProc_CS108()
        {
            // FireStateChangedEvent(RFState.BUSY);

            UInt32 Value = 0;

            // disable compact mode
            MacReadRegister(MACREGISTER.HST_INV_CFG, ref Value);

            Value &= ~(0x03f00000U); // Set delay time to 0
            Value |= (30 << 20);
            Value &= ~(1U << 26); // bit 26
            MacWriteRegister(MACREGISTER.INV_CYCLE_DELAY, 0);
            MacWriteRegister(MACREGISTER.HST_INV_CFG, Value);

            CSLibrary.Structures.InternalTagSearchOneParms parms = new CSLibrary.Structures.InternalTagSearchOneParms();
            parms.avgRssi = m_rdr_opt_parms.TagSearchOne.avgRssi;

            //                m_Result =  TagSearchOne(parms);
            Start18K6CRequest(0, CSLibrary.Constants.SelectFlags.SELECT);

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.INV), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);

            // FireStateChangedEvent(RFState.IDLE);
        }

        private void PreTagSearchOneTagThreadProc_CS108()
        {
            UInt32 Value = 0;

            // disable compact mode
            MacReadRegister(MACREGISTER.HST_INV_CFG, ref Value);
            Value &= ~(0x03f00000U); // Set delay time to 0
            Value |= (30 << 20);
            Value &= ~(1U << 26); // bit 26
            MacWriteRegister(MACREGISTER.INV_CYCLE_DELAY, 0);
            MacWriteRegister(MACREGISTER.HST_INV_CFG, Value);

            CSLibrary.Structures.InternalTagSearchOneParms parms = new CSLibrary.Structures.InternalTagSearchOneParms();
            parms.avgRssi = m_rdr_opt_parms.TagSearchOne.avgRssi;

            //                m_Result =  TagSearchOne(parms);
            Start18K6CRequest(0, CSLibrary.Constants.SelectFlags.SELECT);
        }

        private void ExeTagSearchOneTagThreadProc_CS108()
        {
            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.INV), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }
    }
}
