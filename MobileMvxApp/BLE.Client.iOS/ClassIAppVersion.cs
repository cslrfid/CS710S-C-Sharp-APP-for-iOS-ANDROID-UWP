using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(BLE.Client.iOS.Version_iOS))]
namespace BLE.Client.iOS
{
        public class Version_iOS : IAppVersion
        {
            public string GetVersion()
            {
                return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
            }
            public int GetBuild()
            {
                return int.Parse(NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString());
            }
        }
}