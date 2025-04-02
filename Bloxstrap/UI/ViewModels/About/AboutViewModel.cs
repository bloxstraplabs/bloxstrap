using System.Windows;

namespace Bloxstrap.UI.ViewModels.About
{
    public class AboutViewModel : NotifyPropertyChangedViewModel
    {
        public string Version => string.Format(Strings.Menu_About_Version, App.ShortCommitHash);

        public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;

        public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();
        public string BuildCommitHashUrl => $"{App.ProjectRepository}/commit/{BuildMetadata.CommitHash}";

        public Visibility BuildCommitVisibility => App.IsActionBuild ? Visibility.Visible : Visibility.Collapsed;
    }
}
