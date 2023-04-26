using System;
using MvvmCross.Forms.Views;
using Xamarin.Forms;


namespace BLE.Client.Pages
{
	public partial class PageSettingAdministration : MvxContentPage
	{
        readonly string [] _ShareDataFormatOptions = new string[] { "JSON", "CSV", "Excel CSV" };

        public PageSettingAdministration()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-Settings-50-2-30x30.png";
            }

            switch (BleMvxApplication._config.BatteryLevelIndicatorFormat)
            {
                case 0:
                    buttonBatteryLevelFormat.Text = "Voltage";
                    break;

                default:
                    buttonBatteryLevelFormat.Text = "Percentage";
                    break;
            }

            switchInventoryAlertSound.IsToggled = BleMvxApplication._config.RFID_InventoryAlertSound;

            F1.Text = BleMvxApplication._config.RFID_Shortcut[0].Function.ToString();
            F1MinTime.Text = BleMvxApplication._config.RFID_Shortcut[0].DurationMin.ToString();
            F1MaxTime.Text = BleMvxApplication._config.RFID_Shortcut[0].DurationMax.ToString();
            F2.Text = BleMvxApplication._config.RFID_Shortcut[1].Function.ToString();
            F2MinTime.Text = BleMvxApplication._config.RFID_Shortcut[1].DurationMin.ToString();
            F2MaxTime.Text = BleMvxApplication._config.RFID_Shortcut[1].DurationMax.ToString();

            //entryTagDelay.Text = BleMvxApplication._config.RFID_TagDelayTime.ToString();
            //entryInventoryDuration.Text = BleMvxApplication._config.RFID_InventoryDuration.ToString();

            entryReaderName.Text = BleMvxApplication._reader.ReaderName;

            labelReaderModel.Text = "Reader Model : " + BleMvxApplication._reader.rfid.GetModelName() + BleMvxApplication._reader.rfid.GetCountryCode();

            switchNewTagLocation.IsToggled = BleMvxApplication._config.RFID_NewTagLocation;
            buttonShareDataFormat.Text = _ShareDataFormatOptions[BleMvxApplication._config.RFID_ShareFormat];

            switchRSSIDBm.IsToggled = BleMvxApplication._config.RFID_DBm;
            //switchSavetoFile.IsToggled = BleMvxApplication._config.RFID_SavetoFile;
            switchSavetoCloud.IsToggled = BleMvxApplication._config.RFID_SavetoCloud;
            switchhttpProtocol.IsToggled = (BleMvxApplication._config.RFID_CloudProtocol == 0) ? false : true;
            entryServerIP.Text = BleMvxApplication._config.RFID_IPAddress;

            switchVibration.IsToggled = BleMvxApplication._config.RFID_Vibration;
            //switchVibrationTag.IsToggled = BleMvxApplication._config.RFID_VibrationTag;
            entryVibrationWindow.Text = BleMvxApplication._config.RFID_VibrationWindow.ToString();
            entryVibrationTime.Text = BleMvxApplication._config.RFID_VibrationTime.ToString();

            //entryBatteryIntervalTime.Text = BleMvxApplication._config.RFID_BatteryPollingTime.ToString();

