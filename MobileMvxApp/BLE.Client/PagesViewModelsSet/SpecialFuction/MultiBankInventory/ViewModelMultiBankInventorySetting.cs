using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross;


using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace BLE.Client.ViewModels
{
    public class ViewModelMultiBankInventorySetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        public ICommand OnOKButtonCommand { protected set; get; }

        //string a = BLE.Client.Pages.PageMultiBankInventorySetting._bankSelectionItems [0];

        public bool switchMultiBank1Enable { get { return BleMvxApplication._config.RFID_MBI_MultiBank1Enable; } set { BleMvxApplication._config.RFID_MBI_MultiBank1Enable = value; } }
        public string entryOffset1 { get { return BleMvxApplication._config.RFID_MBI_MultiBank1Offset.ToString(); } set { try { BleMvxApplication._config.RFID_MBI_MultiBank1Offset = UInt16.Parse(value); } catch (Exception ex) { } } }
        public string entryCount1 { get { return BleMvxApplication._config.RFID_MBI_MultiBank1Count.ToString(); } set { try { BleMvxApplication._config.RFID_MBI_MultiBank1Count = UInt16.Parse(value); } catch (Exception ex) { } } }
        public bool switchMultiBank2Enable { get { return BleMvxApplication._config.RFID_MBI_MultiBank2Enable; } set { BleMvxApplication._config.RFID_MBI_MultiBank2Enable = value; } }
        public string entryOffset2 { get { return BleMvxApplication._config.RFID_MBI_MultiBank2Offset.ToString(); } set { try { BleMvxApplication._config.RFID_MBI_MultiBank2Offset = UInt16.Parse(value); } catch (Exception ex) { } } }
        public string entryCount2 { get { return BleMvxApplication._config.RFID_MBI_MultiBank2Count.ToString(); } set { try { BleMvxApplication._config.RFID_MBI_MultiBank2Count = UInt16.Parse(value); } catch (Exception ex) { } } }

        int[] bankSelectItemLocationMapping = new int[] { 2, 3, 0, 1 };

        public string entryBank1
        {
            get
            {
                return BLE.Client.Pages.PageMultiBankInventorySetting._bankSelectionItems[bankSelectItemLocationMapping[(int)BleMvxApplication._config.RFID_MBI_MultiBank1]];
            }
            set
            {
                try
                {
                    BleMvxApplication._config.RFID_MBI_MultiBank1 = (CSLibrary.Constants.MemoryBank)(UInt16.Parse(value.Substring(value.Length - 2, 1)));
                }
                catch (Exception ex)
                { }
            }
        }

        public string entryBank2
        {
            get
            {
                return BLE.Client.Pages.PageMultiBankInventorySetting._bankSelectionItems[bankSelectItemLocationMapping[(int)BleMvxApplication._config.RFID_MBI_MultiBank2]];
            }
            set
            {
                try
                {
                    BleMvxApplication._config.RFID_MBI_MultiBank2 = (CSLibrary.Constants.MemoryBank)(UInt16.Parse(value.Substring(value.Length - 2, 1)));
                }
                catch (Exception ex)
                { }
            }
        }

        public ViewModelMultiBankInventorySetting(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            OnOKButtonCommand = new Command(OnOKButtonClicked);

            RaisePropertyChanged(() => switchMultiBank1Enable);
            RaisePropertyChanged(() => entryBank1);
            RaisePropertyChanged(() => entryOffset1);
            RaisePropertyChanged(() => entryCount1);
            RaisePropertyChanged(() => switchMultiBank2Enable);
            RaisePropertyChanged(() => entryBank2);
            RaisePropertyChanged(() => entryOffset2);
            RaisePropertyChanged(() => entryCount2);
        }

        void OnOKButtonClicked()
        {
            RaisePropertyChanged(() => switchMultiBank1Enable);
            RaisePropertyChanged(() => entryBank1);
            RaisePropertyChanged(() => entryOffset1);
            RaisePropertyChanged(() => entryCount1);
            RaisePropertyChanged(() => switchMultiBank2Enable);
            RaisePropertyChanged(() => entryBank2);
            RaisePropertyChanged(() => entryOffset2);
            RaisePropertyChanged(() => entryCount2);

            BleMvxApplication.SaveConfig();

            //ShowViewModel<ViewModelMultiBankInventory>(new MvxBundle());
            _navigation.Navigate<ViewModelMultiBankInventory>(new MvxBundle());
        }
    }
}
