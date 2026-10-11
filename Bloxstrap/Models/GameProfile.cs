namespace Bloxstrap.Models
{
    public class GameProfile
    {
        public long PlaceId { get; set; }

        public long UniverseId { get; set; }

        public string Name { get; set; } = "";

        public string FastFlagProfile { get; set; } = "";

        public string FramerateCap { get; set; } = "";

        public string QualityLevel { get; set; } = "";

        public ProfileToggle Crosshair { get; set; } = ProfileToggle.Default;

        public string DiscordStatus { get; set; } = "";
    }
}
