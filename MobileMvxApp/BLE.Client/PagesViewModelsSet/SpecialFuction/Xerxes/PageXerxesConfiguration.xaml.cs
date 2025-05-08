using BLE.Client.ViewModels;
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
	public partial class PageXerxesConfiguration : MvxContentPage<ViewModelXerxesConfiguration>
    {
        public PageXerxesConfiguration()
		{
			InitializeComponent();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        async void entryX28Unfocused(object sender, EventArgs args)
        {
            try
            {
                var value = Convert.ToUInt16(entryX28.Text);
                entryX28.Text = value.ToString("D");
            }
            catch (Exception ex)
            {
                entryX28.Text = "0";
                await DisplayAlert ("Error", "Please enter correct value", "OK");
            }
        }

        async void entryX29Unfocused(object sender, EventArgs args)
        {
            try
            {
                var value = (UInt16)(fromFloat(float.Parse(entryX29.Text)));
            }
            catch (Exception ex)
            {
                entryX29.Text = "0.0";
                await DisplayAlert("Error", "Please enter correct value", "OK");
            }
        }

        async void entryX2AUnfocused(object sender, EventArgs args)
        {
            try
            {
                var value = (UInt16)(fromFloat(float.Parse(entryX2A.Text)));
            }
            catch (Exception ex)
            {
                entryX2A.Text = "0.0";
                await DisplayAlert("Error", "Please enter correct value", "OK");
            }
        }

        async void entryX2BUnfocused(object sender, EventArgs args)
        {
            try
            {
                var value = Convert.ToUInt16(entryX2B.Text);
                entryX2B.Text = value.ToString("D");
            }
            catch (Exception ex)
            {
                entryX2B.Text = "0";
                await DisplayAlert("Error", "Please enter correct value", "OK");
            }
        }

        async void entryX2CUnfocused(object sender, EventArgs args)
        {
            try
            {
                var value = Convert.ToUInt16(entryX2C.Text);
                entryX2C.Text = value.ToString("D");
            }
            catch (Exception ex)
            {
                entryX2C.Text = "0";
                await DisplayAlert("Error", "Please enter correct value", "OK");
            }
        }

        async void entryX2DUnfocused(object sender, EventArgs args)
        {
            try
            {
                var value = Convert.ToUInt16(entryX2D.Text);
                entryX2D.Text = value.ToString("D");
            }
            catch (Exception ex)
            {
                entryX2D.Text = "0";
                await DisplayAlert("Error", "Please enter correct value", "OK");
            }
        }

        async void entryX2EUnfocused(object sender, EventArgs args)
        {
            try
            {
                var value = Convert.ToUInt16(entryX2E.Text);
                entryX2E.Text = value.ToString("D");
            }
            catch (Exception ex)
            {
                entryX2E.Text = "0";
                await DisplayAlert("Error", "Please enter correct value", "OK");
            }
        }

        async void entryX2FUnfocused(object sender, EventArgs args)
        {
            try
            {
                var value = Convert.ToUInt16(entryX2F.Text);
                entryX2F.Text = value.ToString("D");
            }
            catch (Exception ex)
            {
                entryX2F.Text = "0";
                await DisplayAlert("Error", "Please enter correct value", "OK");
            }
        }

        async void buttonCleanClicked(object sender, EventArgs args)
        {
            entryX28.Text = "0";
            entryX29.Text = "0.0";
            entryX2A.Text = "0.0";
            entryX2B.Text = "0";
            entryX2C.Text = "0";
            entryX2D.Text = "0";
            entryX2E.Text = "0";
            entryX2F.Text = "0";
        }

        public int fromFloat(float fval)
        {
            int fbits = BitConverter.ToInt32(BitConverter.GetBytes(fval), 0);
            int sign = fbits >> 16 & 0x8000;          // sign only
            int val = (fbits & 0x7fffffff) + 0x1000; // rounded value

            if (val >= 0x47800000)               // might be or become NaN/Inf
            {                                     // avoid Inf due to rounding
                if ((fbits & 0x7fffffff) >= 0x47800000)
                {                                 // is or must become NaN/Inf
                    if (val < 0x7f800000)        // was value but too large
                        return sign | 0x7c00;     // make it +/-Inf
                    return sign | 0x7c00 |        // remains +/-Inf or NaN
                        (fbits & 0x007fffff) >> 13; // keep NaN (and Inf) bits
                }
                return sign | 0x7bff;             // unrounded not quite Inf
            }
            if (val >= 0x38800000)               // remains normalized value
                return sign | val - 0x38000000 >> 13; // exp - 127 + 15
            if (val < 0x33000000)                // too small for subnormal
                return sign;                      // becomes +/-0
            val = (fbits & 0x7fffffff) >> 23;  // tmp exp for subnormal calc
            return sign | ((fbits & 0x7fffff | 0x800000) // add subnormal bit
                 + (0x800000 >> val - 102)     // round depending on cut off
              >> 126 - val);   // div by 2^(1-(exp-127+15)) and >> 13 | exp=0
        }

    }
}
