using Microsoft.Win32;

using Bloxstrap.Models.SettingTasks.Base;
using Bloxstrap.RobloxInterfaces;

namespace Bloxstrap.Models.SettingTasks
{
    public class ChannelTask : StringBaseTask
    {
        private readonly string _registryName;

        private string KeyPath => $"SOFTWARE\\ROBLOX Corporation\\Environments\\{_registryName}\\Channel";

        public ChannelTask(string registryName) : base("Channel", registryName)
        {
            _registryName = registryName;

            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);

            string? channel = key?.GetValue("www.roblox.com") as string;
            OriginalState = String.IsNullOrEmpty(channel) ? Deployment.DefaultChannel : channel;
        }

        public override void Execute()
        {
            string channel = NewState.Trim().ToLowerInvariant();

            if (String.IsNullOrEmpty(channel))
                channel = Deployment.DefaultChannel;

            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValueSafe("www.roblox.com", channel == Deployment.DefaultChannel ? "" : channel);

            OriginalState = channel;
        }
    }
}
