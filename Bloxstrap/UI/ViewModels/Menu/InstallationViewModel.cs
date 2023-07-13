using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class InstallationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICommand BrowseInstallLocationCommand => new RelayCommand(BrowseInstallLocation);
        public ICommand OpenFolderCommand => new RelayCommand(OpenFolder);

        private void BrowseInstallLocation()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            InstallLocation = dialog.SelectedPath;

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
