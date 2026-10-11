namespace Bloxstrap
{
    public static class RobloxCaptureBlocker
    {
        public static string ScreenshotsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Roblox");

        public static string RecordingsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Roblox");

        private static string GetBackupPath(string folder) => $"{folder} (blocked)";

        public static bool IsBlocked(string folder) => File.Exists(folder) && !Directory.Exists(folder);

        private static void MoveContents(string source, string destination)
        {
            foreach (string entry in Directory.EnumerateFileSystemEntries(source).ToList())
            {
                string target = Path.Combine(destination, Path.GetFileName(entry));

                if (File.Exists(target) || Directory.Exists(target))
                    continue;

                if (Directory.Exists(entry))
                    Directory.Move(entry, target);
                else
                    File.Move(entry, target);
            }
        }

        public static void SetBlocked(string folder, bool blocked)
        {
            const string LOG_IDENT = "RobloxCaptureBlocker::SetBlocked";

            if (IsBlocked(folder) == blocked)
                return;

            string backup = GetBackupPath(folder);

            try
            {
                if (blocked)
                {
                    if (Directory.Exists(folder))
                    {
                        if (!Directory.Exists(backup))
                            Directory.Move(folder, backup);
                        else
                            MoveContents(folder, backup);
                    }

                    if (Directory.Exists(folder))
                        Directory.Delete(folder, false);

                    File.WriteAllBytes(folder, Array.Empty<byte>());
                    File.SetAttributes(folder, FileAttributes.ReadOnly | FileAttributes.Hidden);
                }
                else
                {
                    File.SetAttributes(folder, FileAttributes.Normal);
                    File.Delete(folder);

                    if (Directory.Exists(backup))
                        Directory.Move(backup, folder);
                }

                App.Logger.WriteLine(LOG_IDENT, $"{(blocked ? "Blocked" : "Unblocked")} {folder}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to change block state of {folder}");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}
