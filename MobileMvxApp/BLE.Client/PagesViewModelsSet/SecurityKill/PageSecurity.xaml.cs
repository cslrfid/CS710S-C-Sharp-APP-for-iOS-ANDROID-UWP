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
	public partial class PageSecurity : MvxContentPage
	{
		string [] stringLockOprtions = new string[] { "UNLOCK", "PERM_UNLOCK", "LOCK", "PERM_LOCK", "UNCHANGED" };


		public PageSecurity()
		{
			InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-Settings-50-1-30x30.png";
            }
        }

        public async void buttonEPCClicked(object sender, EventArgs e)
		{
			var answer = await DisplayActionSheet(null, "Cancel", null, stringLockOprtions[0], stringLockOprtions[1], stringLockOprtions[2], stringLockOprtions[3], stringLockOprtions[4]);

			if (answer != null && answer !="Cancel")
				buttonEPC.Text = answer;
		}

		public async void buttonACCPWDClicked(object sender, EventArgs e)
		{
			var answer = await DisplayActionSheet(null, "Cancel", null, stringLockOprtions[0], stringLockOprtions[1], stringLockOprtions[2], stringLockOprtions[3], stringLockOprtions[4]);

			if (answer != null && answer !="Cancel")
				buttonACCPWD.Text = answer;
		}

		public async void buttonKILLPWDClicked(object sender, EventArgs e)
		{
			var answer = await DisplayActionSheet(null, "Cancel", null, stringLockOprtions[0], stringLockOprtions[1], stringLockOprtions[2], stringLockOprtions[3], stringLockOprtions[4]);

			if (answer != null && answer !="Cancel")
				buttonKILLPWD.Text = answer;
		}

		public async void buttonTIDClicked(object sender, EventArgs e)
		{
			var answer = await DisplayActionSheet(null, "Cancel", null, stringLockOprtions[0], stringLockOprtions[1], stringLockOprtions[2], stringLockOprtions[3], stringLockOprtions[4]);

			if (answer != null && answer !="Cancel")
				buttonTID.Text = answer;
		}

		public async void buttonUSERClicked(object sender, EventArgs e)
		{
			var answer = await DisplayActionSheet(null, "Cancel", null, stringLockOprtions[0], stringLockOprtions[1], stringLockOprtions[2], stringLockOprtions[3], stringLockOprtions[4]);

			if (answer != null && answer !="Cancel")
				buttonUSER.Text = answer;
		}

        public async void buttonFFFFFLockClicked(object sender, EventArgs e)
        {
            if (buttonFFFFFLock.Text == stringLockOprtions[3])
            {
                buttonFFFFFLock.Text = "NOT APPLY";

                //buttonEPC.Text = stringLockOprtions[4];
                buttonEPC.IsEnabled = true;

                //buttonACCPWD.Text = stringLockOprtions[4];
                buttonACCPWD.IsEnabled = true;

                //buttonKILLPWD.Text = stringLockOprtions[4];
                buttonKILLPWD.IsEnabled = true;

                //buttonUSER.Text = stringLockOprtions[4];
                buttonUSER.IsEnabled = true;

                //buttonTID.Text = stringLockOprtions[4];
                buttonTID.IsEnabled = true;

                //buttonUSER.Text = stringLockOprtions[4];
                buttonUSER.IsEnabled = true;
            }
            else
            {
                buttonFFFFFLock.Text = stringLockOprtions[3];

                //buttonEPC.Text = stringLockOprtions[3];
                buttonEPC.IsEnabled = false;

                //buttonACCPWD.Text = stringLockOprtions[3];
                buttonACCPWD.IsEnabled = false;

                //buttonKILLPWD.Text = stringLockOprtions[3];
                buttonKILLPWD.IsEnabled = false;

                //buttonUSER.Text = stringLockOprtions[3];
                buttonUSER.IsEnabled = false;

                //buttonTID.Text = stringLockOprtions[3];
                buttonTID.IsEnabled = false;

                //buttonUSER.Text = stringLockOprtions[3];
                buttonUSER.IsEnabled = false;
            }


        }
    }
}
