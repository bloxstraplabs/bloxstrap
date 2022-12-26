using Bloxstrap.Enums;
using Bloxstrap.Helpers;

namespace Bloxstrap.Models
{
    public class SettingsFormat
    {
        public string Channel { get; set; } = DeployManager.DefaultChannel;
        public string VersionGuid { get; set; } = "";

        public bool CheckForUpdates { get; set; } = true;

        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.ProgressDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
        public Theme Theme { get; set; } = Theme.Default;

        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = false;
        public bool RFUEnabled { get; set; } = false;
        public bool RFUAutoclose { get; set; } = false;

        public string RFUVersion { get; set; } = "";

        public bool UseOldDeathSound { get; set; } = true;
        public bool UseOldMouseCursor { get; set; } = false;
        public bool UseDisableAppPatch { get; set; } = false;
    }
}
