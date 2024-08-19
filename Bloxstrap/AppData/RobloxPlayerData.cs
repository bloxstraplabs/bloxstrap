using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.AppData
{
    public class RobloxPlayerData : CommonAppData, IAppData
    {
        public string ProductName { get; } = "Roblox";

        public string BinaryType { get; } = "WindowsPlayer";

        public string RegistryName { get; } = "RobloxPlayer";

        public string ExecutableName { get; } = "RobloxPlayerBeta.exe";

        public string StartEvent { get; } = "www.roblox.com/robloxStartedEvent";

        public override IReadOnlyDictionary<string, string> PackageDirectoryMap { get; set; } = new Dictionary<string, string>()
        {
            { "RobloxApp.zip", @"" }
        };
    }
}
