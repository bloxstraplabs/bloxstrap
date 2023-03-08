using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bloxstrap.Helpers;

using Bloxstrap.Models;

using DiscordRPC;

namespace Bloxstrap.Integrations
{
    class DiscordRichPresence : IDisposable
    {
        private readonly DiscordRpcClient _rpcClient = new("1005469189907173486");

        // i'm thinking the functionality for parsing roblox logs could be broadened for more features than just rich presence,
        // like checking the ping and region of the current connected server. maybe that's something to add?
        private const string GameJoiningEntry = "[FLog::Output] ! Joining game";
        private const string GameJoiningUDMUXEntry = "[FLog::Network] UDMUX Address = ";
        private const string GameJoinedEntry = "[FLog::Network] serverId:";
        private const string GameDisconnectedEntry = "[FLog::Network] Time to disconnect replication data:";

        private const string GameJoiningEntryPattern = @"! Joining game '([0-9a-f\-]{36})' place ([0-9]+) at ([0-9\.]+)";
        private const string GameJoiningUDMUXPattern = @"UDMUX Address = ([0-9\.]+), Port = [0-9]+ \| RCC Server Address = ([0-9\.]+), Port = [0-9]+";
        private const string GameJoinedEntryPattern = @"serverId: ([0-9\.]+)\|[0-9]+";

        private int _logEntriesRead = 0;

        // these are values to use assuming the player isn't currently in a game
        private bool _activityInGame = false;
        private long _activityPlaceId = 0;
        private string _activityJobId = "";
        private string _activityMachineAddress = "";

        public DiscordRichPresence()
        {
            _rpcClient.OnReady += (_, e) =>
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] Received ready from user {e.User.Username} ({e.User.ID})");

            _rpcClient.OnPresenceUpdate += (_, e) =>
                App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Updated presence");

            _rpcClient.OnConnectionEstablished += (_, e) =>
                App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Established connection with Discord RPC");

            //spams log as it tries to connect every ~15 sec when discord is closed so not now
            //_rpcClient.OnConnectionFailed += (_, e) =>
            //    App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Failed to establish connection with Discord RPC");

            _rpcClient.OnClose += (_, e) =>
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] Lost connection to Discord RPC - {e.Reason} ({e.Code})");

