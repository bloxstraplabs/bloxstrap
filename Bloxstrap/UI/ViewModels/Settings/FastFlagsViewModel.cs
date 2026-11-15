using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Enums.FlagPresets;
using Bloxstrap.Resources;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class FastFlagsViewModel : NotifyPropertyChangedViewModel
    {
        private Dictionary<string, object>? _preResetFlags;

        public event EventHandler? RequestPageReloadEvent;
        
        public event EventHandler? OpenFlagEditorEvent;

        private void OpenFastFlagEditor() => OpenFlagEditorEvent?.Invoke(this, EventArgs.Empty);

        public ICommand OpenFastFlagEditorCommand => new RelayCommand(OpenFastFlagEditor);
        public ICommand ApplyPerformancePresetCommand => new RelayCommand(() => ApplyClientPreset("performance"));
        public ICommand ApplyQualityPresetCommand => new RelayCommand(() => ApplyClientPreset("quality"));

        public string ClientPresetLabel => ConfigurationProfileManager.GetClientPreset() switch
        {
            "performance" => LumiFeatureStrings.Menu_FastFlags_ClientPreset_Performance,
            "quality" => LumiFeatureStrings.Menu_FastFlags_ClientPreset_Quality,
            _ => LumiFeatureStrings.Menu_FastFlags_ClientPreset_Custom
        };

        public void RefreshPresetState()
        {
            OnPropertyChanged(nameof(ClientPresetLabel));
            OnPropertyChanged(nameof(SelectedMSAALevel));
            OnPropertyChanged(nameof(SelectedRenderingMode));
            OnPropertyChanged(nameof(FixDisplayScaling));
            OnPropertyChanged(nameof(MeshDetailEnabled));
            OnPropertyChanged(nameof(MeshDetail));
            OnPropertyChanged(nameof(FRMQualityOverrideEnabled));
            OnPropertyChanged(nameof(FRMQualityOverride));
            OnPropertyChanged(nameof(SelectedTextureQuality));
        }

        private void ApplyClientPreset(string presetName)
        {
            string presetLabel = presetName == "performance"
                ? LumiFeatureStrings.Menu_FastFlags_ClientPreset_Performance
                : LumiFeatureStrings.Menu_FastFlags_ClientPreset_Quality;
            string prompt = String.Format(LumiFeatureStrings.Menu_FastFlags_ClientPreset_ApplyPrompt, presetLabel);

            if (Frontend.ShowMessageBox(prompt, MessageBoxImage.Warning, MessageBoxButton.YesNo, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            try
            {
                ConfigurationProfileManager.ApplyClientPreset(presetName);
                RefreshPresetState();
                Frontend.ShowMessageBox(
                    String.Format(LumiFeatureStrings.Menu_FastFlags_ClientPreset_Applied, presetLabel),
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("FastFlagsViewModel::ApplyClientPreset", ex);
                Frontend.ShowMessageBox(
                    String.Format(LumiFeatureStrings.Menu_FastFlags_ClientPreset_ApplyFailed, ex.Message),
                    MessageBoxImage.Error);
            }
        }

        public Visibility CanShowFastFlagEditor => App.IsStudioInstalled ? Visibility.Visible : Visibility.Collapsed;

        public bool UseFastFlagManager
        {
            get => App.Settings.Prop.UseFastFlagManager;
            set => App.Settings.Prop.UseFastFlagManager = value;
        }

        public IReadOnlyDictionary<MSAAMode, string?> MSAALevels => FastFlagManager.MSAAModes;

        public MSAAMode SelectedMSAALevel
        {
            get => MSAALevels.FirstOrDefault(x => x.Value == App.FastFlags.GetPreset("Rendering.MSAA")).Key;
            set
            {
                App.FastFlags.SetPreset("Rendering.MSAA", MSAALevels[value]);
                RefreshPresetState();
            }
        }

        public IReadOnlyDictionary<RenderingMode, string> RenderingModes => FastFlagManager.RenderingModes;

        public RenderingMode SelectedRenderingMode
        {
            get => App.FastFlags.GetPresetEnum(RenderingModes, "Rendering.Mode", "True");
            set
            {
                App.FastFlags.SetPresetEnum("Rendering.Mode", value.ToString(), "True");
                RefreshPresetState();
            }
        }

        public bool FixDisplayScaling
        {
            get => App.FastFlags.GetPreset("Rendering.DisableScaling") == "True";
            set
            {
                App.FastFlags.SetPreset("Rendering.DisableScaling", value ? "True" : null);
                RefreshPresetState();
            }
        }

        private static readonly string[] MeshDetailLevels = { "L0", "L12", "L23", "L34" };

        public bool MeshDetailEnabled
        {
            get => App.FastFlags.GetPreset("Geometry.MeshLOD.Static") is not null;
            set
            {
                if (value)
                {
                    MeshDetail = 3;
                }
                else
                {
                    foreach (string level in MeshDetailLevels)
                        App.FastFlags.SetPreset($"Geometry.MeshLOD.{level}", null);

                    App.FastFlags.SetPreset("Geometry.MeshLOD.Static", null);
                }

                OnPropertyChanged(nameof(MeshDetailEnabled));
                RefreshPresetState();
            }
        }

        public int MeshDetail
        {
            get => int.TryParse(App.FastFlags.GetPreset("Geometry.MeshLOD.Static"), out int value) ? value : 0;
            set
            {
                int selectedLevel = Math.Clamp(value, 0, MeshDetailLevels.Length - 1);

                for (int index = 0; index < MeshDetailLevels.Length; index++)
                {
                    int distance = (Math.Clamp(selectedLevel - index, 0, 3) + 1) * 250;
                    App.FastFlags.SetPreset($"Geometry.MeshLOD.{MeshDetailLevels[index]}", distance);
                }

                App.FastFlags.SetPreset("Geometry.MeshLOD.Static", selectedLevel);
                OnPropertyChanged(nameof(MeshDetail));
                OnPropertyChanged(nameof(MeshDetailEnabled));
                RefreshPresetState();
            }
        }

        public bool FRMQualityOverrideEnabled
        {
            get => App.FastFlags.GetPreset("Rendering.FRMQualityOverride") is not null;
            set
            {
                if (value)
                    FRMQualityOverride = 21;
                else
                    App.FastFlags.SetPreset("Rendering.FRMQualityOverride", null);

                OnPropertyChanged(nameof(FRMQualityOverrideEnabled));
                OnPropertyChanged(nameof(FRMQualityOverride));
                RefreshPresetState();
            }
        }

        public int FRMQualityOverride
        {
            get => int.TryParse(App.FastFlags.GetPreset("Rendering.FRMQualityOverride"), out int value) ? value : 21;
            set
            {
                App.FastFlags.SetPreset("Rendering.FRMQualityOverride", Math.Clamp(value, 1, 21));
                OnPropertyChanged(nameof(FRMQualityOverride));
                RefreshPresetState();
            }
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

                RefreshPresetState();
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
