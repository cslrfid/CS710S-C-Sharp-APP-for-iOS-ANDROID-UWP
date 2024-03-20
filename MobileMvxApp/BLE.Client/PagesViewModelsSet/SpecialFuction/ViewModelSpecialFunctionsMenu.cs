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

using Acr.UserDialogs;

using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using MvvmCross;
using BLE.Client.Pages;

namespace BLE.Client.ViewModels
{
    public class ViewModelSpecialFunctionsMenu : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        public ICommand OnMultiBankInventoryButtonCommand { protected set; get; }

        public ICommand OnImpinjSpecialFeaturesButtonCommand { protected set; get; }

        public ICommand OnPhaseChannelInventoryButtonCommand { protected set; get; }
        public ICommand OnPeriodicReadButtonCommand { protected set; get; }
        public ICommand OnUCODEDNAButtonCommand { protected set; get; }
        public ICommand OnRFMicroButtonCommand { protected set; get; }
        public ICommand OnXerxesButtonCommand { protected set; get; }
        public ICommand OnBlockWriteButtonCommand { protected set; get; }
        public ICommand OnReadButtonCommand { protected set; get; }
        public ICommand OnCS83045ButtonCommand { protected set; get; }
        public ICommand OnCS9010ButtonCommand { protected set; get; }
        public ICommand OnTagFocusandFastIDButtonCommand { protected set; get; }
        public ICommand OnCTESIUSTempButtonCommand { protected set; get; }
        public ICommand OnEM4152ButtonCommand { protected set; get; }
        public ICommand OnFM13DT160ButtonCommand { protected set; get; }
        public ICommand OnSmartracThermologgerButtonCommand { protected set; get; }
        public ICommand OnWriteAnyEPCButtonCommand { protected set; get; }
        public ICommand OnPerformanceTestButtonCommand { protected set; get; }
        public ICommand OnEM4325PassiveButtonCommand { protected set; get; }
        public ICommand OnUCODE8ButtonCommand { protected set; get; }
        public ICommand OnMonza4QTButtonCommand { protected set; get; }
        public ICommand OnMagnusS3withGPSforTabletButtonCommand { protected set; get; }
        public ICommand OnKilowayKX2005XBLButtonCommand { protected set; get; }
        public ICommand OnN2ESLButtonCommand { protected set; get; }
        public ICommand OnAsygnButtonCommand { protected set; get; }


        public ViewModelSpecialFunctionsMenu (IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            OnMultiBankInventoryButtonCommand = new Command(OnMultiBankInventoryButtonClicked);
            OnImpinjSpecialFeaturesButtonCommand = new Command(OnImpinjSpecialFeaturesButtonClicked);
            OnRFMicroButtonCommand = new Command(OnRFMicroButtonClicked);
            OnKilowayKX2005XBLButtonCommand = new Command(OnOnKilowayKX2005XBLButtonClicked);
            OnN2ESLButtonCommand = new Command(OnN2ESLButtonClicked);
            OnAsygnButtonCommand = new Command(OnAsygnButtonClicked);

            OnPhaseChannelInventoryButtonCommand = new Command(OnPhaseChannelInventoryButtonClicked);
            OnPeriodicReadButtonCommand = new Command(OnPeriodicReadButtonClicked);
            OnUCODEDNAButtonCommand = new Command(OnUCODEDNAButtonClicked);
            OnXerxesButtonCommand = new Command(OnXerxesButtonClicked);
            OnBlockWriteButtonCommand = new Command(OnBlockWriteButtonClicked);
            OnReadButtonCommand = new Command(OnReadButtonClicked);
            OnCS83045ButtonCommand = new Command(OnCS83045ButtonClicked);
            OnCS9010ButtonCommand = new Command(OnCS9010ButtonClicked);
            OnTagFocusandFastIDButtonCommand = new Command(OnTagFocusandFastIDButtonClicked);
            OnCTESIUSTempButtonCommand = new Command(OnCTESIUSTempButtonClicked);
            OnEM4152ButtonCommand = new Command(OnEM4152ButtonClicked);
            OnFM13DT160ButtonCommand = new Command(OnFM13DT160ButtonClicked);
            OnSmartracThermologgerButtonCommand = new Command(OnSmartracThermologgerButtonClicked);
            OnWriteAnyEPCButtonCommand = new Command(OnWriteAnyEPCButtonClicked);
            OnPerformanceTestButtonCommand = new Command(OnPerformanceTestButtonClicked);
            OnEM4325PassiveButtonCommand = new Command(OnEM4325PassiveButtonClicked);
            OnUCODE8ButtonCommand = new Command(OnUCODE8ButtonClicked);
            OnMonza4QTButtonCommand = new Command(OnMonza4QTButtonClicked);
            OnMagnusS3withGPSforTabletButtonCommand = new Command(OnMagnusS3withGPSforTabletButtonClicked);

        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            BleMvxApplication._reader.rfid.StopOperation();
            BleMvxApplication._reader.barcode.Stop();
            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
        }

        void OnMultiBankInventoryButtonClicked()
        {
            //ShowViewModel<ViewModelMultiBankInventorySetting>(new MvxBundle());
            _navigation.Navigate<ViewModelMultiBankInventorySetting>(new MvxBundle());
        }

        void OnImpinjSpecialFeaturesButtonClicked()
        {
            _navigation.Navigate<ViewModelImpinjSpecialFeaturesInventory>(new MvxBundle());
        }

        void OnPhaseChannelInventoryButtonClicked()
        {
            //ShowViewModel<ViewModelPhaseChannelInventory>(new MvxBundle());
            _navigation.Navigate<ViewModelPhaseChannelInventory>(new MvxBundle());
        }

