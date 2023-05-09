using System.Text.Json.Serialization;

namespace Bloxstrap.Models
{
    public class GameMessage
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = null!;
        
        [JsonPropertyName("data")]
        public string Data { get; set; } = null!;
    }
}
