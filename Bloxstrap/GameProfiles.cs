using Bloxstrap.Models.Entities;
using Bloxstrap.Models.SettingTasks;

namespace Bloxstrap
{
    public static class GameProfiles
    {
        private class UniverseIdResponse
        {
            [JsonPropertyName("universeId")]
            public long? UniverseId { get; set; }
        }

        public static GameProfile? Find(long placeId, long universeId = 0) =>
            App.Settings.Prop.GameProfiles.FirstOrDefault(x => (universeId != 0 && x.UniverseId == universeId) || (placeId != 0 && x.PlaceId == placeId));

        public static long? GetPlaceIdFromLaunchArgs(string launchArgs)
        {
            var match = Regex.Match(launchArgs, @"placeId(?:=|%3D)([0-9]+)", RegexOptions.IgnoreCase);

            return match.Success && long.TryParse(match.Groups[1].Value, out long placeId) ? placeId : null;
        }

        public static async Task<GameProfile> Create(long placeId)
        {
            var universe = await Http.GetJson<UniverseIdResponse>($"https://apis.roblox.com/universes/v1/places/{placeId}/universe");

            if (universe.UniverseId is not long universeId)
                throw new InvalidOperationException($"Place {placeId} does not exist");

            await UniverseDetails.FetchSingle(universeId);
            var details = UniverseDetails.LoadFromCache(universeId);

            return new GameProfile
            {
                PlaceId = details?.Data.RootPlaceId is long rootPlace && rootPlace != 0 ? rootPlace : placeId,
                UniverseId = universeId,
                Name = details?.Data.Name ?? placeId.ToString()
            };
        }

        public static string? GetFastFlagProfilePath(GameProfile profile)
        {
            if (String.IsNullOrEmpty(profile.FastFlagProfile))
                return null;

            string path = Path.Combine(Paths.FastFlagProfiles, $"{profile.FastFlagProfile}.json");

            return File.Exists(path) ? path : null;
        }

        public static Dictionary<string, string>? ApplyRobloxSettings(GameProfile profile)
        {
            const string LOG_IDENT = "GameProfiles::ApplyRobloxSettings";

            var changes = new List<(string Type, string Name, string Value)>();

            if (Int32.TryParse(profile.FramerateCap, out int cap) && cap > 0)
                changes.Add(("int", "FramerateCap", cap.ToString()));

            if (!String.IsNullOrEmpty(profile.QualityLevel))
                changes.Add(("token", "SavedQualityLevel", profile.QualityLevel));

            if (!changes.Any())
                return null;

            if (Process.GetProcessesByName(App.RobloxPlayerAppName).Any())
            {
                App.Logger.WriteLine(LOG_IDENT, "Roblox is already running, skipping game settings");
                return null;
            }

            var task = new RobloxSettingsTask();

            if (!task.Exists)
                return null;

            var original = new Dictionary<string, string>();

            foreach (var change in changes)
            {
                original[$"{change.Type}:{change.Name}"] = task.Get(change.Name) ?? "";
                task.Set(change.Type, change.Name, change.Value);
            }

            task.Execute();
            App.PendingSettingTasks.Remove(task.Name);

            App.Logger.WriteLine(LOG_IDENT, $"Applied game settings for {profile.Name}");

            return original;
        }

        public static void RestoreRobloxSettings(Dictionary<string, string> original)
        {
            var task = new RobloxSettingsTask();

            if (!task.Exists)
                return;

            foreach (var pair in original.Where(x => !String.IsNullOrEmpty(x.Value)))
            {
                string[] parts = pair.Key.Split(':', 2);
                task.Set(parts[0], parts[1], pair.Value);
            }

            task.Execute();
            App.PendingSettingTasks.Remove(task.Name);
        }
    }
}
