using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLE.Client.Pages;
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using Xamarin.Forms;
using static BLE.Client.ViewModels.ViewModelAsygnInventory;

namespace BLE.Client.Pages
{
    public partial class PageAsygnInventory : MvxContentPage<ViewModelAsygnInventory>
    {
		public PageAsygnInventory()
		{
			InitializeComponent();
		}

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
            var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");

            if (answer)
            {
                TagInfoWithTempViewModel Items = (TagInfoWithTempViewModel)e.SelectedItem;

				BleMvxApplication._SELECT_EPC = Items.EPC;
                BleMvxApplication._SELECT_PC = Items.PC;
            }
        }
    }
}
