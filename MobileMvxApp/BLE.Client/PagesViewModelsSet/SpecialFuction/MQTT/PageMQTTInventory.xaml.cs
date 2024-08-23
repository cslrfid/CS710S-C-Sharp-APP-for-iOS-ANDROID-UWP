using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageMQTTInventory : MvxContentPage<ViewModelMQTTInventory>
    {
        static public string[] _displatFormtOptions = {"0. Hex",
                                 "1. UPC",
                                 "2. SGTIN-96",
        };

        public PageMQTTInventory()
		{
			InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-RFID Tag-104-30x30.png";
            }
        }

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
            var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");

            if (answer)
            {
				//BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo Items = (BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo)e.SelectedItem;
				BLE.Client.ViewModels.TagInfoViewModel Items = (BLE.Client.ViewModels.TagInfoViewModel)e.SelectedItem;

				BleMvxApplication._SELECT_EPC = Items.EPC_ORG;
                BleMvxApplication._SELECT_PC = Items.PC;
            }
        }
    }
}
