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

        public static async Task<string> Create(long placeId)
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

            string lnkPath = Path.Combine(Paths.Desktop, $"{safeName}.lnk");

            ShellLink.Shortcut.CreateShortcut(Paths.Application, $"roblox://experiences/start?placeId={placeId}", iconPath, 0).WriteToFile(lnkPath);

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
