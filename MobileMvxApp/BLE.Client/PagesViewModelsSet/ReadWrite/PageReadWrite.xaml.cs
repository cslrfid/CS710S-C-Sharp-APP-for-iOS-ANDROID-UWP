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
    public partial class PageReadWrite : MvxContentPage<ViewModelReadWrite>
    {
        static string[] _bankSelectionItems = new string[] { "Security (Bank 0)", "EPC (Bank 1)", "TID (Bank 2)", "User (Bank 3)" };
        int _EPCLength = 24;

        public PageReadWrite()
		{
			InitializeComponent();

            BleMvxApplication._geiger_Bank = 1;
            buttonBank.Text = _bankSelectionItems[1];
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
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
                            entrySelectedMask.Text = BleMvxApplication._SELECT_EPC;
                            break;

                        case 2: // TID
                            if (BleMvxApplication._SELECT_TID.Length != 0)
                                entrySelectedMask.Text = BleMvxApplication._SELECT_TID;
                            break;
                    }
                }
            }
        }

        void InputFocused(object sender, EventArgs args)
        {
            double curY = ((Entry)sender).Y;
            double move;

            if (curY != 0)
            {
                move = -(curY - 97.5);
            }
            else
            {
                move = -174;
            }

            Content.LayoutTo(new Rectangle(0, move, Content.Bounds.Width, Content.Bounds.Height));
        }

        void InputACCPWDFocused(object sender, EventArgs args)
        {
            Content.LayoutTo(new Rectangle(0, -110, Content.Bounds.Width, Content.Bounds.Height));
        }


        void InputUnfocused(object sender, EventArgs args)
        {
            Content.LayoutTo(new Rectangle(0, 0, Content.Bounds.Width, Content.Bounds.Height));
        }

        int HexVal (string value, int offset = 1)
        {
            offset--;
            byte[] header = UnicodeEncoding.Unicode.GetBytes(value.Substring(offset, 1));

            if (header[0] >= 48 && header[0] <= 57)
                return  (header[0] - 48);
            else if (header[0] >= 65 && header[0] <= 70)
                return (header[0] - 55);
            else if (header[0] >= 97 && header[0] <= 102)
                return  (header[0] - 87);
            else
                return -1;
        }

        public async void onentryPCTextChanged(object sender, EventArgs e)
        {
            _EPCLength = 0;

            if (entryPC.Text.Length > 0)
                if (HexVal(entryPC.Text, entryPC.Text.Length) < 0)
                {
                    entryPC.Text = entryPC.Text.Remove(entryPC.Text.Length - 1);
                    return;
                }

            try
            {
                int epcWordLen;

                epcWordLen = HexVal(entryPC.Text.Substring(0, 1)) << 1;
                epcWordLen |= HexVal(entryPC.Text.Substring(1, 1)) >> 3;

                _EPCLength = epcWordLen * 4; 
                 
                labelEPCLength.Text = "EPC Length " + (epcWordLen * 16).ToString() + " bits";
            }
            catch (Exception ex)
            {

            }
        }

        public async void onentryPCUnfocused(object sender, EventArgs e)
        {
            if (entryPC.Text.Length == 0)
                entryPC.Text = "3000";
            if (entryPC.Text.Length > 4)
                entryPC.Text = entryPC.Text.Remove(4);
            else if (entryPC.Text.Length < 4)
                entryPC.Text += "0000".Remove(4-entryPC.Text.Length);

            onentryEPCTextChanged(sender, e);

            await DisplayAlert("Changing EPC Length will automatically modify to " + (_EPCLength * 4).ToString() + " bits", "", null, "OK");

            if (entrySelectedMask.Text.Length > _EPCLength)
                entrySelectedMask.Text = entrySelectedMask.Text.Substring(0, _EPCLength);
        }

        public async void onentryEPCTextChanged(object sender, EventArgs e)
        {
            if (entryEPC.Text.Length > 0)
                if (HexVal(entryEPC.Text, entryEPC.Text.Length) < 0)
                {
                    entryEPC.Text = entryEPC.Text.Remove(entryEPC.Text.Length - 1);
                    return;
                }

            if (entryEPC.Text.Length < _EPCLength)
                entryEPC.TextColor = Color.Red;
            else
            {
                entryEPC.TextColor = Color.Black;
                if (entryEPC.Text.Length > _EPCLength)
                    entryEPC.Text = entryEPC.Text.Remove(_EPCLength);
            }
        }

        public async void onentryEPCUnfocused(object sender, EventArgs e)
        {
            if (entryEPC.Text.Length != _EPCLength)
            {
                await DisplayAlert("EPC value invalid", "", null, "OK");
                if (switchEPC.IsToggled)
                    entryEPC.Focus();
            }
        }

        public async void onentryTemperatureCodeTextChanged(object sender, EventArgs e)
        {
        }

    }
}
