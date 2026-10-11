using System.Reflection;

namespace Bloxstrap
{
    public static class SettingsImporter
    {
        private static readonly string[] KnownBootstrappers = { "Bloxstrap", "Fishstrap", "Froststrap", "Voidstrap" };

        public static Dictionary<string, string> FindInstalls()
        {
            var installs = new Dictionary<string, string>();

            foreach (string name in KnownBootstrappers)
            {
                string folder = Path.Combine(Paths.LocalAppData, name);

                if (String.Equals(Path.GetFullPath(folder).TrimEnd('\\'), Path.GetFullPath(Paths.Base).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    continue;

                if (File.Exists(Path.Combine(folder, "Settings.json")))
                    installs[name] = folder;
            }

            return installs;
        }

        public static int ImportSettings(string folder)
        {
            const string LOG_IDENT = "SettingsImporter::ImportSettings";

            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "Settings.json")));

            int imported = 0;

            foreach (var property in typeof(Models.Persistable.Settings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.Name == nameof(Models.Persistable.Settings.SelectedCustomTheme))
                    continue;

                if (!property.CanWrite || !document.RootElement.TryGetProperty(property.Name, out var element))
                    continue;

                try
                {
                    object? value = element.Deserialize(property.PropertyType);

                    if (value is null || (property.PropertyType.IsEnum && !Enum.IsDefined(property.PropertyType, value)))
                        continue;

                    property.SetValue(App.Settings.Prop, value);
                    imported++;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Skipped {property.Name} ({ex.Message})");
                }
            }

            if (App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.CustomDialog && App.Settings.Prop.SelectedCustomTheme is null)
                App.Settings.Prop.BootstrapperStyle = BootstrapperStyle.FluentDialog;

            return imported;
        }

        public static int ImportFastFlags(string folder)
        {
            string path = Path.Combine(folder, "Modifications", "ClientSettings", "ClientAppSettings.json");

            if (!File.Exists(path))
                return 0;

            var flags = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));

            if (flags is null)
                return 0;

            foreach (var pair in flags)
                App.FastFlags.SetValue(pair.Key, pair.Value);

            return flags.Count;
        }

        public static int ImportMods(string folder)
        {
            string modsFolder = Path.Combine(folder, "Modifications");

            if (!Directory.Exists(modsFolder))
                return 0;

            int copied = 0;

            foreach (string file in Directory.GetFiles(modsFolder, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(modsFolder, file);

                if (relativePath.StartsWith("ClientSettings", StringComparison.OrdinalIgnoreCase))
                    continue;

                string target = Path.Combine(Paths.Modifications, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                Filesystem.AssertReadOnly(target);
                File.Copy(file, target, true);

                copied++;
            }

            return copied;
        }
    }
}
