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
            get => App.FastFlags.GetValue("DFLogHttpTraceLight") is not null;
            set => App.FastFlags.SetValue("DFLogHttpTraceLight", value ? 12 : null);
        }

        public string HttpRequestProxy
        {
            get => App.FastFlags.GetValue("DFStringDebugPlayerHttpProxyUrl") ?? "";

            set
            {
                bool? boolValue = null;
                string? stringValue = null;

                if (!String.IsNullOrEmpty(value))
                {
                    boolValue = true;
                    stringValue = value;
                }

                App.FastFlags.SetValue("DFFlagDebugEnableHttpProxy", boolValue);
                App.FastFlags.SetValue("DFStringDebugPlayerHttpProxyUrl", stringValue);
                App.FastFlags.SetValue("DFStringHttpCurlProxyHostAndPort", stringValue);
                App.FastFlags.SetValue("DFStringHttpCurlProxyHostAndPortForExternalUrl", stringValue);
            }
        }

        public string StateOverlayFlags
        {
            get => App.FastFlags.GetValue("FStringDebugShowFlagState") ?? "";
            set => App.FastFlags.SetValue("FStringDebugShowFlagState", String.IsNullOrEmpty(value) ? null : value);
        }

        public int FramerateLimit
        {
            get => int.TryParse(App.FastFlags.GetValue("DFIntTaskSchedulerTargetFps"), out int x) ? x : 60;
            set => App.FastFlags.SetValue("DFIntTaskSchedulerTargetFps", value);
        }

        public IReadOnlyDictionary<string, string> RenderingModes => FastFlagManager.RenderingModes;

        public string SelectedRenderingMode
        {
            get
            {
                foreach (var mode in RenderingModes)
                {
                    if (App.FastFlags.GetValue(mode.Value) == "True")
                        return mode.Key;
                }

                return "Automatic";
            }

            set
            {
                foreach (var mode in RenderingModes)
                {
                    if (mode.Key != "Automatic")
                        App.FastFlags.SetValue(mode.Value, null);
                }

                if (value == "Automatic")
                    return;

                App.FastFlags.SetValue(RenderingModes[value], "True");
                App.FastFlags.SetValueIf(value == "Vulkan", "FFlagRenderVulkanFixMinimizeWindow", "True");
            }
        }

        public bool AlternateGraphicsSelectorEnabled
        {
            get => App.FastFlags.GetValue("FFlagFixGraphicsQuality") == "True";
            set => App.FastFlags.SetValue("FFlagFixGraphicsQuality", value ? "True" : null);
        }

        public bool Pre2022TexturesEnabled
        {
            get => App.FastFlags.GetValue("FStringPartTexturePackTable2022") == FastFlagManager.OldTexturesFlagValue;
            set => App.FastFlags.SetValue("FStringPartTexturePackTable2022", value ? FastFlagManager.OldTexturesFlagValue : null);
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
                        if (App.FastFlags.GetValue(flag.Key) != flag.Value)
                            flagsMatch = false;
                    }

                    if (flagsMatch)
                        return version.Key;
                }

                return "Default";
            }

            set
            {
                foreach (var flag in IGMenuVersions[value])
                {
                    App.FastFlags.SetValue(flag.Key, flag.Value);
                }
            }
        }

        public IReadOnlyDictionary<string, string> LightingTechnologies => FastFlagManager.LightingTechnologies;

        // this is basically the same as the code for rendering selection, maybe this could be abstracted in some way?
        public string SelectedLightingTechnology
        {
            get
            {
                foreach (var mode in LightingTechnologies)
                {
                    if (App.FastFlags.GetValue(mode.Value) == "True")
                        return mode.Key;
                }

                return LightingTechnologies.First().Key;
            }

            set
            {
                foreach (var mode in LightingTechnologies)
                {
                    if (mode.Key != LightingTechnologies.First().Key)
                        App.FastFlags.SetValue(mode.Value, null);
                }

                if (value != LightingTechnologies.First().Key)
                    App.FastFlags.SetValue(LightingTechnologies[value], "True");
            }
        }

        public bool GuiHidingEnabled
        {
            get => App.FastFlags.GetValue("DFIntCanHideGuiGroupId") == "32380007";
            set => App.FastFlags.SetValue("DFIntCanHideGuiGroupId", value ? "32380007" : null);
        }
    }
}
