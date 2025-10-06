using Bloxstrap.Models.APIs.RoValra;

namespace Bloxstrap.Models.APIs
{
    public class RoValraTimeResponse
    {
        [JsonPropertyName("servers")]
        public List<RoValraServer>? Servers { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status = null!;
    }
}
