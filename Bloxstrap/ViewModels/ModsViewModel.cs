using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Singletons;

namespace Bloxstrap.ViewModels
{
    public class ModsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

        private void OpenModsFolder() => Process.Start("explorer.exe", Directories.Modifications);

        public bool OldDeathSoundEnabled
        {
            get => App.Settings.Prop.UseOldDeathSound;
            set => App.Settings.Prop.UseOldDeathSound = value;
        }

        public bool OldMouseCursorEnabled
        {
            get => App.Settings.Prop.UseOldMouseCursor;
            set => App.Settings.Prop.UseOldMouseCursor = value;
        }

        public bool DisableAppPatchEnabled
        {
            get => App.Settings.Prop.UseDisableAppPatch;
            set => App.Settings.Prop.UseDisableAppPatch = value;
        }

        public int FramerateLimit
        {
            get => Int32.TryParse(App.FastFlags.GetValue("DFIntTaskSchedulerTargetFps"), out int x) ? x : 60;
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
                    App.FastFlags.SetRenderingMode("Direct3D 11");
                    OnPropertyChanged(nameof(SelectedRenderingMode));
                }
            }
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

        public bool AlternateGraphicsSelectorEnabled
        {
            get => App.FastFlags.GetValue("FFlagFixGraphicsQuality") == "True";
            set => App.FastFlags.SetValue("FFlagFixGraphicsQuality", value ? "True" : null);
        }

        public bool MobileLuaAppInterfaceEnabled
        {
            get => App.FastFlags.GetValue("FFlagLuaAppSystemBar") == "False";
            set => App.FastFlags.SetValue("FFlagLuaAppSystemBar", value ? "False" : null);
        }

        public bool DisableFullscreenOptimizationsEnabled
        {
            get => App.Settings.Prop.DisableFullscreenOptimizations;
            set => App.Settings.Prop.DisableFullscreenOptimizations = value;
        }
        
        public bool ForceFutureEnabled
        {
            get => App.FastFlags.GetValue("FFlagDebugForceFutureIsBrightPhase3") == "True";
            set => App.FastFlags.SetValue("FFlagDebugForceFutureIsBrightPhase3", value ? "True" : null);
        }
    }
}
