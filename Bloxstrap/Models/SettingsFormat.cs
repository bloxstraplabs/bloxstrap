using Bloxstrap.Enums;
using Bloxstrap.Helpers;

namespace Bloxstrap.Models
{
    public class SettingsFormat
    {
        public string Channel { get; set; } = DeployManager.DefaultChannel;
        public string VersionGuid { get; set; } = "";

        public bool CheckForUpdates { get; set; } = true;
        public bool PromptChannelChange { get; set; } = false;

        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.ProgressDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
        public Theme Theme { get; set; } = Theme.Default;

        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = false;
        public bool RFUEnabled { get; set; } = false;
        public bool RFUAutoclose { get; set; } = false;

        // could these be moved to a separate file (something like State.json)?
        // the only problem is i havent yet figured out a way to boil down the settings handler to reduce boilerplate
        // as the Program class needs a Settings and a SettingsManager property
        // once i figure that out, then ig i could move these
        public string RFUVersion { get; set; } = "";
        public string ReShadeConfigVersion { get; set; } = "";
        public string ExtraviPresetsVersion { get; set; } = "";

        public bool UseReShade { get; set; } = false;
        public bool UseReShadeExtraviPresets { get; set; } = false;

        public bool UseOldDeathSound { get; set; } = true;
        public bool UseOldMouseCursor { get; set; } = false;
        public bool UseDisableAppPatch { get; set; } = false;
    }
}
