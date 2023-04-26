using System;
using Acr.UserDialogs;
using Plugin.BLE.Abstractions.Contracts;

using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;


        public ViewModelSetting(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            BleMvxApplication._reader.siliconlabIC.OnAccessCompleted += new EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs>(OnAccessCompletedEvent);
        }

        public override void ViewDisappearing()
        {
            BleMvxApplication._reader.siliconlabIC.OnAccessCompleted -= new EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs>(OnAccessCompletedEvent);

            base.ViewDisappearing();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        void OnAccessCompletedEvent(object sender, CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                switch (e.type)
                {
                    case CSLibrary.SiliconLabIC.Constants.AccessCompletedCallbackType.SERIALNUMBER:
                        _userDialogs.Alert("Serial Number : " + (string)e.info);
                        break;
                }
            });
        }
    }
}
