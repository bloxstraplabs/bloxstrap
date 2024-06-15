using System.Web;
using System.Windows;

using Microsoft.Win32;

namespace Bloxstrap
{
    static class ProtocolHandler
    {
        private const string RobloxPlaceKey = "Roblox.Place";

        public static string ParseUri(string protocol)
        {
            var args = new Dictionary<string, string?>();
            bool channelArgPresent = false;

            foreach (var parameter in protocol.Split('+'))
            {
                if (!parameter.Contains(':'))
                {
                    args[parameter] = null;
                    continue;
                }

                var kv = parameter.Split(':');
                string key = kv[0];
                string val = kv[1];

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

                args.Add(key, val);
            }

            if (!channelArgPresent)
                EnrollChannel(RobloxDeployment.DefaultChannel);

            var pairs = args.Select(x => x.Value != null ? x.Key + ":" + x.Value : x.Key).ToArray();
            return String.Join("+", pairs);
        }

        public static void ChangeChannel(string channel)
        {
            if (channel.ToLowerInvariant() == App.Settings.Prop.Channel.ToLowerInvariant())
                return;

            // don't change if roblox is already running
            if (Process.GetProcessesByName("RobloxPlayerBeta").Any())
            {
                App.Logger.WriteLine("ProtocolHandler::ChangeChannel", $"Ignored channel change from {App.Settings.Prop.Channel} to {channel} because Roblox is already running");
            }
            else
            {
                App.Logger.WriteLine("ProtocolHandler::ChangeChannel", $"Changed Roblox channel from {App.Settings.Prop.Channel} to {channel}");
                App.Settings.Prop.Channel = channel;
            }
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
