using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using BLE.Client.Extensions;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.Permissions.Abstractions;
using Plugin.Settings.Abstractions;

using static CSLibrary.RFIDDEVICE;

namespace BLE.Client.ViewModels
{
    public class DeviceListViewModel : BaseViewModel
    {
        private readonly IBluetoothLE _bluetoothLe;
        private readonly IUserDialogs _userDialogs;
        private readonly ISettings _settings;
        private readonly IMvxNavigationService _navigation;
        
        private Guid _previousGuid;
        private CancellationTokenSource _cancellationTokenSource;

        public IList<IService> Services { get; private set; }
        public IDescriptor Descriptor { get; private set; }

        private string _version;
        public string version { get; set; }

        public MvxCommand RefreshCommand => new MvxCommand(() => TryStartScanning(true));
        public MvxCommand<DeviceListItemViewModel> DisconnectCommand => new MvxCommand<DeviceListItemViewModel>(DisconnectDevice);

        public MvxCommand<DeviceListItemViewModel> ConnectDisposeCommand => new MvxCommand<DeviceListItemViewModel>(ConnectAndDisposeDevice);

        public ObservableCollection<DeviceListItemViewModel> Devices { get; set; } = new ObservableCollection<DeviceListItemViewModel>();
        public bool IsRefreshing => Adapter.IsScanning;
        public bool IsStateOn => _bluetoothLe.IsOn;
        public string StateText => GetStateText();
        public DeviceListItemViewModel SelectedDevice
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    HandleSelectedDevice(value);
                }

