using Bloxstrap.Enums;
using Bloxstrap.Helpers;

namespace Bloxstrap.Models
{
    public class SettingsFormat
    {
        // could these be moved to a separate file (something like State.json)?
        // the only problem is i havent yet figured out a way to boil down the settings handler to reduce boilerplate
        // as the Program class needs a Settings and a SettingsManager property
        // once i figure that out, then ig i could move these
        public string VersionGuid { get; set; } = "";
        public string RFUVersion { get; set; } = "";
        public string ReShadeConfigVersion { get; set; } = "";
        public string ExtraviPresetsVersion { get; set; } = "";

        // bloxstrap configuration
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.ProgressDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
        public Theme Theme { get; set; } = Theme.Default;
        public bool CheckForUpdates { get; set; } = true;
        public bool CreateDesktopIcon { get; set; } = true;
        public bool MultiInstanceLaunching { get; set; } = false;

        // channel configuration
        public string Channel { get; set; } = DeployManager.DefaultChannel;
        public bool PromptChannelChange { get; set; } = false;

        // integration configuration
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = false;
        public bool RFUEnabled { get; set; } = false;
        public bool RFUAutoclose { get; set; } = false;
        public bool UseReShade { get; set; } = false;
        public bool UseReShadeExtraviPresets { get; set; } = false;

        // mod preset configuration
        public bool UseOldDeathSound { get; set; } = true;
        public bool UseOldMouseCursor { get; set; } = false;
        public bool UseDisableAppPatch { get; set; } = false;
    }
}
