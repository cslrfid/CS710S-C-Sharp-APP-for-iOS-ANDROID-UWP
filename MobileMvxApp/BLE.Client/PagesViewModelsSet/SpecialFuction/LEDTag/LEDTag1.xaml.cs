using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLE.Client.Pages;
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageLEDTag1 : MvxContentPage<ViewModelLEDTag1>
    {
		public PageLEDTag1()
		{
			InitializeComponent();
        }

        public async void switchswitchSelectedTagsPropertyChanged(object sender, EventArgs e)
        {
            if (switchSelectedTags == null)
                return;

           stacklayoutSelectedMask.IsVisible = switchSelectedTags.IsToggled;
        }

    }
}
