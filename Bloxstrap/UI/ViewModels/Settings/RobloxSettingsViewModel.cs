using System.Windows;

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
    }
}
