namespace Bloxstrap.Models.RobloxApi
{
    /// <summary>
    /// Roblox.Users.Api.GetUserResponse
    /// </summary>
    public class GetUserResponse
    {
        /// <summary>
        /// The user description.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        /// <summary>
        /// When the user signed up.
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Whether the user is banned
        /// </summary>
        [JsonPropertyName("isBanned")]
        public bool IsBanned { get; set; }

        /// <summary>
        /// Unused, legacy attribute… rely on its existence.
        /// </summary>
        [JsonPropertyName("externalAppDisplayName")]
        public string ExternalAppDisplayName { get; set; } = null!;

        /// <summary>
        /// The user's verified badge status.
        /// </summary>
        [JsonPropertyName("hasVerifiedBadge")]
        public bool HasVerifiedBadge { get; set; }

        /// <summary>
        /// The user Id.
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// The user name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The user DisplayName.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = null!;
    }
}
