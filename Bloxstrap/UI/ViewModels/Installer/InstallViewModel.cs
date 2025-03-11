using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Installer
{
    public class InstallViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Bloxstrap.Installer installer = new();

        private readonly string _originalInstallLocation;

        public EventHandler<bool>? SetCanContinueEvent;

        public string InstallLocation 
        {
            get => installer.InstallLocation;
            set
            {
                if (!String.IsNullOrEmpty(ErrorMessage))
                {
                    SetCanContinueEvent?.Invoke(this, true);

                    installer.InstallLocationError = "";
                    OnPropertyChanged(nameof(ErrorMessage));
                }

                installer.InstallLocation = value;
                OnPropertyChanged(nameof(DataFoundMessageVisibility));
            }
        }

        public Visibility DataFoundMessageVisibility => installer.ExistingDataPresent ? Visibility.Visible : Visibility.Collapsed;

        public string ErrorMessage => installer.InstallLocationError;

        public bool CreateDesktopShortcuts
        {
            get => installer.CreateDesktopShortcuts;
            set => installer.CreateDesktopShortcuts = value;
        }
        
        public bool CreateStartMenuShortcuts
        {
            get => installer.CreateStartMenuShortcuts;
            set => installer.CreateStartMenuShortcuts = value;
        }

        public bool ImportSettings
        {
            get => installer.ImportSettings;
            set => installer.ImportSettings = value;
        }

        public bool ImportSettingsEnabled
        {
            get => Directory.Exists(installer.BloxstrapInstallDirectory);
        }

        public bool ShowNotFound // im lazy
        {
            get => !Directory.Exists(installer.BloxstrapInstallDirectory);
        }

        public ICommand BrowseInstallLocationCommand => new RelayCommand(BrowseInstallLocation);

        public ICommand ResetInstallLocationCommand => new RelayCommand(ResetInstallLocation);

        public ICommand OpenFolderCommand => new RelayCommand(OpenFolder);

        public InstallViewModel()
        {
            _originalInstallLocation = installer.InstallLocation;
        }

        public bool DoInstall()
        {
            if (!installer.CheckInstallLocation())
            {
                SetCanContinueEvent?.Invoke(this, false);

                OnPropertyChanged(nameof(ErrorMessage));
                return false;
            }

            installer.DoInstall();

            return true;
        }

        private void BrowseInstallLocation()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            InstallLocation = dialog.SelectedPath;
            OnPropertyChanged(nameof(InstallLocation));
        }

        private void ResetInstallLocation()
        {
            InstallLocation = _originalInstallLocation;
            OnPropertyChanged(nameof(InstallLocation));
        }

        private void OpenFolder() => Process.Start("explorer.exe", Paths.Base);
    }
}
