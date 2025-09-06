using Wpf.Ui.Controls;

namespace Bloxstrap.Models.APIs.Config
{
    public class RemoteDataBase
    {
        [JsonPropertyName("alertEnabled")]
        public bool AlertEnabled { get; set; } = false!;

        [JsonPropertyName("alertContent")]
        public string AlertContent { get; set; } = null!;

        [JsonPropertyName("alertSeverity")]
        public InfoBarSeverity AlertSeverity { get; set; } = InfoBarSeverity.Warning;
    }
}