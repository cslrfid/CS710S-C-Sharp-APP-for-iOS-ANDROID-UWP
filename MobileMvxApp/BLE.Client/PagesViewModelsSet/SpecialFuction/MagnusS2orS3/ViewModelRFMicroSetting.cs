using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Plugin.BLE.Abstractions.Contracts;

using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using MvvmCross.ViewModels;
using MvvmCross;
using MvvmCross.Navigation;

namespace BLE.Client.ViewModels
{
    public class ViewModelRFMicroSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        public string buttonPowerText { get; set; }
        public string buttonTargetText { get; set; }
        public string buttonIndicatorsProfileText { get; set; }
        public string buttonSensorTypeText { get; set; }
        public string buttonSensorUnitText { get; set; }
        public string entryMinOCRSSIText { get; set; }
        public string entryMaxOCRSSIText { get; set; }
        public ICommand OnOKButtonCommand { protected set; get; }
        public ICommand OnNicknameButtonCommand { protected set; get; }

        public ViewModelRFMicroSetting(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            OnOKButtonCommand = new Command(OnOKButtonClicked);
            OnNicknameButtonCommand = new Command(OnNicknameButtonClicked);
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
        }

        public override void ViewDisappearing()
        {
            base.ViewDisappearing();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        void OnNicknameButtonClicked(object ind)
        {
            //ShowViewModel<ViewModelRFMicroNickname>(new MvxBundle());
            _navigation.Navigate<ViewModelRFMicroNickname>(new MvxBundle());
        }

        void OnOKButtonClicked(object ind)
        {
            if (ind != null)
                if ((int)ind == 1)
                {
                    switch (BleMvxApplication._rfMicro_TagType)
                    {
                        case 0: // S2
                            //ShowViewModel<ViewModelRFMicroS2Inventory>(new MvxBundle());
                            _navigation.Navigate<ViewModelRFMicroS2Inventory>(new MvxBundle());
                            break;

                        case 1: // S3
                            //ShowViewModel<ViewModelRFMicroS3Inventory>(new MvxBundle());
                            _navigation.Navigate<ViewModelRFMicroS3Inventory>(new MvxBundle());
                            break;
                    }
                }
        }
    }
}
