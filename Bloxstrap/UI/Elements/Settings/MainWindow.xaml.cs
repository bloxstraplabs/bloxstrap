using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private Models.Persistable.WindowState _state => App.State.Prop.SettingsWindow;

        private readonly bool _keepProcessAlive;

        public MainWindow(bool showAlreadyRunningWarning, bool keepProcessAlive = false)
        {
            _keepProcessAlive = keepProcessAlive;
            var viewModel = new MainWindowViewModel();

            viewModel.RequestSaveNoticeEvent += (_, _) => SettingsSavedSnackbar.Show();
            viewModel.RequestCloseWindowEvent += (_, _) => Close();

            DataContext = viewModel;
            
            InitializeComponent();

            App.Logger.WriteLine("MainWindow", "Initializing settings window");

            if (showAlreadyRunningWarning)
                ShowAlreadyRunningSnackbar();

            LoadState();
        }

        public void LoadState()
        {
            if (_state.Left > SystemParameters.VirtualScreenWidth)
                _state.Left = 0;

            if (_state.Top > SystemParameters.VirtualScreenHeight)
                _state.Top = 0;

            if (_state.Width > 0)
                this.Width = _state.Width;

            if (_state.Height > 0)
                this.Height = _state.Height;

            if (_state.Left > 0 && _state.Top > 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = _state.Left;
                this.Top = _state.Top;
            }
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

        private void SettingsSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            string query = SettingsSearchBox.Text.Trim();
            if (query.Length == 0)
                return;

            (string Name, Type PageType)[] pages =
            {
                (Strings.Menu_Search_Home, typeof(Pages.HomePage)),
                (Strings.Menu_Search_Integrations, typeof(Pages.IntegrationsPage)),
                (LumiFeatureStrings.Menu_Search_Experiences, typeof(Pages.ExperienceLibraryPage)),
                (LumiFeatureStrings.Menu_Search_Studio, typeof(Pages.StudioPage)),
                (Strings.Menu_Search_Behaviour, typeof(Pages.BehaviourPage)),
                (Strings.Menu_Search_FastFlags, typeof(Pages.FastFlagsPage)),
                (Strings.Menu_Search_Appearance, typeof(Pages.AppearancePage)),
                (Strings.Menu_Search_Profiles, typeof(Pages.ProfilesPage)),
                (Strings.Menu_Search_Shortcuts, typeof(Pages.ShortcutsPage)),
                (Strings.Menu_Search_LumiStrap, typeof(Pages.BloxstrapPage)),
                (Strings.Menu_Search_LumiFix, typeof(Pages.BloxstrapPage))
            };

            foreach ((string name, Type pageType) in pages)
            {
                if (!name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                Navigate(pageType);
                SettingsSearchBox.Clear();
                e.Handled = true;
                return;
            }
        }

        private void WpfUiWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.K && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                SettingsSearchBox.Focus();
                e.Handled = true;
            }
        }

        private void WpfUiWindow_Closing(object sender, CancelEventArgs e)
        {
            if (App.FastFlags.Changed || App.PendingSettingTasks.Any())
            {
                var result = Frontend.ShowMessageBox(Strings.Menu_UnsavedChanges, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                    e.Cancel = true;
            }
            
            _state.Width = this.Width;
            _state.Height = this.Height;

            _state.Top = this.Top;
            _state.Left = this.Left;

            App.State.Save();
        }

        private void WpfUiWindow_Closed(object sender, EventArgs e)
        {
            if (_keepProcessAlive)
                return;

            if (App.LaunchSettings.TestModeFlag.Active)
                LaunchHandler.LaunchRoblox(LaunchMode.Player);
            else
                App.SoftTerminate();
        }
    }
}
