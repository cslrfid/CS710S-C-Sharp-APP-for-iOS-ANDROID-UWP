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
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelUCODEDNA : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string entryChallenge { get; set; }
        public string entryResponse { get; set; }
        public string entrySelectedKey0 { get; set; }       // 128 bits
        public string entrySelectedKey1 { get; set; }       // 16 bits

        public string labelResponseStatus { get; set; } = "";
        public string labelKey0Status { get; set; } = "";
        public string labelKey1Status { get; set; } = "";

        public ICommand OnReadKey0ButtonCommand { protected set; get; }
        public ICommand OnReadKey1ButtonCommand { protected set; get; }
        public ICommand OnWriteKey0ButtonCommand { protected set; get; }
        public ICommand OnWriteKey1ButtonCommand { protected set; get; }
        public ICommand OnHideButtonCommand { protected set; get; }
        public ICommand OnUnhideButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM1ButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM2ButtonCommand { protected set; get; }
        public ICommand OnActivateKey0ButtonCommand { protected set; get; }
        public ICommand OnActivateKey1ButtonCommand { protected set; get; }

        uint accessPwd;

        enum CURRENTOPERATION
        {
            READKEY0,
            READKEY1,
            WRITEKEY0,
            WRITEKEY1,
            ACTIVEKEY0,
            ACTIVEKEY1,
            UNKNOWN
        }

        CURRENTOPERATION _currentOperation = CURRENTOPERATION.UNKNOWN;

        public ViewModelUCODEDNA(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnReadKey0ButtonCommand = new Command(OnReadKey0ButtonButtonClick);
            OnReadKey1ButtonCommand = new Command(OnReadKey1ButtonButtonClick);
            OnWriteKey0ButtonCommand = new Command(OnWriteKey0ButtonButtonClick);
            OnWriteKey1ButtonCommand = new Command(OnWriteKey1ButtonButtonClick);
            OnHideButtonCommand = new Command(OnHideButtonButtonClick);
            OnUnhideButtonCommand = new Command(OnUnhideButtonButtonClick);
            OnActivateKey0ButtonCommand = new Command(OnActivateKey0ButtonButtonClick);
            OnActivateKey1ButtonCommand = new Command(OnActivateKey1ButtonButtonClick);
            OnAuthenticateTAM1ButtonCommand = new Command(OnAuthenticateTAM1ButtonButtonClick);
            OnAuthenticateTAM2ButtonCommand = new Command(OnAuthenticateTAM2ButtonButtonClick);
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            //BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
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
            entryChallenge = "FD5D8048F48DD09AAD22";
            entrySelectedKey0 = "";
            entrySelectedKey1 = "";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => entryChallenge);
            RaisePropertyChanged(() => entrySelectedKey0);
            RaisePropertyChanged(() => entrySelectedKey1);

            InitReader();
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

        // for singal tag read/write
        void InitReader()
        {
            BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);

            // Cancel Alkl Filter
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
        }

        void OnRandomKeyButtonButtonClick()
        {
            Random rnd = new Random();

            entrySelectedKey0 = "";
            entrySelectedKey1 = "";

            for (int cnt = 0; cnt < 8; cnt++)
            {
                entrySelectedKey0 += rnd.Next(0, 65535).ToString("X4");
                entrySelectedKey1 += rnd.Next(0, 65535).ToString("X4");
            }

            RaisePropertyChanged(() => entrySelectedKey0);
            RaisePropertyChanged(() => entrySelectedKey1);
        }

        void OnReadKey0ButtonButtonClick()
        {
            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            TagSelected();
            ReadKey0();
        }

        void OnReadKey1ButtonButtonClick()
        {
            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            TagSelected();
            ReadKey1();
        }

        void OnWriteKey0ButtonButtonClick()
        {
            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            TagSelected();
            WriteKey0();
        }

        void OnWriteKey1ButtonButtonClick()
        {
            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            TagSelected();
            WriteKey1();
        }

        void OnHideButtonButtonClick()
        {
            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagUntraceable.EPCLength = 0; // NXP AN11778 only have EPCLength functional 

            /* for Gen2V2 
            BleMvxApplication._reader.rfid.Options.TagUntraceable.Range = CSLibrary.Structures.UNTRACEABLE_RANGE.Normal;
            BleMvxApplication._reader.rfid.Options.TagUntraceable.User = CSLibrary.Structures.UNTRACEABLE_USER.View;
            BleMvxApplication._reader.rfid.Options.TagUntraceable.TID = CSLibrary.Structures.UNTRACEABLE_TID.HideNone;
            BleMvxApplication._reader.rfid.Options.TagUntraceable.EPC = CSLibrary.Structures.UNTRACEABLE_EPC.Show;
            BleMvxApplication._reader.rfid.Options.TagUntraceable.EPCLength = 0;
            BleMvxApplication._reader.rfid.Options.TagUntraceable.U = CSLibrary.Structures.UNTRACEABLE_U.AssertU;
            */

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_UNTRACEABLE);
        }

        void OnUnhideButtonButtonClick()
        {
            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagUntraceable.EPCLength = 6; // NXP AN11778 only have EPCLength functional
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_UNTRACEABLE);
        }

        void OnActivateKey0ButtonButtonClick()
        {
            _currentOperation = CURRENTOPERATION.ACTIVEKEY0;

            TagSelected();

            labelKey0Status = "A";
            RaisePropertyChanged(() => labelKey0Status);

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0xc8; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = CSLibrary.Tools.Hex.ToUshorts("E200");

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
        }

        void OnActivateKey1ButtonButtonClick()
        {
            _currentOperation = CURRENTOPERATION.ACTIVEKEY1;

            TagSelected();

            labelKey1Status = "A";
            RaisePropertyChanged(() => labelKey1Status);

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0xd8; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = CSLibrary.Tools.Hex.ToUshorts("E200");

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
        }

        void OnAuthenticateTAM1ButtonButtonClick()
        {
            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x60;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "0000" + entryChallenge;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void OnAuthenticateTAM2ButtonButtonClick()
        {
            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x78;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "2001" + entryChallenge + "00001100";
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void TagSelected()
        {
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
        }

        void ReadKey0 ()
        {
            _currentOperation = CURRENTOPERATION.READKEY0;

            labelKey0Status = "R";
            RaisePropertyChanged(() => labelKey0Status);

            BleMvxApplication._reader.rfid.Options.TagReadUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 0xc0;
            BleMvxApplication._reader.rfid.Options.TagReadUser.count = 8;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
        }

        void ReadKey1 ()
        {
            _currentOperation = CURRENTOPERATION.READKEY1;

            labelKey1Status = "R";
            RaisePropertyChanged(() => labelKey1Status);

            BleMvxApplication._reader.rfid.Options.TagReadUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 0xd0;
            BleMvxApplication._reader.rfid.Options.TagReadUser.count = 8;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
        }

        void WriteKey0 ()
        {
            RaisePropertyChanged(() => entrySelectedKey0);
            if (entrySelectedKey0.Length != 32)
            {
                _userDialogs.Alert("Key 0 Error, please input 128bit (32 hex)");
                return;
            }

            _currentOperation = CURRENTOPERATION.WRITEKEY0;

            labelKey0Status = "W";
            RaisePropertyChanged(() => labelKey0Status);

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0xc0; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 8; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = CSLibrary.Tools.Hex.ToUshorts(entrySelectedKey0);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
        }

        void WriteKey1 ()
        {
            RaisePropertyChanged(() => entrySelectedKey1);
            if (entrySelectedKey1.Length != 32)
            {
                _userDialogs.Alert("Key 1 Error, please input 128bit (32 hex)");
                return;
            }

            _currentOperation = CURRENTOPERATION.WRITEKEY1;

            labelKey1Status = "W";
            RaisePropertyChanged(() => labelKey1Status);

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0xd0; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 8; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = CSLibrary.Tools.Hex.ToUshorts(entrySelectedKey1);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.AUTHENTICATE)
            {
                if (e.success)
                {
                    entryResponse = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                    labelResponseStatus = "Ok";
                    RaisePropertyChanged(() => entryResponse);
                }
                else
                {
                    labelResponseStatus = "E";
                }
                RaisePropertyChanged(() => labelResponseStatus);
            }
            else if (e.access == CSLibrary.Constants.TagAccess.UNTRACEABLE)
            {
                if (e.success)
                {
                    _userDialogs.Alert("UNTRACEABLE command success!");
                }
                else
                {
                    _userDialogs.Alert("UNTRACEABLE command fail!!!");
                }
            }
            else if (e.access == CSLibrary.Constants.TagAccess.READ)
            {
                switch (e.bank)
                {
                    case CSLibrary.Constants.Bank.USER:
                        if (e.success)
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.READKEY0:
                                    entrySelectedKey0 = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToString();
                                    labelKey0Status = "O";
                                    RaisePropertyChanged(() => entrySelectedKey0);
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.READKEY1:
                                    entrySelectedKey1 = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToString();
                                    labelKey1Status = "O";
                                    RaisePropertyChanged(() => entrySelectedKey1);
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }
                        else
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.READKEY0:
                                    labelKey0Status = "E";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.READKEY1:
                                    labelKey1Status = "E";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }

                        break;
                }
            }
            else if (e.access == CSLibrary.Constants.TagAccess.WRITE)
            {
                switch (e.bank)
                {
                    case CSLibrary.Constants.Bank.UNTRACEABLE:
                        if (e.success)
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.ACTIVEKEY0:
                                    labelKey0Status = "O";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.ACTIVEKEY1:
                                    labelKey1Status = "O";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }
                        else
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.ACTIVEKEY0:
                                    labelKey0Status = "E";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.ACTIVEKEY1:
                                    labelKey1Status = "E";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }
                        break;

                    case CSLibrary.Constants.Bank.USER:
                        if (e.success)
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.WRITEKEY0:
                                case CURRENTOPERATION.ACTIVEKEY0:
                                    labelKey0Status = "O";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.WRITEKEY1:
                                case CURRENTOPERATION.ACTIVEKEY1:
                                    labelKey1Status = "O";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }
                        else
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.WRITEKEY0:
                                case CURRENTOPERATION.ACTIVEKEY0:
                                    labelKey0Status = "E";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.WRITEKEY1:
                                case CURRENTOPERATION.ACTIVEKEY1:
                                    labelKey1Status = "E";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }

                        break;
                }
            }

        }
    }
}
