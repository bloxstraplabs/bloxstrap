using System.Web;
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

        public string UserId { get; set; } = String.Empty;

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

        private void RejoinServer()
        {
            string playerPath = Path.Combine(Paths.Versions, App.State.Prop.PlayerVersionGuid, "RobloxPlayerBeta.exe");
            
            Process.Start(playerPath, GetInviteDeeplink(false));
        }
    }
}
