using Bloxstrap.Models.Entities;

namespace Bloxstrap
{
    public static class GameShortcuts
    {
        private class UniverseIdResponse
        {
            [JsonPropertyName("universeId")]
            public long? UniverseId { get; set; }
        }

        private static string IconsFolder => Path.Combine(Paths.Base, "GameShortcutIcons");

        public static long? ParsePlaceId(string input)
        {
            input = input.Trim();

            var match = Regex.Match(input, @"games/([0-9]+)");

            if (match.Success)
                input = match.Groups[1].Value;

            return long.TryParse(input, out long placeId) && placeId > 0 ? placeId : null;
        }

        private class SearchResponse
        {
            [JsonPropertyName("searchResults")]
            public List<SearchGroup> SearchResults { get; set; } = new();
        }

        private class SearchGroup
        {
            [JsonPropertyName("contentGroupType")]
            public string ContentGroupType { get; set; } = "";

            [JsonPropertyName("contents")]
            public List<SearchContent> Contents { get; set; } = new();
        }

        private class SearchContent
        {
            [JsonPropertyName("rootPlaceId")]
            public long RootPlaceId { get; set; }
        }

        public static async Task<long?> FindPlaceByName(string name)
        {
            var response = await Http.GetJson<SearchResponse>($"https://apis.roblox.com/search-api/omni-search?searchQuery={Uri.EscapeDataString(name.Trim())}&sessionId={Guid.NewGuid()}&pageType=all");

            long placeId = response.SearchResults.Where(x => x.ContentGroupType == "Game").SelectMany(x => x.Contents).Select(x => x.RootPlaceId).FirstOrDefault(x => x != 0);

            return placeId == 0 ? null : placeId;
        }

        public static string? ParseLinkCode(string input)
        {
            input = input.Trim();

            if (String.IsNullOrEmpty(input))
                return null;

            var match = Regex.Match(input, @"privateServerLinkCode=([0-9A-Za-z_-]+)");

            if (match.Success)
                return match.Groups[1].Value;

            return Regex.IsMatch(input, @"^[0-9A-Za-z_-]+$") ? input : null;
        }

        public static async Task<string> Create(long placeId, string? jobId = null, string? linkCode = null)
        {
            var universe = await Http.GetJson<UniverseIdResponse>($"https://apis.roblox.com/universes/v1/places/{placeId}/universe");

            if (universe.UniverseId is not long universeId)
                throw new InvalidOperationException($"Place {placeId} does not exist");

            await UniverseDetails.FetchSingle(universeId);

            var details = UniverseDetails.LoadFromCache(universeId)
                ?? throw new InvalidOperationException($"Could not load details for universe {universeId}");

            string name = details.Data.Name;
            string safeName = String.Concat(name.Split(Path.GetInvalidFileNameChars())).Trim();

            if (String.IsNullOrEmpty(safeName))
                safeName = placeId.ToString();

            string iconPath = Paths.Application;

            if (details.Thumbnail.ImageUrl is string imageUrl)
            {
                Directory.CreateDirectory(IconsFolder);
                iconPath = Path.Combine(IconsFolder, $"{placeId}.ico");

                byte[] png = await App.HttpClient.GetByteArrayAsync(imageUrl);
                WritePngAsIcon(png, iconPath);
            }

            string deeplink = $"roblox://experiences/start?placeId={placeId}";

            if (!String.IsNullOrEmpty(linkCode))
            {
                deeplink += $"&linkCode={linkCode}";
                safeName += " (private server)";
            }
            else if (!String.IsNullOrEmpty(jobId))
            {
                deeplink += $"&gameInstanceId={jobId}";
                safeName += " (server)";
            }

            string lnkPath = Path.Combine(Paths.Desktop, $"{safeName}.lnk");

            ShellLink.Shortcut.CreateShortcut(Paths.Application, deeplink, iconPath, 0).WriteToFile(lnkPath);

            return name;
        }

        private static void WritePngAsIcon(byte[] png, string path)
        {
            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);

            writer.Write((short)0);
            writer.Write((short)1);
            writer.Write((short)1);

            writer.Write((byte)128);
            writer.Write((byte)128);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((short)1);
            writer.Write((short)32);
            writer.Write(png.Length);
            writer.Write(6 + 16);

            writer.Write(png);
        }
    }
}
