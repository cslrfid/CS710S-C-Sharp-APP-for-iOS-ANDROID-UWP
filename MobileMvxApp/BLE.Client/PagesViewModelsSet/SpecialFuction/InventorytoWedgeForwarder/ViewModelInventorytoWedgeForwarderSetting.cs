using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross;


using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelInventorytoWedgeForwarderSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        public ICommand OnOKButtonCommand { protected set; get; }
        public string entryIPText { get ; set ;}
        public string entryPortText { get; set; }
        public bool switchswitchUniqueIsToggled { get; set; }

        public ViewModelInventorytoWedgeForwarderSetting(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            OnOKButtonCommand = new Command(OnOKButtonClicked);

            //entryIPText = "127.0.0.1";
            //entryPortText = "9394";
            //switchswitchUniqueIsToggled = false;

            //RaisePropertyChanged();
        }

        void RaisePropertyChanged()
        {
            //RaisePropertyChanged(() => entryIPText);
            //RaisePropertyChanged(() => entryPortText);
            //RaisePropertyChanged(() => switchswitchUniqueIsToggled);
        }

        void OnOKButtonClicked()
        {
            //RaisePropertyChanged();

            //BleMvxApplication.SaveConfig();

            _navigation.Navigate<ViewModelInventorytoWedgeForwarder>(new MvxBundle());
        }
    }
}
