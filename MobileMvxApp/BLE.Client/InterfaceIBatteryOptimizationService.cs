using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client
{
    public interface IBatteryOptimizationService
    {
        bool IsBatteryOptimizationDisabled();
        Task OpenBatteryOptimizationsAsync();
    }
}
