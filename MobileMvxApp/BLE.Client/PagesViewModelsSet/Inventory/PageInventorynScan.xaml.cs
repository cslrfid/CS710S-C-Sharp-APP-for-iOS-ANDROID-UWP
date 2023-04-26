using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;

namespace BLE.Client.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false, Animated = true)]
    public partial class PageInventorynScan : MvxTabbedPage<ViewModelInventorynScan>
    {
        public PageInventorynScan()
        {
            InitializeComponent();

            if (BleMvxApplication._inventoryEntryPoint != 0)
            {
                var pages = Children.GetEnumerator();
                pages.MoveNext(); // First page
                pages.MoveNext(); // Second page
                CurrentPage = pages.Current;
            }
        }
    }
}
