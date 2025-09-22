using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false, Animated = true)]
    public partial class PageSetting : MvxTabbedPage<ViewModelSetting>
	{
        public PageSetting()
        {
            InitializeComponent();

            switch (BleMvxApplication._reader.rfid.GetModel())
            {
                case CSLibrary.RFIDDEVICE.MODEL.CS108:
                case CSLibrary.RFIDDEVICE.MODEL.CS710S:
                    this.Children.RemoveAt(2);
                    break;

                case CSLibrary.RFIDDEVICE.MODEL.CS463:
                case CSLibrary.RFIDDEVICE.MODEL.CS203XL:
                    this.Children.RemoveAt(3);
                    break;
            }

        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
        }
    }
}
