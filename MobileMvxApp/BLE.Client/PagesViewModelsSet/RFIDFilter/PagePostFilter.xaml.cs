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
	public partial class PagePostFilter : MvxContentPage
	{
		public PagePostFilter()
		{
			InitializeComponent();

			entryMaskData.Text = BleMvxApplication._POSTFILTER_MASK_EPC;
			entryMaskOffset.Text = BleMvxApplication._POSTFILTER_MASK_Offset.ToString();
			switchMaskMatchEPC.IsToggled = BleMvxApplication._POSTFILTER_MASK_MatchNot;
			switchEnableFilter.IsToggled = BleMvxApplication._POSTFILTER_MASK_Enable;
		}

		public async void btnOKClicked(object sender, EventArgs e)
		{
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            BleMvxApplication._POSTFILTER_MASK_EPC = entryMaskData.Text;
			BleMvxApplication._POSTFILTER_MASK_Offset = uint.Parse(entryMaskOffset.Text);
			BleMvxApplication._POSTFILTER_MASK_MatchNot = switchMaskMatchEPC.IsToggled;
			BleMvxApplication._POSTFILTER_MASK_Enable = switchEnableFilter.IsToggled;

            BleMvxApplication.SaveConfig();
			//await this.Navigation.PopAsync();
		}
	}
}
