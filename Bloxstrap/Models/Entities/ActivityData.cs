using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows;
using System.Windows.Input;
using Bloxstrap.AppData;
using Bloxstrap.Models.APIs;
using Bloxstrap.Models.APIs.RoValra;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.Models.Entities
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

        public string JobId { get; set; } = string.Empty;

        /// <summary>
        /// This will be empty unless the server joined is a private server
        /// </summary>
        public string AccessCode { get; set; } = string.Empty;
        
        public long UserId { get; set; } = 0;

        public string MachineAddress { get; set; } = string.Empty;

        public bool MachineAddressValid => !string.IsNullOrEmpty(MachineAddress) && !MachineAddress.StartsWith("10.");

        public bool IsTeleport { get; set; } = false;

        public ServerType ServerType { get; set; } = ServerType.Public;

        public DateTime TimeJoined { get; set; }

        public DateTime? TimeLeft { get; set; }

        // everything below here is optional strictly for bloxstraprpc, discord rich presence, or game history

        /// <summary>
        /// This is intended only for other people to use, i.e. context menu invite link, rich presence joining
        /// </summary>
        public string RPCLaunchData { get; set; } = string.Empty;

        public UniverseDetails? UniverseDetails { get; set; }

        public string GameHistoryDescription
        {
            get
            {
                string desc = string.Format(
                    "{0} • {1} {2} {3}", 
                    UniverseDetails?.Data.Creator.Name,
                    TimeJoined.ToString("t"), 
                    Locale.CurrentCulture.Name.StartsWith("ja") ? '~' : '-',
                    TimeLeft?.ToString("t")
                );

                if (ServerType != ServerType.Public)
                    desc += " • " + ServerType.ToTranslatedString();

                return desc;
            }
        }

        public ICommand RejoinServerCommand => new RelayCommand(RejoinServer);

        private SemaphoreSlim serverQuerySemaphore = new(1, 1);
        private SemaphoreSlim serverTimeSemaphore = new(1, 1);

        public string GetInviteDeeplink(bool launchData = true)
        {
            string deeplink = $"https://www.fishstrap.app/api/joingame?placeId={PlaceId}";

            if (ServerType == ServerType.Private) // thats not going to work
                deeplink += "&accessCode=" + AccessCode;
            else
                deeplink += "&gameInstanceId=" + JobId;

            if (launchData && !string.IsNullOrEmpty(RPCLaunchData))
                deeplink += "&launchData=" + HttpUtility.UrlEncode(RPCLaunchData);

            return deeplink;
        }

        public async Task<DateTime?> QueryServerTime()
        {
            const string LOG_IDENT = "ActivityData::QueryServerTime";

            if (string.IsNullOrEmpty(JobId))
                throw new InvalidOperationException("JobId is null");

            if (PlaceId == 0)
                throw new InvalidOperationException("PlaceId is null");

            await serverTimeSemaphore.WaitAsync();

            if (GlobalCache.ServerTime.TryGetValue(JobId, out DateTime? time))
            {
                serverTimeSemaphore.Release();
                return time;
            }

            DateTime? firstSeen = DateTime.UtcNow;
            try
            {
                var serverTimeRaw = await Http.GetJson<RoValraTimeResponse>($"https://apis.rovalra.com/v1/server_details?place_id={PlaceId}&server_ids={JobId}");

                var serverBody = new RoValraProcessServerBody
                {
                    PlaceId = PlaceId,
                    ServerIds = new() { JobId }
                };

                string json = JsonSerializer.Serialize(serverBody);
                HttpContent postContent = new StringContent(json, Encoding.UTF8, "application/json");

                // we dont need to await it since its not as important
                // we want to return uptime quickly
                _ = App.HttpClient.PostAsync("https://apis.rovalra.com/process_servers", postContent);


                RoValraServer? server = null;

                if (serverTimeRaw?.Servers != null && serverTimeRaw.Servers.Count > 0)
                    server = serverTimeRaw.Servers[0];

                // if the server hasnt been registered we will simply return UtcNow
                // since firstSeen is UtcNow by default we dont have to check anything else
                if (server?.FirstSeen != null)
                    firstSeen = server.FirstSeen;

                GlobalCache.ServerTime[JobId] = firstSeen;
                serverTimeSemaphore.Release();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to get server time for {PlaceId}/{JobId}");
                App.Logger.WriteException(LOG_IDENT, ex);

                GlobalCache.ServerTime[JobId] = firstSeen;
                serverTimeSemaphore.Release();

                Frontend.ShowConnectivityDialog(
                    string.Format(Strings.Dialog_Connectivity_UnableToConnect, "rovalra.com"),
                    Strings.ActivityWatcher_LocationQueryFailed,
                    MessageBoxImage.Warning,
                    ex
                );
            }

            return firstSeen;
        }

        public async Task<string?> QueryServerLocation()
        {
            const string LOG_IDENT = "ActivityData::QueryServerLocation";

            if (!MachineAddressValid)
                throw new InvalidOperationException($"Machine address is invalid ({MachineAddress})");

            await serverQuerySemaphore.WaitAsync();

            if (GlobalCache.ServerLocation.TryGetValue(MachineAddress, out string? location))
            {
                serverQuerySemaphore.Release();
                return location;
            }

            try
            {
                var ipInfo = await Http.GetJson<IPInfoResponse>($"https://ipinfo.io/{MachineAddress}/json");

                if (string.IsNullOrEmpty(ipInfo.City))
                    throw new InvalidHTTPResponseException("Reported city was blank");

                if (ipInfo.City == ipInfo.Region)
                    location = $"{ipInfo.Region}, {ipInfo.Country}";
                else
                    location = $"{ipInfo.City}, {ipInfo.Region}, {ipInfo.Country}";

                GlobalCache.ServerLocation[MachineAddress] = location;
                serverQuerySemaphore.Release();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to get server location for {MachineAddress}");
                App.Logger.WriteException(LOG_IDENT, ex);

                GlobalCache.ServerLocation[MachineAddress] = location;
                serverQuerySemaphore.Release();

                Frontend.ShowConnectivityDialog(
                    string.Format(Strings.Dialog_Connectivity_UnableToConnect, "ipinfo.io"),
                    Strings.ActivityWatcher_LocationQueryFailed,
                    MessageBoxImage.Warning,
                    ex
                );
            }

            return location;
        }

        public override string ToString() => $"{PlaceId}/{JobId}";

        private void RejoinServer()
        {
            string playerPath = new RobloxPlayerData().ExecutablePath;

            Process.Start(playerPath, GetInviteDeeplink(false));
        }
    }
}
