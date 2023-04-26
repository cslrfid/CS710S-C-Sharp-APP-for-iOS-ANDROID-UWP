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

using Plugin.Share;
using Plugin.Share.Abstractions;
using MvvmCross.ViewModels;
using MvvmCross.Navigation;

namespace BLE.Client.ViewModels
{
    public class ViewModelCS83045Inventory : BaseViewModel
    {
        public class ColdChainTagInfoViewModel : BindableBase
        {
            private string _EPC;
            public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }

            private string _LogStatus;
            public string LogStatus { get { return this._LogStatus; } set { this.SetProperty(ref this._LogStatus, value); } }

            private string _T1;
            public string T1 { get { return this._T1; } set { this.SetProperty(ref this._T1, value); } }

            private string _T2;
            public string T2 { get { return this._T2; } set { this.SetProperty(ref this._T2, value); } }

            private string _B;
            public string B { get { return this._B; } set { this.SetProperty(ref this._B, value); } }
        }

        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand { protected set; get; }
        public ICommand OnStartLogButtonCommand { protected set; get; }
        public ICommand OnStopLogButtonCommand { protected set; get; }
        public ICommand OnGetLogButtonCommand { protected set; get; }

        private ObservableCollection<ColdChainTagInfoViewModel> _TagInfoList = new ObservableCollection<ColdChainTagInfoViewModel>();
        public ObservableCollection<ColdChainTagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

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

        public bool _startInventory = true;

        public int tagsCount = 0;
        int _tagCountForAlert = 0;
        bool _newTagFound = false;

        DateTime InventoryStartTime;
        private double _InventoryTime = 0;
        public string InventoryTime { get { return ((uint)_InventoryTime).ToString() + "s"; } }

        private string _currentPower;
        public string currentPower { get { return _currentPower; } set { _currentPower = value; } }

        bool _cancelVoltageValue = false;

        #endregion