        void OnPeriodicReadButtonClicked()
        {
            //ShowViewModel<ViewModelPeriodicRead>(new MvxBundle());
            _navigation.Navigate<ViewModelPeriodicRead>(new MvxBundle());
        }

        void OnUCODEDNAButtonClicked()
        {
            //ShowViewModel<ViewModelUCODEDNA>(new MvxBundle());
            _navigation.Navigate<ViewModelUCODEDNA>(new MvxBundle());
        }

        void OnRFMicroButtonClicked()
        {
            //ShowViewModel<ViewModelRFMicroSetting>(new MvxBundle());
            _navigation.Navigate<ViewModelRFMicroSetting>(new MvxBundle());
        }

        void OnOnKilowayKX2005XBLButtonClicked()
        {
            _navigation.Navigate<ViewModelLEDTag>(new MvxBundle());
        }
        
        void OnN2ESLButtonClicked()
        {
            //_navigation.Navigate<ViewModelLEDTag1>(new MvxBundle());
            _navigation.Navigate<ViewModelLEDTagWithInventory>(new MvxBundle());
        }

        void OnAsygnButtonClicked()
        {
            //_navigation.Navigate<ViewModelLEDTag1>(new MvxBundle());
            _navigation.Navigate<ViewModelAsygnInventory>(new MvxBundle());
        }


        void OnXerxesButtonClicked()
        {
            //ShowViewModel<ViewModelAxzonSetting>(new MvxBundle());
            _navigation.Navigate<ViewModelAxzonSetting>(new MvxBundle());
        }

        void OnBlockWriteButtonClicked()
        {
            //ShowViewModel<ViewModelBlockWrite>(new MvxBundle());
            _navigation.Navigate<ViewModelBlockWrite>(new MvxBundle());
        }

        void OnReadButtonClicked()
        {
            //ShowViewModel<ViewModelRead>(new MvxBundle());
            _navigation.Navigate<ViewModelRead>(new MvxBundle());
        }

        void OnCS83045ButtonClicked()
        {
            //ShowViewModel<ViewModelCS83045Setting>(new MvxBundle());
            _navigation.Navigate<ViewModelCS83045Setting>(new MvxBundle());
        }

        void OnCS9010ButtonClicked()
        {
            //ShowViewModel<ViewModelCS9010Inventory>(new MvxBundle());
            _navigation.Navigate<ViewModelCS9010Inventory>(new MvxBundle());
        }

        void OnTagFocusandFastIDButtonClicked()
        {
            //ShowViewModel<ViewModelFocusandFastIDSetting>(new MvxBundle());
            _navigation.Navigate<ViewModelFocusandFastIDSetting>(new MvxBundle());
        }

        void OnCTESIUSTempButtonClicked()
        {
            //ShowViewModel< ViewModelCTESIUSTempInventory>(new MvxBundle());
            _navigation.Navigate<ViewModelCTESIUSTempInventory>(new MvxBundle());
        }

        void OnEM4152ButtonClicked()
        {
            //ShowViewModel<ViewModelEM4152Inventory>(new MvxBundle());
//            _navigation.Navigate<ViewModelEM4152Inventory>(new MvxBundle());
        }

        void OnFM13DT160ButtonClicked()
        {
            //ShowViewModel<ViewModelFM13DT160Inventory>(new MvxBundle());
//            _navigation.Navigate<ViewModelFM13DT160Inventory>(new MvxBundle());
        }

        void OnSmartracThermologgerButtonClicked ()
        {
            //ShowViewModel<ViewModelSmartracThermologgerInventory>(new MvxBundle());
            _navigation.Navigate<ViewModelSmartracThermologgerInventory>(new MvxBundle());
        }

        void OnWriteAnyEPCButtonClicked()
        {
            //ShowViewModel<ViewModelWriteAnyEPC>(new MvxBundle());
            _navigation.Navigate<ViewModelWriteAnyEPC>(new MvxBundle());
        }

        void OnPerformanceTestButtonClicked()
        {
            //ShowViewModel<ViewModelPerformanceTest>(new MvxBundle());
            _navigation.Navigate<ViewModelPerformanceTest>(new MvxBundle());
        }

        void OnEM4325PassiveButtonClicked()
        {
//            _navigation.Navigate<ViewModelEM4325PassiveInventory>(new MvxBundle());
        }

        void OnUCODE8ButtonClicked()
        {
            _navigation.Navigate<ViewModelUCODE8Inventory>(new MvxBundle());
        }

        void OnMonza4QTButtonClicked()
        {
            //ShowViewModel<ViewModelQTInventorySlectionMenu>(new MvxBundle());
//            _navigation.Navigate<ViewModelQTInventorySlectionMenu>(new MvxBundle());
        }

        void OnMagnusS3withGPSforTabletButtonClicked()
        {
            //ShowViewModel<ViewModelTempGPSSetting>(new MvxBundle());
            _navigation.Navigate<ViewModelTempGPSSetting>(new MvxBundle());
        }

        /*
        void OnASCIIdemoButtonClicked()
        {
            //ShowViewModel<ViewModelTempGPSSetting>(new MvxBundle());
            _navigation.Navigate<ViewModelASCIIInventory>(new MvxBundle());
        }

        void OnAutotuneButtonClicked()
        {
            //ShowViewModel<ViewModelTempGPSSetting>(new MvxBundle());
            _navigation.Navigate<ViewModelASCIIInventory>(new MvxBundle());
        }*/


    }
}
