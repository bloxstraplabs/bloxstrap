namespace Bloxstrap.Models
{
    public class IPInfoResponse
    {
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("ip")]
        public string IP { get; set; }

        [JsonPropertyName("postal")]
        public string Postal { get; set; }

        [JsonPropertyName("timezone")]
        public string TimeZone { get; set; }

        [JsonPropertyName("readme")]
        public string ReadME { get; set; }

        [JsonPropertyName("org")]
        public string Org { get; set; }

        [JsonPropertyName("loc")]
        public string Loc { get; set; }

        [JsonPropertyName("anycast")]
        public string AnyCast { get; set; }
    }
}
