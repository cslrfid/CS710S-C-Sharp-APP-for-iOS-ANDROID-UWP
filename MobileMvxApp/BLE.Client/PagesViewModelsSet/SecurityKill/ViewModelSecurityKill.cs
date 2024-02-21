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
	public class ViewModelSecurityKill : BaseViewModel
	{
		private readonly IUserDialogs _userDialogs;

		public string entrySelectedEPC { get; set; }
		public string entrySelectedPWD { get; set; }
		public string entryKillSelectedEPC { get; set; }
		//public string entryKillSelectedPWD { get; set; }
		public string entryKillPWD { get; set; }

		public string buttonEPCText { get; set; }
		public string buttonACCPWDText { get; set; }
		public string buttonKILLPWDText { get; set; }
		public string buttonTIDText { get; set; }
		public string buttonUSERText { get; set; }
        public string buttonFFFFFLockText { get; set; }

        public string labelStatus { get; set; }
		public string labelKillStatus { get; set; }

		public ICommand OnApplyButtonCommand { protected set; get; }
		public ICommand OnKillButtonCommand { protected set; get; }

		public ViewModelSecurityKill(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
		{
			_userDialogs = userDialogs;

			OnApplyButtonCommand = new Command(OnApplyButtonClicked);
			OnKillButtonCommand = new Command(OnKillButtonClicked);
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
			entryKillSelectedEPC = BleMvxApplication._SELECT_EPC;
			entrySelectedPWD = "00000000";
			//entryKillSelectedPWD = "00000000";
			entryKillPWD = "00000000";

			buttonEPCText = stringUnchanged;
			buttonACCPWDText = stringUnchanged;
			buttonKILLPWDText = stringUnchanged;
			buttonTIDText = stringUnchanged;
			buttonUSERText = stringUnchanged;
            buttonFFFFFLockText = "NOT APPLY";

            RaisePropertyChanged(() => entrySelectedEPC);
			RaisePropertyChanged(() => entrySelectedPWD);
			RaisePropertyChanged(() => entryKillSelectedEPC);
			//RaisePropertyChanged(() => entryKillSelectedPWD);
			RaisePropertyChanged(() => entryKillPWD);
			RaisePropertyChanged(() => buttonEPCText);
			RaisePropertyChanged(() => buttonACCPWDText);
			RaisePropertyChanged(() => buttonKILLPWDText);
			RaisePropertyChanged(() => buttonTIDText);
			RaisePropertyChanged(() => buttonUSERText);
            RaisePropertyChanged(() => buttonFFFFFLockText);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
		{
			switch (e.access)
			{
				case CSLibrary.Constants.TagAccess.LOCK:
					if (e.success)
					{
						labelStatus = "Update Lock Status SUCCESS";
					}
					else
					{
						labelStatus = "Update Lock Status FAIL";
					}
					RaisePropertyChanged(() => labelStatus);
					break;

				case CSLibrary.Constants.TagAccess.KILL:
					if (e.success)
						labelKillStatus = "Kill Tag command sent!";
					else
                        labelKillStatus = "Kill Tag command ERROR! errcode = 0x" + BleMvxApplication._reader.rfid.LastMacErrorCode.ToString("X4");
                    RaisePropertyChanged(() => labelKillStatus);
					break;
			}
		}

		void OnApplyButtonClicked()
		{
			if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
			{
				//MessageBox.Show("Reader is busy now, please try later.");
				return;
			}

			RaisePropertyChanged(() => entrySelectedEPC);
			RaisePropertyChanged(() => entrySelectedPWD);
			RaisePropertyChanged(() => buttonEPCText);
			RaisePropertyChanged(() => buttonACCPWDText);
			RaisePropertyChanged(() => buttonKILLPWDText);
			RaisePropertyChanged(() => buttonTIDText);
			RaisePropertyChanged(() => buttonUSERText);
			RaisePropertyChanged(() => buttonFFFFFLockText);

            uint accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

			BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
			BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
			BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.DISABLE_ALL;
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

		void OnKillButtonClicked()
		{
			if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
			{
				//MessageBox.Show("Reader is busy now, please try later.");
				return;
			}

			RaisePropertyChanged(() => entryKillSelectedEPC);
			//RaisePropertyChanged(() => entryKillSelectedPWD);
			RaisePropertyChanged(() => entryKillPWD);

			BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
			BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entryKillSelectedEPC);
			BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
			BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
			BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.DISABLE_ALL;
			BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

			//BleMvxApplication._reader.rfid.Options.TagKill.accessPassword = Convert.ToUInt32(entryKillSelectedPWD, 16); ;
			BleMvxApplication._reader.rfid.Options.TagKill.killPassword = Convert.ToUInt32(entryKillPWD, 16); ;
			BleMvxApplication._reader.rfid.Options.TagKill.retryCount = 7;
			BleMvxApplication._reader.rfid.Options.TagKill.flags = CSLibrary.Constants.SelectFlags.SELECT;

			BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_KILL);

			labelKillStatus = "WAIT";

			RaisePropertyChanged(() => labelKillStatus);
		}
	}
}
