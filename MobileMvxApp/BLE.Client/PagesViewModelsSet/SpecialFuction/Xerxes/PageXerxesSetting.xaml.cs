using MvvmCross.Forms.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLE.Client.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PageXerxesSetting : MvxContentPage
	{
        string[] _tagTypeOptions = { "Xerxes" };
        string[] _powerOptions = { "Low (16dBm)", "Mid (23dBm)", "High (30dBm)", "Follow system Setting" };
        string[] _targetOptions = { "A", "B", "Toggle A/B" };

        public PageXerxesSetting ()
		{
			InitializeComponent ();

            buttonTagType.Text = _tagTypeOptions[0];
            buttonPower.Text = _powerOptions[2];
            buttonTarget.Text = _targetOptions[2];
            entryDelay.Text = "15";
        }

        public async void buttonPowerClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Power", "Cancel", null, _powerOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonPower.Text = answer;
            }
        }

        public async void buttonTargetClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Target", "Cancel", null, _targetOptions);

            if (answer != null && answer !="Cancel")
                buttonTarget.Text = answer;
        }

        public async void ButtonOK_Clicked(object sender, EventArgs e)
        {
            //BleMvxApplication._xerxes_Power = Array.IndexOf(_powerOptions, buttonPower.Text);
            //BleMvxApplication._xerxes_Target = Array.IndexOf(_targetOptions, buttonTarget.Text);
            BleMvxApplication._xerxes_delay = int.Parse(entryDelay.Text);

            buttonOK.SetBinding(Button.CommandProperty, new Binding("OnOKButtonCommand"));
            buttonOK.Command.Execute(1);
            buttonOK.RemoveBinding(Button.CommandProperty);
        }
    }
}