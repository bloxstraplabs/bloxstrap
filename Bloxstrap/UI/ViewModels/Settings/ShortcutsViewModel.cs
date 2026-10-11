using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class ShortcutsViewModel : NotifyPropertyChangedViewModel
    {
        public bool IsStudioOptionVisible => App.IsStudioInstalled;

        public ShortcutTask DesktopIconTask { get; } = new("Desktop", Paths.Desktop, $"{App.ProjectName}.lnk");

        public ShortcutTask StartMenuIconTask { get; } = new("StartMenu", Paths.WindowsStartMenu, $"{App.ProjectName}.lnk");

        public ShortcutTask PlayerIconTask { get; } = new("RobloxPlayer", Paths.Desktop, $"{Strings.LaunchMenu_LaunchRoblox}.lnk", "-player");

        public ShortcutTask StudioIconTask { get; } = new("RobloxStudio", Paths.Desktop, $"{Strings.LaunchMenu_LaunchRobloxStudio}.lnk", "-studio");

        public ShortcutTask SettingsIconTask { get; } = new("Settings", Paths.Desktop, $"{Strings.Menu_Title}.lnk", "-settings");

        public ExtractIconsTask ExtractIconsTask { get; } = new();

        public string GameShortcutInput { get; set; } = "";

        public ICommand CreateGameShortcutCommand => new AsyncRelayCommand(CreateGameShortcut);

        private async Task CreateGameShortcut()
        {
            const string LOG_IDENT = "ShortcutsViewModel::CreateGameShortcut";

            long? placeId = GameShortcuts.ParsePlaceId(GameShortcutInput);

            if (placeId is null)
            {
                Frontend.ShowMessageBox(Strings.Menu_Shortcuts_Game_Invalid, MessageBoxImage.Error);
                return;
            }

            try
            {
                string name = await GameShortcuts.Create(placeId.Value);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Shortcuts_Game_Created, name), MessageBoxImage.Information);

                GameShortcutInput = "";
                OnPropertyChanged(nameof(GameShortcutInput));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Shortcuts_Game_Failed, ex.Message), MessageBoxImage.Error);
            }
        }
    }
}
