namespace Bloxstrap.Models.BloxstrapRPC
{
    class RichPresence
    {
        [JsonPropertyName("details")]
        public string? Details { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("timeStart")]
        public ulong? TimestampStart { get; set; }

        [JsonPropertyName("timeEnd")]
        public ulong? TimestampEnd { get; set; }

        [JsonPropertyName("smallImage")]
        public RichPresenceImage? SmallImage { get; set; }

        [JsonPropertyName("largeImage")]
        public RichPresenceImage? LargeImage { get; set; }
    }
}
