using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;

using Prism.Mvvm;

using System.Net.Http;
using System.Net.Http.Headers;

using Plugin.Share;
using Plugin.Share.Abstractions;
using MvvmCross.ViewModels;
using MvvmCross.Navigation;
using BLE.Client.Pages;

namespace BLE.Client.ViewModels
{
    public class ViewModelAxzonInventory : BaseViewModel
	{
        public class RFMicroTagInfoViewModel : BindableBase
        {
            private string _EPC;
            public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }

            private string _NickName;
            public string NickName { get { return this._NickName; } set { this.SetProperty(ref this._NickName, value); } }

            private string _DisplayName;
            public string DisplayName { get { return this._DisplayName; } set { this.SetProperty(ref this._DisplayName, value); } }

            private uint _OCRSSI;
            public uint OCRSSI { get { return this._OCRSSI; } set { this.SetProperty(ref this._OCRSSI, value); } }

            private string _GOODOCRSSI;
            public string GOODOCRSSI { get { return this._GOODOCRSSI; } set { this.SetProperty(ref this._GOODOCRSSI, value); } }

            private string _scanValue;
            public string ScanValue { get { return this._scanValue; } set { this.SetProperty(ref this._scanValue, value); } }

            private string _TemperatureValue;
            public string TemperatureValue { get { return this._TemperatureValue; } set { this.SetProperty(ref this._TemperatureValue, value); } }

            private uint _sucessCount;
            public uint SucessCount { get { return this._sucessCount; } set { this.SetProperty(ref this._sucessCount, value); } }

            private string _RSSIColor;
            public string RSSIColor { get { return this._RSSIColor; } set { this.SetProperty(ref this._RSSIColor, value); } }

            private string _valueColor;
            public string valueColor { get { return this._valueColor; } set { this.SetProperty(ref this._valueColor, value); } }

            public RFMicroTagInfoViewModel()
            {
            }
        }

        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand { protected set; get; }
        public ICommand OnAuthenticationButtonCommand { protected set; get; }
        public ICommand OnConfigurationButtonCommand { protected set; get; }
        public ICommand OnReadTempButtonCommand { protected set; get; }

        //private ObservableCollection<TagInfoViewModel> _TagInfoList = new ObservableCollection<TagInfoViewModel>();
		//public ObservableCollection<TagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }
        private ObservableCollection<RFMicroTagInfoViewModel> _TagInfoList = new ObservableCollection<RFMicroTagInfoViewModel>();
        public ObservableCollection<RFMicroTagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

        public string SensorValueTitle
        {
            get
            {
                // 0 = Average value, 1 = RAW, 2 = Temperature F, 3 = Temperature C, 4 = Dry/Wet
               switch (BleMvxApplication._rfMicro_SensorUnit)
                {
                    case 0:
                        return "RAW";
                    case 1:
                        return "RAW";
                    case 2:
                        return "ºF";
                    case 3:
                        return "ºC";
                    case 4:
                        return "D/W";
                }
                return "Value";
            }
        }

        private System.Collections.Generic.SortedDictionary<string, int> TagInfoListSpeedup = new SortedDictionary<string, int>();

        public bool _InventoryScanning = false;

        private string _startInventoryButtonText = "Start Inventory";
        public string startInventoryButtonText { get { return _startInventoryButtonText; } }

        bool _tagCount = false;

        private string _tagPerSecondText = "0 tags/s     ";
        public string tagPerSecondText { get { return _tagPerSecondText; } }
        private string _numberOfTagsText = "     0 tags";
        public string numberOfTagsText { get { return _numberOfTagsText; } }
		private string _labelVoltage = "";
		public string labelVoltage { get { return _labelVoltage; } }
        public string labelVoltageTextColor { get { return BleMvxApplication._batteryLow ? "Red" : "Black"; } }

        private int _ListViewRowHeight = -1;
		public int ListViewRowHeight { get { return _ListViewRowHeight; } }

