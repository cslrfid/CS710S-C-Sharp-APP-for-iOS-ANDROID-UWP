using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class ViewModelViewPage : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public ViewModelViewPage(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }
    }
}
