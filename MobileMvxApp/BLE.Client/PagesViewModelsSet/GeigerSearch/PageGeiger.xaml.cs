using System;
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;

namespace BLE.Client.Pages
{
    public partial class PageGeiger : MvxContentPage<ViewModelGeiger>
    {
        static string[] _bankSelectionItems = new string[] { "Security (Bank 0)", "EPC (Bank 1)", "TID (Bank 2)", "User (Bank 3)" };
        static uint _rssi;

        public PageGeiger()
        {
            InitializeComponent();

            if (BleMvxApplication._config.RFID_DBm)
            {
                labelThreshold.Text = "Threshold dBm";
                sliderThreshold.Minimum = -90;
                sliderThreshold.Maximum = -10;
                sliderThreshold.Value = -47;
            }
            else
            {
                labelThreshold.Text = "Threshold dBuV";
                sliderThreshold.Minimum = 17;
                sliderThreshold.Maximum = 97;
                sliderThreshold.Value = 60;
            }

            BleMvxApplication._geiger_Bank = 1;
            buttonBank.Text = _bankSelectionItems[1];
        }

        public async void buttonBankClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("", "Cancel", null, _bankSelectionItems);

            if (answer != null && answer != "Cancel")
            {
                if (buttonBank.Text != answer)
                {
                    buttonBank.Text = answer;
                    BleMvxApplication._geiger_Bank = int.Parse(buttonBank.Text.Substring(buttonBank.Text.Length - 2, 1));
                    switch (BleMvxApplication._geiger_Bank)
                    {
                        case 1: // EPC
                            entryMask.Text = BleMvxApplication._SELECT_EPC;
                            break;

                        case 2: // TID
                            if (BleMvxApplication._SELECT_TID.Length != 0)
                                entryMask.Text = BleMvxApplication._SELECT_TID;
                            break;
                    }
                }
            }
        }

        public async void entryPowerCompleted(object sender, EventArgs e)
        {
            uint value;

            try
            {
                value = uint.Parse(entryPower.Text);
                if (value < 0 || value > 300)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryPower.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                entryPower.Text = "300";
            }
        }

        void sliderThresholdValueChanged(object sender, EventArgs e)
        {
            labelThresholdValue.Text = ((int)(sliderThreshold.Value)).ToString();
        }

    }
}
