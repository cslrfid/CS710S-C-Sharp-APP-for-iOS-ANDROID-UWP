using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageInventory : MvxContentPage
    {
        static public string[] _displatFormtOptions = {"0. Hex",
                                 "1. UPC",
                                 "2. SGTIN-96",
        };

/* support
ADI-var
CPI-96, CPI-var
GTDI-96, GDTI-174
GIAI-96, GIAI-202
GID-96
GRAI-96, GRAI-170
GSRN-96
GSRNP-96
ITIP-110, ITIP-212
SGCN-96
SGLN-96, SGLN-195
SGTIN-96, SGTIN-198
SSCC-96
USDOD-96
*/

        public PageInventory()
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

				BleMvxApplication._SELECT_EPC = Items.EPC;
                BleMvxApplication._SELECT_PC = Items.PC;
            }
        }
    }
}
