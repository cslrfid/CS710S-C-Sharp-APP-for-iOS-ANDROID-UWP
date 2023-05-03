using System;
using Acr.UserDialogs;

using MvvmCross.ViewModels;
using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class ViewModelRegisterTag : BaseViewModel
    {
		private readonly IUserDialogs _userDialogs;

        public ICommand OnLabelSelectOffsetTapped { protected set; get; }
        public ICommand OnLabelWriteOffsetTapped { protected set; get; }
        public ICommand OnLabelTagPopulationTapped { protected set; get; }
        public ICommand OnLabelAlgorithmTapped { protected set; get; }

        public ViewModelRegisterTag(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
			_userDialogs = userDialogs;

            OnLabelSelectOffsetTapped = new Command(onlabelSelectOffsetTapped);
            OnLabelWriteOffsetTapped = new Command(onlabelWriteOffsetTapped);
            OnLabelTagPopulationTapped = new Command(onlabelTagPopulationTapped);
            OnLabelAlgorithmTapped = new Command(onlabelAlgorithmTapped);

            RaisePropertyChanged(() => algorithm);
        }

        public override void ViewAppearing()
		{
			base.ViewAppearing();
		}

		public override void ViewDisappearing()
		{
			base.ViewDisappearing();
		}

		protected override void InitFromBundle(IMvxBundle parameters)
		{
			base.InitFromBundle(parameters);
        }

        int _selecttagOffset = 0;
        int _writeOffset = 0;
        int _tagPopulation = 16;
        string _qValueColor = "Blue";
        string _Algorithm = "D";
        public string selecttagOffset { get { return _selecttagOffset.ToString(); } }
        public string writeOffset { get { return _writeOffset.ToString(); } }
        public string tagPopulationValue { get { return _tagPopulation.ToString(); } }
        public string qValueColor { get { return _qValueColor; } }
        public string algorithm { get { return ((_Algorithm == "F") ? "Fixed Q" : "Dynamic Q"); } }

        System.Threading.CancellationTokenSource cancelSrc;

        async void onlabelSelectOffsetTapped()
        {
            var msg = $"Enter a read length value (word)";
            this.cancelSrc?.CancelAfter(TimeSpan.FromSeconds(3));
            var r = await this._userDialogs.PromptAsync(msg, inputType: InputType.Number, placeholder: _selecttagOffset.ToString(), cancelToken: this.cancelSrc?.Token);
            await System.Threading.Tasks.Task.Delay(500);

            _selecttagOffset = int.Parse(r.Text);
           RaisePropertyChanged(() => selecttagOffset);
        }

        async void onlabelWriteOffsetTapped()
        {
            var msg = $"Enter a read length value (word)";
            this.cancelSrc?.CancelAfter(TimeSpan.FromSeconds(3));
            var r = await this._userDialogs.PromptAsync(msg, inputType: InputType.Number, placeholder: _writeOffset.ToString(), cancelToken: this.cancelSrc?.Token);
            await System.Threading.Tasks.Task.Delay(500);

            _writeOffset = int.Parse(r.Text);
            RaisePropertyChanged(() => writeOffset);
        }

        async void onlabelTagPopulationTapped()
        {
            var msg = $"Enter a Tag Population value";
            this.cancelSrc?.CancelAfter(TimeSpan.FromSeconds(3));
            var r = await this._userDialogs.PromptAsync(msg, inputType: InputType.Number, placeholder: _tagPopulation.ToString(), cancelToken: this.cancelSrc?.Token);
            await System.Threading.Tasks.Task.Delay(500);

            _tagPopulation = int.Parse(r.Text);
            _qValueColor = "Red";
            RaisePropertyChanged(() => algorithm);
            RaisePropertyChanged(() => tagPopulationValue);
            RaisePropertyChanged(() => qValueColor);
        }

        async void onlabelAlgorithmTapped()
        {
            if (_Algorithm == "F")
            {
                _Algorithm = "D";
            }
            else
            {
                _Algorithm = "F";
            }

            _qValueColor = "Red";
            RaisePropertyChanged(() => algorithm);
            RaisePropertyChanged(() => tagPopulationValue);
            RaisePropertyChanged(() => qValueColor);
        }
    }
}
