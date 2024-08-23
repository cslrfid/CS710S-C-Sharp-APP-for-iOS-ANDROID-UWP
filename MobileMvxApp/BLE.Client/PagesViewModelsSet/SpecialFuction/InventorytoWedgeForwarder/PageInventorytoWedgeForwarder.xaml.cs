using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLE.Client.Pages;
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageInventorytoWedgeForwarder : MvxContentPage<ViewModelInventorytoWedgeForwarder>
    {
		public PageInventorytoWedgeForwarder()
		{
			InitializeComponent();
		}

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
            var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");

            if (answer)
            {
				//BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo Items = (BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo)e.SelectedItem;
				TagInfoViewModel Items = (TagInfoViewModel)e.SelectedItem;

				BleMvxApplication._SELECT_EPC = Items.EPC;
                BleMvxApplication._SELECT_PC = Items.PC;

                if ((BleMvxApplication._config.RFID_MBI_MultiBank1Enable && BleMvxApplication._config.RFID_MBI_MultiBank1 == CSLibrary.Constants.MemoryBank.TID) ||
                    (!BleMvxApplication._config.RFID_MBI_MultiBank1Enable && BleMvxApplication._config.RFID_MBI_MultiBank2Enable && BleMvxApplication._config.RFID_MBI_MultiBank2 == CSLibrary.Constants.MemoryBank.TID))
                    BleMvxApplication._SELECT_TID = Items.Bank1Data;
                else if (BleMvxApplication._config.RFID_MBI_MultiBank2Enable && BleMvxApplication._config.RFID_MBI_MultiBank2 == CSLibrary.Constants.MemoryBank.TID)
                    BleMvxApplication._SELECT_TID = Items.Bank2Data;
            }
        }
    }
}
