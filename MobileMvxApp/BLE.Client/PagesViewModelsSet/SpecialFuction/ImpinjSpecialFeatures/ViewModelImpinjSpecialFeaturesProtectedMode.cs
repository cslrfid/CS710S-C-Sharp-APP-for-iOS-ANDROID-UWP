using System;

using Acr.UserDialogs;
using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;
using MvvmCross.ViewModels;
using System.Runtime.ConstrainedExecution;
using CSLibrary.Structures;

namespace BLE.Client.ViewModels
{
    public class ViewModelImpinjSpecialFeaturesProtectedMode : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string buttonTagModelText { get; set; }
        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public bool switchDisableAutoTuneIsToggled { get; set; }
        public bool switchEnableProtectToggled { get; set; }
        public bool switchEnableShortRangeIsToggled { get; set; }
        public bool switchEnableUnkillableIsToggled { get; set; }
        public ICommand OnNormalReadConfigWordCommand { protected set; get; }
        public ICommand OnNormalWriteConfigWordCommand { protected set; get; }
        public ICommand OnProtectReadConfigWordCommand { protected set; get; }
        public ICommand OnProtectWriteConfigWordCommand { protected set; get; }
        public ICommand OnResumeInvisbleTagtoNormalCommand { protected set; get; }

        int accessMode = 0;

        public ViewModelImpinjSpecialFeaturesProtectedMode(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnNormalReadConfigWordCommand = new Command(OnNormalReadConfigWordClick);
            OnNormalWriteConfigWordCommand = new Command(OnNormalWriteConfigWordClick);
            OnProtectReadConfigWordCommand = new Command(OnProtectReadConfigWordClick);
            OnProtectWriteConfigWordCommand = new Command(OnProtectWriteConfigWordClick);
            OnResumeInvisbleTagtoNormalCommand = new Command(OnResumeInvisbleTagtoNormalClick);

            buttonTagModelText = "M775";
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
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.READ &&
                e.bank == CSLibrary.Constants.Bank.SPECIFIC)
            {
                if (e.success)
                {
                    UInt16 configWord = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToUshorts()[0];

                    SetConfigWord(configWord);
                    _userDialogs.ShowSuccess ("Read Config Word SUCCESS! Config word value 0x" + configWord.ToString("X4"), 3000);

                    //ShowDialog("Read Config Word SUCCESS! Config word value 0x" + configWord.ToString("X4"));
                }
                else
                {
                    switch (accessMode)
                    {
                        case 1:
                            _userDialogs.ShowError("read Config Word in Normal mode FAIL, please try read in protect mode!!!!!!", 3000);
                            break;

                        case 3:
                            _userDialogs.ShowError("read Config Word in protect mode FAIL!!!!!!", 3000);
                            break;
                    }
                }
            }

            if (e.access == CSLibrary.Constants.TagAccess.WRITE &&
                e.bank == CSLibrary.Constants.Bank.SPECIFIC)
            {
                if (e.success)
                {
                    _userDialogs.ShowSuccess("Write Config Word SUCCESS!", 3000);
                }
                else
                {
                    switch (accessMode)
                    {
                        case 2:
                            _userDialogs.ShowError("write Config Word in Normal mode FAIL, please try read in protect mode!!!!!!", 3000);
                            break;

                        case 4:
                        case 5:
                            _userDialogs.ShowError("write Config Word in protect mode FAIL!!!!!!", 3000);
                            break;
                    }
                }
            }

