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
	public partial class PageXerxesReadTemp : MvxContentPage<ViewModelXerxesReadTemp>
    {
        public PageXerxesReadTemp()
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

    }
}
