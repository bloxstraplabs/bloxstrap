using System.Diagnostics;
using System.Windows.Input;
using Bloxstrap.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.ViewModels
{
    public class ModsViewModel
    {
        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

        public bool CanOpenModsFolder => !App.IsFirstRun;

        private void OpenModsFolder()
        {
            Process.Start("explorer.exe", Directories.Modifications);
        }

        public bool OldDeathSoundEnabled
        {
            get => App.Settings.UseOldDeathSound;
            set => App.Settings.UseOldDeathSound = value;
        }

        public bool OldMouseCursorEnabled
        {
            get => App.Settings.UseOldMouseCursor;
            set => App.Settings.UseOldMouseCursor = value;
        }

        public bool DisableAppPatchEnabled
        {
            get => App.Settings.UseDisableAppPatch;
            set => App.Settings.UseDisableAppPatch = value;
        }
    }
}
