using System.Text.RegularExpressions;

using DiscordRPC;

namespace Bloxstrap.Integrations
{
    public class StudioRichPresence : IDisposable
    {
        private const string PlaceOpenEntry  = "[FLog::PlaceManager] Start to open place";
        private const string PlaceCloseEntry = "[FLog::PlaceManager] PlaceManager::closeCurrentPlayDoc";
        private const string TestStartEntry  = "[FLog::Network] NetworkServer:Start";
        private const string TestEndEntry    = "[FLog::StudioKeyEvents] end play test";

        private static readonly Regex PlaceIdPattern = new(@"open place \(identifier = (\d+)\)", RegexOptions.Compiled);
        private static readonly Regex UniverseIdPattern = new(@"universeId[ =](\d+)", RegexOptions.Compiled);

        private readonly DiscordRpcClient _rpcClient = new("1005469189907173486");

        private readonly DateTime _studioOpened = DateTime.UtcNow;

        private DateTime? _placeOpened;

        private DateTime? _testStarted;

        private long _placeId;

        private long _universeId;

        private UniverseDetails? _universeDetails;

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

            if (_placeOpened is null && logMessage.StartsWith(PlaceOpenEntry, StringComparison.Ordinal))
            {
                App.Logger.WriteLine("StudioRichPresence::ReadLogEntry", "Place opened");
                _placeOpened = DateTime.UtcNow;
                UpdatePresence();
            }
            else if (_placeOpened is not null && logMessage.StartsWith(PlaceCloseEntry, StringComparison.Ordinal))
            {
                App.Logger.WriteLine("StudioRichPresence::ReadLogEntry", "Place closed");
                _placeOpened = null;
                _testStarted = null;
                _placeId = 0;
                _universeId = 0;
                _universeDetails = null;
                UpdatePresence();
            }
            else if (_placeOpened is not null && _placeId == 0 && PlaceIdPattern.Match(logMessage) is { Success: true } placeMatch)
            {
                _placeId = long.Parse(placeMatch.Groups[1].Value);
            }
            else if (_placeOpened is not null && _universeId == 0 && UniverseIdPattern.Match(logMessage) is { Success: true } universeMatch)
            {
                _universeId = long.Parse(universeMatch.Groups[1].Value);
                Task.Run(LoadUniverseDetails);
            }
            else if (_placeOpened is not null && _testStarted is null && logMessage.StartsWith(TestStartEntry, StringComparison.Ordinal))
            {
                _testStarted = DateTime.UtcNow;
                UpdatePresence();
            }
            else if (_testStarted is not null && logMessage.StartsWith(TestEndEntry, StringComparison.Ordinal))
            {
                _testStarted = null;
                UpdatePresence();
            }
        }

        private async Task LoadUniverseDetails()
        {
            long universeId = _universeId;

            try
            {
                await UniverseDetails.FetchSingle(universeId);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("StudioRichPresence::LoadUniverseDetails", ex);
                return;
            }

            if (universeId != _universeId)
                return;

            _universeDetails = UniverseDetails.LoadFromCache(universeId);
            UpdatePresence();
        }

        private void UpdatePresence()
        {
            var settings = App.Settings.Prop;

            bool editing = _placeOpened is not null;
            bool testing = editing && _testStarted is not null && settings.StudioShowTesting;

            string? gameName = settings.StudioShowGameName ? _universeDetails?.Data.Name : null;

            string state;

            if (!editing)
                state = "Idle";
            else if (testing)
                state = gameName is null ? "Testing a place" : $"Testing {gameName}";
            else
                state = gameName is null ? "Editing a place" : $"Editing {gameName}";

            if (state.Length > 128)
                state = state[..128];

            string? icon = settings.StudioShowGameIcon ? _universeDetails?.Thumbnail.ImageUrl : null;

            var buttons = new List<Button>();

            if (settings.StudioShowGameButton && editing && _placeId != 0 && _universeDetails is not null)
                buttons.Add(new Button { Label = "See game page", Url = $"https://www.roblox.com/games/{_placeId}" });

            _rpcClient.SetPresence(new DiscordRPC.RichPresence
            {
                Details = "Roblox Studio",
                State = state,
                StatusDisplay = StatusDisplayType.Details,
                Timestamps = new Timestamps { Start = testing ? _testStarted : editing ? _placeOpened : _studioOpened },
                Buttons = buttons.ToArray(),
                Assets = new Assets
                {
                    LargeImageKey = icon ?? "roblox",
                    LargeImageText = gameName ?? "Roblox Studio",
                    SmallImageKey = icon is null ? null : "roblox",
                    SmallImageText = icon is null ? null : "Roblox Studio"
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
