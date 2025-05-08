using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelXerxesConfiguration : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string entryX28Text { get; set; }
        public string entryX29Text { get; set; }
        public string entryX2AText { get; set; }
        public string entryX2BText { get; set; }
        public string entryX2CText { get; set; }
        public string entryX2DText { get; set; }
        public string entryX2EText { get; set; }
        public string entryX2FText { get; set; }

        public ICommand OnReadButtonCommand { protected set; get; }
        public ICommand OnWriteButtonCommand { protected set; get; }

        uint accessPwd;

        uint readUSERRetryCnt = 0;

        public ViewModelXerxesConfiguration(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnReadButtonCommand = new Command(OnReadButtonButtonClick);
            OnWriteButtonCommand = new Command(OnWriteButtonButtonClick);
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
        }

        public override void ViewDisappearing()
        {
            BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            base.ViewDisappearing();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            entrySelectedEPC = BleMvxApplication._SELECT_EPC;
            entrySelectedPWD = "00000000";

            entryX28Text = "0";
            entryX29Text = "0.0";
            entryX2AText = "0.0";
            entryX2BText = "0";
            entryX2CText = "0";
            entryX2DText = "0";
            entryX2EText = "0";
            entryX2FText = "0";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => entryX28Text);
            RaisePropertyChanged(() => entryX29Text);
            RaisePropertyChanged(() => entryX2AText);
            RaisePropertyChanged(() => entryX2BText);
            RaisePropertyChanged(() => entryX2CText);
            RaisePropertyChanged(() => entryX2DText);
            RaisePropertyChanged(() => entryX2EText);
            RaisePropertyChanged(() => entryX2FText);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.READ)
			{
                switch (e.bank)
                {
                    case CSLibrary.Constants.Bank.USER:
                        if (e.success)
                        {
                            var entryUSER = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts();

                            entryX28Text = entryUSER[0].ToString("D");
                            entryX29Text = toFloat(entryUSER[1]).ToString("0.0#");
                            entryX2AText = toFloat(entryUSER[2]).ToString("0.0#");
                            entryX2BText = entryUSER[3].ToString("D");
                            entryX2CText = entryUSER[4].ToString("D");
                            entryX2DText = entryUSER[5].ToString("D");
                            entryX2EText = entryUSER[6].ToString("D");
                            entryX2FText = entryUSER[7].ToString("D");

                            RaisePropertyChanged(() => entryX28Text);
                            RaisePropertyChanged(() => entryX29Text);
                            RaisePropertyChanged(() => entryX2AText);
                            RaisePropertyChanged(() => entryX2BText);
                            RaisePropertyChanged(() => entryX2CText);
                            RaisePropertyChanged(() => entryX2DText);
                            RaisePropertyChanged(() => entryX2EText);
                            RaisePropertyChanged(() => entryX2FText);

                            _userDialogs.Alert("Read OK!");
                        }
                        else
						{
                            _userDialogs.Alert("Read FAIL!");
                        }
						break;
                }
            }

			if (e.access == CSLibrary.Constants.TagAccess.WRITE)
			{
				switch (e.bank)
				{
					case CSLibrary.Constants.Bank.USER:
						if (e.success)
						{
                            _userDialogs.Alert("Write OK!");
                        }
                        else
						{
                            _userDialogs.Alert("Write FAIL!");
						}
						break;
                }
            }
		}

        System.Threading.CancellationTokenSource cancelSrc;

        void OnReadButtonButtonClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                _userDialogs.Alert("Reader is busy now, please try later.");
                return;
            }

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();

            // EPC
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(BleMvxApplication._SELECT_EPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0x00;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)(BleMvxApplication._SELECT_EPC.Length * 4);
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            uint m_retry_cnt = 7;       // Max 7

            entryX28Text = "0";
            entryX29Text = "0.0";
            entryX2AText = "0.0";
            entryX2BText = "0";
            entryX2CText = "0";
            entryX2DText = "0";
            entryX2EText = "0";
            entryX2FText = "0";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => entryX28Text);
            RaisePropertyChanged(() => entryX29Text);
            RaisePropertyChanged(() => entryX2AText);
            RaisePropertyChanged(() => entryX2BText);
            RaisePropertyChanged(() => entryX2CText);
            RaisePropertyChanged(() => entryX2DText);
            RaisePropertyChanged(() => entryX2EText);
            RaisePropertyChanged(() => entryX2FText);

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            readUSERRetryCnt = m_retry_cnt;

            BleMvxApplication._reader.rfid.Options.TagReadUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 2; // m_readAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagReadUser.count = 8; // m_readAllBank.WordUser;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
        }

        void OnWriteButtonButtonClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                _userDialogs.Alert("Reader is busy now, please try later.");
                return;
            }

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();

            // EPC
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(BleMvxApplication._SELECT_EPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0x00;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)(BleMvxApplication._SELECT_EPC.Length * 4);
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            uint m_retry_cnt = 7;       // Max 7

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            readUSERRetryCnt = m_retry_cnt;

            UInt16 [] entryUSER = new UInt16[8];

            entryUSER[0] = Convert.ToUInt16(entryX28Text);
            entryUSER[1] = (UInt16)(fromFloat(float.Parse(entryX29Text)));
            entryUSER[2] = (UInt16)(fromFloat(float.Parse(entryX2AText)));
            entryUSER[3] = Convert.ToUInt16(entryX2BText);
            entryUSER[4] = Convert.ToUInt16(entryX2CText);
            entryUSER[5] = Convert.ToUInt16(entryX2DText);
            entryUSER[6] = Convert.ToUInt16(entryX2EText);
            entryUSER[7] = Convert.ToUInt16(entryX2FText);

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 2; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 8; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = entryUSER;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
        }

        // ignores the higher 16 bits
        public float toFloat(int hbits)
        {
            int mant = hbits & 0x03ff;            // 10 bits mantissa
            int exp = hbits & 0x7c00;            // 5 bits exponent
            if (exp == 0x7c00)                   // NaN/Inf
                exp = 0x3fc00;                    // -> NaN/Inf
            else if (exp != 0)                   // normalized value
            {
                exp += 0x1c000;                   // exp - 15 + 127
                if (mant == 0 && exp > 0x1c400)  // smooth transition
                    return BitConverter.ToSingle(BitConverter.GetBytes((hbits & 0x8000) << 16
                                                    | exp << 13 | 0x3ff), 0);
            }
            else if (mant != 0)                  // && exp==0 -> subnormal
            {
                exp = 0x1c400;                    // make it normal
                do
                {
                    mant <<= 1;                   // mantissa * 2
                    exp -= 0x400;                 // decrease exp by 1
                } while ((mant & 0x400) == 0); // while not normal
                mant &= 0x3ff;                    // discard subnormal bit
            }                                     // else +/-0 -> +/-0
            return BitConverter.ToSingle(BitConverter.GetBytes(          // combine all parts
                (hbits & 0x8000) << 16          // sign  << ( 31 - 15 )
                | (exp | mant) << 13), 0);         // value << ( 23 - 10 )
        }
        // returns all higher 16 bits as 0 for all results
        public int fromFloat(float fval)
        {
            int fbits = BitConverter.ToInt32(BitConverter.GetBytes(fval), 0);
            int sign = fbits >> 16 & 0x8000;          // sign only
            int val = (fbits & 0x7fffffff) + 0x1000; // rounded value

            if (val >= 0x47800000)               // might be or become NaN/Inf
            {                                     // avoid Inf due to rounding
                if ((fbits & 0x7fffffff) >= 0x47800000)
                {                                 // is or must become NaN/Inf
                    if (val < 0x7f800000)        // was value but too large
                        return sign | 0x7c00;     // make it +/-Inf
                    return sign | 0x7c00 |        // remains +/-Inf or NaN
                        (fbits & 0x007fffff) >> 13; // keep NaN (and Inf) bits
                }
                return sign | 0x7bff;             // unrounded not quite Inf
            }
            if (val >= 0x38800000)               // remains normalized value
                return sign | val - 0x38000000 >> 13; // exp - 127 + 15
            if (val < 0x33000000)                // too small for subnormal
                return sign;                      // becomes +/-0
            val = (fbits & 0x7fffffff) >> 23;  // tmp exp for subnormal calc
            return sign | ((fbits & 0x7fffff | 0x800000) // add subnormal bit
                 + (0x800000 >> val - 102)     // round depending on cut off
              >> 126 - val);   // div by 2^(1-(exp-127+15)) and >> 13 | exp=0
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
                await System.Threading.Tasks.Task.Delay(3000);
            }
        }

    }
}
