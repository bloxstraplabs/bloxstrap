using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string FileLocation => Path.Combine(Directories.Modifications, "ClientSettings\\ClientAppSettings.json");

        // we put any changes we want to make to fastflags here
        // these will apply after bloxstrap finishes installing or after the menu closes
        // to delete a fastflag, set the value to null
        public Dictionary<string, object?> Changes = new();

        // this is the value of the 'FStringPartTexturePackTablePre2022' flag
        public const string OldTexturesFlagValue = "{\"foil\":{\"ids\":[\"rbxassetid://7546645012\",\"rbxassetid://7546645118\"],\"color\":[255,255,255,255]},\"brick\":{\"ids\":[\"rbxassetid://7546650097\",\"rbxassetid://7546645118\"],\"color\":[204,201,200,232]},\"cobblestone\":{\"ids\":[\"rbxassetid://7546652947\",\"rbxassetid://7546645118\"],\"color\":[212,200,187,250]},\"concrete\":{\"ids\":[\"rbxassetid://7546653951\",\"rbxassetid://7546654144\"],\"color\":[208,208,208,255]},\"diamondplate\":{\"ids\":[\"rbxassetid://7547162198\",\"rbxassetid://7546645118\"],\"color\":[170,170,170,255]},\"fabric\":{\"ids\":[\"rbxassetid://7547101130\",\"rbxassetid://7546645118\"],\"color\":[105,104,102,244]},\"glass\":{\"ids\":[\"rbxassetid://7547304948\",\"rbxassetid://7546645118\"],\"color\":[254,254,254,7]},\"granite\":{\"ids\":[\"rbxassetid://7547164710\",\"rbxassetid://7546645118\"],\"color\":[113,113,113,255]},\"grass\":{\"ids\":[\"rbxassetid://7547169285\",\"rbxassetid://7546645118\"],\"color\":[165,165,159,255]},\"ice\":{\"ids\":[\"rbxassetid://7547171356\",\"rbxassetid://7546645118\"],\"color\":[255,255,255,255]},\"marble\":{\"ids\":[\"rbxassetid://7547177270\",\"rbxassetid://7546645118\"],\"color\":[199,199,199,255]},\"metal\":{\"ids\":[\"rbxassetid://7547288171\",\"rbxassetid://7546645118\"],\"color\":[199,199,199,255]},\"pebble\":{\"ids\":[\"rbxassetid://7547291361\",\"rbxassetid://7546645118\"],\"color\":[208,208,208,255]},\"corrodedmetal\":{\"ids\":[\"rbxassetid://7547184629\",\"rbxassetid://7546645118\"],\"color\":[159,119,95,200]},\"sand\":{\"ids\":[\"rbxassetid://7547295153\",\"rbxassetid://7546645118\"],\"color\":[220,220,220,255]},\"slate\":{\"ids\":[\"rbxassetid://7547298114\",\"rbxassetid://7547298323\"],\"color\":[193,193,193,255]},\"wood\":{\"ids\":[\"rbxassetid://7547303225\",\"rbxassetid://7547298786\"],\"color\":[227,227,227,255]},\"woodplanks\":{\"ids\":[\"rbxassetid://7547332968\",\"rbxassetid://7546645118\"],\"color\":[212,209,203,255]},\"asphalt\":{\"ids\":[\"rbxassetid://9873267379\",\"rbxassetid://9438410548\"],\"color\":[123,123,123,234]},\"basalt\":{\"ids\":[\"rbxassetid://9873270487\",\"rbxassetid://9438413638\"],\"color\":[154,154,153,238]},\"crackedlava\":{\"ids\":[\"rbxassetid://9438582231\",\"rbxassetid://9438453972\"],\"color\":[74,78,80,156]},\"glacier\":{\"ids\":[\"rbxassetid://9438851661\",\"rbxassetid://9438453972\"],\"color\":[226,229,229,243]},\"ground\":{\"ids\":[\"rbxassetid://9439044431\",\"rbxassetid://9438453972\"],\"color\":[114,114,112,240]},\"leafygrass\":{\"ids\":[\"rbxassetid://9873288083\",\"rbxassetid://9438453972\"],\"color\":[121,117,113,234]},\"limestone\":{\"ids\":[\"rbxassetid://9873289812\",\"rbxassetid://9438453972\"],\"color\":[235,234,230,250]},\"mud\":{\"ids\":[\"rbxassetid://9873319819\",\"rbxassetid://9438453972\"],\"color\":[130,130,130,252]},\"pavement\":{\"ids\":[\"rbxassetid://9873322398\",\"rbxassetid://9438453972\"],\"color\":[142,142,144,236]},\"rock\":{\"ids\":[\"rbxassetid://9873515198\",\"rbxassetid://9438453972\"],\"color\":[154,154,154,248]},\"salt\":{\"ids\":[\"rbxassetid://9439566986\",\"rbxassetid://9438453972\"],\"color\":[220,220,221,255]},\"sandstone\":{\"ids\":[\"rbxassetid://9873521380\",\"rbxassetid://9438453972\"],\"color\":[174,171,169,246]},\"snow\":{\"ids\":[\"rbxassetid://9439632387\",\"rbxassetid://9438453972\"],\"color\":[218,218,218,255]}}";

        // only one missing here is Metal because lol
        public static IReadOnlyDictionary<string, string> RenderingModes => new Dictionary<string, string>
        {
            { "Automatic", "" },
            { "Vulkan", "FFlagDebugGraphicsPreferVulkan" },
            { "Direct3D 11", "FFlagDebugGraphicsPreferD3D11" },
            { "Direct3D 10", "FFlagDebugGraphicsPreferD3D11FL10" },
            { "OpenGL", "FFlagDebugGraphicsPreferOpenGL" }
        };

        public static IReadOnlyDictionary<string, string> LightingTechnologies => new Dictionary<string, string>
        {
            { "Chosen by game", "" },
            { "Voxel (Phase 1)", "DFFlagDebugRenderForceTechnologyVoxel" },
            { "ShadowMap (Phase 2)", "FFlagDebugForceFutureIsBrightPhase2" },
            { "Future (Phase 3)", "FFlagDebugForceFutureIsBrightPhase3" }
        };

        // this is one hell of a dictionary definition lmao
        // since these all set the same flags, wouldn't making this use bitwise operators be better?
        public static IReadOnlyDictionary<string, Dictionary<string, string?>> IGMenuVersions => new Dictionary<string, Dictionary<string, string?>>
        {
            {
                "Default",
                new Dictionary<string, string?>
                {
                    { "FFlagDisableNewIGMinDUA", null },
                    { "FFlagEnableInGameMenuControls", null },
                    { "FFlagEnableV3MenuABTest3", null },
                    { "FFlagEnableMenuControlsABTest", null }
                }
            },

            {
                "Version 1 (2015)",
                new Dictionary<string, string?>
                {
                    { "FFlagDisableNewIGMinDUA", "True" },
                    { "FFlagEnableInGameMenuControls", "False" },
                    { "FFlagEnableV3MenuABTest3", "False" },
                    { "FFlagEnableMenuControlsABTest", "False" }
                }
            },

            {
                "Version 2 (2020)",
                new Dictionary<string, string?>
                {
                    { "FFlagDisableNewIGMinDUA", "False" },
                    { "FFlagEnableInGameMenuControls", "False" },
                    { "FFlagEnableV3MenuABTest3", "False" },
                    { "FFlagEnableMenuControlsABTest", "False" }
                }
            },

            {
                "Version 4 (2023)",
                new Dictionary<string, string?>
                {
                    { "FFlagDisableNewIGMinDUA", "True" },
                    { "FFlagEnableInGameMenuControls", "True" },
                    { "FFlagEnableV3MenuABTest3", "True" },
                    { "FFlagEnableMenuControlsABTest", "True" }
                }
            }
        };

        // all fflags are stored as strings
        // to delete a flag, set the value as null
        public void SetValue(string key, object? value)
        {
            if (value is null)
            {
                Changes[key] = null;
                App.Logger.WriteLine($"[FastFlagManager::SetValue] Deletion of '{key}' is pending");
            }
            else
            {
                Changes[key] = value.ToString();
                App.Logger.WriteLine($"[FastFlagManager::SetValue] Value change for '{key}' to '{value}' is pending");
            }
        }

        // this will set the flag to the corresponding value if the condition is true
        // if the condition is not true, the flag will be erased
        public void SetValueIf(bool condition, string key, object? value)
        {
            if (condition)
                SetValue(key, value);
            else if (GetValue(key) is not null)
                SetValue(key, null);
        }

        public void SetValueOnce(string key, object? value)
        {
            if (GetValue(key) is null)
                SetValue(key, value);
        }

        // this returns null if the fflag doesn't exist
        public string? GetValue(string key)
        {
            // check if we have an updated change for it pushed first
            if (Changes.TryGetValue(key, out object? changedValue))
                return changedValue?.ToString();

            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();

            return null;
        }

        public override void Load()
        {
            base.Load();

            // set to 9999 by default if it doesnt already exist
            SetValueOnce("DFIntTaskSchedulerTargetFps", 9999);
            SetValueOnce("FFlagHandleAltEnterFullscreenManually", "False");
        }

        public override void Save()
        {
            App.Logger.WriteLine($"[FastFlagManager::Save] Attempting to save JSON to {FileLocation}...");

            // reload for any changes made while the menu was open
            Load();

            if (Changes.Count == 0)
            {
                App.Logger.WriteLine($"[FastFlagManager::Save] No changes to apply, aborting.");
                return;
            }

            foreach (var change in Changes)
            {
                if (change.Value is null)
                {
                    App.Logger.WriteLine($"[FastFlagManager::Save] Removing '{change.Key}'");
                    Prop.Remove(change.Key);
                    continue;
                }

                App.Logger.WriteLine($"[FastFlagManager::Save] Setting '{change.Key}' to '{change.Value}'");
                Prop[change.Key] = change.Value;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(FileLocation)!);
            File.WriteAllText(FileLocation, JsonSerializer.Serialize(Prop, new JsonSerializerOptions { WriteIndented = true }));

            Changes.Clear();

            App.Logger.WriteLine($"[FastFlagManager::Save] JSON saved!");
        }
    }
}
