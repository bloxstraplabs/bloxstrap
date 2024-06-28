using Bloxstrap.Enums.FlagPresets;
using System.Windows.Forms;

using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            { "Network.Log", "FLogNetwork" },

#if DEBUG
            { "HTTP.Log", "DFLogHttpTraceLight" },

            { "HTTP.Proxy.Enable", "DFFlagDebugEnableHttpProxy" },
            { "HTTP.Proxy.Address.1", "DFStringDebugPlayerHttpProxyUrl" },
            { "HTTP.Proxy.Address.2", "DFStringHttpCurlProxyHostAndPort" },
            { "HTTP.Proxy.Address.3", "DFStringHttpCurlProxyHostAndPortForExternalUrl" },
#endif

            { "Rendering.Framerate", "DFIntTaskSchedulerTargetFps" },
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },
            { "Rendering.DisablePostFX", "FFlagDisablePostFx" },
            { "Rendering.ShadowIntensity", "FIntRenderShadowIntensity" },

            { "Rendering.Mode.D3D11", "FFlagDebugGraphicsPreferD3D11" },
            { "Rendering.Mode.D3D10", "FFlagDebugGraphicsPreferD3D11FL10" },
            { "Rendering.Mode.Vulkan", "FFlagDebugGraphicsPreferVulkan" },
            { "Rendering.Mode.Vulkan.Fix", "FFlagRenderVulkanFixMinimizeWindow" },
            { "Rendering.Mode.OpenGL", "FFlagDebugGraphicsPreferOpenGL" },

            { "Rendering.Lighting.Voxel", "DFFlagDebugRenderForceTechnologyVoxel" },
            { "Rendering.Lighting.ShadowMap", "FFlagDebugForceFutureIsBrightPhase2" },
            { "Rendering.Lighting.Future", "FFlagDebugForceFutureIsBrightPhase3" },

            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },
            { "Rendering.TerrainTextureQuality", "FIntTerrainArraySliceSize" },

            { "UI.Hide", "DFIntCanHideGuiGroupId" },
            { "UI.FontSize", "FIntFontSizePadding" },
#if DEBUG
            { "UI.FlagState", "FStringDebugShowFlagState" },
