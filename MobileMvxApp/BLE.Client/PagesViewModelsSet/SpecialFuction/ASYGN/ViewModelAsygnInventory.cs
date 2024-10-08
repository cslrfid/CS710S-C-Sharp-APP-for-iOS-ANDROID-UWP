﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross;


using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;

using Prism.Mvvm;
using MvvmCross.ViewModels;
using BLE.Client.Pages;

namespace BLE.Client.ViewModels
{
    public class ViewModelAsygnInventory : BaseViewModel
	{
        public class TagInfoWithTempViewModel : BindableBase
        {
            private string _EPC;
            public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }
            private string _EPC_ORG;
            public string EPC_ORG { get { return this._EPC_ORG; } set { this.SetProperty(ref this._EPC_ORG, value); } }
            private float _RSSI;
            public float RSSI
            {
                get
                {
                    if (displayFormat == 1)
                        return this._RSSI;

                    if (BleMvxApplication._config.RFID_DBm)
                        return (float)Math.Round(this._RSSI);
                    else
                        return (float)(CSLibrary.Tools.dBConverion.dBm2dBuV(this._RSSI, 0));
                }
                set
                {
                    this.SetProperty(ref this._RSSI, value);
                }
            }
            private float _Temp;
            public float Temp { get { return this._Temp; } set { this.SetProperty(ref this._Temp, value); } }
            private UInt16 _PC;
            public UInt16 PC { get { return this._PC; } set { this.SetProperty(ref this._PC, value); } }

            // Additional for Backend Server
            public DateTime timeOfRead;
            public string locationOfRead;
            public string eCompass;
            public int displayFormat = 0;

            public TagInfoWithTempViewModel()
            {
            }
        }


        private readonly IUserDialogs _userDialogs;

		#region -------------- RFID inventory -----------------

		public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand { protected set; get; }

		private ObservableCollection<TagInfoWithTempViewModel> _TagInfoList = new ObservableCollection<TagInfoWithTempViewModel>();
		public ObservableCollection<TagInfoWithTempViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

		public int tagsCount = 0;
        bool _newTag = false;
        int _tagCountForAlert = 0;
        bool _newTagFound = false;

        public bool _startInventory = true;
        private bool _KeyDown = false;

        public string FilterIndicator { get { return (BleMvxApplication._PREFILTER_Enable | BleMvxApplication._POSTFILTER_MASK_Enable | BleMvxApplication._RSSIFILTER_Type != CSLibrary.Constants.RSSIFILTERTYPE.DISABLE) ? "Filter On" : ""; } }

        private string _startInventoryButtonText = "Start Inventory";
        public string startInventoryButtonText { get { return _startInventoryButtonText; } }

        bool _tagCount = false;

        private string _tagPerSecondText = "0 tags/s";
        public string tagPerSecondText { get { return _tagPerSecondText; } }
        private string _numberOfTagsText = "0 tags";
        public string numberOfTagsText { get { return _numberOfTagsText; } }
		private string _labelVoltage = "";
		public string labelVoltage { get { return _labelVoltage; } }

		private int _ListViewRowHeight = -1;
		public int ListViewRowHeight { get { return _ListViewRowHeight; } set { _ListViewRowHeight = value; } }

        DateTime InventoryStartTime;
        private double _InventoryTime = 0;
        public string InventoryTime { get { return ((uint)_InventoryTime).ToString() + "s"; } }

        private int _DefaultRowHight;

        bool _cancelVoltageValue = false;

        #endregion

