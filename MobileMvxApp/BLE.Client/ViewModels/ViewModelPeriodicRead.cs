using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acr.UserDialogs;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class ViewModelPeriodicRead : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public ViewModelPeriodicRead (IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }


    }
}