#endif

            { "UI.Menu.GraphicsSlider", "FFlagFixGraphicsQuality" },
            { "UI.FullscreenTitlebarDelay", "FIntFullscreenTitleBarTriggerDelayMillis" },
            
            { "UI.Menu.Style.DisableV2", "FFlagDisableNewIGMinDUA" },
            { "UI.Menu.Style.EnableV4.1", "FFlagEnableInGameMenuControls" },
            { "UI.Menu.Style.EnableV4.2", "FFlagEnableInGameMenuModernization" },
            { "UI.Menu.Style.EnableV4Chrome", "FFlagEnableInGameMenuChrome" },

            { "UI.Menu.Style.ABTest.1", "FFlagEnableMenuControlsABTest" },
            { "UI.Menu.Style.ABTest.2", "FFlagEnableV3MenuABTest3" },
            { "UI.Menu.Style.ABTest.3", "FFlagEnableInGameMenuChromeABTest3" }
        };

        // only one missing here is Metal because lol
        public static IReadOnlyDictionary<RenderingMode, string> RenderingModes => new Dictionary<RenderingMode, string>
        {
            { RenderingMode.Default, "None" },
            // { RenderingMode.Vulkan, "Vulkan" },
            { RenderingMode.D3D11, "D3D11" },
            { RenderingMode.D3D10, "D3D10" },
            // { RenderingMode.OpenGL, "OpenGL" }
        };

        public static IReadOnlyDictionary<LightingMode, string> LightingModes => new Dictionary<LightingMode, string>
        {
            { LightingMode.Default, "None" },
            { LightingMode.Voxel, "Voxel" },
            { LightingMode.ShadowMap, "ShadowMap" },
            { LightingMode.Future, "Future" }
        };

        public static IReadOnlyDictionary<MSAAMode, string?> MSAAModes => new Dictionary<MSAAMode, string?>
        {
            { MSAAMode.Default, null },
            { MSAAMode.x1, "1" },
            { MSAAMode.x2, "2" },
            { MSAAMode.x4, "4" }
        };

        public static IReadOnlyDictionary<TextureQuality, string?> TextureQualityLevels => new Dictionary<TextureQuality, string?>
        {
            { TextureQuality.Default, null },
            { TextureQuality.Level0, "0" },
            { TextureQuality.Level1, "1" },
            { TextureQuality.Level2, "2" },
            { TextureQuality.Level3, "3" },
        };

        // this is one hell of a dictionary definition lmao
        // since these all set the same flags, wouldn't making this use bitwise operators be better?
        public static IReadOnlyDictionary<InGameMenuVersion, Dictionary<string, string?>> IGMenuVersions => new Dictionary<InGameMenuVersion, Dictionary<string, string?>>
        {
            {
                InGameMenuVersion.Default,
                new Dictionary<string, string?>
                {
                    { "DisableV2", null },
                    { "EnableV4", null },
                    { "EnableV4Chrome", null },
                    { "ABTest", null }
                }
            },

            {
                InGameMenuVersion.V1,
                new Dictionary<string, string?>
                {
                    { "DisableV2", "True" },
                    { "EnableV4", "False" },
                    { "EnableV4Chrome", "False" },
                    { "ABTest", "False" }
                }
            },

            {
                InGameMenuVersion.V2,
                new Dictionary<string, string?>
                {
                    { "DisableV2", "False" },
                    { "EnableV4", "False" },
                    { "EnableV4Chrome", "False" },
                    { "ABTest", "False" }
                }
            },

            {
                InGameMenuVersion.V4,
                new Dictionary<string, string?>
                {
                    { "DisableV2", "True" },
                    { "EnableV4", "True" },
                    { "EnableV4Chrome", "False" },
                    { "ABTest", "False" }
                }
            },

            {
                InGameMenuVersion.V4Chrome,
                new Dictionary<string, string?>
                {
                    { "DisableV2", "True" },
                    { "EnableV4", "True" },
                    { "EnableV4Chrome", "True" },
                    { "ABTest", "False" }
                }
            }
        };

        // all fflags are stored as strings
        // to delete a flag, set the value as null
        public void SetValue(string key, object? value)
        {
            const string LOG_IDENT = "FastFlagManager::SetValue";

            if (value is null)
            {
                if (Prop.ContainsKey(key))
                    App.Logger.WriteLine(LOG_IDENT, $"Deletion of '{key}' is pending");

                Prop.Remove(key);
            }
            else
            {
                if (Prop.ContainsKey(key))
                {
                    if (key == Prop[key].ToString())
                        return;

                    App.Logger.WriteLine(LOG_IDENT, $"Changing of '{key}' from '{Prop[key]}' to '{value}' is pending");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{value}' is pending");
                }

                Prop[key] = value.ToString()!;
            }
        }

        // this returns null if the fflag doesn't exist
        public string? GetValue(string key)
        {
            // check if we have an updated change for it pushed first
            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();

            return null;
        }

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public void SetPresetEnum(string prefix, string target, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
            {
                if (pair.Key.StartsWith($"{prefix}.{target}"))
                    SetValue(pair.Value, value);
                else
                    SetValue(pair.Value, null);
            }
        }

        public string? GetPreset(string name) => GetValue(PresetFlags[name]);

        public T GetPresetEnum<T>(IReadOnlyDictionary<T, string> mapping, string prefix, string value) where T : Enum
        {
            foreach (var pair in mapping)
            {
                if (pair.Value == "None")
                    continue;

                if (GetPreset($"{prefix}.{pair.Value}") == value)
                    return pair.Key;
            }

            return mapping.First().Key;
        }

        public void CheckManualFullscreenPreset()
        {
            if (GetPreset("Rendering.Mode.Vulkan") == "True" || GetPreset("Rendering.Mode.OpenGL") == "True")
                SetPreset("Rendering.ManualFullscreen", null);
            else
                SetPreset("Rendering.ManualFullscreen", "False");
        }

        public override void Save()
        {
            // convert all flag values to strings before saving

            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;

            base.Save();
        }

        public override void Load()
        {
            base.Load();

            CheckManualFullscreenPreset();

            // TODO - remove when activity tracking has been revamped
            if (GetPreset("Network.Log") != "7")
                SetPreset("Network.Log", "7");

            string? val = GetPreset("UI.Menu.Style.EnableV4.1");
            if (GetPreset("UI.Menu.Style.EnableV4.2") != val)
                SetPreset("UI.Menu.Style.EnableV4.2", val);
        }
    }
}
