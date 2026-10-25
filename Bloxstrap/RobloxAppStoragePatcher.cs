using System.Text.Encodings.Web;

namespace Bloxstrap
{
    internal static class RobloxAppStoragePatcher
    {
        private static bool IsRobloxOpen()
        {
            return
                Process.GetProcessesByName(App.RobloxPlayerAppName).Length > 0 ||
                Process.GetProcessesByName(App.RobloxStudioAppName).Length > 0;
        }

        private static string GetAppStoragePath()
        {
            string path = Paths.RobloxAppData;
            Directory.CreateDirectory(path);

            path = Path.Combine(path, "LocalStorage");
            Directory.CreateDirectory(path);

            path = Path.Combine(path, "appStorage.json");
            return path;
        }

        private static Dictionary<string, string>? ReadAppStorageData(string path)
        {
            const string LOG_IDENT = "RobloxAppStoragePatcher::ReadAppStorageData";

            App.Logger.WriteLine(LOG_IDENT, $"Reading app storage data from {path}");

            if (!File.Exists(path))
            {
                App.Logger.WriteLine(LOG_IDENT, "No app storage found, returning empty data");
                return new Dictionary<string, string>();
            }

            try
            {
                string dataSerialised = File.ReadAllText(path);
                Dictionary<string, string> data = JsonSerializer.Deserialize<Dictionary<string, string>>(dataSerialised) ?? throw new Exception("Bad data");
                return data;
            }
            catch (Exception ex)
            {
                // give up on patching, we don't want to overwrite appstorage if the format changes
                App.Logger.WriteLine(LOG_IDENT, "Failed to read app storage data");
                App.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }

        private static void WriteAppStorageData(string path, Dictionary<string, string> data)
        {
            const string LOG_IDENT = "RobloxAppStoragePatcher::WriteAppStorageData";

            App.Logger.WriteLine(LOG_IDENT, $"Writing app storage data from {path}");

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            try
            {
                string dataSerialised = JsonSerializer.Serialize(data, options);
                File.WriteAllText(path, dataSerialised);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to write app storage data");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static bool SetSetting(Dictionary<string, string> data, string settingName, string newValue)
        {
            if (data.TryGetValue(settingName, out string? value))
            {
                if (value == newValue)
                    return false;
            }

            data[settingName] = newValue;
            return true;
        }

        private static bool UpdateData(Dictionary<string, string> data)
        {
            const string LOG_IDENT = "RobloxAppStoragePatcher::UpdateData";

            App.Logger.WriteLine(LOG_IDENT, "Updating app storage data");

            bool writeRequired = false;

            // these two settings do not work well with bloxstrap
            writeRequired |= SetSetting(data, "LaunchAtStartup", "false");
            writeRequired |= SetSetting(data, "MinimizeToTray", "false");
            // dont bug us about the tray since we're disabling it
            writeRequired |= SetSetting(data, "SystemTrayModalShown", "true");

            return writeRequired;
        }

        public static void Patch()
        {
            const string LOG_IDENT = "RobloxAppStoragePatcher::Path";

            if (IsRobloxOpen())
            {
                App.Logger.WriteLine(LOG_IDENT, "Skipping app storage patch as Roblox is open");
                return;
            }

            string path = GetAppStoragePath();

            Dictionary<string, string>? data = ReadAppStorageData(path);
            if (data == null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Not applying patch");
                return;
            }

            bool writeRequired = UpdateData(data);

            if (writeRequired)
            {
                WriteAppStorageData(path, data);
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "Skipping write");
            }
        }
    }
}
