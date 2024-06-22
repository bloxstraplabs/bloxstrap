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

        private readonly Dictionary<string, byte[]> FontHeaders = new()
        {
            { "ttf", new byte[4] { 0x00, 0x01, 0x00, 0x00 } },
            { "otf", new byte[4] { 0x4F, 0x54, 0x54, 0x4F } },
            { "ttc", new byte[4] { 0x74, 0x74, 0x63, 0x66 } } 
        };

        private void ManageCustomFont()
        {
            if (_usingCustomFont)
            {
                if (App.IsFirstRun)
                {
                    App.CustomFontLocation = null;
                }
                else
                {
                    Filesystem.AssertReadOnly(Paths.CustomFont);
                    File.Delete(Paths.CustomFont);
                }
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = $"{Resources.Strings.Menu_FontFiles}|*.ttf;*.otf;*.ttc"
                };

                if (dialog.ShowDialog() != true)
                    return;

                string type = dialog.FileName.Substring(dialog.FileName.Length-3, 3).ToLowerInvariant();

                if (!FontHeaders.ContainsKey(type) || !File.ReadAllBytes(dialog.FileName).Take(4).SequenceEqual(FontHeaders[type]))
                {
                    Frontend.ShowMessageBox(Resources.Strings.Menu_Mods_Misc_CustomFont_Invalid, MessageBoxImage.Error);
                    return;
                }

                if (App.IsFirstRun)
                {
                    App.CustomFontLocation = dialog.FileName;
                }
                else
                { 
                    Directory.CreateDirectory(Path.GetDirectoryName(Paths.CustomFont)!);
                    File.Copy(dialog.FileName, Paths.CustomFont);
                    Filesystem.AssertReadOnly(Paths.CustomFont);
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

        public IReadOnlyCollection<Enums.CursorType> CursorTypes => CursorTypeEx.Selections;

        public Enums.CursorType SelectedCursorType
        {
            get => App.Settings.Prop.CursorType;
            set => App.Settings.Prop.CursorType = value;
        }

        public bool OldAvatarBackground
        {
            get => App.Settings.Prop.UseOldAvatarBackground;
            set => App.Settings.Prop.UseOldAvatarBackground = value;
        }

        public IReadOnlyCollection<EmojiType> EmojiTypes => EmojiTypeEx.Selections;

        public EmojiType SelectedEmojiType
        {
            get => App.Settings.Prop.EmojiType;
            set => App.Settings.Prop.EmojiType = value;
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
