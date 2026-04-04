using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bloxstrap.AppData
{
    public abstract class CommonAppData
    {
        // in case a new package is added, you can find the corresponding directory
        // by opening the stock bootstrapper in a hex editor
        private IReadOnlyDictionary<string, string>? _commonMap;

        public virtual string ExecutableName { get; } = null!;

        public string Directory => Path.Combine(Paths.Versions, DistributionState.VersionGuid);

        public string ExecutablePath => Path.Combine(Directory, ExecutableName);

        public virtual JsonManager<DistributionState> DistributionStateManager { get; } = null!;

        public DistributionState DistributionState => DistributionStateManager.Prop;

        public List<string> ModManifest => DistributionState.ModManifest;

        public virtual IReadOnlyDictionary<string, string>? PackageDirectoryMap { get; set; }

        public virtual async Task FetchPackageMap()
        {
            const string LOG_IDENT = "CommonAppData::FetchPackageMap";

            try
            {
                _commonMap = await Http.GetJson<Dictionary<string, string>>("https://raw.githubusercontent.com/bloxstraplabs/config/refs/heads/main/package-maps/commonappdata.json");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not fetch package map!");
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(Strings.Dialog_Connectivity_BadConnection, MessageBoxImage.Error);
                return;
            }

            if (PackageDirectoryMap is null)
            {
                PackageDirectoryMap = _commonMap;
                return;
            }

            var merged = new Dictionary<string, string>();

            foreach (var entry in _commonMap)
                merged[entry.Key] = entry.Value;

            foreach (var entry in PackageDirectoryMap)
                merged[entry.Key] = entry.Value;

            PackageDirectoryMap = merged;
        }
    }
}
