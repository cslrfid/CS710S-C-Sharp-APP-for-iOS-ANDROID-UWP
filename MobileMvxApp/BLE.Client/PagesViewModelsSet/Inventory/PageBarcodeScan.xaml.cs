using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false, Animated = true)]
    public partial class PageBarcodeScan : MvxContentPage
    {
        public PageBarcodeScan()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Icon = new FileImageSource();
                this.Icon.File = "find-barcode-icon-90058-30x30.png";
            }
        }
    }
}
