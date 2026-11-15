using Bloxstrap.Models;
using Bloxstrap.Models.Persistable;

namespace Bloxstrap
{
    public static class ConfigurationProfileManager
    {
        private const int PortableProfileFormatVersion = 1;
        private const int MaximumPortableProfileSize = 5 * 1024 * 1024;
        private const int MaximumPortableFastFlags = 5000;
        private const int MaximumHistorySnapshots = 20;

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
            WriteProfile(profile, profile.FilePath);

            return profile;
        }

        public static void ExportPortable(ConfigurationProfile profile, string destinationPath)
        {
            string sourcePath = GetProfilePath(profile);
            var savedProfile = JsonSerializer.Deserialize<ConfigurationProfile>(File.ReadAllText(sourcePath));

            if (savedProfile is null || savedProfile.Settings is null || savedProfile.FastFlags is null)
                throw new InvalidDataException("The selected profile could not be read.");

            savedProfile.FormatVersion = PortableProfileFormatVersion;
            savedProfile.FilePath = String.Empty;

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(destinationPath, JsonSerializer.Serialize(savedProfile, options));
        }

        public static ConfigurationProfile ReadPortable(string sourcePath)
        {
            var fileInfo = new FileInfo(sourcePath);

            if (!fileInfo.Exists)
                throw new FileNotFoundException("The selected profile file could not be found.", sourcePath);

            if (fileInfo.Length == 0 || fileInfo.Length > MaximumPortableProfileSize)
                throw new InvalidDataException("The profile file is empty or larger than the supported 5 MB limit.");

            string json = File.ReadAllText(sourcePath);

            using (JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 64 }))
            {
                JsonElement root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object
                    || !root.TryGetProperty(nameof(ConfigurationProfile.FormatVersion), out JsonElement formatVersion)
                    || !formatVersion.TryGetInt32(out int version)
                    || version != PortableProfileFormatVersion
                    || !root.TryGetProperty(nameof(ConfigurationProfile.Settings), out JsonElement settings)
                    || settings.ValueKind != JsonValueKind.Object
                    || !root.TryGetProperty(nameof(ConfigurationProfile.FastFlags), out JsonElement fastFlags)
                    || fastFlags.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException("This is not a supported LumiStrap profile file.");
                }
            }

            ConfigurationProfile profile = JsonSerializer.Deserialize<ConfigurationProfile>(json)
                ?? throw new InvalidDataException("The profile file could not be read.");
            ValidatePortableProfile(profile);

            profile.FilePath = String.Empty;

            return profile;
        }

        public static ConfigurationProfile ImportPortable(ConfigurationProfile profile)
        {
            ValidatePortableProfile(profile);

            profile.Name = GetAvailableProfileName(profile.Name.Trim());
            profile.CreatedAtUtc = DateTime.UtcNow;
            profile.IsAutomaticUpdateBackup = false;
            profile.IsHistorySnapshot = false;
            profile.FilePath = Path.Combine(ProfilesDirectory, $"{Guid.NewGuid():N}.json");

            Directory.CreateDirectory(ProfilesDirectory);
            WriteProfile(profile, profile.FilePath);

            return profile;
        }

        public static (int SettingsChanged, int FlagsAdded, int FlagsUpdated, int FlagsRemoved) GetApplyPreview(ConfigurationProfile profile)
        {
            string path = GetProfilePath(profile);
            var savedProfile = JsonSerializer.Deserialize<ConfigurationProfile>(File.ReadAllText(path));

            if (savedProfile is null || savedProfile.Settings is null || savedProfile.FastFlags is null)
                throw new InvalidDataException("The selected profile could not be read.");

            JsonElement currentSettings = JsonSerializer.SerializeToElement(App.Settings.Prop);
            JsonElement targetSettings = JsonSerializer.SerializeToElement(savedProfile.Settings);
            var currentProperties = currentSettings.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.GetRawText());
            var targetProperties = targetSettings.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.GetRawText());
            var settingNames = currentProperties.Keys.Concat(targetProperties.Keys).Distinct(StringComparer.Ordinal);
            int settingsChanged = settingNames.Count(name =>
                !currentProperties.TryGetValue(name, out string? currentValue)
                || !targetProperties.TryGetValue(name, out string? targetValue)
                || !String.Equals(currentValue, targetValue, StringComparison.Ordinal));

            var currentFlags = App.FastFlags.Prop.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString() ?? String.Empty);
            int flagsAdded = savedProfile.FastFlags.Keys.Count(name => !currentFlags.ContainsKey(name));
            int flagsUpdated = savedProfile.FastFlags.Count(pair =>
                currentFlags.TryGetValue(pair.Key, out string? currentValue)
                && !String.Equals(currentValue, pair.Value, StringComparison.Ordinal));
            int flagsRemoved = currentFlags.Keys.Count(name => !savedProfile.FastFlags.ContainsKey(name));

            return (settingsChanged, flagsAdded, flagsUpdated, flagsRemoved);
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

            SaveCurrentHistorySnapshot();
            App.PendingSettingTasks.Clear();
            App.Settings.Prop = savedProfile.Settings;
            App.FastFlags.Prop = savedProfile.FastFlags.ToDictionary(x => x.Key, x => (object)x.Value);
            App.Settings.Save();
            App.FastFlags.Save();
        }

        public static void SaveBeforeSettingsSave()
        {
            try
            {
                Settings previousSettings = File.Exists(App.Settings.FileLocation)
                    ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(App.Settings.FileLocation)) ?? new Settings()
                    : new Settings();

                Dictionary<string, string> previousFlags = File.Exists(App.FastFlags.FileLocation)
                    ? JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(App.FastFlags.FileLocation))?.ToDictionary(pair => pair.Key, pair => pair.Value.ToString()) ?? new()
                    : new();

                var currentFlags = App.FastFlags.Prop.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString() ?? String.Empty);
                if (AreConfigurationsEqual(previousSettings, previousFlags, App.Settings.Prop, currentFlags))
                    return;

                SaveHistorySnapshot(previousSettings, previousFlags);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("ConfigurationProfileManager::SaveBeforeSettingsSave", "Could not preserve the previous configuration in history");
                App.Logger.WriteException("ConfigurationProfileManager::SaveBeforeSettingsSave", ex);
            }
        }

        public static void SaveCurrentHistorySnapshot()
        {
            var settings = CloneSettings(App.Settings.Prop);
            var flags = App.FastFlags.Prop.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString() ?? String.Empty);

            ConfigurationProfile? latest = LoadAll()
                .Where(profile => profile.IsHistorySnapshot)
                .OrderByDescending(profile => profile.CreatedAtUtc)
                .FirstOrDefault();

            if (latest is not null && AreConfigurationsEqual(latest.Settings, latest.FastFlags, settings, flags))
                return;

            SaveHistorySnapshot(settings, flags);
        }

        public static string GetClientPreset()
        {
            var currentFlags = App.FastFlags.Prop.ToDictionary(
                pair => pair.Key,
                pair => pair.Value?.ToString() ?? String.Empty,
                StringComparer.Ordinal);

            if (FastFlagsEqual(currentFlags, CreateClientPresetFlags("performance")))
                return "performance";

            if (FastFlagsEqual(currentFlags, CreateClientPresetFlags("quality")))
                return "quality";

            return "custom";
        }

        public static void ApplyClientPreset(string presetName)
        {
            Dictionary<string, string> flags = CreateClientPresetFlags(presetName);
            var currentFlags = App.FastFlags.Prop.ToDictionary(
                pair => pair.Key,
                pair => pair.Value?.ToString() ?? String.Empty,
                StringComparer.Ordinal);

            if (FastFlagsEqual(currentFlags, flags))
                return;

            SaveCurrentHistorySnapshot();
            App.PendingSettingTasks.Clear();
            App.FastFlags.Prop = flags.ToDictionary(pair => pair.Key, pair => (object)pair.Value);
            App.FastFlags.Save();
            App.Logger.WriteLine("ConfigurationProfileManager::ApplyClientPreset", $"Applied client preset '{presetName}'");
        }

        private static Dictionary<string, string> CreateClientPresetFlags(string presetName)
        {
            bool isPerformance = String.Equals(presetName, "performance", StringComparison.OrdinalIgnoreCase);
            bool isQuality = String.Equals(presetName, "quality", StringComparison.OrdinalIgnoreCase);

            if (!isPerformance && !isQuality)
                throw new ArgumentException("The selected client preset is not supported.", nameof(presetName));

            int meshLevel = isPerformance ? 0 : 3;
            var flags = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [FastFlagManager.PresetFlags["Rendering.ManualFullscreen"]] = "False",
                [FastFlagManager.PresetFlags["Rendering.MSAA"]] = isPerformance ? "1" : "4",
                [FastFlagManager.PresetFlags["Rendering.FRMQualityOverride"]] = isPerformance ? "1" : "21",
                [FastFlagManager.PresetFlags["Geometry.MeshLOD.Static"]] = meshLevel.ToString(),
                [FastFlagManager.PresetFlags["Rendering.TextureQuality.OverrideEnabled"]] = "True",
                [FastFlagManager.PresetFlags["Rendering.TextureQuality.Level"]] = isPerformance ? "0" : "3"
            };

            string[] meshLevels = { "L0", "L12", "L23", "L34" };
            for (int index = 0; index < meshLevels.Length; index++)
            {
                int distance = (Math.Clamp(meshLevel - index, 0, 3) + 1) * 250;
                string key = FastFlagManager.PresetFlags[$"Geometry.MeshLOD.{meshLevels[index]}"];
                flags[key] = distance.ToString();
            }

            return flags;
        }

        private static bool FastFlagsEqual(Dictionary<string, string> left, Dictionary<string, string> right)
            => left.Count == right.Count
                && left.All(pair => right.TryGetValue(pair.Key, out string? value)
                    && String.Equals(pair.Value, value, StringComparison.Ordinal));

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

        private static string GetAvailableProfileName(string requestedName)
        {
            var existingNames = LoadAll()
                .Select(profile => profile.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!existingNames.Contains(requestedName))
                return requestedName;

            for (int suffixNumber = 2; ; suffixNumber++)
            {
                string suffix = $" ({suffixNumber})";
                string baseName = requestedName[..Math.Min(requestedName.Length, 40 - suffix.Length)];
                string candidate = baseName + suffix;

                if (!existingNames.Contains(candidate))
                    return candidate;
            }
        }

        private static void SaveHistorySnapshot(Settings settings, Dictionary<string, string> flags)
        {
            if (!Paths.Initialized)
                return;

            Directory.CreateDirectory(ProfilesDirectory);
            string name = String.Format(LumiFeatureStrings.Menu_Profiles_HistoryEntry, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var snapshot = new ConfigurationProfile
            {
                Name = name,
                CreatedAtUtc = DateTime.UtcNow,
                IsHistorySnapshot = true,
                Settings = CloneSettings(settings),
                FastFlags = new(flags, StringComparer.Ordinal),
                FilePath = Path.Combine(ProfilesDirectory, $"{Guid.NewGuid():N}.json")
            };

            WriteProfile(snapshot, snapshot.FilePath);

            foreach (ConfigurationProfile oldSnapshot in LoadAll()
                .Where(profile => profile.IsHistorySnapshot)
                .OrderByDescending(profile => profile.CreatedAtUtc)
                .Skip(MaximumHistorySnapshots))
            {
                try { Delete(oldSnapshot); }
                catch (Exception ex) { App.Logger.WriteException("ConfigurationProfileManager::PruneHistory", ex); }
            }
        }

        private static bool AreConfigurationsEqual(Settings leftSettings, Dictionary<string, string> leftFlags, Settings rightSettings, Dictionary<string, string> rightFlags)
        {
            string leftJson = JsonSerializer.Serialize(leftSettings);
            string rightJson = JsonSerializer.Serialize(rightSettings);
            return String.Equals(leftJson, rightJson, StringComparison.Ordinal)
                && leftFlags.Count == rightFlags.Count
                && leftFlags.All(pair => rightFlags.TryGetValue(pair.Key, out string? value) && String.Equals(pair.Value, value, StringComparison.Ordinal));
        }

        private static Settings CloneSettings(Settings settings)
            => JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings)) ?? new Settings();

        private static void ValidatePortableProfile(ConfigurationProfile? profile)
        {
            if (profile is null
                || profile.Settings is null
                || profile.FastFlags is null
                || profile.FormatVersion != PortableProfileFormatVersion)
            {
                throw new InvalidDataException("This is not a supported LumiStrap profile.");
            }

            if (String.IsNullOrWhiteSpace(profile.Name))
                throw new InvalidDataException("The profile must have a name.");

            profile.Name = profile.Name.Trim();

            if (profile.Name.Length > 40 || profile.Name.Any(Char.IsControl))
                throw new InvalidDataException("The profile name is invalid. Names must be 40 characters or fewer.");

            if (profile.FastFlags.Count > MaximumPortableFastFlags)
                throw new InvalidDataException($"The profile contains more than {MaximumPortableFastFlags} Fast Flags.");

            foreach (var flag in profile.FastFlags)
            {
                if (String.IsNullOrWhiteSpace(flag.Key)
                    || flag.Key.Length > 256
                    || !flag.Key.All(character => Char.IsLetterOrDigit(character) || character == '_')
                    || flag.Value is null
                    || flag.Value.Length > 4096)
                {
                    throw new InvalidDataException("The profile contains an invalid Fast Flag entry.");
                }
            }
        }

        private static void WriteProfile(ConfigurationProfile profile, string destinationPath)
        {
            string temporaryPath = destinationPath + ".tmp";

            try
            {
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temporaryPath, destinationPath);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }
    }
}
