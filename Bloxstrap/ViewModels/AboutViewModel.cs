using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Bloxstrap.Helpers;

namespace Bloxstrap.ViewModels
{
    public class AboutViewModel
    {
        public ICommand OpenWebpageCommand => new RelayCommand<string>(OpenWebpage);

        private void OpenWebpage(string? location)
        {
            if (location is null)
                return;

            Utilities.OpenWebsite(location);
        }

        public string Version => $"Version {App.Version}";
    }
}
