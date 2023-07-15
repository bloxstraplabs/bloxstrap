using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Bloxstrap.Enums;
using Bloxstrap.Extensions;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class ModsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OpenModsFolder() => Process.Start("explorer.exe", Directories.Modifications);

        private string _customFontLocation = Path.Combine(Directories.Modifications, "content\\fonts\\CustomFont.ttf");
        private bool _usingCustomFont => File.Exists(_customFontLocation);

        private void ManageCustomFont()
        {
            if (_usingCustomFont)
            {
                File.Delete(_customFontLocation);
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Font files|*.ttf;*.otf|All files|*.*"
                };

                if (dialog.ShowDialog() != true)
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(_customFontLocation)!);
                File.Copy(dialog.FileName, _customFontLocation);
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
