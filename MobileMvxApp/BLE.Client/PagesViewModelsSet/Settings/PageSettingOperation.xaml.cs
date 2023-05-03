using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

using static CSLibrary.FrequencyBand;

namespace BLE.Client.Pages
{

	public partial class PageSettingOperation : MvxContentPage
	{
        List<RegionCode> Regions;
        string[] ActiveRegionsTextList;
        double[] ActiveFrequencyList;
        string[] ActiveFrequencyTextList;

        RegionCode [] _regionsCode = new RegionCode[] {
            RegionCode.FCC,
            RegionCode.ETSI,
            RegionCode.CN,
            RegionCode.TW,
            RegionCode.KR,
            RegionCode.HK,
            RegionCode.JP,
            RegionCode.AU,
            RegionCode.MY,
            RegionCode.SG,
            RegionCode.IN,
            RegionCode.G800,
            RegionCode.ZA,
            RegionCode.BR1,
            RegionCode.BR2,
            RegionCode.BR3,
            RegionCode.BR4,
            RegionCode.BR5,
            RegionCode.ID,
            RegionCode.TH,
            RegionCode.JE,
            RegionCode.PH,
            RegionCode.ETSIUPPERBAND,
            RegionCode.NZ,
            RegionCode.UH1,
            RegionCode.UH2,
            RegionCode.LH,
            RegionCode.LH1,
            RegionCode.LH2,
            RegionCode.VE,
            RegionCode.AR,
            RegionCode.CL,
            RegionCode.CO,
            RegionCode.CR,
            RegionCode.DO,
            RegionCode.PA,
            RegionCode.PE,
            RegionCode.UY,
            RegionCode.BA,
            RegionCode.VI
        };
        string[] _regionsName = new string[] {
            "USACanada",
            "Europe",
            "China",
            "Taiwan",
            "Korea",
            "Hong Kong",
            "Japan",
            "Australia",
            "Malaysia",
            "Singapore",
            "India",
            "G800",
            "South Africa",
            "Brazil 915-927",
            "Brazil 902-906, 915-927",
            "Brazil 902-906",
            "Brazil 902-904",
            "Brazil 917-924",
            "Indonesia",
            "Thailand",
            "Israel",
            "Philippine",
            "ETSI Upper Band",
            "New Zealand",
            "UH1",
            "UH2",
            "LH",
            "LH1",
            "LH2",
            "Venezuela",
            "Argentina",
            "Chile",
            "Colombia",
            "Costa Rica",
            "Dominican Republic",
            "Panama",
            "Peru",
            "Uruguay",
            "Bangladesh",
            "Vietnam"
        };

        string[] _profileList;
        /*        string[] _profileList = {
                    "103: Miller 1 640kHz Tari 6.25us",
                    "302: Miller 1 640kHz Tari 7.25us",
                    "120: Miller 2 640kHz Tari 6.25us",
                    "323: Miller 2 640kHz Tari 7.5us",
                    "344: Miller 4 640kHz Tari 7.5us",
                    "345: Miller 4 640kHz Tari 7.5us",
                    "223: Miller 2 320kHz Tari 15us",
                    "222: Miller 2 320kHz Tari 20us",
                    "241: Miller 4 320kHz Tari 20us",
                    "244: Miller 4 250kHz Tari 20us",
                    "285: Miller 8 160kHz Tari 20us"
                };
        */

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

            var countryCode = BleMvxApplication._reader.rfid.GetCountryCode();

            if (countryCode == "-2")
                _regionsName[0] = "FCC";

            switch (countryCode)
            {
                case "-1":
                case "-8":
                    _freqOrderOptions = new string[] { "Fixed" };
                    break;

                default:
                    _freqOrderOptions = new string[] { "Hopping" };
                    break;
            }

            Regions = BleMvxApplication._reader.rfid.GetActiveRegionCode();
            ActiveRegionsTextList = Regions.OfType<object>().Select(o => _regionsName[(int)o - 1]).ToArray();

            ActiveFrequencyList = BleMvxApplication._reader.rfid.GetAvailableFrequencyTable(BleMvxApplication._config.RFID_Region);
            ActiveFrequencyTextList = ActiveFrequencyList.OfType<object>().Select(o => o.ToString()).ToArray();

            //buttonRegion.Text = _regionsName[(int)BleMvxApplication._config.RFID_Region - 1];
            buttonRegion.Text = _regionsName[0];
            if (Regions.Count == 1)
                buttonRegion.IsEnabled = false;
            switch (BleMvxApplication._config.RFID_FrequenceSwitch)
            {
                case 0:
                    buttonFrequencyOrder.Text = "Hopping";
                    break;
                case 1:
                    buttonFrequencyOrder.Text = "Fixed";
                    break;
            }
            if (_freqOrderOptions.Length == 1)
                buttonFrequencyOrder.IsEnabled = false;
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
                if (uint.Parse(profilestr.Substring(0, 3)) == BleMvxApplication._config.RFID_Profile)
                {
                    buttonProfile.Text = profilestr;
                    break;
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

                for (cnt = 0; cnt < _regionsName.Length; cnt++)
                {
                    if (_regionsName[cnt] == answer)
                    {
                        ActiveFrequencyList = BleMvxApplication._reader.rfid.GetAvailableFrequencyTable(_regionsCode[cnt]);
                        break;
                    }
                }
                if (cnt == _regionsName.Length)
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
            var answer = await DisplayActionSheet("Frequence Channel Order", "Cancel", null, ActiveFrequencyTextList);

            if (answer != null && answer != "Cancel")
                buttonFixedChannel.Text = answer;
        }

        public async void entryPowerCompleted(object sender, EventArgs e)
        {
            uint value;

            try
            {
                value = uint.Parse(entryPower.Text);
                if (value < 0 || value > 330)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryPower.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
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

            for (cnt = 0; cnt < _regionsName.Length; cnt++)
            {
                if (_regionsName[cnt] == buttonRegion.Text)
                {
                    BleMvxApplication._config.RFID_Region = _regionsCode[cnt];
                    break;
                }
            }
            if (cnt == _regionsName.Length)
                BleMvxApplication._config.RFID_Region = RegionCode.UNKNOWN;

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
                    BleMvxApplication._config.RFID_FixedChannel = (uint)cnt;
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

            BleMvxApplication._config.RFID_Profile = UInt16.Parse(buttonProfile.Text.Substring(0, 3));

            BleMvxApplication.SaveConfig();

            switch (BleMvxApplication._config.RFID_FrequenceSwitch)
            {
                case 0:
                    BleMvxApplication._reader.rfid.SetHoppingChannels(BleMvxApplication._config.RFID_Region);
                    break;
                case 1:
                    BleMvxApplication._reader.rfid.SetFixedChannel(BleMvxApplication._config.RFID_Region, BleMvxApplication._config.RFID_FixedChannel);
                    break;
                case 2:
                    BleMvxApplication._reader.rfid.SetAgileChannels(BleMvxApplication._config.RFID_Region);
                    break;
            }
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
            int cnt;
            RegionCode region = RegionCode.FCC;

            for (cnt = 0; cnt < _regionsName.Length; cnt++)
            {
                if (_regionsName[cnt] == buttonRegion.Text)
                {
                    region = _regionsCode[cnt];
                    break;
                }
            }

            //string[] profileList = new string[currentProfileList.Length];
            //for (cnt = 0; cnt < currentProfileList.Length; cnt++)
            //    profileList[cnt] = _profileList[cnt];

            var answer = await DisplayActionSheet(null, "Cancel", null, _profileList);

            if (answer != null && answer !="Cancel")
                buttonProfile.Text = answer;
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
