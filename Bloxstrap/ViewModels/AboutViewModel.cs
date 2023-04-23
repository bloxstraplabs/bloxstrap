using System.Windows.Input;
using Bloxstrap.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.ViewModels
{
    public class AboutViewModel
    {
        public string Version => $"Version {App.Version}";

        public ICommand CFUWindowCommand => new RelayCommand(CheckForUpdate);

        private void CheckForUpdate()
        {
            Updater.CheckForUpdate();
        }
    }
}
