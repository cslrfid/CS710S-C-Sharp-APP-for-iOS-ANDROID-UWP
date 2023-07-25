using System;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using static CSLibrary.RFIDDEVICE;

namespace BLE.Client.ViewModels
{
    public class DeviceListItemViewModel : MvxNotifyPropertyChanged
    {
        public IDevice Device { get; private set; }
        public MODEL BTServiceType { get; private set; }
        
        public Guid Id => Device.Id;
        public string IdString {
            get {
                if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
                    return Id.ToString();

                string idString = Id.ToString().ToUpper();
                string macString = idString.Substring(idString.Length - 12, 2) + ":";
                macString += idString.Substring(idString.Length - 10, 2) + ":";
                macString += idString.Substring(idString.Length - 8, 2) + ":";
                macString += idString.Substring(idString.Length - 6, 2) + ":";
                macString += idString.Substring(idString.Length - 4, 2) + ":";
                macString += idString.Substring(idString.Length - 2, 2);
                return macString;
            } 
        }
        public string Model => BTServiceType.ToString();
        public bool IsConnected => Device.State == DeviceState.Connected;
        public int Rssi => Device.Rssi;
        public string Name => Device.Name;

        public DeviceListItemViewModel(IDevice device, MODEL BTServiceType)
        {
            this.Device = device;
            this.BTServiceType = BTServiceType;
        }

        public void Update(IDevice newDevice = null)
        {
            if (newDevice != null)
            {
                Device = newDevice;
            }
            RaisePropertyChanged(nameof(IsConnected));
            RaisePropertyChanged(nameof(Rssi));
        }
    }
}
