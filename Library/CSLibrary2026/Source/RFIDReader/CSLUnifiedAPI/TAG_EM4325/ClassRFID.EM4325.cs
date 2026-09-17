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
	public class ClassEM4325
	{
		public enum Operation
		{
			GETSENSORDATA
		}

		public class GETSENSORDATAPARAMETERS
		{
			public bool sendUID = false;
			public bool newSample = false;
			public double temperatureC = 0;
		}

		public class EM4325Paras
		{
			public GETSENSORDATAPARAMETERS GetSensorData = new GETSENSORDATAPARAMETERS();
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
		public EM4325Paras Options = new EM4325Paras();

		private HighLevelInterface _deviceHandler;

		internal ClassEM4325(HighLevelInterface handler)
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
				case Operation.GETSENSORDATA:
					GetSenseDataProc();
					return 0;
			}

			return -1;
		}

		private void GetSensorData(bool sendUID, bool newSample)
		{
			UInt32 value = 0;

			if (sendUID)
				value |= 0x01;

			if (newSample)
				value |= 0x02;

			_deviceHandler.rfid.MacWriteRegister(RFIDReader.MACREGISTER.EM4325_CFG, value);
			_deviceHandler.SendAsync(0, 0, RFIDReader.DOWNLINKCMD.RFIDCMD, _deviceHandler.rfid.PacketData(0xf000, (UInt32)RFIDReader.HST_CMD.CUSTOMEMGETSENSORDATA), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (uint)CSLibrary.Constants.Operation.EM_GetSensorData);
		}

		private bool GetSenseDataProc()
		{
			GetSensorData(Options.GetSensorData.sendUID, Options.GetSensorData.newSample);
			return true;
		}


		// call by Library core
		/// <summary>
		/// System Operation
		///	Subsystem Operation
		///	Sequence
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="TagAccessPacket"></param>
		/// <returns></returns>
		//internal int TagAccessProc(CSLibrary.Constants.Operation mainOperation, Operation subOperation, int sqr, byte[] TagAccessPacket)
		internal bool TagAccessProc(CSLibrary.Constants.Operation mainOperation, byte[] TagAccessPacket)
		{
			switch (mainOperation)
			{
				case CSLibrary.Constants.Operation.EM_GetSensorData:
					{
						var SensorDataMsw = (TagAccessPacket[32] << 0x08 | TagAccessPacket[33]);
						var Temp = (SensorDataMsw & 0x00ff);
						if ((SensorDataMsw & 0x0100) != 00)
							Temp -= 256;

						Options.GetSensorData.temperatureC = Temp * 0.25;
					}
					return true;
			}

			return false;
		}

		// call by Library core
		internal bool CommandEndProc(CSLibrary.Constants.Operation mainOperation, bool success)
		{
			switch (mainOperation)
			{
				case CSLibrary.Constants.Operation.EM_GetSensorData:
					if (OnAccessCompleted != null)
						OnAccessCompleted(this, new OnAccessCompletedEventArgs(Operation.GETSENSORDATA, success));
					return true;
			}

			return false;
		}
	}
}