        public ViewModelAsygnInventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            RaisePropertyChanged(() => ListViewRowHeight);
            _DefaultRowHight = ListViewRowHeight;

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);

            BleMvxApplication._reader.rfid.SetCountry(BleMvxApplication._config.RFID_Region, (int)BleMvxApplication._config.RFID_FixedChannel);
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            SetEvent(true);
            InventorySetting();
        }

        public override void ViewDisappearing()
        {
            SetEvent(false);
            base.ViewDisappearing();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        private void ClearClick()
        {
            InvokeOnMainThread(() =>
            {
                lock (TagInfoList)
                {
                    TagInfoList.Clear();
                    _numberOfTagsText = _TagInfoList.Count.ToString() + " tags";
                    RaisePropertyChanged(() => numberOfTagsText);

                    tagsCount = 0;
                    _tagPerSecondText = tagsCount.ToString() + " tags/s";
                    RaisePropertyChanged(() => tagPerSecondText);
                }
            });
        }

        private void SetEvent(bool enable)
        {
            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.ClearEventHandler();

            // Cancel Barcode event handler
            BleMvxApplication._reader.barcode.ClearEventHandler();

            // Key Button event handler
            BleMvxApplication._reader.notification.ClearEventHandler();

            if (enable)
            {
                // RFID event handler
                BleMvxApplication._reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
                BleMvxApplication._reader.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);

                // Key Button event handler
                BleMvxApplication._reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
                BleMvxApplication._reader.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);
            }
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
                for (uint cnt = 0; cnt < BleMvxApplication._reader.rfid.GetAntennaPort();  cnt++)
                {
                    BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[cnt], cnt);
                }
            }
        }

        void InventorySetting()
        {
            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
            BleMvxApplication._reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;

            SetConfigPower();
            BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell);
            BleMvxApplication._reader.rfid.SetTagDelayTime((uint)BleMvxApplication._config.RFID_CompactInventoryDelayTime); // for CS108 only
            BleMvxApplication._reader.rfid.SetIntraPacketDelayTime((uint)BleMvxApplication._config.RFID_IntraPacketDelayTime); // for CS710S only
            BleMvxApplication._reader.rfid.SetDuplicateEliminationRollingWindow(BleMvxApplication._config.RFID_DuplicateEliminationRollingWindow);
            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);
            BleMvxApplication._reader.rfid.SetOperationMode(BleMvxApplication._config.RFID_OperationMode);
            BleMvxApplication._reader.rfid.SetTagGroup(BleMvxApplication._config.RFID_TagGroup);

            if (BleMvxApplication._config.RFID_Algorithm == CSLibrary.Constants.SingulationAlgorithm.DYNAMICQ)
            {
                BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = BleMvxApplication._config.RFID_ToggleTarget ? 1U : 0;
                BleMvxApplication._reader.rfid.SetDynamicQParms(BleMvxApplication._config.RFID_DynamicQParms);
            }
            else
            {
                BleMvxApplication._config.RFID_FixedQParms.toggleTarget = BleMvxApplication._config.RFID_ToggleTarget ? 1U : 0;
                BleMvxApplication._reader.rfid.SetFixedQParms(BleMvxApplication._config.RFID_FixedQParms);
            }

            // Select Criteria filter
            {
                BleMvxApplication._reader.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;
                BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.TID;
                BleMvxApplication._reader.rfid.Options.TagSelected.Mask = new byte[] { 0xE2, 0x83, 0xa1 };
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 24;
                BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PREFILTER);
            }

            BleMvxApplication._reader.rfid.SetRSSIdBmFilter(BleMvxApplication._RSSIFILTER_Type, BleMvxApplication._RSSIFILTER_Option, BleMvxApplication._RSSIFILTER_Threshold_dBm);

            BleMvxApplication._reader.rfid.Options.TagRanging.multibanks = 1;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.USER;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset1 = 0;
            BleMvxApplication._reader.rfid.Options.TagRanging.count1 = 7;

            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = false;
            BleMvxApplication._reader.rfid.Options.TagRanging.focus = BleMvxApplication._config.RFID_Focus;
            BleMvxApplication._reader.rfid.Options.TagRanging.fastid = BleMvxApplication._config.RFID_FastId;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);
        }

        void StartInventory()
        {
            if (_startInventory == false)
                return;

            //TagInfoList.Clear();

            StartTagCount();
            //if (BleMvxApplication._config.RFID_OperationMode == CSLibrary.Constants.RadioOperationMode.CONTINUOUS)
            {
                _startInventory = false;
                _startInventoryButtonText = "Stop Inventory";
            }

            _ListViewRowHeight = 40 + (int)(BleMvxApplication._reader.rfid.Options.TagRanging.multibanks * 10);
            RaisePropertyChanged(() => ListViewRowHeight);

            InventoryStartTime = DateTime.Now;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);
            _cancelVoltageValue = true;

            RaisePropertyChanged(() => startInventoryButtonText);
        }

        async void InventoryStopped()
        {
            if (_startInventory)
                return;

            _startInventory = true;
            _startInventoryButtonText = "Start Inventory";

            _tagCount = false;
            RaisePropertyChanged(() => startInventoryButtonText);
        }

        void StopInventory ()
        {
            if (_startInventory)
                return;

            _startInventory = true;
            _startInventoryButtonText = "Start Inventory";

            _tagCount = false;
            BleMvxApplication._reader.rfid.StopOperation();
            RaisePropertyChanged(() => startInventoryButtonText);
        }

        void StartInventoryClick()
        {
            if (_startInventory)
            {
                StartInventory();
            }
            else
            {
                StopInventory();
            }
        }

        void StartTagCount()
        {
            tagsCount = 0;
            _tagCount = true;

            // Create a timer that waits one second, then invokes every second.
            Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
            {
                _InventoryTime = (DateTime.Now - InventoryStartTime).TotalSeconds;
                RaisePropertyChanged(() => InventoryTime);

                _tagCountForAlert = 0;

                _numberOfTagsText = _TagInfoList.Count.ToString() + " tags";
                RaisePropertyChanged(() => numberOfTagsText);

                _tagPerSecondText = tagsCount.ToString() + " tags/s";
                RaisePropertyChanged(() => tagPerSecondText);
                tagsCount = 0;

                if (_tagCount)
                    return true;

                return false;
            });
        }

        // CALIB_TEMP (User5)
        // CALIB_ACQ_TEMP (User6)
        // ACQ_TEMP (User1)
        public float decodeAsygnTemperature(int iUser5, int iUser6, int iUser1)
        {
//            String stringUser5 = string.substring(20, 24); int iUser5 = Integer.valueOf(stringUser5, 16);
//            String stringUser6 = string.substring(24, 28); int iUser6 = Integer.valueOf(stringUser6, 16);
//            String stringUser1 = string.substring(4, 8); int iUser1 = Integer.valueOf(stringUser1, 16);

            switch (iUser1 & 0xC000)
            {
                case 0xc000:
                    iUser1 &= 0x1FFF; iUser1 /= 8;
                    break;

                case 0x8000:
                    iUser1 &= 0xFFF; iUser1 /= 4;
                    break;

                case 0x4000:
                    iUser1 &= 0x7FF; iUser1 /= 2;
                    break;

                default:
                    iUser1 &= 0x3FF;
                    break;
            }

            float temperature = -1;

            if (iUser5 == 3000)
            {
                float calibOffset = (float)3860.27 - (float)iUser6;
                float acqTempCorrected = (float)iUser1 + calibOffset / 8;
                temperature = (float)0.3378 * acqTempCorrected - (float)133;
            }
            else if (iUser5 == 1835)
            {
                float expAcqTemp = (float)398.54 - (float)iUser5 / (float)100;
                expAcqTemp /= (float)0.669162;
                float calibOffset = ((float)8 * expAcqTemp) - (float)iUser6;
                float acqTempCorrected = (float)iUser1 + calibOffset;
                acqTempCorrected /= 8;
                temperature = (float)-0.669162 * acqTempCorrected;
                temperature += (float)398.54;
            }

            return (float)System.Math.Round((double)temperature, 2);
        } //4278





