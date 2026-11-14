using Bloxstrap.Models;
using Bloxstrap.Models.Persistable;

namespace Bloxstrap
{
    public static class ConfigurationProfileManager
    {
        private static string ProfilesDirectory => Path.Combine(Paths.Base, "Profiles");

        public static List<ConfigurationProfile> LoadAll()
        {
            var profiles = new List<ConfigurationProfile>();

            if (!Directory.Exists(ProfilesDirectory))
                return profiles;

            foreach (string path in Directory.GetFiles(ProfilesDirectory, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var profile = JsonSerializer.Deserialize<ConfigurationProfile>(File.ReadAllText(path));

                    if (profile is null || String.IsNullOrWhiteSpace(profile.Name) || profile.Settings is null || profile.FastFlags is null)
                        continue;

                    profile.FilePath = path;
                    profiles.Add(profile);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine("ConfigurationProfileManager::LoadAll", $"Skipping invalid profile '{path}'");
                    App.Logger.WriteException("ConfigurationProfileManager::LoadAll", ex);
                }
            }

            return profiles.OrderByDescending(x => x.CreatedAtUtc).ToList();
        }

        public static ConfigurationProfile SaveCurrent(string name, bool isAutomaticUpdateBackup = false)
        {
            if (!Paths.Initialized)
                throw new InvalidOperationException("LumiStrap is not installed yet.");

            name = name.Trim();

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Enter a profile name.", nameof(name));

            if (name.Length > 40 || name.Any(Char.IsControl))
                throw new ArgumentException("Profile names must be 40 characters or fewer.", nameof(name));

            if (LoadAll().Any(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A profile with this name already exists.");

            Directory.CreateDirectory(ProfilesDirectory);

            var profile = new ConfigurationProfile
            {
                Name = name,
                CreatedAtUtc = DateTime.UtcNow,
                IsAutomaticUpdateBackup = isAutomaticUpdateBackup,
                Settings = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(App.Settings.Prop)) ?? new Settings(),
                FastFlags = App.FastFlags.Prop.ToDictionary(x => x.Key, x => x.Value?.ToString() ?? String.Empty)
            };

            profile.FilePath = Path.Combine(ProfilesDirectory, $"{Guid.NewGuid():N}.json");
            string temporaryPath = profile.FilePath + ".tmp";

            try
            {
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temporaryPath, profile.FilePath);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }

            return profile;
        }

        public static ConfigurationProfile SaveUpdateBackup(string version)
        {
            App.Settings.Save();
            App.State.Save();
            App.FastFlags.Save();

            string shortVersion = version.Length > 10 ? version[..10] : version;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            string baseName = String.Format(Strings.Menu_Profiles_UpdateBackupName, shortVersion, timestamp);
            if (baseName.Length > 40)
                baseName = baseName[..40];

            var existingNames = LoadAll()
                .Select(profile => profile.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            string name = baseName;
            for (int suffix = 2; existingNames.Contains(name); suffix++)
            {
                string suffixText = $" ({suffix})";
                int nameLength = Math.Min(baseName.Length, 40 - suffixText.Length);
                name = baseName[..nameLength] + suffixText;
            }

            ConfigurationProfile backup = SaveCurrent(name, isAutomaticUpdateBackup: true);

            foreach (ConfigurationProfile oldBackup in LoadAll()
                .Where(profile => profile.IsAutomaticUpdateBackup)
                .OrderByDescending(profile => profile.CreatedAtUtc)
                .Skip(5))
            {
                try
                {
                    Delete(oldBackup);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("ConfigurationProfileManager::PruneUpdateBackups", ex);
                }
            }

            return backup;
        }

        public static void Apply(ConfigurationProfile profile)
        {
            string path = GetProfilePath(profile);
            var savedProfile = JsonSerializer.Deserialize<ConfigurationProfile>(File.ReadAllText(path));

            if (savedProfile is null || savedProfile.Settings is null || savedProfile.FastFlags is null)
                throw new InvalidDataException("This profile could not be read.");

            App.PendingSettingTasks.Clear();
            App.Settings.Prop = savedProfile.Settings;
            App.FastFlags.Prop = savedProfile.FastFlags.ToDictionary(x => x.Key, x => (object)x.Value);
            App.Settings.Save();
            App.FastFlags.Save();
        }

        public static void Delete(ConfigurationProfile profile)
        {
            string path = GetProfilePath(profile);
            File.Delete(path);
        }

        private static string GetProfilePath(ConfigurationProfile profile)
        {
            string root = Path.GetFullPath(ProfilesDirectory) + Path.DirectorySeparatorChar;
            string path = Path.GetFullPath(profile.FilePath);

            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
                throw new InvalidOperationException("The selected profile is not available.");

            return path;
        }
    }
}
