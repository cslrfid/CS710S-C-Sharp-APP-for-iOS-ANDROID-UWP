using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Prism.Mvvm;

//using PCLStorage;

using System.Net.Http;
using System.Net.Http.Headers;

using Plugin.Share;
using Plugin.Share.Abstractions;
using MvvmCross.ViewModels;
using Plugin.Permissions.Abstractions;
using TagDataTranslation;
using CSLibrary.Barcode.Constants;
using System.Text.RegularExpressions;

// GTIN Convert


namespace BLE.Client.ViewModels
{
    //public class ViewModelInventorynScan : BaseViewModel

    public class TagInfoViewModel : BindableBase
    {
        private string _EPC;
        public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }
        private string _Bank1Data;
        public string Bank1Data { get { return this._Bank1Data; } set { this.SetProperty(ref this._Bank1Data, value); } }
        private string _Bank2Data;
        public string Bank2Data { get { return this._Bank2Data; } set { this.SetProperty(ref this._Bank2Data, value); } }
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
        private Int16 _Phase;
        public Int16 Phase { get { return this._Phase; } set { this.SetProperty(ref this._Phase, value); } }
        private string _Channel;
        public string Channel { get { return this._Channel; } set { this.SetProperty(ref this._Channel, value); } }
        private UInt16 _PC;
        public UInt16 PC { get { return this._PC; } set { this.SetProperty(ref this._PC, value); } }

        // Additional for Backend Server
        public DateTime timeOfRead;
        public string locationOfRead;
        public string eCompass;
        public int displayFormat = 0;

