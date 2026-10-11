using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Enums.FlagPresets;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class FastFlagsViewModel : NotifyPropertyChangedViewModel
    {
        private Dictionary<string, object>? _preResetFlags;

        public event EventHandler? RequestPageReloadEvent;
        
        public event EventHandler? OpenFlagEditorEvent;

        private void OpenFastFlagEditor() => OpenFlagEditorEvent?.Invoke(this, EventArgs.Empty);

        public ICommand OpenFastFlagEditorCommand => new RelayCommand(OpenFastFlagEditor);

        public IReadOnlyList<AllowlistedFlagViewModel> AllowlistedFlags { get; } = FastFlagAllowlist.Flags.Select(x => new AllowlistedFlagViewModel(x)).ToList();

        public ObservableCollection<string> Profiles { get; } = new();

        public string? SelectedProfile { get; set; }

        public string NewProfileName { get; set; } = "";

        public ICommand SaveProfileCommand => new RelayCommand(SaveProfile);

        public ICommand LoadProfileCommand => new RelayCommand(LoadProfile);

        public ICommand DeleteProfileCommand => new RelayCommand(DeleteProfile);

        public FastFlagsViewModel()
        {
            ReloadProfiles();
        }

        private static string GetProfilePath(string name) => Path.Combine(Paths.FastFlagProfiles, $"{name}.json");

        private void ReloadProfiles()
        {
            Profiles.Clear();

            if (!Directory.Exists(Paths.FastFlagProfiles))
                return;

            foreach (string file in Directory.GetFiles(Paths.FastFlagProfiles, "*.json").OrderBy(x => x))
                Profiles.Add(Path.GetFileNameWithoutExtension(file));
        }

        private void SaveProfile()
        {
            const string LOG_IDENT = "FastFlagsViewModel::SaveProfile";

            string name = NewProfileName.Trim();

            if (String.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                Frontend.ShowMessageBox(Strings.Menu_FastFlags_Profiles_InvalidName, MessageBoxImage.Error);
                return;
            }

            if (Profiles.Contains(name))
            {
                var result = Frontend.ShowMessageBox(String.Format(Strings.Menu_FastFlags_Profiles_Overwrite, name), MessageBoxImage.Question, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                Directory.CreateDirectory(Paths.FastFlagProfiles);
                File.WriteAllText(GetProfilePath(name), JsonSerializer.Serialize(App.FastFlags.Prop, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                return;
            }

            ReloadProfiles();

            SelectedProfile = name;
            NewProfileName = "";

            OnPropertyChanged(nameof(SelectedProfile));
            OnPropertyChanged(nameof(NewProfileName));
        }

        private void LoadProfile()
        {
            const string LOG_IDENT = "FastFlagsViewModel::LoadProfile";

            if (SelectedProfile is null)
                return;

            var result = Frontend.ShowMessageBox(String.Format(Strings.Menu_FastFlags_Profiles_LoadConfirm, SelectedProfile), MessageBoxImage.Question, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var flags = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(GetProfilePath(SelectedProfile)));

                if (flags is null)
                    throw new Exception("JSON deserialization returned null");

                App.FastFlags.Prop.Clear();

                foreach (var pair in flags)
                    App.FastFlags.SetValue(pair.Key, pair.Value);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                return;
            }

            RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteProfile()
        {
            if (SelectedProfile is null)
                return;

            var result = Frontend.ShowMessageBox(String.Format(Strings.Menu_FastFlags_Profiles_DeleteConfirm, SelectedProfile), MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            File.Delete(GetProfilePath(SelectedProfile));

            SelectedProfile = null;
            OnPropertyChanged(nameof(SelectedProfile));

            ReloadProfiles();
        }

        public bool UseFastFlagManager
        {
            get => App.Settings.Prop.UseFastFlagManager;
            set => App.Settings.Prop.UseFastFlagManager = value;
        }

        public IReadOnlyDictionary<MSAAMode, string?> MSAALevels => FastFlagManager.MSAAModes;

        public MSAAMode SelectedMSAALevel
        {
            get => MSAALevels.FirstOrDefault(x => x.Value == App.FastFlags.GetPreset("Rendering.MSAA")).Key;
            set => App.FastFlags.SetPreset("Rendering.MSAA", MSAALevels[value]);
        }

        public bool FixDisplayScaling
        {
            get => App.FastFlags.GetPreset("Rendering.DisableScaling") == "True";
            set => App.FastFlags.SetPreset("Rendering.DisableScaling", value ? "True" : null);
        }

        public IReadOnlyDictionary<TextureQuality, string?> TextureQualities => FastFlagManager.TextureQualityLevels;

        public TextureQuality SelectedTextureQuality
        {
            get => TextureQualities.Where(x => x.Value == App.FastFlags.GetPreset("Rendering.TextureQuality.Level")).FirstOrDefault().Key;
            set
            {
                if (value == TextureQuality.Default)
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality", null);
                }
                else
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", "True");
                    App.FastFlags.SetPreset("Rendering.TextureQuality.Level", TextureQualities[value]);
                }
            }
        }
        public bool ResetConfiguration
        {
            get => _preResetFlags is not null;

            set
            {
                if (value)
                {
                    _preResetFlags = new(App.FastFlags.Prop);
                    App.FastFlags.Prop.Clear();
                }
                else
                {
                    App.FastFlags.Prop = _preResetFlags!;
                    _preResetFlags = null;
                }

                RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
