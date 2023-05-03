using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bloxstrap
{
    public class RobloxActivity : IDisposable
    {
        // i'm thinking the functionality for parsing roblox logs could be broadened for more features than just rich presence,
        // like checking the ping and region of the current connected server. maybe that's something to add?
        private const string GameJoiningEntry = "[FLog::Output] ! Joining game";
        private const string GameJoiningUDMUXEntry = "[FLog::Network] UDMUX Address = ";
        private const string GameJoinedEntry = "[FLog::Network] serverId:";
        private const string GameDisconnectedEntry = "[FLog::Network] Time to disconnect replication data:";
        private const string GameTeleportingEntry = "[FLog::SingleSurfaceApp] initiateTeleport";

        private const string GameJoiningEntryPattern = @"! Joining game '([0-9a-f\-]{36})' place ([0-9]+) at ([0-9\.]+)";
        private const string GameJoiningUDMUXPattern = @"UDMUX Address = ([0-9\.]+), Port = [0-9]+ \| RCC Server Address = ([0-9\.]+), Port = [0-9]+";
        private const string GameJoinedEntryPattern = @"serverId: ([0-9\.]+)\|[0-9]+";

        private int _logEntriesRead = 0;

        public event EventHandler? OnGameJoin;
        public event EventHandler? OnGameLeave;

        // these are values to use assuming the player isn't currently in a game
        // keep in mind ActivityIsTeleport is only reset by DiscordRichPresence when it's done accessing it
        // because of the weird chronology of where the teleporting entry is outputted, there's no way to reset it in here
        public bool ActivityInGame = false;
        public long ActivityPlaceId = 0;
        public string ActivityJobId = "";
        public string ActivityMachineAddress = "";
        public bool ActivityMachineUDMUX = false;
        public bool ActivityIsTeleport = false;

        public bool IsDisposed = false;

        public async void StartWatcher()
        {
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

            string logDirectory = Path.Combine(Directories.LocalAppData, "Roblox\\logs");

            if (!Directory.Exists(logDirectory))
                return;

            FileInfo logFileInfo;

            // we need to make sure we're fetching the absolute latest log file
            // if roblox doesn't start quickly enough, we can wind up fetching the previous log file
            // good rule of thumb is to find a log file that was created in the last 15 seconds or so

            App.Logger.WriteLine("[RobloxActivity::StartWatcher] Opening Roblox log file...");

            while (true)
            {
                logFileInfo = new DirectoryInfo(logDirectory).GetFiles().OrderByDescending(x => x.CreationTime).First();

                if (logFileInfo.CreationTime.AddSeconds(15) > DateTime.Now)
                    break;

                App.Logger.WriteLine($"[RobloxActivity::StartWatcher] Could not find recent enough log file, waiting... (newest is {logFileInfo.Name})");
                await Task.Delay(1000);
            }

            FileStream logFileStream = logFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            App.Logger.WriteLine($"[RobloxActivity::StartWatcher] Opened {logFileInfo.Name}");

            AutoResetEvent logUpdatedEvent = new(false);
            FileSystemWatcher logWatcher = new()
            {
                Path = logDirectory,
                Filter = Path.GetFileName(logFileInfo.FullName),
                EnableRaisingEvents = true
            };
            logWatcher.Changed += (s, e) => logUpdatedEvent.Set();

            using StreamReader sr = new(logFileStream);

            while (!IsDisposed)
            {
                string? log = await sr.ReadLineAsync();

                if (string.IsNullOrEmpty(log))
                    logUpdatedEvent.WaitOne(1000);
                else
                    ExamineLogEntry(log);
            }
        }

        private void ExamineLogEntry(string entry)
        {
            // App.Logger.WriteLine(entry);
            _logEntriesRead += 1;

            // debug stats to ensure that the log reader is working correctly
            // if more than 1000 log entries have been read, only log per 100 to save on spam
            if (_logEntriesRead <= 1000 && _logEntriesRead % 50 == 0)
                App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Read {_logEntriesRead} log entries");
            else if (_logEntriesRead % 100 == 0)
                App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Read {_logEntriesRead} log entries");

            if (!ActivityInGame && ActivityPlaceId == 0 && entry.Contains(GameJoiningEntry))
            {
                Match match = Regex.Match(entry, GameJoiningEntryPattern);

                if (match.Groups.Count != 4)
                {
                    App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Failed to assert format for game join entry");
                    App.Logger.WriteLine(entry);
                    return;
                }

                ActivityInGame = false;
                ActivityPlaceId = long.Parse(match.Groups[2].Value);
                ActivityJobId = match.Groups[1].Value;
                ActivityMachineAddress = match.Groups[3].Value;

                App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Joining Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");
            }
            else if (!ActivityInGame && ActivityPlaceId != 0)
            {
                if (entry.Contains(GameJoiningUDMUXEntry))
                {
                    Match match = Regex.Match(entry, GameJoiningUDMUXPattern);

                    if (match.Groups.Count != 3 || match.Groups[2].Value != ActivityMachineAddress)
                    {
                        App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Failed to assert format for game join UDMUX entry");
                        App.Logger.WriteLine(entry);
                        return;
                    }

                    ActivityMachineAddress = match.Groups[1].Value;
                    ActivityMachineUDMUX = true;

                    App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Server is UDMUX protected ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");
                }
                else if (entry.Contains(GameJoinedEntry))
                {
                    Match match = Regex.Match(entry, GameJoinedEntryPattern);

                    if (match.Groups.Count != 2 || match.Groups[1].Value != ActivityMachineAddress)
                    {
                        App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Failed to assert format for game joined entry");
                        App.Logger.WriteLine(entry);
                        return;
                    }

                    App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Joined Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                    ActivityInGame = true;
                    OnGameJoin?.Invoke(this, new EventArgs());
                }
            }
            else if (ActivityInGame && ActivityPlaceId != 0)
            {
                if (entry.Contains(GameDisconnectedEntry))
                {
                    App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Disconnected from Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                    ActivityInGame = false;
                    ActivityPlaceId = 0;
                    ActivityJobId = "";
                    ActivityMachineAddress = "";
                    ActivityMachineUDMUX = false;

                    OnGameLeave?.Invoke(this, new EventArgs());
                }
                else if (entry.Contains(GameTeleportingEntry))
                {
                    App.Logger.WriteLine($"[RobloxActivity::ExamineLogEntry] Initiating teleport to server ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");
                    ActivityIsTeleport = true;
                }
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
