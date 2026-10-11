using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Models.SettingTasks;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class RobloxSettingsViewModel : NotifyPropertyChangedViewModel
    {
        private readonly RobloxSettingsTask _task = new();

        public Visibility EditorVisibility => _task.Exists ? Visibility.Visible : Visibility.Collapsed;

        public Visibility MissingVisibility => _task.Exists ? Visibility.Collapsed : Visibility.Visible;

        // Roblox Settings

        public string FramerateCap
        {
            get => _task.Get("FramerateCap") is string value && value != "-1" ? value : "";
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    _task.Set("int", "FramerateCap", "-1");
                else if (Int32.TryParse(value, out int cap) && cap > 0)
                    _task.Set("int", "FramerateCap", Math.Min(cap, 9999).ToString());
            }
        }

        public IReadOnlyList<string> QualityLevels { get; } = new[] { Strings.Common_Automatic }.Concat(Enumerable.Range(1, 10).Select(x => x.ToString())).ToList();

        public string QualityLevel
        {
            get => _task.Get("SavedQualityLevel") is string value && value != "0" ? value : Strings.Common_Automatic;
            set => _task.Set("token", "SavedQualityLevel", value == Strings.Common_Automatic ? "0" : value);
        }

        public double MasterVolume
        {
            get => Double.TryParse(_task.Get("MasterVolume"), NumberStyles.Float, CultureInfo.InvariantCulture, out double volume) ? volume * 100 : 100;
            set
            {
                _task.Set("float", "MasterVolume", (Math.Clamp(value, 0, 100) / 100).ToString("0.###", CultureInfo.InvariantCulture));
                OnPropertyChanged(nameof(MasterVolume));
            }
        }

        public string MouseSensitivity
        {
            get => Double.TryParse(_task.Get("MouseSensitivity"), NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value.ToString("0.##", CultureInfo.InvariantCulture) : "";
            set
            {
                if (Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double sensitivity) && sensitivity > 0)
                    _task.Set("float", "MouseSensitivity", Math.Min(sensitivity, 10).ToString("0.###", CultureInfo.InvariantCulture));
            }
        }

        private bool GetBool(string name) => String.Equals(_task.Get(name), "true", StringComparison.OrdinalIgnoreCase);

        private void SetBool(string name, bool value) => _task.Set("bool", name, value ? "true" : "false");

        public bool Fullscreen
        {
            get => GetBool("Fullscreen");
            set => SetBool("Fullscreen", value);
        }

        public bool PerformanceStats
        {
            get => GetBool("PerformanceStatsVisible");
            set => SetBool("PerformanceStatsVisible", value);
        }

        public bool ReducedMotion
        {
            get => GetBool("ReducedMotion");
            set => SetBool("ReducedMotion", value);
        }

        public bool CameraYInverted
        {
            get => GetBool("CameraYInverted");
            set => SetBool("CameraYInverted", value);
        }

        private double GetPercent(string name, double fallback) =>
            Double.TryParse(_task.Get(name), NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? Math.Round(value * 100) : fallback;

        private void SetPercent(string name, double value) =>
            _task.Set("float", name, (Math.Clamp(value, 0, 100) / 100).ToString("0.###", CultureInfo.InvariantCulture));

        public double StudioVolume
        {
            get => GetPercent("MasterVolumeStudio", 100);
            set
            {
                SetPercent("MasterVolumeStudio", value);
                OnPropertyChanged(nameof(StudioVolume));
            }
        }

        public double VoiceChatVolume
        {
            get => GetPercent("VoiceChatVolume", 100);
            set
            {
                SetPercent("VoiceChatVolume", value);
                OnPropertyChanged(nameof(VoiceChatVolume));
            }
        }

        public double PartyVolume
        {
            get => GetPercent("PartyVoiceVolume", 100);
            set
            {
                SetPercent("PartyVoiceVolume", value);
                OnPropertyChanged(nameof(PartyVolume));
            }
        }

        public double HapticStrength
        {
            get => GetPercent("HapticStrength", 100);
            set
            {
                SetPercent("HapticStrength", value);
                OnPropertyChanged(nameof(HapticStrength));
            }
        }

        public double UiTransparency
        {
            get => GetPercent("PreferredTransparency", 100);
            set
            {
                SetPercent("PreferredTransparency", value);
                OnPropertyChanged(nameof(UiTransparency));
            }
        }

        public bool Vignette
        {
            get => GetBool("VignetteEnabled");
            set => SetBool("VignetteEnabled", value);
        }

        public bool PlayerListVisible
        {
            get => GetBool("PlayerListVisible");
            set => SetBool("PlayerListVisible", value);
        }

        public bool ChatVisible
        {
            get => GetBool("ChatVisible");
            set => SetBool("ChatVisible", value);
        }

        public bool VREnabled
        {
            get => GetBool("VREnabled");
            set => SetBool("VREnabled", value);
        }

        public IReadOnlyList<string> TextSizes { get; } = new[] { "1", "2", "3", "4" };

        public string TextSize
        {
            get => _task.Get("PreferredTextSize") is string value && TextSizes.Contains(value) ? value : "1";
            set => _task.Set("token", "PreferredTextSize", value);
        }

        public ICommand OpenRobloxFolderCommand => new RelayCommand(() => Utilities.ShellExecute(Path.GetDirectoryName(RobloxSettingsTask.FileLocation)!));

        public ICommand ExportRobloxSettingsCommand => new RelayCommand(() =>
        {
            var dialog = new Microsoft.Win32.SaveFileDialog { FileName = "GlobalBasicSettings_13.xml", Filter = "XML|*.xml" };

            if (dialog.ShowDialog() == true && File.Exists(RobloxSettingsTask.FileLocation))
                File.Copy(RobloxSettingsTask.FileLocation, dialog.FileName, true);
        });

        public ICommand ImportRobloxSettingsCommand => new RelayCommand(() =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "XML|*.xml" };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                System.Xml.Linq.XDocument.Load(dialog.FileName);

                var attributes = File.Exists(RobloxSettingsTask.FileLocation) ? File.GetAttributes(RobloxSettingsTask.FileLocation) : FileAttributes.Normal;

                if (File.Exists(RobloxSettingsTask.FileLocation))
                    File.SetAttributes(RobloxSettingsTask.FileLocation, attributes & ~FileAttributes.ReadOnly);

                File.Copy(dialog.FileName, RobloxSettingsTask.FileLocation, true);
                File.SetAttributes(RobloxSettingsTask.FileLocation, attributes);

                App.PendingSettingTasks.Remove(_task.Name);

                Frontend.ShowMessageBox(Strings.Menu_Roblox_Import_Done, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("RobloxSettingsViewModel::Import", ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        });

        public bool LockSettingsFile
        {
            get => _task.LockFile;
            set => _task.LockFile = value;
        }

        // Window

        public string RobloxWindowTitle
        {
            get => App.Settings.Prop.RobloxWindowTitle;
            set => App.Settings.Prop.RobloxWindowTitle = value.Trim();
        }

        public bool RobloxBorderless
        {
            get => App.Settings.Prop.RobloxBorderless;
            set => App.Settings.Prop.RobloxBorderless = value;
        }

        // Crosshair

        public bool CrosshairEnabled
        {
            get => App.Settings.Prop.CrosshairEnabled;
            set => App.Settings.Prop.CrosshairEnabled = value;
        }

        public IEnumerable<CrosshairStyle> CrosshairStyles { get; } = Enum.GetValues<CrosshairStyle>();

        public CrosshairStyle CrosshairStyle
        {
            get => App.Settings.Prop.CrosshairStyle;
            set => App.Settings.Prop.CrosshairStyle = value;
        }

        public string CrosshairColor
        {
            get => App.Settings.Prop.CrosshairColor;
            set
            {
                if (ModGenerator.TryParseColor(value, out _))
                    App.Settings.Prop.CrosshairColor = value.Trim();
            }
        }

        public double CrosshairSize
        {
            get => App.Settings.Prop.CrosshairSize;
            set
            {
                App.Settings.Prop.CrosshairSize = value;
                OnPropertyChanged(nameof(CrosshairSize));
            }
        }

        public double CrosshairThickness
        {
            get => App.Settings.Prop.CrosshairThickness;
            set
            {
                App.Settings.Prop.CrosshairThickness = value;
                OnPropertyChanged(nameof(CrosshairThickness));
            }
        }

        public double CrosshairGap
        {
            get => App.Settings.Prop.CrosshairGap;
            set
            {
                App.Settings.Prop.CrosshairGap = value;
                OnPropertyChanged(nameof(CrosshairGap));
            }
        }

        // SOCD

        public bool SocdEnabled
        {
            get => App.Settings.Prop.SocdEnabled;
            set => App.Settings.Prop.SocdEnabled = value;
        }

        public IEnumerable<SocdLayout> SocdLayouts { get; } = Enum.GetValues<SocdLayout>();

        public SocdLayout SocdLayout
        {
            get => App.Settings.Prop.SocdLayout;
            set => App.Settings.Prop.SocdLayout = value;
        }

        public IEnumerable<SocdMode> SocdModes { get; } = Enum.GetValues<SocdMode>();

        public SocdMode SocdMode
        {
            get => App.Settings.Prop.SocdMode;
            set => App.Settings.Prop.SocdMode = value;
        }

        // Window

        public bool RobloxGameTitle
        {
            get => App.Settings.Prop.RobloxGameTitle;
            set => App.Settings.Prop.RobloxGameTitle = value;
        }

        public bool RobloxGameTitlePlayerCount
        {
            get => App.Settings.Prop.RobloxGameTitlePlayerCount;
            set => App.Settings.Prop.RobloxGameTitlePlayerCount = value;
        }

        public bool RobloxGameIcon
        {
            get => App.Settings.Prop.RobloxGameIcon;
            set => App.Settings.Prop.RobloxGameIcon = value;
        }

        public IEnumerable<RobloxWindowIcon> RobloxWindowIcons { get; } = Enum.GetValues<RobloxWindowIcon>();

        public RobloxWindowIcon RobloxWindowIcon
        {
            get => App.Settings.Prop.RobloxWindowIcon;
            set
            {
                if (value == RobloxWindowIcon.Custom && !File.Exists(App.Settings.Prop.RobloxIconPath))
                {
                    if (BrowseIcon() is not string path)
                    {
                        OnPropertyChanged(nameof(RobloxWindowIcon));
                        return;
                    }

                    App.Settings.Prop.RobloxIconPath = path;
                }

                App.Settings.Prop.RobloxWindowIcon = value;
                OnPropertyChanged(nameof(RobloxWindowIcon));
            }
        }

        public RobloxWindowIcon StudioWindowIcon
        {
            get => App.Settings.Prop.StudioWindowIcon;
            set
            {
                if (value == RobloxWindowIcon.Custom && !File.Exists(App.Settings.Prop.StudioIconPath))
                {
                    if (BrowseIcon() is not string path)
                    {
                        OnPropertyChanged(nameof(StudioWindowIcon));
                        return;
                    }

                    App.Settings.Prop.StudioIconPath = path;
                }

                App.Settings.Prop.StudioWindowIcon = value;
                OnPropertyChanged(nameof(StudioWindowIcon));
            }
        }

        private static string? BrowseIcon()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.ico;*.bmp" };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public ICommand BrowseRobloxIconCommand => new RelayCommand(() =>
        {
            if (BrowseIcon() is not string path)
                return;

            App.Settings.Prop.RobloxIconPath = path;
            RobloxWindowIcon = RobloxWindowIcon.Custom;
        });

        public ICommand BrowseStudioIconCommand => new RelayCommand(() =>
        {
            if (BrowseIcon() is not string path)
                return;

            App.Settings.Prop.StudioIconPath = path;
            StudioWindowIcon = RobloxWindowIcon.Custom;
        });

        // Roblox app

        public IEnumerable<RobloxAppTheme> RobloxAppThemes { get; } = Enum.GetValues<RobloxAppTheme>();

        public RobloxAppTheme RobloxAppTheme
        {
            get => App.Settings.Prop.RobloxAppTheme;
            set => App.Settings.Prop.RobloxAppTheme = value;
        }

        public bool RobloxBackgroundApp
        {
            get => App.Settings.Prop.RobloxBackgroundApp;
            set => App.Settings.Prop.RobloxBackgroundApp = value;
        }
    }
}
