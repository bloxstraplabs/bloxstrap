using System.Web;

namespace Bloxstrap.Integrations
{
    public class ActivityWatcher : IDisposable
    {
        // i'm thinking the functionality for parsing roblox logs could be broadened for more features than just rich presence,
        // like checking the ping and region of the current connected server. maybe that's something to add?
        private const string GameJoiningEntry = "[FLog::Output] ! Joining game";
        private const string GameJoiningPrivateServerEntry = "[FLog::GameJoinUtil] GameJoinUtil::joinGamePostPrivateServer";
        private const string GameJoiningReservedServerEntry = "[FLog::GameJoinUtil] GameJoinUtil::initiateTeleportToReservedServer";
        private const string GameJoiningUniverseEntry = "[FLog::GameJoinLoadTime] Report game_join_loadtime:";
        private const string GameJoiningUDMUXEntry = "[FLog::Network] UDMUX Address = ";
        private const string GameJoinedEntry = "[FLog::Network] serverId:";
        private const string GameDisconnectedEntry = "[FLog::Network] Time to disconnect replication data:";
        private const string GameTeleportingEntry = "[FLog::SingleSurfaceApp] initiateTeleport";
        private const string GameMessageEntry = "[FLog::Output] [BloxstrapRPC]";
        private const string GameLeavingEntry = "[FLog::SingleSurfaceApp] leaveUGCGameInternal";
        private const string GameJoinLoadTimeEntry = "[FLog::GameJoinLoadTime] Report game_join_loadtime:";
        
        private const string GameJoinLoadTimeEntryPattern = ", userid:([0-9]+)";
        private const string GameJoiningEntryPattern = @"! Joining game '([0-9a-f\-]{36})' place ([0-9]+) at ([0-9\.]+)";
        private const string GameJoiningUniversePattern = @"universeid:([0-9]+)";
        private const string GameJoiningUDMUXPattern = @"UDMUX Address = ([0-9\.]+), Port = [0-9]+ \| RCC Server Address = ([0-9\.]+), Port = [0-9]+";
        private const string GameJoinedEntryPattern = @"serverId: ([0-9\.]+)\|[0-9]+";
        private const string GameMessageEntryPattern = @"\[BloxstrapRPC\] (.*)";

        private int _logEntriesRead = 0;
        private bool _teleportMarker = false;
        private bool _reservedTeleportMarker = false;

        public event EventHandler<string>? OnLogEntry;
        public event EventHandler? OnGameJoin;
        public event EventHandler? OnGameLeave;
        public event EventHandler? OnLogOpen;
        public event EventHandler? OnAppClose;
        public event EventHandler<Message>? OnRPCMessage;

        private readonly Dictionary<string, string> GeolocationCache = new();
        private DateTime LastRPCRequest;

        public string LogLocation = null!;

        // these are values to use assuming the player isn't currently in a game
        // hmm... do i move this to a model?
        public DateTime ActivityTimeJoined;
        public bool ActivityInGame = false;
        public long ActivityPlaceId = 0;
        public long ActivityUniverseId = 0;
        public string ActivityJobId = "";
        public string ActivityUserId = "";
        public string ActivityMachineAddress = "";
        public bool ActivityMachineUDMUX = false;
        public bool ActivityIsTeleport = false;
        public string ActivityLaunchData = "";
        public ServerType ActivityServerType = ServerType.Public;

        public List<ActivityHistoryEntry> ActivityHistory = new();

        public bool IsDisposed = false;

        public async void Start()
        {
            const string LOG_IDENT = "ActivityWatcher::Start";

            // okay, here's the process:
            //
            // - tail the latest log file from %localappdata%\roblox\logs
            // - check for specific lines to determine player's game activity as shown below:
            //
            // - get the place id, job id and machine address from '! Joining game '{{JOBID}}' place {{PLACEID}} at {{MACHINEADDRESS}}' entry
            // - confirm place join with 'serverId: {{MACHINEADDRESS}}|{{MACHINEPORT}}' entry
            // - check for leaves/disconnects with 'Time to disconnect replication data: {{TIME}}' entry
            //
            // we'll tail the log file continuously, monitoring for any log entries that we need to determine the current game activity

            string logDirectory = Path.Combine(Paths.LocalAppData, "Roblox\\logs");

            if (!Directory.Exists(logDirectory))
                return;

            FileInfo logFileInfo;

            // we need to make sure we're fetching the absolute latest log file
            // if roblox doesn't start quickly enough, we can wind up fetching the previous log file
            // good rule of thumb is to find a log file that was created in the last 15 seconds or so

            App.Logger.WriteLine(LOG_IDENT, "Opening Roblox log file...");

            while (true)
            {
                logFileInfo = new DirectoryInfo(logDirectory)
                    .GetFiles()
                    .Where(x => x.Name.Contains("Player", StringComparison.OrdinalIgnoreCase) && x.CreationTime <= DateTime.Now)
                    .OrderByDescending(x => x.CreationTime)
                    .First();

                if (logFileInfo.CreationTime.AddSeconds(15) > DateTime.Now)
                    break;

                App.Logger.WriteLine(LOG_IDENT, $"Could not find recent enough log file, waiting... (newest is {logFileInfo.Name})");
                await Task.Delay(1000);
            }

            OnLogOpen?.Invoke(this, EventArgs.Empty);

            LogLocation = logFileInfo.FullName;
            FileStream logFileStream = logFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            App.Logger.WriteLine(LOG_IDENT, $"Opened {LogLocation}");

            var logUpdatedEvent = new AutoResetEvent(false);
            var logWatcher = new FileSystemWatcher()
            {
                Path = logDirectory,
                Filter = Path.GetFileName(logFileInfo.FullName),
                EnableRaisingEvents = true
            };
            logWatcher.Changed += (s, e) => logUpdatedEvent.Set();

            using var sr = new StreamReader(logFileStream);

            while (!IsDisposed)
            {
                string? log = await sr.ReadLineAsync();

                if (log is null)
                    logUpdatedEvent.WaitOne(250);
                else
                    ReadLogEntry(log);
            }
        }

