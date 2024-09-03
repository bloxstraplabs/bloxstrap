namespace Bloxstrap.Integrations
{
    public class ActivityWatcher : IDisposable
    {
        private const string GameMessageEntry                = "[FLog::Output] [BloxstrapRPC]";
        private const string GameJoiningEntry                = "[FLog::Output] ! Joining game";

        // these entries are technically volatile!
        // they only get printed depending on their configured FLog level, which could change at any time
        // while levels being changed is fairly rare, please limit the number of varying number of FLog types you have to use, if possible

        private const string GameJoiningPrivateServerEntry   = "[FLog::GameJoinUtil] GameJoinUtil::joinGamePostPrivateServer";
        private const string GameJoiningReservedServerEntry  = "[FLog::GameJoinUtil] GameJoinUtil::initiateTeleportToReservedServer";
        private const string GameJoiningUniverseEntry        = "[FLog::GameJoinLoadTime] Report game_join_loadtime:";
        private const string GameJoiningUDMUXEntry           = "[FLog::Network] UDMUX Address = ";
        private const string GameJoinedEntry                 = "[FLog::Network] serverId:";
        private const string GameDisconnectedEntry           = "[FLog::Network] Time to disconnect replication data:";
        private const string GameTeleportingEntry            = "[FLog::SingleSurfaceApp] initiateTeleport";
        private const string GameLeavingEntry                = "[FLog::SingleSurfaceApp] leaveUGCGameInternal";
        private const string GameJoinLoadTimeEntry = "[FLog::GameJoinLoadTime] Report game_join_loadtime:";

        private const string GameJoinLoadTimeEntryPattern = ", userid:([0-9]+)";
        private const string GameJoiningEntryPattern         = @"! Joining game '([0-9a-f\-]{36})' place ([0-9]+) at ([0-9\.]+)";
        private const string GameJoiningPrivateServerPattern = @"""accessCode"":""([0-9a-f\-]{36})""";
        private const string GameJoiningUniversePattern      = @"universeid:([0-9]+)";
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

        private readonly Dictionary<string, string> GeolocationCache = new();
        private DateTime LastRPCRequest;

        public string LogLocation = null!;

        public bool InGame = false;
        
        public ActivityData Data { get; private set; } = new();

        /// <summary>
        /// Ordered by newest to oldest
        /// </summary>
        public List<ActivityData> History = new();

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

            if (!InGame && Data.PlaceId == 0)
            {
                // We are not in a game, nor are in the process of joining one
                
                //gameJoinLoadTime is written to the log file regardless of the server being private or not
                if (entry.Contains(GameJoinLoadTimeEntry))
                {
                    Match match = Regex.Match(entry, GameJoinLoadTimeEntryPattern);

                    if (match.Groups.Count != 2)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to assert format for game join load time entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;                
                    }

                    Data.ActivityUserId = match.Groups[1].Value;
                }
                
                if (entry.Contains(GameJoiningPrivateServerEntry))
                {
                    // we only expect to be joining a private server if we're not already in a game
                
                    Data.ServerType = ServerType.Private;

                    var match = Regex.Match(entry, GameJoiningPrivateServerPattern);

                    if (match.Groups.Count != 2)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to assert format for game join private server entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;
                    }

                    Data.AccessCode = match.Groups[1].Value;
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

                    InGame = false;
                    Data.PlaceId = long.Parse(match.Groups[2].Value);
                    Data.JobId = match.Groups[1].Value;
                    Data.MachineAddress = match.Groups[3].Value;

                    if (_teleportMarker)
                    {
                        Data.IsTeleport = true;
                        _teleportMarker = false;
                    }

                    if (_reservedTeleportMarker)
                    {
                        Data.ServerType = ServerType.Reserved;
                        _reservedTeleportMarker = false;
                    }

                    App.Logger.WriteLine(LOG_IDENT, $"Joining Game ({Data.PlaceId}/{Data.JobId}/{Data.MachineAddress})");
                }
            }
            else if (!InGame && Data.PlaceId != 0)
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

                    Data.UniverseId = long.Parse(match.Groups[1].Value);

                    if (History.Any())
                    {
                        var lastActivity = History.First();

                        if (lastActivity is not null && Data.UniverseId == lastActivity.UniverseId && Data.IsTeleport)
                            Data.RootActivity = lastActivity.RootActivity ?? lastActivity;
                    }
                }
                else if (entry.Contains(GameJoiningUDMUXEntry))
                {
                    var match = Regex.Match(entry, GameJoiningUDMUXPattern);

                    if (match.Groups.Count != 3 || match.Groups[2].Value != Data.MachineAddress)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to assert format for game join UDMUX entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;
                    }

                    Data.MachineAddress = match.Groups[1].Value;

                    App.Logger.WriteLine(LOG_IDENT, $"Server is UDMUX protected ({Data.PlaceId}/{Data.JobId}/{Data.MachineAddress})");
                }
                else if (entry.Contains(GameJoinedEntry))
                {
                    Match match = Regex.Match(entry, GameJoinedEntryPattern);

                    if (match.Groups.Count != 2 || match.Groups[1].Value != Data.MachineAddress)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to assert format for game joined entry");
                        App.Logger.WriteLine(LOG_IDENT, entry);
                        return;
                    }

                    App.Logger.WriteLine(LOG_IDENT, $"Joined Game ({Data.PlaceId}/{Data.JobId}/{Data.MachineAddress})");

                    InGame = true;
                    Data.TimeJoined = DateTime.Now;

                    OnGameJoin?.Invoke(this, new EventArgs());
                }
            }
            else if (InGame && Data.PlaceId != 0)
            {
                // We are confirmed to be in a game

                if (entry.Contains(GameDisconnectedEntry))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Disconnected from Game ({Data.PlaceId}/{Data.JobId}/{Data.MachineAddress})");

                    Data.TimeLeft = DateTime.Now;
                    History.Insert(0, Data);

                    InGame = false;
                    Data = new();

                    OnGameLeave?.Invoke(this, new EventArgs());
                }
                else if (entry.Contains(GameTeleportingEntry))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Initiating teleport to server ({Data.PlaceId}/{Data.JobId}/{Data.MachineAddress})");
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

                        Data.RPCLaunchData = data;
                    }

                    OnRPCMessage?.Invoke(this, message);

                    LastRPCRequest = DateTime.Now;
                }
            }
        }

        public async Task<string> GetServerLocation()
        {
            const string LOG_IDENT = "ActivityWatcher::GetServerLocation";

            if (GeolocationCache.ContainsKey(Data.MachineAddress))
                return GeolocationCache[Data.MachineAddress];

            try
            {
                string location = "";
                var ipInfo = await Http.GetJson<IPInfoResponse>($"https://ipinfo.io/{Data.MachineAddress}/json");

                if (ipInfo is null)
                    return $"? ({Strings.ActivityTracker_LookupFailed})";

                if (string.IsNullOrEmpty(ipInfo.Country))
                    location = "?";
                else if (ipInfo.City == ipInfo.Region)
                    location = $"{ipInfo.Region}, {ipInfo.Country}";
                else
                    location = $"{ipInfo.City}, {ipInfo.Region}, {ipInfo.Country}";

                if (!InGame)
                    return $"? ({Strings.ActivityTracker_LeftGame})";

                GeolocationCache[Data.MachineAddress] = location;

                return location;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to get server location for {Data.MachineAddress}");
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
