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

        public static bool UseFastFlagManager
        {
            get => App.Settings.Prop.UseFastFlagManager;
            set => App.Settings.Prop.UseFastFlagManager = value;
        }

        // FFlags
        // Network
        public static bool DisableAds
        {
            get => App.FastFlags.GetPreset("Network.DisableAds") == "True";
            set => App.FastFlags.SetPreset("Network.DisableAds", value ? "True" : null);
        }

        public static bool DisableTelemetry
        {
            get => App.FastFlags.GetPreset("Network.DisableTelemetry1") == "True";
            set {
                App.FastFlags.SetPreset("Network.DisableTelemetry1", value ? "True" : null);
                App.FastFlags.SetPreset("Network.DisableTelemetry2", value ? "True" : null);
                App.FastFlags.SetPreset("Network.DisableTelemetry3", value ? "True" : null);
                App.FastFlags.SetPreset("Network.DisableTelemetry4", value ? "True" : null);
                App.FastFlags.SetPreset("Network.DisableTelemetry5", value ? "True" : null);
                App.FastFlags.SetPreset("Network.DisableTelemetry6", value ? "True" : null);
                App.FastFlags.SetPreset("Network.DisableTelemetry7", value ? "True" : null);
                App.FastFlags.SetPreset("Network.DisableTelemetry8", value ? "null" : null);
            }
        }

        public static bool NoUnrequiredConnections
        {
            get => App.FastFlags.GetPreset("Network.NoUnrequiredConnections") == "True";
            set => App.FastFlags.SetPreset("Network.NoUnrequiredConnections", value ? "True" : null);
        }

        public static bool ImproveServersideCharPos
        {
            get => App.FastFlags.GetPreset("Network.ImproveServersideCharPos") == "100";
            set => App.FastFlags.SetPreset("Network.ImproveServersideCharPos", value ? "100" : null);
        }



        // Rendering
        public static bool BetterPreload
        {
            get => App.FastFlags.GetPreset("Rendering.BetterPreload1") == "2147483647";
            set {
                App.FastFlags.SetPreset("Rendering.BetterPreload1", value ? "2147483647" : null);
                App.FastFlags.SetPreset("Rendering.BetterPreload2", value ? "2147483647" : null);
            }
        }

        public static bool DisableDynamicHeadAnimations
        {
            get => App.FastFlags.GetPreset("Rendering.DisableDynamicHeadAnimations1") == "0";
            set {
                App.FastFlags.SetPreset("Rendering.DisableDynamicHeadAnimations1", value ? "0" : null);
                App.FastFlags.SetPreset("Rendering.DisableDynamicHeadAnimations2", value ? "0" : null);
                App.FastFlags.SetPreset("Rendering.DisableDynamicHeadAnimations3", value ? "0" : null);
            }
        }

        public static bool DisablePostFX
        {
            get => App.FastFlags.GetPreset("Rendering.DisablePostFX") == "True";
            set => App.FastFlags.SetPreset("Rendering.DisablePostFX", value ? "True" : null);
        }

        public static bool FixDisplayScaling
        {
            get => App.FastFlags.GetPreset("Rendering.FixDisplayScaling") == "True";
            set => App.FastFlags.SetPreset("Rendering.FixDisplayScaling", value ? "True" : null);
        }

        public static bool ForceLowQuality
        {
            get => App.FastFlags.GetPreset("Rendering.ForceLowQuality") == "2";
            set => App.FastFlags.SetPreset("Rendering.ForceLowQuality", value ? "2" : null);
        }

        public int FramerateLimit
        {
            get => int.TryParse(App.FastFlags.GetPreset("Rendering.Framerate1"), out int x) ? x : 0;
            set {
                App.FastFlags.SetPreset("Rendering.Framerate1", value == 0 ? null : value);
                App.FastFlags.SetPreset("Rendering.Framerate2", value == 0 ? null : "False");
            }
        }

        public static bool UseHyperThreading
        {
            get => App.FastFlags.GetPreset("Rendering.HyperThreading1") == "True";
            set {
                App.FastFlags.SetPreset("Rendering.HyperThreading1", value ? "True" : null);
                App.FastFlags.SetPreset("Rendering.HyperThreading2", value ? "True" : null);
            }
        }

        public static bool ImproveRendering
        {
            get => App.FastFlags.GetPreset("Rendering.ImproveRendering") == "True";
            set => App.FastFlags.SetPreset("Rendering.ImproveRendering", value ? "True" : null);
        }

        // Rendering.Lighting
        public static bool BetterShadows
        {
            get => App.FastFlags.GetPreset("Rendering.Lighting.BetterShadows") == "True";
            set => App.FastFlags.SetPreset("Rendering.Lighting.BetterShadows", value ? "True" : null);
        }

        public static bool DisablePlayerShadows
        {
            get => App.FastFlags.GetPreset("Rendering.DisablePlayerShadows") == "0";
            set => App.FastFlags.SetPreset("Rendering.DisablePlayerShadows", value ? "0" : null);
        }

        public static IReadOnlyDictionary<LightingMode, string> LightingModes => FastFlagManager.LightingModes;
        public static LightingMode SelectedLightingMode
        {
            get => App.FastFlags.GetPresetEnum(LightingModes, "Rendering.Lighting", "True");
            set => App.FastFlags.SetPresetEnum("Rendering.Lighting", LightingModes[value], "True");
        }

        public static bool GPULightCulling
        {
            get => App.FastFlags.GetPreset("Rendering.Lighting.UseGPU") == "True";
            set => App.FastFlags.SetPreset("Rendering.Lighting.UseGPU", value ? "True" : null);
        }
        // Rendering.Lighting

        // Rendering.Mode
        public static IReadOnlyDictionary<RenderingMode, string> RenderingModes => FastFlagManager.RenderingModes;
        public static RenderingMode SelectedRenderingMode
        {
            get => App.FastFlags.GetPresetEnum(RenderingModes, "Rendering.Mode", "True");
            set => App.FastFlags.SetPresetEnum("Rendering.Mode", RenderingModes[value], "True");
        }
        // Rendering.Mode

        public static bool MovePrerender
        {
            get => App.FastFlags.GetPreset("Rendering.MovePrerender") == "True";
            set => App.FastFlags.SetPreset("Rendering.MovePrerender", value ? "True" : null);
        }

        public static IReadOnlyDictionary<MSAAMode, string?> MSAALevels => FastFlagManager.MSAAModes;
        public static MSAAMode SelectedMSAALevel
        {
            get => MSAALevels.FirstOrDefault(x => x.Value == App.FastFlags.GetPreset("Rendering.MSAA")).Key;
            set => App.FastFlags.SetPreset("Rendering.MSAA", MSAALevels[value]);
        }
        
        public static bool OcclusionCulling
        {
            get => App.FastFlags.GetPreset("Rendering.OcclusionCulling") == "True";
            set => App.FastFlags.SetPreset("Rendering.OcclusionCulling", value ? "True" : null);
        }

        // Rendering.Terrain
        public static bool DisableTerrainTextures
        {
            get => App.FastFlags.GetPreset("Rendering.Terrain.NoTextures") == "0";
            set => App.FastFlags.SetPreset("Rendering.Terrain.NoTextures", value ? "0" : null);
        }

        public static bool SmoothTerrain
        {
            get => App.FastFlags.GetPreset("Rendering.Terrain.Smooth") == "True";
            set => App.FastFlags.SetPreset("Rendering.Terrain.Smooth", value ? "True" : null);
        }
        // Rendering.Terrain

        // Rendering.TextureQuality
        public static IReadOnlyDictionary<TextureQuality, string?> TextureQualities => FastFlagManager.TextureQualityLevels;
        public static TextureQuality SelectedTextureQuality
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
        // Rendering.TextureQuality



        // UI
        public static bool GuiHidingEnabled
        {
            get => App.FastFlags.GetPreset("UI.Hide") == "32380007";
            set => App.FastFlags.SetPreset("UI.Hide", value ? "32380007" : null);
        }

        public static int? FontSize
        {
            get => int.TryParse(App.FastFlags.GetPreset("UI.FontSize"), out int x) ? x : 1;
            set => App.FastFlags.SetPreset("UI.FontSize", value == 1 ? null : value);
        }

        public static bool PreloadFonts
        {
            get => App.FastFlags.GetPreset("UI.PreloadFonts") == "True";
            set => App.FastFlags.SetPreset("UI.PreloadFonts", value ? "True" : null);
        }

        public static bool FullscreenTitlebarDisabled
        {
            get => int.TryParse(App.FastFlags.GetPreset("UI.RemoveFullscreenTitleBar"), out int x) && x > 5000;
            set => App.FastFlags.SetPreset("UI.RemoveFullscreenTitleBar", value ? "3600000" : null);
        }

        public static bool DisableVCBetaBadge
        {
            get => App.FastFlags.GetPreset("UI.DisableVCBetaBadge1") == "False";
            set {
                App.FastFlags.SetPreset("UI.DisableVCBetaBadge1", value ? "False" : null);
                App.FastFlags.SetPreset("UI.DisableVCBetaBadge2", value ? "False" : null);
                App.FastFlags.SetPreset("UI.DisableVCBetaBadge3", value ? "False" : null);
                App.FastFlags.SetPreset("UI.DisableVCBetaBadge4", value ? "False" : null);
                App.FastFlags.SetPreset("UI.DisableVCBetaBadge5", value ? "False" : null);
            }
        }



        // Misc
        public static bool BetterHaptics
        {
            get => App.FastFlags.GetPreset("Misc.BetterHaptics") == "True";
            set => App.FastFlags.SetPreset("Misc.BetterHaptics", value ? "True" : null);
        }

        public static bool BetterTrackpadScroll
        {
            get => App.FastFlags.GetPreset("Misc.BetterTrackpadScroll") == "True";
            set => App.FastFlags.SetPreset("Misc.BetterTrackpadScroll", value ? "True" : null);
        }

        public static bool DisableVC
        {
            get => App.FastFlags.GetPreset("Misc.DisableVC") == "False";
            set => App.FastFlags.SetPreset("Misc.DisableVC", value ? "False" : null);
        }

        public static bool ImproveRaycast
        {
            get => App.FastFlags.GetPreset("Misc.ImproveRaycast") == "True";
            set => App.FastFlags.SetPreset("Misc.ImproveRaycast", value ? "True" : null);
        }

        public static bool NoAFKKick
        {
            get => App.FastFlags.GetPreset("Misc.NoAFKKick") == "True";
            set => App.FastFlags.SetPreset("Misc.NoAFKKick", value ? "True" : null);
        }
        // FFlags



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
