using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Bloxstrap.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.ViewModels
{
    public class ModsViewModel
    {
        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

        private void OpenModsFolder()
        {
            Process.Start("explorer.exe", Directories.Modifications);
        }

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

        // only one missing here is Metal because lol
        public IReadOnlyDictionary<string, string> RenderingModes { get; set; } = new Dictionary<string, string>()
        {
            { "Automatic", "" },
            { "Direct3D 11", "FFlagDebugGraphicsPreferD3D11" },
            { "OpenGL", "FFlagDebugGraphicsPreferOpenGL" },
            { "Vulkan", "FFlagDebugGraphicsPreferVulkan" }
        };

        // if i ever need to do more fflag handling i'll just add an fflag handler that abstracts away all this boilerplate
        // but for now this is fine
        public bool ExclusiveFullscreenEnabled
        {
            get
            {
                return App.FastFlags.Prop.ContainsKey("FFlagHandleAltEnterFullscreenManually") && App.FastFlags.Prop["FFlagHandleAltEnterFullscreenManually"].ToString() == "False";
            }

            set
            {
                if (!App.IsFirstRun)
                    App.FastFlags.Load();

                if (value)
                    App.FastFlags.Prop["FFlagHandleAltEnterFullscreenManually"] = false;
                else
                    App.FastFlags.Prop.Remove("FFlagHandleAltEnterFullscreenManually");

                if (!App.IsFirstRun)
                    App.FastFlags.Save(true);
            }
        }

        public string SelectedRenderingMode
        { 
            get 
            {
                foreach (var mode in RenderingModes)
                {
                    if (App.FastFlags.Prop.ContainsKey(mode.Value))
                        return mode.Key;
                }

                return "Automatic";
            }

            set 
            {
                if (!App.IsFirstRun)
                    App.FastFlags.Load();

                foreach (var mode in RenderingModes)
                {
                    App.FastFlags.Prop.Remove(mode.Value);
                }

                if (value != "Automatic")
                    App.FastFlags.Prop[RenderingModes[value]] = true;

                if (!App.IsFirstRun)
                    App.FastFlags.Save(true);
            }
        }

        public bool DisableFullscreenOptimizationsEnabled
        {
            get => App.Settings.Prop.DisableFullscreenOptimizations;
            set => App.Settings.Prop.DisableFullscreenOptimizations = value;
        }
    }
}
