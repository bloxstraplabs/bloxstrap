namespace Bloxstrap.Models.APIs.Roblox
{
    internal class ThumbnailRequest
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("targetId")]
        public ulong TargetId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Avatar";

        [JsonPropertyName("size")]
        public string Size { get; set; } = "30x30";

        [JsonPropertyName("format")]
        public string Format { get; set; } = "Png";

        [JsonPropertyName("isCircular")]
        public bool IsCircular { get; set; } = true;
    }
}