#if javacode
        public float decodeAsygnTemperature(String string)
        {

            String stringUser5 = string.substring(20, 24); int iUser5 = Integer.valueOf(stringUser5, 16);

            String stringUser6 = string.substring(24, 28); int iUser6 = Integer.valueOf(stringUser6, 16);

            String stringUser1 = string.substring(4, 8); int iUser1 = Integer.valueOf(stringUser1, 16);

            switch (iUser1 & 0xC000)
            {

                case 0xc000:

                    iUser1 &= 0x1FFF; iUser1 /= 8;

                    break;

                case 0x8000:

                    iUser1 &= 0xFFF; iUser1 /= 4;

                    break;

                case 0x4000:

                    iUser1 &= 0x7FF; iUser1 /= 2;

                    break;

                default:

                    iUser1 &= 0x3FF;

                    break;

            }

            float temperature = -1;

            appendToLog("input string " + string + ", user1 = " + stringUser1 + ", user5 = " + stringUser5 + ", user6 = " + stringUser6);

            //iUser1 = 495; iUser6 = 3811;

            appendToLog("iUser1 = " + iUser1 + ", iUser5 = " + iUser5 + ", iUser6 = " + iUser6);

            if (iUser5 == 3000)
            {

                float calibOffset = (float)3860.27 - (float)iUser6;

                appendToLog("calibOffset = " + calibOffset);

                float acqTempCorrected = (float)iUser1 + calibOffset / 8;

                appendToLog("acqTempCorrected = " + acqTempCorrected);

                temperature = (float)0.3378 * acqTempCorrected - (float)133;

                appendToLog("temperature = " + temperature);

            }
            else if (iUser5 == 1835)
            {

                float expAcqTemp = (float)398.54 - (float)iUser5 / (float)100;

                appendToLog("expAcqTemp = " + expAcqTemp);

                expAcqTemp /= (float)0.669162;

                appendToLog("expAcqTemp = " + expAcqTemp);

                float calibOffset = ((float)8 * expAcqTemp) - (float)iUser6;

                float acqTempCorrected = (float)iUser1 + calibOffset;

                acqTempCorrected /= 8;

                temperature = (float)-0.669162 * acqTempCorrected;

                temperature += 398.54;

                appendToLog("expAcqTemp = " + expAcqTemp + ". calibOffset = " + calibOffset + ", acqTempCorrected = " + acqTempCorrected + ", temperature = " + temperature);

            }

            return temperature;

        } //4278
