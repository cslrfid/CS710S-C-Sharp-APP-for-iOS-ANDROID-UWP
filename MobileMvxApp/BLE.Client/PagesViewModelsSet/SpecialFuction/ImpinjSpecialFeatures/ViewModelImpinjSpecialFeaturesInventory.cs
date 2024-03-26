using System;
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
using MvvmCross.Navigation;

namespace BLE.Client.ViewModels
{
    public class ViewModelImpinjSpecialFeaturesInventory : BaseViewModel
	{
		private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        #region -------------- RFID inventory -----------------

        public bool switchUsePinIsToggled { set; get; }
        public string entrySelectedPWD { set; get; }
        public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand { protected set; get; }
        public ICommand OnAuthenticateButtonCommand { protected set; get; }
        public ICommand OnProtectedModeButtonCommand { protected set; get; }


        private ObservableCollection<TagInfoViewModel> _TagInfoList = new ObservableCollection<TagInfoViewModel>();
		public ObservableCollection<TagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

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

/*
        private bool _switchTagTypeIsToggled = false;
        public bool switchTagTypeIsToggled { get { return _switchTagTypeIsToggled; } set { _switchTagTypeIsToggled = value; } }
*/

        private int _DefaultRowHight;

        bool _cancelVoltageValue = false;

        #endregion

        public ViewModelImpinjSpecialFeaturesInventory(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            RaisePropertyChanged(() => ListViewRowHeight);
            _DefaultRowHight = ListViewRowHeight;

            entrySelectedPWD = "00000000";
            RaisePropertyChanged(() => entrySelectedPWD);

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);
            OnAuthenticateButtonCommand = new Command(AuthenticateClick);
            OnProtectedModeButtonCommand = new Command(ProtectedModeClick);
        }

        ~ViewModelImpinjSpecialFeaturesInventory()
        {
            BleMvxApplication._reader.barcode.Stop();
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            InventorySetting();
            SetEvent(true);
        }

