using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
	public class ViewModelFilter : BaseViewModel
	{
		private readonly IUserDialogs _userDialogs;

		private IDevice _device;
		private System.Threading.Tasks.Task<IService> _services;


		public ViewModelFilter(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
		{
			_userDialogs = userDialogs;
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
	}
}
