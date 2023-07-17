using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class InstallationViewModel : NotifyPropertyChangedViewModel
    {
        private string _originalInstallLocation = App.BaseDirectory;

        public ICommand BrowseInstallLocationCommand => new RelayCommand(BrowseInstallLocation);
        public ICommand ResetInstallLocationCommand => new RelayCommand(ResetInstallLocation);
        public ICommand OpenFolderCommand => new RelayCommand(OpenFolder);

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

        private void OpenFolder()
        {
            Process.Start("explorer.exe", Directories.Base);
        }

        public string InstallLocation
        {
            get => App.BaseDirectory;
            set => App.BaseDirectory = value;
        }
    }
}
