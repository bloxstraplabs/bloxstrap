using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Bloxstrap.Enums;
using Bloxstrap.Helpers;

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
        public bool MultiInstanceLaunching { get; set; } = false;

        // channel configuration
        public string Channel { get; set; } = DeployManager.DefaultChannel;
        public bool PromptChannelChange { get; set; } = false;

        // integration configuration
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = true;
        public bool RFUEnabled { get; set; } = false;
        public bool RFUAutoclose { get; set; } = false;
        public bool UseReShade { get; set; } = true;
        public bool UseReShadeExtraviPresets { get; set; } = true;
        public bool ShowServerDetails { get; set; } = false;
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();

        // mod preset configuration
        public bool UseOldDeathSound { get; set; } = true;
        public bool UseOldMouseCursor { get; set; } = false;
        public bool UseDisableAppPatch { get; set; } = false;
        public bool DisableFullscreenOptimizations { get; set; } = false;
    }
}
