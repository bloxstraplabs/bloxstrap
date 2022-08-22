using System.Text.Json.Serialization;

namespace Bloxstrap.Models
{
    public class RobloxThumbnails
    {
        [JsonPropertyName("data")]
        public List<RobloxThumbnail>? Data { get; set; }
    }

    public class RobloxThumbnail
    {
        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }
    }
}