                RaisePropertyChanged();
            }
        }

        public MvxCommand StopScanCommand => new MvxCommand(() =>
        {
            try
            {
                Devices.Clear();

                _cancellationTokenSource.Cancel();
                CleanupCancellationToken();
                RaisePropertyChanged(() => IsRefreshing);
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("can not stop _cancellationTokenSource");
            }
        }, () => _cancellationTokenSource != null);

        public DeviceListViewModel(IBluetoothLE bluetoothLe, IAdapter adapter, IUserDialogs userDialogs, ISettings settings, IPermissions permissions, IMvxNavigationService navigation) : base(adapter)
        {
            _bluetoothLe = bluetoothLe;
            _userDialogs = userDialogs;
            _settings = settings;
            _navigation = navigation;

            // quick and dirty :>
            _bluetoothLe.StateChanged += OnStateChanged;
            Adapter.DeviceDiscovered += OnDeviceDiscovered;
            Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceConnectionLost;
            //Adapter.DeviceConnected += (sender, e) => Adapter.DisconnectDeviceAsync(e.Device);

            _ = BleMvxApplication._reader.DisconnectAsync();

        }

        private void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();

            _userDialogs.HideLoading();
            _userDialogs.ErrorToast("Error", $"Connection LOST {e.Device.Name} Please reconnect reader", TimeSpan.FromMilliseconds(5000));
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            RaisePropertyChanged(nameof(IsStateOn));
            RaisePropertyChanged(nameof(StateText));
        }

        private string GetStateText()
        {
            try
            {
                switch (_bluetoothLe.State)
                {
                    //case BluetoothState.Unknown:
                    //return "Unknown BLE state.";
                    case BluetoothState.Unavailable:
                        return "BLE is not available on this device.";
                    case BluetoothState.Unauthorized:
                        return "You are not allowed to use BLE.";
                    case BluetoothState.TurningOn:
                        return "BLE is warming up, please wait.";
                    case BluetoothState.On:
                        return "BLE is on.";
                    case BluetoothState.TurningOff:
                        return "BLE is turning off. That's sad!";
                    case BluetoothState.Off:
                        if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
                            _userDialogs.Alert("Please put finger at bottom of screen and swipe up “Control Center” and turn on Bluetooth.  If Bluetooth is already on, turn it off and on again");
                        return "BLE is off. Turn it on!";
                }
            }
            catch (Exception ex)
            {
            }

            return "Unknown BLE state.";
        }

        bool _scanAgain = true;

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsRefreshing);

            CleanupCancellationToken();

            if (_scanAgain)
                ScanForDevices();
        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs args)
        {
            try
            {
                bool CSLRFIDReaderService = false;
                MODEL BTServiceType = MODEL.UNKNOWN;

                // CS108 filter
                switch (Xamarin.Forms.Device.RuntimePlatform)
                {
                    case Xamarin.Forms.Device.UWP:
                        if (args.Device.AdvertisementRecords.Count == 0)
                            CSLRFIDReaderService = true;
                        break;

                    default:
                        if (args.Device.AdvertisementRecords.Count < 1)
                            return;

                        foreach (AdvertisementRecord service in args.Device.AdvertisementRecords)
                        {
                            if (service.Data.Length == 2)
                            {
                                // CS108 Service ID = 0x9800
                                if (service.Data[0] == 0x98 && service.Data[1] == 0x00)
                                {
                                    BTServiceType = MODEL.CS108;
                                    CSLRFIDReaderService = true;
                                    break;
                                }

                                // CS710S Service ID ios = 0x9802, android = 0x5350
                                if ((service.Data[0] == 0x98 && service.Data[1] == 0x02) || (service.Data[0] == 0x53 && service.Data[1] == 0x50))
                                {
                                    BTServiceType = MODEL.CS710S;
                                    CSLRFIDReaderService = true;
                                    break;
                                }
                            }
                        }
                        break;
                }

                if (!CSLRFIDReaderService)
                    return;

                AddOrUpdateDevice(args.Device, BTServiceType);
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("Can not handle desconvered device");
            }
        }

        private void AddOrUpdateDevice(IDevice device, MODEL BTServiceType)
        {
            InvokeOnMainThread(() =>
            {
                try
                {
                    var vm = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                    if (vm != null)
                    {
                        vm.Update(device);
                    }
                    else
                    {
                        Devices.Add(new DeviceListItemViewModel(device, BTServiceType));
                    }
                }
                catch (Exception ex)
                {
                    CSLibrary.Debug.WriteLine("Can not add device");
                }
            });
        }

        public override async void ViewAppearing()
        {
            try
            {
                base.ViewAppearing();
                TryStartScanning();
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("Device Resume Error");
            }
        }

        public List<DeviceListItemViewModel> SystemDevices { get; private set; }

        public override void ViewDisappearing()
        {
            try
            {
                base.ViewDisappearing();

                Adapter.StopScanningForDevicesAsync();
                RaisePropertyChanged(() => IsRefreshing);
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("Device Suspend error");
            }
        }

        private async void TryStartScanning(bool refresh = false)
        {
            if (IsStateOn && (refresh || !Devices.Any()) && !IsRefreshing)
            {
                Devices.Clear();

                ScanForDevices();
            }
        }

        private async void ScanForDevices()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                RaisePropertyChanged(() => StopScanCommand);

                RaisePropertyChanged(() => IsRefreshing);
                Adapter.ScanMode = ScanMode.LowLatency;
                await Adapter.StartScanningForDevicesAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("Can not Scan devices");
            }
        }

        private void CleanupCancellationToken()
        {
            try
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                RaisePropertyChanged(() => StopScanCommand);

                if (_scanAgain)
                    ScanForDevices();
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("Can not stop _cancellationTokenSource");
            }
        }

        private async void DisconnectDevice(DeviceListItemViewModel device)
        {
            if (BleMvxApplication._reader.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
                BleMvxApplication._reader.DisconnectAsync();

            try
            {
                if (!device.IsConnected)
                    return;

                _userDialogs.ShowLoading($"Disconnecting {device.Name}...");

                await Adapter.DisconnectDeviceAsync(device.Device);
            }
            catch (Exception ex)
            {
                _userDialogs.Alert(ex.Message, "Disconnect error");
            }
            finally
            {
                device.Update();
                _userDialogs.HideLoading();
            }
        }

        private async void HandleSelectedDevice(DeviceListItemViewModel devices)
        {
            try
            {
                if (await ConnectDeviceAsync(devices))
                {
                    // Connect to CS108

                    //var Services = Adapter.ConnectedDevices.FirstOrDefault(d => d.Id.ToString().Equals(device.Device.Id.ToString()));
                    var device = Adapter.ConnectedDevices.FirstOrDefault(d => d.Id.Equals(devices.Device.Id));

                    if (device == null)
                        return;

                    Connect(device, devices.BTServiceType);

                    _ = _navigation.Close(this);
                }
            }
            catch (Exception ex)
            {
                _userDialogs.Alert(ex.Message, "Disconnect error");
            }

        }

        private async Task<bool> ConnectDeviceAsync(DeviceListItemViewModel device, bool showPrompt = true)
        {
            if (showPrompt && !await _userDialogs.ConfirmAsync($"Connect to device '{device.Name}'?"))
            {
                return false;
            }

            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                await Adapter.ConnectToDeviceAsync(device.Device, new ConnectParameters(autoConnect: false, forceBleTransport: true), tokenSource.Token);

                //CSLibraryv4: increase wait time to 10s
                _userDialogs.ShowSuccess($"Initializing Reader, Please Wait.", 10000);

                return true;

            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message, "Connection error");
                Trace.Message(ex.Message);
                return false;
            }
            finally
            {
                //_userDialogs.HideLoading();
                device.Update();
            }
        }

        private async void Connect(IDevice _device, MODEL deviceType)
        {
            Trace.Message("device name :" + _device.Name);

            await BleMvxApplication._reader.ConnectAsync(Adapter, _device, deviceType);

            Trace.Message("load config");

            //bool LoadSuccess = await BleMvxApplication.LoadConfig(_device.Name);
            //BleMvxApplication._config.readerID = _device.Name;
            //bool LoadSuccess = await BleMvxApplication.LoadConfig(_device.Id.ToString(), BleMvxApplication._reader.rfid.GetAntennaPort());
            bool LoadSuccess = await BleMvxApplication.LoadConfig(_device.Id.ToString(), deviceType);
            BleMvxApplication._config.readerID = _device.Id.ToString();
        }

        private async void ConnectAndDisposeDevice(DeviceListItemViewModel item)
        {
            try
            {
                using (item.Device)
                {
                    await Adapter.ConnectToDeviceAsync(item.Device);
                    item.Update();
                }
            }
            catch (Exception ex)
            {
                _userDialogs.Alert(ex.Message, "Failed to connect and dispose.");
            }
            finally
            {
                _userDialogs.HideLoading();
            }
        }

        private void OnDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
            _userDialogs.HideLoading();
            _userDialogs.Toast($"Disconnected {e.Device.Name}");
        }
    }
}
