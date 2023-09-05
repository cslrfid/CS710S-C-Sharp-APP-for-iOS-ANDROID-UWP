using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(BLE.Client.UWP.Version_UWP))]
namespace BLE.Client.UWP
{
    public class Version_UWP : IAppVersion
    {
        static Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

        public string GetVersion()
        {
            return version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();
        }
        public int GetBuild()
        {
            return version.Revision;
        }
    }
}