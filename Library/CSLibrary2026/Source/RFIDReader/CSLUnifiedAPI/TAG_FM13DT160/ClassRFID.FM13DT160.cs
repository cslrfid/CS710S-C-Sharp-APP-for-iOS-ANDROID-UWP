using System;
using System.Collections.Generic;
using System.Text;

using CSLibrary;
using CSLibrary.Constants;
using CSLibrary.Structures;
using CSLibrary.Events;
using CSLibrary.Tools;

namespace CSLibrary
{
	public class ClassFM13DT160
	{
		public enum Operation
		{
			READMEMORY,		// Read memory
			WRITEMEMORY,	// Write memory
			READREGISTER,	// Read Register
			WRITEREGISTER,	// Write Register
			AUTH,			// Auth
			GETTEMP,		// Get Temp
			STARTLOG,		// Start Log
			STOPLOG,		// Stop Log
			DEEPSLEEP,		// Reset to sleep mode
			OPMODECHK,		// Operation
			INITIALREGFILE,	// Init Tag
			LEDCTRL			// Tuen on LED 1s
		}

		/// <summary>
		/// offset = Starting address
		/// count = Read size (byte)
		/// data = Result
		/// </summary>
		public class ReadMemoryParms
		{
			public UInt16 offset;
			public uint count;		// Byte size (divide by 4)
			public byte[] data;
		}

		/// <summary>
		/// offset = Starting address
		/// count = Write size (byte)
		/// data = Data
		/// </summary>
		public class WriteMemoryParms
		{
			public UInt16 offset;
			public uint count;		// Byte size (max 4)
			public byte[] data;
		}

		/// <summary>
		/// offset = Starting address
		/// count = Write size (byte)
		/// data = Data
		/// </summary>
		public class ReadRegisterParms
		{
			public UInt16 address;
			public UInt16 value;
		}

		/// <summary>
		/// offset = Starting address
		/// count = Read size (byte)
		/// data = Result
		/// </summary>
		public class WriteRegisterParms
		{
			public UInt16 address;
			public UInt16 value;
		}

		/// <summary>
		/// </summary>
		public class AuthParms
		{
		}

		/// <summary>
		/// </summary>
		public class GetTempParms
		{
			public enum CMDCFG : UInt32
			{
				TEMP = 0x86,
				BATTERY = 0x92
			}

			public UInt32 cmd_cfg = (UInt32)CMDCFG.TEMP;
			public UInt32 ewblock_addr = 0x00;
		}

		/// <summary>
		/// </summary>
		public class StartLogParms
		{
		}

		/// <summary>
		/// </summary>
		public class StopLogParms
		{
			public UInt32 password;
		}

		public class DeepSleepParms
		{
			public bool enable;
		}

		public class OpModeChkParms
		{
			public bool enable;
			public bool user_access_en;
			public bool RTC_logging;
			public bool vdet_process_flag;
			public bool light_chk_flag;
			public bool vbat_pwr_flag;
		}

		public class LedCtrlParms
		{
			public bool enable;
		}

		public class FM13DT160Paras
		{
			public ReadMemoryParms ReadMemory = new ReadMemoryParms();
			public WriteMemoryParms WriteMemory = new WriteMemoryParms();
			public ReadRegisterParms ReadRegister = new ReadRegisterParms();
			public WriteRegisterParms WriteRegister = new WriteRegisterParms();
			public AuthParms Auth = new AuthParms();
			public GetTempParms GetTemp = new GetTempParms();
			public StartLogParms StartLog = new StartLogParms();
			public StopLogParms StopLog = new StopLogParms();
			public DeepSleepParms DeepSleep = new DeepSleepParms();
			public OpModeChkParms OpModeChk = new OpModeChkParms();
			public LedCtrlParms LedCtrl = new LedCtrlParms();
		}

		public class OnAccessCompletedEventArgs : EventArgs
		{
			public Operation operation;
			public bool success;

			public OnAccessCompletedEventArgs(Operation operation, bool success)
			{
				this.operation = operation;
				this.success = success;
			}
		}

		public event EventHandler<OnAccessCompletedEventArgs> OnAccessCompleted;
		public FM13DT160Paras Options = new FM13DT160Paras();

		private HighLevelInterface _deviceHandler;

		internal ClassFM13DT160(HighLevelInterface handler)
		{
			_deviceHandler = handler;
		}

		public void ClearEventHandler()
		{
			OnAccessCompleted = null;
		}

