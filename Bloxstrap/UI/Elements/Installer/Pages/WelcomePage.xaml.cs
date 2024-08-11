using System.Windows;
using Bloxstrap.UI.ViewModels.Installer;

namespace Bloxstrap.UI.Elements.Installer.Pages
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage
    {
        private readonly WelcomeViewModel _viewModel = new();

        public WelcomePage()
        {
            _viewModel.CanContinueEvent += (_, _) =>
            {
                if (Window.GetWindow(this) is MainWindow window)
                    window.SetButtonEnabled("next", true);
            };

            DataContext = _viewModel;
            InitializeComponent();
        }

        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.SetNextButtonText("Next");

            _viewModel.DoChecks();
        }
    }
}
