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

//using PCLStorage;

using System.Net.Http;
using System.Net.Http.Headers;

using Plugin.Share;
using Plugin.Share.Abstractions;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelPerformanceTest : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand { protected set; get; }

        HashSet<string> TagList = new HashSet<string>();

        public bool _InventoryScanning = false;

        public string FilterIndicator { get { return (BleMvxApplication._PREFILTER_Enable | BleMvxApplication._POSTFILTER_MASK_Enable) ? "Filter On" : ""; } }

        private string _startInventoryButtonText = "Start Inventory";
        public string startInventoryButtonText { get { return _startInventoryButtonText; } }

        bool _tagCount = false;



        private string _labelRunningTimeText;
        public string labelRunningTimeText { get { return _labelRunningTimeText; } }
        private string _labelTotalTagsText;
        public string labelTotalTagsText { get { return _labelTotalTagsText; } }
        private string _labelTagpsText;
        public string labelTagpsText { get { return _labelTagpsText; } }
        private string _labelNewTagsText;
        public string labelNewTagsText { get { return _labelNewTagsText; } }
        
        private string minRSSIEPC = "";
        public string labelminRSSIEPCText { get { return minRSSIEPC; } }

        private float minRSSI = 10000;
        public string labelminRSSIText { get { if (minRSSI == 10000) return "0"; else return minRSSI.ToString("F1"); } }

        public string labelminRSSIdBmText { get { if (minRSSI == 10000) return "-107.0"; else return (CSLibrary.Tools.dBConverion.dBuV2dBm(minRSSI)).ToString("F1"); } }

        private string _tagPerSecondText = "0/0 new/tags/s     ";
        public string tagPerSecondText { get { return _tagPerSecondText; } }
        private string _numberOfTagsText = "     0 tags";
        public string numberOfTagsText { get { return _numberOfTagsText; } }
        private string _labelVoltage = "";
        public string labelVoltage { get { return _labelVoltage; } }
        public string labelVoltageTextColor { get { return BleMvxApplication._batteryLow ? "Red" : "Black"; } }

        DateTime InventoryStartTime;
        private double _InventoryTime = 0;
        public string InventoryTime { get { return ((uint)_InventoryTime).ToString() + "s"; } }

        public string _DebugMessage = "";
        public string DebugMessage { get { return _DebugMessage; } }

        bool _cancelVoltageValue = false;

        bool _waitingRFIDIdle = false;

        // Tag Counter for Inventory Alert
        uint _tagPerSecond = 0;
        uint _newTagPerSecond = 0;

        #endregion

        public ViewModelPerformanceTest(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);

            //SetEvent(true);

            InventorySetting();
        }

        ~ViewModelPerformanceTest()
        {
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
            _InventoryScanning = false;
            StopInventory();
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();

            SetEvent(false);

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

            // Setting 1
            BleMvxApplication._reader.rfid.SetTagDelayTime((uint)BleMvxApplication._config.RFID_CompactInventoryDelayTime); // for CS108 only
            BleMvxApplication._reader.rfid.SetIntraPacketDelayTime((uint)BleMvxApplication._config.RFID_IntraPacketDelayTime); // for CS710S only
            BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell);

            // Set Power
            SetConfigPower();

            // Setting 3
            BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = BleMvxApplication._config.RFID_ToggleTarget ? 1U : 0;
            BleMvxApplication._reader.rfid.SetDynamicQParms(BleMvxApplication._config.RFID_DynamicQParms);

            // Setting 4
            BleMvxApplication._config.RFID_FixedQParms.toggleTarget = BleMvxApplication._config.RFID_ToggleTarget ? 1U : 0;
            BleMvxApplication._reader.rfid.SetFixedQParms(BleMvxApplication._config.RFID_FixedQParms);

            // Setting 2
            BleMvxApplication._reader.rfid.SetOperationMode(BleMvxApplication._config.RFID_OperationMode);
            BleMvxApplication._reader.rfid.SetTagGroup(BleMvxApplication._config.RFID_TagGroup);
            BleMvxApplication._reader.rfid.SetCurrentSingulationAlgorithm(BleMvxApplication._config.RFID_Algorithm);
            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            // Select Criteria filter
            if (BleMvxApplication._PREFILTER_Enable)
            {
                BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                if (BleMvxApplication._PREFILTER_Bank == 1) // if EPC
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

            // Post Match Criteria filter
            if (BleMvxApplication._POSTFILTER_MASK_Enable)
            {
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(BleMvxApplication._POSTFILTER_MASK_EPC);

                CSLibrary.Structures.SingulationCriterion[] sel = new CSLibrary.Structures.SingulationCriterion[1];
                sel[0] = new CSLibrary.Structures.SingulationCriterion();
                sel[0].match = BleMvxApplication._POSTFILTER_MASK_MatchNot ? 0U : 1U;
                sel[0].mask = new CSLibrary.Structures.SingulationMask(BleMvxApplication._POSTFILTER_MASK_Offset, (uint)(BleMvxApplication._POSTFILTER_MASK_EPC.Length * 4), BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.ToBytes());
                BleMvxApplication._reader.rfid.SetPostMatchCriteria(sel);
                BleMvxApplication._reader.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.POSTMATCH;
            }

            // Multi bank inventory
            BleMvxApplication._reader.rfid.Options.TagRanging.multibanks = 0;
            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = true;
            BleMvxApplication._reader.rfid.Options.TagRanging.focus = BleMvxApplication._config.RFID_Focus;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);

            //ShowDialog("Configuring RFID");
        }

        void StartInventory()
        {
            if (BleMvxApplication._reader.BLEBusy)
            {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
                return;
            }

            _startInventoryButtonText = "Stop Inventory";

            TagList.Clear();
            _tagPerSecond = 0;
            _newTagPerSecond = 0;
            minRSSIEPC = "";
            minRSSI = 10000;
            _InventoryScanning = true;
            StartTagCount();
            InventoryStartTime = DateTime.Now;
            UpdateScreen();

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);
            _cancelVoltageValue = true;

            RaisePropertyChanged(() => startInventoryButtonText);
        }

        async void StopInventory()
        {
            BleMvxApplication._reader.rfid.StopOperation();
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();
            _waitingRFIDIdle = true;
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


        void UpdateScreen()
        {
            InvokeOnMainThread(() =>
            {
                TimeSpan runningTime = (DateTime.Now - InventoryStartTime);

                _labelRunningTimeText = runningTime.Hours.ToString() + ":" + runningTime.Minutes.ToString() + ":" + runningTime.Seconds.ToString();
                RaisePropertyChanged(() => labelRunningTimeText);

                _labelTotalTagsText = TagList.Count.ToString();
                RaisePropertyChanged(() => labelTotalTagsText);

                _labelTagpsText = _tagPerSecond.ToString();
                RaisePropertyChanged(() => labelTagpsText);

                _labelNewTagsText = _newTagPerSecond.ToString();
                RaisePropertyChanged(() => labelNewTagsText);

                RaisePropertyChanged(() => labelminRSSIEPCText);

                RaisePropertyChanged(() => labelminRSSIText);

                _InventoryTime = runningTime.TotalSeconds;
                RaisePropertyChanged(() => InventoryTime);

                _DebugMessage = CSLibrary.InventoryDebug._inventoryPacketCount.ToString() + " OK, " + CSLibrary.InventoryDebug._inventorySkipPacketCount.ToString() + " Fail";
                RaisePropertyChanged(() => DebugMessage);

                _numberOfTagsText = "     " + TagList.Count.ToString() + " tags";
                RaisePropertyChanged(() => numberOfTagsText);

                _tagPerSecondText = _newTagPerSecond.ToString() + "/" + _tagPerSecond.ToString() + " new/tags/s     ";
                RaisePropertyChanged(() => tagPerSecondText);

                _tagPerSecond = 0;
                _newTagPerSecond = 0;
            });
        }

        void StartTagCount()
        {
            _tagCount = true;

            // Create a timer that waits one second, then invokes every second.
            Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
            {
                UpdateScreen();

                if (_tagCount)
                    return true;

                return false;
            });
        }

        void StopInventoryClick()
        {
            BleMvxApplication._reader.rfid.StopOperation();
            _waitingRFIDIdle = true;
        }

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING)
                return;

            string EPC = e.info.epc.ToString();

            if (e.info.Bank1Data != null)
                if (e.info.Bank1Data.Length > 0)
                    EPC += CSLibrary.Tools.Hex.ToString(e.info.Bank1Data);

            _tagPerSecond++;

            if (TagList.Add(EPC))
            {
                _newTagPerSecond++;
            }

            if (e.info.rssi < minRSSI)
            {
                minRSSI = e.info.rssi;
                minRSSIEPC = EPC;
            }
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
                                _userDialogs.Alert("Mac error : 0x" + BleMvxApplication._reader.rfid.LastMacErrorCode.ToString("X4"));
                                break;
                        }

                        //InventoryStopped();
                        break;
                }
            });
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
                                _userDialogs.AlertAsync("20% Battery Life Left, Please Recharge CS108 or Replace Freshly Charged CS108B");
                            //else if (batlow == ClassBattery.BATTERYLEVELSTATUS.LOW_17)
                            //    _userDialogs.AlertAsync("8% Battery Life Left, Please Recharge CS108 or Replace with Freshly Charged CS108B");

                            RaisePropertyChanged(() => labelVoltageTextColor);
                        }
                    }

                    switch (BleMvxApplication._config.BatteryLevelIndicatorFormat)
                    {
                        case 0:
                            _labelVoltage = "Battery " + voltage.ToString("0.000") + "v"; //			v
                            break;

                        default:
                            _labelVoltage = voltage.ToString("0.000") + "v " + ClassBattery.Voltage2Percent(voltage).ToString("0") + "%"; //			%
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
            InvokeOnMainThread(() =>
            {
                Page currentPage;

                Trace.Message("Receive Key Event");

                if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
                {
                    if (e.KeyDown)
                    {
                        if (!_InventoryScanning)
                            StartInventory();
                    }
                    else
                    {
                        StopInventory();
                    }
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
    }
}
