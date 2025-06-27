using Acr.UserDialogs;
using Android.App;
using Android.Bluetooth;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;

namespace BLE.Client.Droid
{
    [Activity(ScreenOrientation = ScreenOrientation.User
        ,ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity 
        : MvxFormsAppCompatActivity

    //		: MvxFormsAppCompatActivity
    {
        protected override async void OnCreate(Bundle bundle)
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
            //Xamarin.Essentials.Permissions.RequestAsync<BLEPermission>();
            //CheckConnectedDevice();

            var status = await Xamarin.Essentials.Permissions.RequestAsync<BLEPermission>();
            //if (status == Xamarin.Essentials.PermissionStatus.Granted)
            {
                CheckConnectedDevice();
            }
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
                (Android.Manifest.Permission.BluetoothConnect, true),
                (Android.Manifest.Permission.Internet, true),
                (Android.Manifest.Permission.AccessNetworkState, true),
                (Android.Manifest.Permission.ReadExternalStorage, true),
                (Android.Manifest.Permission.WriteExternalStorage, true)
            }.ToArray();
        }

        void CheckConnectedDevice()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null || !adapter.IsEnabled)
                return;

            foreach (var device in adapter.BondedDevices)
                if (device.Type == BluetoothDeviceType.Le || device.Type == BluetoothDeviceType.Dual)
                    device.ConnectGatt(this, false, new ServiceCheckGattCallback(device));
        }

        class ServiceCheckGattCallback : BluetoothGattCallback
        {
            BluetoothDevice _device;
            public ServiceCheckGattCallback(BluetoothDevice device)
            {
                _device = device;
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
            {
                base.OnConnectionStateChange(gatt, status, newState);
                if (newState == ProfileState.Connected)
                {
                    gatt.DiscoverServices();
                }
                else if (newState == ProfileState.Disconnected)
                {
                    gatt.Close();
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
            {
                base.OnServicesDiscovered(gatt, status);
                var targetService = gatt.Services.FirstOrDefault(
                    s => s.Uuid.ToString().ToLower() == "00009802-0000-1000-8000-00805f9b34fb"
                );
                if (targetService != null)
                {
                    try
                    {
                        var m = _device.Class.GetMethod("removeBond");
                        m.Invoke(_device, null);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("removeBond failed: " + ex.Message);
                    }
                }
                gatt.Close();
            }
        }

    }
}