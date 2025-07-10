namespace Bloxstrap.Models.APIs.Roblox
{
    /// <summary>
    /// Roblox.Web.Responses.Thumbnails.ThumbnailResponse
    /// </summary>
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

        /// <summary>
        /// Valid states:
        /// - Error
        /// - Completed
        /// - InReview
        /// - Pending
        /// - Blocked
        /// - TemporarilyUnavailable
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = null!;

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; } = null!;
    }
}
