using System.Text.Encodings.Web;
using System.Text.Json.Nodes;

namespace Bloxstrap
{
    public static class RobloxAppStorage
    {
        public static string FileLocation => Path.Combine(Paths.LocalAppData, "Roblox", "LocalStorage", "appStorage.json");

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static void Apply()
        {
            const string LOG_IDENT = "RobloxAppStorage::Apply";

            if (!File.Exists(FileLocation))
                return;

            try
            {
                if (JsonNode.Parse(File.ReadAllText(FileLocation)) is not JsonObject storage)
                    return;

                string background = App.Settings.Prop.RobloxBackgroundApp ? "true" : "false";

                storage["LaunchAtStartup"] = background;
                storage["MinimizeToTray"] = background;

                ApplyTheme(storage);

                File.WriteAllText(FileLocation, storage.ToJsonString(SerializerOptions));
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to update Roblox app storage");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static void ApplyTheme(JsonObject storage)
        {
            var theme = App.Settings.Prop.RobloxAppTheme;

            if (theme == RobloxAppTheme.Default)
                return;

            string value = theme == RobloxAppTheme.Dark ? "dark" : "light";

            storage["AuthenticatedTheme"] = value;

            if (storage["UserId"] is not JsonValue idNode || !idNode.TryGetValue(out string? userId) || String.IsNullOrEmpty(userId))
                return;

            JsonObject themes = new();

            if (storage["DeviceLevelTheme"] is JsonValue themesNode
                && themesNode.TryGetValue(out string? raw)
                && !String.IsNullOrWhiteSpace(raw)
                && JsonNode.Parse(raw) is JsonObject parsed)
                themes = parsed;

            themes[userId] = value;
            storage["DeviceLevelTheme"] = themes.ToJsonString(SerializerOptions);
        }
    }
}
