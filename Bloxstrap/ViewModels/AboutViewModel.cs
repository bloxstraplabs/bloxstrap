using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Bloxstrap.Helpers;

namespace Bloxstrap.ViewModels
{
    public class AboutViewModel
    {
        public string Version => $"Version {App.Version}";
    }
}
