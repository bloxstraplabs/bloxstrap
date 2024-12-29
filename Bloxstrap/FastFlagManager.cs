using Bloxstrap.Enums.FlagPresets;
using System.Security.Policy;
using System.Windows;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string ClassName => nameof(FastFlagManager);

        public override string LOG_IDENT_CLASS => ClassName;

        public override string ProfilesLocation => Path.Combine(Paths.Base, "Profiles");

        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

        public bool Changed => !OriginalProp.SequenceEqual(Prop);

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            // Activity watcher
            { "Network.Log", "FLogNetwork" },
            { "Players.LogLevel", "FStringDebugLuaLogLevel" },
            { "Players.LogPattern", "FStringDebugLuaLogPattern" },

            // Debug
            { "Debug.FlagState", "FStringDebugShowFlagState"},
            { "Debug.PingBreakdown", "DFFlagDebugPrintDataPingBreakDown" },

            // Presets and stuff
            { "Rendering.Framerate", "DFIntTaskSchedulerTargetFps" },
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },
            { "Rendering.DisablePostFX", "FFlagDisablePostFx" },
            { "Rendering.ShadowIntensity", "FIntRenderShadowIntensity" },

            // Rendering engines
            { "Rendering.Mode.DisableD3D11", "FFlagDebugGraphicsDisableDirect3D11" },
            { "Rendering.Mode.D3D11", "FFlagDebugGraphicsPreferD3D11" },
            { "Rendering.Mode.Vulkan", "FFlagDebugGraphicsPreferVulkan" },
            { "Rendering.Mode.OpenGL", "FFlagDebugGraphicsPreferOpenGL" },
            { "Rendering.Mode.D3D10", "FFlagDebugGraphicsPreferD3D11FL10" },
            { "Rendering.FixHighlights", "FFlagHighlightOutlinesOnMobile"},

            // Lighting technology
            { "Rendering.Lighting.Voxel", "DFFlagDebugRenderForceTechnologyVoxel" },
            { "Rendering.Lighting.ShadowMap", "FFlagDebugForceFutureIsBrightPhase2" },
            { "Rendering.Lighting.Future", "FFlagDebugForceFutureIsBrightPhase3" },

            // Texture quality
            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },
            { "Rendering.TerrainTextureQuality", "FIntTerrainArraySliceSize" },

            // Guis
            { "UI.Hide", "DFIntCanHideGuiGroupId" },
            { "UI.Hide.Toggles", "FFlagUserShowGuiHideToggles"},
            { "UI.FontSize", "FIntFontSizePadding" },

            // Telemetry
            { "Telemetry.EpCounter", "FFlagDebugDisableTelemetryEphemeralCounter"},
            { "Telemetry.EpStats", "FFlagDebugDisableTelemetryEphemeralStat"},
            { "Telemetry.Event", "FFlagDebugDisableTelemetryEventIngest"},
            { "Telemetry.V2Counter", "FFlagDebugDisableTelemetryV2Counter"},
            { "Telemetry.V2Event", "FFlagDebugDisableTelemetryV2Event"},
            { "Telemetry.V2Stats", "FFlagDebugDisableTelemetryV2Stat"},

            // Fullscreen bar
            { "UI.FullscreenTitlebarDelay", "FIntFullscreenTitleBarTriggerDelayMillis" },

            // useless
            { "UI.Menu.Style.V2Rollout", "FIntNewInGameMenuPercentRollout3" },
            { "UI.Menu.Style.EnableV4.1", "FFlagEnableInGameMenuControls" },
            { "UI.Menu.Style.EnableV4.2", "FFlagEnableInGameMenuModernization" },
            { "UI.Menu.Style.EnableV4Chrome", "FFlagEnableInGameMenuChrome" },
            { "UI.Menu.Style.ReportButtonCutOff", "FFlagFixReportButtonCutOff" },

            // Chrome ui
            { "UI.Menu.ChromeUI", "FFlagEnableInGameMenuChromeABTest4" },

            // Menu stuff
            { "Menu.VRToggles", "FFlagAlwaysShowVRToggleV3" },
            { "Menu.Feedback", "FFlagDisableFeedbackSoothsayerCheck" },
            { "Menu.LanguageSelector", "FIntV1MenuLanguageSelectionFeaturePerMillageRollout" },
            { "Menu.Haptics", "FFlagAddHapticsToggle" },
            { "Menu.Framerate", "FFlagGameBasicSettingsFramerateCap5"},
            { "Menu.ChatTranslation", "FFlagChatTranslationSettingEnabled3" }


            //{ "UI.Menu.Style.ABTest.1", "FFlagEnableMenuControlsABTest" },
            //{ "UI.Menu.Style.ABTest.2", "FFlagEnableV3MenuABTest3" },
            //{ "UI.Menu.Style.ABTest.3", "FFlagEnableInGameMenuChromeABTest3" },
            //{ "UI.Menu.Style.ABTest.4", "FFlagEnableInGameMenuChromeABTest4" }
        };

        public static IReadOnlyDictionary<RenderingMode, string> RenderingModes => new Dictionary<RenderingMode, string>
        {
            { RenderingMode.Default, "None" },
            { RenderingMode.Vulkan, "Vulkan" },
            { RenderingMode.OpenGL, "OpenGL" },
            { RenderingMode.D3D11, "D3D11" },
            { RenderingMode.D3D10, "D3D10" },
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
        //public static IReadOnlyDictionary<InGameMenuVersion, Dictionary<string, string?>> IGMenuVersions => new Dictionary<InGameMenuVersion, Dictionary<string, string?>>
        //{
        //    {
        //        InGameMenuVersion.Default,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", null },
        //            { "EnableV4", null },
        //            { "EnableV4Chrome", null },
        //            { "ABTest", null },
        //            { "ReportButtonCutOff", null }
        //        }
        //    },

        //    {
        //        InGameMenuVersion.V1,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", "0" },
        //            { "EnableV4", "False" },
        //            { "EnableV4Chrome", "False" },
        //            { "ABTest", "False" },
        //            { "ReportButtonCutOff", "False" }
        //        }
        //    },

        //    {
        //        InGameMenuVersion.V2,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", "100" },
        //            { "EnableV4", "False" },
        //            { "EnableV4Chrome", "False" },
        //            { "ABTest", "False" },
        //            { "ReportButtonCutOff", null }
        //        }
        //    },

        //    {
        //        InGameMenuVersion.V4,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", "0" },
        //            { "EnableV4", "True" },
        //            { "EnableV4Chrome", "False" },
        //            { "ABTest", "False" },
        //            { "ReportButtonCutOff", null }
        //        }
        //    },

        //    {
        //        InGameMenuVersion.V4Chrome,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", "0" },
        //            { "EnableV4", "True" },
        //            { "EnableV4Chrome", "True" },
        //            { "ABTest", "False" },
        //            { "ReportButtonCutOff", null }
        //        }
        //    }
        //};

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

        public string? GetPreset(string name)
        {
            if (!PresetFlags.ContainsKey(name))
            {
                App.Logger.WriteLine("FastFlagManager::GetPreset", $"Could not find preset {name}");
                Debug.Assert(false, $"Could not find preset {name}");
                return null;
            }

            return GetValue(PresetFlags[name]);
        }

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

        public override void Save()
        {
            // convert all flag values to strings before saving

            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;

            base.Save();

            // clone the dictionary
            OriginalProp = new(Prop);
        }

        public override void Load(bool alertFailure = true)
        {
            base.Load(alertFailure);

            // clone the dictionary
            OriginalProp = new(Prop);

            if (GetPreset("Network.Log") != "7")
                SetPreset("Network.Log", "7");

            if (GetPreset("Rendering.ManualFullscreen") != "False")
                SetPreset("Rendering.ManualFullscreen", "False");

            if (GetPreset("Rendering.FixHighlights") != "True")
                SetPreset("Rendering.FixHighlights", "True");
        }

        public void DeleteProfile(string Profile)
        {
            try
            {
                string profilesDirectory = Path.Combine(Paths.Base, Paths.SavedFlagProfiles);

                if (!Directory.Exists(profilesDirectory))
                    Directory.CreateDirectory(profilesDirectory);

                if (String.IsNullOrEmpty(Profile))
                    return;

                File.Delete(Path.Combine(profilesDirectory, Profile));
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        }
    }
}
