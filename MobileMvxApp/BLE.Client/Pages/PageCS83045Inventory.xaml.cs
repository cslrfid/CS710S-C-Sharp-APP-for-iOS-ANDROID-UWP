using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageCS83045Inventory : MvxContentPage<ViewModelCS83045Inventory>
    {
		public PageCS83045Inventory()
		{
			InitializeComponent();
		}

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");

            if (answer)
            {
                //BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo Items = (BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo)e.SelectedItem;
                BLE.Client.ViewModels.ViewModelCS83045Inventory.ColdChainTagInfoViewModel Items = (BLE.Client.ViewModels.ViewModelCS83045Inventory.ColdChainTagInfoViewModel)e.SelectedItem;

                BleMvxApplication._SELECT_EPC = Items.EPC;
                //BleMvxApplication._SELECT_PC = Items.PC;
            }
        }
    }
}
