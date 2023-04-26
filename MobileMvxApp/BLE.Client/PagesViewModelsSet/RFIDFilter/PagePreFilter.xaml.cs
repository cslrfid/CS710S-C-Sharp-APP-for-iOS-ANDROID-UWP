using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Forms.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLE.Client.Pages
{
	public partial class PagePreFilter : MvxContentPage
	{
		static public string[] _bankSelectionItems = new string[] { "Security (Bank 0)", "EPC (Bank 1)", "TID (Bank 2)", "User (Bank 3)" };

		public PagePreFilter()
		{
			InitializeComponent();

			entryMaskData.Text = BleMvxApplication._PREFILTER_MASK_EPC;
			entryMaskOffset.Text = BleMvxApplication._PREFILTER_MASK_Offset.ToString();
			buttonBank.Text = _bankSelectionItems[BleMvxApplication._PREFILTER_Bank];
			switchEnableFilter.IsToggled = BleMvxApplication._PREFILTER_Enable;
		}

		public async void buttonBankClicked(object sender, EventArgs e)
		{
			var answer = await DisplayActionSheet("", "Cancel", null, _bankSelectionItems);

			if (answer != null && answer != "Cancel")
				buttonBank.Text = answer;
		}

		public async void btnOKClicked(object sender, EventArgs e)
		{
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            BleMvxApplication._PREFILTER_MASK_EPC = entryMaskData.Text;
			BleMvxApplication._PREFILTER_MASK_Offset = uint.Parse(entryMaskOffset.Text);
			BleMvxApplication._PREFILTER_Bank = int.Parse(buttonBank.Text.Substring(buttonBank.Text.Length - 2, 1));
			BleMvxApplication._PREFILTER_Enable = switchEnableFilter.IsToggled;

            BleMvxApplication.SaveConfig();
		}
    }
}
