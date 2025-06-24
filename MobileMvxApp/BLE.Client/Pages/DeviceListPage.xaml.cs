using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using System;

namespace BLE.Client.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false, Animated = true)]
    public partial class DeviceListPage : MvxTabbedPage<DeviceListViewModel>
    {
        public DeviceListPage()
        {
            InitializeComponent();
        }

        public async void buttonWarningMessageClicked(object sender, EventArgs e)
        {
            await DisplayAlert(null, "Cannot find your reader?" + Environment.NewLine+ Environment.NewLine +
                "1) Please check if it is still in OS Bluetooth " + Environment.NewLine +
                "device list. If yes, please \"forget\" it" + Environment.NewLine +
                "2) Please make sure reader is in normal mode (Bluetooth LED slow flashing)", 
                "OK");
        }
    }
}
