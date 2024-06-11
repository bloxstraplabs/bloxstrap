namespace Bloxstrap.Models
{
    public class ClientFlagSettings
    {
        [JsonPropertyName("applicationSettings")]
        public Dictionary<string, string>? ApplicationSettings { get; set; }
    }
}
