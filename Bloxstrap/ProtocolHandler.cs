using System.Web;
using System.Windows;

using Microsoft.Win32;

namespace Bloxstrap
{
    static class ProtocolHandler
    {
        public static string ParseUri(string protocol)
        {
            var args = new Dictionary<string, string>();
            bool channelArgPresent = false;

            foreach (var parameter in protocol.Split('+'))
            {
                if (!parameter.Contains(':'))
                    continue;

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

            var pairs = args.Select(x => x.Key + ":" + x.Value).ToArray();
            return String.Join("+", pairs);
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
                    $"Roblox is attempting to set your channel to {channel}, however your current preferred channel is {App.Settings.Prop.Channel}.\n\n" +
                    $"Would you like to switch your preferred channel to {channel}?",
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
                App.Logger.WriteLine("Protocol::Unregister", $"Failed to unregister {key}: {ex}");
            }
        }
    }
}
