namespace Bloxstrap.Models
{
    public class Supporter
    {
        [JsonPropertyName("imageAsset")]
        public string ImageAsset { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        public string Image => $"https://raw.githubusercontent.com/bloxstraplabs/config/main/assets/{ImageAsset}";
    }
}
