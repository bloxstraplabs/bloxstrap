using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using Bloxstrap.Models;

using DiscordRPC;

namespace Bloxstrap.Helpers.Integrations
{
    class DiscordRichPresence : IDisposable
    {
        readonly DiscordRpcClient RichPresence = new("1005469189907173486");

        const string GameJoiningEntry = "[FLog::Output] ! Joining game";
        const string GameJoinedEntry = "[FLog::Network] serverId:";
        const string GameDisconnectedEntry = "[FLog::Network] Client:Disconnect";

        const string GameJoiningEntryPattern = @"! Joining game '([0-9a-f\-]{36})' place ([0-9]+) at ([0-9\.]+)";
        const string GameJoinedEntryPattern = @"serverId: ([0-9\.]+)\|([0-9]+)";

        // these are values to use assuming the player isn't currently in a game
        bool ActivityInGame = false;
        long ActivityPlaceId = 0;
        string ActivityJobId = "";
        string ActivityMachineAddress = ""; // we're only really using this to confirm a place join. todo: maybe this could be used to see server location/ping?

        public DiscordRichPresence()
        {
            RichPresence.Initialize();
        }

        private async Task ExamineLogEntry(string entry)
        {
            Debug.WriteLine(entry);

            if (entry.Contains(GameJoiningEntry) && !ActivityInGame && ActivityPlaceId == 0)
            {
                Match match = Regex.Match(entry, GameJoiningEntryPattern);

                if (match.Groups.Count != 4)
                    return;

                ActivityInGame = false;
                ActivityPlaceId = Int64.Parse(match.Groups[2].Value);
                ActivityJobId = match.Groups[1].Value;
                ActivityMachineAddress = match.Groups[3].Value;

                Debug.WriteLine($"[DiscordRichPresence] Joining Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");
            }
            else if (entry.Contains(GameJoinedEntry) && !ActivityInGame && ActivityPlaceId != 0)
            {
                Match match = Regex.Match(entry, GameJoinedEntryPattern);

                if (match.Groups.Count != 3 || match.Groups[1].Value != ActivityMachineAddress)
                    return;

                Debug.WriteLine($"[DiscordRichPresence] Joined Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                ActivityInGame = true;
                await SetPresence();
            }
            else if (entry.Contains(GameDisconnectedEntry) && ActivityInGame && ActivityPlaceId != 0)
            {
                Debug.WriteLine($"[DiscordRichPresence] Disconnected from Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

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
            // - read the latest log file from %localappdata%\roblox\logs approx every 30 sec or so
            // - check for specific lines to determine player's game activity as shown below:
            //
            // - get the place id, job id and machine address from '! Joining game '{{JOBID}}' place {{PLACEID}} at {{MACHINEADDRESS}}' entry
            // - confirm place join with 'serverId: {{MACHINEADDRESS}}|{{MACHINEPORT}}' entry
            // - check for leaves/disconnects with 'Client:Disconnect' entry
            //
            // we'll tail the log file continuously, monitoring for any log entries that we need to determine the current game activity

            string logDirectory = Path.Combine(Program.LocalAppData, "Roblox\\logs");

            if (!Directory.Exists(logDirectory))
                return;

            FileInfo logFileInfo = new DirectoryInfo(logDirectory).GetFiles().OrderByDescending(f => f.LastWriteTime).First();
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
                        //Debug.WriteLine(log);
                        await ExamineLogEntry(log);
                    }
                }
            }

            // no need to close the event, its going to be finished with when the program closes anyway
            // ...rr im too lazy to fix the event still be updating when its closed... lol
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

            if (!Program.Settings.HideRPCButtons)
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
            RichPresence.Dispose();
        }
    }
}
