using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Bloxstrap.Models
{
    public class ActivityHistoryEntry
    {
        public long UniverseId { get; set; }

        public long PlaceId { get; set; }

        public string JobId { get; set; } = String.Empty;

        public DateTime TimeJoined { get; set; }

        public DateTime TimeLeft { get; set; }

        public string TimeJoinedFriendly => String.Format("{0} - {1}", TimeJoined.ToString("h:mm tt"), TimeLeft.ToString("h:mm tt"));

        public bool DetailsLoaded = false;

        public string GameName { get; set; } = String.Empty;

        public string GameThumbnail { get; set; } = String.Empty;

        public ICommand RejoinServerCommand => new RelayCommand(RejoinServer);

        private void RejoinServer()
        {
            string playerPath = Path.Combine(Paths.Versions, App.State.Prop.PlayerVersionGuid, "RobloxPlayerBeta.exe");
            string deeplink = $"roblox://experiences/start?placeId={PlaceId}&gameInstanceId={JobId}";

            // start RobloxPlayerBeta.exe directly since Roblox can reuse the existing window
            // ideally, i'd like to find out how roblox is doing it
            Process.Start(playerPath, deeplink);
        }
    }
}
