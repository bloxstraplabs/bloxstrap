using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.Menu.ViewModels
{
    public class ModsViewModel
    {
        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

        private void OpenModsFolder() => Process.Start("explorer.exe", Directories.Modifications);

        public bool OldDeathSoundEnabled
        {
            get => App.Settings.Prop.UseOldDeathSound;
            set => App.Settings.Prop.UseOldDeathSound = value;
        }

        public bool OldCharacterSoundsEnabled
        {
            get => App.Settings.Prop.UseOldCharacterSounds;
            set => App.Settings.Prop.UseOldCharacterSounds = value;
        }

        public bool OldMouseCursorEnabled
        {
            get => App.Settings.Prop.UseOldMouseCursor;
            set => App.Settings.Prop.UseOldMouseCursor = value;
        }

        public bool DisableAppPatchEnabled
        {
            get => App.Settings.Prop.UseDisableAppPatch;
            set => App.Settings.Prop.UseDisableAppPatch = value;
        }

        public bool DisableFullscreenOptimizationsEnabled
        {
            get => App.Settings.Prop.DisableFullscreenOptimizations;
            set => App.Settings.Prop.DisableFullscreenOptimizations = value;
        }
    }
}
