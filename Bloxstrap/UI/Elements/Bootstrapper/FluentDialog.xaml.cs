using System.Windows.Forms;

using Wpf.Ui.Appearance;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

using Bloxstrap.UI.ViewModels.Bootstrapper;
using Bloxstrap.UI.Elements.Bootstrapper.Base;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    /// <summary>
    /// Interaction logic for FluentDialog.xaml
    /// </summary>
    public partial class FluentDialog : IBootstrapperDialog
    {
        private readonly IThemeService _themeService = new ThemeService();

        private readonly BootstrapperDialogViewModel _viewModel;

        public Bloxstrap.Bootstrapper? Bootstrapper { get; set; }

        #region UI Elements
        public string Message
        {
            get => _viewModel.Message;
            set
            {
                _viewModel.Message = value;
                _viewModel.OnPropertyChanged(nameof(_viewModel.Message));
            }
        }

        public ProgressBarStyle ProgressStyle
        {
            get => _viewModel.ProgressIndeterminate ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            set
            {
                _viewModel.ProgressIndeterminate = (value == ProgressBarStyle.Marquee);
                _viewModel.OnPropertyChanged(nameof(_viewModel.ProgressIndeterminate));
            }
        }

        public int ProgressValue
        {
            get => _viewModel.ProgressValue;
            set
            {
                _viewModel.ProgressValue = value;
                _viewModel.OnPropertyChanged(nameof(_viewModel.ProgressValue));
            }
        }

        public bool CancelEnabled
        {
            get => _viewModel.CancelEnabled;
            set
            {
                _viewModel.CancelEnabled = value;

                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelButtonVisibility));
                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelEnabled));
            }
        }
        #endregion

        public FluentDialog()
        {
            _viewModel = new BootstrapperDialogViewModel(this);
            DataContext = _viewModel;
            Title = App.Settings.Prop.BootstrapperTitle;
            Icon = App.Settings.Prop.BootstrapperIcon.GetIcon().GetImageSource();

            _themeService.SetTheme(App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);
            _themeService.SetSystemAccent();

            InitializeComponent();
        }

        #region IBootstrapperDialog Methods

        public void ShowBootstrapper() => this.ShowDialog();

        public void CloseBootstrapper() => Dispatcher.BeginInvoke(this.Close);

        public void ShowSuccess(string message, Action? callback) => BaseFunctions.ShowSuccess(message, callback);
        #endregion
    }
}
