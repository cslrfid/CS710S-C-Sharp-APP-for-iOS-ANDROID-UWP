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
    public class ViewModelSmartracThermologgerClearLogger : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public bool switchClearAllConfigurationDataIsToggled { get; set; }
        public bool switchClearAllMeasurementDataIsToggled { get; set; }
        public bool switchClearAllErrorMemoryIsToggled { get; set; }
        public bool switchStopRealTimeClockIsToggled { get; set; }
        public bool switchResetSystemIsToggled { get; set; }
        public string labelLoggerConditionText { get; set; }
        public string labelReadStatusText { get; set; }
        public ICommand OnReadButtonCommand { protected set; get; }

        public ViewModelSmartracThermologgerClearLogger(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
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

                        if (AnswerIdentifierArea == 0x220)
                        {
                            if (AnswerMessageArea != 0x00)
                            {
                                System.Threading.Thread.Sleep(100);
                                ReadCommunicationBuffer();
                            }
                            else
                            {
                                var LoggerCondition = (CommunicationBuffer[1] >> 8) & 0xff;

                                if (LoggerCondition == 0x00)
                                    labelLoggerConditionText += "Logger is in initial State" + System.Environment.NewLine;
                                else
                                {
                                    if ((LoggerCondition & 0x80) != 00)
                                        labelLoggerConditionText += "Logger Start  Date/Time is set" + System.Environment.NewLine;
                                    if ((LoggerCondition & 0x40) != 00)
                                        labelLoggerConditionText += "Logger Config Date/Time is set" + System.Environment.NewLine;
                                    if ((LoggerCondition & 0x20) != 00)
                                        labelLoggerConditionText += "Logger Interval Time is set" + System.Environment.NewLine;
                                    if ((LoggerCondition & 0x10) != 00)
                                        labelLoggerConditionText += "Logger is active logging" + System.Environment.NewLine;
                                    if ((LoggerCondition & 0x08) != 00)
                                        labelLoggerConditionText += "Logger is waiting for getting active" + System.Environment.NewLine;
                                    if ((LoggerCondition & 0x04) != 00)
                                        labelLoggerConditionText += "Logger is stopped by command" + System.Environment.NewLine;
                                    if ((LoggerCondition & 0x02) != 00)
                                        labelLoggerConditionText += "Logger is stopped by Out of Logger Memory Data" + System.Environment.NewLine;
                                    if ((LoggerCondition & 0x01) != 00)
                                        labelLoggerConditionText += "Logger is stopped by Power Down Reset" + System.Environment.NewLine;
                                }
                                labelReadStatusText = "Read Success";

                                RaisePropertyChanged(() => labelLoggerConditionText);
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
            RaisePropertyChanged(() => switchClearAllConfigurationDataIsToggled);
            RaisePropertyChanged(() => switchClearAllMeasurementDataIsToggled);
            RaisePropertyChanged(() => switchClearAllErrorMemoryIsToggled);
            RaisePropertyChanged(() => switchStopRealTimeClockIsToggled);
            RaisePropertyChanged(() => switchResetSystemIsToggled);
            RaisePropertyChanged(() => labelReadStatusText);

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
            UInt16 para = 0x00;
            if (switchClearAllConfigurationDataIsToggled)
                para |= 0x8000;
            if (switchClearAllMeasurementDataIsToggled)
                para |= 0x4000;
            if (switchClearAllErrorMemoryIsToggled)
                para |= 0x2000;
            if (switchStopRealTimeClockIsToggled)
                para |= 0x1000;
            if (switchResetSystemIsToggled)
                para |= 0x0001;

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = Convert.ToUInt32(entrySelectedPWD, 16);
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0X104;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 2;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new ushort[2] { 0x0000 | 0x0220, para}; // reading System Information
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
