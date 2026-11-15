using Bloxstrap.Models;

namespace Bloxstrap
{
    public static class ExperienceLibraryManager
    {
        private static string FileLocation => Path.Combine(Paths.Base, "ExperienceLibrary.json");

        public static List<ExperienceEntry> LoadAll()
        {
            if (!Paths.Initialized)
                return new();

            using var mutex = new Mutex(false, $"Local\\{App.ProjectName}_ExperienceLibrary");
            if (!mutex.WaitOne(TimeSpan.FromSeconds(3)))
                return new();

            try
            {
                return LoadUnsafe()
                    .OrderByDescending(entry => entry.IsFavorite)
                    .ThenByDescending(entry => entry.LastPlayedAtUtc ?? entry.AddedAtUtc)
                    .ToList();
            }
            finally { mutex.ReleaseMutex(); }
        }

        public static bool TryParsePlaceId(string value, out long placeId)
        {
            placeId = 0;
            string input = value.Trim();

            if (long.TryParse(input, out placeId) && placeId > 0)
                return true;

            if (!Uri.TryCreate(input, UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                || !(uri.Host.Equals("roblox.com", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.EndsWith(".roblox.com", StringComparison.OrdinalIgnoreCase)))
            {
                placeId = 0;
                return false;
            }

            Match match = Regex.Match(uri.AbsolutePath, @"^/games/(\d+)(?:/|$)", RegexOptions.IgnoreCase);
            return match.Success && long.TryParse(match.Groups[1].Value, out placeId) && placeId > 0;
        }

        public static void AddOrUpdate(long placeId, string? name)
        {
            Mutate(entries =>
            {
                ExperienceEntry? entry = entries.FirstOrDefault(item => item.PlaceId == placeId);
                if (entry is null)
                {
                    entry = new ExperienceEntry { PlaceId = placeId, AddedAtUtc = DateTime.UtcNow };
                    entries.Add(entry);
                }

                if (!String.IsNullOrWhiteSpace(name))
                    entry.Name = name.Trim();
                else if (String.IsNullOrWhiteSpace(entry.Name))
                    entry.Name = String.Format(LumiFeatureStrings.Menu_Experiences_DefaultName, placeId);
            });
        }

        public static void RecordRecent(long placeId)
        {
            if (placeId <= 0 || !Paths.Initialized)
                return;

            try
            {
                Mutate(entries =>
                {
                    ExperienceEntry? entry = entries.FirstOrDefault(item => item.PlaceId == placeId);
                    if (entry is null)
                    {
                        entry = new ExperienceEntry
                        {
                            PlaceId = placeId,
                            Name = String.Format(LumiFeatureStrings.Menu_Experiences_DefaultName, placeId),
                            AddedAtUtc = DateTime.UtcNow
                        };
                        entries.Add(entry);
                    }

                    entry.LastPlayedAtUtc = DateTime.UtcNow;
                });
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("ExperienceLibraryManager::RecordRecent", "Could not save the recent experience");
                App.Logger.WriteException("ExperienceLibraryManager::RecordRecent", ex);
            }
        }

        public static void ToggleFavorite(long placeId)
            => Mutate(entries =>
            {
                ExperienceEntry? entry = entries.FirstOrDefault(item => item.PlaceId == placeId);
                if (entry is not null)
                    entry.IsFavorite = !entry.IsFavorite;
            });

        public static void Remove(long placeId)
            => Mutate(entries => entries.RemoveAll(item => item.PlaceId == placeId));

        private static void Mutate(Action<List<ExperienceEntry>> action)
        {
            if (!Paths.Initialized)
                throw new InvalidOperationException("LumiStrap is not installed yet.");

            using var mutex = new Mutex(false, $"Local\\{App.ProjectName}_ExperienceLibrary");
            if (!mutex.WaitOne(TimeSpan.FromSeconds(3)))
                throw new IOException("The experience library is busy. Try again in a moment.");

            try
            {
                List<ExperienceEntry> entries = LoadUnsafe();
                action(entries);
                SaveUnsafe(entries);
            }
            finally { mutex.ReleaseMutex(); }
        }

        private static List<ExperienceEntry> LoadUnsafe()
        {
            try
            {
                if (!File.Exists(FileLocation))
                    return new();

                return JsonSerializer.Deserialize<List<ExperienceEntry>>(File.ReadAllText(FileLocation))?
                    .Where(entry => entry.PlaceId > 0)
                    .GroupBy(entry => entry.PlaceId)
                    .Select(group => group.OrderByDescending(entry => entry.LastPlayedAtUtc ?? entry.AddedAtUtc).First())
                    .ToList() ?? new();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("ExperienceLibraryManager::Load", "The experience list could not be read");
                App.Logger.WriteException("ExperienceLibraryManager::Load", ex);
                return new();
            }
        }

        private static void SaveUnsafe(List<ExperienceEntry> entries)
        {
            Directory.CreateDirectory(Paths.Base);
            string temporaryPath = FileLocation + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporaryPath, FileLocation, true);
        }
    }
}
