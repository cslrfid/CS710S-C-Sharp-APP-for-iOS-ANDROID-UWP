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

namespace BLE.Client.ViewModels
{
    public class XerxesTIDListViewModel : BindableBase
    {
        private string _TID;
        public string TID { get { return this._TID; } set { this.SetProperty(ref this._TID, value); } }
    }

    public class ViewModelXerxesTIDList : BaseViewModel
    {

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------
        
        static public ObservableCollection<RFMicroTagNicknameViewModel> _TagList = new ObservableCollection<RFMicroTagNicknameViewModel>();
        public ObservableCollection<RFMicroTagNicknameViewModel> TagList { get { return _TagList; } set { SetProperty(ref _TagList, value); } }

        #endregion

        public ViewModelXerxesTIDList(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        ~ViewModelXerxesTIDList()
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
