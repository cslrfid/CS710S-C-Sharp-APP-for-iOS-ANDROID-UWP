
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageMainMenu : MvxContentPage<ViewModelMainMenu>
    {
        public PageMainMenu()
        {
            InitializeComponent();
            this.Title = "CSL RFID Reader (C# " + DependencyService.Get<IAppVersion>().GetVersion() + ")";
        }
    }
}