            _rpcClient.Initialize();
        }

        private async Task ExamineLogEntry(string entry)
        {
            // App.Logger.WriteLine(entry);
            _logEntriesRead += 1;

            // debug stats to ensure that the log reader is working correctly
            // if more than 5000 log entries have been read, only log per 100 to save on spam
            if (_logEntriesRead <= 5000 && _logEntriesRead % 50 == 0)
                App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Read {_logEntriesRead} log entries");
            else if (_logEntriesRead % 100 == 0)
                App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Read {_logEntriesRead} log entries");

            if (!_activityInGame && _activityPlaceId == 0)
            {
                if (entry.Contains(GameJoiningEntry))
                {
                    Match match = Regex.Match(entry, GameJoiningEntryPattern);

                    if (match.Groups.Count != 4)
                    {
                        App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Failed to assert format for game join entry");
                        App.Logger.WriteLine(entry);
                        return;
                    }

                    _activityInGame = false;
                    _activityPlaceId = long.Parse(match.Groups[2].Value);
                    _activityJobId = match.Groups[1].Value;
                    _activityMachineAddress = match.Groups[3].Value;

                    App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Joining Game ({_activityPlaceId}/{_activityJobId}/{_activityMachineAddress})");
                }
                else if (entry.Contains(GameJoiningUDMUXEntry))
                {
                    Match match = Regex.Match(entry, GameJoiningUDMUXPattern);

                    if (match.Groups.Count != 3 || match.Groups[2].Value != _activityMachineAddress)
                    {
                        App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Failed to assert format for game join UDMUX entry");
                        App.Logger.WriteLine(entry);
                        return;
                    }

                    _activityMachineAddress = match.Groups[1].Value;
                    
                    App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Server is UDMUX protected ({_activityPlaceId}/{_activityJobId}/{_activityMachineAddress})");
                }
            }
            else if (entry.Contains(GameJoinedEntry) && !_activityInGame && _activityPlaceId != 0)
            {
                Match match = Regex.Match(entry, GameJoinedEntryPattern);

                if (match.Groups.Count != 2 || match.Groups[1].Value != _activityMachineAddress)
                {
                    App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Failed to assert format for game joined entry");
                    App.Logger.WriteLine(entry);
                    return;
                }

                App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Joined Game ({_activityPlaceId}/{_activityJobId}/{_activityMachineAddress})");

                _activityInGame = true;
                await SetPresence();
            }
            else if (entry.Contains(GameDisconnectedEntry) && _activityInGame && _activityPlaceId != 0)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Disconnected from Game ({_activityPlaceId}/{_activityJobId}/{_activityMachineAddress})");

                _activityInGame = false;
                _activityPlaceId = 0;
                _activityJobId = "";
                _activityMachineAddress = "";
                await SetPresence();
            }
        }

        public async void MonitorGameActivity()
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

            App.Logger.WriteLine("[DiscordRichPresence::MonitorGameActivity] Opening Roblox log file...");

            while (true)
            {
                logFileInfo = new DirectoryInfo(logDirectory).GetFiles().OrderByDescending(x => x.CreationTime).First();

                if (logFileInfo.CreationTime.AddSeconds(15) > DateTime.Now)
                    break;

                App.Logger.WriteLine($"[DiscordRichPresence::MonitorGameActivity] Could not find recent enough log file, waiting... (newest is {logFileInfo.Name})");
                await Task.Delay(1000);
            }

            FileStream logFileStream = logFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            App.Logger.WriteLine($"[DiscordRichPresence::MonitorGameActivity] Opened {logFileInfo.Name}");

            AutoResetEvent logUpdatedEvent = new(false);
            FileSystemWatcher logWatcher = new()
            {
                Path = logDirectory,
                Filter = Path.GetFileName(logFileInfo.FullName),
                EnableRaisingEvents = true
            };
            logWatcher.Changed += (s, e) => logUpdatedEvent.Set();

            using StreamReader sr = new(logFileStream);

            while (true)
            {
                string? log = await sr.ReadLineAsync();

                if (string.IsNullOrEmpty(log))
                    logUpdatedEvent.WaitOne(1000);
                else
                    await ExamineLogEntry(log);
            }

            // no need to close the event, its going to be finished with when the program closes anyway
        }

        public async Task<bool> SetPresence()
        {
            if (!_activityInGame)
            {
                _rpcClient.ClearPresence();
                return true;
            }

            string placeThumbnail = "roblox";

            var placeInfo = await Utilities.GetJson<RobloxAsset>($"https://economy.roblox.com/v2/assets/{_activityPlaceId}/details");

            if (placeInfo is null || placeInfo.Creator is null)
                return false;

            var thumbnailInfo = await Utilities.GetJson<RobloxThumbnails>($"https://thumbnails.roblox.com/v1/places/gameicons?placeIds={_activityPlaceId}&returnPolicy=PlaceHolder&size=512x512&format=Png&isCircular=false");

            if (thumbnailInfo is not null)
                placeThumbnail = thumbnailInfo.Data![0].ImageUrl!;

            List<Button> buttons = new()
            {
                new Button
                {
                    Label = "See Details",
                    Url = $"https://www.roblox.com/games/{_activityPlaceId}"
                }
            };

            if (!App.Settings.Prop.HideRPCButtons)
            {
                buttons.Insert(0, new Button
                {
                    Label = "Join",
                    Url = $"roblox://experiences/start?placeId={_activityPlaceId}&gameInstanceId={_activityJobId}"
                });
            }

            _rpcClient.SetPresence(new RichPresence
            {
                Details = placeInfo.Name,
                State = $"by {placeInfo.Creator.Name}",
                Timestamps = new Timestamps { Start = DateTime.UtcNow },
                Buttons = buttons.ToArray(),
                Assets = new Assets
                {
                    LargeImageKey = placeThumbnail,
                    LargeImageText = placeInfo.Name,
                    SmallImageKey = "roblox",
                    SmallImageText = "Roblox"
                }
            });

            return true;
        }

        public void Dispose()
        {
            App.Logger.WriteLine("[DiscordRichPresence::Dispose] Cleaning up Discord RPC and Presence");
            _rpcClient.ClearPresence();
            _rpcClient.Dispose();
        }
    }
}
