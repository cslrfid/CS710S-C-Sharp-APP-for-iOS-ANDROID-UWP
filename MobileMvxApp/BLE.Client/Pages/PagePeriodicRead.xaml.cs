using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PCLStorage;
using MvvmCross.Forms.Views;
using BLE.Client.ViewModels;

namespace BLE.Client.Pages
{
    public partial class PagePeriodicRead : MvxContentPage<ViewModelPeriodicRead>
    {
        bool _started = false;
        uint _tagCount = 0;
        uint _rountCount = 0;

        DateTime _st;

        public PagePeriodicRead()
        {
            InitializeComponent();
            InventorySetting();
        }

        ~PagePeriodicRead()
        {
            BleMvxApplication._reader.rfid.StopOperation();
            BleMvxApplication._reader.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
        }

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING || !_started)
                return;

                _tagCount++;
            });
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                switch (e.state)
                {
                    case CSLibrary.Constants.RFState.IDLE:
                        Log_InventoryResult();
                        StartNextInventory();
                        break;
                }
            });
        }

        async void StartNextInventory ()
        {
            int a = int.Parse(entryNoReadTime.Text) * 1000;
            await Task.Delay(a);
            //await Task.Delay(60000);

            _tagCount = 0;
            if (_started)
                StartInventory();
            else
                BleMvxApplication._reader.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
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

        void InventorySetting()
        {
            BleMvxApplication._reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;

            // Setting 1
            //BleMvxApplication._reader.rfid.SetInventoryDuration((uint)BleMvxApplication._config.RFID_DWellTime);
            //BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power);
            SetConfigPower();

            // Setting 3
            BleMvxApplication._reader.rfid.SetDynamicQParms(BleMvxApplication._config.RFID_DynamicQParms);

            // Setting 4
            BleMvxApplication._reader.rfid.SetFixedQParms(BleMvxApplication._config.RFID_FixedQParms);

            // Setting 2
            BleMvxApplication._reader.rfid.SetOperationMode(CSLibrary.Constants.RadioOperationMode.CONTINUOUS);
            //BleMvxApplication._reader.rfid.SetOperationMode(BleMvxApplication._config.RFID_OperationMode);
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
                    BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
                    BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)(BleMvxApplication._PREFILTER_MASK_EPC.Length) * 4;
                }
                else
                {
                    BleMvxApplication._reader.rfid.Options.TagSelected.bank = (CSLibrary.Constants.MemoryBank)(BleMvxApplication._PREFILTER_Bank);
                    BleMvxApplication._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.Hex.ToBytes(BleMvxApplication._PREFILTER_MASK_EPC);
                    BleMvxApplication._reader.rfid.Options.TagSelected.MaskOffset = 0;
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
            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = false;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);
        }

        void StartInventory()
        {
            Log_CurrentBattery();
            _rountCount++;
            labelRound.Text = "Round " + _rountCount.ToString();
            Log_Message("Start Round " + _rountCount.ToString() + "," + ((double)BleMvxApplication._reader.notification.GetCurrentBatteryLevel() / 1000).ToString() + "v");
            //BleMvxApplication._reader.rfid.SetInventoryDuration(uint.Parse(entryReadTime.Text) * 1000);
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);

            Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(uint.Parse(entryReadTime.Text) * 1000), () =>
            {
                BleMvxApplication._reader.rfid.StopOperation();
                return false;
            });

        }

        public async void btnStartClicked(object sender, EventArgs e)
        {
            if (_started)
            {
                buttonStart.Text = "Start";
                //Log_CloseFile();
                _started = false;
            }
            else
            {
                _st = DateTime.Now;
                _recordsLog = "";
                editorRecordsLog.Text = "";
                _rountCount = 0;
                _tagCount = 0;
                buttonStart.Text = "Stop";
                Log_Start();
                //Log_OpenFile();
                // run once inventory

                BleMvxApplication._reader.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
                BleMvxApplication._reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
                BleMvxApplication._reader.rfid.OnStateChanged -= new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
                BleMvxApplication._reader.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);

                StartInventory();
                _started = true;
            }
        }

        public async void btnViewLogClicked(object sender, EventArgs e)
        {
            if (_started)
            {
                return;
            }

            //            IFolder rootFolder = FileSystem.Current.LocalStorage;
            //            IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
            //            IFile logFile = await sourceFolder.CreateFileAsync(BleMvxApplication._config.readerID + ".log", CreationCollisionOption.OpenIfExists);

            /*
            System.IO.Stream streamToRead = await logFile.OpenAsync(FileAccess.ReadAndWrite).ConfigureAwait(false);
            byte [] 
            streamToRead.Read ()



                _logWriteStream.WriteAsync(bufferArray, 0, bufferArray.Length).ConfigureAwait(false);

            */

            string a = _recordsLog;

            a += "1";
            //string message;
            //message = await _logFile.ReadAllTextAsync ();
        }

        private IFile _logFile;
        System.IO.Stream _logWriteStream;
        string _recordsLog;

        async void Log_OpenFile()
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
            _logFile = await sourceFolder.CreateFileAsync(BleMvxApplication._config.readerID + ".log", CreationCollisionOption.ReplaceExisting);
            _logWriteStream = await _logFile.OpenAsync(FileAccess.ReadAndWrite).ConfigureAwait(false);
            //Log_Start();
        }

        void Log_CloseFile()
        {
            try
            {
                //_logWriteStream.Flush();
                //_logWriteStream = null;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        void Log_Start()
        {
            string message = "Start Test";
            Log_Message(message);
        }

        void Log_Stop()
        {
            string message = "Stop Test";
            Log_Message(message);
        }

        void Log_CurrentBattery()
        {
            //string message = "CurrentBatteryLevel" + "," + ((double)BleMvxApplication._reader.notification.GetCurrentBatteryLevel() / 1000).ToString();
            //Log_Message(message);
        }

        void Log_InventoryResult()
        {
            //string message = "Round "+ _rountCount.ToString() + " Result" + "," + _tagCount.ToString() + "," + ((double)BleMvxApplication._reader.notification.GetCurrentBatteryLevel() / 1000).ToString() + "v";
            string message = "RND="+ _rountCount.ToString() + ",Tag=" + _tagCount.ToString() + "," + ((double)BleMvxApplication._reader.notification.GetCurrentBatteryLevel() / 1000).ToString() + "v" + ",ET=" + ((uint)((DateTime.Now - _st).TotalMinutes)).ToString();
            Log_Message(message);
        }

        async void Log_Message(string message)
        {
            try
            {
//                Device.BeginInvokeOnMainThread(() =>
                {
                    string addDateTimeMessage = DateTime.Now.ToString("dd/MM/yy HH:mm:ss") + "," + message + Environment.NewLine;
                    _recordsLog += addDateTimeMessage;
                    editorRecordsLog.Text = addDateTimeMessage + editorRecordsLog.Text;
                } //);
            }
            catch (Exception ex)
            {

            }
        }

    }
}