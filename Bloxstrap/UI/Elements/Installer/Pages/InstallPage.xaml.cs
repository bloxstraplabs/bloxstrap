using System.Windows;
using System.Windows.Controls;

using Bloxstrap.UI.ViewModels.Installer;

namespace Bloxstrap.UI.Elements.Installer.Pages
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class InstallPage
    {
        private readonly InstallViewModel _viewModel = new();

        public InstallPage()
        {
            DataContext = _viewModel;

            _viewModel.SetCanContinueEvent += (_, state) =>
            {
                if (Window.GetWindow(this) is MainWindow window)
                    window.SetButtonEnabled("next", state);
            };

            InitializeComponent();
        }

        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow window)
            {
                window.SetNextButtonText("Install");
                window.NextPageCallback += NextPageCallback;
            }
        }

        public bool NextPageCallback() => _viewModel.DoInstall();
    }
}