        DateTime InventoryStartTime;
        private double _InventoryTime = 0;
        public string InventoryTime { get { return ((uint)_InventoryTime).ToString() + "s"; } }

        private string _currentPower;
        public string currentPower { get { return _currentPower; } set { _currentPower = value; } }

        public string _DebugMessage = "";
        public string DebugMessage { get { return _DebugMessage; } }

        bool _cancelVoltageValue = false;

        bool _waitingRFIDIdle = false;

        // Tag Counter for Inventory Alert
        uint _tagCount4Display = 0;
        uint _tagCount4BeepSound = 0;
        uint _newtagCount4BeepSound = 0;
        uint _newtagCount4Vibration = 0;
        bool _Vibrating = false;
        uint _noNewTag = 0;
        uint _newTagPerSecond = 0;

        #endregion

        public ViewModelAxzonInventory(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);

            OnAuthenticationButtonCommand = new Command(AuthenticationButtonClick);
            OnConfigurationButtonCommand = new Command(ConfigurationButtonClick);
            OnReadTempButtonCommand = new Command(ReadTempButtonClick);

            //InventorySetting();
            //SetPowerString();

            //SetEvent(true);

            Setup();
        }

        ~ViewModelAxzonInventory()
        {
        }

        private void SetEvent (bool enable)
        {
            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.ClearEventHandler();

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

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            Setup();
        }

        public override void ViewDisappearing()
        {
            SetEvent(false);
            base.ViewDisappearing();
        }

        void Setup()
        {
            InventorySetting();
            SetPowerString();
            SetEvent(true);
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
                    _InventoryTime = 0;
                    RaisePropertyChanged(() => InventoryTime);

                    _DebugMessage = "";
                    RaisePropertyChanged(() => DebugMessage);

                    TagInfoList.Clear();
                    TagInfoListSpeedup.Clear();
                    _numberOfTagsText = "     " + _TagInfoList.Count.ToString() + " tags";
                    RaisePropertyChanged(() => numberOfTagsText);

                    _tagCount4Display = 0;
                    _tagPerSecondText = _tagCount4Display.ToString() + " tags/s     ";

                    RaisePropertyChanged(() => tagPerSecondText);
                }
            });
        }

        void InventorySetting()
        {
            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();

            BleMvxApplication._reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;

            // Setting 1
            BleMvxApplication._reader.rfid.SetTagDelayTime((uint)BleMvxApplication._config.RFID_CompactInventoryDelayTime); // for CS108 only
            BleMvxApplication._reader.rfid.SetIntraPacketDelayTime((uint)BleMvxApplication._config.RFID_IntraPacketDelayTime); // for CS710S only
            BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell);
            SetPower(BleMvxApplication._rfMicro_Power);

            // Setting 3
            BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = (BleMvxApplication._rfMicro_Target == 2) ? 1U : 0U;
            BleMvxApplication._reader.rfid.SetDynamicQParms(BleMvxApplication._config.RFID_DynamicQParms);

            // Setting 4
            BleMvxApplication._config.RFID_FixedQParms.toggleTarget = (BleMvxApplication._rfMicro_Target == 2) ? 1U : 0U;
            BleMvxApplication._reader.rfid.SetFixedQParms(BleMvxApplication._config.RFID_FixedQParms);

            // Setting 2
            BleMvxApplication._reader.rfid.SetOperationMode(BleMvxApplication._config.RFID_OperationMode);
            BleMvxApplication._reader.rfid.SetTagGroup(CSLibrary.Constants.Selected.ASSERTED, BleMvxApplication._config.RFID_TagGroup.session, (BleMvxApplication._rfMicro_Target != 1) ? CSLibrary.Constants.SessionTarget.A : CSLibrary.Constants.SessionTarget.B);
            BleMvxApplication._reader.rfid.SetCurrentSingulationAlgorithm(BleMvxApplication._config.RFID_Algorithm);
            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            {
                // Set Filter on Xerxesl Tag
                BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.TID;
                BleMvxApplication._reader.rfid.Options.TagSelected.Mask = new byte[] { 0xE2, 0x82, 0x40, 0x5B };
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 28;
                BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PREFILTER);

                {
                    CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

                    extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.DSLINVB_NOTHING, 0, BleMvxApplication._xerxes_delay);
                    extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0x3b0, 8, new byte[] { 0x00 });
                    BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);
                }
            }

            BleMvxApplication._reader.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;

            // Multi bank inventory
            BleMvxApplication._reader.rfid.Options.TagRanging.multibanks = 2;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.USER;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset1 = 0x12;
            BleMvxApplication._reader.rfid.Options.TagRanging.count1 = 0x04;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank2 = 0x00;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset2 = 0x0a;
            BleMvxApplication._reader.rfid.Options.TagRanging.count2 = 0x05;

            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = false;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);

            //ShowDialog("Configuring RFID");
        }

        void SetPowerString()
        {
            string[] _powerOptions = { "Low (16dBm)", "Mid (23dBm)", "High (30dBm)", "Auto ", "Follow System Setting" };

            if (BleMvxApplication._rfMicro_Power == 3)
                currentPower = "Auto " + _powerOptions[_powerRunning];
            else
                currentPower = _powerOptions[BleMvxApplication._rfMicro_Power];

            RaisePropertyChanged(() => currentPower);
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
                for (uint cnt = BleMvxApplication._reader.rfid.GetAntennaPort() - 1; cnt >= 0; cnt--)
                {
                    BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[cnt], cnt);
                }
            }
        }

        void SetPower(int index)
        {
            switch (index)
            {
                case 0:
                    BleMvxApplication._reader.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader.rfid.SetPowerLevel(160);
                    break;
                case 1:
                    BleMvxApplication._reader.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader.rfid.SetPowerLevel(230);
                    break;
                case 2:
                    BleMvxApplication._reader.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader.rfid.SetPowerLevel(300);
                    break;
                case 3:
                    SetPower(_powerRunning);
                    break;
                case 4:
                    SetConfigPower();
					break;
            }
        }

        int _powerRunning = 0;
        void StartInventory()
        {
            if (BleMvxApplication._reader.BLEBusy)
            {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
                return;
            }

            StartTagCount();
            _InventoryScanning = true;
            _startInventoryButtonText = "Stop Inventory";

            _ListViewRowHeight = 40 + (int)(BleMvxApplication._reader.rfid.Options.TagRanging.multibanks * 10);
            RaisePropertyChanged(() => ListViewRowHeight);

            InventoryStartTime = DateTime.Now;

            _Vibrating = false;
            _noNewTag = 0;
            //if (BleMvxApplication._config.RFID_Vibration && BleMvxApplication._config.RFID_VibrationTag)
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOn(CSLibrary.BarcodeReader.VIBRATORMODE.INVENTORYON, BleMvxApplication._config.RFID_VibrationTime);

            //SetPower(BleMvxApplication._xerxes_Power);
            SetPower(BleMvxApplication._rfMicro_Power);
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);
            _cancelVoltageValue = true;

            RaisePropertyChanged(() => startInventoryButtonText);
        }

        async void StopInventory ()
        {
            BleMvxApplication._reader.rfid.StopOperation();
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();
            _waitingRFIDIdle = true;
            _InventoryScanning = false;
            _tagCount = false;
            _startInventoryButtonText = "Start Inventory";
            RaisePropertyChanged(() => startInventoryButtonText);

            if (_powerRunning >= 2)
                _powerRunning = 0;
            else
                _powerRunning++;
            SetPowerString();
        }

        void StartInventoryClick()
        {
            if (!_InventoryScanning)
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
            _tagCount = true;

            _tagCount4Display = 0;
            _tagCount4BeepSound = 0;
            _newtagCount4BeepSound = 0;
            _newtagCount4Vibration = 0;

            // Create a timer that waits one second, then invokes every second.
            Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
            {
                /*
                if (BleMvxApplication._config.RFID_Vibration && !BleMvxApplication._config.RFID_VibrationTag)
                {
                    if (_newtagCount4Vibration > 0)
                    {
                        _newtagCount4Vibration = 0;
                        _noNewTag = 0;
                        if (!_Vibrating)
                        {
                            _Vibrating = true;
                            BleMvxApplication._reader.barcode.VibratorOn(CSLibrary.BarcodeReader.VIBRATORMODE.INVENTORYON, BleMvxApplication._config.RFID_VibrationTime);
                        }
                    }
                    else
                    {
                        if (_Vibrating)
                        {
                            _noNewTag++;

                            if (_noNewTag > BleMvxApplication._config.RFID_VibrationWindow)
                            { 
                                _Vibrating = false;
                                BleMvxApplication._reader.barcode.VibratorOff();
                            }
                        }
                    }
                }*/

                _InventoryTime = (DateTime.Now - InventoryStartTime).TotalSeconds;
                RaisePropertyChanged(() => InventoryTime);

                _DebugMessage =  CSLibrary.InventoryDebug._inventoryPacketCount.ToString () + " OK, " + CSLibrary.InventoryDebug._inventorySkipPacketCount.ToString() + " Fail";
                RaisePropertyChanged(() => DebugMessage);

                _tagCount4BeepSound = 0;

                //_numberOfTagsText = "  " + newTagPerSecond.ToString() + @"\" + _TagInfoList.Count.ToString() + " tags";
                _numberOfTagsText = "     " + _TagInfoList.Count.ToString() + " tags";
                RaisePropertyChanged(() => numberOfTagsText);

                _tagPerSecondText = _newTagPerSecond.ToString() + @"\" + _tagCount4Display.ToString() + " tags/s     ";
                //_tagPerSecondText = _tagCount4Display.ToString() + " tags/s     ";
                RaisePropertyChanged(() => tagPerSecondText);
                _tagCount4Display = 0;
                _newTagPerSecond = 0;

                if (_tagCount)
                    return true;

                return false;
            });
        }

        void StopInventoryClick()
        {
            BleMvxApplication._reader.rfid.StopOperation();
            _Vibrating = false;
            _waitingRFIDIdle = true;
        }

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING)
                return;

            if (e.info.Bank1Data == null || e.info.Bank1Data.Length != 4) // TID
                return;

            if (e.info.Bank2Data == null || e.info.Bank2Data.Length != 5) // TID
                return;

            if (_waitingRFIDIdle) // ignore display tags
                return;

            InvokeOnMainThread(() =>
            {
                _tagCount4Display++;
                _tagCount4BeepSound++;

                if (_tagCount4BeepSound == 1)
                {
                    if (BleMvxApplication._config.RFID_InventoryAlertSound)
                    {
                        if (_newtagCount4BeepSound > 0)
                            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(3);
                        else
                            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(2);
                        _newtagCount4BeepSound = 0;
                    }
                }
                else if (_tagCount4BeepSound >= 40) // from 5
                    _tagCount4BeepSound = 0;

                AddOrUpdateTagData(e.info);
            });
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                switch (e.state)
                {
                    case CSLibrary.Constants.RFState.IDLE:
                    ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                    _cancelVoltageValue = true;
                    _waitingRFIDIdle = false;
                    switch (BleMvxApplication._reader.rfid.LastMacErrorCode)
                        {
                            case 0x00:  // normal end
                                break;

                            case 0x0309:    // 
                                _userDialogs.Alert("Too near to metal, please move CS108 away from metal and start inventory again.");
                                break;

                            default:
                                _userDialogs.Alert("Mac error : 0x" + BleMvxApplication._reader.rfid.LastMacErrorCode.ToString ("X4"));
                                break;
                        }

                        //InventoryStopped();
                        break;
                }
            });
        }

        private void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info)
        {
            InvokeOnMainThread(() =>
            {
                bool found = false;
                int cnt;

                if (BleMvxApplication._rfMicro_Power == 4 && BleMvxApplication._config.RFID_PowerSequencing_NumberofPower != 0)
                {
                    currentPower = (BleMvxApplication._config.RFID_PowerSequencing_Level[info.antennaPort] / 10).ToString("0.0") + "dB";
                    RaisePropertyChanged(() => currentPower);
                }

                lock (TagInfoList)
                {
                    string epcstr = info.epc.ToString();

                    try
                    {
                        TagInfoListSpeedup.Add(epcstr, TagInfoList.Count);

                        RFMicroTagInfoViewModel item = new RFMicroTagInfoViewModel();

                        item.EPC = info.epc.ToString();
                        item.NickName = GetNickName(item.EPC);
                        if (item.NickName != "")
                            item.DisplayName = item.NickName;
                        else
                            item.DisplayName = item.EPC;
                        item.RSSIColor = "Black";
                        item.valueColor = "Black";
                        item.ScanValue = info.Bank2Data[2].ToString();

                        switch (BleMvxApplication._rfMicro_SensorUnit)
                        {
                            case 0:
                                item.TemperatureValue = info.Bank2Data[4].ToString();
                                break;

                            case 2:
                                item.TemperatureValue = tempF(info.Bank2Data[4], info.Bank1Data[0], info.Bank1Data[1], info.Bank1Data[2], info.Bank1Data[3]).ToString("#0.0");
                                break;

                            case 3:
                                item.TemperatureValue = temp(info.Bank2Data[4], info.Bank1Data[0], info.Bank1Data[1], info.Bank1Data[2], info.Bank1Data[3]).ToString("#0.0");
                                break;
                        }

                        item.OCRSSI = info.Bank2Data[3];

                        if (item.OCRSSI >= BleMvxApplication._rfMicro_minOCRSSI && item.OCRSSI <= BleMvxApplication._rfMicro_maxOCRSSI)
                        {
                            item.RSSIColor = "Black"; 
                            item.SucessCount = 1;
                        }
                        else
                        {
                            item.RSSIColor = "Red";
                            item.SucessCount = 0;
                        }

                        item.EPC = "";
                        item.GOODOCRSSI = "";
                        item.NickName = "";

                        TagInfoList.Insert(0, item);

                        _newtagCount4BeepSound ++;
                        _newtagCount4Vibration ++;
                        _newTagPerSecond ++;

                        Trace.Message("EPC Data = {0}", item.EPC);
                    }
                    catch (Exception ex)
                    {
                        int index;

                        if (TagInfoListSpeedup.TryGetValue(epcstr, out index))
                        {
                            index = TagInfoList.Count - index;
                            index--;

                            TagInfoList[index].ScanValue = info.Bank2Data[2].ToString();

                            switch (BleMvxApplication._rfMicro_SensorUnit)
                            {
                                case 0:
                                    TagInfoList[index].TemperatureValue = info.Bank2Data[4].ToString();
                                    break;

                                case 2:
                                    TagInfoList[index].TemperatureValue = tempF(info.Bank2Data[4], info.Bank1Data[0], info.Bank1Data[1], info.Bank1Data[2], info.Bank1Data[3]).ToString("#0.0");
                                    break;

                                case 3:
                                    TagInfoList[index].TemperatureValue = temp(info.Bank2Data[4], info.Bank1Data[0], info.Bank1Data[1], info.Bank1Data[2], info.Bank1Data[3]).ToString("#0.0");
                                    break;
                            }


                            TagInfoList[index].OCRSSI = info.Bank2Data[3];

                            if (TagInfoList[index].OCRSSI >= BleMvxApplication._rfMicro_minOCRSSI && TagInfoList[index].OCRSSI <= BleMvxApplication._rfMicro_maxOCRSSI)
                            {
                                TagInfoList[index].RSSIColor = "Black";
                                TagInfoList[index].SucessCount++;
                            }
                            else
                            {
                                TagInfoList[index].RSSIColor = "Red";
                            }
                        }
                        else
                        {
                            // error found epc
                        }

                    }
                }
            });
        }

        string GetNickName(string EPC)
        {
            for (int index = 0; index < ViewModelRFMicroNickname._TagNicknameList.Count; index++)
                if (ViewModelRFMicroNickname._TagNicknameList[index].EPC == EPC)
                    return ViewModelRFMicroNickname._TagNicknameList[index].Nickname;

            return "";
        }

        double tempF(int CODE, int add_12, int add_13, int add_14, int add_15)
        {
            double tempC = temp(CODE, add_12, add_13, add_14, add_15);

            return (tempC * 1.8 + 32.0);
        }

        double temp(int CODE, int add_12, int add_13, int add_14, int add_15)
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
            double TEMP = slope * (CODE - CalCode1) + CalTemp1;

            return TEMP;
        }

        void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e)
		{
            InvokeOnMainThread(() =>
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

                    double voltage = (double)e.Voltage / 1000;

                    {
                        var batlow = ClassBattery.BatteryLow(voltage);

                        if (BleMvxApplication._batteryLow && batlow == ClassBattery.BATTERYLEVELSTATUS.NORMAL)
                        {
                            BleMvxApplication._batteryLow = false;
                            RaisePropertyChanged(() => labelVoltageTextColor);
                        }
                        else
                        if (!BleMvxApplication._batteryLow && batlow != ClassBattery.BATTERYLEVELSTATUS.NORMAL)
                        {
                            BleMvxApplication._batteryLow = true;

                            if (batlow == ClassBattery.BATTERYLEVELSTATUS.LOW)
                                _userDialogs.AlertAsync("20% Battery Life Left, Please Recharge RFID Reader or Replace Freshly Charged Battery");

                            RaisePropertyChanged(() => labelVoltageTextColor);
                        }
                    }

                    switch (BleMvxApplication._config.BatteryLevelIndicatorFormat)
                    {
                        case 0:
                            _labelVoltage = "Battery " + voltage.ToString("0.000") + "v"; //			v
                            break;

                        default:
                            _labelVoltage = "Battery " + ClassBattery.Voltage2Percent(voltage).ToString("0") + "%"; //			%
                                                                                                                       //_labelVoltage = ClassBattery.Voltage2Percent((double)e.Voltage / 1000).ToString("0") + "% " + ((double)e.Voltage / 1000).ToString("0.000") + "v"; //			%
                            break;
                    }
                }

                RaisePropertyChanged(() => labelVoltage);
            });
		}

        #region Key_event

        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
            {
                if (e.KeyDown)
                {
                    StartInventory();
                }
                else
                {
                    StopInventory();
                }
            }
        }
        #endregion

        void AuthenticationButtonClick ()
        {
            //ShowViewModel<ViewModelXerxesAuthentication>(new MvxBundle());
            _navigation.Navigate<ViewModelXerxesAuthentication>(new MvxBundle());
        }

        void ConfigurationButtonClick()
        {
            //ShowViewModel<ViewModelXerxesConfiguration>(new MvxBundle());
            _navigation.Navigate<ViewModelXerxesConfiguration>(new MvxBundle());
        }

        void ReadTempButtonClick ()
        {
            //ShowViewModel<ViewModelXerxesReadTemp>(new MvxBundle());
            _navigation.Navigate<ViewModelXerxesReadTemp>(new MvxBundle());
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
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }

        async System.Threading.Tasks.Task<bool> BackupData()
        {
            try
            {
                RESTfulHeader data = new RESTfulHeader();

                data.sequenceNumber = BleMvxApplication._sequenceNumber ++;
                data.rfidReaderName = BleMvxApplication._reader.ReaderName;

                data.rfidReaderSerialNumber = BleMvxApplication._reader.siliconlabIC.GetSerialNumberSync();
                if (data.rfidReaderSerialNumber == null)
                    _userDialogs.Alert("No Serial Number");

                data.rfidReaderInternalSerialNumber = BleMvxApplication._reader.rfid.GetPCBAssemblyCode();
                data.numberOfTags = (UInt16)_TagInfoList.Count;

                /*
                foreach (var tagitem in _TagInfoList)
                {
                    RESTfulSDetail item = new RESTfulSDetail();
                    item.pc = tagitem.PC.ToString("X4");
                    item.epc = tagitem.EPC.ToString();
                    item.timeOfRead = tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss.fff");
                    item.timeZone = tagitem.timeOfRead.ToString("zzz");
                    data.tags.Add(item);
                }
                */

                string JSONdata = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                // Post to server when parameters
                if (BleMvxApplication._config.RFID_SavetoCloud && BleMvxApplication._config.RFID_CloudProtocol == 1)
                {
                    //string rootPath = @"https://www.convergence.com.hk:29090/WebServiceRESTs/1.0/req";
                    //string rootPath = @"https://192.168.25.21:29090/WebServiceRESTs/1.0/req";
                    string fullPath = BleMvxApplication._config.RFID_IPAddress;

                    if (fullPath.Length >= 28 && fullPath.Substring(8, 28) == "democloud.convergence.com.hk")
                        fullPath += @"/create-update-delete/update-entity/tagdata";

                    var uri = new Uri(fullPath + "?" + JSONdata);

                    HttpClient client = new HttpClient();
                    client.MaxResponseContentBufferSize = 102400;

                    HttpResponseMessage response = null;

                    try
                    {
                        response = await client.PostAsync(uri, new StringContent("", System.Text.Encoding.UTF8, "application/json"));
                        if (response.IsSuccessStatusCode)
                        {
                            var a = response.Content;
                            var b = await a.ReadAsStringAsync();
                            _userDialogs.Alert("Success Save to Cloud Server : " + b);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Message(ex.Message);
                    }

                    _userDialogs.Alert("Fail to Save to Cloud Server !!!!!");
                }

                // Post to server when body
                if (BleMvxApplication._config.RFID_SavetoCloud && BleMvxApplication._config.RFID_CloudProtocol == 0)
                {
                    //string rootPath = @"https://www.convergence.com.hk:29090/WebServiceRESTs/1.0/req";
                    //string rootPath = @"https://192.168.25.21:29090/WebServiceRESTs/1.0/req";
                    string fullPath1 = BleMvxApplication._config.RFID_IPAddress;

                    if (fullPath1.Length >= 28 && fullPath1.Substring(8, 28) == "democloud.convergence.com.hk")
                        fullPath1 += @"/create-update-delete/update-entity/tagdata";

                    var uri1 = new Uri(string.Format(fullPath1, string.Empty));
                    var content1 = new StringContent(JSONdata, System.Text.Encoding.UTF8, "application/json");

                    HttpClient client1 = new HttpClient();
                    client1.MaxResponseContentBufferSize = 102400;

                    HttpResponseMessage response1 = null;

                    try
                    {
                        response1 = await client1.PostAsync(uri1, content1);
                        //response = await client.PutAsync(uri, content);
                        if (response1.IsSuccessStatusCode)
                        {
                            var a = response1.Content;
                            var b = await a.ReadAsStringAsync();
                            _userDialogs.Alert("Success Save to Cloud Server : " + b);
                            return true;
                        }
                    }
                    catch (Exception ex1)
                    {
                        Trace.Message(ex1.Message);
                    }

                    _userDialogs.Alert("Fail to Save to Cloud Server !!!!!");
                }

            }
            catch (Exception ex)
            {
                Trace.Message("data fail");
                var a = ex.Message;
            }

            return false;
        }
    }
}
