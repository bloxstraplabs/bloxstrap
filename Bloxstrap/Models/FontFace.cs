namespace Bloxstrap.Models
{
    public class FontFace
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("weight")]
        public int Weight { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; } = null!;

        [JsonPropertyName("assetId")]
        public string AssetId { get; set; } = null!;
    }
}
