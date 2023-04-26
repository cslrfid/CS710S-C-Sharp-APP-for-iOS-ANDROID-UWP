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
    public class ViewModelBlockWrite : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        public string editorSelectedEPCText { get; set; }
        public string buttonBankText { get; set; }
        public string buttonSizeText { get; set; }
        public string buttonPaddingText { get; set; }
        public string entryReadChunkSizeText { get; set; }
        public string entryOffsetText { get; set; }
        public string entryLengthText { get; set; }
        public string buttonResultText { get; set; }

        public ICommand buttonBlockWriteCommand { protected set; get; }
        public ICommand buttonReadVerifyCommand { protected set; get; }
        public ICommand buttonViewReadDataCommand { protected set; get; }
        public ICommand buttonBlockWritewOffsetnCountCommand { protected set; get; }

        DateTime _startingTime;
        UInt16 _CurrentPadding;
        UInt16 _RemainWriteSize;
        UInt16 _RemainReadSize;
        UInt16 _ReadChunkSize = 48;

        string[] _bankOptions = new string[] { "Bank3 (User Bank)", "Bank1 (EPC Bank)" };
        string[] _sizeOptions = new string[] { "4K bit", "8K bit" };
        string[] _paddingOptions = new string[] { "repeat 55AA", "repeat AA55", "repeat 0000", "repeat FFFF", "repeat 0001", "repeat 0002", "repeat 0004", "repeat 0008", "repeat 0010", "repeat 0020", "repeat 0040", "repeat 0080", "repeat 0100", "repeat 0200", "repeat 0400", "repeat 0800", "repeat 1000", "repeat 2000", "repeat 4000", "repeat 8000", "repeat FFFE", "repeat FFFD", "repeat FFFB", "repeat FFF7", "repeat FFEF", "repeat FFDF", "repeat FFBF", "repeat FF7F", "repeat FEFF", "repeat FDFF", "repeat FBFF", "repeat F7FF", "repeat EFFF", "repeat DFFF", "repeat BFFF", "repeat 7FFF" };
        UInt16[] _paddingValue = new UInt16[] { 0x55AA, 0xAA55, 0x0000, 0xFFFF, 0x0001, 0x0002, 0x0004, 0x0008, 0x0010, 0x0020, 0x0040, 0x0080, 0x0100, 0x0200, 0x0400, 0x0800, 0x1000, 0x2000, 0x4000, 0x8000, 0xFFFE, 0xFFFD, 0xFFFB, 0xFFF7, 0xFFEF, 0xFFDF, 0xFFBF, 0xFF7F, 0xFEFF, 0xFDFF, 0xFBFF, 0xF7FF, 0xEFFF, 0xDFFF, 0xBFFF, 0x7FFF };

        public ViewModelBlockWrite(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            buttonBlockWriteCommand = new Command(buttonBlockWriteClicked);
            buttonReadVerifyCommand = new Command(buttonReadVerifyClicked);
            buttonViewReadDataCommand = new Command(buttonViewReadDataClicked);
            buttonBlockWritewOffsetnCountCommand = new Command(buttonBlockWritewOffsetnCount);

            editorSelectedEPCText = BleMvxApplication._SELECT_EPC;
            buttonBankText = _bankOptions[0];
            buttonSizeText = _sizeOptions[0];
            buttonPaddingText = _paddingOptions[0];
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
                RaisePropertyChanged(() => buttonSizeText);
                RaisePropertyChanged(() => buttonPaddingText);
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
                                int i;
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

                                for (i = 0; i < data.Length; i++)
                                {
                                    if (data[i] != _CurrentPadding)
                                    {
                                        buttonResultText = "Read OK, Verify Fail : Offset " + (BleMvxApplication._reader.rfid.Options.TagRead.offset + i).ToString();
                                        break;
                                    }
                                }

                                CSLibrary.Debug.WriteLine("3");
                                if (i == data.Length)
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

                if (e.access == CSLibrary.Constants.TagAccess.WRITE)
                {
                    switch (e.bank)
                    {
                        case CSLibrary.Constants.Bank.SPECIFIC: // Block write bank
                            if (!e.success)
                            {
                                buttonResultText = "Block Write Test Fail : Offset " + BleMvxApplication._reader.rfid.Options.TagBlockWrite.offset.ToString() + " Length " + BleMvxApplication._reader.rfid.Options.TagBlockWrite.count.ToString();
                                break;
                            }
                            else
                            {
                                if (_RemainWriteSize == 0)
                                {
                                    buttonResultText = "Write Test success time " + (DateTime.Now - _startingTime).TotalSeconds.ToString("F2") + "s";
                                    CSLibrary.Debug.WriteLine("Write Test Finish" + DateTime.Now + "/" + _startingTime);
                                }
                                else
                                {
                                    BleMvxApplication._reader.rfid.Options.TagBlockWrite.offset += BleMvxApplication._reader.rfid.Options.TagBlockWrite.count;
                                    BleMvxApplication._reader.rfid.Options.TagBlockWrite.count = _RemainWriteSize;

                                    TurnBlockWriteSize();
                                    FullPadding();
                                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_BLOCK_WRITE);
                                }
                            }
                            break;
                    }
                }
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

        async void buttonBlockWriteClicked()
        {
            UpdatePage();
            UInt16 dataWordSize = (Array.IndexOf(_sizeOptions, buttonSizeText) == 0 ? (UInt16)256 : (UInt16)512);

            BlockWrite(0, dataWordSize);
        }

        async void buttonReadVerifyClicked()
        {
            BleMvxApplication._LargeContent = "";

            UpdatePage();
            int paddingType = Array.IndexOf(_paddingOptions, buttonPaddingText);

            _ReadChunkSize = UInt16.Parse(entryReadChunkSizeText);
            _RemainReadSize = (UInt16)(((Array.IndexOf(_sizeOptions, buttonSizeText)) == 1) ? 512 : 256);
            _CurrentPadding = _paddingValue[paddingType];

            SelectTag();

            switch (Array.IndexOf(_bankOptions, buttonBankText))
            {
                case 0:
                    BleMvxApplication._reader.rfid.Options.TagRead.bank = CSLibrary.Constants.MemoryBank.USER;
                    break;

                default:
                    BleMvxApplication._reader.rfid.Options.TagRead.bank = CSLibrary.Constants.MemoryBank.BANK1;
                    break;
            }
            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = 0;
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 0; // 0
            BleMvxApplication._reader.rfid.Options.TagRead.count = _ReadChunkSize; // max 253 word
            _RemainReadSize -= _ReadChunkSize;

            buttonResultText = buttonSizeText + " Reading...";
            UpdatePage();
            _startingTime = DateTime.Now;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }

        async void buttonViewReadDataClicked()
        {
            //ShowViewModel<ViewModelViewPage>(new MvxBundle());
            _navigation.Navigate<ViewModelViewPage>(new MvxBundle());
        }

        async void buttonBlockWritewOffsetnCount()
        {
            UpdatePage();
            BlockWrite(UInt16.Parse(entryOffsetText), UInt16.Parse(entryLengthText));
        }

        void BlockWrite(UInt16 offset, UInt16 count)
        {
            _CurrentPadding = _paddingValue[Array.IndexOf(_paddingOptions, buttonPaddingText)];

            SelectTag();

            switch (Array.IndexOf(_bankOptions, buttonBankText))
            {
                case 0:
                    BleMvxApplication._reader.rfid.Options.TagBlockWrite.bank = CSLibrary.Constants.MemoryBank.USER;
                    break;

                default:
                    BleMvxApplication._reader.rfid.Options.TagBlockWrite.bank = CSLibrary.Constants.MemoryBank.BANK1;
                    break;
            }
            BleMvxApplication._reader.rfid.Options.TagBlockWrite.flags = CSLibrary.Constants.SelectFlags.SELECT;
            BleMvxApplication._reader.rfid.Options.TagBlockWrite.accessPassword = 0;
            BleMvxApplication._reader.rfid.Options.TagBlockWrite.offset = offset;
            BleMvxApplication._reader.rfid.Options.TagBlockWrite.count = count;
            BleMvxApplication._reader.rfid.Options.TagBlockWrite.retryCount = 31;

            TurnBlockWriteSize();
            FullPadding();

            CSLibrary.Debug.WriteLine("Block Write Test Start");
            buttonResultText = "Block Writing...";
            UpdatePage();
            _startingTime = DateTime.Now;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_BLOCK_WRITE);
        }

        void TurnBlockWriteSize()
        {
            _RemainWriteSize = 0;

            if (BleMvxApplication._reader.rfid.Options.TagBlockWrite.offset < 256)
            { // first 4k bank
                int lastPostition = BleMvxApplication._reader.rfid.Options.TagBlockWrite.offset + BleMvxApplication._reader.rfid.Options.TagBlockWrite.count;

                if (lastPostition > 256)
                {
                    _RemainWriteSize += (UInt16)(BleMvxApplication._reader.rfid.Options.TagBlockWrite.count - (256 - BleMvxApplication._reader.rfid.Options.TagBlockWrite.offset));
                    BleMvxApplication._reader.rfid.Options.TagBlockWrite.count -= _RemainWriteSize;
                }
            }

            if (BleMvxApplication._reader.rfid.Options.TagBlockWrite.count > 255)
            {
                _RemainWriteSize += (UInt16)(BleMvxApplication._reader.rfid.Options.TagBlockWrite.count - 255);
                BleMvxApplication._reader.rfid.Options.TagBlockWrite.count = 255;
            }
        }

        void FullPadding()
        {
            UInt16[] data = new UInt16[BleMvxApplication._reader.rfid.Options.TagBlockWrite.count];

            for (int i = 0; i < BleMvxApplication._reader.rfid.Options.TagBlockWrite.count; i++)
                data[i] = _CurrentPadding;

            BleMvxApplication._reader.rfid.Options.TagBlockWrite.data = data;
        }

    }
}
