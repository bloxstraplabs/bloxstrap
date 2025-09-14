using Bloxstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.AppData
{
    public class RobloxPlayerData : CommonAppData, IAppData
    {
        public string ProductName => "Roblox";

        public string BinaryType => "WindowsPlayer";

        public string RegistryName => "RobloxPlayer";
        public override string ExecutableName => App.RobloxPlayerAppName;
        public override AppState State => App.RobloxState.Prop.Player;
    }
}