#endif


        void StopInventoryClick()
        {
            BleMvxApplication._reader.rfid.StopOperation();
        }

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING)
                return;

            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks >= 1)
            {
                if (e.info.Bank1Data.Length != BleMvxApplication._reader.rfid.Options.TagRanging.count1)
                    return;
            }

            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks >= 2)
            {
                if (e.info.Bank2Data.Length != BleMvxApplication._reader.rfid.Options.TagRanging.count2)
                    return;
            }

            InvokeOnMainThread(() =>
            {
                _tagCountForAlert++;
                if (_tagCountForAlert == 1)
                {
                    if (BleMvxApplication._config.RFID_InventoryAlertSound)
                    {
                        if (_newTagFound)
                            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(3);
                        else
                            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(2);
                        _newTagFound = false;
                    }
                }
                else if (_tagCountForAlert >= 5)
                    _tagCountForAlert = 0;

                AddOrUpdateTagData(e.info);
                tagsCount++;
            });
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            //InvokeOnMainThread(() =>
            //{
            switch (e.state)
            {
                case CSLibrary.Constants.RFState.IDLE:
                    ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                    _cancelVoltageValue = true;
                    if (BleMvxApplication._reader.rfid.GetModelName() == "CS710S")
                    {
                        switch (BleMvxApplication._reader.rfid.LastMacErrorCode)
                        {
                            case 0x00:  // normal end
                                break;

                            default:
                                _userDialogs.Alert("Last error : 0x" + BleMvxApplication._reader.rfid.LastMacErrorCode.ToString("X4"));
                                break;
                        }
                    }
                    else
                    {
                        switch (BleMvxApplication._reader.rfid.LastMacErrorCode)
                        {
                            case 0x00:  // normal end
                                break;

                            case 0x0309:    // 
                                _userDialogs.Alert("Too near to metal, please move CS108 away from metal and start inventory again.");
                                break;

                            default:
                                _userDialogs.Alert("Mac error : 0x" + BleMvxApplication._reader.rfid.LastMacErrorCode.ToString("X4"));
                                break;
                        }
                    }

                    InventoryStopped();
                    break;
            }
            //});
        }

        private void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info)
        {
            if (info.Bank1Data.Length != 7)
                return;

            InvokeOnMainThread(() =>
            {
                bool found = false;

                int cnt;

                lock (TagInfoList)
                {
                    for (cnt = 0; cnt < TagInfoList.Count; cnt++)
                    {
                        if (TagInfoList[cnt].EPC == info.epc.ToString())
                        {
                            TagInfoList[cnt].RSSI = info.rssidBm;
                            TagInfoList[cnt].Temp = decodeAsygnTemperature(info.Bank1Data[5], info.Bank1Data[6], info.Bank1Data[1]);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        TagInfoWithTempViewModel item = new TagInfoWithTempViewModel();

                        item.EPC = info.epc.ToString();
                        item.RSSI = info.rssidBm;
                        item.Temp = decodeAsygnTemperature(info.Bank1Data[5], info.Bank1Data[6], info.Bank1Data[1]);
                        item.PC = info.pc.ToUshorts()[0];

                        TagInfoList.Insert(0, item);

                        _newTagFound = true;

                        Trace.Message("EPC Data = {0}", item.EPC);

                        _newTag = true;
                    }
                }
            });
        }

		void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e)
		{
            if (e.Voltage == 0xffff)
            {
                _labelVoltage = "Battery ERROR"; //			3.98v
            }
            else
            {
                // to fix CS108 voltage bug
                if (_cancelVoltageValue)
                {
                    _cancelVoltageValue = false;
                    return;
                }

                switch (BleMvxApplication._config.BatteryLevelIndicatorFormat)
                {
                    case 0:
                        _labelVoltage = "Battery " + ((double)e.Voltage / 1000).ToString("0.000") + "v"; //			v
                        break;

                    default:
                        _labelVoltage = "Battery " + ClassBattery.Voltage2Percent((double)e.Voltage / 1000).ToString("0") + "%"; //			%
                        break;
                }
            }

			RaisePropertyChanged(() => labelVoltage);
		}

        #region Key_event

        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
            {
                if (e.KeyDown)
                {
                    if (!_KeyDown)
                        StartInventory();
                    _KeyDown = true;
                }
                else
                {
                    if (_KeyDown)
                        StopInventory();
                    _KeyDown = false;
                }
            }
        }
        #endregion

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
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }
    }
}
