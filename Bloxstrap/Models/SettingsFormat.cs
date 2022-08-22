using System;
using System.Collections.Generic;
using System.Linq;
using Bloxstrap.Enums;

namespace Bloxstrap.Models
{
    public class SettingsFormat
    {
        public string VersionGuid { get; set; } = "";

        public bool CheckForUpdates { get; set; } = true;

        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.ProgressDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;

        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = false;
        public bool RFUEnabled { get; set; } = false;
        public bool RFUAutoclose { get; set; } = false;

        public bool UseOldDeathSound { get; set; } = true;
        public bool UseOldMouseCursor { get; set; } = false;
    }
}
