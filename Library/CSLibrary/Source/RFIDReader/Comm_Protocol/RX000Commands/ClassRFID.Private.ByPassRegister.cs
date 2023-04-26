using System;
using System.Collections.Generic;
using System.Text;

using CSLibrary.Constants;

namespace CSLibrary
{
    public partial class RFIDReader
    {
        /// <summary>
        /// Writes directly to a radio-module hardware register.  The radio 
        /// module's hardware registers may not be written while a radio 
        /// module is executing a tag-protocol operation. 
        /// </summary>
        /// <param name="address">The 16-bit address of the radio-module hardware 
        /// register to be written.  An address that is beyond the 
        /// end of the radio module's register set Results in an 
        /// invalid-parameter return status. </param>
        /// <param name="value">The 16-bit value to write to the radio-module 
        /// hardware register specified by address. </param>
        /// <returns></returns>
        private Result MacBypassWriteRegister(ushort address, ushort value)
        {
            try
            {
                MacWriteRegister(MACREGISTER.HST_MBP_ADDR, address);

                MacWriteRegister(MACREGISTER.HST_MBP_DATA, value);

                //COMM_HostCommand(HST_CMD.MBPWRREG);
                // Issue read OEM command
                _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, PacketData(0xf000, (UInt32)HST_CMD.MBPWRREG), HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_COMMANDENDRESPONSE, (UInt32)0xffffffff);
            }
            catch (Exception ex)
            {
            }
            catch
            {
                m_Result = Result.SYSTEM_CATCH_EXCEPTION;
            }

            return m_Result;
        }
    }
}
