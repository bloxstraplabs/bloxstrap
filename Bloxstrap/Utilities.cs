namespace Bloxstrap
{
    static class Utilities
    {
        public static long GetFreeDiskSpace(string path)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (path.StartsWith(drive.Name))
                    return drive.AvailableFreeSpace;
            }

            return -1;
        }

        public static void ShellExecute(string website) => Process.Start(new ProcessStartInfo { FileName = website, UseShellExecute = true });

        public static int VersionToNumber(string version)
        {
            // yes this is kinda stupid lol
            version = version.Replace("v", "").Replace(".", "");
            return Int32.Parse(version);
        }
    }
}
