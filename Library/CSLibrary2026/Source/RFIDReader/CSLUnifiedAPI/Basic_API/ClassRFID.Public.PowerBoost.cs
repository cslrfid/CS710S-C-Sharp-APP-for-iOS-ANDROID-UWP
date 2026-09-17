using System;
using System.Collections.Generic;
using System.Text;

using CSLibrary.Constants;

namespace CSLibrary
{
    using static RFIDDEVICE;

    public partial class RFIDReader
    {
        public Result SetPowerBoost(bool enable)
        {
            switch (_deviceType)
            {
                case MODEL.CS710S:
                case MODEL.CS203XL:
                    if (enable)
                        RFIDRegister.PowerBoost.Set(1);
                    else
                        RFIDRegister.PowerBoost.Set(0);
                    break;

                default:
                    // Unsupported device type for Power Boost
                    return Result.DEVICE_NOT_SUPPORT;
            }

            return Result.OK;
        }
    }
}
