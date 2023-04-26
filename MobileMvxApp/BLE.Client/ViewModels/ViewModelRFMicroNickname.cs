using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;

using Prism.Mvvm;
using MvvmCross.ViewModels;

//using Plugin.Share;
//using Plugin.Share.Abstractions;

namespace BLE.Client.ViewModels
{
    public class RFMicroTagNicknameViewModel : BindableBase
    {
        private string _EPC;
        public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }

        private string _Nickname;
        public string Nickname { get { return this._Nickname; } set { this.SetProperty(ref this._Nickname, value); } }
    }

    public class ViewModelRFMicroNickname : BaseViewModel
    {

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------
        
        static public ObservableCollection<RFMicroTagNicknameViewModel> _TagNicknameList = new ObservableCollection<RFMicroTagNicknameViewModel>();
        public ObservableCollection<RFMicroTagNicknameViewModel> TagNicknameList { get { return _TagNicknameList; } set { SetProperty(ref _TagNicknameList, value); } }

        #endregion

        public ViewModelRFMicroNickname(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        ~ViewModelRFMicroNickname()
        {
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

        async void ShowDialog(string Msg)
        {
            var config = new ProgressDialogConfig()
            {
                Title = Msg,
                IsDeterministic = true,
                MaskType = MaskType.Gradient,
            };

            using (var progress = _userDialogs.Progress(config))
            {
                progress.Show();
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }
    }
}
