using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace BLE.Client.Pages
{
    public partial class PageInventorytoWedgeForwarderSetting : MvxContentPage<ViewModelInventorytoWedgeForwarderSetting>
    {
        string[] duplicateFilterOptions_cs710 = new string[] { "0. Send Unique Tag only once \n (until buffer cleared)", "1. Send All Tags received", "2. Send unique tags using \n duplicate eliminate window" };
        string[] duplicateFilterOptions_cs108 = new string[] { "0. Send Unique Tag only once \n (until buffer cleared)", "1. Send All Tags received" };

        public PageInventorytoWedgeForwarderSetting()
		{
			InitializeComponent();

            entryIP.Text = BleMvxApplication._WedgeIP;
            entryPort.Text = BleMvxApplication._WedgePort.ToString();
            if (BleMvxApplication._reader.rfid.GetModel() == CSLibrary.RFIDDEVICE.MODEL.CS108)
            {
                buttonTagDuplicateFilter.Text = duplicateFilterOptions_cs710[1];
            }
            else
            {
                buttonTagDuplicateFilter.Text = duplicateFilterOptions_cs710[2];
            }
            entryRollingWindows.Text = BleMvxApplication._WedgeRollingWindows.ToString() ;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Device.RuntimePlatform == Device.Android)
            {
                CheckBatteryOptimization();
            }
            else
            {
                BatteryUsage.IsVisible = false;
                Normal.IsVisible = true;
            }
        }

        void CheckBatteryOptimization()
        {
            if (!DependencyService.Get<IBatteryOptimizationService>().IsBatteryOptimizationDisabled())
            {
                // Be sure to select unrestricted in battery usage to work properly.
                BatteryUsage.IsVisible = true;
                Normal.IsVisible = false;
            }
            else{
                BatteryUsage.IsVisible = false;
                Normal.IsVisible = true;
            }
        }

        public async void buttonTagDuplicateFilterClicked(object sender, EventArgs e)
        {
            string answer;

            if (BleMvxApplication._reader.rfid.GetModel() == CSLibrary.RFIDDEVICE.MODEL.CS710S)
                answer = await DisplayActionSheet("Duplicate Filter", "Cancel", null, duplicateFilterOptions_cs710);
            else
                answer = await DisplayActionSheet("Duplicate Filter", "Cancel", null, duplicateFilterOptions_cs108);

            if (answer != null && answer != "Cancel")
                buttonTagDuplicateFilter.Text = answer;
        }


        public async void buttonTagDuplicateFilterPropertyChanged(object sender, EventArgs e)
        {
            try
            {
                if (buttonTagDuplicateFilter != null)
                if (buttonTagDuplicateFilter.Text.Substring(0, 1) == "2")
                    sracklayoutRollingWindows.IsVisible = true;
                else
                    sracklayoutRollingWindows.IsVisible = false;
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine(ex.Message);
            }
        }

        public async void buttonOKClicked(object sender, EventArgs e)
        {
            BleMvxApplication._WedgeIP = entryIP.Text;
            try
            {
                BleMvxApplication._WedgePort = int.Parse(entryPort.Text);
                BleMvxApplication._WedgeDuplicateFilter = int.Parse(buttonTagDuplicateFilter.Text.Substring(0, 1));
                BleMvxApplication._WedgeRollingWindows = int.Parse(entryRollingWindows.Text);
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine(ex.Message);
            }
        }

        public async void buttonGotoBatteryUsageSettingClicked(object sender, EventArgs e)
        {
            await DependencyService.Get<IBatteryOptimizationService>().OpenBatteryOptimizationsAsync();

            //System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            //System.Diagnostics.Process.GetCurrentProcess().Close();
        }
    }
}
