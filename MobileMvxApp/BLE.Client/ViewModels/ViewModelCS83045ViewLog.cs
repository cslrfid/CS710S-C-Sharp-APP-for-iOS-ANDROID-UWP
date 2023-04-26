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
using Newtonsoft.Json.Serialization;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelCS83045ViewLog : BaseViewModel
    {
        public class ColdChainTagTempLogInfoViewModel : BindableBase
        {
            private string _Index;
            public string Index { get { return this._Index; } set { this.SetProperty(ref this._Index, value); } }

            private string _Time;
            public string Time { get { return this._Time; } set { this.SetProperty(ref this._Time, value); } }

            private string _Temp;
            public string Temp { get { return this._Temp; } set { this.SetProperty(ref this._Temp, value); } }
        }

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------

        private ObservableCollection<ColdChainTagTempLogInfoViewModel> _TagInfoList = new ObservableCollection<ColdChainTagTempLogInfoViewModel>();
        public ObservableCollection<ColdChainTagTempLogInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

        #endregion

        int _ProcessState = 0;
        UInt16 _ReadChunkSize = 48;  // read word size (even number)

        public ViewModelCS83045ViewLog(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            _userDialogs.ShowLoading("Please wait until get log finish!!" );

            SetEvent(true);
            _ProcessState = 0;
            GetLog();
        }

        ~ViewModelCS83045ViewLog()
        {
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
        }

        public override void ViewDisappearing()
        {
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

            // Key Button event handler
            BleMvxApplication._reader.notification.ClearEventHandler();

            if (enable)
            {
                // RFID event handler
                BleMvxApplication._reader.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
                BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
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
                for (uint cnt = BleMvxApplication._reader.rfid.GetAntennaPort() - 1; cnt >= 0; cnt--)
                {
                    BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[cnt], cnt);
                }
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

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (!e.success)
            {
                GetLog();
            }

            // process result
            switch (_ProcessState)
            {
                // run next step pcocess
                case 0: 
                case 1:
                case 2:
                case 7:
                case 8:
                    System.Threading.Thread.Sleep(500);
                    _ProcessState++;
                    GetLog();
                    break;

                case 3:
                    {
                        UInt16[] TagData = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts();

                        StartTime = UnixTime(TagData[0] << 16 | TagData[1]);
                        Interval = TagData[2];
                        TempOffset = 0.25 * (TagData[3]);
                        Total = TagData[4];
                        //label17.Text = "Total : " + Total.ToString();
                        if (Total >= 10752)
                        {
                            //label17.Text += " (MemoryBank Full)";
                        }


                        // read first data set
                        _ProcessState++;
                        GetLog();
                    }
                    break;

                case 4:
                    {
                        System.Threading.Thread.Sleep(2500);
                        _ProcessState++;
                        GetLog();
                    }
                    break;

                case 5:
                    { 
                        if ((BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts()[0] & 0x04) != 0x0000)
                        {
                            _ProcessState++;

                            ReadRecord = (UInt16)(Total - ReadOffset);

                            if (ReadRecord > 366)
                                ReadRecord = 366;

                            ReadUInt16 = (UInt16)((ReadRecord + 1) / 2);

                            ReadRecordOffset = 0;

                            if (ReadUInt16 > _ReadChunkSize)
                                ReadRecordSize = _ReadChunkSize;
                            else
                                ReadRecordSize = ReadUInt16;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(500);
                        }
                        GetLog();
                    }
                    break;

                case 6:
                    InvokeOnMainThread(() =>
                    {
                        UInt16[] DataPtr = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts();

                        for (int cnt = 0; cnt < DataPtr.Length; cnt++)
                        {
                            ColdChainTagTempLogInfoViewModel ins;
                            double temp;

                            temp = ((DataPtr[cnt] >> 8) * 0.25) - TempOffset;
                            ins = new ColdChainTagTempLogInfoViewModel();
                            ins.Index = (TagInfoList.Count + 1).ToString();
                            ins.Time = StartTime.ToString();
                            ins.Temp = temp.ToString();
                            TagInfoList.Add(ins);

                            StartTime = StartTime.AddSeconds(Interval);

                            if (TagInfoList.Count != Total)
                            {
                                temp = ((DataPtr[cnt] & 0xff) * 0.25) - TempOffset;
                                ins = new ColdChainTagTempLogInfoViewModel();
                                ins.Index = (TagInfoList.Count + 1).ToString();
                                ins.Time = StartTime.ToString();
                                ins.Temp = temp.ToString();
                                TagInfoList.Add(ins);

                                StartTime = StartTime.AddSeconds(Interval);
                            }
                        }

                        if ((ReadRecordOffset + ReadRecordSize) >= ReadUInt16)
                        {
                            ReadOffset += (UInt16)(ReadUInt16 * 2);
                            if (ReadOffset < Total)
                                _ProcessState = 4;
                            else
                                _ProcessState = 7;
                        }
                        else
                        {
                            ReadUInt16 -= ReadRecordSize;
                            ReadRecordOffset += ReadRecordSize;

                            if (ReadUInt16 > _ReadChunkSize)
                                ReadRecordSize = _ReadChunkSize;
                            else
                                ReadRecordSize = ReadUInt16;
                        }
                        GetLog();
                    }); 
                    break;
            }
        }

        DateTime StartTime = DateTime.Now;
        UInt16 Interval = 0;
        double TempOffset = 0;
        UInt16 Total = 0;
        UInt16 ReadOffset = 0;
        UInt16 ReadRecord = 0;
        UInt16 ReadUInt16 = 0;
        UInt16 ReadRecordOffset = 0;
        UInt16 ReadRecordSize = 0;

        private void GetLog()
        {
            switch (_ProcessState)
            {
                case 0:
                    {
                        // Select Tag
                        BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(BleMvxApplication._SELECT_EPC);
                        BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
                        BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

                        BleMvxApplication._reader.rfid.Options.TagReadUser.accessPassword = 0;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = 0;

                        // Write 240 to 0xa600
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 240;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0xa600;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 1:
                    {
                        // Read 264
                        BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 264;
                        BleMvxApplication._reader.rfid.Options.TagReadUser.count = 1;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
                    }
                    break;

                case 2:
                    {
                        // write 264
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 264;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = (UInt16)(BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts()[0] & 0xFFFEU);
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 3:
                    {
                        BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 0; // Temp Sensor Calibration Word
                        BleMvxApplication._reader.rfid.Options.TagReadUser.count = 5;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
                    }
                    break;

                case 4:
                    {
                        //while (ReadOffset < Total)
                        //{
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 260;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 2;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[2];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0x000a;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[1] = ReadOffset;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 5:
                    {
                        BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 260; // Temp Sensor Calibration Word
                        BleMvxApplication._reader.rfid.Options.TagReadUser.count = 1;  // 183 * 2 records will be read
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
                    }
                    break;

                case 6:
                    {
                        BleMvxApplication._reader.rfid.Options.TagReadUser.offset = (UInt16)(5 + ReadRecordOffset);
                        BleMvxApplication._reader.rfid.Options.TagReadUser.count = ReadRecordSize;
                        //ReadOffset += (UInt16)(ReadUInt16 * 2);

                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
                    }
                    break;

                    case 7:
                    { 
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 240;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1;
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = new UInt16[1];
                        BleMvxApplication._reader.rfid.Options.TagWriteUser.pData[0] = 0x0000;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
                    }
                    break;

                case 8:
                    // Dummy read
                    {
                        BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 240;
                        BleMvxApplication._reader.rfid.Options.TagReadUser.count = 1;
                        BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
                    }
                    break;

                case 9:
                    _userDialogs.HideLoading();
                    break;
            }
        }

            /*
                    private void FormColdChainViewLog_Load(object sender, EventArgs e)
                    {
                        DateTime StartTime = DateTime.Now;
                        UInt16 Interval = 0;
                        double TempOffset = 0;
                        UInt16 Total = 0;

                        UInt16 ReadOffset = 0;
                        UInt16 ReadRecord = 0;
                        UInt16 ReadUInt16 = 0;

                        listView5.Clear();

                        this.listView5.Columns.Add(this.columnHeader26);
                        this.listView5.Columns.Add(this.columnHeader24);
                        this.listView5.Columns.Add(this.columnHeader25);

                        Program.ReaderXP.Options.TagSelected.flags = SelectMaskFlags.ENABLE_TOGGLE;
                        Program.ReaderXP.Options.TagSelected.bank = MemoryBank.EPC;
                        //Comment:If enable PC lock, please make sure you are not using Higgs3 Tag. Otherwise, write will fail
                        Program.ReaderXP.Options.TagSelected.epcMask = new S_MASK(textBox14.Text);
                        Program.ReaderXP.Options.TagSelected.epcMaskLength = (uint)Program.ReaderXP.Options.TagSelected.epcMask.Length * 8;
                        if (Program.ReaderXP.StartOperation(Operation.TAG_SELECTED, true) != Result.OK)
                            return;

                        System.Threading.Thread.Sleep(100);

                        Program.ReaderXP.Options.TagWriteUser.offset = 240;
                        Program.ReaderXP.Options.TagWriteUser.count = 1;
                        Program.ReaderXP.Options.TagWriteUser.pData = new UInt16[1];
                        Program.ReaderXP.Options.TagWriteUser.pData[0] = 0xa600;
                        if (Program.ReaderXP.StartOperation(Operation.TAG_WRITE_USER, true) != Result.OK)
                            return;

                        Program.ReaderXP.Options.TagReadUser.retryCount = 7;
                        Program.ReaderXP.Options.TagReadUser.offset = 264;
                        Program.ReaderXP.Options.TagReadUser.count = 1;
                        Program.ReaderXP.Options.TagReadUser.pData = new S_DATA(new UInt16[1]);
                        if (Program.ReaderXP.StartOperation(Operation.TAG_READ_USER, true) != Result.OK)
                            return;

                        System.Threading.Thread.Sleep(100);

                        Program.ReaderXP.Options.TagWriteUser.offset = 264;
                        Program.ReaderXP.Options.TagWriteUser.count = 1;
                        Program.ReaderXP.Options.TagWriteUser.pData = new UInt16[1];
                        Program.ReaderXP.Options.TagWriteUser.pData[0] = (UInt16)(Program.ReaderXP.Options.TagReadUser.pData.ToUshorts()[0] & 0xFFFEU);

                        if (Program.ReaderXP.StartOperation(Operation.TAG_WRITE_USER, true) != Result.OK)
                            return;

                        Program.ReaderXP.Options.TagReadUser.accessPassword = 0;
                        Program.ReaderXP.Options.TagReadUser.retryCount = 7;
                        Program.ReaderXP.Options.TagReadUser.offset = 0; // Temp Sensor Calibration Word
                        Program.ReaderXP.Options.TagReadUser.count = 5;
                        Program.ReaderXP.Options.TagReadUser.pData = new S_DATA(new byte[5]);
                        if (Program.ReaderXP.StartOperation(Operation.TAG_READ_USER, true) != Result.OK)
                        {
                            MessageBox.Show("Read Data Length Error");
                            return;
                        }

                        UInt16[] TagData = Program.ReaderXP.Options.TagReadUser.pData.ToUshorts();

                        StartTime = UnixTime(TagData[0] << 16 | TagData[1]);
                        Interval = TagData[2];
                        TempOffset = 0.25 * (TagData[3]);
                        Total = TagData[4];
                        label17.Text = "Total : " + Total.ToString();
                        if (Total >= 10752)
                            label17.Text += " (MemoryBank Full)";

                        while (ReadOffset < Total)
                        {
                            Program.ReaderXP.Options.TagWriteUser.retryCount = 7;
                            Program.ReaderXP.Options.TagWriteUser.accessPassword = 0x00000000;
                            Program.ReaderXP.Options.TagWriteUser.offset = 260;
                            Program.ReaderXP.Options.TagWriteUser.count = 2;
                            Program.ReaderXP.Options.TagWriteUser.pData = new UInt16[2];
                            Program.ReaderXP.Options.TagWriteUser.pData[0] = 0x000a;
                            Program.ReaderXP.Options.TagWriteUser.pData[1] = ReadOffset;
                            if (Program.ReaderXP.StartOperation(Operation.TAG_WRITE_USER, true) != Result.OK)
                                return;

                            System.Threading.Thread.Sleep(2500);

                            Program.ReaderXP.Options.TagReadUser.offset = 260; // Temp Sensor Calibration Word
                            Program.ReaderXP.Options.TagReadUser.count = 1;  // 183 * 2 records will be read
                            Program.ReaderXP.Options.TagReadUser.pData = new S_DATA(new byte[1]);
                            while (true)
                            {
                                if (Program.ReaderXP.StartOperation(Operation.TAG_READ_USER, true) != Result.OK)
                                {
                                    MessageBox.Show("Read Data Length Error");
                                    return;
                                }

                                if ((Program.ReaderXP.Options.TagReadUser.pData.ToUshorts()[0] & 0x04) != 0x0000)
                                    break;

                                System.Threading.Thread.Sleep(100);
                            }

                            ReadRecord = (UInt16)(Total - ReadOffset);

                            if (ReadRecord > 366)
                                ReadRecord = 366;

                            ReadUInt16 = (UInt16)((ReadRecord + 1) / 2);

                            Program.ReaderXP.Options.TagReadUser.offset = 5;
                            Program.ReaderXP.Options.TagReadUser.count = ReadUInt16;
                            Program.ReaderXP.Options.TagReadUser.pData = new S_DATA(new byte[ReadUInt16]);
                            ReadOffset += (UInt16)(ReadUInt16 * 2);

                            if (Program.ReaderXP.StartOperation(Operation.TAG_READ_USER, true) != Result.OK)
                            {
                                MessageBox.Show("Read Data Length Error");
                                return;
                            }

                            UInt16[] DataPtr = Program.ReaderXP.Options.TagReadUser.pData.ToUshorts();

                            for (int cnt = 0; cnt < ReadUInt16; cnt++)
                            {
                                ListViewItem ins;
                                double temp;

                                temp = ((DataPtr[cnt] >> 8) * 0.25) - TempOffset;
                                ins = new ListViewItem((listView5.Items.Count + 1).ToString());
                                ins.SubItems.Add(StartTime.ToString());
                                ins.SubItems.Add(temp.ToString());
                                listView5.Items.Add(ins);

                                StartTime = StartTime.AddSeconds(Interval);

                                if (listView5.Items.Count == Total)
                                    break;

                                temp = ((DataPtr[cnt] & 0xff) * 0.25) - TempOffset;
                                ins = new ListViewItem((listView5.Items.Count + 1).ToString());
                                ins.SubItems.Add(StartTime.ToString());
                                ins.SubItems.Add(temp.ToString());
                                listView5.Items.Add(ins);

                                StartTime = StartTime.AddSeconds(Interval);
                            }
                        }

                        FormColdChainFeatures.ColdChain_DataLog("[Read Temp Log]," + textBox14.Text + "," + "Current Time," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        for (int cnt = 0; cnt < listView5.Items.Count; cnt++)
                            FormColdChainFeatures.ColdChain_DataLog(listView5.Items[cnt].SubItems[0].Text + "," + listView5.Items[cnt].SubItems[1].Text + "," + listView5.Items[cnt].SubItems[2].Text);

                        Program.ReaderXP.Options.TagWriteUser.offset = 240;
                        Program.ReaderXP.Options.TagWriteUser.count = 1;
                        Program.ReaderXP.Options.TagWriteUser.pData = new UInt16[1];
                        Program.ReaderXP.Options.TagWriteUser.pData[0] = 0x0000;
                        if (Program.ReaderXP.StartOperation(Operation.TAG_WRITE_USER, true) != Result.OK)
                            return;

                        // Dummy read
                        {
                            S_DATA value = new S_DATA("0000");
                            //FormColdChainFeatures.ReadUserData(240, 1, ref value);

                            Program.ReaderXP.Options.TagReadUser.offset = 240;
                            Program.ReaderXP.Options.TagReadUser.count = 1;
                            Program.ReaderXP.Options.TagReadUser.pData = new S_DATA(new byte[ReadUInt16]);
                            Program.ReaderXP.StartOperation(Operation.TAG_READ_USER, true);
                        }
                    }
            */

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
