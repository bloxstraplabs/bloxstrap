namespace Bloxstrap.Models.APIs
{
    public class RoValraGeolocation
    {
        [JsonPropertyName("location")]
        public RoValraLocation? Location { get; set; }
    }

    public class RoValraLocation
    {
        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("country_name")]
        public string? Country { get; set; }
    }
}
