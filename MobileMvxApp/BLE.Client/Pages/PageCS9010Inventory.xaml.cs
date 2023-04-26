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
    public partial class PageCS9010Inventory : MvxContentPage<ViewModelCS9010Inventory>
    {
		public PageCS9010Inventory()
		{
			InitializeComponent();
		}
        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");

            if (answer)
            {
                BLE.Client.ViewModels.ViewModelCS9010Inventory.ColdChainTagInfoViewModel Items = (BLE.Client.ViewModels.ViewModelCS9010Inventory.ColdChainTagInfoViewModel)e.SelectedItem;

                BleMvxApplication._SELECT_EPC = Items.EPC;
                BleMvxApplication._SELECT_PC = 0x3000;
            }
        }
    }
}
