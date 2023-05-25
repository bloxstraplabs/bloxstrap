using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Singletons;
using System.ComponentModel;

namespace Bloxstrap.UI.Menu.ViewModels
{
    public class FastFlagsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICommand OpenClientSettingsCommand => new RelayCommand(OpenClientSettings);

        private void OpenClientSettings() => Utilities.OpenWebsite(Path.Combine(Directories.Modifications, "ClientSettings\\ClientAppSettings.json"));

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

            set => App.FastFlags.SetRenderingMode(value);
        }

        // this flag has to be set to false to work, weirdly enough
        public bool ExclusiveFullscreenEnabled
        {
            get => App.FastFlags.GetValue("FFlagHandleAltEnterFullscreenManually") == "False";
            set
            {
                App.FastFlags.SetValue("FFlagHandleAltEnterFullscreenManually", value ? "False" : null);

                if (value)
                {
                    if (!(App.FastFlags.GetValue("FFlagDebugGraphicsPreferD3D11") == "True" || App.FastFlags.GetValue("FFlagDebugGraphicsPreferD3D11FL10") == "True"))
                    {
                        App.FastFlags.SetRenderingMode("Direct3D 11");
                    }

                    OnPropertyChanged(nameof(SelectedRenderingMode));
                }
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

                return "Automatic";
            }

            set
            {
                foreach (var mode in LightingTechnologies)
                {
                    if (mode.Key != "Automatic")
                        App.FastFlags.SetValue(mode.Value, null);
                }

                if (value != "Automatic")
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
