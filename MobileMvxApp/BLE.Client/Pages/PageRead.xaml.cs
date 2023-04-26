using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLE.Client.Pages
{
	public partial class PageRead : MvxContentPage<ViewModelRead>
	{
        string[] _bankOptions = new string[] { "Bank0 (Reserved)", "Bank1 (EPC)", "Bank2 (TID)", "Bank3 (User)" };
        //string[] _bankOptions = new string []{ "Bank3 (User Bank)", "Bank1 (EPC Bank)" };

        public PageRead()
		{
			InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        public async void buttonBankClicked(object sender, EventArgs args)
        {
            var answer = await DisplayActionSheet("Bank", "Cancel", null, _bankOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonBank.Text = answer;
            }
        }

    }
}
