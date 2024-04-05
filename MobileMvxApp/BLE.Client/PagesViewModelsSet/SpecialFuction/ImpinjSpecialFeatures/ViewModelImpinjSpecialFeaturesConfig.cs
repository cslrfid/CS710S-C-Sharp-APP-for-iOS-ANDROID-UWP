using System;

using Acr.UserDialogs;
using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;
using MvvmCross.ViewModels;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BLE.Client.ViewModels
{
    public class ViewModelImpinjSpecialFeaturesConfig : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string entryAuthenticatedResultText { get; set; }
        public string entryAuthServerURLText { get; set; }
        public string entryVerificationemailText { get; set; }
        public string entryVerificationpasswordText { get; set; }
        public string entryVerificationResultText { get; set; }

        public bool buttonTAM1AuthenticateIsEnabled { get; set; }
        public bool buttonTAM2AuthenticateIsEnabled { get; set; }
        public bool buttonTagVerificationIsEnabled { get; set; }

        public ICommand OnAuthenticatedReadCommand { protected set; get; }
        public ICommand OnTAM2AuthenticateCommand { protected set; get; }
        public ICommand OnSentToServerCommand { protected set; get; }

        uint accessPwd;
        bool _TagVerificationProcess = false;

        public ViewModelImpinjSpecialFeaturesConfig(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnAuthenticatedReadCommand = new Command(OnAuthenticatedReadClick);
            OnTAM2AuthenticateCommand = new Command(OnTAM2AuthenticatedClick);
            OnSentToServerCommand = new Command(OnOnSentToServerClick);

            entryAuthServerURLText = BleMvxApplication._config.Impinj_AuthenticateServerURL;
            entryVerificationemailText = BleMvxApplication._config.Impinj_AuthenticateEmail;
            entryVerificationpasswordText = BleMvxApplication._config.Impinj_AuthenticatePassword;
            RaisePropertyChanged(() => entryVerificationemailText);
            RaisePropertyChanged(() => entryVerificationpasswordText);
            EnableAllButton();
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

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
        }

        void SetEvent(bool onoff)
        {
            BleMvxApplication._reader.rfid.ClearEventHandler();
            
            if (onoff)
                BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.AUTHENTICATE && e.success)
            {
                entryAuthenticatedResultText = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                RaisePropertyChanged(() => entryAuthenticatedResultText);

                if (_TagVerificationProcess)
                {
                    _TagVerificationProcess = false;
                    TagVerification();
                }
                else
                {
                    EnableAllButton();
                }
            }
            else
            {
                _userDialogs.ShowError("Authenticated Read ERROR!!!", 5000);
                EnableAllButton();
            }
        }

        void DisableAllButton ()
        {
            buttonTAM1AuthenticateIsEnabled = false;
            buttonTAM2AuthenticateIsEnabled = false;
            buttonTagVerificationIsEnabled = false;
            RaisePropertyChanged(() => buttonTAM1AuthenticateIsEnabled);
            RaisePropertyChanged(() => buttonTAM2AuthenticateIsEnabled);
            RaisePropertyChanged(() => buttonTagVerificationIsEnabled);
        }

        void EnableAllButton()
        {
            buttonTAM1AuthenticateIsEnabled = true;
            buttonTAM2AuthenticateIsEnabled = true;
            buttonTagVerificationIsEnabled = true;
            RaisePropertyChanged(() => buttonTAM1AuthenticateIsEnabled);
            RaisePropertyChanged(() => buttonTAM2AuthenticateIsEnabled);
            RaisePropertyChanged(() => buttonTagVerificationIsEnabled);
        }

        void OnAuthenticatedReadClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            DisableAllButton();
            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.password = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.CSI = 1;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x30;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "009ca53e55ea";
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.ResponseLen = 0x40;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void OnTAM2AuthenticatedClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            DisableAllButton();
            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.password = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.CSI = 1;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x30;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "049ca53e55ea";
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.ResponseLen = 0x80;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }


        public class RESTfulLoginDetail
        {
            public string email;
            public string password;
        }

        public class RESTfulTagVerifyDetail
        {
            public string tid;
            public string challenge;
            public string tagResponse;
        }

        public class RESTfulImpinjAuthenticationCommand
        {
            public RESTfulTagVerifyDetail[] tagVerify;
            public bool sendSignature = true;
            public bool sendSalt = true;
            public bool sendTime = true;
        }

        public class RESTfulTagValidityDetail
        {
            public string tid { get; set; }
            public bool tagValid { get; set; }
        }

        public class RESTfulTagValidityResponses
        {
            public List<RESTfulTagValidityDetail> tagValidity { get; set; }
        }

        async void OnOnSentToServerClick()
        {
            DisableAllButton();
            _TagVerificationProcess = true;

            entryVerificationResultText = "";
            RaisePropertyChanged(() => entryVerificationemailText);
            RaisePropertyChanged(() => entryVerificationpasswordText);
            RaisePropertyChanged(() => entryVerificationResultText);

            // Tag Verification Only support TAM1 Authenticated
            OnAuthenticatedReadClick();
        }

        async void TagVerification()
        {
            try
            {
                if (entryAuthenticatedResultText == null || entryAuthenticatedResultText.Length == 0)
                {
                    _userDialogs.ShowError("Authenticated Result CANNOT empty!!!", 3000);
                    return;
                }

                if (entryAuthenticatedResultText.Length != 16)
                {
                    _userDialogs.ShowError("ONLY SUPPORT TAM1 !!!", 5000);
                    return;
                }

                RaisePropertyChanged(() => entryAuthServerURLText);
                RaisePropertyChanged(() => entryVerificationemailText);
                RaisePropertyChanged(() => entryVerificationpasswordText);

                var token = await LoginCSLServer(entryVerificationemailText, entryVerificationpasswordText);

                if (token == null)
                {
                    _userDialogs.ShowError("User Name and password error, can not get token from server!!!", 3000);
                    return;
                }

                bool check;
                check = await VerifyTag(token, BleMvxApplication._SELECT_TID, "009ca53e55ea", entryAuthenticatedResultText);

                if (check)
                {
                    entryVerificationResultText = "Valid";
                    _userDialogs.ShowSuccess("Authenticate Result Valid !!!", 5000);
                }
                else
                {
                    entryVerificationResultText = "NOT Valid";
                    _userDialogs.ShowError("Authenticate Result NOT Valid !!!", 5000);
                }
            }
            finally 
            {
                RaisePropertyChanged(() => entryVerificationResultText);
                EnableAllButton();
            }
        }

        async System.Threading.Tasks.Task<string> LoginCSLServer(string email, string password)
        {
            try
            {
                string LoginAddress = entryAuthServerURLText + "/api/Auth/login";
                RESTfulLoginDetail logindata = new RESTfulLoginDetail();
                logindata.email = email;
                logindata.password = password;

                string JSONdata = Newtonsoft.Json.JsonConvert.SerializeObject(logindata);

                var uri1 = new Uri(string.Format(LoginAddress, string.Empty));
                var content1 = new StringContent(JSONdata, System.Text.Encoding.UTF8, "application/json");

                HttpClient client1 = new HttpClient();
                client1.MaxResponseContentBufferSize = 102400;

                HttpResponseMessage response1 = null;

                try
                {
                    response1 = await client1.PostAsync(uri1, content1);
                    if (response1.IsSuccessStatusCode)
                    {
                        var a = response1.Content;
                        var b = await a.ReadAsStringAsync();

                        return b;
                    }
                }
                catch (Exception ex1)
                {
                    var a = ex1.Message;
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }

            return null;
        }

        async System.Threading.Tasks.Task<bool> VerifyTag(string token, string tid, string challenge, string tagResponse)
        {
            try
            {
                string LoginAddress = entryAuthServerURLText + "/api/ImpinjAuthentication/authenticate";
                RESTfulTagVerifyDetail[] tagVerify = new RESTfulTagVerifyDetail[1];
                tagVerify[0] = new RESTfulTagVerifyDetail
                {
                    tid = tid,
                    challenge = challenge,
                    tagResponse = tagResponse
                };

                RESTfulImpinjAuthenticationCommand authenticationDetail = new RESTfulImpinjAuthenticationCommand
                {
                    tagVerify = tagVerify,
                    //sendSignature = token,
                    //sendSalt = "",
                    //sendTime = ""
                };


                string JSONdata = Newtonsoft.Json.JsonConvert.SerializeObject(authenticationDetail);

                var uri1 = new Uri(string.Format(LoginAddress, string.Empty));
                var content1 = new StringContent(JSONdata, System.Text.Encoding.UTF8, "application/json");

                HttpClient client1 = new HttpClient();
                client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client1.MaxResponseContentBufferSize = 102400;

                HttpResponseMessage response1 = null;

                try
                {
                    response1 = await client1.PostAsync(uri1, content1);
                    if (response1.IsSuccessStatusCode)
                    {
                        var a = response1.Content;
                        var b = await a.ReadAsStringAsync();

                        RESTfulTagValidityResponses responses = JsonConvert.DeserializeObject<RESTfulTagValidityResponses>(b);

                        return responses.tagValidity[0].tagValid;
                    }
                }
                catch (Exception ex1)
                {
                    var a = ex1.Message;
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }

            return false;
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
