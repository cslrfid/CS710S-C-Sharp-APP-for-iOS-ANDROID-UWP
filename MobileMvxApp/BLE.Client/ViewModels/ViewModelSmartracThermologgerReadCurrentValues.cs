using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelSmartracThermologgerReadCurrentValues : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string labelCurrentLoggerDateText { get; set; }
        public string labelCurrentLoggerTimeText { get; set; }
        public string labelCurrentLogCycleText { get; set; }
        public string labelCurrentTemperatureText { get; set; }
        public string labelAlternativeTemperatureText { get; set; }
        public string labelBatteryVoltageText { get; set; }
        public string labelErrorEventCounterText { get; set; }
        public string labelReadStatusText { get; set; }
        public ICommand OnReadButtonCommand { protected set; get; }

        public ViewModelSmartracThermologgerReadCurrentValues(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnReadButtonCommand = new Command(OnReadButtonClick);
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

            entrySelectedEPC = BleMvxApplication._SELECT_EPC;
            entrySelectedPWD = "00000000";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                if (e.access == CSLibrary.Constants.TagAccess.WRITE)
                {
                    if (e.success)
                    {
                        System.Threading.Thread.Sleep(100);
                        ReadCommunicationBuffer();
                    }
                    else
                    {
                        labelReadStatusText = "Read Error";
                        RaisePropertyChanged(() => labelReadStatusText);
                    }
                }
                else if (e.access == CSLibrary.Constants.TagAccess.READ)
                {
                    if (e.success)
                    {
                        UInt16[] CommunicationBuffer = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts();
                        var AnswerMessageArea = ((CommunicationBuffer[0] >> 10) & 0x3F);
                        var AnswerIdentifierArea = (CommunicationBuffer[0] & 0x3FF);

                        if (AnswerIdentifierArea == 0x320)
                        {
                            if (AnswerMessageArea != 0x00)
                            {
                                System.Threading.Thread.Sleep(100);
                                ReadCommunicationBuffer();
                            }
                            else
                            {
                                DateTime CurrentLogger = new DateTime(((CommunicationBuffer[1] >> 8) & 0xff) + 2000, CommunicationBuffer[1] & 0xff, (CommunicationBuffer[2] >> 8) & 0xff, CommunicationBuffer[2] & 0xff, (CommunicationBuffer[3] >> 8) & 0xff, CommunicationBuffer[3] & 0xff);
                                int CurrentLogCycle = CommunicationBuffer[4];
                                int CurrentTemperature = CommunicationBuffer[5];
                                int AlternativeTemperature = CommunicationBuffer[6];
                                int BatteryVoltage = CommunicationBuffer[7] >> 4 & 0xfff;
                                int ErrorEventCounter = CommunicationBuffer[7] & 0x0f;

                                labelCurrentLoggerDateText = CurrentLogger.ToString("yy/MM/dd");
                                labelCurrentLoggerTimeText = CurrentLogger.ToString("hh:mm:ss");
                                labelCurrentLogCycleText = CurrentLogCycle.ToString();
                                if (CurrentTemperature == 0x100)
                                    labelCurrentTemperatureText = "invalid measurement";
                                else
                                {
                                    if ((CurrentTemperature & 0x100) == 0x100)
                                        labelCurrentTemperatureText = ((-64.0 + (double)(CurrentTemperature & 0xff)) * 0.25).ToString();
                                    else
                                        labelCurrentTemperatureText = ((double)(CurrentTemperature) * 0.25).ToString();
                                }
                                labelAlternativeTemperatureText = AlternativeTemperature.ToString();
                                labelBatteryVoltageText = BatteryVoltage.ToString() + "mV";
                                if ((ErrorEventCounter & 0x08) == 0x00)
                                    labelErrorEventCounterText = "No Error detected";
                                else
                                    labelErrorEventCounterText = (ErrorEventCounter & 0x07).ToString();
                                labelReadStatusText = "Read Success";

                                RaisePropertyChanged(() => labelCurrentLoggerDateText);
                                RaisePropertyChanged(() => labelCurrentLoggerTimeText);
                                RaisePropertyChanged(() => labelCurrentLogCycleText);
                                RaisePropertyChanged(() => labelCurrentTemperatureText);
                                RaisePropertyChanged(() => labelAlternativeTemperatureText);
                                RaisePropertyChanged(() => labelBatteryVoltageText);
                                RaisePropertyChanged(() => labelErrorEventCounterText);
                                RaisePropertyChanged(() => labelReadStatusText);
                            }
                        }
                    }
                }
            });
        }

        void OnReadButtonClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }
            labelReadStatusText = "Reading...";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => labelReadStatusText);

            labelReadStatusText = "Reading...";
            TagSelected();
            SendReadSystemInformation();
        }

        void TagSelected ()
        {
            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
        }

        // Read
        void SendReadSystemInformation ()
        {
            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = Convert.ToUInt32(entrySelectedPWD, 16);
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0X104;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new ushort[] { 0x8800 | 0x0320 }; // reading System Information
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
		}

        void ReadCommunicationBuffer()
        {
            BleMvxApplication._reader.rfid.Options.TagReadUser.accessPassword = Convert.ToUInt32(entrySelectedPWD, 16);
            BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 0X104;
            BleMvxApplication._reader.rfid.Options.TagReadUser.count = 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
        }

    }
}
