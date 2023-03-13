using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Bloxstrap.Helpers;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace Bloxstrap.ViewModels
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
            if (String.IsNullOrEmpty(App.BaseDirectory))
            {
                App.ShowMessageBox("You must set an install location", MessageBoxImage.Error);
                return;
            }

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
                App.ShowMessageBox($"{App.ProjectName} does not have write access to the install location you selected. Please choose another install location.", MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                App.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                return;
            }

            if (!App.IsFirstRun)
            {
                App.ShouldSaveConfigs = true;

                if (App.BaseDirectory != _originalBaseDirectory)
                {
                    App.Logger.WriteLine($"[MainWindowViewModel::ConfirmSettings] Changing install location from {_originalBaseDirectory} to {App.BaseDirectory}");
                    App.ShowMessageBox($"{App.ProjectName} will install to the new location you've set the next time it runs.", MessageBoxImage.Information);

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
