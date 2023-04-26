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
	public partial class PageRSSIFilter : MvxContentPage
	{
		static public string[] _filterTypeItems = new string[] { "Disable", "Enable RSSI Filter" };
		static public string[] _filterOptionItems = new string[] { "Disable", "Less than or Equal", "Greater than or Equal" };

		public PageRSSIFilter()
		{
			InitializeComponent();

			buttonFilterType.Text = _filterTypeItems[(uint)BleMvxApplication._RSSIFILTER_Type];
			buttonOptions.Text = _filterOptionItems[(uint)BleMvxApplication._RSSIFILTER_Option];
			if (BleMvxApplication._config.RFID_DBm)
			{
				labelThreshold.Text = "Threshold(dBm)";
				entryThreshold.Text = BleMvxApplication._RSSIFILTER_Threshold_dBm.ToString("F2");
			}
			else
			{
				labelThreshold.Text = "Threshold(dBV)";
				entryThreshold.Text = (BleMvxApplication._RSSIFILTER_Threshold_dBm + 106.98).ToString("F2");
			}
		}
	
		public async void buttonFilterTypeClicked(object sender, EventArgs e)
		{
			var answer = await DisplayActionSheet("", "Cancel", null, _filterTypeItems);

			if (answer != null && answer != "Cancel")
				buttonFilterType.Text = answer;
		}

		public async void buttonOptionsClicked(object sender, EventArgs e)
		{
			var answer = await DisplayActionSheet("", "Cancel", null, _filterOptionItems);

			if (answer != null && answer != "Cancel")
				buttonOptions.Text = answer;
		}

		public async void btnOKClicked(object sender, EventArgs e)
		{
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

			BleMvxApplication._RSSIFILTER_Type =  (CSLibrary.Constants.RSSIFILTERTYPE)Array.IndexOf (_filterTypeItems, buttonFilterType.Text);
			BleMvxApplication._RSSIFILTER_Option = (CSLibrary.Constants.RSSIFILTEROPTION)Array.IndexOf(_filterOptionItems, buttonOptions.Text);

			BleMvxApplication._RSSIFILTER_Threshold_dBm = double.Parse(entryThreshold.Text);
			if (!BleMvxApplication._config.RFID_DBm)
				BleMvxApplication._RSSIFILTER_Threshold_dBm -= 106.98;

            BleMvxApplication.SaveConfig();
		}
    }
}
