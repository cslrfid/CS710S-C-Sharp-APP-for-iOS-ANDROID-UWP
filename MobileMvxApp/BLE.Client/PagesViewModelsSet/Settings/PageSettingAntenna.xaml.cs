using System;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
	public partial class PageSettingAntenna : MvxContentPage
	{
        class ANTENNAOPTION
        {
            private global::Xamarin.Forms.Switch switchAntennaEnable;
            private global::Xamarin.Forms.Entry entryPower;
            private global::Xamarin.Forms.Entry entryDwell;
        }

        public PageSettingAntenna()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-Settings-50-3-30x30.png";
            }

            // the page only support 4 ports
            switch (BleMvxApplication._reader.rfid.GetAntennaPort())
            {
                case 2:
                    stacklayoutAntenna4.IsVisible = false;
                    break;

                case 4:
                    stacklayoutAntenna4.IsVisible = true;
                    break;

                default:
                    return;
            }

            switch (BleMvxApplication._reader.rfid.GetModel())
            {
                case CSLibrary.RFIDDEVICE.MODEL.CS203XL:
                    labelAntenna1.Text = "Antenna 1 (External)";
                    labelAntenna2.Text = "Antenna 2 (Internal)";
                    break;
            }

            ANTENNAOPTION[] antennaOptions = new ANTENNAOPTION[BleMvxApplication._reader.rfid.AntennaList.Count];

            switchAntenna1Enable.IsToggled = BleMvxApplication._config.RFID_AntennaEnable[0];
            switchAntenna2Enable.IsToggled = BleMvxApplication._config.RFID_AntennaEnable[1];
            switchAntenna3Enable.IsToggled = BleMvxApplication._config.RFID_AntennaEnable[2];
            switchAntenna4Enable.IsToggled = BleMvxApplication._config.RFID_AntennaEnable[3];

            entryPower1.Text = BleMvxApplication._config.RFID_Antenna_Power[0].ToString();
            entryPower2.Text = BleMvxApplication._config.RFID_Antenna_Power[1].ToString();
            entryPower3.Text = BleMvxApplication._config.RFID_Antenna_Power[2].ToString();
            entryPower4.Text = BleMvxApplication._config.RFID_Antenna_Power[3].ToString();

            entryDwell1.Text = BleMvxApplication._config.RFID_Antenna_Dwell[0].ToString();
            entryDwell2.Text = BleMvxApplication._config.RFID_Antenna_Dwell[1].ToString();
            entryDwell3.Text = BleMvxApplication._config.RFID_Antenna_Dwell[2].ToString();
            entryDwell4.Text = BleMvxApplication._config.RFID_Antenna_Dwell[3].ToString();
        }

        protected override void OnAppearing()
        {
            if (BleMvxApplication._settingPage1TagPopulationChanged)
            {
                BleMvxApplication._settingPage1TagPopulationChanged = false;
            }

            base.OnAppearing();
        }

        public async void btnOKClicked(object sender, EventArgs e)
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            BleMvxApplication._config.RFID_AntennaEnable[0] = switchAntenna1Enable.IsToggled;
            BleMvxApplication._config.RFID_AntennaEnable[1] = switchAntenna2Enable.IsToggled;
            BleMvxApplication._config.RFID_AntennaEnable[2] = switchAntenna3Enable.IsToggled;
            BleMvxApplication._config.RFID_AntennaEnable[3] = switchAntenna4Enable.IsToggled;

            BleMvxApplication._config.RFID_Antenna_Power[0] = uint.Parse(entryPower1.Text);
            BleMvxApplication._config.RFID_Antenna_Power[1] = uint.Parse(entryPower2.Text);
            BleMvxApplication._config.RFID_Antenna_Power[2] = uint.Parse(entryPower3.Text);
            BleMvxApplication._config.RFID_Antenna_Power[3] = uint.Parse(entryPower4.Text);

            BleMvxApplication._config.RFID_Antenna_Dwell[0] = uint.Parse(entryDwell1.Text);
            BleMvxApplication._config.RFID_Antenna_Dwell[1] = uint.Parse(entryDwell2.Text);
            BleMvxApplication._config.RFID_Antenna_Dwell[2] = uint.Parse(entryDwell3.Text);
            BleMvxApplication._config.RFID_Antenna_Dwell[3] = uint.Parse(entryDwell4.Text);

            BleMvxApplication.SaveConfig();

            for (int cnt = 0; cnt < BleMvxApplication._reader.rfid.GetAntennaPort(); cnt++)
            {
                BleMvxApplication._reader.rfid.SetAntennaPortState((uint)cnt, BleMvxApplication._config.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[cnt], cnt);
                BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell[cnt], (uint)cnt);
            }
        }
    }
}
