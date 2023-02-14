using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Bloxstrap.Models;

using DiscordRPC;

namespace Bloxstrap.Helpers.Integrations
{
    class DiscordRichPresence : IDisposable
    {
        readonly DiscordRpcClient RichPresence = new("1005469189907173486");

        const string GameJoiningEntry = "[FLog::Output] ! Joining game";
        const string GameJoinedEntry = "[FLog::Network] serverId:";
        const string GameDisconnectedEntry = "[FLog::Network] Time to disconnect replication data:";

        const string GameJoiningEntryPattern = @"! Joining game '([0-9a-f\-]{36})' place ([0-9]+) at ([0-9\.]+)";
        const string GameJoinedEntryPattern = @"serverId: ([0-9\.]+)\|([0-9]+)";

        // these are values to use assuming the player isn't currently in a game
        bool ActivityInGame = false;
        long ActivityPlaceId = 0;
        string ActivityJobId = "";
        string ActivityMachineAddress = ""; // we're only really using this to confirm a place join. todo: maybe this could be used to see server location/ping?

        public DiscordRichPresence()
        {
            RichPresence.OnReady += (_, e) => 
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] Received ready from user {e.User.Username} ({e.User.ID})");

            RichPresence.OnPresenceUpdate += (_, e) => 
                App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Updated presence");

            RichPresence.OnConnectionEstablished += (_, e) =>
                App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Established connection with Discord RPC");

            //spams log as it tries to connect every ~15 sec when discord is closed so not now
            //RichPresence.OnConnectionFailed += (_, e) =>
            //    App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Failed to establish connection with Discord RPC");

            RichPresence.OnClose += (_, e) =>
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] Lost connection to Discord RPC - {e.Reason} ({e.Code})");

            RichPresence.Initialize();
        }

        private async Task ExamineLogEntry(string entry)
        {
            // App.Logger.WriteLine(entry);

            if (entry.Contains(GameJoiningEntry) && !ActivityInGame && ActivityPlaceId == 0)
            {
                Match match = Regex.Match(entry, GameJoiningEntryPattern);

                if (match.Groups.Count != 4)
                    return;

                ActivityInGame = false;
                ActivityPlaceId = Int64.Parse(match.Groups[2].Value);
                ActivityJobId = match.Groups[1].Value;
                ActivityMachineAddress = match.Groups[3].Value;

                App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Joining Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");
            }
            else if (entry.Contains(GameJoinedEntry) && !ActivityInGame && ActivityPlaceId != 0)
            {
                Match match = Regex.Match(entry, GameJoinedEntryPattern);

                if (match.Groups.Count != 3 || match.Groups[1].Value != ActivityMachineAddress)
                    return;

                App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Joined Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                ActivityInGame = true;
                await SetPresence();
            }
            else if (entry.Contains(GameDisconnectedEntry) && ActivityInGame && ActivityPlaceId != 0)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::ExamineLogEntry] Disconnected from Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                ActivityInGame = false;
                ActivityPlaceId = 0;
                ActivityJobId = "";
                ActivityMachineAddress = "";
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

            while (true)
            {
                logFileInfo = new DirectoryInfo(logDirectory).GetFiles().OrderByDescending(x => x.CreationTime).First();

                if (logFileInfo.CreationTime.AddSeconds(15) > DateTime.Now)
                    break;

                await Task.Delay(1000);
            }

            FileStream logFileStream = logFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            AutoResetEvent logUpdatedEvent = new(false);
            FileSystemWatcher logWatcher = new()
            {
                Path = logDirectory,
                Filter = Path.GetFileName(logFileInfo.FullName),
                EnableRaisingEvents = true
            };
            logWatcher.Changed += (s, e) => logUpdatedEvent.Set();

            using (StreamReader sr = new(logFileStream))
            {
                string? log = null;

                while (true)
                {
                    log = await sr.ReadLineAsync();

                    if (String.IsNullOrEmpty(log))
                    {
                        logUpdatedEvent.WaitOne(1000);
                    }
                    else
                    {
                        //App.Logger.WriteLine(log);
                        await ExamineLogEntry(log);
                    }
                }
            }

            // no need to close the event, its going to be finished with when the program closes anyway
        }

        public async Task<bool> SetPresence()
        {
            if (!ActivityInGame)
            {
                RichPresence.ClearPresence();
                return true;
            }

            string placeThumbnail = "roblox";

            var placeInfo = await Utilities.GetJson<RobloxAsset>($"https://economy.roblox.com/v2/assets/{ActivityPlaceId}/details");

            if (placeInfo is null || placeInfo.Creator is null)
                return false;

            var thumbnailInfo = await Utilities.GetJson<RobloxThumbnails>($"https://thumbnails.roblox.com/v1/places/gameicons?placeIds={ActivityPlaceId}&returnPolicy=PlaceHolder&size=512x512&format=Png&isCircular=false");

            if (thumbnailInfo is not null)
                placeThumbnail = thumbnailInfo.Data![0].ImageUrl!;

            List<DiscordRPC.Button> buttons = new()
            {
                new DiscordRPC.Button()
                {
                    Label = "See Details",
                    Url = $"https://www.roblox.com/games/{ActivityPlaceId}"
                }
            };

            if (!App.Settings.Prop.HideRPCButtons)
            {
                buttons.Insert(0, new DiscordRPC.Button()
                {
                    Label = "Join",
                    Url = $"https://www.roblox.com/games/start?placeId={ActivityPlaceId}&gameInstanceId={ActivityJobId}&launchData=%7B%7D"
                });
            }

            RichPresence.SetPresence(new RichPresence()
            {
                Details = placeInfo.Name,
                State = $"by {placeInfo.Creator.Name}",
                Timestamps = new Timestamps() { Start = DateTime.UtcNow },
                Buttons = buttons.ToArray(),
                Assets = new Assets()
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
            RichPresence.ClearPresence();
            RichPresence.Dispose();
        }
    }
}
