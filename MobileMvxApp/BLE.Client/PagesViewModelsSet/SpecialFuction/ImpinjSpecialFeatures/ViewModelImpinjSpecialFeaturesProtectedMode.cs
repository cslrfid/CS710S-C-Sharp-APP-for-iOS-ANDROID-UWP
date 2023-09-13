using System;

using Acr.UserDialogs;
using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;
using MvvmCross.ViewModels;
using System.Runtime.ConstrainedExecution;

namespace BLE.Client.ViewModels
{
    public class ViewModelImpinjSpecialFeaturesProtectedMode : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string buttonConfigurationModeText { get; set; }

        public ICommand OnWriteConfigWordCommand { protected set; get; }
        public ICommand OnResumeInvisbleTagtoNormalCommand { protected set; get; }

        uint accessPwd;

        public ViewModelImpinjSpecialFeaturesProtectedMode(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnWriteConfigWordCommand = new Command(OnAuthenticatedReadClick);
            OnResumeInvisbleTagtoNormalCommand = new Command(OnResumeInvisbleTagtoNormalClick);
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            SetEvent(true);
        }

        public override void ViewDisappearing()
        {
            base.ViewDisappearing();
        }

        public void SetEvent(bool onoff)
        {
            BleMvxApplication._reader.rfid.ClearEventHandler();

            if (onoff)
            {
                BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
                BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            }
        }


        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            entrySelectedEPC = BleMvxApplication._SELECT_EPC;
            entrySelectedPWD = "00000000";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            //RaisePropertyChanged(() => buttonConfigurationModeText);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.WRITE &&
                e.bank == CSLibrary.Constants.Bank.SPECIFIC &&
                e.success)
            {
                ShowDialog("Write Protected Mode Configuration SUCCESS!");
            }
            else
            {
                ShowDialog("Write Protected Mode Configuration FAIL!!!!!!");
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
            RaisePropertyChanged(() => buttonConfigurationModeText);

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);
            string newValue = buttonConfigurationModeText.Substring(2, 4);

            if (accessPwd == 0x00)
            {
                ShowDialog("Access Password can not ZERO!!!");
                return;
            }

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);


            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.BANK0;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 4;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 1;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = new UInt16[1] { CSLibrary.Tools.Hex.ToUInt16(newValue) };
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = accessPwd;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
        }



        
        void OnResumeInvisbleTagtoNormalClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            RaisePropertyChanged(() => entrySelectedPWD);

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            if (accessPwd == 0x00)
            {
                ShowDialog("Access Password can not ZERO!!!");
                return;
            }

            byte[] protectedModePIN = new byte[4];

            protectedModePIN[3] = (byte)(accessPwd);
            protectedModePIN[2] = (byte)(accessPwd >> 8);
            protectedModePIN[1] = (byte)(accessPwd >> 16);
            protectedModePIN[0] = (byte)(accessPwd >> 24);

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.BANK3;
            BleMvxApplication._reader.rfid.Options.TagSelected.Mask = protectedModePIN;
            BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 32;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);


            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.BANK0;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 4;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 1;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = new UInt16[1] { 0x0000 };
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = accessPwd;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
        }




        async void ShowDialog(string Msg)
        {
            var config = new ProgressDialogConfig()
            {
                Title = Msg,
                IsDeterministic = true,
                MaskType = MaskType.None,
            };
            /*
             *                 Black,
                            Gradient,
                            Clear,
                            None

             */

            using (var progress = _userDialogs.Progress(config))
            {
                progress.Show();
                await System.Threading.Tasks.Task.Delay(3000);
            }
        }

    }
}
