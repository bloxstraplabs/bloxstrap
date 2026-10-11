namespace Bloxstrap.Models
{
    public class CustomIntegration
    {
        public string Name { get; set; } = "";
        public string Location { get; set; } = "";
        public string LaunchArgs { get; set; } = "";
        public bool AutoClose { get; set; } = true;
        public string PlaceId { get; set; } = "";
        public bool CloseWithGame { get; set; } = true;

        [JsonIgnore]
        public bool IsGameSpecific => long.TryParse(PlaceId.Trim(), out _);
    }
}
