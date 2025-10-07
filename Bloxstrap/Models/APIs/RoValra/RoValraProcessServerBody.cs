namespace Bloxstrap.Models.APIs.RoValra
{
    public class RoValraProcessServerBody
    {
        [JsonPropertyName("place_id")]
        public long PlaceId { get; set; }

        [JsonPropertyName("server_ids")]
        public List<string> ServerIds { get; set; } = null!;
    }
}