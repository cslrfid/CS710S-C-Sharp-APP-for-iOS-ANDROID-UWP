using System;
using MvvmCross.Logging;

//Default
namespace BLE.Client.Droid
{
    public class DebugTrace : IMvxLog
    {
        public bool IsLogLevelEnabled(MvxLogLevel logLevel)
        {
            //To be implemented
            return true;
        }

        public bool Log(MvxLogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
        {
            //To be implemented
            return true;
        }
    }
}
