using System.Web;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.Models
{
    public class ActivityData
    {
        private long _universeId = 0;

        /// <summary>
        /// If the current activity stems from an in-universe teleport, then this will be
        /// set to the activity that corresponds to the initial game join
        /// </summary>
        public ActivityData? RootActivity;

        public long UniverseId 
        {
            get => _universeId;
            set
            {
                _universeId = value;
                UniverseDetails.LoadFromCache(value);
            }
        }

        public long PlaceId { get; set; } = 0;

        public string JobId { get; set; } = String.Empty;

        /// <summary>
        /// This will be empty unless the server joined is a private server
        /// </summary>
        public string AccessCode { get; set; } = String.Empty;
        
        public string MachineAddress { get; set; } = String.Empty;

        public bool MachineAddressValid => !String.IsNullOrEmpty(MachineAddress) && !MachineAddress.StartsWith("10.");

        public bool IsTeleport { get; set; } = false;
        
        public ServerType ServerType { get; set; } = ServerType.Public;

        public DateTime TimeJoined { get; set; }

        public DateTime? TimeLeft { get; set; }

        // everything below here is optional strictly for bloxstraprpc, discord rich presence, or game history

        /// <summary>
        /// This is intended only for other people to use, i.e. context menu invite link, rich presence joining
        /// </summary>
        public string RPCLaunchData { get; set; } = String.Empty;

        public UniverseDetails? UniverseDetails { get; set; }
        
        public string GameHistoryDescription
        {
            get
            {
                string desc = String.Format("{0} • {1} - {2}", UniverseDetails?.Data.Creator.Name, TimeJoined.ToString("h:mm tt"), TimeLeft?.ToString("h:mm tt"));

                if (ServerType != ServerType.Public)
                    desc += " • " + ServerType.ToTranslatedString();

                return desc;
            }
        }

        public ICommand RejoinServerCommand => new RelayCommand(RejoinServer);

        public string GetInviteDeeplink(bool launchData = true)
        {
            string deeplink = $"roblox://experiences/start?placeId={PlaceId}";

            if (ServerType == ServerType.Private)
                deeplink += "&accessCode=" + AccessCode;
            else
                deeplink += "&gameInstanceId=" + JobId;

            if (launchData && !String.IsNullOrEmpty(RPCLaunchData))
                deeplink += "&launchData=" + HttpUtility.UrlEncode(RPCLaunchData);

            return deeplink;
        }

        public async Task<string> QueryServerLocation()
        {
            const string LOG_IDENT = "ActivityData::QueryServerLocation";

            if (!MachineAddressValid)
                throw new InvalidOperationException($"Machine address is invalid ({MachineAddress})");

            if (GlobalCache.PendingTasks.TryGetValue(MachineAddress, out Task? task))
                await task;

            if (GlobalCache.ServerLocation.TryGetValue(MachineAddress, out string? location))
                return location;

            try
            {
                location = "";
                var ipInfoTask = Http.GetJson<IPInfoResponse>($"https://ipinfo.io/{MachineAddress}/json");

                GlobalCache.PendingTasks.Add(MachineAddress, ipInfoTask);

                var ipInfo = await ipInfoTask;

                GlobalCache.PendingTasks.Remove(MachineAddress);

                if (String.IsNullOrEmpty(ipInfo.City))
                    throw new InvalidHTTPResponseException("Reported city was blank");

                if (ipInfo.City == ipInfo.Region)
                    location = $"{ipInfo.Region}, {ipInfo.Country}";
                else
                    location = $"{ipInfo.City}, {ipInfo.Region}, {ipInfo.Country}";

                GlobalCache.ServerLocation[MachineAddress] = location;

                return location;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to get server location for {MachineAddress}");
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox($"{Strings.ActivityWatcher_LocationQueryFailed}\n\n{ex.Message}", MessageBoxImage.Warning);

                return "?";
            }
        }

        public override string ToString() => $"{PlaceId}/{JobId}";

        private void RejoinServer()
        {
            string playerPath = Path.Combine(Paths.Roblox, "Player", "RobloxPlayerBeta.exe");
            
            Process.Start(playerPath, GetInviteDeeplink(false));
        }
    }
}
