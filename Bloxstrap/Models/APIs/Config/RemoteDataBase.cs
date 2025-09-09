using Wpf.Ui.Controls;

namespace Bloxstrap.Models.APIs.Config
{
    public class RemoteDataBase
    {
        // alert
        [JsonPropertyName("alertEnabled")]
        public bool AlertEnabled { get; set; } = false!;

        [JsonPropertyName("alertContent")]
        public string AlertContent { get; set; } = null!;

        [JsonPropertyName("alertSeverity")]
        public InfoBarSeverity AlertSeverity { get; set; } = InfoBarSeverity.Warning;

        // flags
        [JsonPropertyName("killFlags")]
        public bool KillFlags { get; set; } = false;
    }
}