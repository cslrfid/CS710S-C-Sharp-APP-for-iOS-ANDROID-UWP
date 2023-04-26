using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
	public class ViewModelSecurity : BaseViewModel
	{
		private readonly IUserDialogs _userDialogs;

		public string entrySelectedEPC { get; set; }
		public string entrySelectedPWD { get; set; }

		public string buttonEPCText { get; set; }
		public string buttonACCPWDText { get; set; }
		public string buttonKILLPWDText { get; set; }
		public string buttonTIDText { get; set; }
		public string buttonUSERText { get; set; }
        public string buttonFFFFFLockText { get; set; }

        public string labelStatus { get; set; }

		public ICommand OnApplyButtonCommand { protected set; get; }

		public ViewModelSecurity(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
		{
			_userDialogs = userDialogs;

			OnApplyButtonCommand = new Command(OnApplyButtonClicked);
		}

		public override void ViewAppearing()
		{
			base.ViewAppearing();
			BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
		}

		public override void ViewDisappearing()
		{
			BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
			base.ViewDisappearing();
		}

		protected override void InitFromBundle(IMvxBundle parameters)
		{
			base.InitFromBundle(parameters);

			string stringUnchanged = "UNCHANGED";

			entrySelectedEPC = BleMvxApplication._SELECT_EPC;
			entrySelectedPWD = "00000000";

			buttonEPCText = stringUnchanged;
			buttonACCPWDText = stringUnchanged;
			buttonKILLPWDText = stringUnchanged;
			buttonTIDText = stringUnchanged;
			buttonUSERText = stringUnchanged;
            buttonFFFFFLockText = "NOT APPLY";

            RaisePropertyChanged(() => entrySelectedEPC);
			RaisePropertyChanged(() => entrySelectedPWD);
			RaisePropertyChanged(() => buttonEPCText);
			RaisePropertyChanged(() => buttonACCPWDText);
			RaisePropertyChanged(() => buttonKILLPWDText);
			RaisePropertyChanged(() => buttonTIDText);
			RaisePropertyChanged(() => buttonUSERText);
            RaisePropertyChanged(() => buttonFFFFFLockText);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
		{
			if (e.access == CSLibrary.Constants.TagAccess.LOCK)
			{
				if (e.success)
				{
					labelStatus = "SUCCESS";
				}
				else
				{
					labelStatus = "FAIL";
				}

				RaisePropertyChanged(() => labelStatus);
			}
		}

		void OnApplyButtonClicked()
		{
			RaisePropertyChanged(() => entrySelectedEPC);
			RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => buttonFFFFFLockText);

            uint accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(/*m_record.pc.ToString() + */entrySelectedEPC);

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.DISABLE_ALL;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            BleMvxApplication._reader.rfid.Options.TagLock.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagLock.retryCount = 7;
            BleMvxApplication._reader.rfid.Options.TagLock.flags = CSLibrary.Constants.SelectFlags.SELECT;

            if (buttonFFFFFLockText == CSLibrary.Constants.Permission.PERM_LOCK.ToString())
            {
                BleMvxApplication._reader.rfid.Options.TagLock.permanentLock = true;
            }
            else
            {
                BleMvxApplication._reader.rfid.Options.TagLock.permanentLock = false;
                BleMvxApplication._reader.rfid.Options.TagLock.epcMemoryBankPermissions = (CSLibrary.Constants.Permission)Enum.Parse(typeof(CSLibrary.Constants.Permission), buttonEPCText);
                BleMvxApplication._reader.rfid.Options.TagLock.accessPasswordPermissions = (CSLibrary.Constants.Permission)Enum.Parse(typeof(CSLibrary.Constants.Permission), buttonACCPWDText);
                BleMvxApplication._reader.rfid.Options.TagLock.killPasswordPermissions = (CSLibrary.Constants.Permission)Enum.Parse(typeof(CSLibrary.Constants.Permission), buttonKILLPWDText);
                BleMvxApplication._reader.rfid.Options.TagLock.tidMemoryBankPermissions = (CSLibrary.Constants.Permission)Enum.Parse(typeof(CSLibrary.Constants.Permission), buttonTIDText);
                BleMvxApplication._reader.rfid.Options.TagLock.userMemoryBankPermissions = (CSLibrary.Constants.Permission)Enum.Parse(typeof(CSLibrary.Constants.Permission), buttonUSERText);
            }

			BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_LOCK);

			labelStatus = "WAIT";

			RaisePropertyChanged(() => labelStatus);
		}
	}
}
