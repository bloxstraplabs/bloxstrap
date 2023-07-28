namespace Bloxstrap.Models.BloxstrapRPC
{
    class RichPresenceImage
    {
        [JsonPropertyName("assetId")]
        public ulong? AssetId { get; set; }

        [JsonPropertyName("hoverText")]
        public string? HoverText { get; set; }
    }
}
