using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using System;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageBarcodeandRFIDInventory : MvxContentPage<ViewModelBarcodeandRFIDInventory>
    {
        public PageBarcodeandRFIDInventory()
		{
			InitializeComponent();
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
                entryPower.Text = "100";
            }
        }
    }
}
