using System;

using Acr.UserDialogs;
using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelImpinjSpecialFeaturesConfig : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }

        public string entryAuthenticatedResultText { get; set; }

        public ICommand OnAuthenticatedReadCommand { protected set; get; }

        uint accessPwd;

        public ViewModelImpinjSpecialFeaturesConfig(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnAuthenticatedReadCommand = new Command(OnAuthenticatedReadClick);
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            SetEvent(true);
        }

        public override void ViewDisappearing()
        {
            SetEvent(false);
            base.ViewDisappearing();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            entrySelectedEPC = BleMvxApplication._SELECT_EPC;
            entrySelectedPWD = "00000000";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
        }

        void SetEvent(bool onoff)
        {
            BleMvxApplication._reader.rfid.ClearEventHandler();
            
            if (onoff)
                BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.AUTHENTICATE && e.success)
            {
                entryAuthenticatedResultText = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                RaisePropertyChanged(() => entryAuthenticatedResultText);
            }
            else
            {
                ShowDialog("Authenticated Read ERROR!!!");
            }


            //            if (e.access == CSLibrary.Constants.TagAccess.READ)
            //            {
            /*
                            switch (e.bank)
                            {
                                case CSLibrary.Constants.Bank:
                                    break;
            */


            /*

                        if (e.access == CSLibrary.Constants.TagAccess.READ)
                        {
                            switch (e.bank)
                            {
                                case CSLibrary.Constants.Bank.PC:

                                case CSLibrary.Constants.Bank.EPC:
                                    if (e.success)
                                    {
                                        entryEPC = BleMvxApplication._reader.rfid.Options.TagReadEPC.epc.ToString();

                                        RaisePropertyChanged(() => entryEPC);
                                        labelEPCStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--readEPCRetryCnt == 0)
                                            labelEPCStatus = "Er";
                                        else
                                            ReadEPC();
                                    }
                                    RaisePropertyChanged(() => labelEPCStatus);
                                    break;

                                case CSLibrary.Constants.Bank.ACC_PWD:
                                    if (e.success)
                                    {
                                        entryACCPWD = BleMvxApplication._reader.rfid.Options.TagReadAccPwd.password.ToString();
                                        RaisePropertyChanged(() => entryACCPWD);
                                        labelACCPWDStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--readACCPWDRetryCnt == 0)
                                            labelACCPWDStatus = "Er";
                                        else
                                            ReadACCPWD();
                                    }
                                    RaisePropertyChanged(() => labelACCPWDStatus);
                                    break;

                                case CSLibrary.Constants.Bank.KILL_PWD:
                                    if (e.success)
                                    {
                                        entryKILLPWD = BleMvxApplication._reader.rfid.Options.TagReadKillPwd.password.ToString();
                                        RaisePropertyChanged(() => entryKILLPWD);
                                        labelKILLPWDStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--readKILLPWDRetryCnt == 0)
                                            labelKILLPWDStatus = "Er";
                                        else
                                            ReadKILLPWD();
                                    }
                                    RaisePropertyChanged(() => labelKILLPWDStatus);
                                    break;

                                case CSLibrary.Constants.Bank.TID:
                                    if (e.success)
                                    {
                                        entryTIDUID = BleMvxApplication._reader.rfid.Options.TagReadTid.tid.ToString();
                                        RaisePropertyChanged(() => entryTIDUID);
                                        labelTIDUIDStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--readTIDUIDRetryCnt == 0)
                                            labelTIDUIDStatus = "Er";
                                        else
                                            ReadTIDUID();
                                    }
                                    RaisePropertyChanged(() => labelTIDUIDStatus);
                                    break;

                                case CSLibrary.Constants.Bank.USER:
                                    if (e.success)
                                    {
                                        entryUSER = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToString();
                                        RaisePropertyChanged(() => entryUSER);
                                        labelUSERStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--readUSERRetryCnt == 0)
                                            labelUSERStatus = "Er";
                                        else
                                            ReadUSER();
                                    }
                                    RaisePropertyChanged(() => labelUSERStatus);
                                    break;

                                case CSLibrary.Constants.Bank.SPECIFIC:
                                    switch (_process)
                                    {
                                        default:
                                            if (e.success)
                                            {
                                                entryKILLPWD = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                                RaisePropertyChanged(() => entryKILLPWD);
                                                labelKILLPWDStatus = "Ok";
                                            }
                                            else
                                            {
                                                if (--readKILLPWDRetryCnt == 0)
                                                    labelKILLPWDStatus = "Er";
                                                else
                                                    ReadKILLPWD();
                                            }
                                            RaisePropertyChanged(() => labelKILLPWDStatus);
                                            break;

                                        case 1:
                                            if (e.success)
                                            {
                                                entryMulti = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                                RaisePropertyChanged(() => entryMulti);
                                                labelMultiStatus = "Ok";
                                            }
                                            else
                                            {
                                                if (--readMultiRetryCnt == 0)
                                                    labelMultiStatus = "Er";
                                                else
                                                    ReadMulti();
                                            }
                                            RaisePropertyChanged(() => labelMultiStatus);
                                            break;
                                    }
                                    break;
                            }
                        }

                        if (e.access == CSLibrary.Constants.TagAccess.WRITE)
                        {
                            switch (e.bank)
                            {
                                case CSLibrary.Constants.Bank.PC:
                                    if (e.success)
                                    {
                                        labelPCStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--writePCRetryCnt == 0)
                                            labelPCStatus = "Er";
                                        else
                                            WritePC();
                                    }
                                    RaisePropertyChanged(() => labelPCStatus);
                                    break;

                                case CSLibrary.Constants.Bank.EPC:
                                    if (e.success)
                                    {
                                        labelEPCStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--writeEPCRetryCnt == 0)
                                            labelEPCStatus = "Er";
                                        else
                                            WriteEPC();
                                    }
                                    RaisePropertyChanged(() => labelEPCStatus);
                                    break;

                                case CSLibrary.Constants.Bank.ACC_PWD:
                                    if (e.success)
                                    {
                                        labelACCPWDStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--writeACCPWDRetryCnt == 0)
                                            labelACCPWDStatus = "Er";
                                        else
                                            WriteACCPWD();
                                    }
                                    RaisePropertyChanged(() => labelACCPWDStatus);
                                    break;

                                case CSLibrary.Constants.Bank.KILL_PWD:
                                    if (e.success)
                                    {
                                        labelKILLPWDStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--writeKILLPWDRetryCnt == 0)
                                            labelKILLPWDStatus = "Er";
                                        else
                                            WriteKILLPWD();
                                    }
                                    RaisePropertyChanged(() => labelKILLPWDStatus);
                                    break;

                                case CSLibrary.Constants.Bank.USER:
                                    if (e.success)
                                    {
                                        labelUSERStatus = "Ok";
                                    }
                                    else
                                    {
                                        if (--writeUSERRetryCnt == 0)
                                            labelUSERStatus = "Er";
                                        else
                                            WriteUSER();
                                    }
                                    RaisePropertyChanged(() => labelUSERStatus);
                                    break;

                                case CSLibrary.Constants.Bank.SPECIFIC:
                                    switch (_process)
                                    {
                                        default:
                                            if (e.success)
                                            {
                                                labelKILLPWDStatus = "Ok";
                                            }
                                            else
                                            {
                                                if (--writeKILLPWDRetryCnt == 0)
                                                    labelKILLPWDStatus = "Er";
                                                else
                                                    WriteKILLPWD();
                                            }
                                            RaisePropertyChanged(() => labelKILLPWDStatus);
                                            break;

                                        case 1:
                                            if (e.success)
                                            {
                                                labelMultiStatus = "Ok";
                                            }
                                            else
                                            {
                                                if (--writeMultiRetryCnt == 0)
                                                    labelMultiStatus = "Er";
                                                else
                                                    WriteMulti();
                                            }
                                            RaisePropertyChanged(() => labelMultiStatus);
                                            break;
                                    }
                                    break;

                            }
                        }
              */
            //}
            //          }
        }

        void OnAuthenticatedReadClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.CSI = 1;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x30;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "049ca53e55ea";
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.ResponseLen = 0x80;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
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
                await System.Threading.Tasks.Task.Delay(3000);
            }
        }

    }
}