        public ViewModelCS83045Inventory(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            RaisePropertyChanged(() => ListViewRowHeight);

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);
            OnStartLogButtonCommand = new Command(OnStartLogButtonClcked);
            OnStopLogButtonCommand = new Command(OnStopLogButtonClcked);
            OnGetLogButtonCommand = new Command(OnGetLogButtonClcked);
        }

        ~ViewModelCS83045Inventory()
        {
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            // RFID event handler
            BleMvxApplication._reader.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
            BleMvxApplication._reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
            BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);

            // Key Button event handler
            BleMvxApplication._reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
            BleMvxApplication._reader.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);

            InventorySetting();
        }

        public override void ViewDisappearing()
        {
            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();                // Confirm cancel all filter

            BleMvxApplication._reader.rfid.StopOperation();
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
            BleMvxApplication._reader.barcode.Stop();

            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.OnStateChanged -= new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
            BleMvxApplication._reader.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
            BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);

            // Key Button event handler
            BleMvxApplication._reader.notification.OnKeyEvent -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
            BleMvxApplication._reader.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);

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

        //private TagInfoViewModel _ItemSelected;
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

        void InventorySetting()
        {
            switch (BleMvxApplication._config.RFID_FrequenceSwitch)
            {
                case 0:
                    BleMvxApplication._reader.rfid.SetHoppingChannels(BleMvxApplication._config.RFID_Region);
                    break;
                case 1:
                    BleMvxApplication._reader.rfid.SetFixedChannel(BleMvxApplication._config.RFID_Region, BleMvxApplication._config.RFID_FixedChannel);
                    break;
                case 2:
                    BleMvxApplication._reader.rfid.SetAgileChannels(BleMvxApplication._config.RFID_Region);
                    break;
            }

            BleMvxApplication._reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;

            // Setting 1
            SetConfigPower();

            // Setting 3  // MUST SET for RFMicro
            BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = BleMvxApplication._config.RFID_ToggleTarget ? 1U : 0;
            BleMvxApplication._reader.rfid.SetDynamicQParms(BleMvxApplication._config.RFID_DynamicQParms);

            // Setting 4
            BleMvxApplication._config.RFID_FixedQParms.toggleTarget = BleMvxApplication._config.RFID_ToggleTarget ? 1U : 0;
            BleMvxApplication._reader.rfid.SetFixedQParms(BleMvxApplication._config.RFID_FixedQParms);

            // Setting 2
            BleMvxApplication._reader.rfid.SetOperationMode(BleMvxApplication._config.RFID_OperationMode);
            //BleMvxApplication._reader.rfid.SetTagGroup(CSLibrary.Constants.Selected.ASSERTED, CSLibrary.Constants.Session.S1, CSLibrary.Constants.SessionTarget.A);
            BleMvxApplication._reader.rfid.SetCurrentSingulationAlgorithm(BleMvxApplication._config.RFID_Algorithm);
            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            // Multi bank inventory
            BleMvxApplication._reader.rfid.Options.TagRanging.multibanks = 2;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.BANK3;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset1 = 188;
            BleMvxApplication._reader.rfid.Options.TagRanging.count1 = 2;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank2 = CSLibrary.Constants.MemoryBank.BANK3;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset2 = 256;
            BleMvxApplication._reader.rfid.Options.TagRanging.count2 = 9;
            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = false;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);
        }

        void StartInventory()
        {
            if (_startInventory == false)
                return;

            InventorySetting(); // reset inventory setting

            StartTagCount();
            {
                _startInventory = false;
                _startInventoryButtonText = "Stop Inventory";
            }

            InventoryStartTime = DateTime.Now;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);
            _cancelVoltageValue = true;

            RaisePropertyChanged(() => startInventoryButtonText);
        }

        void StopInventory()
        {
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

            // data validation
            if (e.info.Bank1Data.Length != 2 || e.info.Bank2Data.Length != 9)
                return;

            // Check Tag ID
            if (e.info.Bank1Data[0] != 0x8304 && e.info.Bank1Data[0] != 0x8305)
                return;

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
            InvokeOnMainThread(() =>
            {
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
            });
        }

        private void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info)
        {
            bool found = false;
            int cnt;

            lock (TagInfoList)
            {
                string EPC = info.epc.ToString();
                string LogStatus = "";
                string T1Status = "";
                string T2Status = "";
                string BStatus = "";

                switch (info.Bank2Data[4] & 0x03)
                {
                    case 0:
                    case 2:
                        LogStatus = "Stop";
                        break;
                    case 1:
                        LogStatus = "Recording";
                        break;
                    case 3:
                        LogStatus = "Error";
                        break;
                }
                    
                //if ((tagdata[e.info.pc.EPCLength + 10] & 0x02) != 0)
                if ((info.Bank2Data[8] & 0x02) != 0)
                    T1Status = "Fail";
                else
                    T1Status = "OK";

                if ((info.Bank2Data[8] & 0x04) != 0)
                    T2Status = "Fail";
                else
                    T2Status = "OK";

                if ((info.Bank2Data[0] & 0x8000) != 0)
                    BStatus = "Fail";
                else
                    BStatus = "OK";

                for (cnt = 0; cnt < TagInfoList.Count; cnt++)
                {
                    if (TagInfoList[cnt].EPC == EPC)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    TagInfoList[cnt].LogStatus = LogStatus;
                    TagInfoList[cnt].T1 = T1Status;
                    TagInfoList[cnt].T2 = T2Status;
                    TagInfoList[cnt].B = BStatus;
                }
                else
                {
                    ColdChainTagInfoViewModel item = new ColdChainTagInfoViewModel();

                    item.EPC = EPC;
                    item.LogStatus = LogStatus;
                    item.T1 = T1Status;
                    item.T2 = T2Status;
                    item.B = BStatus;

                    TagInfoList.Insert(0, item);
                }
            }
        }

        bool _EnableAllTagsLog = false;
        bool _DisableAllTagsLog = false;
        int _ProcessTagNumber = 0;
        int _ProcessState = 0;
        int _ProcessRetry = 10;

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                if (_EnableAllTagsLog)
                    StartLogResultProccess(e);
                else if (_DisableAllTagsLog)
                    StopLogResultProccess(e);
                else
                {
                    _userDialogs.ShowSuccess("Process Error!", 2000);
                    // Error
                }
            });
        }

        void OnStartLogButtonClcked()
        {
            if (_EnableAllTagsLog || _DisableAllTagsLog)
                return;

            _EnableAllTagsLog = true;
            _ProcessTagNumber = 0;
            _ProcessState = 0;
            _ProcessRetry = 10;

            StartLog();
        }

        void OnStopLogButtonClcked()
        {
            if (_EnableAllTagsLog || _DisableAllTagsLog)
                return;

            _DisableAllTagsLog = true;
            _ProcessTagNumber = 0;
            _ProcessState = 0;
            _ProcessRetry = 10;

            StopLog();
        }

        void OnGetLogButtonClcked ()
        {
            //ShowViewModel<ViewModelCS83045ViewLog>(new MvxBundle());
            _navigation.Navigate<ViewModelCS83045ViewLog>(new MvxBundle());
        }

        private void StartLogResultProccess(CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (!e.success)
            {
                // if process fail
                if (_ProcessRetry == 0)
                {
                    // Start Log Fail
                    TagInfoList[_ProcessTagNumber].LogStatus = "Error";
                    _ProcessTagNumber++;
                    _ProcessState = 0;
                }
                else
                    _ProcessRetry--;

                StartLog();
            }
            else
            {
                _ProcessState++;
                _ProcessRetry = 10;

                StartLog();
            }
        }

        /// <summary>
        /// return 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void StartLog()
        {
            switch (_ProcessState)
            {
                case 0:
                    {
                        for (; _ProcessTagNumber < TagInfoList.Count; _ProcessTagNumber++)
                        {
                            if (TagInfoList[_ProcessTagNumber].LogStatus != "Recording")
                                break;
                        }

                        if (_ProcessTagNumber >= TagInfoList.Count)
                        {
                            _userDialogs.ShowSuccess("Start Log Finish!", 2000);
                            _EnableAllTagsLog = false;
                            return;
                        }

                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(TagInfoList[_ProcessTagNumber].EPC);
                        BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

                        BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = 0x0;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 240;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0xa600;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 1:
                    {
                        UInt32 uut = (UInt32)UnixTime(DateTime.Now);
                        UInt16 Offset = (UInt16)(Math.Abs(BleMvxApplication._coldChain_TempOffset) * 4);

                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 4;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[4];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = (UInt16)(uut >> 16);
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[1] = (UInt16)(uut & 0xffff);
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[2] = (UInt16)BleMvxApplication._coldChain_LogInterval;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[3] = (UInt16)(Offset);
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 2:
                    {
                        //UInt16 OTemp1 = (UInt16)(double.Parse(textBox_OTH1.Text) * 4);
                        //UInt16 UTemp1 = (UInt16)(double.Parse(textBox_UTH1.Text) * 4);
                        //UInt16 Count = (UInt16)((UInt16.Parse(textBox_THC1.Text) << 3) | (UInt16.Parse(textBox_THC2.Text) << 9));
                        UInt16 OTemp1 = (UInt16)(BleMvxApplication._coldChain_Temp1THOver * 4);
                        UInt16 UTemp1 = (UInt16)(BleMvxApplication._coldChain_Temp1THUnder * 4);
                        UInt16 Count = (UInt16)((BleMvxApplication._coldChain_Temp1THCount << 3) | (BleMvxApplication._coldChain_Temp2THCount << 9));

                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 262;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 3;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[3];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = (UInt16)(OTemp1);
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[1] = (UInt16)(UTemp1);
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[2] = Count;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 3:
                    {
                        //UInt16 UTemp2 = (UInt16)(double.Parse(textBox_UTH2.Text) * 4);
                        //UInt16 OTemp2 = (UInt16)(double.Parse(textBox_OTH2.Text) * 4);
                        UInt16 UTemp2 = (UInt16)(BleMvxApplication._coldChain_Temp2THUnder * 4);
                        UInt16 OTemp2 = (UInt16)(BleMvxApplication._coldChain_Temp2THOver * 4);

                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 266;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 2;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[2];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = (UInt16)(OTemp2);
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[1] = (UInt16)(UTemp2);
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 4:
                    {
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 260;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0x0001;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 5:
                    {
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 240;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0xa000;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 6:
                    {
                        CSLibrary.Structures.S_DATA value = new CSLibrary.Structures.S_DATA("0000");
                        ReadUserData(240, 1, ref value);
                    }
                    break;

                case 7:
                    {
                        TagInfoList[_ProcessTagNumber].LogStatus = "Recording";
                        TagInfoList[_ProcessTagNumber].T1 = "";
                        TagInfoList[_ProcessTagNumber].T2 = "";

                        _ProcessTagNumber++;
                        _ProcessState = 0;
                        StartLog();
                    }
                    break;
            }
        }

        private void StopLogResultProccess(CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (!e.success)
            {
                // if process fail
                if (_ProcessRetry == 0)
                {
                    // Start Log Fail
                    TagInfoList[_ProcessTagNumber].LogStatus = "Error";
                    _ProcessTagNumber++;
                    _ProcessState = 0;
                }
                else
                    _ProcessRetry--;

                StopLog();
            }
            else
            {
                _ProcessState++;
                _ProcessRetry = 10;

                StopLog();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void StopLog()
        {
            switch (_ProcessState)
            {
                case 0:
                    {
                        for (; _ProcessTagNumber < TagInfoList.Count; _ProcessTagNumber++)
                        {
                            if (TagInfoList[_ProcessTagNumber].LogStatus != "Stop")
                                break;
                        }

                        if (_ProcessTagNumber >= TagInfoList.Count)
                        {
                            _userDialogs.ShowSuccess("Stop Log Finish!", 2000);
                            _DisableAllTagsLog = false;
                            return;
                        }

                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(TagInfoList[_ProcessTagNumber].EPC);
                        BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

                        BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = 0x0;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 260;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0x0002;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 1:
                    {
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 240;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0xa600;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 2:
                    {
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 240;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0xa600;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 3:
                    {
                        BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 264;
                        BleMvxApplication._reader.rfid.Options.TagReadUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagReadUser.pData = new CSLibrary.Structures.S_DATA(new UInt16[1]);
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
                    }
                    break;

                case 4:
                    {
                        TagInfoList[_ProcessTagNumber].T1 = "OK";
                        TagInfoList[_ProcessTagNumber].T2 = "OK";
                        if ((BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts()[0] & 0x0006) != 0x0000)
                        {
                            if ((BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts()[0] & 0x0002) != 0x0000)
                                TagInfoList[_ProcessTagNumber].T1 = "Fail";

                            if ((BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts()[0] & 0x0004) != 0x0000)
                                TagInfoList[_ProcessTagNumber].T2 = "Fail";

                            // Show Temperature alarm on LED
                            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 264;
                            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = (UInt16)(BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts()[0] | 0x01);
                            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                        }
                        else
                        {
                            _ProcessState ++;
                            StopLog();
                        }
                    }
                    break;

                case 5:
                    {
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 240;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0x0000;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 6:
                    {
                        CSLibrary.Structures.S_DATA value = new CSLibrary.Structures.S_DATA("0000");
                        ReadUserData(240, 1, ref value);
                    }
                    break;

                case 7:
                    {
                        TagInfoList[_ProcessTagNumber].LogStatus = "Stop";
                        TagInfoList[_ProcessTagNumber].B = "";

                        _ProcessTagNumber++;
                        _ProcessState = 0;

                        StopLog();
                    }
                    break;
            }
        }

        void ReadUserData(ushort Offset, ushort Count, ref CSLibrary.Structures.S_DATA data)
        {
            Console.Write("Read User Data Offset:{0} Count:{1}", Offset, Count);

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = 0;
            BleMvxApplication._reader.rfid.Options.TagReadUser.offset = Offset;
            BleMvxApplication._reader.rfid.Options.TagReadUser.count = Count;
            BleMvxApplication._reader.rfid.Options.TagReadUser.pData = data;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
        }

        DateTime UnixTimeStampBase = new DateTime(1970, 1, 1, 00, 0, 0, 0);
        DateTime UnixTime(double second)
        {
            return UnixTimeStampBase.AddSeconds(second);
        }

        double UnixTime(DateTime st)
        {
            return (st - UnixTimeStampBase).TotalSeconds;
        }

        void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                if (e.Voltage == 0xffff)
                {
                    _labelVoltage = "CS108 Bat. ERROR"; //			3.98v
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
                            _labelVoltage = "CS108 Bat. " + ((double)e.Voltage / 1000).ToString("0.000") + "v"; //			v
                            break;

                        default:
                            _labelVoltage = "CS108 Bat. " + ClassBattery.Voltage2Percent((double)e.Voltage / 1000).ToString("0") + "%"; //			%
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
