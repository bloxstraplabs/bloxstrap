using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

using System.Linq;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class MainWindowViewModel
    {
        private readonly Window _window;
        private readonly IDialogService _dialogService;
        private readonly string _originalBaseDirectory = App.BaseDirectory; // we need this to check if the basedirectory changes

        public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);
        public ICommand ConfirmSettingsCommand => new RelayCommand(ConfirmSettings);

        public string ConfirmButtonText => App.IsFirstRun ? "Install" : "Save";

        public MainWindowViewModel(Window window, IDialogService dialogService)
        {
            _window = window;
            _dialogService = dialogService;
        }

        private void CloseWindow() => _window.Close();

        private void ConfirmSettings()
        {
            if (string.IsNullOrEmpty(App.BaseDirectory))
            {
                Controls.ShowMessageBox("You must set an install location", MessageBoxImage.Error);
                return;
            }

            bool shouldCheckInstallLocation = App.IsFirstRun || App.BaseDirectory != _originalBaseDirectory;

            if (shouldCheckInstallLocation)
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
                    Controls.ShowMessageBox(
                        $"{App.ProjectName} does not have write access to the install location you've selected. Please choose another location.",
                        MessageBoxImage.Error
                    );
                    return;
                }
                catch (Exception ex)
                {
                    Controls.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                    return;
                }

                if (!App.BaseDirectory.EndsWith(App.ProjectName) && Directory.Exists(App.BaseDirectory) && Directory.EnumerateFileSystemEntries(App.BaseDirectory).Any())
                {
                    string suggestedChange = Path.Combine(App.BaseDirectory, App.ProjectName);

                    MessageBoxResult result = Controls.ShowMessageBox(
                        $"The folder you've chosen to install {App.ProjectName} to already exists and is NOT empty. It is strongly recommended for {App.ProjectName} to be installed to its own independent folder.\n\n" +
                        "Changing to the following location is suggested:\n" +
                        $"{suggestedChange}\n\n" +
                        "Would you like to change your install location to this?\n" +
                        "Selecting 'No' will ignore this warning and continue installation.",
                        MessageBoxImage.Warning,
                        MessageBoxButton.YesNoCancel,
                        MessageBoxResult.Yes
                    );

                    if (result == MessageBoxResult.Yes)
                        App.BaseDirectory = suggestedChange;
                    else if (result == MessageBoxResult.Cancel)
                        return;
                }
            }

            if (!App.IsFirstRun)
            {
                App.ShouldSaveConfigs = true;
                App.FastFlags.Save();

                if (shouldCheckInstallLocation)
                {
                    App.Logger.WriteLine($"[MainWindowViewModel::ConfirmSettings] Changing install location from {_originalBaseDirectory} to {App.BaseDirectory}");

                    Controls.ShowMessageBox(
                        $"{App.ProjectName} will install to the new location you've set the next time it runs.",
                        MessageBoxImage.Information
                    );

                    using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"Software\{App.ProjectName}");
                    registryKey.SetValue("InstallLocation", App.BaseDirectory);
                    registryKey.SetValue("OldInstallLocation", _originalBaseDirectory);
                    Directories.Initialize(App.BaseDirectory);
                }

                CloseWindow();
            }
            else
            {
                IDialogControl dialogControl = _dialogService.GetDialogControl();

                dialogControl.ButtonRightClick += (_, _) =>
                {
                    dialogControl.Hide();
                    App.IsSetupComplete = true;
                    CloseWindow();
                };

                dialogControl.ShowAndWaitAsync(
                    "What to know before you install",
                    "After installation, you can open this menu again by searching for it in the Start menu.\n" +
                    "If you want to revert back to the original Roblox launcher, just uninstall Bloxstrap and it will automatically revert."
                );
            }
        }
    }
}
