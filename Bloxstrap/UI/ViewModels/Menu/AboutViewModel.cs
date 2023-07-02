using System;
using System.Windows;

using Bloxstrap.Extensions;
using Bloxstrap.Models.Attributes;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class AboutViewModel
    {
        public string Version => $"Version {App.Version}";

        public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;

        public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();
        public string BuildCommitHashUrl => $"https://github.com/{App.ProjectRepository}/commit/{BuildMetadata.CommitHash}";

        public Visibility BuildInformationVisibility => BuildMetadata.CommitRef.StartsWith("tag") ? Visibility.Collapsed : Visibility.Visible;
        public Visibility BuildCommitVisibility => string.IsNullOrEmpty(BuildMetadata.CommitHash) ? Visibility.Collapsed : Visibility.Visible;
    }
}
