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
    public partial class PageImpinjSpecialFeaturesProtectedMode : MvxContentPage<ViewModelImpinjSpecialFeaturesProtectedMode>
    {
        public PageImpinjSpecialFeaturesProtectedMode()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
