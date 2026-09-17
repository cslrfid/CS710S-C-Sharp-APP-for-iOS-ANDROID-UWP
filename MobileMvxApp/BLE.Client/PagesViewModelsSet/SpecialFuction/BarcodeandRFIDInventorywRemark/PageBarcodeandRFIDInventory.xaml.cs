using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using System;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageBarcodeandRFIDInventorywRemark : MvxContentPage<ViewModelBarcodeandRFIDInventorywRemark>
    {
        public PageBarcodeandRFIDInventorywRemark()
		{
			InitializeComponent();
        }

        public async void entryPowerCompleted(object sender, EventArgs e)
        {
            uint value;

            try
            {
                value = uint.Parse(entryPower.Text);
                if (value < 0 || value > 320)
                    throw new System.ArgumentException("Power can only be set to 320 or below", "Power");
                entryPower.Text = value.ToString();

                BleMvxApplication._config._InventoryBarkcode_Power = (int)value;
                BleMvxApplication.SaveConfig();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Power", "Power can only be set to 320 or below", "OK");
                entryPower.Text = BleMvxApplication._config._InventoryBarkcode_Power.ToString();
            }
        }

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            BLE.Client.ViewModels.ViewModelBarcodeandRFIDInventorywRemark.BarcodeandRFIDTagInfoViewModel Items = (BLE.Client.ViewModels.ViewModelBarcodeandRFIDInventorywRemark.BarcodeandRFIDTagInfoViewModel)e.SelectedItem;

            string result = await DisplayPromptAsync("Please input barcode", "EPC " + Items.EPC, "OK", "Cancel", "", maxLength: 30, keyboard: Keyboard.Default);

            if (result != null)
            {
                Items.BARCODE = result;
            }
            /*
                        var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");

                        if (answer)
                        {
                            //BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo Items = (BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo)e.SelectedItem;
                            BLE.Client.ViewModels.TagInfoViewModel Items = (BLE.Client.ViewModels.TagInfoViewModel)e.SelectedItem;
                        }
            */
        }

        public async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            BLE.Client.ViewModels.ViewModelBarcodeandRFIDInventorywRemark.BarcodeandRFIDTagInfoViewModel Item = (BLE.Client.ViewModels.ViewModelBarcodeandRFIDInventorywRemark.BarcodeandRFIDTagInfoViewModel)e.Item;

            string barcodeResult = await DisplayPromptAsync("Please input BARCODE", "EPC : " + Item.EPC, "OK", "Cancel", Item.BARCODE, maxLength: 50, keyboard: Keyboard.Default);

            if (barcodeResult != null)
            {
                Item.BARCODE = barcodeResult;
                
                // 添加第二個 prompt 輸入 Remark
                string remarkResult = await DisplayPromptAsync("Please input REMARK", "EPC : " + Item.EPC + System.Environment.NewLine + "Barcode : " + Item.BARCODE, "OK", "Cancel", Item.REMARK, maxLength: 30, keyboard: Keyboard.Default);
                
                if (remarkResult != null)
                {
                    Item.REMARK = remarkResult;
                }
            }
        }
    }
}
