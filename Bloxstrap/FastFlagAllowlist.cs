namespace Bloxstrap
{
    public static class FastFlagAllowlist
    {
        public static readonly IReadOnlyList<string> Flags = new List<string>
        {
            // Geometry
            "DFIntCSGLevelOfDetailSwitchingDistance",
            "DFIntCSGLevelOfDetailSwitchingDistanceL12",
            "DFIntCSGLevelOfDetailSwitchingDistanceL23",
            "DFIntCSGLevelOfDetailSwitchingDistanceL34",

            // Rendering
            "FFlagHandleAltEnterFullscreenManually",
            "DFFlagTextureQualityOverrideEnabled",
            "DFIntTextureQualityOverride",
            "FIntDebugForceMSAASamples",
            "DFFlagDisableDPIScale",
            "FFlagDebugGraphicsPreferD3D11",
            "FFlagDebugSkyGray",
            "DFFlagDebugPauseVoxelizer",
            "DFIntDebugFRMQualityLevelOverride",
            "FIntFRMMaxGrassDistance",
            "FIntFRMMinGrassDistance",
            "FFlagDebugGraphicsPreferVulkan",
            "FFlagDebugGraphicsPreferOpenGL",

            // User Interface
            "FIntGrassMovementReducedMotionFactor"
        };

        private static readonly HashSet<string> _lookup = new(Flags, StringComparer.Ordinal);

        public static bool Contains(string name) => _lookup.Contains(name.Trim());

        public static bool IsBoolFlag(string name) => name.TrimStart('D', 'S').StartsWith("FFlag");
    }
}
