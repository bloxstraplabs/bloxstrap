namespace Bloxstrap.Models.APIs.Config
{
    public class FFlagPresets
    {
        public string Version { get; set; } = null!;

        public Dictionary<string, string> Flags { get; set; } = null!;

        public Dictionary<string, List<FFlagPreset>> Presets { get; set; } = null!;
    }
}
