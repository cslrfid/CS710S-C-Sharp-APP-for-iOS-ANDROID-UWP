using System;
using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using MvvmCross.Forms.Platforms.Android.Views;
using MvvmCross;
using Xamarin.Forms;
using System.Collections.Generic;

namespace BLE.Client.Droid
{
    [Activity(ScreenOrientation = ScreenOrientation.User
        ,ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity 
		: MvxFormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate
            {
                return true;
            };

            base.OnCreate(bundle);

            Xamarin.Essentials.Platform.Init(this, bundle); // add this line to your code, it may also be called: bundle

            if (Device.Idiom == TargetIdiom.Phone)
                this.RequestedOrientation = ScreenOrientation.Portrait;
            else
                this.RequestedOrientation = ScreenOrientation.Landscape;

            //await Permissions.RequestAsync<BLEPermission>();
            Xamarin.Essentials.Permissions.RequestAsync<BLEPermission>();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public class BLEPermission : Xamarin.Essentials.Permissions.BasePlatformPermission
        {
            public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
            {
                (Android.Manifest.Permission.BluetoothScan, true),
                (Android.Manifest.Permission.BluetoothConnect, true)
            }.ToArray();
        }
    }
}