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
using MvvmCross.Navigation;

namespace BLE.Client.ViewModels
{
    public class ViewModelRead : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        public string editorSelectedEPCText { get; set; }
        public string buttonBankText { get; set; }
        public string entryReadChunkSizeText { get; set; }
        public string entryOffsetText { get; set; }
        public string entryLengthText { get; set; }
        public string buttonResultText { get; set; }

        public ICommand buttonReadVerifyCommand { protected set; get; }
        public ICommand buttonViewReadDataCommand { protected set; get; }

        DateTime _startingTime;
        UInt16 _RemainWriteSize;
        UInt16 _RemainReadSize;
        UInt16 _ReadChunkSize = 48;

        string[] _bankOptions = new string[] { "Bank0 (Reserved)", "Bank1 (EPC)", "Bank2 (TID)", "Bank3 (User)" };
        //string[] _bankOptions = new string[] { "Bank3 (User Bank)", "Bank1 (EPC Bank)" };

        public ViewModelRead(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            buttonReadVerifyCommand = new Command(buttonReadVerifyClicked);
            buttonViewReadDataCommand = new Command(buttonViewReadDataClicked);

            editorSelectedEPCText = BleMvxApplication._SELECT_EPC;
            buttonBankText = _bankOptions[3];
            entryReadChunkSizeText = "48";
            entryOffsetText = "0";
            entryLengthText = "256";
            BleMvxApplication._LargeContent = "";

            UpdatePage();

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
        }

        public override void ViewDisappearing()
        {
            base.ViewDisappearing();

            BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        void UpdatePage()
        {
            InvokeOnMainThread(() =>
            {
                RaisePropertyChanged(() => editorSelectedEPCText);
                RaisePropertyChanged(() => buttonBankText);
                RaisePropertyChanged(() => entryReadChunkSizeText);
                RaisePropertyChanged(() => entryOffsetText);
                RaisePropertyChanged(() => entryLengthText);
                RaisePropertyChanged(() => buttonResultText);
            });
        }

        async void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                if (e.access == CSLibrary.Constants.TagAccess.READ)
                {
                    switch (e.bank)
                    {
                        case CSLibrary.Constants.Bank.SPECIFIC:
                            if (!e.success)
                            {
                                buttonResultText = "Read Test Fail : Offset " + BleMvxApplication._reader.rfid.Options.TagRead.offset.ToString();
                                break;
                            }
                            else
                            {
                                UInt16[] data = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToUshorts();
                                BleMvxApplication._LargeContent += BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();

                                CSLibrary.Debug.WriteLine("Read Test : Offset " + BleMvxApplication._reader.rfid.Options.TagRead.offset.ToString() + " Result : " + (e.success ? "Success" : "Fail"));

                                CSLibrary.Debug.WriteLine("1");
                                if (data.Length != BleMvxApplication._reader.rfid.Options.TagRead.count)
                                {
                                    buttonResultText = "Read size error : Offset " + (BleMvxApplication._reader.rfid.Options.TagRead.offset).ToString() + " Count " + data.Length.ToString();
                                    break;
                                }
                                CSLibrary.Debug.WriteLine("2");

                                CSLibrary.Debug.WriteLine("3");
                                if (_RemainReadSize == 0)
                                {
                                    buttonResultText = "Read/Verify Test success time " + (DateTime.Now - _startingTime).TotalSeconds.ToString("F2") + "s";
                                    CSLibrary.Debug.WriteLine("Read Test Finish " + DateTime.Now + "/" + _startingTime);
                                    CSLibrary.Debug.WriteLine("4");
                                }
                                else
                                {
                                    BleMvxApplication._reader.rfid.Options.TagRead.offset += _ReadChunkSize;
                                    if (_RemainReadSize > _ReadChunkSize)
                                    {
                                        BleMvxApplication._reader.rfid.Options.TagRead.count = _ReadChunkSize;
                                        _RemainReadSize -= _ReadChunkSize;
                                    }
                                    else
                                    {
                                        BleMvxApplication._reader.rfid.Options.TagRead.count = _RemainReadSize;
                                        _RemainReadSize = 0;
                                    }
                                    buttonResultText = "Reading... Offset " + BleMvxApplication._reader.rfid.Options.TagRead.offset.ToString();
                                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
                                    CSLibrary.Debug.WriteLine("5");
                                }
                            }
                            break;
                    }
                }
                CSLibrary.Debug.WriteLine("6");

                UpdatePage();
            });
        }

        void SelectTag()
        {
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(/*m_record.pc.ToString() + */editorSelectedEPCText);

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
        }

        async void buttonReadVerifyClicked()
        {
            BleMvxApplication._LargeContent = "";

            UpdatePage();

            _ReadChunkSize = UInt16.Parse(entryReadChunkSizeText);
            _RemainReadSize = UInt16.Parse(entryLengthText);

            SelectTag();

            BleMvxApplication._reader.rfid.Options.TagRead.bank = (CSLibrary.Constants.MemoryBank)Array.IndexOf(_bankOptions, buttonBankText);
            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = 0;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = UInt16.Parse(entryOffsetText); // 0
            if (_RemainReadSize <= _ReadChunkSize)
            {
                BleMvxApplication._reader.rfid.Options.TagRead.count = _RemainReadSize; // max 253 word
                _RemainReadSize = 0;
            }
            else
            {
                BleMvxApplication._reader.rfid.Options.TagRead.count = _ReadChunkSize; // max 253 word
                _RemainReadSize -= _ReadChunkSize;
            }

            buttonResultText = " Reading...";
            UpdatePage();
            _startingTime = DateTime.Now;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        async void buttonViewReadDataClicked()
        {
            //ShowViewModel<ViewModelViewPage>(new MvxBundle());
            _navigation.Navigate<ViewModelViewPage>(new MvxBundle());
        }

    }
}
