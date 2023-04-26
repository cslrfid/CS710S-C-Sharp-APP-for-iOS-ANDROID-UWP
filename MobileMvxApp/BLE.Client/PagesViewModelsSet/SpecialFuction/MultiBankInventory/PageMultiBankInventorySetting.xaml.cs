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
    public partial class PageMultiBankInventorySetting : MvxContentPage<ViewModelMultiBankInventorySetting>
    {
        static public string[] _bankSelectionItems = new string[] { "TID (Bank 2)", "User (Bank 3)", "Security (Bank 0)", "EPC (Bank 1)" };

        public PageMultiBankInventorySetting()
		{
			InitializeComponent();
		}

        void InputUnfocused(object sender, EventArgs args)
        {
            Content.LayoutTo(new Rectangle(0, 0, Content.Bounds.Width, Content.Bounds.Height));
        }

        void InputFocused_114(object sender, EventArgs args)                            
        {
            MovePage(-114);
        }

        void InputFocused_144(object sender, EventArgs args)
        {
            MovePage(-144);
        }

        void InputFocused_174(object sender, EventArgs args)
        {
            MovePage(-174);
        }

        void MovePage(double move)
        {
              Content.LayoutTo(new Rectangle(0, move, Content.Bounds.Width, Content.Bounds.Height));
        }

        public async void buttonBank1Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("", "Cancel", null, _bankSelectionItems);

            if (answer != null && answer !="Cancel")
                buttonBank1.Text = answer;
        }

        public async void buttonBank2Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("", "Cancel", null, _bankSelectionItems);

            if (answer != null && answer !="Cancel")
                buttonBank2.Text = answer;
        }
    }
}
