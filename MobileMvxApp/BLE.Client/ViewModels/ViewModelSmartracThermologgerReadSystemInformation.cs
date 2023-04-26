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
    public class ViewModelSmartracThermologgerReadSystemInformation : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string labelNumberOfLoggerObjectText { get; set; }
        public string labelLoggerObjectConfigurationText { get; set; }
        public string labelHardwareVersionText { get; set; }
        public string labelHardwareVersionDateText { get; set; }
        public string labelSoftwareVersionText { get; set; }
        public string labelSoftwareVersionDateText { get; set; }
        public string labelReadStatusText { get; set; }
        public ICommand OnReadButtonCommand { protected set; get; }

        public ViewModelSmartracThermologgerReadSystemInformation(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
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

                        if (AnswerIdentifierArea == 0x340)
                        {
                            if (AnswerMessageArea != 0x00)
                            {
                                System.Threading.Thread.Sleep(100);
                                ReadCommunicationBuffer();
                            }
                            else
                            {
                                string[] LoggerObjectConfigurationOptions = new string [] { "TMP112 Temperature Sensor", "CR2032 Battery Voltage", "EM4325 Temperature Sensor", };
                                var NumberOfLoggerObject = ((CommunicationBuffer[1] >> 8) & 0xfff);
                                var LoggerObjectConfiguration = (CommunicationBuffer[1] & 0xf);
                                Version HardwareVersion = new Version((CommunicationBuffer[2] >> 8) & 0xff, CommunicationBuffer[2] & 0xff, (CommunicationBuffer[3] >> 8) & 0xff);
                                DateTime HardwareVersionDate = new DateTime(CommunicationBuffer[3] & 0xff + 2000, (CommunicationBuffer[4] >> 8) & 0xff, CommunicationBuffer[4] & 0xff);
                                Version SoftwareVersion = new Version((CommunicationBuffer[5] >> 8) & 0xff, CommunicationBuffer[5] & 0xff, (CommunicationBuffer[6] >> 8) & 0xff);
                                DateTime SoftwareVersionDate = new DateTime(CommunicationBuffer[6] & 0xff + 2000, (CommunicationBuffer[7] >> 8) & 0xff, CommunicationBuffer[7] & 0xff);

                                labelNumberOfLoggerObjectText = NumberOfLoggerObject.ToString();
                                switch (LoggerObjectConfiguration)
                                {
                                    case 0x01:
                                        labelLoggerObjectConfigurationText = LoggerObjectConfigurationOptions[0];
                                        break;
                                    case 0x02:
                                        labelLoggerObjectConfigurationText = LoggerObjectConfigurationOptions[1];
                                        break;
                                    case 0x04:
                                        labelLoggerObjectConfigurationText = LoggerObjectConfigurationOptions[2];
                                        break;
                                }
                                labelHardwareVersionText = HardwareVersion.ToString();
                                labelHardwareVersionDateText = HardwareVersionDate.ToString("yy/MM/dd");
                                labelSoftwareVersionText = SoftwareVersion.ToString();
                                labelSoftwareVersionDateText = SoftwareVersionDate.ToString("yy/MM/dd");
                                labelReadStatusText = "Read Success";

                                RaisePropertyChanged(() => labelNumberOfLoggerObjectText);
                                RaisePropertyChanged(() => labelLoggerObjectConfigurationText);
                                RaisePropertyChanged(() => labelHardwareVersionText);
                                RaisePropertyChanged(() => labelHardwareVersionDateText);
                                RaisePropertyChanged(() => labelSoftwareVersionText);
                                RaisePropertyChanged(() => labelSoftwareVersionDateText);
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
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new ushort[] { 0x8800 | 0x0340 }; // reading System Information
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
