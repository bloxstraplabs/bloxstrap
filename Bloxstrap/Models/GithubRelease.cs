using System.Text.Json.Serialization;

namespace Bloxstrap.Models
{
    public class GithubRelease
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("body")]
        public string? Body { get; set; }
        
        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }
        
        [JsonPropertyName("assets")]
        public List<GithubReleaseAsset>? Assets { get; set; }
    }

    public class GithubReleaseAsset
    {
        [JsonPropertyName("browser_download_url")]
        public string? BrowserDownloadUrl { get; set; }
    }
}
