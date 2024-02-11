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
        [LaunchFlag(new[] { "-preferences", "-menu" })]
        public bool IsMenuLaunch { get; private set; } = false;

        [LaunchFlag("-quiet")]
        public bool IsQuiet { get; private set; } = false;

        [LaunchFlag("-uninstall")]
        public bool IsUninstall { get; private set; } = false;

        [LaunchFlag("-nolaunch")]
        public bool IsNoLaunch { get; private set; } = false;

        [LaunchFlag("-upgrade")]
        public bool IsUpgrade { get; private set; } = false;

        public LaunchMode RobloxLaunchMode { get; private set; } = LaunchMode.Player;

        public string RobloxLaunchArgs { get; private set; } = "--app";

        /// <summary>
        /// Original launch arguments
        /// </summary>
        public string[] Args { get; private set; }

        private Dictionary<string, PropertyInfo>? _flagMap;
        
        // pizzaboxer wanted this
        private void ParseLaunchFlagProps()
        {
            _flagMap = new Dictionary<string, PropertyInfo>();

            foreach (var prop in typeof(LaunchSettings).GetProperties())
            {
                var attr = prop.GetCustomAttribute<LaunchFlagAttribute>();

                if (attr == null)
                    continue;

                if (!string.IsNullOrEmpty(attr.Name))
                {
                    _flagMap[attr.Name] = prop;
                }
                else
                {
                    foreach (var name in attr.Names!)
                        _flagMap[name] = prop;
                }
            }
        }
        
        private void ParseFlag(string arg)
        {
            const string LOG_IDENT = "LaunchSettings::ParseFlag";

            arg = arg.ToLowerInvariant();

            if (_flagMap!.ContainsKey(arg))
            {
                var prop = _flagMap[arg];
                prop.SetValue(this, true);
                App.Logger.WriteLine(LOG_IDENT, $"Started with {prop.Name} flag");
            }
        }

        private void ParseRoblox(string arg, ref int i)
        {
            if (arg.StartsWith("roblox-player:"))
            {
                RobloxLaunchArgs = ProtocolHandler.ParseUri(arg);

                RobloxLaunchMode = LaunchMode.Player;
            }
            else if (arg.StartsWith("roblox:"))
            {
                if (App.Settings.Prop.UseDisableAppPatch)
                    Frontend.ShowMessageBox(
                        Resources.Strings.Bootstrapper_DeeplinkTempEnabled,
                        MessageBoxImage.Information
                    );

                RobloxLaunchArgs = $"--app --deeplink {arg}";

                RobloxLaunchMode = LaunchMode.Player;
            }
            else if (arg.StartsWith("roblox-studio:"))
            {
                RobloxLaunchArgs = ProtocolHandler.ParseUri(arg);

                if (!RobloxLaunchArgs.Contains("-startEvent"))
                    RobloxLaunchArgs += " -startEvent www.roblox.com/robloxQTStudioStartedEvent";

                RobloxLaunchMode = LaunchMode.Studio;
            }
            else if (arg.StartsWith("roblox-studio-auth:"))
            {
                RobloxLaunchArgs = HttpUtility.UrlDecode(arg);

                RobloxLaunchMode = LaunchMode.StudioAuth;
            }
            else if (arg == "-ide")
            {
                RobloxLaunchMode = LaunchMode.Studio;

                if (Args.Length >= 2)
                {
                    string pathArg = Args[i + 1];

                    if (pathArg.StartsWith('-'))
                        return; // likely a launch flag, ignore it.

                    i++; // path arg
                    RobloxLaunchArgs = $"-task EditFile -localPlaceFile \"{pathArg}\"";
                }
            }
        }

        private void Parse()
        {
            const string LOG_IDENT = "LaunchSettings::Parse";

            App.Logger.WriteLine(LOG_IDENT, "Parsing launch arguments");

#if DEBUG
            App.Logger.WriteLine(LOG_IDENT, $"Launch arguments: {string.Join(' ', Args)}");
#endif

            if (Args.Length == 0)
            {
                App.Logger.WriteLine(LOG_IDENT, "No launch arguments to parse");
                return;
            }

            int idx = 0;
            string firstArg = Args[0];

            // check & handle roblox arg
            if (!firstArg.StartsWith('-') || firstArg == "-ide")
            {
                ParseRoblox(firstArg, ref idx);
                idx++; // roblox arg
            }

            // check if there are any launch flags
            if (idx > Args.Length - 1)
                return;

            App.Logger.WriteLine(LOG_IDENT, "Parsing launch flags");

            // map out launch flags
            ParseLaunchFlagProps();

            // parse any launch flags
            for (int i = idx; i < Args.Length; i++)
                ParseFlag(Args[i]);

            // cleanup flag map
            _flagMap!.Clear();
            _flagMap = null;
        }

        public LaunchSettings(string[] args)
        {
            Args = args;
            Parse();
        }
    }
}
