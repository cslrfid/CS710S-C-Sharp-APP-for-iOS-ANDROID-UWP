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
	public partial class PageKill : MvxContentPage
	{
		public PageKill()
		{
			InitializeComponent();

			if (Device.RuntimePlatform == Device.iOS)
			{
				this.Icon = new FileImageSource();
				this.Icon.File = "icons8-Settings-50-2-30x30.png";
			}
		}
	}
}
