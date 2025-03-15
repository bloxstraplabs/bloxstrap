using Bloxstrap.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Bloxstrap
{
    public class LaunchSettings
    {
        public LaunchFlag MenuFlag                  { get; } = new("preferences,menu,settings");

        public LaunchFlag WatcherFlag               { get; } = new("watcher");

        public LaunchFlag MultiInstanceWatcherFlag  { get; } = new("multiinstancewatcher");

        public LaunchFlag BackgroundUpdaterFlag     { get; } = new("backgroundupdater");

        public LaunchFlag QuietFlag                 { get; } = new("quiet");

        public LaunchFlag UninstallFlag             { get; } = new("uninstall");

        public LaunchFlag NoLaunchFlag              { get; } = new("nolaunch");
        
        public LaunchFlag TestModeFlag              { get; } = new("testmode");

        public LaunchFlag NoGPUFlag                 { get; } = new("nogpu");

        public LaunchFlag UpgradeFlag               { get; } = new("upgrade");
        
        public LaunchFlag PlayerFlag                { get; } = new("player");
        
        public LaunchFlag StudioFlag                { get; } = new("studio");

        public LaunchFlag VersionFlag               { get; } = new("version");

        public LaunchFlag ChannelFlag               { get; } = new("channel");

        public LaunchFlag ForceFlag                 { get; } = new("force");

#if DEBUG
        public bool BypassUpdateCheck => true;
#else
        public bool BypassUpdateCheck => UninstallFlag.Active || WatcherFlag.Active;
#endif

        public LaunchMode RobloxLaunchMode { get; set; } = LaunchMode.None;

        public string RobloxLaunchArgs { get; set; } = "";

        /// <summary>
        /// Original launch arguments
        /// </summary>
        public string[] Args { get; private set; }

        public LaunchSettings(string[] args)
        {
            const string LOG_IDENT = "LaunchSettings::LaunchSettings";

#if DEBUG
            App.Logger.WriteLine(LOG_IDENT, $"Launched with arguments: {string.Join(' ', args)}");
#endif

            Args = args;

            Dictionary<string, LaunchFlag> flagMap = new();

            // build flag map
            foreach (var prop in this.GetType().GetProperties())
            {
                if (prop.PropertyType != typeof(LaunchFlag))
                    continue;

                if (prop.GetValue(this) is not LaunchFlag flag)
                    continue;

                foreach (string identifier in flag.Identifiers.Split(','))
                    flagMap.Add(identifier, flag);
            }

            int startIdx = 0;

            // infer roblox launch uris
            if (Args.Length >= 1)
            {
                string arg = Args[0];

                if (arg.StartsWith("roblox:", StringComparison.OrdinalIgnoreCase) 
                    || arg.StartsWith("roblox-player:", StringComparison.OrdinalIgnoreCase))
                {
                    App.Logger.WriteLine(LOG_IDENT, "Got Roblox player argument");
                    RobloxLaunchMode = LaunchMode.Player;
                    RobloxLaunchArgs = arg;
                    startIdx = 1;
                }
                else if (arg.StartsWith("version-"))
                {
                    App.Logger.WriteLine(LOG_IDENT, "Got version argument");
                    VersionFlag.Active = true;
                    VersionFlag.Data = arg;
                    startIdx = 1;
                }
            }

            // parse
            for (int i = startIdx; i < Args.Length; i++)
            {
                string arg = Args[i];

                if (!arg.StartsWith('-'))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Invalid argument: {arg}");
                    continue;
                }

                string identifier = arg[1..];

                if (!flagMap.TryGetValue(identifier, out LaunchFlag? flag) || flag is null)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Unknown argument: {identifier}");
                    continue;
                }

                if (flag.Active)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Tried to set {identifier} flag twice");
                    continue;
                }

                flag.Active = true;

                if (i < Args.Length - 1 && Args[i+1] is string nextArg && !nextArg.StartsWith('-'))
                {
                    flag.Data = nextArg;
                    i++;
                    App.Logger.WriteLine(LOG_IDENT, $"Identifier '{identifier}' is active with data");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Identifier '{identifier}' is active");
                }
            }

            if (VersionFlag.Active)
                RobloxLaunchMode = LaunchMode.Unknown; // determine in bootstrapper

            if (PlayerFlag.Active)
                ParsePlayer(PlayerFlag.Data);
            else if (StudioFlag.Active)
                ParseStudio(StudioFlag.Data);
        }

        private void ParsePlayer(string? data)
        {
            const string LOG_IDENT = "LaunchSettings::ParsePlayer";

            RobloxLaunchMode = LaunchMode.Player;

            if (!String.IsNullOrEmpty(data))
            {
                App.Logger.WriteLine(LOG_IDENT, "Got Roblox launch arguments");
                RobloxLaunchArgs = data;
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "No Roblox launch arguments were provided");
            }
        }

        private void ParseStudio(string? data)
        {
            const string LOG_IDENT = "LaunchSettings::ParseStudio";

            RobloxLaunchMode = LaunchMode.Studio;

            if (String.IsNullOrEmpty(data))
            {
                App.Logger.WriteLine(LOG_IDENT, "No Roblox launch arguments were provided");
                return;
            }

            if (data.StartsWith("roblox-studio:"))
            {
                App.Logger.WriteLine(LOG_IDENT, "Got Roblox Studio launch arguments");
                RobloxLaunchArgs = data;
            }
            else if (data.StartsWith("roblox-studio-auth:"))
            {
                App.Logger.WriteLine(LOG_IDENT, "Got Roblox Studio Auth launch arguments");
                RobloxLaunchMode = LaunchMode.StudioAuth;
                RobloxLaunchArgs = data;
            }
            else
            {
                // likely a local path
                App.Logger.WriteLine(LOG_IDENT, "Got Roblox Studio local place file");
                RobloxLaunchArgs = $"-task EditFile -localPlaceFile \"{data}\"";
            }
        }
    }
}
