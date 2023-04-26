using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client
{
    public interface IAppVersion
    {
        string GetVersion();
        int GetBuild();
    }
}