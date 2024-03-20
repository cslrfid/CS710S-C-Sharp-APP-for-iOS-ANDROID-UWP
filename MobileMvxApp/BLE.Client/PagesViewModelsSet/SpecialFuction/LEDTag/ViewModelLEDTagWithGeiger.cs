using System;
using Acr.UserDialogs;
using Plugin.BLE.Abstractions.Contracts;

using System.Windows.Input;
using Xamarin.Forms;
using MvvmCross.ViewModels;
using BLE.Client.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelLEDTagWithGeiger : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public ICommand OnStartGeigerButtonCommand { protected set; get; }

        private int _rssidBuV = 0;
        private string _rssiString = "RSSI";
        public string rssiStart { get { return _rssiString; } }

        double _progressbarRSSIValue = 0;
        public double progressbarRSSIValue { get { return _progressbarRSSIValue; } }

        private string _startGeigerButtonText = "Start";
        public string startGeigerButtonText { get { return _startGeigerButtonText; } }

        private int _buttonBank = 1;
        public int buttonBank { get { return _buttonBank; } set { _buttonBank = value; } }

        private string _entryEPC;
        public string entryEPC { get { return _entryEPC; } set { _entryEPC = value; } }

        private uint _power = 300;
        public uint power { get { return _power; } set { _power = value; } }

        private int _Threshold = 0;
        public string labelThresholdValueText { get { return _Threshold.ToString(); } set { try { _Threshold = int.Parse(value); } catch (Exception ex) { } } }


        // end for test

        bool _startInventory = false;
        public bool _KeyDown = false;
        int _beepSoundCount = 0;
        int _noTagCount = 0;

        public ViewModelLEDTagWithGeiger(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            _entryEPC = BleMvxApplication._SELECT_TID;

            OnStartGeigerButtonCommand = new Command(StartGeigerButtonClick);

            RaisePropertyChanged(() => entryEPC);
            _Threshold = BleMvxApplication._config.RFID_DBm ? -47 : 60;
        }

        private void SetEvent(bool enable)
        {
            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.ClearEventHandler();

            // Key Button event handler
            BleMvxApplication._reader.notification.ClearEventHandler();

            if (enable)
            {
                BleMvxApplication._reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagSearchOneEvent);

                // Key Button event handler
                BleMvxApplication._reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
            }
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            SetEvent(true);
            InventorySetting();
        }

        public override void ViewDisappearing()
        {
            // don't turn off event handler is you need program work in sleep mode.
            StopGeiger();
            SetEvent(false);
            base.ViewDisappearing();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        void InventorySetting()
        {
            // Cancel old setting
            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
            BleMvxApplication._reader.rfid.SetPowerSequencing(0);

            // Set Geiger parameters
            BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell);
            BleMvxApplication._reader.rfid.SetTagDelayTime((uint)BleMvxApplication._config.RFID_CompactInventoryDelayTime); // for CS108 only
            BleMvxApplication._reader.rfid.SetIntraPacketDelayTime((uint)BleMvxApplication._config.RFID_IntraPacketDelayTime); // for CS710S only
            BleMvxApplication._reader.rfid.SetDuplicateEliminationRollingWindow(0);
            BleMvxApplication._config.RFID_FixedQParms.qValue = 1;
            BleMvxApplication._config.RFID_FixedQParms.toggleTarget = 1;
            BleMvxApplication._reader.rfid.SetFixedQParms(BleMvxApplication._config.RFID_FixedQParms);
            BleMvxApplication._reader.rfid.SetCurrentSingulationAlgorithm(CSLibrary.Constants.SingulationAlgorithm.FIXEDQ);
            BleMvxApplication._reader.rfid.SetRSSIFilter(CSLibrary.Constants.RSSIFILTERTYPE.DISABLE);
            switch (BleMvxApplication._reader.rfid.GetModel())
            {
                case CSLibrary.RFIDDEVICE.MODEL.CS710S:
                    if (BleMvxApplication._reader.rfid.GetCountry() == 1)
                        BleMvxApplication._reader.rfid.SetCurrentLinkProfile(241);
                    else
                        BleMvxApplication._reader.rfid.SetCurrentLinkProfile(244);
                    break;

                default:
                    BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);
                    break;
            }
            BleMvxApplication._reader.rfid.SetOperationMode(BleMvxApplication._config.RFID_OperationMode);
            BleMvxApplication._reader.rfid.SetTagGroup(BleMvxApplication._config.RFID_TagGroup);

            // Multi bank inventory
            BleMvxApplication._reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.SELECT;
            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = false;
            BleMvxApplication._reader.rfid.Options.TagRanging.focus = BleMvxApplication._config.RFID_Focus;

            BleMvxApplication._reader.rfid.Options.TagRanging.multibanks = 1;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.USER;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset1 = 112;
            BleMvxApplication._reader.rfid.Options.TagRanging.count1 = 1;
        }


        void StartGeiger()
        {
            if (_startInventory)
                return;

            _startGeigerButtonText = "Stop";
            _startInventory = true;

            RaisePropertyChanged(() => entryEPC);
            RaisePropertyChanged(() => power);

            BleMvxApplication._reader.rfid.SetPowerLevel(_power);

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            if (BleMvxApplication._geiger_Bank == 1) // if EPC
            {
                BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(_entryEPC);
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)_entryEPC.Length * 4;
            }
            else
            {
                BleMvxApplication._reader.rfid.Options.TagSelected.bank = (CSLibrary.Constants.MemoryBank)BleMvxApplication._geiger_Bank;
                BleMvxApplication._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.HexEncoding.ToBytes(_entryEPC);
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
                BleMvxApplication._reader.rfid.Options.TagSelected.MaskLength = (uint)_entryEPC.Length * 4;
            }
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_RANGING);

            RaisePropertyChanged(() => startGeigerButtonText);

            // Create a beep sound timer.
            _beepSoundCount = 0;
            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                if (_rssidBuV == 0)
                {
                    _noTagCount++;

                    if (_noTagCount > 2)
                        DependencyService.Get<ISystemSound>().SystemSound(-1);
                }
                else
                {
                    if (_beepSoundCount == 0 && _rssidBuV >= 20 && _rssidBuV < 60)
                        //if (_beepSoundCount == 0)
                        DependencyService.Get<ISystemSound>().SystemSound(3);

                    _beepSoundCount++;

                    if ((BleMvxApplication._config.RFID_DBm && CSLibrary.Tools.dBConverion.dBuV2dBm(_rssidBuV) >= _Threshold) ||
                        (!BleMvxApplication._config.RFID_DBm && _rssidBuV >= _Threshold))
                    {
                        DependencyService.Get<ISystemSound>().SystemSound(4);
                        _beepSoundCount = 1;
                        _rssidBuV = 0;
                    }
                    else if (_rssidBuV >= 50)
                    {
                        if (_beepSoundCount >= 5)
                        {
                            _beepSoundCount = 0;
                            _rssidBuV = 0;
                        }
                    }
                    else if (_rssidBuV >= 40)
                    {
                        if (_beepSoundCount >= 10)
                        {
                            _beepSoundCount = 0;
                            _rssidBuV = 0;
                        }
                    }
                    else if (_rssidBuV >= 30)
                    {
                        if (_beepSoundCount >= 20)
                        {
                            _beepSoundCount = 0;
                            _rssidBuV = 0;
                        }
                    }
                    else if (_rssidBuV >= 20)
                    {
                        if (_beepSoundCount >= 40)
                        {
                            _beepSoundCount = 0;
                            _rssidBuV = 0;
                        }
                    }
                }

                if (_startInventory)
                    return true;

                // Stop all sound
                DependencyService.Get<ISystemSound>().SystemSound(-1);
                return false;
            });
        }

        void StopGeiger()
        {
            _startInventory = false;
            _startGeigerButtonText = "Start";
            BleMvxApplication._reader.rfid.StopOperation();
            RaisePropertyChanged(() => startGeigerButtonText);
        }

        void StartGeigerButtonClick()
        {
            if (!_startInventory)
            {
                StartGeiger();
            }
            else
            {
                StopGeiger();
            }
        }

        public void TagSearchOneEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            switch (e.type)
            {
                //case CSLibrary.Constants.CallbackType.TAG_SEARCHING:
                case CSLibrary.Constants.CallbackType.TAG_RANGING:

                    _rssidBuV = (int)Math.Round(e.info.rssi);
                    _noTagCount = 0;

                    if (BleMvxApplication._config.RFID_DBm)
                    {
                        // 0~1
                        _progressbarRSSIValue = e.info.rssidBm;
                    }
                    else
                    {
                        // 0~1
                        _progressbarRSSIValue = e.info.rssi;
                    }
                    _rssiString = ((int)Math.Round(_progressbarRSSIValue)).ToString();

                    RaisePropertyChanged(() => rssiStart);
                    RaisePropertyChanged(() => progressbarRSSIValue);
                    break;
            }
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            switch (e.state)
            {
                case CSLibrary.Constants.RFState.IDLE:
                    break;
            }
        }

        bool CheckPageActive()
        {
            try
            {
                if (Application.Current.MainPage.Navigation.NavigationStack.Count > 0)
                {
                    var currPage = Application.Current.MainPage.Navigation.NavigationStack[Application.Current.MainPage.Navigation.NavigationStack.Count - 1];

                    if (currPage.Title == "Geiger")
                        return true;
                }
            }
            catch (Exception ex)
            {
            }

            return false;
        }


        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            Page currentPage;

            //if (!CheckPageActive())
            //    return;

            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
            {
                if (e.KeyDown)
                {
                    if (!_KeyDown)
                        StartGeiger();
                    _KeyDown = true;
                }
                else
                {
                    if (_KeyDown == true)
                        StopGeiger();
                    _KeyDown = false;
                }
            }
        }
    }
}
