using System;
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
        string ActivityMachineAddress = ""; // we're only really using this to confirm a place join

		public DiscordRichPresence()
		{
            RichPresence.Initialize();
        }

		private static IEnumerable<string> GetLog()
		{
            Debug.WriteLine("[DiscordRichPresence] Reading log file...");

            string logDirectory = Path.Combine(Program.LocalAppData, "Roblox\\logs");

            if (!Directory.Exists(logDirectory))
                return Enumerable.Empty<string>();

            FileInfo logFileInfo = new DirectoryInfo(logDirectory).GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            List<string> log = new();

			// we just want to read the last 3500 lines of the log file
			// this should typically more than cover the last 30 seconds of logs
			// it has to be last 3500 lines (~360KB) because voice chat outputs a loooot of logs :')

			ReverseLineReader rlr = new(() => logFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
			log = rlr.Take(3500).ToList();

            Debug.WriteLine("[DiscordRichPresence] Finished reading log file");

            return log;
        }

		private async Task ExamineLog(List<string> log)
		{
            Debug.WriteLine("[DiscordRichPresence] Examining log file...");
            
			foreach (string entry in log)
            {
                if (entry.Contains(GameJoiningEntry) && !ActivityInGame && ActivityPlaceId == 0)
                {
                    Match match = Regex.Match(entry, GameJoiningEntryPattern);

                    if (match.Groups.Count != 4)
                        continue;

                    ActivityInGame = false;
                    ActivityPlaceId = Int64.Parse(match.Groups[2].Value);
                    ActivityJobId = match.Groups[1].Value;
                    ActivityMachineAddress = match.Groups[3].Value;

                    Debug.WriteLine($"[DiscordRichPresence] Joining Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                    // examine log again to check for immediate changes
                    await Task.Delay(1000);
					MonitorGameActivity(false);
                    break;
                }
                else if (entry.Contains(GameJoinedEntry) && !ActivityInGame && ActivityPlaceId != 0)
                {
                    Match match = Regex.Match(entry, GameJoinedEntryPattern);

                    if (match.Groups.Count != 3 || match.Groups[1].Value != ActivityMachineAddress)
                        continue;

                    Debug.WriteLine($"[DiscordRichPresence] Joined Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

                    ActivityInGame = true;
                    await SetPresence();

                    // examine log again to check for immediate changes
                    await Task.Delay(1000);
                    MonitorGameActivity(false);
                    break;
                }
                //else if (entry.Contains(GameDisconnectedEntry) && ActivityInGame && ActivityPlaceId != 0)
                else if (entry.Contains(GameDisconnectedEntry))
                {
					// for this one, we want to break as soon as we see this entry
					// or else it'll match a game join entry and think we're joining again
					if (ActivityInGame && ActivityPlaceId != 0)
					{
						Debug.WriteLine($"[DiscordRichPresence] Disconnected from Game ({ActivityPlaceId}/{ActivityJobId}/{ActivityMachineAddress})");

						ActivityInGame = false;
						ActivityPlaceId = 0;
						ActivityJobId = "";
						ActivityMachineAddress = "";
						await SetPresence();

                        // examine log again to check for immediate changes
                        await Task.Delay(1000);
                        MonitorGameActivity(false);
                    }

                    break;
                }
            }

            Debug.WriteLine("[DiscordRichPresence] Finished examining log file");
        }

        public async void MonitorGameActivity(bool loop = true)
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
			// we'll read the log file from bottom-to-top and find which line meets the criteria
			// the processes for reading and examining the log files are separated since the log may have to be examined multiple times

			// read log file
			List<string> log = GetLog().ToList();

			// and now let's get the current status from the log
			await ExamineLog(log);

			if (!loop)
				return;

			await Task.Delay(ActivityInGame ? 30000 : 10000);
			MonitorGameActivity();
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

			DiscordRPC.Button[]? buttons = null;

			if (!Program.Settings.HideRPCButtons)
			{
				buttons = new DiscordRPC.Button[]
				{
					new DiscordRPC.Button()
					{
						Label = "Join",
						Url = $"https://www.roblox.com/games/start?placeId={ActivityPlaceId}&gameInstanceId={ActivityJobId}&launchData=%7B%7D"
					},

					new DiscordRPC.Button()
					{
						Label = "See Details",
						Url = $"https://www.roblox.com/games/{ActivityPlaceId}"
					}
				};
			}

			RichPresence.SetPresence(new RichPresence()
			{
				Details = placeInfo.Name,
				State = $"by {placeInfo.Creator.Name}",
				Timestamps = new Timestamps() { Start = DateTime.UtcNow },
				Buttons = buttons,
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
