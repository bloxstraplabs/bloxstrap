using System.Web;
using System.Windows;

using Microsoft.Win32;

namespace Bloxstrap
{
    static class ProtocolHandler
    {
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
            { "channel", "-channel " }
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
                    ChangeChannel(val);

                    // we'll set the arg when launching
                    continue;
                }

                commandLine.Append(UriKeyArgMap[key] + val + " ");
            }

            if (!channelArgPresent)
                ChangeChannel(RobloxDeployment.DefaultChannel);

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
                MessageBoxResult result = Controls.ShowMessageBox(
                    $"Roblox is attempting to set your channel to {channel}, however your current preferred channel is {App.Settings.Prop.Channel}.\n\n" +
                    $"Would you like to switch channels from {App.Settings.Prop.Channel} to {channel}?",
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo
                );

                if (result != MessageBoxResult.Yes)
                    return;
            }

            App.Logger.WriteLine($"[Protocol::ParseUri] Changed Roblox build channel from {App.Settings.Prop.Channel} to {channel}");
            App.Settings.Prop.Channel = channel;
        }

        public static void Register(string key, string name, string handler)
        {
            string handlerArgs = $"\"{handler}\" %1";
            RegistryKey uriKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{key}");
            RegistryKey uriIconKey = uriKey.CreateSubKey("DefaultIcon");
            RegistryKey uriCommandKey = uriKey.CreateSubKey(@"shell\open\command");

            if (uriKey.GetValue("") is null)
            {
                uriKey.SetValue("", $"URL: {name} Protocol");
                uriKey.SetValue("URL Protocol", "");
            }

            if ((string?)uriCommandKey.GetValue("") != handlerArgs)
            {
                uriIconKey.SetValue("", handler);
                uriCommandKey.SetValue("", handlerArgs);
            }

            uriKey.Close();
            uriIconKey.Close();
            uriCommandKey.Close();
        }

        public static void Unregister(string key)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{key}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine($"[Protocol::Unregister] Failed to unregister {key}: {ex}");
            }
        }
    }
}
