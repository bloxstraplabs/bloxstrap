namespace Bloxstrap.Integrations
{
    public class ActivityWatcher : IDisposable
    {
        private const string GameMessageEntry                = "[FLog::CreatorOutput] [BloxstrapRPC]";
        private const string GameJoiningEntry                = "[FLog::Output] ! Joining game";

        private const string GameTeleportingEntry            = "doTeleport";
        private const string GameTeleportJoinUtilEntry        = "initiateTeleport";
        private const string GameJoiningUniverseEntry        = "[FLog::GameJoinLoadTime] Report game_join_loadtime:";
        private const string GameJoiningUDMUXEntry           = "[FLog::Network] UDMUX Address = ";
        private const string GameJoinedEntry                 = "[FLog::Network] Replicator created: ";
        private const string GameDisconnectedEntry           = "[FLog::Network] Time to disconnect replication data:";
        private const string GameLeavingEntry                = "[FLog::SingleSurfaceApp] leaveUGCGameInternal";

        private const string GameJoiningEntryPattern         = @"! Joining game '([0-9a-f\-]{36})' place ([0-9]+) at ([0-9\.]+)";
        private const string GameJoinReferralPattern         = @"referral_page:([^,]+)";
        private const string GameTeleportJoinTypePattern     = @"JoinTypeId""%3a(\d+)%2c";
        private const string GameJoiningUniversePattern      = @"universeid:([0-9]+).*userid:([0-9]+)";
        private const string GameJoiningUDMUXPattern         = @"UDMUX Address = ([0-9\.]+), Port = [0-9]+ \| RCC Server Address = ([0-9\.]+), Port = [0-9]+";
        private const string GameJoinedEntryPattern          = @"serverId: ([0-9\.]+)\|[0-9]+";
        private const string GameMessageEntryPattern         = @"\[BloxstrapRPC\] (.*)";

        private int _logEntriesRead = 0;
        private bool _teleportMarker = false;
        private bool _reservedTeleportMarker = false;
        
        public event EventHandler<string>? OnLogEntry;
        public event EventHandler? OnGameJoin;
        public event EventHandler? OnGameLeave;
        public event EventHandler? OnLogOpen;
        public event EventHandler? OnAppClose;
        public event EventHandler<Message>? OnRPCMessage;

        private DateTime LastRPCRequest;

        public string LogLocation = null!;

        public bool InGame = false;
        public bool InTeleport = false;
        
        public ActivityData Data { get; private set; } = new();

        /// <summary>
        /// Ordered by newest to oldest
        /// </summary>
        public List<ActivityData> History = new();

        public bool IsDisposed = false;

        public ActivityWatcher(string? logFile = null)
        {
            if (!string.IsNullOrEmpty(logFile))
                LogLocation = logFile;
        }

        public async void Start()
        {
            FileInfo logFileInfo;

            if (string.IsNullOrEmpty(LogLocation))
            {
                string logDirectory = Path.Combine(Paths.LocalAppData, "Roblox\\logs");
                if (!Directory.Exists(logDirectory))
                    return;

                logFileInfo = await FindLatestLogFileAsync(logDirectory);
                LogLocation = logFileInfo.FullName;
            }
            else
            {
                logFileInfo = new FileInfo(LogLocation);
            }

            OnLogOpen?.Invoke(this, EventArgs.Empty);
            await TailLogStreamAsync(logFileInfo);
        }

        private async Task<FileInfo> FindLatestLogFileAsync(string logDirectory)
        {
            const string LOG_IDENT = "ActivityWatcher::FindLatestLogFile";
            App.Logger.WriteLine(LOG_IDENT, "Opening Roblox log file...");

            while (true)
            {
                var logFileInfo = new DirectoryInfo(logDirectory)
                    .GetFiles()
                    .Where(x => x.Name.Contains("Player", StringComparison.OrdinalIgnoreCase) && x.CreationTime <= DateTime.Now)
                    .OrderByDescending(x => x.CreationTime)
                    .FirstOrDefault();

                if (logFileInfo != null && logFileInfo.CreationTime.AddSeconds(15) > DateTime.Now)
                    return logFileInfo;

                string newestName = logFileInfo?.Name ?? "none";
                App.Logger.WriteLine(LOG_IDENT, $"Could not find recent enough log file, waiting... (newest is {newestName})");
                await Task.Delay(1000);
            }
        }

        private async Task TailLogStreamAsync(FileInfo logFileInfo)
        {
            const string LOG_IDENT = "ActivityWatcher::TailLogStream";
            App.Logger.WriteLine(LOG_IDENT, $"Opened {LogLocation}");

            var logFileStream = logFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(logFileStream);

            while (!IsDisposed)
            {
                string? log = await streamReader.ReadLineAsync();
                if (log is null)
                    await Task.Delay(1000);
                else
                    ReadLogEntry(log);
            }
        }

        private void ReadLogEntry(string entry)
        {
            OnLogEntry?.Invoke(this, entry);
            _logEntriesRead++;

            int logMessageIdx = entry.IndexOf(' ');
            if (logMessageIdx == -1)
                return;

            string logMessage = entry[(logMessageIdx + 1)..];

            if (logMessage.StartsWith(GameLeavingEntry))
            {
                HandleAppLeaving();
            }
            else if (!InGame && Data.PlaceId == 0)
            {
                if (logMessage.StartsWith(GameJoiningEntry))
                    HandleGameJoining(logMessage);
            }
            else if (!InGame && Data.PlaceId != 0)
            {
                if (logMessage.StartsWith(GameJoiningUniverseEntry))
                    HandleGameJoiningUniverse(logMessage);
                else if (logMessage.StartsWith(GameJoiningUDMUXEntry))
                    HandleGameJoiningUDMUX(logMessage);
                else if (logMessage.StartsWith(GameJoinedEntry))
                    HandleGameJoined();
            }
            else if (InGame && Data.PlaceId != 0)
            {
                if (logMessage.StartsWith(GameDisconnectedEntry))
                    HandleGameDisconnected();
                else if (logMessage.Contains(GameTeleportingEntry) || logMessage.Contains(GameTeleportJoinUtilEntry))
                    HandleGameTeleporting(logMessage);
                else if (logMessage.StartsWith(GameMessageEntry))
                    HandleRPCMessage(logMessage);
            }
        }

        private void HandleAppLeaving()
        {
            const string LOG_IDENT = "ActivityWatcher::HandleAppLeaving";
            App.Logger.WriteLine(LOG_IDENT, "User is back into the desktop app");

            bool wasInGameOrTeleport = InGame || InTeleport;
            InGame = false;
            InTeleport = false;
            _teleportMarker = false;
            _reservedTeleportMarker = false;

            OnAppClose?.Invoke(this, EventArgs.Empty);

            if (wasInGameOrTeleport)
            {
                Data.TimeLeft = DateTime.Now;
                if (Data.PlaceId != 0)
                    History.Insert(0, Data);

                Data = new();
                OnGameLeave?.Invoke(this, EventArgs.Empty);
            }
            else if (Data.PlaceId != 0)
            {
                App.Logger.WriteLine(LOG_IDENT, "User appears to be leaving from a cancelled/errored join");
                Data = new();
            }
        }

        private void HandleGameJoining(string logMessage)
        {
            const string LOG_IDENT = "ActivityWatcher::HandleGameJoining";
            Match match = Regex.Match(logMessage, GameJoiningEntryPattern);

            if (match.Groups.Count != 4)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to assert format for game join entry");
                App.Logger.WriteLine(LOG_IDENT, logMessage);
                return;
            }

            InGame = false;
            Data.PlaceId = long.Parse(match.Groups[2].Value);
            Data.JobId = match.Groups[1].Value;
            Data.MachineAddress = match.Groups[3].Value;

            if (App.Settings.Prop.ShowServerDetails && Data.MachineAddressValid)
                _ = Data.QueryServerLocation();

            if (_teleportMarker || InTeleport)
            {
                Data.IsTeleport = true;
                _teleportMarker = false;
            }

            if (_reservedTeleportMarker)
            {
                Data.ServerType = ServerType.Reserved;
                _reservedTeleportMarker = false;
            }

            App.Logger.WriteLine(LOG_IDENT, $"Joining Game ({Data})");
        }

        private void HandleGameJoiningUniverse(string logMessage)
        {
            const string LOG_IDENT = "ActivityWatcher::HandleGameJoiningUniverse";
            var match = Regex.Match(logMessage, GameJoiningUniversePattern);

            if (match.Groups.Count != 3)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to assert format for game join universe entry");
                App.Logger.WriteLine(LOG_IDENT, logMessage);
                return;
            }

            Data.UniverseId = long.Parse(match.Groups[1].Value);
            Data.UserId = long.Parse(match.Groups[2].Value);

            var loadTimeMatch = Regex.Match(logMessage, GameJoinReferralPattern);
            if (loadTimeMatch.Groups.Count == 2)
            {
                string referral = loadTimeMatch.Groups[1].Value;
                if (referral.Contains("RequestPrivateGame", StringComparison.OrdinalIgnoreCase) ||
                    referral.Contains("GameDetailPageJSHybridEvent", StringComparison.OrdinalIgnoreCase))
                {
                    Data.ServerType = ServerType.Private;
                }
            }

            if (History.Any() && Data.IsTeleport)
            {
                var lastActivity = History.First();
                Data.RootActivity = lastActivity.RootActivity ?? lastActivity;
            }

            if (Data.RootActivity?.UniverseDetails is not null && Data.UniverseDetails is null)
                Data.UniverseDetails = Data.RootActivity.UniverseDetails;
        }

        private void HandleGameJoiningUDMUX(string logMessage)
        {
            const string LOG_IDENT = "ActivityWatcher::HandleGameJoiningUDMUX";
            var match = Regex.Match(logMessage, GameJoiningUDMUXPattern);

            if (match.Groups.Count != 3 || match.Groups[2].Value != Data.MachineAddress)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to assert format for game join UDMUX entry");
                App.Logger.WriteLine(LOG_IDENT, logMessage);
                return;
            }

            Data.MachineAddress = match.Groups[1].Value;

            if (App.Settings.Prop.ShowServerDetails)
                _ = Data.QueryServerLocation();

            App.Logger.WriteLine(LOG_IDENT, $"Server is UDMUX protected ({Data})");
        }

        private void HandleGameJoined()
        {
            const string LOG_IDENT = "ActivityWatcher::HandleGameJoined";
            App.Logger.WriteLine(LOG_IDENT, $"Joined Game ({Data})");

            InGame = true;
            InTeleport = false;
            _teleportMarker = false;
            Data.TimeJoined = DateTime.Now;

            OnGameJoin?.Invoke(this, EventArgs.Empty);
        }

        private void HandleGameDisconnected()
        {
            const string LOG_IDENT = "ActivityWatcher::HandleGameDisconnected";
            App.Logger.WriteLine(LOG_IDENT, $"Disconnected from replication ({Data})");

            Data.TimeLeft = DateTime.Now;
            History.Insert(0, Data);

            var rootActivity = Data.RootActivity ?? Data;
            var universeDetails = Data.UniverseDetails ?? rootActivity.UniverseDetails;
            long universeId = Data.UniverseId != 0 ? Data.UniverseId : rootActivity.UniverseId;

            InGame = false;
            InTeleport = true;

            Data = new ActivityData
            {
                UniverseId = universeId,
                UniverseDetails = universeDetails,
                RootActivity = rootActivity,
                IsTeleport = true
            };
        }

        private void HandleGameTeleporting(string logMessage)
        {
            const string LOG_IDENT = "ActivityWatcher::HandleGameTeleporting";
            App.Logger.WriteLine(LOG_IDENT, $"Initiating teleport to server ({Data})");

            _teleportMarker = true;
            InTeleport = true;

            var joinTypeMatch = Regex.Match(logMessage, GameTeleportJoinTypePattern);
            if (!joinTypeMatch.Success || !int.TryParse(joinTypeMatch.Groups[1].Value, out int joinTypeId))
                return;

            var joinType = (ServerSessionJoinType)joinTypeId;
            App.Logger.WriteLine(LOG_IDENT, $"Teleport JoinTypeId: {joinTypeId}");

            if (joinType is ServerSessionJoinType.NewGamePrivateGame or ServerSessionJoinType.SpecificPrivateGame)
            {
                _reservedTeleportMarker = true;
                App.Logger.WriteLine(LOG_IDENT, "Detected reserved server teleport");
            }
        }

        private void HandleSetLaunchData(Message message)
        {
            const string LOG_IDENT = "ActivityWatcher::HandleSetLaunchData";
            string? data;

            try
            {
                data = message.Data.Deserialize<string>();
            }
            catch (Exception)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to parse launch data");
                return;
            }

            if (data is null || data.Length > 200)
            {
                App.Logger.WriteLine(LOG_IDENT, "Launch data is null or exceeds 200 characters");
                return;
            }

            Data.RPCLaunchData = data;
        }

        private void HandleRPCMessage(string logMessage)
        {
            const string LOG_IDENT = "ActivityWatcher::HandleRPCMessage";
            var match = Regex.Match(logMessage, GameMessageEntryPattern);

            if (match.Groups.Count != 2)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to assert format for RPC message entry");
                App.Logger.WriteLine(LOG_IDENT, logMessage);
                return;
            }

            if ((DateTime.Now - LastRPCRequest).TotalSeconds <= 1)
            {
                App.Logger.WriteLine(LOG_IDENT, "Dropping message as ratelimit has been hit");
                return;
            }

            Message? message;
            try
            {
                message = JsonSerializer.Deserialize<Message>(match.Groups[1].Value);
            }
            catch (Exception)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to parse message JSON");
                return;
            }

            if (message is null || string.IsNullOrEmpty(message.Command))
                return;

            if (message.Command == "SetLaunchData")
                HandleSetLaunchData(message);

            OnRPCMessage?.Invoke(this, message);
            LastRPCRequest = DateTime.Now;
        }

        public void Dispose()
        {
            IsDisposed = true;
            if (InGame || InTeleport)
            {
                InGame = false;
                InTeleport = false;
                OnGameLeave?.Invoke(this, EventArgs.Empty);
            }
            GC.SuppressFinalize(this);
        }
    }
}
