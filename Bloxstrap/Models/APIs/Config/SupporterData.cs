namespace Bloxstrap.Models.APIs.Config
{
    public class SupporterData
    {
        [JsonPropertyName("monthly")]
        public SupporterGroup Monthly { get; set; } = new();

        [JsonPropertyName("oneoff")]
        public SupporterGroup OneOff { get; set; } = new();
    }
}
