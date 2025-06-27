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
    public class ViewModelXerxesReadTemp : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }

        public string _5_3 { get; set; }
        public string _5_4 { get; set; }
        public string _5_5 { get; set; }
        public string _Temp { get; set; }

        public string labelRWTagIDStatus { get; set; } = "";
        public string labelCalibrationStatus { get; set; } = "";
        public string labelSensorCodeStatus { get; set; } = "";
        public string labelRssiCodeStatus { get; set; } = "";
        public string labelTemperatureCodeStatus { get; set; } = "";

        public Boolean switchRWTagIDIsToggled { get; set; } = false;
        public Boolean switchCalibrationIsToggled { get; set; } = false;
        public Boolean switchSensorCodeIsToggled { get; set; } = false;
        public Boolean switchRssiCodeIsToggled { get; set; } = false;
        public Boolean switchTemperatureCodeIsToggled { get; set; } = false;

        public string labelTemperatureText { get; set; }

        public ICommand OnReadButtonCommand { protected set; get; }

        uint _readProcedure = 0;

        int CODE, add_12, add_13, add_14, add_15, delay;
        int _retryCount = 100;


        public ViewModelXerxesReadTemp(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnReadButtonCommand = new Command(OnReadButtonButtonClick);
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

            switchRWTagIDIsToggled = true;
            switchCalibrationIsToggled = true;
            switchSensorCodeIsToggled = true;
            switchRssiCodeIsToggled = true;
            switchTemperatureCodeIsToggled = true;

            RaisePropertyChanged(() => switchRWTagIDIsToggled);
            RaisePropertyChanged(() => switchCalibrationIsToggled);
            RaisePropertyChanged(() => switchSensorCodeIsToggled);
            RaisePropertyChanged(() => switchRssiCodeIsToggled);
            RaisePropertyChanged(() => switchTemperatureCodeIsToggled);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                if (e.access == CSLibrary.Constants.TagAccess.READ)
                {
                    if (e.success)
                    {
                        _retryCount = 100;

                        switch (_readProcedure)
                        {

                            case 0:
                                {
                                    UInt16[] data = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToUshorts();
                                    switch ((data[0] >> 8) & 0x03)
                                    {
                                        case 0:
                                            delay = 3;
                                            break;
                                        case 1:
                                            delay = 24;
                                            break;
                                        case 2:
                                            delay = 96;
                                            break;
                                        default:
                                            delay = 3;
                                            break;
                                    }
                                }
                                break;

                            case 1:
                                {
                                    _5_5 = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                    RaisePropertyChanged(() => _5_5);

                                    UInt16[] data = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToUshorts();
                                    add_12 = data[0];
                                    add_13 = data[1];
                                    add_14 = data[2];
                                    add_15 = data[3];
                                }
                                break;

                            case 2:
                                _5_3 = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                RaisePropertyChanged(() => _5_3);
                                break;

                            case 3:
                                {
                                    _5_4 = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                    RaisePropertyChanged(() => _5_4);

                                    UInt16[] data = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToUshorts();
                                    CODE = data[4];
                                }
                                break;
                        }

                        _readProcedure++;
                        Process();
                    }
                    else
                    {
                        if (--_retryCount > 0)
                            Process();
                        else
                        {
                            if (_readProcedure == 1 || _readProcedure == 0)
                            {
                                _5_5 = "Reading error";
                                RaisePropertyChanged(() => _5_5);
                                _5_3 = "Stoped";
                                RaisePropertyChanged(() => _5_3);
                                _5_4 = "Stopped";
                                RaisePropertyChanged(() => _5_4);
                            }
                            else if (_readProcedure == 2)
                            {
                                _5_3 = "Reading error";
                                RaisePropertyChanged(() => _5_3);
                                _5_4 = "Stopped";
                                RaisePropertyChanged(() => _5_4);
                            }
                            else if (_readProcedure == 3)
                            {
                                _5_4 = "Reading error";
                                RaisePropertyChanged(() => _5_4);
                            }

                            _Temp = "Stopped";
                            RaisePropertyChanged(() => _Temp);
                        }
                    }

                }
            });
        }

        System.Threading.CancellationTokenSource cancelSrc;

        void OnReadButtonButtonClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            _5_3 = "Reading";
            _5_4 = "Reading";
            _5_5 = "Reading";
            _Temp = "Reading";
            _retryCount = 100;

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => _5_3);
            RaisePropertyChanged(() => _5_4);
            RaisePropertyChanged(() => _5_5);
            RaisePropertyChanged(() => _Temp);

            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = Convert.ToUInt32(entrySelectedPWD, 16);

            _readProcedure = 0;
            Process();

            //if (SetParameters())
            //    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
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

        void Process ()
        {
            switch (_readProcedure)
            {
                case 0:
                    BleMvxApplication._reader.rfid.CancelAllSelectCriteria();

                    // EPC
                    BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
                    BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(BleMvxApplication._SELECT_EPC);
                    BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0x00;
                    BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)(BleMvxApplication._SELECT_EPC.Length * 4);
                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
                    ReadSampleCount();
                    break;

                case 1:
                    ConvertingTEMPERATURECODEintoC();
                    break;

                case 2:
                    MeasuringTemperatureUsingSELECT();
                    break;

                case 3:
                    ReadingTemperatureinMASTERMOD();
                    break;

                case 4:
                    _Temp = temp().ToString("#0.0");
                    RaisePropertyChanged(() => _Temp);
                    //ResistorMeasurementsPort1();
                    break;

                case 5:
                    //_Temp = temp().ToString();
                    break;
            }
        }

        void MeasuringTemperatureUsingSELECT()
        {
            CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

            extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.DSLINVB_NOTHING, 0, delay);
            extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0x3e0, 8, new byte[1] { 0x00 });
            BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);

            //extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0, 0);
            //extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.EPC, BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset, BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength, BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.ToBytes());
            //BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);

            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = 0x00000000;
            BleMvxApplication._reader.rfid.Options.TagRead.bank = 0x00;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 0x0e;
            BleMvxApplication._reader.rfid.Options.TagRead.count = 0x01;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        void ReadingTemperatureinMASTERMOD()
        {
            CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

            extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.DSLINVB_NOTHING, 0, delay);
            extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0x3b0, 8, new byte[] { 0x00 });
            BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);

            //extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0, 0);
            //extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.EPC, BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset, BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength, BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.ToBytes());
            //BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);

            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = 0x00000000;
            BleMvxApplication._reader.rfid.Options.TagRead.bank = 0x00;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 0x0a;
            BleMvxApplication._reader.rfid.Options.TagRead.count = 0x05;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        void ResistorMeasurementsPort1()
        {
            CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

            extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0, delay);
            extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0x3c0, 8, new byte[1] { 0x00 });
            BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);

            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = 0x00000000;
            BleMvxApplication._reader.rfid.Options.TagRead.bank = 0x00;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 0x0a;
            BleMvxApplication._reader.rfid.Options.TagRead.count = 0x01;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        } 

        void ResistorMeasurementsPort2()
        {
            CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

            extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0, delay);
            extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0x3f0, 8, new byte[1] { 0x00 });
            BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);

            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = 0x00000000;
            BleMvxApplication._reader.rfid.Options.TagRead.bank = 0x00;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 0x0b;
            BleMvxApplication._reader.rfid.Options.TagRead.count = 0x01;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        void ConvertingTEMPERATURECODEintoC()
        {
            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = 0x00000000;
            BleMvxApplication._reader.rfid.Options.TagRead.bank =  CSLibrary.Constants.MemoryBank.USER;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 0x12;
            BleMvxApplication._reader.rfid.Options.TagRead.count = 0x04;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        void ReadSampleCount()
        {
            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = 0x00000000;
            BleMvxApplication._reader.rfid.Options.TagRead.bank = CSLibrary.Constants.MemoryBank.RESERVED;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 0x5;
            BleMvxApplication._reader.rfid.Options.TagRead.count = 0x01;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        double temp ()
        {
            int FormatCode = (add_15 >> 13) & 0x07;
            int Parity1 = (add_15 >> 12) & 0x01;
            int Parity2 = (add_15 >> 11) & 0x01;
            int Temperature1 = add_15 & 0x07ff;
            int TemperatureCode1 = add_14 & 0xffff;
            int RFU = (add_13 >> 13) & 0x07;
            int Parity3 = (add_13 >> 12) & 0x01;
            int Parity4 = (add_13 >> 11) & 0x01;
            int Temperature2 = add_13 & 0x07ff;
            int TemperatureCode2 = add_12 & 0xffff;

            double CalTemp1 = 0.1 * Temperature1 - 60;
            double CalTemp2 = 0.1 * Temperature2 - 60;
            double CalCode1 = 0.0625 * TemperatureCode1;
            double CalCode2 = 0.0625 * TemperatureCode2;

            double slope = (CalTemp2 - CalTemp1) / (CalCode2 - CalCode1);
            //double TEMP = slope * (CalCode1 - CODE) + CalTemp1;
            double TEMP = slope * (CODE - CalCode1) + CalTemp1;

            return TEMP;
        }


    }
}
