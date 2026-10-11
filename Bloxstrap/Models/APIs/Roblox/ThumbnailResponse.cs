namespace Bloxstrap.Models.APIs.Roblox
{
    public class ThumbnailResponse
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = null!;

        [JsonPropertyName("errorCode")]
        public int ErrorCode { get; set; } = 0;

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; } = null;

        [JsonPropertyName("targetId")]
        public long TargetId { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; } = null!;

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; } = null!;
    }
}
