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
	public partial class PageBlockWrite : MvxContentPage<ViewModelBlockWrite>
	{
        string[] _bankOptions = new string []{ "Bank3 (User Bank)", "Bank1 (EPC Bank)" };
        string[] _sizeOptions = new string[] { "4K bit", "8K bit" };
        string[] _paddingOptions = new string[] { "repeat 55AA", "repeat AA55", "repeat 0000", "repeat FFFF", "repeat 0001", "repeat 0002", "repeat 0004", "repeat 0008", "repeat 0010", "repeat 0020", "repeat 0040", "repeat 0080", "repeat 0100", "repeat 0200", "repeat 0400", "repeat 0800", "repeat 1000", "repeat 2000", "repeat 4000", "repeat 8000", "repeat FFFE", "repeat FFFD", "repeat FFFB", "repeat FFF7", "repeat FFEF", "repeat FFDF", "repeat FFBF", "repeat FF7F", "repeat FEFF", "repeat FDFF", "repeat FBFF", "repeat F7FF", "repeat EFFF", "repeat DFFF", "repeat BFFF", "repeat 7FFF" };
        //UInt16[] _paddingValue = new UInt16[] { 0x55AA, 0xAA55, 0x0000, 0xFFFF, 0x0001, 0x0002, 0x0004, 0x0008, 0x0010, 0x0020, 0x0040, 0x0080, 0x0100, 0x0200, 0x0400, 0x0800, 0x1000, 0x2000, 0x4000, 0x8000, 0xFFFE, 0xFFFD, 0xFFFB, 0xFFF7, 0xFFEF, 0xFFDF, 0xFFBF, 0xFF7F, 0xFEFF, 0xFDFF, 0xFBFF, 0xF7FF, 0xEFFF, 0xDFFF, 0xBFFF, 0x7FFF };

        public PageBlockWrite()
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

        public async void buttonSizeClicked(object sender, EventArgs args)
        {
            var answer = await DisplayActionSheet("Data Size", "Cancel", null, _sizeOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonSize.Text = answer;
            }
        }

        public async void buttonPaddingClicked(object sender, EventArgs args)
        {
            var answer = await DisplayActionSheet("Sensor Type", "Cancel", null, _paddingOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonPadding.Text = answer;
            }
        }
    }
}
