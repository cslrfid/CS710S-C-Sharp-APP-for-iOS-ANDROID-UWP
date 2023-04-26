using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;

namespace BLE.Client.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false, Animated = true)]
    public partial class PageSecurityKill : MvxTabbedPage<ViewModelSecurityKill>
    {
        public PageSecurityKill()
        {
            InitializeComponent();
        }
    }
}
