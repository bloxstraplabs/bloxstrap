using System.IO.Compression;

using Bloxstrap.Models.SettingTasks;

namespace Bloxstrap
{
    public static class Backups
    {
        private const string ModPackFolder = "mods/";

        private const string AppFolder = "reistrap/";

        private const string RobloxSettingsEntry = "roblox/GlobalBasicSettings_13.xml";

        private static readonly string[] BackupFiles = { "Settings.json", "GameStats.json" };

        private static readonly string[] BackupFolders = { "Modifications", "FastFlagProfiles", "CursorSets", "CustomThemes" };

        private static void AddFolder(ZipArchive archive, string folder, string entryPrefix, Func<string, bool>? include = null)
        {
            if (!Directory.Exists(folder))
                return;

            foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(folder, file).Replace('\\', '/');

                if (include is not null && !include(relative))
                    continue;

                archive.CreateEntryFromFile(file, entryPrefix + relative);
            }
        }

        private static string? GetSafeTarget(string root, string relative)
        {
            string target = Path.GetFullPath(Path.Combine(root, relative));
            string fullRoot = Path.GetFullPath(root).TrimEnd('\\') + '\\';

            return target.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) ? target : null;
        }

        private static void ExtractEntry(ZipArchiveEntry entry, string target)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            Filesystem.AssertReadOnly(target);
            entry.ExtractToFile(target, true);
        }

        // Mod Packs

        public static int ExportModPack(string zipPath)
        {
            File.Delete(zipPath);

            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);

            AddFolder(archive, Paths.Modifications, ModPackFolder, x => !x.StartsWith("ClientSettings/", StringComparison.OrdinalIgnoreCase));

            return archive.Entries.Count;
        }

        public static int ImportModPack(string zipPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            var entries = archive.Entries.Where(x => x.FullName.StartsWith(ModPackFolder, StringComparison.OrdinalIgnoreCase) && !String.IsNullOrEmpty(x.Name)).ToList();

            if (!entries.Any())
                throw new InvalidDataException(Strings.Menu_Mods_ModPack_Invalid);

            int imported = 0;

            foreach (var entry in entries)
            {
                string relative = entry.FullName[ModPackFolder.Length..];

                if (relative.StartsWith("ClientSettings/", StringComparison.OrdinalIgnoreCase))
                    continue;

                string? target = GetSafeTarget(Paths.Modifications, relative);

                if (target is null)
                    continue;

                ExtractEntry(entry, target);
                imported++;
            }

            return imported;
        }

        // Full Backups

        public static void CreateBackup(string zipPath)
        {
            File.Delete(zipPath);

            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);

            foreach (string file in BackupFiles.Select(x => Path.Combine(Paths.Base, x)).Where(File.Exists))
                archive.CreateEntryFromFile(file, AppFolder + Path.GetFileName(file));

            foreach (string folder in BackupFolders)
                AddFolder(archive, Path.Combine(Paths.Base, folder), $"{AppFolder}{folder}/");

            if (File.Exists(RobloxSettingsTask.FileLocation))
                archive.CreateEntryFromFile(RobloxSettingsTask.FileLocation, RobloxSettingsEntry);
        }

        public static void RestoreBackup(string zipPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            if (!archive.Entries.Any(x => x.FullName == AppFolder + "Settings.json"))
                throw new InvalidDataException(Strings.Menu_Bloxstrap_Backup_Invalid);

            foreach (var entry in archive.Entries.Where(x => !String.IsNullOrEmpty(x.Name)))
            {
                string? target = null;

                if (entry.FullName.StartsWith(AppFolder, StringComparison.Ordinal))
                {
                    string relative = entry.FullName[AppFolder.Length..];
                    string top = relative.Split('/')[0];

                    if (BackupFiles.Contains(relative) || BackupFolders.Contains(top))
                        target = GetSafeTarget(Paths.Base, relative);
                }
                else if (entry.FullName == RobloxSettingsEntry && !Process.GetProcessesByName(App.RobloxPlayerAppName).Any())
                {
                    if (File.Exists(RobloxSettingsTask.FileLocation))
                        File.SetAttributes(RobloxSettingsTask.FileLocation, FileAttributes.Normal);

                    target = RobloxSettingsTask.FileLocation;
                }

                if (target is not null)
                    ExtractEntry(entry, target);
            }
        }
    }
}
