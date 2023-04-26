using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageAxzonInventory : MvxContentPage<ViewModelAxzonInventory>
    {
        bool pageView = true;

		public PageAxzonInventory()
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
				BLE.Client.ViewModels.ViewModelAxzonInventory.RFMicroTagInfoViewModel Items = (BLE.Client.ViewModels.ViewModelAxzonInventory.RFMicroTagInfoViewModel)e.SelectedItem;

				BleMvxApplication._SELECT_EPC = Items.DisplayName;
                BleMvxApplication._SELECT_PC = 0x3000;
            }
        }

		~PageAxzonInventory()
        {
            pageView = false;
        }
	}
}
