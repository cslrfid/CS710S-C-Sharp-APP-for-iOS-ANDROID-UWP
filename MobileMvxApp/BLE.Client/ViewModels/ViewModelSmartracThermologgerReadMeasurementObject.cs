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
    public class ViewModelSmartracThermologgerReadMeasurementObject : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string entrRecordIndexText { get; set; }
        public string labelReadMeasurementObjectIndexText { get; set; }
        public string labelTemperatureText { get; set; }
        public string labelReadStatusText { get; set; }
        public ICommand OnReadButtonCommand { protected set; get; }

        public ViewModelSmartracThermologgerReadMeasurementObject(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            entrRecordIndexText = "1";
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

                        if (AnswerMessageArea != 0x01)
                        {
                            System.Threading.Thread.Sleep(100);
                            ReadCommunicationBuffer();
                        }
                        else
                        {
                            int ReadMeasurementObjectIndex = CommunicationBuffer[0] & 0xff;
                            int Temperature = CommunicationBuffer[1] >> 8 & 0xff;

                            labelReadMeasurementObjectIndexText = ReadMeasurementObjectIndex.ToString();
                            if ((Temperature & 0x80) == 0x80)
                                labelTemperatureText = ((-64.0 + (double)(Temperature & 0x7f)) * 0.5).ToString();
                            else
                                labelTemperatureText = ((double)(Temperature) * 0.5).ToString();
                            labelReadStatusText = "Read Success";

                            RaisePropertyChanged(() => labelReadMeasurementObjectIndexText);
                            RaisePropertyChanged(() => labelTemperatureText);
                            RaisePropertyChanged(() => labelReadStatusText);
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
            RaisePropertyChanged(() => entrRecordIndexText);
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
            int value = 0x8C00 | (UInt16)((uint.Parse(entrRecordIndexText) & 0x3ff));

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = Convert.ToUInt32(entrySelectedPWD, 16);
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0X104;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new ushort[] { (UInt16)(value) }; // reading System Information
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
