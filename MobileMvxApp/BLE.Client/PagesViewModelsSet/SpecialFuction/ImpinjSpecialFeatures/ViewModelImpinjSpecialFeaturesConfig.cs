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
        public ICommand OnTAM2AuthenticateCommand { protected set; get; }

        uint accessPwd;

        public ViewModelImpinjSpecialFeaturesConfig(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnAuthenticatedReadCommand = new Command(OnAuthenticatedReadClick);
            OnTAM2AuthenticateCommand = new Command(OnTAM2AuthenticatedClick);
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

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.password = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.CSI = 1;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x30;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "009ca53e55ea";
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.ResponseLen = 0x40;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void OnTAM2AuthenticatedClick()
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

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.password = accessPwd;
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
