using System.Windows;

namespace Bloxstrap.AppData
{
    public class RobloxStudioData : CommonAppData, IAppData
    {
        public string ProductName => "Roblox Studio";

        public string BinaryType => "WindowsStudio64";

        public string RegistryName => "RobloxStudio";

        public string ProcessName => "RobloxStudioBeta";

        public override string ExecutableName => "RobloxStudioBeta.exe";

        public override JsonManager<DistributionState> DistributionStateManager => App.StudioState;

        public override IReadOnlyDictionary<string, string>? PackageDirectoryMap { get; set; }

        public override async Task FetchPackageMap()
        {
            const string LOG_IDENT = "RobloxStudioData::FetchPackageMap";

            try
            {
                PackageDirectoryMap = await Http.GetJson<Dictionary<string, string>>("https://raw.githubusercontent.com/bloxstraplabs/config/refs/heads/main/package-maps/studiodata.json");
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