            if (Device.RuntimePlatform != Device.Android)
            {
                //switchSavetoFile.IsEnabled = false;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public async void btnOKClicked(object sender, EventArgs e)
        {
            int cnt;

            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            switch (buttonBatteryLevelFormat.Text)
            {
                case "Voltage":
                    BleMvxApplication._config.BatteryLevelIndicatorFormat = 0;
                    break;

                default:
                    BleMvxApplication._config.BatteryLevelIndicatorFormat = 1;
                    break;
            }

            BleMvxApplication._config.RFID_InventoryAlertSound = switchInventoryAlertSound.IsToggled;

            BleMvxApplication._config.RFID_Shortcut[0].Function = (CONFIG.MAINMENUSHORTCUT.FUNCTION)Enum.Parse(typeof(CONFIG.MAINMENUSHORTCUT.FUNCTION), F1.Text);
            BleMvxApplication._config.RFID_Shortcut[0].DurationMin = uint.Parse(F1MinTime.Text);
            BleMvxApplication._config.RFID_Shortcut[0].DurationMax = uint.Parse(F1MaxTime.Text);
            BleMvxApplication._config.RFID_Shortcut[1].Function = (CONFIG.MAINMENUSHORTCUT.FUNCTION)Enum.Parse(typeof(CONFIG.MAINMENUSHORTCUT.FUNCTION), F2.Text);
            BleMvxApplication._config.RFID_Shortcut[1].DurationMin = uint.Parse(F2MinTime.Text);
            BleMvxApplication._config.RFID_Shortcut[1].DurationMax = uint.Parse(F2MaxTime.Text);

            BleMvxApplication._config.RFID_DBm = switchRSSIDBm.IsToggled;
            //BleMvxApplication._config.RFID_SavetoFile = switchSavetoFile.IsToggled;
            BleMvxApplication._config.RFID_SavetoCloud = switchSavetoCloud.IsToggled;
            BleMvxApplication._config.RFID_CloudProtocol = switchhttpProtocol.IsToggled ? 1 : 0;
            BleMvxApplication._config.RFID_IPAddress = entryServerIP.Text;

            BleMvxApplication._config.RFID_NewTagLocation = switchNewTagLocation.IsToggled;
            BleMvxApplication._config.RFID_ShareFormat = Array.IndexOf(_ShareDataFormatOptions, buttonShareDataFormat.Text);

            //BleMvxApplication._config.RFID_TagDelayTime = int.Parse(entryTagDelay.Text);
            //BleMvxApplication._config.RFID_InventoryDuration = UInt32.Parse(entryInventoryDuration.Text);

            BleMvxApplication._config.RFID_Vibration = switchVibration.IsToggled;
            //BleMvxApplication._config.RFID_VibrationTag = switchVibrationTag.IsToggled;
            BleMvxApplication._config.RFID_VibrationWindow = UInt32.Parse(entryVibrationWindow.Text);
            BleMvxApplication._config.RFID_VibrationTime = UInt32.Parse(entryVibrationTime.Text);

            //BleMvxApplication._config.RFID_BatteryPollingTime = uint.Parse(entryBatteryIntervalTime.Text);

            BleMvxApplication.SaveConfig();

            if (entryReaderName.Text != BleMvxApplication._reader.ReaderName)
            {
                BleMvxApplication._reader.bluetoothIC.SetDeviceName (entryReaderName.Text);
                await DisplayAlert("New Reader Name effective after reset CS108", "", null, "OK");
            }
        }

        public async void buttonBatteryLevelFormatClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("View Battery Level Format", "Cancel", null, "Voltage", "Percentage");

            if (answer != null && answer !="Cancel")
                buttonBatteryLevelFormat.Text = answer;
        }

        public async void buttonShareDataFormatClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Share Data Format", null, null, _ShareDataFormatOptions);

            if (answer != null)
                buttonShareDataFormat.Text = answer;
        }

        public void btnBarcodeResetClicked(object sender, EventArgs e)
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.barcode.state == CSLibrary.BarcodeReader.STATE.NOTVALID)
            {
                DisplayAlert(null, "Barcode module not exists", "OK");
                return;
            }

            BleMvxApplication._reader.barcode.FactoryReset();
        }

        public async void btnConfigResetClicked(object sender, EventArgs e)
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);
            BleMvxApplication.ResetConfig();
            BleMvxApplication._reader.rfid.SetDefaultChannel();

            BleMvxApplication._config.RFID_Region = BleMvxApplication._reader.rfid.SelectedRegionCode;

            if (BleMvxApplication._reader.rfid.IsFixedChannel)
            {
                BleMvxApplication._config.RFID_FrequenceSwitch = 1;
                BleMvxApplication._config.RFID_FixedChannel = BleMvxApplication._reader.rfid.SelectedChannel;
            }
            else
            {
                BleMvxApplication._config.RFID_FrequenceSwitch = 0; // Hopping
            }

            BleMvxApplication.SaveConfig();
        }

        public async void btnGetSerialNumber(object sender, EventArgs e)
        {
            BleMvxApplication._reader.siliconlabIC.GetSerialNumber();
        }

        public async void btnFunctionSelectedClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(null, BLE.Client.CONFIG.MAINMENUSHORTCUT.FUNCTION.NONE.ToString(), null, BLE.Client.CONFIG.MAINMENUSHORTCUT.FUNCTION.INVENTORY.ToString(), BLE.Client.CONFIG.MAINMENUSHORTCUT.FUNCTION.BARCODE.ToString());

            Button b = (Button)sender;
            b.Text = answer;
        }

        public async void btnCSLCloudClicked(object sender, EventArgs e)
        {
            switchhttpProtocol.IsToggled = false;
            //entryServerIP.Text = "https://www.convergence.com.hk:29090/WebServiceRESTs/1.0/req";
            entryServerIP.Text = "https://democloud.convergence.com.hk:29090/WebServiceRESTs/1.0/req";
        }

        /*
        public async void entryTagDelayCompleted(object sender, EventArgs e)
        {
            uint value;

            try
            {
                value = uint.Parse(entryTagDelay.Text);
                if (value < 0 || value > 15)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryTagDelay.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                entryTagDelay.Text = "0";
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
        */

    }
}
