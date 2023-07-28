namespace Bloxstrap.Models.BloxstrapRPC
{
    class RichPresence
    {
        [JsonPropertyName("details")]
        public string? Details { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("timestampStart")]
        public ulong? TimestampStart { get; set; }

        [JsonPropertyName("timestampEnd")]
        public ulong? TimestampEnd { get; set; }

        [JsonPropertyName("smallImage")]
        public RichPresenceImage? SmallImage { get; set; }

        [JsonPropertyName("largeImage")]
        public RichPresenceImage? LargeImage { get; set; }
    }
}
