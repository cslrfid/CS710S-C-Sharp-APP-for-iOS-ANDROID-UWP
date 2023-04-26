using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;
using CSLibrary.Structures;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelWriteAnyEPC : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        int numofTags = 0;
        int initValue = 0;
        int sucessCount = 0;
        int failCount = 0;


        public string entryNumberofTagsText { get; set; }
        public string entryInitValueText { get; set; }
        public string entryMaskText { get; set; }
        public bool switchuseHexIsToggled { get; set; }
        public string labelSuccessCount { get; set; }
        public string labelFailCount { get; set; }
        public string buttonStartText { get; set; }
        public ICommand OnWriteButtonCommand { protected set; get; }

        public ViewModelWriteAnyEPC(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnWriteButtonCommand = new Command(OnWriteButtonButtonClick);

            InitReader();
            SetEvent(true);
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

            entryNumberofTagsText = "0";
            entryInitValueText = "0";
            entryMaskText = "";
            switchuseHexIsToggled = false;
            labelSuccessCount = "0";
            labelFailCount = "0";
            buttonStartText = "Start";

            RaisePropertyChanged(() => entryNumberofTagsText);
            RaisePropertyChanged(() => entryInitValueText);
            RaisePropertyChanged(() => entryMaskText);
            RaisePropertyChanged(() => switchuseHexIsToggled);
            RaisePropertyChanged(() => labelSuccessCount);
        }

        private void SetEvent(bool enable)
        {
            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.ClearEventHandler();

            // Key Button event handler
            BleMvxApplication._reader.notification.ClearEventHandler();

            if (enable)
            {
                BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            }
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (buttonStartText == "Start")
                return;

            if (e.success)
            {
                sucessCount++;
                labelSuccessCount = sucessCount.ToString();
                RaisePropertyChanged(() => labelSuccessCount);

                if (sucessCount == numofTags)
                {
                    buttonStartText = "Start";
                    RaisePropertyChanged(() => buttonStartText);

                    return;
                }
            }
            else
            {
                failCount++;
                labelFailCount = failCount.ToString();
                RaisePropertyChanged(() => labelFailCount);
            }

            WriteEPC();
        }

        void SetConfigPower()
        {
            if (BleMvxApplication._reader.rfid.GetAntennaPort() == 1)
            {
                if (BleMvxApplication._config.RFID_PowerSequencing_NumberofPower == 0)
                {
                    BleMvxApplication._reader.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[0]);
                }
                else
                    BleMvxApplication._reader.rfid.SetPowerSequencing(BleMvxApplication._config.RFID_PowerSequencing_NumberofPower, BleMvxApplication._config.RFID_PowerSequencing_Level, BleMvxApplication._config.RFID_PowerSequencing_DWell);
            }
            else
            {
                uint port = BleMvxApplication._reader.rfid.GetAntennaPort();

                for (uint cnt = 0; cnt < port; cnt++)
                {
                    BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[cnt], cnt);
                }
            }
        }

        void InitReader()
        {
            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();

            // Setting 1
            BleMvxApplication._reader.rfid.SetTagDelayTime((uint)BleMvxApplication._config.RFID_TagDelayTime);
            BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell);

            // Set Power
            SetConfigPower();

            // Setting 3
            BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = BleMvxApplication._config.RFID_ToggleTarget ? 1U : 0;
            BleMvxApplication._reader.rfid.SetDynamicQParms(BleMvxApplication._config.RFID_DynamicQParms);

            // Setting 4
            BleMvxApplication._config.RFID_FixedQParms.toggleTarget = BleMvxApplication._config.RFID_ToggleTarget ? 1U : 0;
            BleMvxApplication._reader.rfid.SetFixedQParms(BleMvxApplication._config.RFID_FixedQParms);

            // Setting 2
            BleMvxApplication._reader.rfid.SetOperationMode(BleMvxApplication._config.RFID_OperationMode);
            BleMvxApplication._reader.rfid.SetTagGroup(BleMvxApplication._config.RFID_TagGroup);
            BleMvxApplication._reader.rfid.SetCurrentSingulationAlgorithm(BleMvxApplication._config.RFID_Algorithm);
            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);
        }

        void OnWriteButtonButtonClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            RaisePropertyChanged(() => buttonStartText);

            if (buttonStartText == "Start")
            {
                if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
                {
                    //MessageBox.Show("Reader is busy now, please try later.");
                    return;
                }

                sucessCount = 0;
                failCount = 0;

                labelSuccessCount = "0";
                labelFailCount = "0";
                buttonStartText = "Stop";

                RaisePropertyChanged(() => entryNumberofTagsText);
                RaisePropertyChanged(() => entryInitValueText);
                RaisePropertyChanged(() => entryMaskText);
                RaisePropertyChanged(() => switchuseHexIsToggled);
                RaisePropertyChanged(() => labelSuccessCount);
                RaisePropertyChanged(() => labelFailCount);
                RaisePropertyChanged(() => buttonStartText);

                numofTags = int.Parse(entryNumberofTagsText);
                initValue = int.Parse(entryInitValueText);

                //lock all tag this is not same as our filter
                BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entryMaskText);
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = CSLibrary.Tools.Hex.GetBitCount(entryMaskText);
                BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE | CSLibrary.Constants.SelectMaskFlags.ENABLE_NON_MATCH;
                //BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

                WriteEPC();
            }
            else
            {
                buttonStartText = "Start";
                RaisePropertyChanged(() => buttonStartText);
            }

        }

        void WriteEPC ()
        {
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.accessPassword = 0;
            //BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.offset = 0;
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.count = 6; // word
			BleMvxApplication._reader.rfid.Options.TagWriteEPC.epc = new S_EPC(GetEPCwithPC());
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_EPC);
        }

        private string GetEPCwithPC()
        {
            string newEPC;

            if (switchuseHexIsToggled)
            {
                newEPC = (initValue + sucessCount).ToString("X");
            }
            else
            {
                newEPC = (initValue + sucessCount).ToString("D");
            }

            //newEPC = "3000" + entryMaskText + (new String('0', 24 - entryMaskText.Length - newEPC.Length)) + newEPC;
            newEPC = entryMaskText + (new String('0', 24 - entryMaskText.Length - newEPC.Length)) + newEPC;

            return newEPC;
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
