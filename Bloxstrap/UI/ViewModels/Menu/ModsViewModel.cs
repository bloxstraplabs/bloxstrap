using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class ModsViewModel : NotifyPropertyChangedViewModel
    {
        private void OpenModsFolder() => Process.Start("explorer.exe", Paths.Modifications);

        private bool _usingCustomFont => App.IsFirstRun && App.CustomFontLocation is not null || !App.IsFirstRun && File.Exists(Paths.CustomFont);

        private void ManageCustomFont()
        {
            if (_usingCustomFont)
            {
                if (App.IsFirstRun)
                    App.CustomFontLocation = null;
                else
                    File.Delete(Paths.CustomFont);
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Font files|*.ttf;*.otf|All files|*.*"
                };

                if (dialog.ShowDialog() != true)
                    return;

                if (App.IsFirstRun)
                {
                    App.CustomFontLocation = dialog.FileName;
                }
                else
                { 
                    Directory.CreateDirectory(Path.GetDirectoryName(Paths.CustomFont)!);
                    File.Copy(dialog.FileName, Paths.CustomFont);
                }
            }

            OnPropertyChanged(nameof(ChooseCustomFontVisibility));
            OnPropertyChanged(nameof(DeleteCustomFontVisibility));
        }

        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

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

        public IReadOnlyDictionary<string, Enums.CursorType> CursorTypes => CursorTypeEx.Selections;

        public string SelectedCursorType
        {
            get => CursorTypes.FirstOrDefault(x => x.Value == App.Settings.Prop.CursorType).Key;
            set => App.Settings.Prop.CursorType = CursorTypes[value];
        }

        public bool OldAvatarBackground
        {
            get => App.Settings.Prop.UseOldAvatarBackground;
            set => App.Settings.Prop.UseOldAvatarBackground = value;
        }

        public bool DisableAppPatchEnabled
        {
            get => App.Settings.Prop.UseDisableAppPatch;
            set => App.Settings.Prop.UseDisableAppPatch = value;
        }

        public IReadOnlyDictionary<string, EmojiType> EmojiTypes => EmojiTypeEx.Selections;

        public string SelectedEmojiType
        {
            get => EmojiTypes.FirstOrDefault(x => x.Value == App.Settings.Prop.EmojiType).Key;
            set => App.Settings.Prop.EmojiType = EmojiTypes[value];
        }

        public Visibility ChooseCustomFontVisibility => _usingCustomFont ? Visibility.Collapsed : Visibility.Visible;
        public Visibility DeleteCustomFontVisibility => _usingCustomFont ? Visibility.Visible : Visibility.Collapsed;

        public ICommand ManageCustomFontCommand => new RelayCommand(ManageCustomFont);

        public bool DisableFullscreenOptimizations
        {
            get => App.Settings.Prop.DisableFullscreenOptimizations;
            set => App.Settings.Prop.DisableFullscreenOptimizations = value;
        }
    }
}
