using System.Collections.ObjectModel;

namespace Bloxstrap.Models
{
    public class Settings
    {
        // bloxstrap configuration
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.FluentDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
        public string BootstrapperTitle { get; set; } = App.ProjectName;
        public string BootstrapperIconCustomLocation { get; set; } = "";
        public Theme Theme { get; set; } = Theme.Default;
        public bool CheckForUpdates { get; set; } = true;
        public bool CreateDesktopIcon { get; set; } = true;
        public bool LaunchConfirmation { get; set; } = true;

        // channel configuration
        public string Channel { get; set; } = RobloxDeployment.DefaultChannel;

        // integration configuration
        public bool EnableActivityTracking { get; set; } = true;
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = true;
        public bool ShowServerDetails { get; set; } = false;
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();

        // mod preset configuration
        public bool UseOldDeathSound { get; set; } = true;
        public bool UseOldCharacterSounds { get; set; } = false;
        public bool UseDisableAppPatch { get; set; } = false;
        public bool UseOldAvatarBackground { get; set; } = false;
        public CursorType CursorType { get; set; } = CursorType.Default;
        public EmojiType EmojiType { get; set; } = EmojiType.Default;
        public bool DisableFullscreenOptimizations { get; set; } = false;
    }
}