        public TagInfoViewModel()
        {
        }
    }

    public class RESTfulSDetail
    {
        public string accessPassword;
        public string killPassword;
        public string pc;
        public string epc;
        public string tidBank;
        public string userBank;
        public string timeOfRead;
        public string timeZone;
        public string locationOfRead;
        public string eCompass;
        public string antennaPort;
    }

    public class RESTfulHeader
    {
        public UInt16 sequenceNumber;
        public UInt16 numberOfTags;
        public List<RESTfulSDetail> tags = new List<RESTfulSDetail>();
        public string userDescription;
        public string rfidReaderName;
        public string rfidReaderSerialNumber;
        public string rfidReaderInternalSerialNumber;
        public string smartPhoneName;
        public string smartPhoneSerialNumber;
        public string smartPhoneBluetoothMACAddress;
        public string smartPhoneWiFiMACAddress;
        public string smartPhoneUUID;
        public string pcName;
        public string pcEthernetMACAddress;
        public string pcWiFiMACAddress;
        public string operatorId;
        public string operatorSiteId;
    }

    public class RESTFULCLASSREADERSTATUS
    {
        public UInt16 sequenceNumber;
        public string rfidReaderName;
        public string rfidReaderModel;
        public string rfidReaderSerialNumber;
        public string rfidReaderInternalSerialNumber;
        public string antennaPortEnabled;
        public string timeOfStatusUpload;
        public string timeZone;
        public string locationOfRead;
        public string Compass;
        public string readerBatteryLevelPercentage;
        public string userDescription;
        public string smartPhoneName;
        public string smartPhoneSerialNumber;
        public string smartPhoneBluetoothMACAddress;
        public string smartPhoneWiFiMACAddress;
        public string smartPhoneUUID;
        public string pcName;
        public string pcEthernetMACAddress;
        public string pcWiFiMACAddress;
        public string readerEthernetMACAddress;
        public string readerWiFiMACAddress;
        public string readerBluetoothMACAddress;
        public string readerActiveAPI;
        public string readerActiveEventID;
        public string readerUpTime;
        public string readerHealth;
        public string operatorId;
        public string operatorSiteId;
        public string operatorSiteDescription;
    }



    public class ViewModelInventorynScan : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        readonly IPermissions _permissions;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand { protected set; get; }
        public ICommand OnHEXButtonCommand { protected set; get; }
        public ICommand OnUPCButtonommand { protected set; get; }
        public ICommand OnSGTINButtonCommand { protected set; get; }

        private ObservableCollection<TagInfoViewModel> _TagInfoList = new ObservableCollection<TagInfoViewModel>();
        public ObservableCollection<TagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

        private System.Collections.Generic.SortedDictionary<string, (int index, string URI)> TagInfoListSpeedup = new SortedDictionary<string, (int, string)>();
        private System.Collections.Generic.SortedDictionary<string, int> TagInfoListSpeedup1 = new SortedDictionary<string, int>();

        public bool _InventoryScanning = false;
        public bool _KeyDown = false;

        public string FilterIndicator { get { return (BleMvxApplication._PREFILTER_Enable | BleMvxApplication._POSTFILTER_MASK_Enable | BleMvxApplication._RSSIFILTER_Type != CSLibrary.Constants.RSSIFILTERTYPE.DISABLE) ? "Filter On" : ""; } }

        private string _startInventoryButtonText = "Start Inventory";
        public string startInventoryButtonText { get { return _startInventoryButtonText; } }

        bool _tagCount = false;

        private string _tagPerSecondText = "0/0/0 internal/new/tags/s     ";
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

        public string _DebugMessage = "";
        public string DebugMessage { get { return _DebugMessage; } }

        string _EPCHeaderText;
        public string EPCHeaderText { get { return _EPCHeaderText; } }
        string _RSSIHeaderText;
        public string RSSIHeaderText { get { return _RSSIHeaderText; } }

        bool _cancelVoltageValue = false;

        //bool _waitingRFIDIdle = false;

        // Tag Counter for Inventory Alert
        uint _tagCount4Display = 0;
        uint _tagCount4BeepSound = 0;
        uint _newtagCount4BeepSound = 0;
        uint _newtagCount4Vibration = 0;
        bool _Vibrating = false;
        uint _noNewTag = 0;
        uint _newTagPerSecond = 0;

        int _displayFormat = 0; // Display format, 0=HEX, 1=UPC, 2=GTIN

        #endregion

        public ViewModelInventorynScan(IAdapter adapter, IUserDialogs userDialogs, IPermissions permissions) : base(adapter)
        {
            _userDialogs = userDialogs;
            _permissions = permissions;

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);

            OnStartBarcodeScanButtonCommand = new Command(StartBarcodeScanButtonClick);
            OnClearBarcodeDataButtonCommand = new Command(ClearBarcodeDataButtonClick);
            OnSendDataCommand = new Command(SendDataButtonClick);
            OnShareDataCommand = new Command(ShareDataButtonClick);
            OnSaveDataCommand = new Command(SaveDataButtonClick);
            OnHEXButtonCommand = new Command(HEXButtonClick);
            OnUPCButtonommand = new Command(UPCButtonClick);
            OnSGTINButtonCommand = new Command(SGTINButtonClick);

            _EPCHeaderText = "EPC";
            _RSSIHeaderText = "RSSI";

            //SetEvent(true);

            InventorySetting();
        }

        ~ViewModelInventorynScan()
        {
            //BleMvxApplication._reader.barcode.Stop();
            //_barcodeScanning = false;
            //SetEvent(false);
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
                BleMvxApplication._reader.rfid.OnInventoryTagRateCallback += new EventHandler<CSLibrary.Events.OnInventoryTagRateCallbackEventArgs>(InventoryTagRateCallback);

                // Barcode event handler
                BleMvxApplication._reader.barcode.OnCapturedNotify += new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Linkage_CaptureCompleted);

                // Key Button event handler
                BleMvxApplication._reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
                BleMvxApplication._reader.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);
            }
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            SetEvent(true);

            try
            {
                Page currentPage;

                currentPage = ((TabbedPage)Application.Current.MainPage.Navigation.NavigationStack[1]).CurrentPage;

                if (currentPage.Title == "Barcode Scan")
                {
                    BleMvxApplication._reader.barcode.FastBarcodeMode(true);
                }
                else
                {
                    BleMvxApplication._reader.barcode.FastBarcodeMode(false);
                }
            }
            catch (Exception ex)
            {
            }

        }

        public override void ViewDisappearing()
        {
            _barcodeScanning = false;
            _InventoryScanning = false;
            StopInventory();
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();

            //SetEvent(false);

            try
            {
                Page currentPage;

                currentPage = ((TabbedPage)Application.Current.MainPage.Navigation.NavigationStack[1]).CurrentPage;

                //if (currentPage.Title != "Barcode Scan")
                {
                    BleMvxApplication._reader.barcode.FastBarcodeMode(false);
                }
            }
            catch (Exception ex)
            {
            }


            // don't turn off event handler is you need program work in sleep mode.
            //SetEvent(false);
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
                    ClearClickImmediately();
                }
            });
        }

        private void ClearClickImmediately()
        {
            _InventoryTime = 0;
            RaisePropertyChanged(() => InventoryTime);

            _DebugMessage = "";
            RaisePropertyChanged(() => DebugMessage);

            TagInfoList.Clear();
            TagInfoListSpeedup.Clear();
            TagInfoListSpeedup1.Clear();
            _numberOfTagsText = "     " + _TagInfoList.Count.ToString() + " tags";
            RaisePropertyChanged(() => numberOfTagsText);

            _tagCount4Display = 0;
            _tagPerSecondText = "0/" + _newTagPerSecond.ToString() + "/" + _tagCount4Display.ToString() + " internal/new/tags/s     ";

            RaisePropertyChanged(() => tagPerSecondText);
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

        void InventorySetting()
        {
            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
            BleMvxApplication._reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;

            BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell);
            BleMvxApplication._reader.rfid.SetTagDelayTime((uint)BleMvxApplication._config.RFID_CompactInventoryDelayTime); // for CS108 only
            BleMvxApplication._reader.rfid.SetIntraPacketDelayTime((uint)BleMvxApplication._config.RFID_IntraPacketDelayTime); // for CS710S only
            BleMvxApplication._reader.rfid.SetDuplicateEliminationRollingWindow(BleMvxApplication._config.RFID_DuplicateEliminationRollingWindow); // for CS710S only
            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);
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
            if (BleMvxApplication._PREFILTER_Enable)
            {
                BleMvxApplication._reader.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;
                BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                if (BleMvxApplication._PREFILTER_Bank == 1)
                {
                    BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
                    BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(BleMvxApplication._PREFILTER_MASK_EPC);
                    BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = BleMvxApplication._PREFILTER_MASK_Offset;
                    BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)(BleMvxApplication._PREFILTER_MASK_EPC.Length) * 4;
                }
                else
                {
                    BleMvxApplication._reader.rfid.Options.TagSelected.bank = (CSLibrary.Constants.MemoryBank)(BleMvxApplication._PREFILTER_Bank);
                    BleMvxApplication._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.Hex.ToBytes(BleMvxApplication._PREFILTER_MASK_EPC);
                    BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = BleMvxApplication._PREFILTER_MASK_Offset;
                    BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = (uint)(BleMvxApplication._PREFILTER_MASK_EPC.Length) * 4;
                }
                BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PREFILTER);
                BleMvxApplication._reader.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;
            }

            BleMvxApplication._reader.rfid.SetRSSIdBmFilter(BleMvxApplication._RSSIFILTER_Type, BleMvxApplication._RSSIFILTER_Option, BleMvxApplication._RSSIFILTER_Threshold_dBm);

            BleMvxApplication._reader.rfid.Options.TagRanging.multibanks = 0;
            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = true;
            BleMvxApplication._reader.rfid.Options.TagRanging.focus = BleMvxApplication._config.RFID_Focus;
            BleMvxApplication._reader.rfid.Options.TagRanging.fastid = BleMvxApplication._config.RFID_FastId;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);

            // Set Power setting and clone antenna 0 setting to other antennas
            // the command MUST in last inventory setting if use power sequencing
            SetConfigPower();
        }

        void StartInventory()
        {
            if (_InventoryScanning)
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
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOn(CSLibrary.BarcodeReader.VIBRATORMODE.INVENTORYON, BleMvxApplication._config.RFID_VibrationTime);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_RANGING);
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);
            _cancelVoltageValue = true;

            RaisePropertyChanged(() => startInventoryButtonText);
        }

        async void InventoryStopped()
        {
            if (!_InventoryScanning)
                return;

            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();
            //_waitingRFIDIdle = true;
            _InventoryScanning = false;
            _tagCount = false;
            _startInventoryButtonText = "Start Inventory";
            RaisePropertyChanged(() => startInventoryButtonText);
        }

        async void StopInventory ()
        {
            if (!_InventoryScanning)
                return;

            BleMvxApplication._reader.rfid.StopOperation();
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();
            //_waitingRFIDIdle = true;
            _InventoryScanning = false;
            _tagCount = false;
            _startInventoryButtonText = "Start Inventory";
            RaisePropertyChanged(() => startInventoryButtonText);
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
            //_tagCount4Vibration = 0;
            _newtagCount4Vibration = 0;

            // Create a timer that waits one second, then invokes every second.
            Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
            {
/*                if (BleMvxApplication._config.RFID_Vibration && !BleMvxApplication._config.RFID_VibrationTag)
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

                _tagPerSecondText = _readerInventoryTagRate.ToString() + "/" + _newTagPerSecond.ToString() + "/" + _tagCount4Display.ToString() + " internal/new/tags/s     ";
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
            //_waitingRFIDIdle = true;
        }

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING)
                return;

            //if (_waitingRFIDIdle) // ignore display tags
            //    return;

            //InvokeOnMainThread(() =>
            //{
                _tagCount4Display++;
                _tagCount4BeepSound++;

                if (_tagCount4BeepSound == 1)
                {
                    if (BleMvxApplication._config.RFID_InventoryAlertSound)
                    {
                        InvokeOnMainThread(() =>
                        {
                        if (_newtagCount4BeepSound > 0)
                                Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(3);
                            else
                                Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(2);
                        });
                        _newtagCount4BeepSound = 0;
                    }
                }
                else if (_tagCount4BeepSound >= 40) // from 5
                    _tagCount4BeepSound = 0;


                switch (_displayFormat)
                {
                    case 1:
                        AddOrUpdateTagDataUPC(e.info);
                        break;

                    case 2:
                        AddOrUpdateTagDataGTIN(e.info);
                        break;

                    default:
                        AddOrUpdateTagData(e.info);
                        break;
                }
            //});
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
                        //_waitingRFIDIdle = false;

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
            });
        }

        uint _readerInventoryTagRate = 0;
        void InventoryTagRateCallback(object sender, CSLibrary.Events.OnInventoryTagRateCallbackEventArgs e)
        {
            _readerInventoryTagRate = e.inventoryTagRate;
        }

        private void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info)
        {
            InvokeOnMainThread(() =>
            {
                lock (TagInfoList)
                {
                    if (_displayFormat != 0)
                        return;

#if not_binarysearch
                    for (cnt = 0; cnt < TagInfoList.Count; cnt++)
                    {
                        if (TagInfoList[cnt].EPC == info.epc.ToString())
                        {
                            TagInfoList[cnt].Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);
                            TagInfoList[cnt].Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);
                            TagInfoList[cnt].RSSI = info.rssi;
                            //TagInfoList[cnt].Phase = info.phase;
                            //TagInfoList[cnt].Channel = (byte)info.freqChannel;

                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        TagInfoViewModel item = new TagInfoViewModel();

                        item.timeOfRead = DateTime.Now;
                        item.EPC = info.epc.ToString();
                        item.Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);
                        item.Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);
                        item.RSSI = info.rssi;
                        //item.Phase = info.phase;
                        //item.Channel = (byte)info.freqChannel;
                        item.PC = info.pc.ToUshorts()[0];

                        //TagInfoList.Add(item);
                        TagInfoList.Insert(0, item);

                        _newTagFound = true;

                        Trace.Message("EPC Data = {0}", item.EPC);

                        _newTag = true;
                    }
#else
                    string epcstr = info.epc.ToString();

                    try
                    {
                        TagInfoListSpeedup.Add(epcstr, (TagInfoList.Count, null));

                        TagInfoViewModel item = new TagInfoViewModel();

                        item.timeOfRead = DateTime.Now;
                        item.EPC = info.epc.ToString();
                        item.Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);
                        item.Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);
                        item.RSSI = info.rssidBm;
                        //item.Phase = info.phase;
                        //item.Channel = (byte)info.freqChannel;
                        item.PC = info.pc.ToUshorts()[0];

                        //TagInfoList.Add(item);
                        if (BleMvxApplication._config.RFID_NewTagLocation)
                            TagInfoList.Insert(0, item);
                        else
                            TagInfoList.Add(item);

                        _newtagCount4BeepSound++;
                        _newtagCount4Vibration++;
                        _newTagPerSecond++;

                        Trace.Message("EPC Data = {0}", item.EPC);

                        //_newTag = true;
                    }
                    catch (Exception ex)
                    {
                        if (TagInfoListSpeedup.TryGetValue(epcstr, out var values))
                        {
                            if (BleMvxApplication._config.RFID_NewTagLocation)
                            {
                                values.index = TagInfoList.Count - values.index;
                                values.index--;
                            }

                            TagInfoList[values.index].Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);
                            TagInfoList[values.index].Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);
                            TagInfoList[values.index].RSSI = info.rssidBm;
                        }
                        else
                        {
                            // error found epc
                        }

                    }
#endif
                }
            });
        }

        private void AddOrUpdateTagDataUPC(CSLibrary.Structures.TagCallbackInfo info)
        {
            string epcstr = info.epc.ToString();

            if (epcstr.Substring(0, 2) != "30")
                return;

            InvokeOnMainThread(() =>
            {
                if (_displayFormat != 1)
                    return;

                lock (TagInfoList)
                {
                    try
                    {
                        string URI = null;
                        string PureURI;
                        string[] PureURIItems;
                        string UPC;
                        string Serial;

                        if (TagInfoListSpeedup.TryGetValue(epcstr, out var values))
                        {
                            return;
                        }
                        else
                        {
                            try
                            {
                                TDTEngine engine = new TDTEngine();
                                string epcIdentifier = engine.HexToBinary(epcstr);
                                string parameterList = @"tagLength=96";
                                URI = engine.Translate(epcIdentifier, parameterList, @"PURE_IDENTITY");
                                PureURI = URI.Split(':')[4];
                                PureURIItems = PureURI.Split('.');
                                UPC = PureURIItems[0] + PureURIItems[1];
                                Serial = PureURIItems[2];
                            }
                            catch (Exception ex)
                            {
                                TagInfoListSpeedup.Add(epcstr, (TagInfoList.Count, null));
                                return;
                            }

                            TagInfoListSpeedup.Add(epcstr, (TagInfoList.Count, URI));
                        }

                        if (TagInfoListSpeedup1.TryGetValue(UPC, out int index))
                        {
                            if (BleMvxApplication._config.RFID_NewTagLocation)
                            {
                                index = TagInfoList.Count - index;
                                index--;
                            }

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.fastid)
                                TagInfoList[index].Bank1Data = CSLibrary.Tools.Hex.ToString(info.FastTid);

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks > 0)
                                TagInfoList[index].Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks > 1)
                                TagInfoList[index].Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);

                            TagInfoList[index].RSSI++;
                        }
                        else
                        {
                            TagInfoListSpeedup1.Add(UPC, TagInfoList.Count);

                            TagInfoViewModel item = new TagInfoViewModel();

                            item.timeOfRead = DateTime.Now;
                            item.EPC = UPC;

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.fastid)
                                item.Bank1Data = CSLibrary.Tools.Hex.ToString(info.FastTid);

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks > 0)
                                item.Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks > 1)
                                item.Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);

                            item.displayFormat = 1;
                            item.RSSI = 1;
                            item.PC = info.pc.ToUshorts()[0];

                            if (BleMvxApplication._config.RFID_NewTagLocation)
                                TagInfoList.Insert(0, item);
                            else
                                TagInfoList.Add(item);

                            _newtagCount4BeepSound++;
                            _newtagCount4Vibration++;
                            _newTagPerSecond++;

                            Trace.Message("EPC Data = {0}", item.EPC);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
        }

        private void AddOrUpdateTagDataGTIN(CSLibrary.Structures.TagCallbackInfo info)
        {
            string epcstr = info.epc.ToString();

            if (epcstr.Substring(0, 2) != "30")
                return;

            // EPC display convertion

            InvokeOnMainThread(() =>
            {
                if (_displayFormat != 2)
                    return;

                lock (TagInfoList)
                {
                    try
                    {
                        string URI = null;
                        string TagURI = "";
                        if (TagInfoListSpeedup.TryGetValue(epcstr, out var values))
                        {
                            if (values.URI == null)
                                return;

                            URI = values.URI;
                            TagURI = URI.Split(':')[4];
                        }
                        else
                        {
                            try
                            {
                                TDTEngine engine = new TDTEngine();
                                string epcIdentifier = engine.HexToBinary(epcstr);
                                string parameterList = @"tagLength=96";
                                URI = engine.Translate(epcIdentifier, parameterList, @"TAG_ENCODING");
                                TagURI = URI.Split(':')[4];
                            }
                            catch (Exception ex)
                            {
                                TagInfoListSpeedup.Add(epcstr, (TagInfoList.Count, null));
                                return;
                            }

                            TagInfoListSpeedup.Add(epcstr, (TagInfoList.Count, URI));
                        }

                        if (TagInfoListSpeedup1.TryGetValue(TagURI, out int index))
                        {
                            if (BleMvxApplication._config.RFID_NewTagLocation)
                            {
                                index = TagInfoList.Count - index;
                                index--;
                            }

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.fastid)
                                TagInfoList[index].Bank1Data = CSLibrary.Tools.Hex.ToString(info.FastTid);

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks > 0)
                                TagInfoList[index].Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks > 1)
                                TagInfoList[index].Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);

                            TagInfoList[index].RSSI = info.rssidBm;
                        }
                        else
                        {
                            TagInfoListSpeedup1.Add(TagURI, TagInfoList.Count);

                            TagInfoViewModel item = new TagInfoViewModel();

                            item.timeOfRead = DateTime.Now;
                            item.EPC = TagURI;

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.fastid)
                                item.Bank1Data = CSLibrary.Tools.Hex.ToString(info.FastTid);

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks > 0)
                                item.Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks > 1)
                                item.Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);

                            item.RSSI = info.rssidBm;
                            item.PC = info.pc.ToUshorts()[0];

                            if (BleMvxApplication._config.RFID_NewTagLocation)
                                TagInfoList.Insert(0, item);
                            else
                                TagInfoList.Add(item);

                            _newtagCount4BeepSound++;
                            _newtagCount4Vibration++;
                            _newTagPerSecond++;

                            Trace.Message("EPC Data = {0}", item.EPC);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
        }

        void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e)
		{
            InvokeOnMainThread(() =>
            {
                if (e.Voltage == 0xffff)
                {
                    _labelVoltage = "Battery ERROR";
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
                            //else if (batlow == ClassBattery.BATTERYLEVELSTATUS.LOW_17)
                            //    _userDialogs.AlertAsync("8% Battery Life Left, Please Recharge RFID Reader or Replace with Freshly Charged Battery");

                            RaisePropertyChanged(() => labelVoltageTextColor);
                        }
                    }

                    switch (BleMvxApplication._config.BatteryLevelIndicatorFormat)
                    {
                        case 0:
                            _labelVoltage = " //\t\t\t3.98v " + voltage.ToString("0.000") + "v"; //			v
                            break;

                        default:
                            _labelVoltage = voltage.ToString("0.000") + "v " + ClassBattery.Voltage2Percent(voltage).ToString("0") + "%"; //			%
                                                                                                                       //_labelVoltage = ClassBattery.Voltage2Percent((double)e.Voltage / 1000).ToString("0") + "% " + ((double)e.Voltage / 1000).ToString("0.000") + "v"; //			%
                            break;
                    }
                }


#if nouse
                {
                        double p = ClassBattery.Voltage2Percent((double)e.Voltage / 1000);
                        string a;

                        a = "Bat. " + p.ToString("0.0") + "%  " + ((double)e.Voltage / 1000).ToString("0.000") + "v"; //			3.98v

                        if (p == 100)
                            a += "  over 90min";
                        else if (p == 0)
                            a += "  under 1min";
                        else
                            a += "  " + (0.9 * p).ToString ("0.0") + "min"; 

                        _labelVoltage = a;
                    }
                    else
                        _labelVoltage = "Bat. " + ((double)e.Voltage / 1000).ToString("0.000") + "v"; //			3.98v
#endif

                RaisePropertyChanged(() => labelVoltage);
            });
		}

#region -------------------- Barcode Scan -------------------

		public ICommand OnStartBarcodeScanButtonCommand { protected set; get; }
        public ICommand OnClearBarcodeDataButtonCommand { protected set; get; }
        public ICommand OnSendDataCommand { protected set; get; }
        public ICommand OnShareDataCommand { protected set; get; }
        public ICommand OnSaveDataCommand { protected set; get; }

        private string _startBarcodeScanButtonText = "Start Scan";
        public string startBarcodeScanButtonText { get { return _startBarcodeScanButtonText; } }

        public class BARCODEInfoViewModel : BindableBase
        {
            private string _code;
            public string code { get { return this._code; } set { this.SetProperty(ref this._code, value); } }
            private uint _count;
            public uint count { get { return this._count; } set { this.SetProperty(ref this._count, value); } }
        }

        public ObservableCollection<BARCODEInfoViewModel> barcodeData { get; set; } = new ObservableCollection<BARCODEInfoViewModel>();

        bool _barcodeScanning = false;

        void StartBarcodeScanButtonClick()
        {
            if (_barcodeScanning)
            {
                BarcodeStop();
            }
            else
            {
                if (BleMvxApplication._reader.BLEBusy)
                {
                    _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
                }
                else
                {
                    BarcodeStart();
                }
            }
        }

        private void ClearBarcodeDataButtonClick()
        {
            barcodeData.Clear();
        }

        private void SendDataButtonClick ()
        {
            var result = BackupData();

            CSLibrary.Debug.WriteLine("BackupData : {0}", result.ToString());
        }

/*        private void ShareDataButtonClick(object ind)
        {
            if (ind == null || (int)ind != 1)
                return;

            var result = ShareData();
            CSLibrary.Debug.WriteLine("Share Data : {0}", result.ToString());
        }
*/
        private void ShareDataButtonClick()
        {
            var result = ShareData();
            CSLibrary.Debug.WriteLine("Share Data : {0}", result.ToString());
        }
        private void SaveDataButtonClick()
        {
            var result = SaveData();
            CSLibrary.Debug.WriteLine("Save Data : {0}", result.ToString());
        }

        private void HEXButtonClick()
        {
            _displayFormat = 0;
            _EPCHeaderText = "EPC";
            _RSSIHeaderText = "RSSI";
            RaisePropertyChanged(() => EPCHeaderText);
            RaisePropertyChanged(() => RSSIHeaderText);
            ClearClick();
        }

        private void UPCButtonClick()
        {
            _displayFormat = 1;
            _EPCHeaderText = "UPC";
            _RSSIHeaderText = "Unit Count";
            RaisePropertyChanged(() => EPCHeaderText);
            RaisePropertyChanged(() => RSSIHeaderText);
            ClearClick();
        }

        private void SGTINButtonClick()
        {
            _displayFormat = 2;
            _EPCHeaderText = "SGTIN";
            _RSSIHeaderText = "RSSI";
            RaisePropertyChanged(() => EPCHeaderText);
            RaisePropertyChanged(() => RSSIHeaderText);
            ClearClick();
        }

        void BarcodeStart ()
        {
            if (BleMvxApplication._reader.barcode.state == CSLibrary.BarcodeReader.STATE.NOTVALID)
            {
                _userDialogs.ShowError ("Barcode module not exists");
                return;
            }

            _barcodeScanning = true;
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOn(CSLibrary.BarcodeReader.VIBRATORMODE.BAROCDEGOODREAD, BleMvxApplication._config.RFID_VibrationTime);
            BleMvxApplication._reader.barcode.Start();
            _startBarcodeScanButtonText = "Stop Scan";
            RaisePropertyChanged(() => startBarcodeScanButtonText);
        }

        void BarcodeStop ()
        {
            _barcodeScanning = false;
            BleMvxApplication._reader.barcode.Stop();
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();
            //Vibrator(false);
            _startBarcodeScanButtonText = "Start Scan";
            RaisePropertyChanged(() => startBarcodeScanButtonText);
        }

        void Linkage_CaptureCompleted(object sender, CSLibrary.Barcode.BarcodeEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                switch (e.MessageType)
                {
                    case CSLibrary.Barcode.Constants.MessageType.DEC_MSG:
                        AddOrUpdateBarcodeData((CSLibrary.Barcode.Structures.DecodeMessage)e.Message);
                        Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);
                        break;

                    case CSLibrary.Barcode.Constants.MessageType.ERR_MSG:
                        break;
                }
            });
        }

        private void AddOrUpdateBarcodeData(CSLibrary.Barcode.Structures.DecodeMessage decodeInfo)
        {
            if (decodeInfo != null)
            {
                int cnt = 0;
                bool found = false;

                for (; cnt < barcodeData.Count; cnt++)
                {
                    if (barcodeData[cnt].code == decodeInfo.pchMessage)
                    {
                        barcodeData[cnt].count++;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    BARCODEInfoViewModel item = new BARCODEInfoViewModel();

                    item.code = decodeInfo.pchMessage;
                    item.count = 1;

                    barcodeData.Insert(0, item);
                }
            }
        }

#endregion


#region Key_event

        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                Page currentPage;

                Trace.Message("Receive Key Event");

                // try to get current page
                try
                {
                    currentPage = ((TabbedPage)Application.Current.MainPage.Navigation.NavigationStack[1]).CurrentPage;
                }
                catch (Exception ex)
                {
                    return;
                }

                switch (currentPage.Title)
                {
                    case "RFID Inventory":
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
                                if (_KeyDown == true)
                                    StopInventory();
                                _KeyDown = false;
                            }
                        }
                        break;

                    case "Barcode Scan":
                        /*
                        if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
                        {
                            if (e.KeyDown)
                            {
                                BarcodeStart();
                            }
                            else
                            {
                                BarcodeStop();
                            }
                        }*/
                        break;
                }
            });
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

        string GetJsonData ()
        {
            try
            {
                RESTfulHeader data = new RESTfulHeader();

                data.sequenceNumber = BleMvxApplication._sequenceNumber++;
                data.rfidReaderName = BleMvxApplication._reader.ReaderName;


                data.rfidReaderSerialNumber = BleMvxApplication._reader.siliconlabIC.GetSerialNumberSync();
                if (data.rfidReaderSerialNumber == null)
                    _userDialogs.Alert("No Serial Number");

                data.rfidReaderInternalSerialNumber = BleMvxApplication._reader.rfid.GetPCBAssemblyCode();
                data.numberOfTags = (UInt16)_TagInfoList.Count;

                foreach (var tagitem in _TagInfoList)
                {
                    RESTfulSDetail item = new RESTfulSDetail();
                    item.pc = tagitem.PC.ToString("X4");
                    item.epc = tagitem.EPC.ToString();
                    item.timeOfRead = tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss.fff");
                    item.timeZone = tagitem.timeOfRead.ToString("zzz");
                    data.tags.Add(item);
                }

                string JSONdata = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                return JSONdata;
            }
            catch (Exception)
            {
                return null;
            }
        }

        string GetCSVData()
        {
            try
            {
                string CSVdata = "";

                foreach (var tagitem in _TagInfoList)
                {
                    CSVdata += tagitem.PC.ToString("X4") + ",";
                    CSVdata += tagitem.EPC.ToString() + ",";
                    CSVdata += tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss.fff") + ",";
                    CSVdata += tagitem.timeOfRead.ToString("zzz");
                    CSVdata += System.Environment.NewLine;
                    
                    /*
                                        CSVdata += "\"" + tagitem.PC.ToString("X4") + "\",";
                                        CSVdata += "\"" + tagitem.EPC.ToString() + "\",";
                                        CSVdata += tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss.fff") + "\",";
                                        CSVdata += "\"" + tagitem.timeOfRead.ToString("zzz") + "\"";
                                        CSVdata += System.Environment.NewLine;
                    */
                }

                return CSVdata;
            }
            catch (Exception)
            {
                return null;
            }
        }

        string GetExcelCSVData()
        {
            try
            {
                string CSVdata = "";

                foreach (var tagitem in _TagInfoList)
                {
                    CSVdata += "=\"" + tagitem.PC.ToString("X4") + "\",";
                    CSVdata += "=\"" + tagitem.EPC.ToString() + "\",";
                    CSVdata += tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss") + ",";
                    CSVdata += tagitem.timeOfRead.ToString("zzz");
                    CSVdata += System.Environment.NewLine;
                }

                return CSVdata;
            }
            catch (Exception)
            {
                return null;
            }
        }

        async System.Threading.Tasks.Task<bool> ShareData()
        {
            bool r = false;

            switch (BleMvxApplication._config.RFID_ShareFormat)
            {
                case 0: // JSON
                    r = await CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage
                    {
                        Text = GetJsonData(),
                        Title = "tags list"
                    });
                    break;

                case 1:
                    r = await CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage
                    {
                        Text = GetCSVData(),
                        Title = "tags list.csv"
                    });
                    break;

                case 2:
                    r = await CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage
                    {
                        Text = GetExcelCSVData(),
                        Title = "tags list.csv"
                    });
                    break;
            }

            return r;
        }

        async System.Threading.Tasks.Task<bool> SaveData()
        {
            string fileExtName = "";
            string Text = "";

            switch (BleMvxApplication._config.RFID_ShareFormat)
            {
                case 0: // JSON
                    fileExtName = "json";
                    Text = GetJsonData();
                    break;

                case 1:
                    fileExtName = "csv";
                    Text = GetCSVData();
                    break;

                case 2:
                    fileExtName = "csv";
                    Text = GetExcelCSVData();
                    break;

                default:
                    fileExtName = "txt";
                    break;
            }

            switch (Xamarin.Forms.Device.RuntimePlatform)
            {
                case Xamarin.Forms.Device.Android:
                    {
                        if (await _permissions.CheckPermissionStatusAsync<Plugin.Permissions.StoragePermission>() != PermissionStatus.Granted)
                        {
                            await _permissions.RequestPermissionAsync<Plugin.Permissions.StoragePermission>();
                        }

                        //string documents = @"/storage/emulated/0/Download/";
                        //string filename = documents + "InventoryData-" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "." + fileExtName;

                        var documents = DependencyService.Get<IExternalStorage>().GetPath();
                        var filename = System.IO.Path.Combine(documents, "InventoryData-" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "." + fileExtName);
                        //System.IO.File.WriteAllText(filename, Text);
                        using (var writer = System.IO.File.CreateText(filename))
                        {
                            await writer.WriteLineAsync(Text);
                        }
                    }
                    break;

                default:
                    {
                        var documents = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var filename = System.IO.Path.Combine(documents, "InventoryData-" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "." + fileExtName);
                        System.IO.File.WriteAllText(filename, Text);
                    }
                    break;
            }

            _userDialogs.AlertAsync("File saved, please check file in public folder");

            return true;
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

                foreach (var tagitem in _TagInfoList)
                {
                    RESTfulSDetail item = new RESTfulSDetail();
                    item.pc = tagitem.PC.ToString("X4");
                    item.epc = tagitem.EPC.ToString();
                    item.timeOfRead = tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss.fff");
                    item.timeZone = tagitem.timeOfRead.ToString("zzz");
                    data.tags.Add(item);
                }

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
