using System.Web;
using System.Windows;

using Microsoft.Win32;

namespace Bloxstrap
{
    static class ProtocolHandler
    {
        private const string RobloxPlaceKey = "Roblox.Place";

        // map uri keys to command line args
        private static readonly IReadOnlyDictionary<string, string> UriKeyArgMap = new Dictionary<string, string>()
        {
            // excluding roblox-player and launchtime
            { "launchmode", "--" },
            { "gameinfo", "-t " },
            { "placelauncherurl", "-j "},
            { "launchtime", "--launchtime=" },
            { "browsertrackerid", "-b " },
            { "robloxLocale", "--rloc " },
            { "gameLocale", "--gloc " },
            { "channel", "-channel " },
            // studio
            { "task", "-task " },
            { "placeId", "-placeId " },
            { "universeId", "-universeId " },
            { "userId", "-userId " }
        };

        public static string ParseUri(string protocol)
        {
            string[] keyvalPair;
            string key;
            string val;
            bool channelArgPresent = false;

            StringBuilder commandLine = new();

            foreach (var parameter in protocol.Split('+'))
            {
                if (!parameter.Contains(':'))
                    continue;

                keyvalPair = parameter.Split(':');
                key = keyvalPair[0];
                val = keyvalPair[1];

                if (!UriKeyArgMap.ContainsKey(key) || string.IsNullOrEmpty(val))
                    continue;

                if (key == "launchmode" && val == "play")
                    val = "app";

                if (key == "placelauncherurl")
                    val = HttpUtility.UrlDecode(val);

                // we'll set this before launching because for some reason roblox just refuses to launch if its like a few minutes old so ???
                if (key == "launchtime")
                    val = "LAUNCHTIMEPLACEHOLDER";

                if (key == "channel" && !String.IsNullOrEmpty(val))
                {
                    channelArgPresent = true;
                    EnrollChannel(val);

                    // we'll set the arg when launching
                    continue;
                }

                commandLine.Append(UriKeyArgMap[key] + val + " ");
            }

            if (!channelArgPresent)
                EnrollChannel(RobloxDeployment.DefaultChannel);

            return commandLine.ToString();
        }

        public static void ChangeChannel(string channel)
        {
            if (channel.ToLowerInvariant() == App.Settings.Prop.Channel.ToLowerInvariant())
                return;

            if (App.Settings.Prop.ChannelChangeMode == ChannelChangeMode.Ignore)
                return;

            if (App.Settings.Prop.ChannelChangeMode != ChannelChangeMode.Automatic)
            {
                if (channel == App.State.Prop.LastEnrolledChannel)
                    return;

                MessageBoxResult result = Controls.ShowMessageBox(
                    string.Format(Resources.Strings.ProtocolHandler_RobloxSwitchedChannel, channel, App.Settings.Prop.Channel),
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo
                );

                if (result != MessageBoxResult.Yes)
                    return;
            }

            App.Logger.WriteLine("Protocol::ParseUri", $"Changed Roblox channel from {App.Settings.Prop.Channel} to {channel}");
            App.Settings.Prop.Channel = channel;
        }

        public static void EnrollChannel(string channel)
        {
            ChangeChannel(channel);
            App.State.Prop.LastEnrolledChannel = channel;
            App.State.Save();
        }

        public static void Register(string key, string name, string handler)
        {
            string handlerArgs = $"\"{handler}\" %1";
            
            using RegistryKey uriKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{key}");
            using RegistryKey uriIconKey = uriKey.CreateSubKey("DefaultIcon");
            using RegistryKey uriCommandKey = uriKey.CreateSubKey(@"shell\open\command");

            if (uriKey.GetValue("") is null)
            {
                uriKey.SetValue("", $"URL: {name} Protocol");
                uriKey.SetValue("URL Protocol", "");
            }

            if (uriCommandKey.GetValue("") as string != handlerArgs)
            {
                uriIconKey.SetValue("", handler);
                uriCommandKey.SetValue("", handlerArgs);
            }
        }

        public static void RegisterRobloxPlace(string handler)
        {
            const string keyValue = "Roblox Place";
            string handlerArgs = $"\"{handler}\" -ide \"%1\"";
            string iconValue = $"{handler},0";

            using RegistryKey uriKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + RobloxPlaceKey);
            using RegistryKey uriIconKey = uriKey.CreateSubKey("DefaultIcon");
            using RegistryKey uriOpenKey = uriKey.CreateSubKey(@"shell\Open");
            using RegistryKey uriCommandKey = uriOpenKey.CreateSubKey(@"command");

            if (uriKey.GetValue("") as string != keyValue)
                uriKey.SetValue("", keyValue);

            if (uriCommandKey.GetValue("") as string != handlerArgs)
                uriCommandKey.SetValue("", handlerArgs);

            if (uriOpenKey.GetValue("") as string != "Open")
                uriOpenKey.SetValue("", "Open");

            if (uriIconKey.GetValue("") as string != iconValue)
                uriIconKey.SetValue("", iconValue);
        }

        public static void RegisterExtension(string key)
        {
            using RegistryKey uriKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{key}");
            uriKey.CreateSubKey(RobloxPlaceKey + @"\ShellNew");

            if (uriKey.GetValue("") as string != RobloxPlaceKey)
                uriKey.SetValue("", RobloxPlaceKey);
        }

        public static void Unregister(string key)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{key}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("Protocol::Unregister", $"Failed to unregister {key}: {ex}");
            }
        }
    }
}