            accessMode = 0;
        }

        void SelectNormalTag (string epc)
        {
            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new S_MASK(epc);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)(epc.Length) * 4;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
        }

        void SelectProtectTag (UInt32 password)
        {
            byte[] protectedModePIN = new byte[4];

            protectedModePIN[3] = (byte)(password);
            protectedModePIN[2] = (byte)(password >> 8);
            protectedModePIN[1] = (byte)(password >> 16);
            protectedModePIN[0] = (byte)(password >> 24);

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.BANK3;
            BleMvxApplication._reader.rfid.Options.TagSelected.Mask = protectedModePIN;
            BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 32;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
        }

        UInt16 GetConfigWord()
        {
            UInt16 value = 0;
            
            RaisePropertyChanged(() => switchDisableAutoTuneIsToggled);
            RaisePropertyChanged(() => switchEnableProtectToggled);
            RaisePropertyChanged(() => switchEnableShortRangeIsToggled);
            RaisePropertyChanged(() => switchEnableUnkillableIsToggled);

            if (switchDisableAutoTuneIsToggled)
                value |= 0x01;

            if (switchEnableProtectToggled)
                value |= 0x02;

            if (switchEnableShortRangeIsToggled)
                value |= 0x04;

            if (switchEnableUnkillableIsToggled)
                value |= 0x08;

            return value;
        }

        void SetConfigWord(UInt16 configWord)
        {
            UInt16 value = 0;


            if ((configWord & 0x01) == 0x00)
                switchDisableAutoTuneIsToggled = false;
            else
                switchDisableAutoTuneIsToggled = true;

            if ((configWord & 0x02) == 0x00)
                switchEnableProtectToggled = false;
            else
                switchEnableProtectToggled = true;

            if ((configWord & 0x04) == 0x00)
                switchEnableShortRangeIsToggled = false;
            else
                switchEnableShortRangeIsToggled = true;

            if ((configWord & 0x08) == 0x00)
                switchEnableUnkillableIsToggled = false;
            else
                switchEnableUnkillableIsToggled = true;

            RaisePropertyChanged(() => switchDisableAutoTuneIsToggled);
            RaisePropertyChanged(() => switchEnableProtectToggled);
            RaisePropertyChanged(() => switchEnableShortRangeIsToggled);
            RaisePropertyChanged(() => switchEnableUnkillableIsToggled);
        }

        void OnNormalReadConfigWordClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                ShowDialog("Reader is busy now, please try later.");
                return;
            }

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            SelectNormalTag(entrySelectedEPC);

            BleMvxApplication._reader.rfid.Options.TagRead.bank = CSLibrary.Constants.MemoryBank.BANK0;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 4;
            BleMvxApplication._reader.rfid.Options.TagRead.count = 1;
            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = Convert.ToUInt32(entrySelectedPWD, 16);

            accessMode = 1;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        void OnNormalWriteConfigWordClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                ShowDialog("Reader is busy now, please try later.");
                return;
            }

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            var password = Convert.ToUInt32(entrySelectedPWD, 16);

            if (password == 00)
            {
                _userDialogs.ShowError("Password can not all zero!!!", 3000);
                return;
            }

            SelectNormalTag(entrySelectedEPC);
            var configWord = GetConfigWord();

            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.BANK0;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 4;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 1;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = new UInt16[1] { configWord };
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = password;

            accessMode = 2;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
        }

        void OnProtectReadConfigWordClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                ShowDialog("Reader is busy now, please try later.");
                return;
            }

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            var password = Convert.ToUInt32(entrySelectedPWD, 16);

            if (password == 00)
            {
                _userDialogs.ShowError("Password can not all zero!!!", 3000);
                return;
            }

            SelectProtectTag(password);

            BleMvxApplication._reader.rfid.Options.TagRead.bank = CSLibrary.Constants.MemoryBank.BANK0;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 4;
            BleMvxApplication._reader.rfid.Options.TagRead.count = 1;
            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = password;

            accessMode = 3;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        void OnProtectWriteConfigWordClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                ShowDialog("Reader is busy now, please try later.");
                return;
            }

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            var password = Convert.ToUInt32(entrySelectedPWD, 16);

            SelectProtectTag(password);
            var configWord = GetConfigWord();

            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.BANK0;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 4;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 1;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = new UInt16[1] { configWord };
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = password;

            accessMode = 4;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
        }

        void OnResumeInvisbleTagtoNormalClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                ShowDialog("Reader is busy now, please try later.");
                return;
            }

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            var password = Convert.ToUInt32(entrySelectedPWD, 16);

            if (password == 00)
            {
                _userDialogs.ShowError("Password can not all zero!!!", 3000);
                return;
            }

            SelectProtectTag(password);
            UInt16 configWord = 0x0000;

            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.BANK0;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 4;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 1;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = new UInt16[1] { configWord };
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = password;

            accessMode = 5;
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

            using (var progress = _userDialogs.Progress(config))
            {
                progress.Show();
                await System.Threading.Tasks.Task.Delay(3000);
            }
        }

    }
}
