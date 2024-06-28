using Android.OS;
using Android.Content;
using Android.Provider;
using Xamarin.Essentials;
using Xamarin.Forms;
using Android.Net;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(BLE.Client.Droid.BatteryOptimizationService))]

namespace BLE.Client.Droid
{
    public class BatteryOptimizationService : BLE.Client.IBatteryOptimizationService
    {
        public bool IsBatteryOptimizationDisabled()
        {
            // If Android 12 or older
            if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                return true;

            var powerManager = (PowerManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.PowerService);
            var packageName = Android.App.Application.Context.PackageName;
            if (powerManager.IsIgnoringBatteryOptimizations(packageName))
                return true;

            return false;
        }

        public async Task OpenBatteryOptimizationsAsync()
        {
            if (!IsBatteryOptimizationDisabled())
            {
                var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                var uri = Uri.FromParts("package", Forms.Context.PackageName, null);
                intent.SetData(uri); 
                intent.AddFlags(ActivityFlags.NewTask);
                Forms.Context.StartActivity(intent);
            }

            return;
        }
    }
}