		// call by Application
		public int StartOperation(Operation operation)
		{
			switch (operation)
			{
				case Operation.READMEMORY:
					FM13DTReadMemoryThreadProc();
					break;

				case Operation.WRITEMEMORY:
					FM13DTWriteMemoryThreadProc();
					break;

				case Operation.READREGISTER:
					FM13DTReadRegThreadProc();
					break;

				case Operation.WRITEREGISTER:
					FM13DTWriteRegThreadProc();
					break;

				case Operation.AUTH:
					FM13DTAuthThreadProc();
					break;

				case Operation.GETTEMP:
					FM13DTGetTempThreadProc();
					break;

				case Operation.STARTLOG:
					FM13DTStartLogThreadProc();
					break;

				case Operation.STOPLOG:
					FM13DTStopLogChkThreadProc();
					break;

				case Operation.DEEPSLEEP:
					FM13DTDeepSleepThreadProc();
					break;

				case Operation.OPMODECHK:
					FM13DTOpModeChkThreadProc();
					break;

				case Operation.INITIALREGFILE:
					FM13DTInitialRegFileThreadProc();
					break;

				case Operation.LEDCTRL:
					FM13DTLedCtrlThreadProc();
					break;
			}

			return -1;
		}

		private bool FM13DTReadMemoryThreadProc()
		{
			if (Options.ReadMemory.offset > 0xffff)
				return false;

			if ((Options.ReadMemory.count & 0x3) != 0)
				return false;

			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_STARTADDRPAR, (uint)Options.ReadMemory.offset);
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_READWRITELENPAR, (uint)Options.ReadMemory.count);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTREADMEMORY), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_READMEMORY);

			return true;
		}

		private bool FM13DTWriteMemoryThreadProc()
		{
			if (Options.WriteMemory.offset > 0xffff)
				return false;

			if ((Options.WriteMemory.count & 0x1) != 0)
				return false;

			UInt32 value = (UInt32)((Options.WriteMemory.data[0] << 24) | (Options.WriteMemory.data[1] << 16) | (Options.WriteMemory.data[2] << 8) | Options.WriteMemory.data[3]);

			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_STARTADDRPAR, (uint)Options.WriteMemory.offset);
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_READWRITELENPAR, (uint)Options.WriteMemory.count);
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_DATAPAR, value);
			//_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTWRITEMEMORY), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_WRITEMEMORY);

			return true;
		}

		private bool FM13DTReadRegThreadProc()
		{
			if (Options.ReadRegister.address < 0xc000 || Options.ReadRegister.address > 0xc0ff)
				return false;

			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_REGADDRPAR, (uint)Options.ReadRegister.address);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTREADREG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_READREGISTER);
			return true;
		}

		private bool FM13DTWriteRegThreadProc()
		{
			if (Options.ReadRegister.address < 0xc000 || Options.ReadRegister.address > 0xcff)
				return false;

			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_REGADDRPAR, (uint)Options.WriteRegister.address);
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_WRITEPAR, (uint)Options.WriteRegister.value);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTWRITEREG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_WRITEREGISTER);
			return true;
		}

		private bool FM13DTAuthThreadProc()
		{
//			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_CMDCFGPAR, (uint)mode);
//			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_PWDPAR, password);

			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTAUTH), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_AUTH);
			return true;
		}

		private bool FM13DTGetTempThreadProc()
		{
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_CMDCFGPAR, Options.GetTemp.cmd_cfg);
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_STOBLOCKADDPAR, Options.GetTemp.ewblock_addr);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTGETTEMP), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_GETTEMP);
			return true;
		}
		private bool FM13DTStartLogThreadProc()
		{
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_CMDCFGPAR, 0x00);

