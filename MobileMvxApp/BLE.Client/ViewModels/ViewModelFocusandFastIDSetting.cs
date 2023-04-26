using System;
using System.Collections.Generic;
using Acr.UserDialogs;

using Plugin.BLE.Abstractions.Contracts;

using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using MvvmCross.ViewModels;
using MvvmCross.Navigation;

namespace BLE.Client.ViewModels
{
    public class ViewModelFocusandFastIDSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        public ICommand OnbuttonOKCommand { protected set; get; }

        public Boolean switchFocusIsToggled { get; set; }
        public Boolean switchFastIDIsToggled { get; set; }

        public ViewModelFocusandFastIDSetting(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            switchFocusIsToggled = false;
            switchFastIDIsToggled = false;
            OnbuttonOKCommand = new Command(OnButtonOKClicked);
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        void OnButtonOKClicked(object ind)
        {
            BleMvxApplication._focus = switchFocusIsToggled;
            BleMvxApplication._fastID = switchFastIDIsToggled;

            //ShowViewModel<ViewModelFocusandFastIDInventory>(new MvxBundle());
            _navigation.Navigate<ViewModelFocusandFastIDInventory>(new MvxBundle());
        }
    }
}
