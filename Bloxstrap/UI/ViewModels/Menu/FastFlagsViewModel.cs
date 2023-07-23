using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class FastFlagsViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand OpenClientSettingsCommand => new RelayCommand(OpenClientSettings);

        private void OpenClientSettings() => Utilities.ShellExecute(Path.Combine(Directories.Modifications, "ClientSettings\\ClientAppSettings.json"));

        public Visibility ShowDebugFlags => App.Settings.Prop.OhHeyYouFoundMe ? Visibility.Visible : Visibility.Collapsed;

        public bool HttpRequestLogging
        {
            get => App.FastFlags.GetPreset("HTTP.Log") is not null;
            set => App.FastFlags.SetPreset("HTTP.Log", value ? 12 : null);
        }

        public string HttpRequestProxy
        {
            get => App.FastFlags.GetPreset("HTTP.Proxy.Address.1") ?? "";

            set
            {
                App.FastFlags.SetPreset("HTTP.Proxy.Enable", String.IsNullOrEmpty(value) ? null : true);
                App.FastFlags.SetPreset("HTTP.Proxy.Address", String.IsNullOrEmpty(value) ? null : value);
            }
        }

        public string StateOverlayFlags
        {
            get => App.FastFlags.GetPreset("UI.FlagState") ?? "";
            set => App.FastFlags.SetPreset("UI.FlagState", String.IsNullOrEmpty(value) ? null : value);
        }

        public int FramerateLimit
        {
            get => int.TryParse(App.FastFlags.GetPreset("Rendering.Framerate"), out int x) ? x : 60;
            set => App.FastFlags.SetPreset("Rendering.Framerate", value);
        }

        public IReadOnlyDictionary<string, string> RenderingModes => FastFlagManager.RenderingModes;

        public string SelectedRenderingMode
        {
            get => App.FastFlags.GetPresetEnum(RenderingModes, "Rendering.Mode", "True");
            set => App.FastFlags.SetPresetEnum("Rendering.Mode", RenderingModes[value], "True");
        }

        public bool AlternateGraphicsSelectorEnabled
        {
            get => App.FastFlags.GetPreset("UI.Menu.GraphicsSlider") == "True";
            set => App.FastFlags.SetPreset("UI.Menu.GraphicsSlider", value ? "True" : null);
        }

        public bool Pre2022TexturesEnabled
        {
            get => App.FastFlags.GetPreset("Rendering.TexturePack") == FastFlagManager.OldTexturesFlagValue;
            set => App.FastFlags.SetPreset("Rendering.TexturePack", value ? FastFlagManager.OldTexturesFlagValue : null);
        }

        public IReadOnlyDictionary<string, Dictionary<string, string?>> IGMenuVersions => FastFlagManager.IGMenuVersions;

        public string SelectedIGMenuVersion
        {
            get
            {
                // yeah this kinda sucks
                foreach (var version in IGMenuVersions)
                {
                    bool flagsMatch = true;

                    foreach (var flag in version.Value)
                    {
                        if (App.FastFlags.GetPreset($"UI.Menu.Style.{flag.Key}") != flag.Value)
                            flagsMatch = false;
                    }

                    if (flagsMatch)
                        return version.Key;
                }

                return IGMenuVersions.First().Key;
            }

            set
            {
                foreach (var flag in IGMenuVersions[value])
                    App.FastFlags.SetPreset($"UI.Menu.Style.{flag.Key}", flag.Value);
            }
        }

        public IReadOnlyDictionary<string, string> LightingModes => FastFlagManager.LightingModes;

        public string SelectedLightingMode
        {
            get => App.FastFlags.GetPresetEnum(LightingModes, "Rendering.Lighting", "True");
            set => App.FastFlags.SetPresetEnum("Rendering.Lighting", LightingModes[value], "True");
        }

        public bool GuiHidingEnabled
        {
            get => App.FastFlags.GetPreset("UI.Hide") == "32380007";
            set => App.FastFlags.SetPreset("UI.Hide", value ? "32380007" : null);
        }
    }
}
