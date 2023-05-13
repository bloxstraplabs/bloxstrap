using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Bloxstrap.Enums;
using Bloxstrap.Extensions;
using Bloxstrap.ViewModels;

namespace Bloxstrap.Dialogs
{
    /// <summary>
    /// Interaction logic for ByfronDialog.xaml
    /// </summary>
    public partial class ByfronDialog : IBootstrapperDialog
    {
        private readonly ByfronDialogViewModel _viewModel;

        public Bootstrapper? Bootstrapper { get; set; }

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
            get => _viewModel.CancelButtonVisibility == Visibility.Visible;
            set
            {
                _viewModel.CancelButtonVisibility = (value ? Visibility.Visible : Visibility.Collapsed);
                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelButtonVisibility));
            }
        }
        #endregion

        public ByfronDialog()
        {
            _viewModel = new ByfronDialogViewModel(this);
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

        #region IBootstrapperDialog Methods
        // Referencing FluentDialog
        public void ShowBootstrapper() => this.ShowDialog();

        public void CloseBootstrapper() => Dispatcher.BeginInvoke(this.Close);

        public void ShowSuccess(string message)
        {
            App.ShowMessageBox(message, MessageBoxImage.Information);
            App.Terminate();
        }

        public void ShowError(string message)
        {
            App.ShowMessageBox($"An error occurred while starting Roblox\n\nDetails: {message}", MessageBoxImage.Error);
            App.Terminate(Bootstrapper.ERROR_INSTALL_FAILURE);
        }

        public void PromptShutdown()
        {
            MessageBoxResult result = App.ShowMessageBox(
                "Roblox is currently running, but needs to close. Would you like close Roblox now?",
                MessageBoxImage.Information,
                MessageBoxButton.OKCancel
            );

            if (result != MessageBoxResult.OK)
                Environment.Exit(Bootstrapper.ERROR_INSTALL_USEREXIT);
        }
        #endregion
    }
}
