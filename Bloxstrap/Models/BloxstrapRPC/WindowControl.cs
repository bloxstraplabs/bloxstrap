namespace Bloxstrap.Models.BloxstrapRPC
{
    public class WindowMessage
    {
        [JsonPropertyName("x")]
        public int? X { get; set; }

        [JsonPropertyName("y")]
        public int? Y { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("scaleWidth")]
        public int? ScaleWidth { get; set; }

        [JsonPropertyName("scaleHeight")]
        public int? ScaleHeight { get; set; }

        [JsonPropertyName("reset")]
        public bool? Reset { get; set; }
    }

    public class WindowTransparencyMessage
    {
        [JsonPropertyName("transparency")]
        public float? Transparency { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("useAlpha")]
        public bool? UseAlpha { get; set; }
    }

    public class WindowColorMessage
    {
        [JsonPropertyName("border")]
        public string? Border { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("reset")]
        public bool? Reset { get; set; }
    }

    public class WallpaperMessage
    {
        [JsonPropertyName("assetId")]
        public ulong? AssetId { get; set; }

        [JsonPropertyName("style")]
        public string? Style { get; set; }

        [JsonPropertyName("reset")]
        public bool? Reset { get; set; }
    }

    public class DesktopVisibilityMessage
    {
        [JsonPropertyName("taskbar")]
        public bool? Taskbar { get; set; }

        [JsonPropertyName("desktopIcons")]
        public bool? DesktopIcons { get; set; }
    }

    public class NotificationMessage
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }
    }
}
