namespace Bloxstrap.Models.RobloxApi
{
    /// <summary>
    /// Roblox.Web.Responses.Users.UserInfoResponse
    /// </summary>
    public class UserInfoResponse
    {
        [JsonPropertyName("description")]
        public string ProfileDescription { get; set; }

        [JsonPropertyName("created")]
        public string JoinDate { get; set; }

        [JsonPropertyName("isBanned")]
        public bool IsBanned { get; set; }

        [JsonPropertyName("hasVerifiedBadge")]
        public bool HasVerifiedBadge { get; set; }

        [JsonPropertyName("name")]
        public string Username { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }
}
