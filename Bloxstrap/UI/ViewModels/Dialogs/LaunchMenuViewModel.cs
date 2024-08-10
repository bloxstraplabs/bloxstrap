using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Resources;

namespace Bloxstrap.UI.ViewModels.Installer
{
    // TODO: have it so it shows "Launch Roblox"/"Install and Launch Roblox" depending on state of /App/ folder
    public class LaunchMenuViewModel
    {
        public string Version => string.Format(Strings.Menu_About_Version, App.Version);

        public ICommand LaunchSettingsCommand => new RelayCommand(LaunchSettings);

        public ICommand LaunchRobloxCommand => new RelayCommand(LaunchRoblox);

        public event EventHandler<NextAction>? CloseWindowRequest;

        private void LaunchSettings() => CloseWindowRequest?.Invoke(this, NextAction.LaunchSettings);

        private void LaunchRoblox() => CloseWindowRequest?.Invoke(this, NextAction.LaunchRoblox);
    }
}
