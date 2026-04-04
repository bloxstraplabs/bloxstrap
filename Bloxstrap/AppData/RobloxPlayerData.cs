using System.Windows;

namespace Bloxstrap.AppData
{
    public class RobloxPlayerData : CommonAppData, IAppData
    {
        public string ProductName => "Roblox";

        public string BinaryType => "WindowsPlayer";

        public string RegistryName => "RobloxPlayer";

        public string ProcessName => "RobloxPlayerBeta";

        public override string ExecutableName => "RobloxPlayerBeta.exe";

        public override JsonManager<DistributionState> DistributionStateManager => App.PlayerState;

        public override IReadOnlyDictionary<string, string>? PackageDirectoryMap { get; set; }

        public override async Task FetchPackageMap()
        {
            const string LOG_IDENT = "RobloxPlayerData::FetchPackageMap";

            try
            {
                PackageDirectoryMap = await Http.GetJson<Dictionary<string, string>>("https://raw.githubusercontent.com/bloxstraplabs/config/refs/heads/main/package-maps/playerdata.json");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not fetch package map!");
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(Strings.Dialog_Connectivity_BadConnection, MessageBoxImage.Error);
                return;
            }

            await base.FetchPackageMap();
        }
    }
}
