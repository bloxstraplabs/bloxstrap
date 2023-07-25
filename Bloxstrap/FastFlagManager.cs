using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

        // this is the value of the 'FStringPartTexturePackTablePre2022' flag
        public const string OldTexturesFlagValue = "{\"foil\":{\"ids\":[\"rbxassetid://7546645012\",\"rbxassetid://7546645118\"],\"color\":[255,255,255,255]},\"brick\":{\"ids\":[\"rbxassetid://7546650097\",\"rbxassetid://7546645118\"],\"color\":[204,201,200,232]},\"cobblestone\":{\"ids\":[\"rbxassetid://7546652947\",\"rbxassetid://7546645118\"],\"color\":[212,200,187,250]},\"concrete\":{\"ids\":[\"rbxassetid://7546653951\",\"rbxassetid://7546654144\"],\"color\":[208,208,208,255]},\"diamondplate\":{\"ids\":[\"rbxassetid://7547162198\",\"rbxassetid://7546645118\"],\"color\":[170,170,170,255]},\"fabric\":{\"ids\":[\"rbxassetid://7547101130\",\"rbxassetid://7546645118\"],\"color\":[105,104,102,244]},\"glass\":{\"ids\":[\"rbxassetid://7547304948\",\"rbxassetid://7546645118\"],\"color\":[254,254,254,7]},\"granite\":{\"ids\":[\"rbxassetid://7547164710\",\"rbxassetid://7546645118\"],\"color\":[113,113,113,255]},\"grass\":{\"ids\":[\"rbxassetid://7547169285\",\"rbxassetid://7546645118\"],\"color\":[165,165,159,255]},\"ice\":{\"ids\":[\"rbxassetid://7547171356\",\"rbxassetid://7546645118\"],\"color\":[255,255,255,255]},\"marble\":{\"ids\":[\"rbxassetid://7547177270\",\"rbxassetid://7546645118\"],\"color\":[199,199,199,255]},\"metal\":{\"ids\":[\"rbxassetid://7547288171\",\"rbxassetid://7546645118\"],\"color\":[199,199,199,255]},\"pebble\":{\"ids\":[\"rbxassetid://7547291361\",\"rbxassetid://7546645118\"],\"color\":[208,208,208,255]},\"corrodedmetal\":{\"ids\":[\"rbxassetid://7547184629\",\"rbxassetid://7546645118\"],\"color\":[159,119,95,200]},\"sand\":{\"ids\":[\"rbxassetid://7547295153\",\"rbxassetid://7546645118\"],\"color\":[220,220,220,255]},\"slate\":{\"ids\":[\"rbxassetid://7547298114\",\"rbxassetid://7547298323\"],\"color\":[193,193,193,255]},\"wood\":{\"ids\":[\"rbxassetid://7547303225\",\"rbxassetid://7547298786\"],\"color\":[227,227,227,255]},\"woodplanks\":{\"ids\":[\"rbxassetid://7547332968\",\"rbxassetid://7546645118\"],\"color\":[212,209,203,255]},\"asphalt\":{\"ids\":[\"rbxassetid://9873267379\",\"rbxassetid://9438410548\"],\"color\":[123,123,123,234]},\"basalt\":{\"ids\":[\"rbxassetid://9873270487\",\"rbxassetid://9438413638\"],\"color\":[154,154,153,238]},\"crackedlava\":{\"ids\":[\"rbxassetid://9438582231\",\"rbxassetid://9438453972\"],\"color\":[74,78,80,156]},\"glacier\":{\"ids\":[\"rbxassetid://9438851661\",\"rbxassetid://9438453972\"],\"color\":[226,229,229,243]},\"ground\":{\"ids\":[\"rbxassetid://9439044431\",\"rbxassetid://9438453972\"],\"color\":[114,114,112,240]},\"leafygrass\":{\"ids\":[\"rbxassetid://9873288083\",\"rbxassetid://9438453972\"],\"color\":[121,117,113,234]},\"limestone\":{\"ids\":[\"rbxassetid://9873289812\",\"rbxassetid://9438453972\"],\"color\":[235,234,230,250]},\"mud\":{\"ids\":[\"rbxassetid://9873319819\",\"rbxassetid://9438453972\"],\"color\":[130,130,130,252]},\"pavement\":{\"ids\":[\"rbxassetid://9873322398\",\"rbxassetid://9438453972\"],\"color\":[142,142,144,236]},\"rock\":{\"ids\":[\"rbxassetid://9873515198\",\"rbxassetid://9438453972\"],\"color\":[154,154,154,248]},\"salt\":{\"ids\":[\"rbxassetid://9439566986\",\"rbxassetid://9438453972\"],\"color\":[220,220,221,255]},\"sandstone\":{\"ids\":[\"rbxassetid://9873521380\",\"rbxassetid://9438453972\"],\"color\":[174,171,169,246]},\"snow\":{\"ids\":[\"rbxassetid://9439632387\",\"rbxassetid://9438453972\"],\"color\":[218,218,218,255]}}";

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            { "HTTP.Log", "DFLogHttpTraceLight" },

            { "HTTP.Proxy.Enable", "DFFlagDebugEnableHttpProxy" },
            { "HTTP.Proxy.Address.1", "DFStringDebugPlayerHttpProxyUrl" },
            { "HTTP.Proxy.Address.2", "DFStringHttpCurlProxyHostAndPort" },
            { "HTTP.Proxy.Address.3", "DFStringHttpCurlProxyHostAndPortForExternalUrl" },

            { "Rendering.Framerate", "DFIntTaskSchedulerTargetFps" },
            { "Rendering.Fullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.TexturePack", "FStringPartTexturePackTable2022" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },

            { "Rendering.Mode.D3D11", "FFlagDebugGraphicsPreferD3D11" },
            { "Rendering.Mode.D3D10", "FFlagDebugGraphicsPreferD3D11FL10" },
            { "Rendering.Mode.Vulkan", "FFlagDebugGraphicsPreferVulkan" },
            { "Rendering.Mode.Vulkan.Fix", "FFlagRenderVulkanFixMinimizeWindow" },
            { "Rendering.Mode.OpenGL", "FFlagDebugGraphicsPreferOpenGL" },

            { "Rendering.Lighting.Voxel", "DFFlagDebugRenderForceTechnologyVoxel" },
            { "Rendering.Lighting.ShadowMap", "FFlagDebugForceFutureIsBrightPhase2" },
            { "Rendering.Lighting.Future", "FFlagDebugForceFutureIsBrightPhase3" },

            { "UI.Hide", "DFIntCanHideGuiGroupId" },
            { "UI.FlagState", "FStringDebugShowFlagState" },

            { "UI.Menu.GraphicsSlider", "FFlagFixGraphicsQuality" },
            
            { "UI.Menu.Style.DisableV2", "FFlagDisableNewIGMinDUA" },
            { "UI.Menu.Style.EnableV4", "FFlagEnableInGameMenuControls" }
        };

        // only one missing here is Metal because lol
        public static IReadOnlyDictionary<string, string> RenderingModes => new Dictionary<string, string>
        {
            { "Automatic", "None" },
            { "Vulkan", "Vulkan" },
            { "Direct3D 11", "D3D11" },
            { "Direct3D 10", "D3D10" },
            { "OpenGL", "OpenGL" }
        };

        public static IReadOnlyDictionary<string, string> LightingModes => new Dictionary<string, string>
        {
            { "Chosen by game", "None" },
            { "Voxel (Phase 1)", "Voxel" },
            { "ShadowMap (Phase 2)", "ShadowMap" },
            { "Future (Phase 3)", "Future" }
        };

        // this is one hell of a dictionary definition lmao
        // since these all set the same flags, wouldn't making this use bitwise operators be better?
        public static IReadOnlyDictionary<string, Dictionary<string, string?>> IGMenuVersions => new Dictionary<string, Dictionary<string, string?>>
        {
            {
                "Default",
                new Dictionary<string, string?>
                {
                    { "DisableV2", null },
                    { "EnableV4", null }
                }
            },

            {
                "Version 1 (2015)",
                new Dictionary<string, string?>
                {
                    { "DisableV2", "True" },
                    { "EnableV4", "False" }
                }
            },

            {
                "Version 2 (2020)",
                new Dictionary<string, string?>
                {
                    { "DisableV2", "False" },
                    { "EnableV4", "False" }
                }
            },

            {
                "Version 4 (2023)",
                new Dictionary<string, string?>
                {
                    { "DisableV2", "True" },
                    { "EnableV4", "True" }
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
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' from '{Prop[key]}' to '{value}' is pending");
                else
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{value}' is pending");

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

        public void SetPresetOnce(string key, object? value)
        {
            if (GetPreset(key) is null)
                SetPreset(key, value);
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

        public string GetPresetEnum(IReadOnlyDictionary<string, string> mapping, string prefix, string value)
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

            SetPresetOnce("Rendering.Framerate", 9999);
            SetPresetOnce("Rendering.Fullscreen", "False");
        }
    }
}
