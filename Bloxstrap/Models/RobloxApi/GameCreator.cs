using System.Text.Json.Serialization;

namespace Bloxstrap.Models.RobloxApi
{
    /// <summary>
    /// Roblox.Games.Api.Models.Response.GameCreator
    /// Response model for getting the game creator
    /// </summary>
    public class GameCreator
    {
        /// <summary>
        /// The game creator id
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// The game creator name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The game creator type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// The game creator account is Luobu Real Name Verified
        /// </summary>
        [JsonPropertyName("isRNVAccount")]
        public bool IsRNVAccount { get; set; }

        /// <summary>
        /// Builder verified badge status.
        /// </summary>
        [JsonPropertyName("hasVerifiedBadge")]
        public bool HasVerifiedBadge { get; set; }
    }
}
