using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels
{
    public static class GlobalViewModel
    {
        public static ICommand OpenWebpageCommand => new RelayCommand<string>(OpenWebpage);

        public static bool IsNotFirstRun => !App.IsFirstRun;

        public static Visibility ShowDebugStuff => App.Settings.Prop.OhHeyYouFoundMe ? Visibility.Visible : Visibility.Collapsed;

        private static void OpenWebpage(string? location)
        {
            if (location is null)
                return;

            Utilities.ShellExecute(location);
        }
    }
}
