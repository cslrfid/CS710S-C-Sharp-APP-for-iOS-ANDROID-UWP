using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
	[MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false, Animated = true)]
	public partial class PageFilter : MvxTabbedPage<ViewModelFilter>
	{
		public PageFilter()
		{
			InitializeComponent();
		}
	}
}
