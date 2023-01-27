namespace Bloxstrap.Models
{
    public class RobloxAsset
    {
        public string? Name { get; set; }
        public RobloxAssetCreator? Creator { get; set; }
    }

    public class RobloxAssetCreator
    {
        public string? Name { get; set; }
    }
}
