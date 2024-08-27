using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public MainWindow(bool showAlreadyRunningWarning)
        {
            var viewModel = new MainWindowViewModel();
            viewModel.RequestSaveNoticeEvent += (_, _) => SettingsSavedSnackbar.Show();

            DataContext = viewModel;
            
            InitializeComponent();

            App.Logger.WriteLine("MainWindow::MainWindow", "Initializing menu");

#if DEBUG // easier access
            EditorWarningNavItem.Visibility = Visibility.Visible;
#endif

            if (showAlreadyRunningWarning)
                ShowAlreadyRunningSnackbar();
        }

        private async void ShowAlreadyRunningSnackbar()
        {
            await Task.Delay(500); // wait for everything to finish loading
            AlreadyRunningSnackbar.Show();
        }

        #region INavigationWindow methods

        public Frame GetFrame() => RootFrame;

        public INavigation GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        private void WpfUiWindow_Closing(object sender, CancelEventArgs e)
        {
            if (App.FastFlags.Changed || App.PendingSettingTasks.Any())
            {
                var result = Frontend.ShowMessageBox(Strings.Menu_UnsavedChanges, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                    e.Cancel = true;
            }
        }
    }
}
