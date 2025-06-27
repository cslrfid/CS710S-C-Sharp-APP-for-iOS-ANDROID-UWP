using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Net;

using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Prism.Mvvm;

using System.Net.Http;
using System.Net.Http.Headers;

using Plugin.Share;
using Plugin.Share.Abstractions;
using MvvmCross.ViewModels;
//using Plugin.Permissions.Abstractions;
using TagDataTranslation;
using CSLibrary.Barcode.Constants;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace BLE.Client.ViewModels
{
    public class ViewModelBarcodeandRFIDInventory : BaseViewModel
    {
        public class BarcodeandRFIDTagInfoViewModel : BindableBase
        {
            private string _INDEX;
            public string INDEX { get { return this._INDEX; } set { this.SetProperty(ref this._INDEX, value); } }

            private string _BARCODE;
            public string BARCODE { get { return this._BARCODE; } set { this.SetProperty(ref this._BARCODE, value); } }

            private string _EPC;
            public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }

            public DateTime timeOfRead;
            public float RSSI = 0;

            public BarcodeandRFIDTagInfoViewModel()
            {
            }
            public BarcodeandRFIDTagInfoViewModel(int index)
            {
                INDEX = index.ToString();
            }
        }

        public class RESTful
        {
            public string index;
            public string barcode;
            public string epc;
            public string datetime;
        }

        private readonly IUserDialogs _userDialogs;
        //readonly IPermissions _permissions;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand { protected set; get; }
        public ICommand OnShareDataCommand { protected set; get; }
        public ICommand OnSaveDataCommand { protected set; get; }

        private ObservableCollection<BarcodeandRFIDTagInfoViewModel> _TagInfoList = new ObservableCollection<BarcodeandRFIDTagInfoViewModel>();
        public ObservableCollection<BarcodeandRFIDTagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

        public bool _InventoryScanning = false;
        public bool _KeyDown = false;

        private string _startInventoryButtonText = "Start Inventory";
        public string startInventoryButtonText { get { return _startInventoryButtonText; } }

        private string _labelVoltage = "";
        public string labelVoltage { get { return _labelVoltage; } }
        public string labelVoltageTextColor { get { return BleMvxApplication._batteryLow ? "Red" : "Black"; } }

        private int _ListViewRowHeight = -1;
        public int ListViewRowHeight { get { return _ListViewRowHeight; } }

        public string _DebugMessage = "";
        public string DebugMessage { get { return _DebugMessage; } }

        public string _entryPowerText = "100";
        public string entryPowerText { get { return _entryPowerText; } set { _entryPowerText = value; } }

        bool _cancelVoltageValue = false;

        int _index = 0;

        #endregion

        public ViewModelBarcodeandRFIDInventory(IAdapter adapter, IUserDialogs userDialogs/* , IPermissions permissions */) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);
            OnShareDataCommand = new Command(ShareDataButtonClick);
            OnSaveDataCommand = new Command(SaveDataButtonClick);

            RaisePropertyChanged(() => entryPowerText);

            InventorySetting();
        }

        ~ViewModelBarcodeandRFIDInventory()
        {
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            SetEvent(true);
            BleMvxApplication._reader.barcode.FastBarcodeMode(true);
        }

        public override void ViewDisappearing()
        {
            _InventoryScanning = false;
            StopInventory();
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();

            BleMvxApplication._reader.barcode.FastBarcodeMode(false);

            // don't turn off event handler is you need program work in sleep mode.
            SetEvent(false);
            base.ViewDisappearing();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
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

        private void ClearClick()
        {
            InvokeOnMainThread(() =>
            {
                lock (TagInfoList)
                    ClearClickImmediately();
            });
            _index = 0;
        }

        private void ClearClickImmediately()
        {
            TagInfoList.Clear();

            _DebugMessage = "";
            RaisePropertyChanged(() => DebugMessage);
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

            BleMvxApplication._reader.rfid.SetPowerSequencing(0);
            BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[0]);
        }

        void StartInventory()
        {
            if (_InventoryScanning)
            {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
                return;
            }

            _InventoryScanning = true;
            _startInventoryButtonText = "Stop Inventory";
            RaisePropertyChanged(() => startInventoryButtonText);

            _ListViewRowHeight = 40 + (int)(BleMvxApplication._reader.rfid.Options.TagRanging.multibanks * 10);
            RaisePropertyChanged(() => ListViewRowHeight);

            RaisePropertyChanged(() => entryPowerText);
            BleMvxApplication._reader.rfid.SetPowerLevel(int.Parse(entryPowerText));

            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOn(CSLibrary.BarcodeReader.VIBRATORMODE.INVENTORYON, BleMvxApplication._config.RFID_VibrationTime);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_RANGING);

            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);
            _cancelVoltageValue = true;

            if (TagInfoList.Count == 0 || TagInfoList[0].BARCODE != null)
            {
                _index++;
                TagInfoList.Insert(0, new BarcodeandRFIDTagInfoViewModel(_index));
            }
        }

        async void InventoryStopped()
        {
            if (!_InventoryScanning)
                return;

            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();
            _InventoryScanning = false;
            _startInventoryButtonText = "Start Inventory";
            RaisePropertyChanged(() => startInventoryButtonText);

            if (TagInfoList[0].BARCODE != null && TagInfoList[0].EPC == null)
            {
                TagInfoList[0].EPC = "Missing RFID";
            }
        }

        async void StopInventory ()
        {
            if (!_InventoryScanning)
                return;

            BleMvxApplication._reader.rfid.StopOperation();
            if (BleMvxApplication._config.RFID_Vibration)
                BleMvxApplication._reader.barcode.VibratorOff();
            _InventoryScanning = false;
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

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING)
                return;

            //Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(2);

            AddOrUpdateTagData(e.info);
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
                                    _userDialogs.Alert("Last error : 0x" + BleMvxApplication._reader.rfid.LastMacErrorCode.ToString("X4") + System.Environment.NewLine + CSLibrary.CS710SErrorCodes.GetErrorDescription((int)BleMvxApplication._reader.rfid.LastMacErrorCode));
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

        private void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info)
        {
            InvokeOnMainThread(() =>
            {
                lock (TagInfoList)
                {
                    int i = 0;

                    if (TagInfoList[i].BARCODE == null)
                        return;

                    if (TagInfoList[i].EPC == null)
                        Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(3);

                    if (TagInfoList[i].RSSI < info.rssi)
                    {
                        string epcstr = info.epc.ToString();
                        TagInfoList[i].EPC = epcstr;
                        TagInfoList[i].timeOfRead = DateTime.Now;
                        TagInfoList[i].RSSI = info.rssi;
                    }
                }
            });
        }

        private void AddOrUpdateBarcodeData(CSLibrary.Barcode.Structures.DecodeMessage decodeInfo)
        {
            if (decodeInfo == null)
                return;

            InvokeOnMainThread(() =>
            {
                lock (TagInfoList)
                {
                    int i = 0;

                    if (TagInfoList[i].BARCODE != null)
                        if (TagInfoList[i].BARCODE != decodeInfo.pchMessage)
                            if (TagInfoList[i].EPC != null)
                            {
                                _index++;
                                TagInfoList.Insert(0, new BarcodeandRFIDTagInfoViewModel(_index));
                            }

                    TagInfoList[i].BARCODE = decodeInfo.pchMessage;
                    TagInfoList[i].timeOfRead = DateTime.Now;

                    Trace.Message("EPC Data = {0}", decodeInfo.pchMessage);
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
                            _labelVoltage = voltage.ToString("0.000") + "v"; //			v
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

#region -------------------- Barcode Scan -------------------

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

        void Linkage_CaptureCompleted(object sender, CSLibrary.Barcode.BarcodeEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                switch (e.MessageType)
                {
                    case CSLibrary.Barcode.Constants.MessageType.DEC_MSG:
                        AddOrUpdateBarcodeData((CSLibrary.Barcode.Structures.DecodeMessage)e.Message);
                        //Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);
                        break;

                    case CSLibrary.Barcode.Constants.MessageType.ERR_MSG:
                        break;
                }
            });
        }

#endregion


#region Key_event

        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            InvokeOnMainThread(() =>
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
                        if (_KeyDown == true)
                            StopInventory();
                        _KeyDown = false;
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

        string GetJsonData()
        {
            try
            {
                List<RESTful> data = new List<RESTful>();

                foreach (var tagitem in _TagInfoList)
                {
                    RESTful item = new RESTful();
                    item.index = tagitem.INDEX;
                    item.barcode = tagitem.BARCODE;
                    item.epc = tagitem.EPC;
                    item.datetime = tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss.fff");
                    data.Add(item);
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
                    CSVdata += tagitem.INDEX + ",";
                    CSVdata += tagitem.BARCODE + ",";
                    CSVdata += tagitem.EPC + ",";
                    CSVdata += tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss.fff");
                    CSVdata += System.Environment.NewLine;
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
                    CSVdata += "=\"" + tagitem.INDEX + "\",";
                    CSVdata += "=\"" + tagitem.BARCODE + "\",";
                    CSVdata += "=\"" + tagitem.EPC + "\",";
                    CSVdata += tagitem.timeOfRead.ToString("yyyy/MM/dd HH:mm:ss.fff");
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
            string documents;
            string filename;
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

                        /*                        while (await _permissions.CheckPermissionStatusAsync<Plugin.Permissions.StoragePermission>() != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                                                {
                                                    await _permissions.RequestPermissionAsync<Plugin.Permissions.StoragePermission>();
                                                }
                        */

                        var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                        if (status != PermissionStatus.Granted)
                        {
                            status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                        }

                        filename = "InventoryData-" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "." + fileExtName;
                        DependencyService.Get<IExternalStorage>().SaveTextFileToDocuments(filename, Text, BleMvxApplication._config.RFID_ShareFormat);
                    }
                    break;

                default:
                    {
                        documents = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        filename = System.IO.Path.Combine(documents, "InventoryData-" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "." + fileExtName);
                        System.IO.File.WriteAllText(filename, Text);
                    }
                    break;
            }

            //_userDialogs.AlertAsync("File saved, please check file in Ducuments folder : " + documents);
            _userDialogs.AlertAsync("File saved, please check file in Documents folder");

            return true;
        }

#if NETSTANDARD2_0
        private static bool SelfSignedForLocalhost(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            // For HTTPS requests to this specific host, we expect this specific certificate.
            // In practice, you'd want this to be configurable and allow for multiple certificates per host, to enable
            // seamless certificate rotations.
            return sender is HttpWebRequest httpWebRequest
                    && httpWebRequest.RequestUri.Host == "localhost"
                    && certificate is X509Certificate2 x509Certificate2
                    && x509Certificate2.Thumbprint == "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    && sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors;
        }
#endif
    }
}