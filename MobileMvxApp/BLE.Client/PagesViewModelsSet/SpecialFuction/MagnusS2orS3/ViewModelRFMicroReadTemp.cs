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
    public class ViewModelRFMicroReadTemp : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }

        public string entryUpperRSSILimit { get; set; }
        public string entryLowerRSSILimit { get; set; }
        public string entryRWTagID { get; set; }
        public string entryCalibration { get; set; }
        public string entrySensorCode { get; set; }
        public string entryRssiCode { get; set; }
        public string entryTemperatureCode { get; set; }

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

        public string labelOCRSSIText { get; set; }
        public string labelOCRSSIColor { get; set; }

        public string labelTemperatureText { get; set; }

        public ICommand OnReadButtonCommand { protected set; get; }

        uint readRetryCnt = 0;

        //RFMicro data location
        const int _setOfData = 8;
        CSLibrary.Constants.MemoryBank[] _dataBank = new CSLibrary.Constants.MemoryBank[_setOfData];
        UInt16[] _dataOffset = new ushort[_setOfData];
        UInt16[] _dataCount = new ushort[_setOfData];

        uint _readProcedure = 0;
        uint _tagMode = 3;
        UInt64 _calibrationCode;


        public ViewModelRFMicroReadTemp(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnReadButtonCommand = new Command(OnReadButtonButtonClick);

            // RWTagID
            _dataBank[0] = CSLibrary.Constants.MemoryBank.BANK2; _dataOffset[0] = 0; _dataCount[0] = 2;
            // Calibration
            _dataBank[1] = CSLibrary.Constants.MemoryBank.BANK3; _dataOffset[1] = 8; _dataCount[1] = 4;
            // Sensor Code
            _dataBank[2] = CSLibrary.Constants.MemoryBank.BANK3; _dataOffset[2] = 11; _dataCount[2] = 1;   // for mode 1
            _dataBank[3] = CSLibrary.Constants.MemoryBank.BANK0; _dataOffset[3] = 11; _dataCount[3] = 1;   // for mode 2
            _dataBank[4] = CSLibrary.Constants.MemoryBank.BANK0; _dataOffset[4] = 12; _dataCount[4] = 1;   // for mode 3
            // Rssi Code
            _dataBank[5] = CSLibrary.Constants.MemoryBank.BANK3; _dataOffset[5] = 9; _dataCount[5] = 1;    // for mode 1
            _dataBank[6] = CSLibrary.Constants.MemoryBank.BANK0; _dataOffset[6] = 13; _dataCount[6] = 1;   // for mode 2/3
            //Temperature Code
            _dataBank[7] = CSLibrary.Constants.MemoryBank.BANK0; _dataOffset[7] = 14; _dataCount[7] = 1;
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

            entryUpperRSSILimit = "21";
            entryLowerRSSILimit = "13";

            RaisePropertyChanged(() => entryUpperRSSILimit);
            RaisePropertyChanged(() => entryLowerRSSILimit);
            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            switchRWTagIDIsToggled = true;
            switchCalibrationIsToggled = true;
            switchSensorCodeIsToggled = true;
            switchRssiCodeIsToggled = true;
            switchTemperatureCodeIsToggled = true;

            labelOCRSSIColor = "Black";

            RaisePropertyChanged(() => switchRWTagIDIsToggled);
            RaisePropertyChanged(() => switchCalibrationIsToggled);
            RaisePropertyChanged(() => switchSensorCodeIsToggled);
            RaisePropertyChanged(() => switchRssiCodeIsToggled);
            RaisePropertyChanged(() => switchTemperatureCodeIsToggled);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.READ)
			{
                if (e.bank == CSLibrary.Constants.Bank.SPECIFIC)
                {
                    if (!e.success)
                    {
                        if (--readRetryCnt > 0)
                        {
                            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
                            return;
                        }
                    }

                    switch (_readProcedure)
                    {
                        case 0:
                            if (e.success)
                            {
                                entryRWTagID = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                RaisePropertyChanged(() => entryRWTagID);
                                labelRWTagIDStatus = "Ok";
                                RaisePropertyChanged(() => labelRWTagIDStatus);
                            }
                            else
                            {
                                labelRWTagIDStatus = "Er";
                                RaisePropertyChanged(() => labelRWTagIDStatus);
                            }
                            break;
                        case 1:
                            if (e.success)
                            {
                                entryCalibration = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                RaisePropertyChanged(() => entryCalibration);
                                labelCalibrationStatus = "Ok";
                                RaisePropertyChanged(() => labelCalibrationStatus);
                                {
                                    UInt16 [] cal = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToUshorts();
                                    _calibrationCode = (UInt64)(((UInt64)cal[0] << 48) | ((UInt64)cal[1] << 32) | ((UInt64)cal[2] << 16) | ((UInt64)cal[3]));
                                }
                            }
                            else
                            {
                                labelCalibrationStatus = "Er";
                                RaisePropertyChanged(() => labelCalibrationStatus);
                            }
                            break;
                        case 2:
                            if (e.success)
                            {
                                entrySensorCode = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                RaisePropertyChanged(() => entrySensorCode);
                                labelSensorCodeStatus = "Ok";
                                RaisePropertyChanged(() => labelSensorCodeStatus);
                            }
                            else
                            {
                                labelSensorCodeStatus = "Er";
                                RaisePropertyChanged(() => labelSensorCodeStatus);
                            }
                            break;
                        case 3:
                            if (e.success)
                            {
                                entryRssiCode = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                RaisePropertyChanged(() => entryRssiCode);
                                labelRssiCodeStatus = "Ok";
                                RaisePropertyChanged(() => labelRssiCodeStatus);

                                {
                                    UInt16 rssi = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToUshorts()[0];
                                    labelOCRSSIText = rssi.ToString("D");

                                    try
                                    {
                                        if (rssi < UInt16.Parse(entryLowerRSSILimit) || rssi > UInt16.Parse(entryUpperRSSILimit))
                                            labelOCRSSIColor = "Red";
                                        else
                                            labelOCRSSIColor = "Black";
                                    }
                                    catch (Exception ex)
                                    {
                                        labelOCRSSIColor = "Blue";
                                    }

                                    RaisePropertyChanged(() => labelOCRSSIText);
                                    RaisePropertyChanged(() => labelOCRSSIColor);
                                }
                            }
                            else
                            {
                                labelRssiCodeStatus = "Er";
                                RaisePropertyChanged(() => labelRssiCodeStatus);
                            }
                            break;
                        case 4:
                            if (e.success)
                            {
                                UInt16 [] var = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToUshorts();

                                entryTemperatureCode = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                                RaisePropertyChanged(() => entryTemperatureCode);
                                labelTemperatureCodeStatus = "Ok";
                                RaisePropertyChanged(() => labelTemperatureCodeStatus);
                                RaisePropertyChanged(() => labelTemperatureText);

                                double temperatue = Math.Round(getTemperatue(var[0], _calibrationCode), 2);

                                labelTemperatureText = temperatue.ToString() + " C";
                                RaisePropertyChanged(() => labelTemperatureText);
                            }
                            else
                            {
                                labelTemperatureCodeStatus = "Er";
                                RaisePropertyChanged(() => labelTemperatureCodeStatus);
                            }
                            break;
                    }

                    ReadNextData();
                }
            }
		}

        void ReadNextData ()
        {
            if (_readProcedure >= 5)
            {
                BleMvxApplication._reader.rfid.CancelAllSelectCriteria();                // Confirm cancel all filter
                return;
            }

            _readProcedure++;
            if (SetParameters())
                BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        void StartReadData ()
        {
            _readProcedure = 0;
            if (SetParameters())
                BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        bool SetParameters ()
        {
            switch (_readProcedure)
            {
                case 0: //
                    if (!switchRWTagIDIsToggled)
                    {
                        _readProcedure++;
                        return SetParameters();
                    }
                    BleMvxApplication._reader.rfid.Options.TagRead.bank =  (CSLibrary.Constants.MemoryBank)_dataBank[0];
                    BleMvxApplication._reader.rfid.Options.TagRead.offset = _dataOffset[0];
                    BleMvxApplication._reader.rfid.Options.TagRead.count = _dataCount[0];
                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
                    BleMvxApplication._reader.rfid.SetSelectCriteria(1, null);
                    break;
                case 1:
                    if (!switchCalibrationIsToggled)
                    {
                        _readProcedure++;
                        return SetParameters();
                    }
                    BleMvxApplication._reader.rfid.Options.TagRead.bank = _dataBank[1];
                    BleMvxApplication._reader.rfid.Options.TagRead.offset = _dataOffset[1];
                    BleMvxApplication._reader.rfid.Options.TagRead.count = _dataCount[1];
                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
                    BleMvxApplication._reader.rfid.SetSelectCriteria(1, null);
                    break;
                case 2:
                    if (!switchSensorCodeIsToggled)
                    {
                        _readProcedure++;
                        return SetParameters();
                    }
                    BleMvxApplication._reader.rfid.Options.TagRead.bank = _dataBank[_tagMode + 1];
                    BleMvxApplication._reader.rfid.Options.TagRead.offset = _dataOffset[_tagMode + 1];
                    BleMvxApplication._reader.rfid.Options.TagRead.count = _dataCount[_tagMode + 1];
                    {
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
                        BleMvxApplication._reader.rfid.SetTagGroup(CSLibrary.Constants.Selected.ASSERTED, CSLibrary.Constants.Session.S1, CSLibrary.Constants.SessionTarget.A); // better for read rangage

                        CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

                        extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                        extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xe0, 0, new byte[1]);
                        BleMvxApplication._reader.rfid.SetSelectCriteria(0, extraSlecetion);

                        extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                        extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.EPC, 0x20, 0x60, CSLibrary.Tools.Hex.ToBytes(entrySelectedEPC));
                        BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);
                    }
                    break;
                case 3:
                    if (!switchRssiCodeIsToggled)
                    {
                        _readProcedure++;
                        return SetParameters();
                    }
                    if (_tagMode == 1)
                    {
                        BleMvxApplication._reader.rfid.Options.TagRead.bank = _dataBank[5];
                        BleMvxApplication._reader.rfid.Options.TagRead.offset = _dataOffset[5];
                        BleMvxApplication._reader.rfid.Options.TagRead.count = _dataCount[5];
                    }
                    else
                    {
                        BleMvxApplication._reader.rfid.Options.TagRead.bank = _dataBank[6];
                        BleMvxApplication._reader.rfid.Options.TagRead.offset = _dataOffset[6];
                        BleMvxApplication._reader.rfid.Options.TagRead.count = _dataCount[6];
                    }
                    {
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
                        BleMvxApplication._reader.rfid.SetTagGroup(CSLibrary.Constants.Selected.ASSERTED, CSLibrary.Constants.Session.S1, CSLibrary.Constants.SessionTarget.A); // better for read rangage

                        CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

                        extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                        extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xd0, 8, new byte[] { 0x20 });
                        BleMvxApplication._reader.rfid.SetSelectCriteria(0, extraSlecetion);

                        extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                        extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.EPC, 0x20, 0x60, CSLibrary.Tools.Hex.ToBytes(entrySelectedEPC));

                        BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);
                    }

                    break;
                case 4:
                    if (!switchTemperatureCodeIsToggled)
                    {
                        _readProcedure++;
                        return SetParameters();
                    }
                    BleMvxApplication._reader.rfid.Options.TagRead.bank = _dataBank[7];
                    BleMvxApplication._reader.rfid.Options.TagRead.offset = _dataOffset[7];
                    BleMvxApplication._reader.rfid.Options.TagRead.count = _dataCount[7];

                    {
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
                        BleMvxApplication._reader.rfid.SetTagGroup(CSLibrary.Constants.Selected.ASSERTED, CSLibrary.Constants.Session.S1, CSLibrary.Constants.SessionTarget.A);// better for read rangage

                        CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

                        extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                        extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xe0, 0, new byte[] { 0x00 });
                        BleMvxApplication._reader.rfid.SetSelectCriteria(0, extraSlecetion);

                        extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                        extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.EPC, 0x20, 0x60, CSLibrary.Tools.Hex.ToBytes(entrySelectedEPC));
                        BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);
                    }
                    break;
                default:
                    BleMvxApplication._reader.rfid.CancelAllSelectCriteria();                // Confirm cancel all filter
                    return false;
                    break;
            }

            readRetryCnt = 7;
            return true;
        }

        System.Threading.CancellationTokenSource cancelSrc;

        void OnReadButtonButtonClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

			RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            RaisePropertyChanged(() => switchRWTagIDIsToggled);
            RaisePropertyChanged(() => switchCalibrationIsToggled);
            RaisePropertyChanged(() => switchSensorCodeIsToggled);
            RaisePropertyChanged(() => switchRssiCodeIsToggled);
            RaisePropertyChanged(() => switchTemperatureCodeIsToggled);

            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = Convert.ToUInt32(entrySelectedPWD, 16);


            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(/*m_record.pc.ToString() + */entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            //BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            if (switchRWTagIDIsToggled)
            {
                labelRWTagIDStatus = "R";
                RaisePropertyChanged(() => labelRWTagIDStatus);
            }

            if (switchCalibrationIsToggled)
            {
                labelCalibrationStatus = "R";
                RaisePropertyChanged(() => labelCalibrationStatus);
            }

            if (switchSensorCodeIsToggled)
            {
                labelSensorCodeStatus = "R";
                RaisePropertyChanged(() => labelSensorCodeStatus);
            }

            if (switchRssiCodeIsToggled)
            {
                labelRssiCodeStatus = "R";
                RaisePropertyChanged(() => labelRssiCodeStatus);
            }

            if (switchTemperatureCodeIsToggled)
            {
                labelTemperatureCodeStatus = "R";
                RaisePropertyChanged(() => labelTemperatureCodeStatus);
            }

            _readProcedure = 0;

            if (SetParameters())
                BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
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


        /*
        String getTemperatue(String stringInput)
        {
            byte bValue = Byte.parseByte(stringInput.substring(0, 1), 16);
            byte bValue2 = Byte.parseByte(stringInput.substring(1, 2), 16); bValue2 <<= 4;
            byte bValue3 = Byte.parseByte(stringInput.substring(2, 3), 16); bValue2 |= bValue3;
            String stringValue = "";
            if ((bValue & 0x01) != 0) { stringValue = "-"; bValue2 ^= 0xFF; if (bValue2 != 0xFF) bValue2++; }
            stringValue += String.valueOf((bValue2 & 0xFF) >> 2);
            switch (bValue2 & 0x03)
            {
                case 1:
                    stringValue += ".25";
                    break;
                case 2:
                    stringValue += ".50";
                    break;
                case 3:
                    stringValue += ".75";
                    break;
            }
            return stringValue;
        
            }

    boolean setCalibrationVersion(String strUser) {
        if (strUser == null) return false;
        if (strUser.length() < 16) return false;
        int crc = Integer.parseInt(strUser.substring(0, 4), 16);
        calCode1 = Integer.parseInt(strUser.substring(4, 7), 16);
        calTemp1 = Integer.parseInt(strUser.substring(7, 10), 16); calTemp1 >>= 1;
        calCode2 = Integer.parseInt(strUser.substring(9, 13), 16); calCode2 >>= 1; calCode2 &= 0xFFF;
        calTemp2 = Integer.parseInt(strUser.substring(12, 16), 16); calTemp2 >>= 2; calTemp2 &= 0x7FF;
        calVer = Integer.parseInt(strUser.substring(15, 16),16); calVer &= 0x3;
        MainActivity.mCs108Library4a.appendToLog("crc = " + crc + ", code1 = " + calCode1 + ", temp1 = " + calTemp1 + ", code2 = " + calCode2 + ", temp2 = " + calTemp2 + ", ver = " + calVer);
        textViewCalibrationVersion.setText(strUser); //String.valueOf(calVer)
        return true;
    }

                    if (accessResult.length() >= 4) {
                        float fTemperature = Integer.parseInt(accessResult.substring(0,4), 16);
                        fTemperature = ((float)calTemp2 - (float)calTemp1) * (fTemperature - (float) calCode1);
                        fTemperature /= ((float) (calCode2) - (float)calCode1);
                        fTemperature += (float) calTemp1;
                        fTemperature -= 800;
                        fTemperature /= 10;
                        textViewTemperatureCode.setText(accessResult.substring(0,4) + (calVer != -1 ? ("(" + String.format("%.1f", fTemperature) + (char) 0x00B0 + "C" + ")") : ""));
                    }


        */

        double getTemperatue(UInt16 temp, UInt64 CalCode)
        {
            int crc = (int)(CalCode >> 48) & 0xffff;
            int calCode1 = (int)(CalCode >> 36) & 0x0fff;
            int calTemp1 = (int)(CalCode >> 25) & 0x07ff;
            int calCode2 = (int)(CalCode >> 13) & 0x0fff;
            int calTemp2 = (int)(CalCode >> 2) & 0x7FF;
            int calVer = (int)(CalCode & 0x03);

            double fTemperature = temp;
            fTemperature = ((double)calTemp2 - (double)calTemp1) * (fTemperature - (double)calCode1);
            fTemperature /= ((double)(calCode2) - (double)calCode1);
            fTemperature += (double)calTemp1;
            fTemperature -= 800;
            fTemperature /= 10;
            //textViewTemperatureCode.setText(accessResult.substring(0, 4) + (calVer != -1 ? ("(" + String.format("%.1f", fTemperature) + (char)0x00B0 + "C" + ")") : ""));

            return fTemperature;
        }

        /*
        double getTemperatue_old(UInt16 temp)
        {
            double readableTemp = 0;

            byte bValue = (byte)(temp >> 16);
            byte bValue2 = (byte)(temp >> 8); bValue2 <<= 4;
            byte bValue3 = (byte)(temp); bValue2 |= bValue3;

            if ((bValue & 0x01) != 0)
            {
                double absTemp;

                bValue2 ^= 0xFF;
                if (bValue2 != 0xFF)
                    bValue2++;

                absTemp = bValue2 >> 2;
                absTemp += (bValue2 & 0x03) * 0.25;

                readableTemp = 0 - absTemp;
            }
            else
            {
                readableTemp = bValue2 >> 2;
                readableTemp += (bValue2 & 0x03) * 0.25;
            }

            return readableTemp;
        }
        */


    }
}
