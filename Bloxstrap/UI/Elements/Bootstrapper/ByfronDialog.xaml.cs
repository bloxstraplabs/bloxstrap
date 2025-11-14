using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Bloxstrap.UI.ViewModels.Bootstrapper;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    /// <summary>
    /// Interaction logic for ByfronDialog.xaml
    /// </summary>
    public partial class ByfronDialog
    {
        #region UI Elements
        public override string Message
        {
            get => _viewModel.Message;
            set
            {
                string message = value;
                if (message.EndsWith("..."))
                    message = message[..^3];

                _viewModel.Message = message;
                _viewModel.OnPropertyChanged(nameof(_viewModel.Message));
            }
        }

        public override bool CancelEnabled
        {
            get => _viewModel.CancelEnabled;
            set
            {
                _viewModel.CancelEnabled = value;

                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelEnabled));
                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelButtonVisibility));

                _viewModel.OnPropertyChanged(nameof(ByfronDialogViewModel.VersionTextVisibility));
                _viewModel.OnPropertyChanged(nameof(ByfronDialogViewModel.VersionText));
            }
        }
        #endregion

        public ByfronDialog()
            : base()
        {
            string version = Utilities.GetRobloxVersionStr(Bootstrapper?.IsStudioLaunch ?? false);
            ByfronDialogViewModel viewModel = new ByfronDialogViewModel(this, version);
            _viewModel = viewModel;
            DataContext = viewModel;

            if (App.Settings.Prop.Theme.GetFinal() == Theme.Light)
            {
                // Matching the roblox website light theme as close as possible.
                viewModel.DialogBorder = new Thickness(1);
                viewModel.Background = new SolidColorBrush(Color.FromRgb(242, 244, 245));
                viewModel.Foreground = new SolidColorBrush(Color.FromRgb(57, 59, 61));
                viewModel.IconColor = new SolidColorBrush(Color.FromRgb(57, 59, 61));
                viewModel.ProgressBarBackground = new SolidColorBrush(Color.FromRgb(189, 190, 190));
                viewModel.ByfronLogoLocation = new BitmapImage(new Uri("pack://application:,,,/Resources/BootstrapperStyles/ByfronDialog/ByfronLogoLight.jpg"));
            }

            InitializeComponent();
        }
    }
}
