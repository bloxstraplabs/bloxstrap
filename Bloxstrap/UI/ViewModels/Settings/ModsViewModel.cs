using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Microsoft.Win32;

using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.Foundation;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Models.SettingTasks;
using Bloxstrap.AppData;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class ModsViewModel : NotifyPropertyChangedViewModel
    {
        public ModsViewModel()
        {
            _previewTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, (_, _) => RefreshPreview(), Dispatcher.CurrentDispatcher);

            GradientStops.Add(new GradientStopItem(0, "#FFFFFF", QueuePreview));
            GradientStops.Add(new GradientStopItem(1, "#9B6DFF", QueuePreview));

            QueuePreview();
        }

        private void OpenModsFolder() => Process.Start("explorer.exe", Paths.Modifications);

        private readonly Dictionary<string, byte[]> FontHeaders = new()
        {
            { "ttf", new byte[4] { 0x00, 0x01, 0x00, 0x00 } },
            { "otf", new byte[4] { 0x4F, 0x54, 0x54, 0x4F } },
            { "ttc", new byte[4] { 0x74, 0x74, 0x63, 0x66 } } 
        };

        private void ManageCustomFont()
        {
            if (!String.IsNullOrEmpty(TextFontTask.NewState))
            {
                TextFontTask.NewState = "";
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = $"{Strings.Menu_FontFiles}|*.ttf;*.otf;*.ttc"
                };

                if (dialog.ShowDialog() != true)
                    return;

                string type = dialog.FileName.Substring(dialog.FileName.Length-3, 3).ToLowerInvariant();

                if (!FontHeaders.ContainsKey(type) 
                    || !FontHeaders.Any(x => File.ReadAllBytes(dialog.FileName).Take(4).SequenceEqual(x.Value)))
                {
                    Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_CustomFont_Invalid, MessageBoxImage.Error);
                    return;
                }

                TextFontTask.NewState = dialog.FileName;
            }

            OnPropertyChanged(nameof(ChooseCustomFontVisibility));
            OnPropertyChanged(nameof(DeleteCustomFontVisibility));
        }

        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

        public Visibility ChooseCustomFontVisibility => !String.IsNullOrEmpty(TextFontTask.NewState) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility DeleteCustomFontVisibility => !String.IsNullOrEmpty(TextFontTask.NewState) ? Visibility.Visible : Visibility.Collapsed;

        public ICommand ManageCustomFontCommand => new RelayCommand(ManageCustomFont);

        public ICommand OpenCompatSettingsCommand => new RelayCommand(OpenCompatSettings);

        public ModPresetTask OldAvatarBackgroundTask { get; } = new("OldAvatarBackground", @"ExtraContent\places\Mobile.rbxl", "OldAvatarBackground.rbxl");

        public ModPresetTask OldCharacterSoundsTask { get; } = new("OldCharacterSounds", new()
        {
            { @"content\sounds\action_footsteps_plastic.mp3", "Sounds.OldWalk.mp3"  },
            { @"content\sounds\action_jump.mp3",              "Sounds.OldJump.mp3"  },
            { @"content\sounds\action_get_up.mp3",            "Sounds.OldGetUp.mp3" },
            { @"content\sounds\action_falling.mp3",           "Sounds.Empty.mp3"    },
            { @"content\sounds\action_jump_land.mp3",         "Sounds.Empty.mp3"    },
            { @"content\sounds\action_swim.mp3",              "Sounds.Empty.mp3"    },
            { @"content\sounds\impact_water.mp3",             "Sounds.Empty.mp3"    }
        });

        public EmojiModPresetTask EmojiFontTask { get; } = new();

        public EnumModPresetTask<Enums.CursorType> CursorTypeTask { get; } = new("CursorType", new()
        {
            {
                Enums.CursorType.From2006, new()
                {
                    { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2006.ArrowCursor.png"    },
                    { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2006.ArrowFarCursor.png" }
                }
            },
            {
                Enums.CursorType.From2013, new()
                {
                    { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2013.ArrowCursor.png"    },
                    { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2013.ArrowFarCursor.png" }
                }
            }
        });

        public FontModPresetTask TextFontTask { get; } = new();

        private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47 };

        private static readonly byte[] OggHeader = { 0x4F, 0x67, 0x67, 0x53 };

        public FileModViewModel CustomCursor { get; } = new(new FileModTask("CustomCursor", ModGenerator.CursorFiles), $"{Strings.Menu_Mods_CustomFiles_ImageFiles}|*.png", PngHeader);

        public FileModViewModel CustomShiftLock { get; } = new(new FileModTask("CustomShiftLock", ModGenerator.ShiftLockFile), $"{Strings.Menu_Mods_CustomFiles_ImageFiles}|*.png", PngHeader);

        public FileModViewModel CustomDeathSound { get; } = new(new FileModTask("CustomDeathSound", ModGenerator.DeathSoundFile), $"{Strings.Menu_Mods_CustomFiles_SoundFiles}|*.ogg", OggHeader);

        public bool ApplyModsToPlayer
        {
            get => App.Settings.Prop.ApplyModsToPlayer;
            set => App.Settings.Prop.ApplyModsToPlayer = value;
        }

        public bool ApplyModsToStudio
        {
            get => App.Settings.Prop.ApplyModsToStudio;
            set => App.Settings.Prop.ApplyModsToStudio = value;
        }

        // Skybox

        private static string SkyboxFolder => Path.Combine(Paths.Modifications, "PlatformContent", "pc", "textures", "sky");

        private static IEnumerable<string> SkyboxFiles => Directory.Exists(SkyboxFolder)
            ? Directory.EnumerateFiles(SkyboxFolder, "*.tex").Where(x => Path.GetFileName(x).StartsWith("sky512_", StringComparison.OrdinalIgnoreCase))
            : Enumerable.Empty<string>();

        public string SkyboxStatus => SkyboxFiles.Any() ? Strings.Menu_Mods_Skybox_Applied : Strings.Menu_Mods_Skybox_None;

        public ICommand ImportSkyboxCommand => new RelayCommand(ImportSkybox);

        public ICommand RemoveSkyboxCommand => new RelayCommand(RemoveSkybox);

        private void ImportSkybox()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = Strings.Menu_Mods_Skybox_Pick, UseDescriptionForTitle = true };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var files = Directory.EnumerateFiles(dialog.SelectedPath, "*.tex", SearchOption.AllDirectories)
                .Where(x => Path.GetFileName(x).StartsWith("sky512_", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!files.Any())
            {
                Frontend.ShowMessageBox(Strings.Menu_Mods_Skybox_Invalid, MessageBoxImage.Error);
                return;
            }

            try
            {
                Directory.CreateDirectory(SkyboxFolder);

                foreach (string file in files)
                {
                    string target = Path.Combine(SkyboxFolder, Path.GetFileName(file).ToLowerInvariant());
                    Filesystem.AssertReadOnly(target);
                    File.Copy(file, target, true);
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ModsViewModel::ImportSkybox", ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }

            OnPropertyChanged(nameof(SkyboxStatus));
        }

        private void RemoveSkybox()
        {
            foreach (string file in SkyboxFiles.ToList())
            {
                Filesystem.AssertReadOnly(file);
                File.Delete(file);
            }

            OnPropertyChanged(nameof(SkyboxStatus));
        }

        // Color Mods

        public class GradientStopItem : NotifyPropertyChangedViewModel
        {
            private readonly Action _changed;

            private double _offset;

            private string _hex;

            public GradientStopItem(double offset, string hex, Action changed)
            {
                _offset = offset;
                _hex = hex;
                _changed = changed;
            }

            public double Offset
            {
                get => _offset;
                set
                {
                    _offset = value;
                    OnPropertyChanged(nameof(Offset));
                    _changed();
                }
            }

            public string Hex
            {
                get => _hex;
                set
                {
                    _hex = value;
                    OnPropertyChanged(nameof(Hex));
                    OnPropertyChanged(nameof(Swatch));
                    _changed();
                }
            }

            public Brush Swatch => ModGenerator.TryParseColor(Hex, out var color) ? new SolidColorBrush(color) : Brushes.Transparent;
        }

        private readonly DispatcherTimer _previewTimer;

        private double _gradientAngle = 90;

        public ObservableCollection<GradientStopItem> GradientStops { get; } = new();

        public ObservableCollection<BitmapSource> IconPreview { get; } = new();

        public double GradientAngle
        {
            get => _gradientAngle;
            set
            {
                _gradientAngle = value;
                OnPropertyChanged(nameof(GradientAngle));
                QueuePreview();
            }
        }

        public bool ColorIcons { get; set; } = true;

        public bool ColorCursors { get; set; } = true;

        public bool ColorShiftLock { get; set; } = true;

        public bool ColorEmoteWheel { get; set; } = false;

        public ICommand AddGradientStopCommand => new RelayCommand(() =>
        {
            GradientStops.Add(new GradientStopItem(1, "#FFFFFF", QueuePreview));
            QueuePreview();
        });

        public ICommand RemoveGradientStopCommand => new RelayCommand<GradientStopItem>(item =>
        {
            if (item is null || GradientStops.Count <= 1)
                return;

            GradientStops.Remove(item);
            QueuePreview();
        });

        public ICommand GenerateColorModsCommand => new AsyncRelayCommand(GenerateColorMods);

        public ICommand RemoveColorModsCommand => new RelayCommand(RemoveColorMods);

        private void QueuePreview()
        {
            _previewTimer.Stop();
            _previewTimer.Start();
        }

        private List<IconGenerator.Stop>? ParseStops()
        {
            var stops = new List<IconGenerator.Stop>();

            foreach (var item in GradientStops)
            {
                if (!ModGenerator.TryParseColor(item.Hex, out var color))
                    return null;

                stops.Add(new IconGenerator.Stop(item.Offset, color));
            }

            return stops.Any() ? stops.OrderBy(x => x.Offset).ToList() : null;
        }

        private void RefreshPreview()
        {
            _previewTimer.Stop();

            if (ParseStops() is not List<IconGenerator.Stop> stops)
                return;

            var sheets = IconGenerator.FindSheets(new RobloxPlayerData().Directory)
                .Where(x => x.Contains("_2x_", StringComparison.Ordinal))
                .Select(ModGenerator.GetOriginal)
                .OfType<string>()
                .ToList();

            if (!sheets.Any())
                return;

            double angle = GradientAngle;

            Task.Run(() => IconGenerator.Preview(sheets, stops, angle, 40)).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    App.Logger.WriteException("ModsViewModel::RefreshPreview", task.Exception!);
                    return;
                }

                IconPreview.Clear();

                foreach (var icon in task.Result)
                    IconPreview.Add(icon);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task GenerateColorMods()
        {
            const string LOG_IDENT = "ModsViewModel::GenerateColorMods";

            if (ParseStops() is not List<IconGenerator.Stop> stops)
            {
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Mods_ColorMods_InvalidColor, String.Join(", ", GradientStops.Select(x => x.Hex))), MessageBoxImage.Error);
                return;
            }

            int written = 0;

            bool icons = ColorIcons, cursors = ColorCursors, shiftLock = ColorShiftLock, emoteWheel = ColorEmoteWheel;
            double angle = GradientAngle;
            string robloxDirectory = new RobloxPlayerData().Directory;

            try
            {
                written = await Task.Run(() =>
                {
                    int count = 0;

                    if (icons)
                        count += ModGenerator.RecolorGradient(IconGenerator.FindSheets(robloxDirectory), stops, angle, true);

                    if (cursors)
                        count += ModGenerator.RecolorGradient(ModGenerator.CursorFiles, stops, angle, false);

                    if (shiftLock)
                        count += ModGenerator.RecolorGradient(new[] { ModGenerator.ShiftLockFile }, stops, angle, false);

                    if (emoteWheel)
                        count += ModGenerator.RecolorGradient(ModGenerator.EmoteWheelFiles, stops, angle, false);

                    return count;
                });
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                return;
            }

            if (written == 0)
                Frontend.ShowMessageBox(Strings.Menu_Mods_ColorMods_NoOriginals, MessageBoxImage.Warning);
            else
                Frontend.ShowMessageBox(Strings.Menu_Mods_ColorMods_Generated, MessageBoxImage.Information);
        }

        private void RemoveColorMods()
        {
            var result = Frontend.ShowMessageBox(Strings.Menu_Mods_ColorMods_RemoveConfirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            ModGenerator.RemoveGenerated(ModGenerator.CursorFiles.Append(ModGenerator.ShiftLockFile).Concat(ModGenerator.EmoteWheelFiles).Concat(IconGenerator.FindSheets(new RobloxPlayerData().Directory)));
        }

        // Mod Packs

        public ICommand ExportModPackCommand => new RelayCommand(ExportModPack);

        public ICommand ImportModPackCommand => new RelayCommand(ImportModPack);

        private void ExportModPack()
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"{App.ProjectName}-modpack.zip",
                Filter = $"{Strings.Menu_Mods_ModPack_Files}|*.zip"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                int count = Backups.ExportModPack(dialog.FileName);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Mods_ModPack_Exported, count), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ModsViewModel::ExportModPack", ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        }

        private void ImportModPack()
        {
            var dialog = new OpenFileDialog { Filter = $"{Strings.Menu_Mods_ModPack_Files}|*.zip" };

            if (dialog.ShowDialog() != true)
                return;

            var result = Frontend.ShowMessageBox(Strings.Menu_Mods_ModPack_ImportConfirm, MessageBoxImage.Question, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                int count = Backups.ImportModPack(dialog.FileName);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Mods_ModPack_Imported, count), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ModsViewModel::ImportModPack", ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        }

        // Cursor Sets

        public ObservableCollection<string> CursorSets { get; } = new(ModGenerator.GetCursorSets());

        public string? SelectedCursorSet { get; set; }

        public string NewCursorSetName { get; set; } = "";

        public ICommand SaveCursorSetCommand => new RelayCommand(SaveCursorSet);

        public ICommand ApplyCursorSetCommand => new RelayCommand(ApplyCursorSet);

        public ICommand DeleteCursorSetCommand => new RelayCommand(DeleteCursorSet);

        private void ReloadCursorSets()
        {
            CursorSets.Clear();

            foreach (string name in ModGenerator.GetCursorSets())
                CursorSets.Add(name);
        }

        private void SaveCursorSet()
        {
            string name = NewCursorSetName.Trim();

            if (String.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                Frontend.ShowMessageBox(Strings.Menu_FastFlags_Profiles_InvalidName, MessageBoxImage.Error);
                return;
            }

            if (!ModGenerator.SaveCursorSet(name))
            {
                Frontend.ShowMessageBox(Strings.Menu_Mods_CursorSets_NothingToSave, MessageBoxImage.Information);
                return;
            }

            ReloadCursorSets();

            SelectedCursorSet = name;
            NewCursorSetName = "";

            OnPropertyChanged(nameof(SelectedCursorSet));
            OnPropertyChanged(nameof(NewCursorSetName));
        }

        private void ApplyCursorSet()
        {
            if (SelectedCursorSet is null)
                return;

            ModGenerator.ApplyCursorSet(SelectedCursorSet);
            Frontend.ShowMessageBox(String.Format(Strings.Menu_Mods_CursorSets_Applied, SelectedCursorSet), MessageBoxImage.Information);
        }

        private void DeleteCursorSet()
        {
            if (SelectedCursorSet is null)
                return;

            var result = Frontend.ShowMessageBox(String.Format(Strings.Menu_Mods_CursorSets_DeleteConfirm, SelectedCursorSet), MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            ModGenerator.DeleteCursorSet(SelectedCursorSet);

            SelectedCursorSet = null;
            OnPropertyChanged(nameof(SelectedCursorSet));

            ReloadCursorSets();
        }

        private void OpenCompatSettings()
        {
            string path = new RobloxPlayerData().ExecutablePath;

            if (File.Exists(path))
                PInvoke.SHObjectProperties(HWND.Null, SHOP_TYPE.SHOP_FILEPATH, path, "Compatibility");
            else
                Frontend.ShowMessageBox(Strings.Common_RobloxNotInstalled, MessageBoxImage.Error);

        }
    }
}
