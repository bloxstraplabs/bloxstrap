namespace Bloxstrap
{
    public static class Cleaner
    {
        public static string RobloxLogs => Path.Combine(Paths.LocalAppData, "Roblox", "logs");

        public static string RobloxCache => Path.Combine(Path.GetTempPath(), "Roblox");

        private static TimeSpan? GetMaxAge(CleanerAge age) => age switch
        {
            CleanerAge.OneDay => TimeSpan.FromDays(1),
            CleanerAge.OneWeek => TimeSpan.FromDays(7),
            CleanerAge.OneMonth => TimeSpan.FromDays(30),
            _ => null
        };

        private static IEnumerable<string> GetFolders()
        {
            var settings = App.Settings.Prop;

            if (settings.CleanRobloxLogs)
                yield return RobloxLogs;

            if (settings.CleanRobloxCache)
                yield return RobloxCache;

            if (settings.CleanAppLogs)
                yield return Paths.Logs;

            if (settings.CleanAppDownloads)
                yield return Paths.Downloads;
        }

        public static long Run(bool ignoreAge = false)
        {
            const string LOG_IDENT = "Cleaner::Run";

            var maxAge = GetMaxAge(App.Settings.Prop.CleanerAge);

            if (maxAge is null && !ignoreAge)
                return 0;

            var threshold = ignoreAge ? DateTime.Now : DateTime.Now - maxAge!.Value;

            long freed = 0;
            int deleted = 0;

            foreach (string folder in GetFolders().Where(Directory.Exists))
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var info = new FileInfo(file);

                        if (info.LastWriteTime > threshold || String.Equals(file, App.Logger.FileLocation, StringComparison.OrdinalIgnoreCase))
                            continue;

                        long size = info.Length;
                        info.IsReadOnly = false;
                        info.Delete();

                        freed += size;
                        deleted++;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            App.Logger.WriteLine(LOG_IDENT, $"Deleted {deleted} files, freeing {freed / 1024 / 1024} MB");

            return freed;
        }
    }
}
