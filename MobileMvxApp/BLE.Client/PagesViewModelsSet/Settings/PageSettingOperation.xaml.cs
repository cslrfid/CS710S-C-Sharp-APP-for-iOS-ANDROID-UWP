using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

using static CSLibrary.FrequencyBand;

namespace BLE.Client.Pages
{

	public partial class PageSettingOperation : MvxContentPage
	{
        string[] ActiveRegionsTextList;
        double[] ActiveFrequencyList;
        string[] ActiveFrequencyTextList;
        string[] _profileList;
        string[] _freqOrderOptions;

        public PageSettingOperation()
        {
            InitializeComponent();

            _profileList = BleMvxApplication._reader.rfid.GetActiveLinkProfileName();

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-Settings-50-1-30x30.png";
            }

            stackLayoutInventoryDuration.IsVisible = stackLayoutPower.IsVisible = (BleMvxApplication._reader.rfid.GetAntennaPort() == 1);

            ActiveRegionsTextList = BleMvxApplication._reader.rfid.GetActiveCountryNameList();
            ActiveFrequencyList = BleMvxApplication._reader.rfid.GetAvailableFrequencyTable();
            ActiveFrequencyTextList = ActiveFrequencyList.OfType<object>().Select(o => o.ToString() + " MHz").ToArray();
            buttonRegion.Text = BleMvxApplication._config.RFID_Region;

            switch (BleMvxApplication._config.RFID_FrequenceSwitch)
            {
                case 0:
                    buttonFrequencyOrder.Text = "Hopping";
                    break;
                case 1:
                    buttonFrequencyOrder.Text = "Fixed";
                    break;
            }

            if (ActiveRegionsTextList.Count() == 1)
                buttonRegion.IsEnabled = false;

//            if (_freqOrderOptions.Length == 1)
                buttonFrequencyOrder.IsEnabled = false;

            if (BleMvxApplication._config.RFID_FixedChannel == -1)
                buttonFixedChannel.Text = ActiveFrequencyTextList[0];
            else
                buttonFixedChannel.Text = ActiveFrequencyTextList[BleMvxApplication._config.RFID_FixedChannel];

            checkbuttonFixedChannel();

            entryPower.Text = BleMvxApplication._config.RFID_Antenna_Power[0].ToString();
            entryInventoryDuration.Text = BleMvxApplication._config.RFID_Antenna_Dwell[0].ToString();
            entryCompactInventoryDelay.Text = BleMvxApplication._config.RFID_CompactInventoryDelayTime.ToString();
            entryIntraPacketDelay.Text = BleMvxApplication._config.RFID_IntraPacketDelayTime.ToString();
            entryDuplicateEliminationRollingWindow.Text = BleMvxApplication._config.RFID_DuplicateEliminationRollingWindow.ToString();

            buttonSession.Text = BleMvxApplication._config.RFID_TagGroup.session.ToString();
            if (BleMvxApplication._config.RFID_ToggleTarget)
            {
                buttonTarget.Text = "Toggle A/B";
            }
            else
            {
                buttonTarget.Text = BleMvxApplication._config.RFID_TagGroup.target.ToString();
            }

            switchFocus.IsToggled = BleMvxApplication._config.RFID_Focus;
            switchFastId.IsToggled = BleMvxApplication._config.RFID_FastId;

            buttonAlgorithm.Text = BleMvxApplication._config.RFID_Algorithm.ToString();
            entryTagPopulation.Text = BleMvxApplication._config.RFID_TagPopulation.ToString();
            if (BleMvxApplication._config.RFID_QOverride)
            {
                entryQOverride.IsEnabled = true;
                buttonQOverride.Text = "Reset";
            }
            else
            {
                entryQOverride.IsEnabled = false;
                buttonQOverride.Text = "Override";
            }

            entryMaxQ.Text = BleMvxApplication._config.RFID_DynamicQParms.maxQValue.ToString();
            entryMinQ.Text = BleMvxApplication._config.RFID_DynamicQParms.minQValue.ToString();
            entryMinQCycled.Text = BleMvxApplication._config.RFID_DynamicQParms.MinQCycles.ToString();

            switchQIncreaseUseQuery.IsToggled = BleMvxApplication._config.RFID_DynamicQParms.QIncreaseUseQuery;
            switchQDecreaseUseQuery.IsToggled = BleMvxApplication._config.RFID_DynamicQParms.QDecreaseUseQuery;
            entryNoEPCMaxQ.Text = BleMvxApplication._config.RFID_DynamicQParms.NoEPCMaxQ.ToString();