        public string GetActivityDeeplink()
        {
            string deeplink = $"roblox://experiences/start?placeId={ActivityPlaceId}&gameInstanceId={ActivityJobId}";

            if (!String.IsNullOrEmpty(ActivityLaunchData))
                deeplink += "&launchData=" + HttpUtility.UrlEncode(ActivityLaunchData);

            return deeplink;
        }

        // TODO: i need to double check how this handles failed game joins (connection error, invalid permissions, etc)
        private void ReadLogEntry(string entry)
        {
            const string LOG_IDENT = "ActivityWatcher::ReadLogEntry";

            OnLogEntry?.Invoke(this, entry);

            _logEntriesRead += 1;

            // debug stats to ensure that the log reader is working correctly
            // if more than 1000 log entries have been read, only log per 100 to save on spam
            if (_logEntriesRead <= 1000 && _logEntriesRead % 50 == 0)
                App.Logger.WriteLine(LOG_IDENT, $"Read {_logEntriesRead} log entries");
            else if (_logEntriesRead % 100 == 0)
                App.Logger.WriteLine(LOG_IDENT, $"Read {_logEntriesRead} log entries");

            if (entry.Contains(GameLeavingEntry))
                OnAppClose?.Invoke(this, new EventArgs());

            if (ActivityUserId == "" && entry.Contains(GameJoinLoadTimeEntry))
            {
                Match match = Regex.Match(entry, GameJoinLoadTimeEntryPattern);

                if (match.Groups.Count != 2)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to assert format for game join load time entry");
                    App.Logger.WriteLine(LOG_IDENT, entry);
                    return;                
                }

                ActivityUserId = match.Groups[1].Value;
            }
            if (!ActivityInGame && ActivityPlaceId == 0)
            {
                // We are not in a game, nor are in the process of joining one

                if (entry.Contains(GameJoiningPrivateServerEntry))
                {
                    // we only expect to be joining a private server if we're not already in a game
                    ActivityServerType = ServerType.Private;
                }
                else if (entry.Contains(GameJoiningEntry))
                {
                    Match match = Regex.Match(entry, GameJoiningEntryPattern);

                    if (match.Groups.Count != 4)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to assert format for game join entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;
                    }

                    ActivityInGame = false;
                    ActivityPlaceId = long.Parse(match.Groups[2].Value);
                    ActivityJobId = match.Groups[1].Value;
                    ActivityMachineAddress = match.Groups[3].Value;

                    if (_teleportMarker)
                    {
                        ActivityIsTeleport = true;
                        _teleportMarker = false;
                    }

                    if (_reservedTeleportMarker)
                    {
                        ActivityServerType = ServerType.Reserved;
                        _reservedTeleportMarker = false;
                    }

                    App.Logger.WriteLine(LOG_IDENT, $"Joining Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");
                }
            }
            else if (!ActivityInGame && ActivityPlaceId != 0)
            {
                // We are not confirmed to be in a game, but we are in the process of joining one

                if (entry.Contains(GameJoiningUniverseEntry))
                {
                    var match = Regex.Match(entry, GameJoiningUniversePattern);

                    if (match.Groups.Count != 2)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to assert format for game join universe entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;
                    }

                    ActivityUniverseId = long.Parse(match.Groups[1].Value);
                }
                else if (entry.Contains(GameJoiningUDMUXEntry))
                {
                    Match match = Regex.Match(entry, GameJoiningUDMUXPattern);

                    if (match.Groups.Count != 3 || match.Groups[2].Value != ActivityMachineAddress)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to assert format for game join UDMUX entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;
                    }

                    ActivityMachineAddress = match.Groups[1].Value;
                    ActivityMachineUDMUX = true;

