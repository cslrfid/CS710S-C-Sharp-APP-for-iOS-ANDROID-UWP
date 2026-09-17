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
	public partial class RFIDReader
	{
		/// <summary>
		/// FM13DT Tag Access completed event
		/// </summary>
		public event EventHandler<CSLibrary.Events.OnFM13DTAccessCompletedEventArgs> OnFM13DTAccessCompleted;

		//MacWriteRegister(MACREGISTER.HST_ANT_DESC_SEL, 0);
		//MacReadRegister(MACREGISTER.HST_ANT_DESC_RFPOWER, ref pwrlvl);
		//FM13DT160_CMDCFGPAR = 0x117,
		//FM13DT160_REGADDRPAR = 0x118,
		//FM13DT160_WRITEPAR = 0x119,
		//FM13DT160_PWDPAR = 0x11a,
		//FM13DT160_STOBLOCKADDPAR = 0x11b,
		//FM13DT160_STARTADDRPAR = 0x11c,
		//FM13DT160_READWRITELENPAR = 0x11d,
		//FM13DT160_DATAPAR = 0x11e,

		void FM13DT160_ReadMemory(uint offset, uint size)
		{
			MacWriteRegister(MACREGISTER.FM13DT160_STARTADDRPAR, (uint)offset);
			MacWriteRegister(MACREGISTER.FM13DT160_READWRITELENPAR, (uint)size);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTREADMEMORY), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_READMEMORY);
		}

		void FM13DT160_WriteMemory(uint offset, byte [] data)
		{
			UInt32 value = (UInt32)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);

			MacWriteRegister(MACREGISTER.FM13DT160_STARTADDRPAR, (uint)offset);
			MacWriteRegister(MACREGISTER.FM13DT160_DATAPAR, value);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTWRITEMEMORY), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_WRITEMEMORY);
		}

		/// <summary>
		/// mode = user, unlock, stop logging
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		void FM13DT160_Auth(uint mode, uint password)
		{
			MacWriteRegister(MACREGISTER.FM13DT160_CMDCFGPAR, (uint)mode);
			MacWriteRegister(MACREGISTER.FM13DT160_PWDPAR, password);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTAUTH), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_AUTH);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		void FM13DT160_GetTemp(int mode, int flag, uint storeto)
		{
			uint value = (uint)(0x80 | (flag = 0x04) | (flag = 0x02) | (flag = 0x01));

			MacWriteRegister(MACREGISTER.FM13DT160_CMDCFGPAR, value);
			MacWriteRegister(MACREGISTER.FM13DT160_READWRITELENPAR, storeto);
			
			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTGETTEMP), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_GETTEMP);
		}

		void FM13DT160_StartLog()
		{
			MacWriteRegister(MACREGISTER.FM13DT160_CMDCFGPAR, 0x00);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTSTARTLOG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_STARTLOG);

		}

		void FM13DT160_StopLog(uint password)
		{
			MacWriteRegister(MACREGISTER.FM13DT160_CMDCFGPAR, 0x50);
			MacWriteRegister(MACREGISTER.FM13DT160_PWDPAR, password);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTSTOPLOG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_STOPLOG);
		}

		void FM13DT160_WriteReg(int offset, int value)
		{
			MacWriteRegister(MACREGISTER.FM13DT160_REGADDRPAR, (uint)offset);
			MacWriteRegister(MACREGISTER.FM13DT160_WRITEPAR, (uint)value);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTWRITEREG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_WRITEREGISTER);
		}

		void FM13DT160_ReadReg(int offset)
		{
			MacWriteRegister(MACREGISTER.FM13DT160_REGADDRPAR, (uint)offset);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTREADREG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_READREGISTER);
		}

		void FM13DT160_DeepSleep(bool enable)
		{
			MacWriteRegister(MACREGISTER.FM13DT160_CMDCFGPAR, enable ? 1U : 0U);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTDEEPSLEEP), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_DEEPSLEEP);
		}

		void FM13DT160_OpModeChk(bool enable)
		{
			uint value = 0;

			if (enable)
				value = 0x01;

			MacWriteRegister(MACREGISTER.FM13DT160_CMDCFGPAR, value);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTOPMODECHK), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1, (uint)CSLibrary.Constants.Operation.FM13DT_OPMODECHK);
		}

		void FM13DT160_InitialRegFile()
		{
			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTINITIALREGFILE), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.FM13DT_INITIALREGFILE);
		}

		void FM13DT160_LedCtrl(bool enable)
		{
			uint value = 0;

			if (enable)
				value = 0x02;

			MacWriteRegister(MACREGISTER.FM13DT160_CMDCFGPAR, value);

			_deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.CUSTOMMFM13DTLEDCTRL), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1, (uint)CSLibrary.Constants.Operation.FM13DT_LEDCTRL);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TagAccessPacket"></param>
		/// <returns></returns>
		bool FM13DT160_TagAccessProc (CSLibrary.Constants.Operation operation, byte[] TagAccessPacket)
		{
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

						m_rdr_opt_parms.FM13DTOpModeChk.user_access_en = (returnvalue >> 5 & 0x01) != 0;
						m_rdr_opt_parms.FM13DTOpModeChk.RTC_logging = (returnvalue >> 4 & 0x01) != 0;
						m_rdr_opt_parms.FM13DTOpModeChk.vdet_process_flag = (returnvalue >> 3 & 0x01) != 0;
						m_rdr_opt_parms.FM13DTOpModeChk.light_chk_flag = (returnvalue >> 1 & 0x01) != 0;
						m_rdr_opt_parms.FM13DTOpModeChk.vbat_pwr_flag = (returnvalue & 0x01) != 0;
					}
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_READMEMORY:
					{
						if (TagAccessPacket.Length != (m_rdr_opt_parms.FM13DTReadMemory.count + 20))
							break;

						m_rdr_opt_parms.FM13DTReadMemory.data = new byte[m_rdr_opt_parms.FM13DTReadMemory.count];
						Buffer.BlockCopy(TagAccessPacket, 20, m_rdr_opt_parms.FM13DTReadMemory.data, 0, (int)(m_rdr_opt_parms.FM13DTReadMemory.count));
					}
					return true;
					break;

				case CSLibrary.Constants.Operation.FM13DT_READREGISTER:
					break;

				case CSLibrary.Constants.Operation.FM13DT_STARTLOG:
					break;

				case CSLibrary.Constants.Operation.FM13DT_STOPLOG:
					break;

				case CSLibrary.Constants.Operation.FM13DT_WRITEMEMORY:
					break;

				case CSLibrary.Constants.Operation.FM13DT_WRITEREGISTER:
					break;
			}

			return false;
		}

		int FM13DT160_CommandEnd(CSLibrary.Constants.Operation operation, bool success)
		{
			//if (OnFM13DTAccessCompleted == null)
			//	return 0;

			switch (operation)
			{
				case CSLibrary.Constants.Operation.FM13DT_AUTH:
					if (OnFM13DTAccessCompleted != null)
					OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.AUTH, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_DEEPSLEEP:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.DEEPSLEEP, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_GETTEMP:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.GETTEMP, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_INITIALREGFILE:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.INITIALREGFILE, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_LEDCTRL:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.LEDCTRL, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_OPMODECHK:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.OPMODECHK, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_READMEMORY:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.READMEMORY, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_READREGISTER:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.READREGISTER, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_STARTLOG:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.STARTLOG, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_STOPLOG:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.STOPLOG, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_WRITEMEMORY:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.WRITEMEMORY, success));
					break;

				case CSLibrary.Constants.Operation.FM13DT_WRITEREGISTER:
					if (OnFM13DTAccessCompleted != null)
						OnFM13DTAccessCompleted(this, new OnFM13DTAccessCompletedEventArgs(FM13DTAccess.WRITEREGISTER, success));
					break;

				default:
					return 0;
			}

			return 1;




			/*
			switch (operation)
			{
				case CSLibrary.Constants.Operation.FM13DT_OPMODECHK:
					break;
			}


						switch (operation)
						{
							case CSLibrary.Constants.Operation.FM13DT_READMEMORY:
								{
									FireFM13DTAccessCompletedEvent(
										new OnFM13DTAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										FM13DTAccess.READMEMORY));
								}
								break;

							case CSLibrary.Constants.Operation.FM13DT_WRITEMEMORY:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;

							case CSLibrary.Constants.Operation.FM13DT_READREGISTER:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;

							case CSLibrary.Constants.Operation.FM13DT_WRITEREGISTER:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;

							case CSLibrary.Constants.Operation.FM13DT_AUTH:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;
							case CSLibrary.Constants.Operation.FM13DT_GETTEMP:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;
							case CSLibrary.Constants.Operation.FM13DT_STARTLOG:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;
							case CSLibrary.Constants.Operation.FM13DT_STOPLOG:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;
							case CSLibrary.Constants.Operation.FM13DT_DEEPSLEEP:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;
							case CSLibrary.Constants.Operation.FM13DT_OPMODECHK:
								{
									FireAccessCompletedEvent(
										new OnFM13DTAccessCompletedEventArgs(
											FM13DTAccess.OPMODECHK, 
											(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) == 0
									)));
								}
								break;
							case CSLibrary.Constants.Operation.FM13DT_INITIALREGFILE:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;
							case CSLibrary.Constants.Operation.FM13DT_LEDCTRL:
								{
									CSLibrary.Debug.WriteLine("Tag lock end {0}", currentCommandResponse);

									FireAccessCompletedEvent(
										new OnAccessCompletedEventArgs(
										(((currentCommandResponse | result) & HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1) != 0),
										Bank.UNKNOWN,
										TagAccess.LOCK,
										null));
								}
								break;
						}
			*/
			return -1;
		}



