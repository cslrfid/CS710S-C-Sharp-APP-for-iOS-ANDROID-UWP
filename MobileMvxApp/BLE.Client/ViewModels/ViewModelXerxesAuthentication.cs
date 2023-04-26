/*
Copyright (c) 2018 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;

using System.Security.Cryptography;
using System.IO;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelXerxesAuthentication : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string entryChallenge { get; set; }
        public string entryResponse { get; set; }
        public string entrySelectedKey0 { get; set; }       // 128 bits
        public string entrySelectedKey1 { get; set; }       // 16 bits
        public string entryOffsetText { get; set; }
        public string entryProtectMode1Text { get; set; }
        public string entryProtectMode2Text { get; set; }

        public string labelResponseStatus { get; set; } = "";
        public string labelKey0Status { get; set; } = "";
        public string labelKey1Status { get; set; } = "";

        public string labelResult1Text { get; set; } = "";
        public string labelResult2Text { get; set; } = "";
        public string labelResult2DateText { get; set; } = "";
        public string labelResult3Text { get; set; } = "";
        public string labelResult3DateText { get; set; } = "";

        public bool switchEncryptionIsToggled { get; set; } = false;

        public bool switchDataValidityIsToggled { get; set; } = false;

        public string entryJsonServAddress { get; set; } = "";

        public ICommand OnSetKey1ButtonCommand { protected set; get; }
        public ICommand OnSetKey2ButtonCommand { protected set; get; }
        public ICommand OnWriteKey0ButtonCommand { protected set; get; }
        public ICommand OnWriteKey1ButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM1ButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM2ButtonCommand { protected set; get; }
        

        uint accessPwd;
        int _currentProcess;
        int protMode;
        
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

        public ViewModelXerxesAuthentication(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnSetKey1ButtonCommand = new Command(OnSetKey1ButtonButtonClick);
            OnSetKey2ButtonCommand = new Command(OnSetKey2ButtonButtonClick);
            OnWriteKey0ButtonCommand = new Command(OnWriteKey0ButtonButtonClick);
            OnWriteKey1ButtonCommand = new Command(OnWriteKey1ButtonButtonClick);
            OnAuthenticateTAM1ButtonCommand = new Command(OnAuthenticateTAM1ButtonButtonClick);
            OnAuthenticateTAM2ButtonCommand = new Command(OnAuthenticateTAM2ButtonButtonClick);

            entryOffsetText = "0";
            entryProtectMode1Text = "0";
            entryProtectMode2Text = "2";
            entryJsonServAddress = "http://localhost";

            SetEvent(true);
        }

        private void SetEvent(bool enable)
        {
            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.ClearEventHandler();

            if (enable)
            {
                BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            }
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            SetEvent(true);
        }

        public override void ViewDisappearing()
        {
            SetEvent(false);
            base.ViewDisappearing();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            entrySelectedEPC = BleMvxApplication._SELECT_EPC;
            entrySelectedPWD = "00000000";
            entryChallenge = "96564402375796C69664";
            entrySelectedKey0 = "";
            entrySelectedKey1 = "";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => entryChallenge);
            RaisePropertyChanged(() => entrySelectedKey0);
            RaisePropertyChanged(() => entrySelectedKey1);

            SetConfigPower();
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

        void OnSetKey1ButtonButtonClick()
        {
            entrySelectedKey0 = "000102030405060708090A0B0C0D0E0F";
            RaisePropertyChanged(() => entrySelectedKey0);
        }

        void OnSetKey2ButtonButtonClick()
        {
            entrySelectedKey1 = "2B7E151628AED2A6ABF7158809CF4F3C";
            RaisePropertyChanged(() => entrySelectedKey1);
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

        void OnAuthenticateTAM1ButtonButtonClick()
        {
            _currentProcess = 0;
            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            labelResult1Text = "Reading...";
            RaisePropertyChanged(() => labelResult1Text);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x60;

            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "0000" + entryChallenge;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM1Message(0, false, 0, 0, entryChallenge);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void OnAuthenticateTAM2ButtonButtonClick()
        {
            labelResponseStatus = "R";
            entryResponse = "";
            RaisePropertyChanged(() => labelResponseStatus);
            RaisePropertyChanged(() => entryResponse);

            labelResult2Text = "Reading...";
            labelResult2DateText = "";
            RaisePropertyChanged(() => labelResult2Text);
            RaisePropertyChanged(() => entryProtectMode1Text);
            RaisePropertyChanged(() => labelResult2DateText);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x78;

            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 1, entryChallenge, 0, 0, 1, int.Parse(entryProtectMode1Text));

            _currentProcess = 1;
            if (!switchEncryptionIsToggled && !switchDataValidityIsToggled)
            {
                protMode = 0;
            }
            else if (switchEncryptionIsToggled && !switchDataValidityIsToggled)
            {
                protMode = 1;
            }
            else if (!switchEncryptionIsToggled && switchDataValidityIsToggled)
            {
                protMode = 2;
            }
            else // if (switchEncryptionIsToggled && switchDataValidityIsToggled)
            {
                protMode = 3;
            }

            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 1, entryChallenge, 0, int.Parse(entryOffsetText), 1, protMode);
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 0, entryChallenge, 0, int.Parse(entryOffsetText), 1, protMode);
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        string TAM1Message(int authMethod, bool customerData, int RFU, int keyId, string challenge)
        {
            string msg = "";
            int value = 0;

            if (authMethod > 3)
                return "";

            if (RFU > 31)
                return "";

            if (keyId > 3)
                return "";

            if (challenge.Length != 20)
                return "";

            value = (authMethod << 14) | (customerData ? 1 : 0) | (RFU << 8) | (keyId);

            msg = value.ToString("X4");

            msg += challenge;

            return msg;
        }

        string TAM2Message (int authMethod, bool customerData, int RFU, int keyId, string challenge, int profile, int offset, int blockCount, int portMode)
        {
            string msg = "";
            int value = 0;

            if (authMethod > 3)
                return "";

            if (RFU > 31)
                return "";

            if (keyId > 3)
                return "";

            if (challenge.Length != 20)
                return "";

            if (profile > 15)
                return "";

            if (offset > 4095)
                return "";

            if (blockCount > 15)
                return "";

            if (portMode > 15)
                return "";

            value = (authMethod << 14) | (customerData ? 0x2000 : 0) | (RFU << 8) | (keyId);

            msg = value.ToString("X4");
            
            msg += challenge;

            msg += profile.ToString("X1");

            msg += offset.ToString("X3");

            msg += blockCount.ToString("X1");

            msg += portMode.ToString("X1");

            return msg;
        }


        void TagSelected()
        {
            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
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

            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.RESERVED;
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 0x10; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 8; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = CSLibrary.Tools.Hex.ToUshorts(entrySelectedKey0);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
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

            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.RESERVED;
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 0x18; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 8; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = CSLibrary.Tools.Hex.ToUshorts(entrySelectedKey1);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
            if (e.access == CSLibrary.Constants.TagAccess.AUTHENTICATE)
            {
                if (e.success)
                {
                    switch (_currentProcess)
                    {
                        case 0:
                            {
                                var Response = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                                string decChallenge;

                                if (TAM1Decrypt(Response, entrySelectedKey0, out decChallenge))
                                {
                                    labelResponseStatus = "Ok";
                                }
                                else
                                {
                                    labelResponseStatus = "Decode Error";
                                }
                                RaisePropertyChanged(() => entryResponse);

                                if (decChallenge == entryChallenge)
                                    labelResult1Text = "Challenge Match, Success !";
                                else
                                    labelResult1Text = "Challenge NOT match, Fail !";
                                RaisePropertyChanged(() => labelResult1Text);
                            }
                            break;
                        case 1:
                            {
                                var Response = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                                CSLibrary.Debug.WriteLine("Response Length : " + Response.Length);
                                byte[] DecResponse = null;
                                string result = "";

                                try
                                {
                                    switch (protMode)
                                    {
                                        case 0:
                                            {
                                                string decChallenge;
                                                string data;

                                                if (TAM2ProtMode0Decrypt(Response, entrySelectedKey0, out decChallenge, out data))
                                                {
                                                    labelResponseStatus = "Ok";
                                                    entryResponse = Response;
                                                }
                                                else
                                                {
                                                    labelResponseStatus = "Decode Error";
                                                    entryResponse = "";
                                                }
                                                RaisePropertyChanged(() => entryResponse);

                                                if (decChallenge == entryChallenge)
                                                {
                                                    labelResult2Text = "Challenge Match, Success !";
                                                    labelResult2DateText = data;
                                                }
                                                else
                                                {
                                                    labelResult2Text = "Challenge NOT match, Fail !";
                                                    labelResult2DateText = "";
                                                }
                                            }
                                            break;

                                        case 1:
                                            {
                                                string decChallenge;
                                                string data;

                                                if (TAM2ProtMode1Decrypt(Response, entrySelectedKey0, out decChallenge, out data))
                                                {
                                                    labelResponseStatus = "Ok";
                                                        entryResponse = Response;
                                                    }
                                                    else
                                                {
                                                    labelResponseStatus = "Decode Error";
                                                        entryResponse = "";
                                                    }
                                                    RaisePropertyChanged(() => entryResponse);

                                                if (decChallenge == entryChallenge)
                                                {
                                                    labelResult2Text = "Challenge Match, Success !";
                                                    labelResult2DateText = data;
                                                }
                                                else
                                                {
                                                    labelResult2Text = "Challenge NOT match, Fail !";
                                                    labelResult2DateText = "";
                                                }
                                            }
                                            break;

                                        case 2:
                                            {
                                                string decChallenge;
                                                string data;

                                                if (TAM2ProtMode2Decrypt(Response, entrySelectedKey0, out decChallenge, out data))
                                                {
                                                    labelResponseStatus = "Ok";
                                                        entryResponse = Response;
                                                    }
                                                    else
                                                {
                                                    labelResponseStatus = "Decode Error";
                                                        entryResponse = "";
                                                    }
                                                    RaisePropertyChanged(() => entryResponse);

                                                if (decChallenge == entryChallenge)
                                                {
                                                    labelResult2Text = "Challenge Match, Success !";
                                                    labelResult2DateText = data;
                                                }
                                                else
                                                {
                                                    labelResult2Text = "Challenge NOT match, Fail !";
                                                    labelResult2DateText = "";
                                                }
                                            }
                                            break;

                                        case 3:
                                            {
                                                string decChallenge;
                                                string data;

                                                if (TAM2ProtMode3Decrypt(Response, entrySelectedKey0, out decChallenge, out data))
                                                {
                                                    labelResponseStatus = "Ok";
                                                        entryResponse = Response;
                                                    }
                                                    else
                                                {
                                                    labelResponseStatus = "Decode Error";
                                                        entryResponse = "";
                                                    }
                                                    RaisePropertyChanged(() => entryResponse);

                                                if (decChallenge == entryChallenge)
                                                {
                                                    labelResult2Text = "Challenge Match, Success !";
                                                    labelResult2DateText = data;
                                                }
                                                else
                                                {
                                                    labelResult2Text = "Challenge NOT match, Fail !";
                                                    labelResult2DateText = "";
                                                }
                                            }
                                            break;
                                        }

                                    }
                                catch (Exception ex)
                                {
                                }

                                RaisePropertyChanged(() => entryResponse);
                                RaisePropertyChanged(() => labelResponseStatus);
                                RaisePropertyChanged(() => labelResult2Text);
                                RaisePropertyChanged(() => labelResult2DateText);
                            }

                                break;
                        }
                }
                else
                {
                    labelResponseStatus = "E";

                    switch (_currentProcess)
                    {
                        case 0:
                            labelResult1Text = "Read error";
                            RaisePropertyChanged(() => labelResult1Text);
                            break;

                        case 1:
                            labelResult2Text = "Read error";
                            labelResult2DateText = "";
                            RaisePropertyChanged(() => labelResult2Text);
                            RaisePropertyChanged(() => labelResult2DateText);
                            break;
                        }
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

                    case CSLibrary.Constants.Bank.SPECIFIC:
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
            });
        }


        public static byte[] ToByteArray(String hexString)
        {
            byte[] retval = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
                retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return retval;
        }


        // return decode success
        bool TAM1Decrypt(string toDecrypt, string key, out string Challenge)
        {
            try
            {
                var result = ASEDecrypt(ToByteArray(toDecrypt), ToByteArray(key), CipherMode.ECB);
                Challenge = CSLibrary.Tools.Hex.ToString(result, 6, 10);

                return true;
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("ASE decode error : " + ex.Message);
            }

            Challenge = null;
            return false;
        }

        bool TAM2ProtMode0Decrypt(string toDecrypt, string key, out string Challenge, out string Data)
        {
            try
            {
                byte[] Decrypt = ToByteArray(toDecrypt);
                var result = ASEDecrypt(Decrypt, ToByteArray(key), CipherMode.ECB);
                Challenge = CSLibrary.Tools.Hex.ToString(result, 6, 10);
                Data = CSLibrary.Tools.Hex.ToString(Decrypt, 16, 16);

                return true;
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("ASE decode error : " + ex.Message);
            }

            Challenge = Data = null;
            return false;
        }

        bool TAM2ProtMode1Decrypt(string toDecrypt, string key, out string Challenge, out string Data)
        {
            try
            {
                byte[] Decrypt = ToByteArray(toDecrypt);
                var result = ASEDecrypt(Decrypt, ToByteArray(key), CipherMode.ECB);
                var result1 = ASEDecrypt(Decrypt, ToByteArray(key), CipherMode.CBC);

                Challenge = CSLibrary.Tools.Hex.ToString(result, 6, 10);
                Data = CSLibrary.Tools.Hex.ToString(result1, 16, 16);

                return true;
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("ASE decode error : " + ex.Message);
            }

            Challenge = Data = null;
            return false;
        }

        bool TAM2ProtMode2Decrypt(string toDecrypt, string key, out string Challenge, out string Data)
        {
            try
            {
                toDecrypt = toDecrypt.Substring(0, 64);

                byte[] Decrypt = ToByteArray(toDecrypt);
                var result = ASEDecrypt(ToByteArray(toDecrypt), ToByteArray(key), CipherMode.ECB);
                Challenge = CSLibrary.Tools.Hex.ToString(result, 6, 10);
                Data = CSLibrary.Tools.Hex.ToString(Decrypt, 16, 16);

                return true;
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("ASE decode error : " + ex.Message);
            }

            Challenge = Data = null;
            return false;
        }

        bool TAM2ProtMode3Decrypt(string toDecrypt, string key, out string Challenge, out string Data)
        {
            try
            {
                toDecrypt = toDecrypt.Substring(0, 64);

                byte[] Decrypt = ToByteArray(toDecrypt);
                var result = ASEDecrypt(Decrypt, ToByteArray(key), CipherMode.ECB);
                var result1 = ASEDecrypt(Decrypt, ToByteArray(key), CipherMode.CBC);

                Challenge = CSLibrary.Tools.Hex.ToString(result, 6, 10);
                Data = CSLibrary.Tools.Hex.ToString(result1, 16, 16);

                return true;
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("ASE decode error : " + ex.Message);
            }

            Challenge = Data = null;
            return false;
        }

        byte[] ASEDecrypt(byte[] toDecrypt, byte[] key, CipherMode mode)
        {
            try
            {
                SymmetricAlgorithm crypt = Aes.Create();
                crypt.Key = key;
                crypt.Mode = mode;
                crypt.Padding = PaddingMode.None;

                using (MemoryStream memoryStream = new MemoryStream(toDecrypt))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        byte[] decryptedBytes = new byte[toDecrypt.Length];
                        cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                        return decryptedBytes;
                    }
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("ASE Decrypt Error : " + ex.Message);
            }

            return null;
        }
    }
}
