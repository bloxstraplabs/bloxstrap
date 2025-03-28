namespace Bloxstrap.Models.APIs.Roblox
{
    internal class ThumbnailRequest
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("targetId")]
        public ulong TargetId { get; set; }

        /// <summary>
        /// TODO: make this an enum
        /// List of valid types can be found at https://thumbnails.roblox.com//docs/index.html
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "Avatar";

        /// <summary>
        /// List of valid sizes can be found at https://thumbnails.roblox.com//docs/index.html
        /// </summary>
        [JsonPropertyName("size")]
        public string Size { get; set; } = "30x30";

        /// <summary>
        /// TODO: make this an enum
        /// List of valid types can be found at https://thumbnails.roblox.com//docs/index.html
        /// </summary>
        [JsonPropertyName("format")]
        public string Format { get; set; } = "Png";

        [JsonPropertyName("isCircular")]
        public bool IsCircular { get; set; } = true;
    }
}
