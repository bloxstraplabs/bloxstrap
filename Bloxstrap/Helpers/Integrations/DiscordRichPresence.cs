using Bloxstrap.Models;
using DiscordRPC;

namespace Bloxstrap.Helpers.Integrations
{
	internal class DiscordRichPresence : IDisposable
	{
		readonly DiscordRpcClient RichPresence = new("1005469189907173486");

		public async Task<bool> SetPresence(string placeId)
		{
			string placeThumbnail;

			var placeInfo = await Utilities.GetJson<RobloxAsset>($"https://economy.roblox.com/v2/assets/{placeId}/details");

			if (placeInfo is null || placeInfo.Creator is null)
				return false;

			var thumbnailInfo = await Utilities.GetJson<RobloxThumbnails>($"https://thumbnails.roblox.com/v1/places/gameicons?placeIds={placeId}&returnPolicy=PlaceHolder&size=512x512&format=Png&isCircular=false");

			if (thumbnailInfo is null)
				placeThumbnail = "roblox"; //fallback
			else
				placeThumbnail = thumbnailInfo.Data[0].ImageUrl;

			DiscordRPC.Button[]? buttons = null;

			if (!Program.Settings.HideRPCButtons)
			{
				buttons = new DiscordRPC.Button[]
				{
					new DiscordRPC.Button()
					{
						Label = "Play",
						Url = $"https://www.roblox.com/games/start?placeId={placeId}&launchData=%7B%7D"
					},

					new DiscordRPC.Button()
					{
						Label = "View Details",
						Url = $"https://www.roblox.com/games/{placeId}"
					}
				};
			}

			RichPresence.Initialize();

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
