namespace Bloxstrap.Models.RobloxApi
{
    /// <summary>
    /// Roblox.Web.Responses.Users.UserInfoResponse
    /// </summary>
    public class UserInfoResponse
    {
        [JsonPropertyName("description")]
        public string ProfileDescription { get; set; } = null!;

        [JsonPropertyName("created")]
        public string JoinDate { get; set; } = null!;

        [JsonPropertyName("isBanned")]
        public bool IsBanned { get; set; } = null!;

        [JsonPropertyName("hasVerifiedBadge")]
        public bool HasVerifiedBadge { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = null!;
    }
}
