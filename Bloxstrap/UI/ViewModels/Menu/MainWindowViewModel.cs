using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

using Wpf.Ui.Mvvm.Contracts;

using Bloxstrap.UI.Elements.Menu;
using Bloxstrap.UI.Elements.Menu.Pages;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class MainWindowViewModel : NotifyPropertyChangedViewModel
    {
        private readonly MainWindow _window;

        public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);
        public ICommand ConfirmSettingsCommand => new RelayCommand(ConfirmSettings);

        public Visibility NavigationVisibility { get; set; } = Visibility.Visible;
        public string ConfirmButtonText => App.IsFirstRun ? Resources.Strings.Menu_Install : Resources.Strings.Menu_Save;
        public string CloseButtonText => App.IsFirstRun ? Resources.Strings.Common_Cancel : Resources.Strings.Common_Close;
        public bool ConfirmButtonEnabled { get; set; } = true;

        public MainWindowViewModel(MainWindow window)
        {
            _window = window;
        }

        private void CloseWindow() => _window.Close();

        private void ConfirmSettings()
        {
            if (!App.IsFirstRun)
            {
                App.ShouldSaveConfigs = true;
                App.Settings.Save();
                App.State.Save();
                App.FastFlags.Save();
                App.ShouldSaveConfigs = false;

                _window.SettingsSavedSnackbar.Show();

                return;
            }

            if (string.IsNullOrEmpty(App.BaseDirectory))
            {
                Frontend.ShowMessageBox(Resources.Strings.Menu_InstallLocation_NotSet, MessageBoxImage.Error);
                return;
            }

            if (NavigationVisibility == Visibility.Visible)
            {
                try
                {
                    // check if we can write to the directory (a bit hacky but eh)
                    string testFile = Path.Combine(App.BaseDirectory, $"{App.ProjectName}WriteTest.txt");

                    Directory.CreateDirectory(App.BaseDirectory);
                    File.WriteAllText(testFile, "hi");
                    File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    Frontend.ShowMessageBox(
                        Resources.Strings.Menu_InstallLocation_NoWritePerms,
                        MessageBoxImage.Error
                    );
                    return;
                }
                catch (Exception ex)
                {
                    Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                    return;
                }

                if (!App.BaseDirectory.EndsWith(App.ProjectName) && Directory.Exists(App.BaseDirectory) && Directory.EnumerateFileSystemEntries(App.BaseDirectory).Any())
                {
                    string suggestedChange = Path.Combine(App.BaseDirectory, App.ProjectName);

                    MessageBoxResult result = Frontend.ShowMessageBox(
                        string.Format(Resources.Strings.Menu_InstallLocation_NotEmpty, suggestedChange),
                        MessageBoxImage.Warning,
                        MessageBoxButton.YesNoCancel,
                        MessageBoxResult.Yes
                    );

                    if (result == MessageBoxResult.Yes)
                        App.BaseDirectory = suggestedChange;
                    else if (result == MessageBoxResult.Cancel)
                        return;
                }

                if (
                    App.BaseDirectory.Length <= 3 || // prevent from installing to the root of a drive
                    App.BaseDirectory.StartsWith("\\\\") || // i actually haven't encountered anyone doing this and i dont even know if this is possible but this is just to be safe lmao
                    App.BaseDirectory.ToLowerInvariant().Contains("onedrive") || // prevent from installing to a onedrive folder
                    Directory.GetParent(App.BaseDirectory)!.ToString().ToLowerInvariant() == Paths.UserProfile.ToLowerInvariant() // prevent from installing to an essential user profile folder
                )
                {
                    Frontend.ShowMessageBox(
                        Resources.Strings.Menu_InstallLocation_CantInstall,
                        MessageBoxImage.Error,
                        MessageBoxButton.OK
                    );

                    return;
                }
            }
            
            if (NavigationVisibility == Visibility.Visible)
            {
                ((INavigationWindow)_window).Navigate(typeof(PreInstallPage));

                NavigationVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(NavigationVisibility));
                    
                ConfirmButtonEnabled = false;
                OnPropertyChanged(nameof(ConfirmButtonEnabled));

                Task.Run(async delegate
                {
                    await Task.Delay(3000);
                    
                    ConfirmButtonEnabled = true;
                    OnPropertyChanged(nameof(ConfirmButtonEnabled));
                });
            }
            else
            {
                App.IsSetupComplete = true;
                CloseWindow();
            }
        }
    }
}