        public override void ViewDisappearing()
        {
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

        private void AuthenticateClick()
        {
            _navigation.Navigate<ViewModelImpinjSpecialFeaturesConfig>(new MvxBundle());
        }

        private void ProtectedModeClick()
        {
            _navigation.Navigate<ViewModelImpinjSpecialFeaturesProtectedMode>(new MvxBundle());
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

                // Barcode event handler
                BleMvxApplication._reader.barcode.OnCapturedNotify += new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Linkage_CaptureCompleted);

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
            BleMvxApplication._reader.rfid.SetCountry(BleMvxApplication._config.RFID_Region, (int)BleMvxApplication._config.RFID_FixedChannel);
            BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell);
            BleMvxApplication._reader.rfid.SetTagDelayTime((uint)BleMvxApplication._config.RFID_CompactInventoryDelayTime); // for CS108 only
            BleMvxApplication._reader.rfid.SetIntraPacketDelayTime((uint)BleMvxApplication._config.RFID_IntraPacketDelayTime); // for CS710S only
            BleMvxApplication._reader.rfid.SetDuplicateEliminationRollingWindow(BleMvxApplication._config.RFID_DuplicateEliminationRollingWindow);
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
            BleMvxApplication._reader.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;
            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.TID;
            BleMvxApplication._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.Hex.ToBytes("E2C0118");
            BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 25;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PREFILTER);

            BleMvxApplication._reader.rfid.SetRSSIdBmFilter(BleMvxApplication._RSSIFILTER_Type, BleMvxApplication._RSSIFILTER_Option, BleMvxApplication._RSSIFILTER_Threshold_dBm);

            BleMvxApplication._reader.rfid.Options.TagRanging.multibanks = 1;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.TID;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset1 = 0;
            BleMvxApplication._reader.rfid.Options.TagRanging.count1 = 6;

            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = false;
            BleMvxApplication._reader.rfid.Options.TagRanging.focus = BleMvxApplication._config.RFID_Focus;
            BleMvxApplication._reader.rfid.Options.TagRanging.fastid = BleMvxApplication._config.RFID_FastId;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);
        }

        void SelectProtectTag(UInt32 password)
        {
            byte[] protectedModePIN = new byte[4];

            protectedModePIN[3] = (byte)(password);
            protectedModePIN[2] = (byte)(password >> 8);
            protectedModePIN[1] = (byte)(password >> 16);
            protectedModePIN[0] = (byte)(password >> 24);

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.BANK3;
            BleMvxApplication._reader.rfid.Options.TagSelected.Mask = protectedModePIN;
            BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 32;
            //BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PREFILTER);
        }

        void StartInventory()
        {
            if (_startInventory == false)
                return;

            RaisePropertyChanged(() => switchUsePinIsToggled);
            if (switchUsePinIsToggled)
            {
                // Inventory invisable tag with PIN
                RaisePropertyChanged(() => entrySelectedPWD);

                var password = Convert.ToUInt32(entrySelectedPWD, 16);

                if (password == 00)
                {
                    _userDialogs.ShowError("Password can not all zero!!!", 3000);
                    return;
                }

                SelectProtectTag(password);
            }
            else
            {
                BleMvxApplication._reader.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;
                BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.TID;
                BleMvxApplication._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.Hex.ToBytes("E2C0118");
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 24;
                //BleMvxApplication._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.Hex.ToBytes("0118");
                //BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 12;
                //BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 13;
                BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PREFILTER);
            }

            /*
            //TagInfoList.Clear();
            if (switchTagTypeIsToggled)
            {
                // M730 / M750
                BleMvxApplication._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.Hex.ToBytes("E280119");
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 28;
            }
            else
            {
                // M775
                BleMvxApplication._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.Hex.ToBytes("E2C011A2");
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = 32;
            }
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PREFILTER);
            */


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
                    break;
            }
            //});
        }

        private void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info)
        {
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
                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks >= 1 && TagInfoList[cnt].Bank1Data != CSLibrary.Tools.Hex.ToString(info.Bank1Data))
                                continue;

                            if (BleMvxApplication._reader.rfid.Options.TagRanging.multibanks == 2 && TagInfoList[cnt].Bank2Data != CSLibrary.Tools.Hex.ToString(info.Bank2Data))
                                continue;

                            TagInfoList[cnt].Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);
                            TagInfoList[cnt].Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);
                            TagInfoList[cnt].RSSI = info.rssidBm;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        TagInfoViewModel item = new TagInfoViewModel();

                        item.EPC = info.epc.ToString();
                        item.Bank1Data = CSLibrary.Tools.Hex.ToString(info.Bank1Data);
                        item.Bank2Data = CSLibrary.Tools.Hex.ToString(info.Bank2Data);
                        item.RSSI = info.rssidBm;
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

#region -------------------- Barcode Scan -------------------

		public ICommand OnStartBarcodeScanButtonCommand { protected set; get; }
        public ICommand OnClearBarcodeDataButtonCommand { protected set; get; }

        private string _startBarcodeScanButtonText = "Start Scan";
        public string startBarcodeScanButtonText { get { return _startBarcodeScanButtonText; } }

        public class BARCODERECORD
        {
            public string code { get; set; }
            public uint count { get; set; }
        }

        public ObservableCollection<BARCODERECORD> barcodeData { get; set; } = new ObservableCollection<BARCODERECORD>();

        void Linkage_CaptureCompleted(object sender, CSLibrary.Barcode.BarcodeEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                switch (e.MessageType)
                {
                    case CSLibrary.Barcode.Constants.MessageType.DEC_MSG:
                        AddOrUpdateBarcodeData((CSLibrary.Barcode.Structures.DecodeMessage)e.Message);
                        //UpdateUI((DecodeMessage)e.Message, "Barcode Captured...");
                        break;
                    case CSLibrary.Barcode.Constants.MessageType.ERR_MSG:
                        //UpdateUI(null, String.Format("Barcode Returned: {0}", e.ErrorMessage));
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
                    BARCODERECORD item = new BARCODERECORD();

                    item.code = decodeInfo.pchMessage;
                    item.count = 1;

                    barcodeData.Add(item);
                }
            }
        }

        #endregion

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
