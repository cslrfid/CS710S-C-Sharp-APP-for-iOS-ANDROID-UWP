using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLE.Client.Pages
{
    public partial class PageImpinjSpecialFeaturesProtectedMode : MvxContentPage<ViewModelImpinjSpecialFeaturesProtectedMode>
    {

        string[] modeOptions = new string[] {"0x0000:\nDefault values.Tag will be visible and start \nin the ready state, in normal range with AutoTune enabled.",
                                      "0x0010:\nTag will be visible and start in the ready state, \nin short-range with AutoTune enabled.",
                                      "0x0012:\nTag will be invisible and start in the protected state, \nin shortrange with AutoTune enabled.",
                                      "0x0002:\nTag will be invisible and start in the protected state, \nin normal range with AutoTune enabled.",
                                      "0x0001:\nTag will visible and start in the ready state, \nin normal range with AutoTune disabled." };


        /*
        string[] modeOptions1 = new string[] {"0x0000",
                                      "0x0010",
                                      "0x0012",
                                      "0x0002",
                                      "0x0001" };

        List<string> modeOptions1 = new List<string> {"0x0000",
                                      "0x0010",
                                      "0x0012",
                                      "0x0002",
                                      "0x0001" };
        */
        public PageImpinjSpecialFeaturesProtectedMode()
        {
            InitializeComponent();

            buttonConfigurationMode.Text = "AAAAA";
            //buttonConfigurationMode.Text = modeOptions[0];
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            buttonConfigurationMode.Text = modeOptions[0];
            protectedModeOption.ItemsSource = modeOptions;
        }

        public async void buttonConfigurationModeClicked(object sender, EventArgs e)
        {
            protectedModeOption.IsVisible = true;

            /*
            var answer = await DisplayActionSheet("Mode", null, null, modeOptions);

            if (answer != null && answer != "Cancel")
                buttonConfigurationMode.Text = answer;
            */

            //protectedModeOption.IsVisible = false;
        }

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            buttonConfigurationMode.Text = (string)e.SelectedItem;
            protectedModeOption.IsVisible = false;
        }

    }
}
