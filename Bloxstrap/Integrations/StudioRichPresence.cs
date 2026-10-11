using DiscordRPC;

namespace Bloxstrap.Integrations
{
    public class StudioRichPresence : IDisposable
    {
        private const string PlaceOpenEntry  = "[FLog::PlaceManager] Start to open place";
        private const string PlaceCloseEntry = "[FLog::PlaceManager] PlaceManager::closeCurrentPlayDoc";

        private readonly DiscordRpcClient _rpcClient = new("1005469189907173486");

        private readonly DateTime _studioOpened = DateTime.UtcNow;

        private DateTime? _placeOpened;

        public StudioRichPresence(ActivityWatcher activityWatcher)
        {
            const string LOG_IDENT = "StudioRichPresence";

            activityWatcher.OnLogEntry += (_, entry) => ReadLogEntry(entry);

            _rpcClient.OnError += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, $"An RPC error occurred - {e.Message}");

            _rpcClient.Initialize();

            UpdatePresence();
        }

        private void ReadLogEntry(string entry)
        {
            int logMessageIdx = entry.IndexOf(' ');

            if (logMessageIdx == -1)
                return;

            string logMessage = entry[(logMessageIdx + 1)..];

            if (_placeOpened is null && logMessage.StartsWith(PlaceOpenEntry))
            {
                App.Logger.WriteLine("StudioRichPresence::ReadLogEntry", "Place opened");
                _placeOpened = DateTime.UtcNow;
                UpdatePresence();
            }
            else if (_placeOpened is not null && logMessage.StartsWith(PlaceCloseEntry))
            {
                App.Logger.WriteLine("StudioRichPresence::ReadLogEntry", "Place closed");
                _placeOpened = null;
                UpdatePresence();
            }
        }

        private void UpdatePresence()
        {
            bool editing = _placeOpened is not null;

            _rpcClient.SetPresence(new DiscordRPC.RichPresence
            {
                Details = "Roblox Studio",
                State = editing ? "Editing a place" : "Idle",
                StatusDisplay = StatusDisplayType.Details,
                Timestamps = new Timestamps { Start = editing ? _placeOpened : _studioOpened },
                Assets = new Assets
                {
                    LargeImageKey = "roblox",
                    LargeImageText = "Roblox Studio"
                }
            });
        }

        public void Dispose()
        {
            _rpcClient.ClearPresence();
            _rpcClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
