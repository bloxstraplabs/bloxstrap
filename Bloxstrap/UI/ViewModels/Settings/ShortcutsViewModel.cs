using Bloxstrap.Models.SettingTasks;
using Bloxstrap.Resources;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class ShortcutsViewModel : NotifyPropertyChangedViewModel
    {
        private ShortcutTask _desktopIconTask = new(Path.Combine(Paths.Desktop, "Bloxstrap.lnk"))
        { 
            Name = "DesktopIcon"
        };

        private ShortcutTask _startMenuIconTask = new(Path.Combine(Paths.WindowsStartMenu, "Bloxstrap.lnk"))
        {
            Name = "StartMenuIcon"
        };

        private ShortcutTask _playerIconTask = new(Path.Combine(Paths.Desktop, $"{Strings.LaunchMenu_LaunchRoblox}.lnk"))
        {
            Name = "RobloxPlayerIcon",
            ExeFlags = "-player"
        };

        private ShortcutTask _settingsIconTask = new(Path.Combine(Paths.Desktop, $"{Strings.Menu_Title}.lnk"))
        {
            Name = "SettingsIcon",
            ExeFlags = "-settings"
        };

        public bool DesktopIcon
        {
            get => _desktopIconTask.NewState;
            set => _desktopIconTask.NewState = value;
        }

        public bool StartMenuIcon
        {
            get => _startMenuIconTask.NewState;
            set => _startMenuIconTask.NewState = value;
        }

        public bool PlayerIcon
        {
            get => _playerIconTask.NewState;
            set => _playerIconTask.NewState = value;
        }

        public bool SettingsIcon
        {
            get => _settingsIconTask.NewState;
            set => _settingsIconTask.NewState = value;
        }
    }
}
