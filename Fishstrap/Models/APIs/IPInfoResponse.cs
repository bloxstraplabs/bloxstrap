namespace Bloxstrap.Models.APIs
{
    public class IPInfoResponse
    {
        [JsonPropertyName("city")]
        public string City { get; set; } = null!;

        [JsonPropertyName("country")]
        public string Country { get; set; } = null!;

        [JsonPropertyName("region")]
        public string Region { get; set; } = null!;
    }
}
