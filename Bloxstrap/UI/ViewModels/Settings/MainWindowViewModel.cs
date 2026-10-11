using System.Windows;
using System.Windows.Input;
using Bloxstrap.UI.Elements.About;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class MainWindowViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand OpenAboutCommand => new RelayCommand(OpenAbout);

        public ICommand OpenCreditsCommand => new RelayCommand(() => new CreditsWindow().ShowDialog());
        
        public ICommand SaveSettingsCommand => new RelayCommand(SaveSettings);
        
        public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);

        public ICommand SaveAndLaunchCommand => new RelayCommand<LaunchMode>(SaveAndLaunch);

        public ICommand RestartCommand => new RelayCommand(Restart);

        public LaunchMode LaunchAfterClose { get; private set; } = LaunchMode.None;

        public bool RestartRequested { get; private set; }

        public string VersionText => $"v{App.Version}";

        public EventHandler? RequestSaveNoticeEvent;
        
        public EventHandler? RequestCloseWindowEvent;

        public bool TestModeEnabled
        {
            get => App.LaunchSettings.TestModeFlag.Active;
            set
            {
                if (value)
                {
                    var result = Frontend.ShowMessageBox(Strings.Menu_TestMode_Prompt, MessageBoxImage.Information, MessageBoxButton.YesNo);

                    if (result != MessageBoxResult.Yes)
                        return;
                }

                App.LaunchSettings.TestModeFlag.Active = value;
            }
        }

        private void OpenAbout() => new MainWindow().ShowDialog();

        private void CloseWindow() => RequestCloseWindowEvent?.Invoke(this, EventArgs.Empty);

        public void CancelPendingClose()
        {
            LaunchAfterClose = LaunchMode.None;
            RestartRequested = false;
        }

        private void SaveAndLaunch(LaunchMode mode)
        {
            SaveSettings(false);
            LaunchAfterClose = mode;
            CloseWindow();
        }

        private void Restart()
        {
            SaveSettings(false);
            RestartRequested = true;
            CloseWindow();
        }

        private void SaveSettings() => SaveSettings(true);

        private void SaveSettings(bool notify)
        {
            const string LOG_IDENT = "MainWindowViewModel::SaveSettings";

            App.Settings.Save();
            App.State.Save();
            App.FastFlags.Save();

            foreach (var pair in App.PendingSettingTasks)
            {
                var task = pair.Value;

                if (task.Changed)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Executing pending task '{task}'");
                    task.Execute();
                }
            }

            App.PendingSettingTasks.Clear();

            if (notify)
                RequestSaveNoticeEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
