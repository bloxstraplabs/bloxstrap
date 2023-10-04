using System.Windows;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Bloxstrap.UI.Elements.Bootstrapper.Base;
using Bloxstrap.UI.ViewModels.Bootstrapper;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    /// <summary>
    /// Interaction logic for ByfronDialog.xaml
    /// </summary>
    public partial class ByfronDialog : IBootstrapperDialog
    {
        private readonly ByfronDialogViewModel _viewModel;

        public Bloxstrap.Bootstrapper? Bootstrapper { get; set; }

        private bool _isClosing;

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

                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelEnabled));
                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelButtonVisibility));
                
                _viewModel.OnPropertyChanged(nameof(_viewModel.VersionTextVisibility));
                _viewModel.OnPropertyChanged(nameof(_viewModel.VersionText));
            }
        }
        #endregion

        public ByfronDialog()
        {
            _viewModel = new ByfronDialogViewModel(this, Bootstrapper?.IsStudioLaunch ?? false);
            DataContext = _viewModel;
            Title = App.Settings.Prop.BootstrapperTitle;
            Icon = App.Settings.Prop.BootstrapperIcon.GetIcon().GetImageSource();

            if (App.Settings.Prop.Theme.GetFinal() == Theme.Light)
            {
                // Matching the roblox website light theme as close as possible.
                _viewModel.DialogBorder = new Thickness(1);
                _viewModel.Background = new SolidColorBrush(Color.FromRgb(242, 244, 245));
                _viewModel.Foreground = new SolidColorBrush(Color.FromRgb(57, 59, 61));
                _viewModel.IconColor = new SolidColorBrush(Color.FromRgb(57, 59, 61));
                _viewModel.ProgressBarBackground = new SolidColorBrush(Color.FromRgb(189, 190, 190));
                _viewModel.ByfronLogoLocation = new BitmapImage(new Uri("pack://application:,,,/Resources/BootstrapperStyles/ByfronDialog/ByfronLogoLight.jpg"));
            }

            InitializeComponent();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_isClosing)
                Bootstrapper?.CancelInstall();
        }

        #region IBootstrapperDialog Methods
        // Referencing FluentDialog
        public void ShowBootstrapper() => this.ShowDialog();

        public void CloseBootstrapper()
        {
            _isClosing = true;
            Dispatcher.BeginInvoke(this.Close);
        }

        public void ShowSuccess(string message, Action? callback) => BaseFunctions.ShowSuccess(message, callback);
        #endregion
    }
}