//			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTSTARTLOG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_STARTLOG);
			return true;
		}

		private bool FM13DTStopLogChkThreadProc()
		{
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_CMDCFGPAR, 0x50);
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_PWDPAR, Options.StopLog.password);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTSTOPLOG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_STOPLOG);
			return true;
		}
		private bool FM13DTDeepSleepThreadProc()
		{
			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_CMDCFGPAR, 1U);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTDEEPSLEEP), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_DEEPSLEEP);
			return true;
		}
		private bool FM13DTOpModeChkThreadProc()
		{
			uint value = 0;

			if (Options.OpModeChk.enable)
				value = 0x01;

			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_CMDCFGPAR, value);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTOPMODECHK), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1, (uint)CSLibrary.Constants.Operation.FM13DT_OPMODECHK);

			return true;
		}

		private bool FM13DTInitialRegFileThreadProc()
		{
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTINITIALREGFILE), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_INITIALREGFILE);
			return true;
		}

		private bool FM13DTLedCtrlThreadProc()
		{
			uint value = 0;

			if (Options.LedCtrl.enable)
				value = 0x02;

			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.FM13DT160_CMDCFGPAR, value);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMMFM13DTLEDCTRL), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1, (uint)CSLibrary.Constants.Operation.FM13DT_LEDCTRL);

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TagAccessPacket"></param>
		/// <returns></returns>
		internal bool TagAccessProc(CSLibrary.Constants.Operation operation, byte[] TagAccessPacket)
		{
			var a = 10;

			switch (operation)
			{
				case CSLibrary.Constants.Operation.FM13DT_AUTH:

					break;

				case CSLibrary.Constants.Operation.FM13DT_DEEPSLEEP:
					break;

				case CSLibrary.Constants.Operation.FM13DT_GETTEMP:
					{
						int returnvalue = (TagAccessPacket[TagAccessPacket.Length - 4] << 8) | TagAccessPacket[TagAccessPacket.Length - 3];

						switch (returnvalue)
						{
							case 0x0f:
								break;

							case 0xfffa: // 场能量足够
								return true;

							case 0xfff5: // 场能量不足
								return true;

							case 0xfff0: // 未启动场能量检测
								return true;
						}
					}
					break;

				case CSLibrary.Constants.Operation.FM13DT_INITIALREGFILE:
					{
						var returnvalue = TagAccessPacket[TagAccessPacket.Length - 4];
						if (TagAccessPacket[TagAccessPacket.Length - 4] != 00 || TagAccessPacket[TagAccessPacket.Length - 3] != 00)
							break;
					}
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_LEDCTRL:
					{
						var returnvalue = TagAccessPacket[TagAccessPacket.Length - 4];
					}
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_OPMODECHK:
					{
						/*
												result[15:14]：= 00 RFU
												result[13]：= 1 user_access_en，高有效，表示当前用户权限是否有效
												result[12]：= 0 RTC logging，高有效，表示当前处于rtc测温流程
												result[11]：= 0 vdet_process_flag，高有效，表示一次测温流程被打断
												result[10]：= 0 RFU
												result[9]：= 0 light_chk_flag，高表示光强超过预设值
												result[8]：= 1 vbat_pwr_flag，高表示电池电压高于0.9V

												result[7:4]：= 0000 RFU
												result[3:0]: = 0001 RFU
						*/
						var returnvalue = TagAccessPacket[TagAccessPacket.Length - 4];

						Options.OpModeChk.user_access_en = (returnvalue >> 5 & 0x01) != 0;
						Options.OpModeChk.RTC_logging = (returnvalue >> 4 & 0x01) != 0;
						Options.OpModeChk.vdet_process_flag = (returnvalue >> 3 & 0x01) != 0;
						Options.OpModeChk.light_chk_flag = (returnvalue >> 1 & 0x01) != 0;
						Options.OpModeChk.vbat_pwr_flag = (returnvalue & 0x01) != 0;
					}
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_READMEMORY:
					{
						if (TagAccessPacket.Length != (Options.ReadMemory.count + 20))
							break;

						Options.ReadMemory.data = new byte[Options.ReadMemory.count];
						Buffer.BlockCopy(TagAccessPacket, 20, Options.ReadMemory.data, 0, (int)(Options.ReadMemory.count));
					}
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_READREGISTER:
					Options.ReadRegister.value = (UInt16)((TagAccessPacket[20] << 8) | TagAccessPacket[21]);
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_STARTLOG:
					break;

				case CSLibrary.Constants.Operation.FM13DT_STOPLOG:
					break;

				case CSLibrary.Constants.Operation.FM13DT_WRITEMEMORY:
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_WRITEREGISTER:
					return true;
					break;
			}

			return false;
		}

		internal bool CommandEndPProc(CSLibrary.Constants.Operation operation, bool success)
		{
			switch (operation)
			{
				case CSLibrary.Constants.Operation.FM13DT_AUTH:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.AUTH, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_DEEPSLEEP:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.DEEPSLEEP, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_GETTEMP:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.GETTEMP, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_INITIALREGFILE:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.INITIALREGFILE, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_LEDCTRL:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.LEDCTRL, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_OPMODECHK:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.OPMODECHK, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_READMEMORY:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.READMEMORY, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_READREGISTER:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.READREGISTER, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_STARTLOG:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.STARTLOG, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_STOPLOG:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.STOPLOG, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_WRITEMEMORY:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.WRITEMEMORY, success));
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_WRITEREGISTER:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.WRITEREGISTER, success));
					return true;
					break;

				default:
					break;
			}

			return false;
		}
	}
}
