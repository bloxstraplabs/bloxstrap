using System.Windows;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class AboutViewModel
    {
        public string Version => string.Format(Resources.Strings.Menu_About_Version, App.Version);

        public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;

        public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();
        public string BuildCommitHashUrl => $"https://github.com/{App.ProjectRepository}/commit/{BuildMetadata.CommitHash}";

        public Visibility BuildInformationVisibility => BuildMetadata.CommitRef.StartsWith("tag") ? Visibility.Collapsed : Visibility.Visible;
        public Visibility BuildCommitVisibility => string.IsNullOrEmpty(BuildMetadata.CommitHash) ? Visibility.Collapsed : Visibility.Visible;
    }
}
