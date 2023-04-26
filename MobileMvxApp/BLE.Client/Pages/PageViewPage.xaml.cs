using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLE.Client.ViewModels;
using MvvmCross.Forms.Views;

namespace BLE.Client.Pages
{
    public partial class PageViewPage : MvxContentPage<ViewModelViewPage>
    {
        string[] _DisplayModeOptions = { "Text Mode", "Line Mode" };

        public PageViewPage()
        {
            InitializeComponent();

            labelViewCharLength.Text = "Totel Characters : " + BleMvxApplication._LargeContent.Length.ToString();
            labelViewBitLength.Text = "Number of bits : " + (BleMvxApplication._LargeContent.Length * 4).ToString();
            labelViewByteLength.Text = "Number of bytes : " + BleMvxApplication._LargeContent.Length.ToString();
            labelViewWordLength.Text = "Number of words : " + (BleMvxApplication._LargeContent.Length / 4).ToString();

            buttonDisplayMode.Text = _DisplayModeOptions[1];
            //editorContent.Text = BleMvxApplication._LargeContent;
        }

        public async void buttonDisplayModeClicked(object sender, EventArgs args)
        {
            var answer = await DisplayActionSheet("Display Mode", "Cancel", null, _DisplayModeOptions);

            if (answer != null && answer != "Cancel")
            {
                buttonDisplayMode.Text = answer;
            }
        }

        public async void buttonDisplayModePropertyChanged(object sender, EventArgs args)
        {
            if (buttonDisplayMode != null)
            {
                switch (Array.IndexOf(_DisplayModeOptions, buttonDisplayMode.Text))
                {
                    case 0:
                        editorContent.Text = BleMvxApplication._LargeContent;
                        break;

                    case 1:
                        editorContent.Text = "";

                        if (BleMvxApplication._LargeContent.Length > 0)
                        {
                            int lineLength = 20;

                            for (int cnt = 0; cnt < BleMvxApplication._LargeContent.Length; cnt += lineLength)
                            {
                                if (cnt + lineLength <= BleMvxApplication._LargeContent.Length)
                                    editorContent.Text += cnt.ToString() + " : "+ BleMvxApplication._LargeContent.Substring(cnt, lineLength);
                                else
                                    editorContent.Text += cnt.ToString() + " : " + BleMvxApplication._LargeContent.Substring(cnt, BleMvxApplication._LargeContent.Length - cnt);

                                editorContent.Text += Environment.NewLine;
                            }
                        }
                        break;
				}
			}
        }

    }
}
