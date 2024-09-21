public class GithubReleaseAsset
{
    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}