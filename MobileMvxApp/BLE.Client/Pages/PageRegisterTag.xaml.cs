using System;
using System.Threading.Tasks;
using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
	public partial class PageRegisterTag : MvxContentPage<ViewModelRegisterTag>
	{
        string[] stringSlectMaskBankSelectionList = new string[] { "EPC", "TID" };
        string[] stringWriteBankSelectionList = new string[] { "USER", "EPC" };
        uint _tagCount = 0;
        uint _successTagCount = 0;
        UInt16 _automatic = 0;
        bool _alreadySelect = false;



        public PageRegisterTag()
		{
            InitializeComponent();
            buttonSelectBank.Text = stringSlectMaskBankSelectionList[0];
            buttonWriteBank.Text = stringWriteBankSelectionList[1];

            //BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[0]);
            SetConfigPower();
            BleMvxApplication._reader.rfid.SetCurrentLinkProfile((uint)BleMvxApplication._config.RFID_Profile);

            //entryMask.Text = BleMvxApplication._config.SELECT_EPC;
        }

        ~PageRegisterTag()
        {
            BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            StopScanBarcode();
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

        void Barcode_CaptureCompleted(object sender, CSLibrary.Barcode.BarcodeEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                switch (e.MessageType)
                {
                    case CSLibrary.Barcode.Constants.MessageType.DEC_MSG:
                        entryBarcode.Text = ((CSLibrary.Barcode.Structures.DecodeMessage)e.Message).pchMessage;
                        StopScanBarcode();
                        if (_automatic == 1)
                            buttonWriteClicked(this, null);
                        else
                            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);
                        break;
                    case CSLibrary.Barcode.Constants.MessageType.ERR_MSG:
                        //UpdateUI(null, String.Format("Barcode Returned: {0}", e.ErrorMessage));
                        break;
                }
            });
        }

        public async void buttonBarcodereadClicked(object sender, EventArgs e)
        {
            StartScanBarcode();
        }

        public async void buttonSelectBankClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(null, "Cancel", null, stringSlectMaskBankSelectionList);

            if (answer != null && answer !="Cancel")
                buttonSelectBank.Text = answer;
        }
        
        public async void buttonSelectClicked(object sender, EventArgs e)
        {
            try
            { 
                BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
                BleMvxApplication._reader.rfid.Options.TagSelected.bank = (buttonSelectBank.Text == stringSlectMaskBankSelectionList[0]) ? CSLibrary.Constants.MemoryBank.EPC : CSLibrary.Constants.MemoryBank.TID;//CSLibrary.Constants.MemoryBank.EPC;
                labelAlgorithm.TextColor = Color.Blue;
                labelQvalueText.TextColor = Color.Blue;
                labelTagPopulation.TextColor = Color.Blue;
                BleMvxApplication._reader.rfid.Options.TagSelected.Qvalue = (uint)(Math.Log((uint.Parse(labelTagPopulation.Text) * 2), 2)) + 1  ;
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entryMask.Text);
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = UInt32.Parse(labelSelectOffset.Text);
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)(entryMask.Text.Length) * 4;

                if (labelAlgorithm.Text.Substring(0, 1) == "D")
                {
                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTEDDYNQ);
                }
                else
                {
                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
                }

                entryMask.TextColor = Color.Black;
                _alreadySelect = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Something error, please verify value", null, "OK");
            }
        }

        public async void buttonWriteBankClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(null, "Cancel", null, stringWriteBankSelectionList);

            if (answer != null && answer !="Cancel")
                buttonWriteBank.Text = answer;
        }

        public async void buttonWriteClicked(object sender, EventArgs e)
        {
            if (!_alreadySelect)
            {
                await DisplayAlert("Warring", "Please Press 'Select' button!!!", "OK");
                return;
            }

            BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);

            if (buttonWriteBank.Text == stringWriteBankSelectionList[0])
                WriteUSER();
            else
                WriteEPC();
        }

        void WriteEPC()
        {
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.accessPassword = 0;
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.offset = UInt16.Parse(labelWriteOffset.Text) ;
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.count = CSLibrary.Tools.Hex.GetWordCount(entryBarcode.Text);
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.epc = new CSLibrary.Structures.S_EPC(entryBarcode.Text); // Write HEX

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_EPC);
        }

        void WriteUSER()
        {
            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = 0;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = UInt16.Parse(labelWriteOffset.Text);
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = CSLibrary.Tools.Hex.GetWordCount(entryBarcode.Text);
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = CSLibrary.Tools.Hex.ToUshorts(entryBarcode.Text);   // Write ASCII

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (e.access != CSLibrary.Constants.TagAccess.WRITE)
                    return;

                if (e.success)
                {
                    labelLastWriteResult.Text = "Write Success";
                    _successTagCount++;
                }
                else
                    labelLastWriteResult.Text = "Write Fail";

                BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);

                Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);
                _tagCount++;
                labelTagCount.Text = _tagCount.ToString();
                labelSuccessTagCount.Text = _successTagCount.ToString();

                switch (_automatic)
                {
                    case 1:
                        StartAutomaticRegisterTag();
                        break;

                    case 2:
                        StartAutomatic1RegisterTag();
                        break;
                }
            });
        }

        public async void buttonAutomaticClicked(object sender, EventArgs e)
        {
            if (_automatic != 0)
            {
                _automatic = 0;
                buttonAutomatic1.IsEnabled = true;
                buttonAutomatic.Text = "Automatic Cycle 1, 2, 3 with Step 2 Fixed";
                StopScanBarcode();
            }
            else
            {
                if (!_alreadySelect)
                {
                    await DisplayAlert("Warring", "Please Press 'Select' button!!!", "OK");
                    return;
                }

                _tagCount = 0;
                _successTagCount = 0;

                labelTagCount.Text = _tagCount.ToString();
                labelSuccessTagCount.Text = _successTagCount.ToString();

                _automatic = 1;
                buttonAutomatic.Text = "Stop";
                buttonAutomatic1.IsEnabled = false;
                buttonBarcodereadClicked(this, null);
            }
        }

        public async void buttonAutomatic1Clicked(object sender, EventArgs e)
        {
            if (_automatic != 0)
            {
                _automatic = 0;
                buttonAutomatic.IsEnabled = true;
                buttonAutomatic1.Text = "Automatic Cycle 2, 3 using same UPC from Step 1";
                StopScanBarcode();
            }
            else
            {
                if (!_alreadySelect)
                {
                    await DisplayAlert("Warring", "Please Press 'Select' button!!!", "OK");
                    return;
                }

                _tagCount = 0;
                _successTagCount = 0;

                labelTagCount.Text = _tagCount.ToString();
                labelSuccessTagCount.Text = _successTagCount.ToString();

                _automatic = 2;
                buttonAutomatic1.Text = "Stop";
                buttonAutomatic.IsEnabled = false;
                buttonWriteClicked(this, null);
            }
        }


        async void StartAutomaticRegisterTag ()
        {
            //await Task.Delay(2000);
            buttonBarcodereadClicked(this, null);
        }

        async void StartAutomatic1RegisterTag()
        {
            await Task.Delay(1500);
            buttonWriteClicked(this, null);
        }

        public async void onentryMaskTextChanged(object sender, EventArgs e)
        {
            entryMask.TextColor = Color.Red;
        }

        bool _barcodeScanning = false;

        void StartScanBarcode ()
        {
            BleMvxApplication._reader.barcode.OnCapturedNotify -= new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Barcode_CaptureCompleted);
            BleMvxApplication._reader.barcode.OnCapturedNotify += new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Barcode_CaptureCompleted);
            //if (BleMvxApplication._reader.barcode.State == CSLibrary.Barcode.Constants.BarcodeState.IDLE)
            _barcodeScanning = true;
            BleMvxApplication._reader.barcode.Start();
        }

        void StopScanBarcode ()
        {
            //if (BleMvxApplication._reader.barcode.State != CSLibrary.Barcode.Constants.BarcodeState.IDLE)
            _barcodeScanning = false;
            BleMvxApplication._reader.barcode.Stop();
            BleMvxApplication._reader.barcode.OnCapturedNotify -= new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Barcode_CaptureCompleted);
        }

    }
}