#if nouse
		bool FM13DT160_GetTemp()
		{
/*
			set 117 = 0
			call 5c
			set 119 = 0
			set 1108 = c012
			call 59
			set 117 = 6
			call 56
			set 117 = 86
			call 56
*/
			FM13DT160_OpModeChk(false);
			FM13DT160_WriteReg(0xc012, 0);

			FM13DT160_GetTemp(int mode, int flag, int storeto); // 0x06
			FM13DT160_GetTemp(int mode, int flag, int storeto); // 0x86
		}

		bool FM13DT160_GetBat()
		{
			/*
			117 = 0
			call 5c
			119 = 08
			118 = c012
			call 59
			117 = 12
			call 56
			117 = 92
			call 56 */

			FM13DT160_OpModeChk(false);
			FM13DT160_WriteReg(0xc012, 0x08);

			FM13DT160_GetTemp(int mode, int flag, int storeto); // 0x12
			FM13DT160_GetTemp(int mode, int flag, int storeto); // 0x92
		}

		bool FM13DT160_StartLog()
		{
			/*
			117 = 0
			call 5c
			11c = b040
			11d = 0004
			11e = 4cb329d6
			call 54
			11c = b094
			11d = 0004
			11e = 03000000 --
			call 54
			11c = b0a4
			11d = 0004
			11e = 0a000100
			call 54
			118 = c084
			119 = 01     --
			call 59
			line 3638344
			118 = c085
			119 = 0014   --
			call 59
			118 = c099
			119 = 0
			call 59
			118 = c098
			119 = 00000100
			call 59
			117 = 0
			call 57
			118 = c084
			call 5a */

			FM13DT160_OpModeChk(false);
			FM13DT160_WriteMemory(0xb040, 4, 0x4cb329d6);
			FM13DT160_WriteMemory(0xb094, 4, 0x03);
			FM13DT160_WriteMemory(0xb0a4, 4, 0x0a000100);
			FM13DT160_WriteReg(0xc084, 0x1);
			FM13DT160_WriteReg(0xc085, 0x14);
			FM13DT160_WriteReg(0xc099, 0);
			FM13DT160_WriteReg(0xc098, 0x100);
			FM13DT160_StartLog();
			FM13DT160_ReadReg(0xc084);
		}
#endif






	}
}