                    App.Logger.WriteLine(LOG_IDENT, $"Server is UDMUX protected ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");
                }
                else if (entry.Contains(GameJoinedEntry))
                {
                    Match match = Regex.Match(entry, GameJoinedEntryPattern);

                    if (match.Groups.Count != 2 || match.Groups[1].Value != ActivityMachineAddress)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to assert format for game joined entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;
                    }

                    App.Logger.WriteLine(LOG_IDENT, $"Joined Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                    ActivityInGame = true;
                    ActivityTimeJoined = DateTime.Now;

                    OnGameJoin?.Invoke(this, new EventArgs());
                }
            }
            else if (ActivityInGame && ActivityPlaceId != 0)
            {
                // We are confirmed to be in a game

                if (entry.Contains(GameDisconnectedEntry))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Disconnected from Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                    // TODO: should this be including launchdata?
                    if (ActivityServerType != ServerType.Reserved)
                    {
                        ActivityHistory.Insert(0, new ActivityHistoryEntry
                        {
                            PlaceId = ActivityPlaceId,
                            UniverseId = ActivityUniverseId,
                            JobId = ActivityJobId,
                            TimeJoined = ActivityTimeJoined,
                            TimeLeft = DateTime.Now
                        });
                    }

                    ActivityInGame = false;
                    ActivityPlaceId = 0;
                    ActivityUniverseId = 0;
                    ActivityJobId = "";
                    ActivityMachineAddress = "";
                    ActivityMachineUDMUX = false;
                    ActivityIsTeleport = false;
                    ActivityLaunchData = "";
                    ActivityServerType = ServerType.Public;
                    ActivityUserId = "";
                    
                    OnGameLeave?.Invoke(this, new EventArgs());
                }
                else if (entry.Contains(GameTeleportingEntry))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Initiating teleport to server ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");
                    _teleportMarker = true;
                }
                else if (_teleportMarker && entry.Contains(GameJoiningReservedServerEntry))
                {
                    // we only expect to be joining a reserved server if we're teleporting to one from a game
                    _reservedTeleportMarker = true;
                }
                else if (entry.Contains(GameMessageEntry))
                {
                    var match = Regex.Match(entry, GameMessageEntryPattern);

                    if (match.Groups.Count != 2)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to assert format for RPC message entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;
                    }

                    string messagePlain = match.Groups[1].Value;
                    Message? message;

                    App.Logger.WriteLine(LOG_IDENT, $"Received message: '{messagePlain}'");

                    if ((DateTime.Now - LastRPCRequest).TotalSeconds <= 1)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Dropping message as ratelimit has been hit");
                        return;
                    }

                    try
                    {
                        message = JsonSerializer.Deserialize<Message>(messagePlain);
                    }
                    catch (Exception)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to parse message! (JSON deserialization threw an exception)");
                        return;
                    }

                    if (message is null)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to parse message! (JSON deserialization returned null)");
                        return;
                    }

                    if (string.IsNullOrEmpty(message.Command))
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to parse message! (Command is empty)");
                        return;
                    }

                    if (message.Command == "SetLaunchData")
                    {
                        string? data;

                        try
                        {
                            data = message.Data.Deserialize<string>();
                        }
                        catch (Exception)
                        {
                            App.Logger.WriteLine(LOG_IDENT, "Failed to parse message! (JSON deserialization threw an exception)");
                            return;
                        }

                        if (data is null)
                        {
                            App.Logger.WriteLine(LOG_IDENT, "Failed to parse message! (JSON deserialization returned null)");
                            return;
                        }

                        if (data.Length > 200)
                        {
                            App.Logger.WriteLine(LOG_IDENT, "Data cannot be longer than 200 characters");
                            return;
                        }

                        ActivityLaunchData = data;
                    }

                    OnRPCMessage?.Invoke(this, message);

                    LastRPCRequest = DateTime.Now;
                }
            }
        }

        public async Task<string> GetServerLocation()
        {
            const string LOG_IDENT = "ActivityWatcher::GetServerLocation";

            if (GeolocationCache.ContainsKey(ActivityMachineAddress))
                return GeolocationCache[ActivityMachineAddress];

            try
            {
                string location = "";
                var ipInfo = await Http.GetJson<IPInfoResponse>($"https://ipinfo.io/{ActivityMachineAddress}/json");

                if (ipInfo is null)
                    return $"? ({Strings.ActivityTracker_LookupFailed})";

                if (string.IsNullOrEmpty(ipInfo.Country))
                    location = "?";
                else if (ipInfo.City == ipInfo.Region)
                    location = $"{ipInfo.Region}, {ipInfo.Country}";
                else
                    location = $"{ipInfo.City}, {ipInfo.Region}, {ipInfo.Country}";

                if (!ActivityInGame)
                    return $"? ({Strings.ActivityTracker_LeftGame})";

                GeolocationCache[ActivityMachineAddress] = location;

                return location;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to get server location for {ActivityMachineAddress}");
                App.Logger.WriteException(LOG_IDENT, ex);

                return $"? ({Strings.ActivityTracker_LookupFailed})";
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