            foreach (string profilestr in _profileList)
            {
                int colonIndex = profilestr.IndexOf(":");
                if (colonIndex > 0 && uint.Parse(profilestr.Substring(0, colonIndex)) == BleMvxApplication._config.RFID_Profile)
                {
                    buttonProfile.Text = profilestr;
                    break;
                }
            }

            SetQvalue();
        }

        public async void buttonRegionClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Regions", "Cancel", null, ActiveRegionsTextList);

            if (answer != null && answer != "Cancel")
            {
                int cnt;

                buttonRegion.Text = answer;

                for (cnt = 0; cnt < ActiveRegionsTextList.Length; cnt++)
                {
                    if (ActiveRegionsTextList[cnt] == answer)
                    {
                        ActiveFrequencyList = BleMvxApplication._reader.rfid.GetAvailableFrequencyTable(ActiveRegionsTextList[cnt]).ToArray();
                        break;
                    }
                }
                if (cnt == ActiveRegionsTextList.Length)
                    ActiveFrequencyList = new double[1] { 0.0 };

                ActiveFrequencyTextList = ActiveFrequencyList.OfType<object>().Select(o => o.ToString()).ToArray();
                buttonFixedChannel.Text = ActiveFrequencyTextList[0];
            }
        }

        public async void buttonFrequencyOrderClicked(object sender, EventArgs e)
        {
            string answer;

            answer = await DisplayActionSheet("Frequence Channel Order", "Cancel", null, _freqOrderOptions);

            if (answer != null && answer != "Cancel")
                buttonFrequencyOrder.Text = answer;

            checkbuttonFixedChannel();
        }

        void checkbuttonFixedChannel()
        {
            if (buttonFrequencyOrder.Text == "Fixed")
                buttonFixedChannel.IsEnabled = true;
            else
                buttonFixedChannel.IsEnabled = false;
        }

        public async void buttonFixedChannelClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Frequency Channel Order", "Cancel", null, ActiveFrequencyTextList);

            if (answer != null && answer != "Cancel")
                buttonFixedChannel.Text = answer;
        }

        public async void entryPowerCompleted(object sender, EventArgs e)
        {
            uint value;

            try
            {
                value = uint.Parse(entryPower.Text);
                if (value < 0 || value > 320)
                    throw new System.ArgumentException("Power can only be set to 320 or below", "Power");
                entryPower.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Power", "Power can only be set to 320 or below", "OK");
                entryPower.Text = "300";
            }
        }

        public async void entryTagPopulationCompleted(object sender, EventArgs e)
        {
            uint tagPopulation;

            try
            {
                tagPopulation = uint.Parse(entryTagPopulation.Text);
                if (tagPopulation < 1 || tagPopulation > 8192)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryTagPopulation.Text = tagPopulation.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                tagPopulation = 60;
                entryTagPopulation.Text = "60";
            }

            if (!entryQOverride.IsEnabled)
                entryQOverride.Text = ((uint)(Math.Log((tagPopulation * 2), 2)) + 1).ToString();
        }

        public async void entryQOverrideCompiled(object sender, EventArgs e)
        {
            uint Q;
            try
            {
                Q = uint.Parse(entryQOverride.Text);
                if (Q < 0 || Q > 15)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryQOverride.Text = Q.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                Q = 7;
                entryQOverride.Text = "7";
            }

            //entryTagPopulation.Text = (1U << (int)Q).ToString();
        }

        public async void buttonQOverrideClicked(object sender, EventArgs e)
        {
            if (entryQOverride.IsEnabled)
            {
                entryQOverride.IsEnabled = false;
                buttonQOverride.Text = "Override";
                entryTagPopulationCompleted(null, null);
            }
            else
            {
                entryQOverride.IsEnabled = true;
                buttonQOverride.Text = "Reset";
            }
        }

        public async void entryDuplicateEliminationRollingWindowCompleted(object sender, EventArgs e)
        {
            uint value;

            try
            {
                value = uint.Parse(entryDuplicateEliminationRollingWindow.Text);
                if (value < 0 || value > 255)
                    throw new System.ArgumentException("Value not valid", "DuplicateEliminationRollingWindow");
                entryDuplicateEliminationRollingWindow.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                entryDuplicateEliminationRollingWindow.Text = "0";
            }
        }

        public async void entryCompactInventoryDelayCompleted(object sender, EventArgs e)
        {
            int value;

            try
            {
                value = int.Parse(entryCompactInventoryDelay.Text);
                if (value < 0 || value > 15)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryCompactInventoryDelay.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                entryCompactInventoryDelay.Text = "0";
            }
        }

        public async void entryIntraPacketDelayCompleted(object sender, EventArgs e)
        {
            int value;

            try
            {
                value = int.Parse(entryIntraPacketDelay.Text);
                if (value < 0 || value > 255)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryIntraPacketDelay.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                entryIntraPacketDelay.Text = "0";
            }
        }



        protected override void OnAppearing()
        {
            if (BleMvxApplication._settingPage1TagPopulationChanged)
            {
                BleMvxApplication._settingPage1TagPopulationChanged = false;
                entryTagPopulation.Text = BleMvxApplication._config.RFID_TagPopulation.ToString();
            }

            base.OnAppearing();
        }

        public async void btnOKClicked(object sender, EventArgs e)
        {
            int cnt;

            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            for (cnt = 0; cnt < ActiveRegionsTextList.Length; cnt++)
            {
                if (ActiveRegionsTextList[cnt] == buttonRegion.Text)
                {
                    BleMvxApplication._config.RFID_Region = ActiveRegionsTextList[cnt];
                    break;
                }
            }
            if (cnt == ActiveRegionsTextList.Length)
                BleMvxApplication._config.RFID_Region = "UNKNOWN";

            switch (buttonFrequencyOrder.Text)
            {
                case "Hopping":
                    BleMvxApplication._config.RFID_FrequenceSwitch = 0;
                    break;
                case "Fixed":
                    BleMvxApplication._config.RFID_FrequenceSwitch = 1;
                    break;
            }

            for (cnt = 0; cnt < ActiveFrequencyTextList.Length; cnt++)
            {
                if (buttonFixedChannel.Text == ActiveFrequencyTextList[cnt])
                {
                    BleMvxApplication._config.RFID_FixedChannel = cnt;
                    break;
                }
            }
            if (cnt == ActiveFrequencyTextList.Length)
                BleMvxApplication._config.RFID_FixedChannel = 0;

            BleMvxApplication._config.RFID_Antenna_Power[0] = UInt16.Parse(entryPower.Text);
            BleMvxApplication._config.RFID_Antenna_Dwell[0] = UInt16.Parse(entryInventoryDuration.Text);
            BleMvxApplication._config.RFID_CompactInventoryDelayTime = int.Parse(entryCompactInventoryDelay.Text);
            BleMvxApplication._config.RFID_IntraPacketDelayTime = int.Parse(entryIntraPacketDelay.Text);
            BleMvxApplication._config.RFID_DuplicateEliminationRollingWindow = byte.Parse(entryDuplicateEliminationRollingWindow.Text);

            switch (buttonSession.Text)
            {
                case "S0":
                    BleMvxApplication._config.RFID_TagGroup.session = CSLibrary.Constants.Session.S0;
                    break;

                case "S1":
                    BleMvxApplication._config.RFID_TagGroup.session = CSLibrary.Constants.Session.S1;
                    break;

                case "S2":
                    BleMvxApplication._config.RFID_TagGroup.session = CSLibrary.Constants.Session.S2;
                    break;

                case "S3":
                    BleMvxApplication._config.RFID_TagGroup.session = CSLibrary.Constants.Session.S3;
                    break;
            }

            switch (buttonTarget.Text)
            {
                case "A":
                    BleMvxApplication._config.RFID_ToggleTarget = false;
                    BleMvxApplication._config.RFID_TagGroup.target = CSLibrary.Constants.SessionTarget.A;
                    BleMvxApplication._config.RFID_FixedQParms.toggleTarget = 0;
                    BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = 0;
                    break;
                case "B":
                    BleMvxApplication._config.RFID_ToggleTarget = false;
                    BleMvxApplication._config.RFID_TagGroup.target = CSLibrary.Constants.SessionTarget.B;
                    BleMvxApplication._config.RFID_FixedQParms.toggleTarget = 0;
                    BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = 0;
                    break;
                default:
                    BleMvxApplication._config.RFID_ToggleTarget = true;
                    BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = 1;
                    BleMvxApplication._config.RFID_FixedQParms.toggleTarget = 1;
                    break;
            }
            BleMvxApplication._config.RFID_Focus = switchFocus.IsToggled;
            BleMvxApplication._config.RFID_FastId = switchFastId.IsToggled;

            if (buttonAlgorithm.Text == "DYNAMICQ")
            {
                BleMvxApplication._config.RFID_Algorithm = CSLibrary.Constants.SingulationAlgorithm.DYNAMICQ;
            }
            else
            {
                BleMvxApplication._config.RFID_Algorithm = CSLibrary.Constants.SingulationAlgorithm.FIXEDQ;
            }
            BleMvxApplication._config.RFID_TagPopulation = UInt16.Parse(entryTagPopulation.Text);
            BleMvxApplication._config.RFID_QOverride = entryQOverride.IsEnabled;
            BleMvxApplication._config.RFID_DynamicQParms.startQValue = uint.Parse(entryQOverride.Text);
            BleMvxApplication._config.RFID_DynamicQParms.maxQValue = uint.Parse(entryMaxQ.Text);
            BleMvxApplication._config.RFID_DynamicQParms.minQValue = uint.Parse(entryMinQ.Text);
            BleMvxApplication._config.RFID_FixedQParms.qValue = uint.Parse(entryQOverride.Text);
            BleMvxApplication._config.RFID_DynamicQParms.QIncreaseUseQuery = switchQIncreaseUseQuery.IsToggled;
            BleMvxApplication._config.RFID_DynamicQParms.QDecreaseUseQuery = switchQDecreaseUseQuery.IsToggled;
            BleMvxApplication._config.RFID_DynamicQParms.MinQCycles = uint.Parse(entryMinQCycled.Text);
            BleMvxApplication._config.RFID_DynamicQParms.NoEPCMaxQ = uint.Parse(entryNoEPCMaxQ.Text);

            int colonIndex = buttonProfile.Text.IndexOf(":");
            if (colonIndex > 0)
                BleMvxApplication._config.RFID_Profile = UInt16.Parse(buttonProfile.Text.Substring(0, colonIndex));

            BleMvxApplication.SaveConfig();

            BleMvxApplication._reader.rfid.SetCountry(BleMvxApplication._config.RFID_Region, (int)BleMvxApplication._config.RFID_FixedChannel);
        }

        public async void entryInventoryDurationCompleted(object sender, EventArgs e)
        {
            uint value;

            try
            {
                value = uint.Parse(entryInventoryDuration.Text);
                if (value < 0 || value > 3000)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryInventoryDuration.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                entryInventoryDuration.Text = "0";
            }
        }

        public async void buttonSessionClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Session", "Cancel", null, "S0", "S1", "S2", "S3"); // S2 S3

            if (answer != null && answer !="Cancel")
                buttonSession.Text = answer;
        }

        public async void buttonTargetClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(null, "Cancel", null, "A", "B", "Toggle A/B");

            if (answer != null && answer !="Cancel")
                buttonTarget.Text = answer;
        }

        public async void buttonAlgorithmClicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Algorithm", "", "DYNAMICQ", "FIXEDQ");
            buttonAlgorithm.Text = answer ? "DYNAMICQ" : "FIXEDQ";
        }

        void SetQvalue ()
        {
            switch (buttonAlgorithm.Text)
            {
                default:
                    entryQOverride.Text = "0";
                    break;

                case "DYNAMICQ":
                    entryQOverride.Text = BleMvxApplication._config.RFID_DynamicQParms.startQValue.ToString();
                    break;

                case "FIXEDQ":
                    entryQOverride.Text = BleMvxApplication._config.RFID_FixedQParms.qValue.ToString();
                    break;
            }
        }

        public async void buttonProfileClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(null, "Cancel", null, _profileList);

            if (answer != null && answer != "Cancel")
            {
                buttonProfile.Text = answer;

                if (BleMvxApplication._reader.rfid.GetModel() == CSLibrary.RFIDDEVICE.MODEL.CS108)
                {
                    if (uint.Parse(answer.Substring(0, 3)) == 3)
                        entryCompactInventoryDelay.Text = "2";
                    else
                        entryCompactInventoryDelay.Text = "0";
                }
            }
        }

        public async void switchFocusPropertyChanged(object sender, EventArgs e)
        {
            if (switchFocus == null)
                return;

            if (switchFocus.IsToggled)
            {
                buttonSession.Text = "S1";
                buttonTarget.Text = "A";
                entryCompactInventoryDelay.Text = "0";
                entryInventoryDuration.Text = "2000";
                buttonSession.IsEnabled = false;
                buttonTarget.IsEnabled = false;
                entryCompactInventoryDelay.IsEnabled = false;
                entryInventoryDuration.IsEnabled = false;
            }
            else
            {
                buttonSession.IsEnabled = true;
                buttonTarget.IsEnabled = true;
                entryCompactInventoryDelay.IsEnabled = true;
                entryInventoryDuration.IsEnabled = true;
            }

        }
    }
}
