namespace Bloxstrap.Models.RobloxApi
{
    public class GetUserResponse
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("isBanned")]
        public bool IsBanned { get; set; }

        [JsonPropertyName("externalAppDisplayName")]
        public string ExternalAppDisplayName { get; set; } = null!;

        [JsonPropertyName("hasVerifiedBadge")]
        public bool HasVerifiedBadge { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = null!;
    }
}
