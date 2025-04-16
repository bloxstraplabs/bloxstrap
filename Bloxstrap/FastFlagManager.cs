using Bloxstrap.Enums.FlagPresets;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string ClassName => nameof(FastFlagManager);

        public override string LOG_IDENT_CLASS => ClassName;
        
        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

        public bool Changed => !OriginalProp.SequenceEqual(Prop);

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            // Network
            { "Network.DisableAds", "FFlagAdServiceEnabled" },

            { "Network.DisableTelemetry1", "FFlagDebugDisableTelemetryEphemeralCounter" },
            { "Network.DisableTelemetry2", "FFlagDebugDisableTelemetryEphemeralStat" },
            { "Network.DisableTelemetry3", "FFlagDebugDisableTelemetryEventIngest" },
            { "Network.DisableTelemetry4", "FFlagDebugDisableTelemetryPoint" },
            { "Network.DisableTelemetry5", "FFlagDebugDisableTelemetryV2Counter" },
            { "Network.DisableTelemetry6", "FFlagDebugDisableTelemetryV2Event" },
            { "Network.DisableTelemetry7", "FFlagDebugDisableTelemetryV2Stat" },
            { "Network.DisableTelemetry8", "FStringTencentAuthPath" },

            { "Network.NoUnrequiredConnections", "FFlagUserUpdateInputConnections" },

            { "Network.ImproveServersideCharPos", "DFIntS2PhysicsSenderRate" },



            // Rendering
            { "Rendering.BetterPreload1", "DFIntNumAssetsMaxToPreload" },
            { "Rendering.BetterPreload2", "DFIntAssetPreloading" },

            { "Rendering.DisableDynamicHeadAnimations1", "DFIntAnimationLodFacsDistanceMin" },
            { "Rendering.DisableDynamicHeadAnimations2", "DFIntAnimationLodFacsDistanceMax" },
            { "Rendering.DisableDynamicHeadAnimations3", "DFIntAnimationLodFacsVisibilityDenominator" },

            { "Rendering.DisablePostFX", "FFlagDisablePostFx" },

            { "Rendering.ExclusiveFullscreen", "FFlagHandleAltEnterFullscreenManually" },

            { "Rendering.FixDisplayScaling", "DFFlagDisableDPIScale" },

            { "Rendering.ForceLowQuality", "DFIntDebugFRMQualityLevelOverride" },

            { "Rendering.Framerate1", "DFIntTaskSchedulerTargetFps" },
            { "Rendering.Framerate2", "FFlagTaskSchedulerLimitTargetFpsTo2402" },

            { "Rendering.HyperThreading1", "FFlagDebugCheckRenderThreading" },
            { "Rendering.HyperThreading2", "FFlagRenderDebugCheckThreading2" },

            { "Rendering.ImproveRendering", "FFlagRenderCBRefactor2" },

            // Rendering.Lighting
            { "Rendering.Lighting.BetterShadows", "FFlagRenderInitShadowmaps" },

            { "Rendering.Lighting.DisablePlayerShadows", "FIntRenderShadowIntensity" },
            
            { "Rendering.Lighting.Voxel", "DFFlagDebugRenderForceTechnologyVoxel" },
            { "Rendering.Lighting.ShadowMap", "FFlagDebugForceFutureIsBrightPhase2" },
            { "Rendering.Lighting.Future", "FFlagDebugForceFutureIsBrightPhase3" },
            { "Rendering.Lighting.Unified", "FFlagRenderUnifiedLighting12" },
            { "Rendering.Lighting.Unified2", "FFlagUnifiedLightingBetaFeature" },

            { "Rendering.Lighting.UseGPU", "FFlagFastGPULightCulling3" },
            // Rendering.Lighting

            // Rendering.Mode
            { "Rendering.Mode.D3D10", "FFlagDebugGraphicsPreferD3D11FL10" },
            { "Rendering.Mode.D3D11", "FFlagDebugGraphicsPreferD3D11" },
            { "Rendering.Mode.OpenGL", "FFlagDebugGraphicsPreferOpenGL" },
            { "Rendering.Mode.Vulkan", "FFlagDebugGraphicsPreferVulkan" },
            { "Rendering.Mode.Metal", "FFlagDebugGraphicsPreferMetal" },
            // Rendering.Mode

            { "Rendering.MovePrerender", "FFlagMovePrerender" },

            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },

            { "Rendering.OcclusionCulling", "DFFlagUseVisBugChecks" },

            // Rendering.Terrain
            { "Rendering.Terrain.NoGrass1", "FIntFRMMinGrassDistance" },
            { "Rendering.Terrain.NoGrass2", "FIntFRMMaxGrassDistance" },
            { "Rendering.Terrain.NoGrass3", "FIntRenderGrassDetailStrands" },

            { "Rendering.Terrain.NoTextures", "FIntTerrainArraySliceSize" },

            { "Rendering.Terrain.Smooth", "FFlagDebugRenderingSetDeterministic" },
            // Rendering.Terrain

            // Rendering.TextureQuality
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },
            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            // Rendering.TextureQuality
            


            // UI
            { "UI.Hide", "DFIntCanHideGuiGroupId" },

            { "UI.FontSize", "FIntFontSizePadding" },

            { "UI.PreloadFonts", "FFlagPreloadAllFonts" },

            { "UI.RemoveFullscreenTitleBar", "FIntFullscreenTitleBarTriggerDelayMillis" },

            { "UI.DisableVCBetaBadge1", "FFlagVoiceBetaBadge" },
            { "UI.DisableVCBetaBadge2", "FFlagTopBarUseNewBadge" },
            { "UI.DisableVCBetaBadge3", "FFlagBetaBadgeLearnMoreLinkFormview" },
            { "UI.DisableVCBetaBadge4", "FFlagControlBetaBadgeWithGuac" },
            { "UI.DisableVCBetaBadge5", "FStringVoiceBetaBadgeLearnMoreLink" },



            // Misc
            { "Misc.BetterHaptics", "FFlagEnableBetterHapticsResultHandling" },

            { "Misc.BetterTrackpadScroll", "FFlagBetterTrackpadScrolling" },

            { "Misc.DisableVC", "DFFlagVoiceChat4" },

            { "Misc.ImproveRaycast", "FFlagUserRaycastPerformanceImprovements" },

            { "Misc.NoAFKKick", "DFFlagDebugDisableTimeoutDisconnect" },
        };

        public static IReadOnlyDictionary<RenderingMode, string> RenderingModes => new Dictionary<RenderingMode, string>
        {
            { RenderingMode.Default, "Default" },
            { RenderingMode.D3D10, "D3D10" },
            { RenderingMode.D3D11, "D3D11" },
            { RenderingMode.OpenGL, "OpenGL" },
            { RenderingMode.Vulkan, "Vulkan" },
            { RenderingMode.Metal, "Metal" },
        };

        public static IReadOnlyDictionary<LightingMode, string> LightingModes => new Dictionary<LightingMode, string>
        {
            { LightingMode.Default, "Default" },
            { LightingMode.Voxel, "Voxel" },
            { LightingMode.ShadowMap, "ShadowMap" },
            { LightingMode.Future, "Future" },
            { LightingMode.Unified, "Unified" }
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
                if (pair.Value == "Default")
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

            if (GetPreset("Rendering.ExclusiveFullscreen") != "False")
                SetPreset("Rendering.ExclusiveFullscreen", "False");

            if (GetPreset("Rendering.Lighting.Unified") == "True")
                SetPreset("Rendering.Lighting.Unified2", "True");
            else
                SetPreset("Rendering.Lighting.Unified2", null);
        }
    }
}
