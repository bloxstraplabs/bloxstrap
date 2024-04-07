using Bloxstrap.Enums.FlagPresets;
using System.Windows.Forms;

using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

        // this is the value of the 'FStringPartTexturePackTablePre2022' flag
        public const string OldTexturesFlagValue = "{\"foil\":{\"ids\":[\"rbxassetid://7546645012\",\"rbxassetid://7546645118\"],\"color\":[255,255,255,255]},\"brick\":{\"ids\":[\"rbxassetid://7546650097\",\"rbxassetid://7546645118\"],\"color\":[204,201,200,232]},\"cobblestone\":{\"ids\":[\"rbxassetid://7546652947\",\"rbxassetid://7546645118\"],\"color\":[212,200,187,250]},\"concrete\":{\"ids\":[\"rbxassetid://7546653951\",\"rbxassetid://7546654144\"],\"color\":[208,208,208,255]},\"diamondplate\":{\"ids\":[\"rbxassetid://7547162198\",\"rbxassetid://7546645118\"],\"color\":[170,170,170,255]},\"fabric\":{\"ids\":[\"rbxassetid://7547101130\",\"rbxassetid://7546645118\"],\"color\":[105,104,102,244]},\"glass\":{\"ids\":[\"rbxassetid://7547304948\",\"rbxassetid://7546645118\"],\"color\":[254,254,254,7]},\"granite\":{\"ids\":[\"rbxassetid://7547164710\",\"rbxassetid://7546645118\"],\"color\":[113,113,113,255]},\"grass\":{\"ids\":[\"rbxassetid://7547169285\",\"rbxassetid://7546645118\"],\"color\":[165,165,159,255]},\"ice\":{\"ids\":[\"rbxassetid://7547171356\",\"rbxassetid://7546645118\"],\"color\":[255,255,255,255]},\"marble\":{\"ids\":[\"rbxassetid://7547177270\",\"rbxassetid://7546645118\"],\"color\":[199,199,199,255]},\"metal\":{\"ids\":[\"rbxassetid://7547288171\",\"rbxassetid://7546645118\"],\"color\":[199,199,199,255]},\"pebble\":{\"ids\":[\"rbxassetid://7547291361\",\"rbxassetid://7546645118\"],\"color\":[208,208,208,255]},\"corrodedmetal\":{\"ids\":[\"rbxassetid://7547184629\",\"rbxassetid://7546645118\"],\"color\":[159,119,95,200]},\"sand\":{\"ids\":[\"rbxassetid://7547295153\",\"rbxassetid://7546645118\"],\"color\":[220,220,220,255]},\"slate\":{\"ids\":[\"rbxassetid://7547298114\",\"rbxassetid://7547298323\"],\"color\":[193,193,193,255]},\"wood\":{\"ids\":[\"rbxassetid://7547303225\",\"rbxassetid://7547298786\"],\"color\":[227,227,227,255]},\"woodplanks\":{\"ids\":[\"rbxassetid://7547332968\",\"rbxassetid://7546645118\"],\"color\":[212,209,203,255]},\"asphalt\":{\"ids\":[\"rbxassetid://9873267379\",\"rbxassetid://9438410548\"],\"color\":[123,123,123,234]},\"basalt\":{\"ids\":[\"rbxassetid://9873270487\",\"rbxassetid://9438413638\"],\"color\":[154,154,153,238]},\"crackedlava\":{\"ids\":[\"rbxassetid://9438582231\",\"rbxassetid://9438453972\"],\"color\":[74,78,80,156]},\"glacier\":{\"ids\":[\"rbxassetid://9438851661\",\"rbxassetid://9438453972\"],\"color\":[226,229,229,243]},\"ground\":{\"ids\":[\"rbxassetid://9439044431\",\"rbxassetid://9438453972\"],\"color\":[114,114,112,240]},\"leafygrass\":{\"ids\":[\"rbxassetid://9873288083\",\"rbxassetid://9438453972\"],\"color\":[121,117,113,234]},\"limestone\":{\"ids\":[\"rbxassetid://9873289812\",\"rbxassetid://9438453972\"],\"color\":[235,234,230,250]},\"mud\":{\"ids\":[\"rbxassetid://9873319819\",\"rbxassetid://9438453972\"],\"color\":[130,130,130,252]},\"pavement\":{\"ids\":[\"rbxassetid://9873322398\",\"rbxassetid://9438453972\"],\"color\":[142,142,144,236]},\"rock\":{\"ids\":[\"rbxassetid://9873515198\",\"rbxassetid://9438453972\"],\"color\":[154,154,154,248]},\"salt\":{\"ids\":[\"rbxassetid://9439566986\",\"rbxassetid://9438453972\"],\"color\":[220,220,221,255]},\"sandstone\":{\"ids\":[\"rbxassetid://9873521380\",\"rbxassetid://9438453972\"],\"color\":[174,171,169,246]},\"snow\":{\"ids\":[\"rbxassetid://9439632387\",\"rbxassetid://9438453972\"],\"color\":[218,218,218,255]}}";
        public const string NewTexturesFlagValue = "{\"foil\":{\"ids\":[\"rbxassetid://9873266399\",\"rbxassetid://9438410239\"],\"color\":[238,238,238,255]},\"asphalt\":{\"ids\":[\"rbxassetid://9930003180\",\"rbxassetid://9438410548\"],\"color\":[227,227,228,234]},\"basalt\":{\"ids\":[\"rbxassetid://9920482224\",\"rbxassetid://9438413638\"],\"color\":[160,160,158,238]},\"brick\":{\"ids\":[\"rbxassetid://9920482992\",\"rbxassetid://9438453972\"],\"color\":[229,214,205,227]},\"cobblestone\":{\"ids\":[\"rbxassetid://9919719550\",\"rbxassetid://9438453972\"],\"color\":[218,219,219,243]},\"concrete\":{\"ids\":[\"rbxassetid://9920484334\",\"rbxassetid://9438453972\"],\"color\":[225,225,224,255]},\"crackedlava\":{\"ids\":[\"rbxassetid://9920485426\",\"rbxassetid://9438453972\"],\"color\":[76,79,81,156]},\"diamondplate\":{\"ids\":[\"rbxassetid://10237721036\",\"rbxassetid://9438453972\"],\"color\":[210,210,210,255]},\"fabric\":{\"ids\":[\"rbxassetid://9920517963\",\"rbxassetid://9438453972\"],\"color\":[221,221,221,255]},\"glacier\":{\"ids\":[\"rbxassetid://9920518995\",\"rbxassetid://9438453972\"],\"color\":[225,229,229,243]},\"glass\":{\"ids\":[\"rbxassetid://9873284556\",\"rbxassetid://9438453972\"],\"color\":[254,254,254,7]},\"granite\":{\"ids\":[\"rbxassetid://9920550720\",\"rbxassetid://9438453972\"],\"color\":[210,206,200,255]},\"grass\":{\"ids\":[\"rbxassetid://9920552044\",\"rbxassetid://9438453972\"],\"color\":[196,196,189,241]},\"ground\":{\"ids\":[\"rbxassetid://9920554695\",\"rbxassetid://9438453972\"],\"color\":[165,165,160,240]},\"ice\":{\"ids\":[\"rbxassetid://9920556429\",\"rbxassetid://9438453972\"],\"color\":[235,239,241,248]},\"leafygrass\":{\"ids\":[\"rbxassetid://9920558145\",\"rbxassetid://9438453972\"],\"color\":[182,178,175,234]},\"limestone\":{\"ids\":[\"rbxassetid://9920561624\",\"rbxassetid://9438453972\"],\"color\":[250,248,243,250]},\"marble\":{\"ids\":[\"rbxassetid://9873292869\",\"rbxassetid://9438453972\"],\"color\":[181,183,193,249]},\"metal\":{\"ids\":[\"rbxassetid://9920574966\",\"rbxassetid://9438453972\"],\"color\":[226,226,226,255]},\"mud\":{\"ids\":[\"rbxassetid://9920578676\",\"rbxassetid://9438453972\"],\"color\":[193,192,193,252]},\"pavement\":{\"ids\":[\"rbxassetid://9920580094\",\"rbxassetid://9438453972\"],\"color\":[218,218,219,236]},\"pebble\":{\"ids\":[\"rbxassetid://9920581197\",\"rbxassetid://9438453972\"],\"color\":[204,203,201,234]},\"plastic\":{\"ids\":[\"\",\"rbxassetid://9475422736\"],\"color\":[255,255,255,255]},\"rock\":{\"ids\":[\"rbxassetid://10129366149\",\"rbxassetid://9438453972\"],\"color\":[211,211,210,248]},\"corrodedmetal\":{\"ids\":[\"rbxassetid://9920589512\",\"rbxassetid://9439557520\"],\"color\":[206,177,163,180]},\"salt\":{\"ids\":[\"rbxassetid://9920590478\",\"rbxassetid://9438453972\"],\"color\":[249,249,249,255]},\"sand\":{\"ids\":[\"rbxassetid://9920591862\",\"rbxassetid://9438453972\"],\"color\":[218,216,210,240]},\"sandstone\":{\"ids\":[\"rbxassetid://9920596353\",\"rbxassetid://9438453972\"],\"color\":[241,234,230,246]},\"slate\":{\"ids\":[\"rbxassetid://9920600052\",\"rbxassetid://9439613006\"],\"color\":[235,234,235,254]},\"snow\":{\"ids\":[\"rbxassetid://9920620451\",\"rbxassetid://9438453972\"],\"color\":[239,240,240,255]},\"wood\":{\"ids\":[\"rbxassetid://9920625499\",\"rbxassetid://9439649548\"],\"color\":[217,209,208,255]},\"woodplanks\":{\"ids\":[\"rbxassetid://9920626896\",\"rbxassetid://9438453972\"],\"color\":[207,208,206,254]}}";

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            { "Network.Log", "FLogNetwork" },
            
            { "HTTP.Log", "DFLogHttpTraceLight" },

            { "HTTP.Proxy.Enable", "DFFlagDebugEnableHttpProxy" },
            { "HTTP.Proxy.Address.1", "DFStringDebugPlayerHttpProxyUrl" },
            { "HTTP.Proxy.Address.2", "DFStringHttpCurlProxyHostAndPort" },
            { "HTTP.Proxy.Address.3", "DFStringHttpCurlProxyHostAndPortForExternalUrl" },

            { "Rendering.Framerate", "DFIntTaskSchedulerTargetFps" },
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },

            { "Rendering.Materials.NewTexturePack", "FStringPartTexturePackTable2022" },
            { "Rendering.Materials.OldTexturePack", "FStringPartTexturePackTablePre2022" },

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
            { "UI.Menu.Style.EnableV4.1", "FFlagEnableInGameMenuControls" },
            { "UI.Menu.Style.EnableV4.2", "FFlagEnableInGameMenuModernization" },

            { "UI.Menu.Style.ABTest.1", "FFlagEnableMenuControlsABTest" },
            { "UI.Menu.Style.ABTest.2", "FFlagEnableMenuModernizationABTest" },
            { "UI.Menu.Style.ABTest.3", "FFlagEnableMenuModernizationABTest2" },
            { "UI.Menu.Style.ABTest.4", "FFlagEnableV3MenuABTest3" }
        };

        // only one missing here is Metal because lol
        public static IReadOnlyDictionary<RenderingMode, string> RenderingModes => new Dictionary<RenderingMode, string>
        {
            { RenderingMode.Default, "None" },
            { RenderingMode.Vulkan, "Vulkan" },
            { RenderingMode.D3D11, "D3D11" },
            { RenderingMode.D3D10, "D3D10" },
            { RenderingMode.OpenGL, "OpenGL" }
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
            { MSAAMode.x4, "4" },
            { MSAAMode.x8, "8" }
        };

        public static IReadOnlyDictionary<MaterialVersion, string> MaterialVersions => new Dictionary<MaterialVersion, string>
        {
            { MaterialVersion.Default, "None" },
            { MaterialVersion.Old, "NewTexturePack" },
            { MaterialVersion.New, "OldTexturePack" }
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
                    { "ABTest", null }
                }
            },

            {
                InGameMenuVersion.V1,
                new Dictionary<string, string?>
                {
                    { "DisableV2", "True" },
                    { "EnableV4", "False" },
                    { "ABTest", "False" }
                }
            },

            {
                InGameMenuVersion.V2,
                new Dictionary<string, string?>
                {
                    { "DisableV2", "False" },
                    { "EnableV4", "False" },
                    { "ABTest", "False" }
                }
            },

            {
                InGameMenuVersion.V4,
                new Dictionary<string, string?>
                {
                    { "DisableV2", "True" },
                    { "EnableV4", "True" },
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

            if (GetPreset("Rendering.Framerate") is not null)
                return;

            // set it to be the framerate of the primary display by default

            var screen = Screen.AllScreens.Where(x => x.Primary).Single();
            var devmode = new DEVMODEW();

            PInvoke.EnumDisplaySettings(screen.DeviceName, ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref devmode);

            uint framerate = devmode.dmDisplayFrequency;

            if (framerate <= 100)
                framerate *= 2;

            SetPreset("Rendering.Framerate", framerate);
        }
    }
}
