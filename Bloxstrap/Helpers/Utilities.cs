using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bloxstrap.Helpers
{
    public class Utilities
    {
        public static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public static long GetFreeDiskSpace(string path)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (path.StartsWith(drive.Name))
                    return drive.AvailableFreeSpace;
            }

            return -1;
        }

        public static int GetProcessCount(string processName, bool log = true)
        {
            if (log)
                App.Logger.WriteLine($"[Utilities::CheckIfProcessRunning] Checking if '{processName}' is running...");

            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");

            if (log)
                App.Logger.WriteLine($"[Utilities::CheckIfProcessRunning] Found {processes.Length} process(es) running for '{processName}'");

            return processes.Length;
        }

        public static void OpenWebsite(string website)
        {
            Process.Start(new ProcessStartInfo { FileName = website, UseShellExecute = true });
        }

        public static async Task<T?> GetJson<T>(string url)
        {
            try
            {
                string json = await App.HttpClient.GetStringAsync(url);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static string MD5File(string filename)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static string MD5Data(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        // quick and hacky way of getting a value from any key/value pair formatted list
        // (command line args, uri params, etc)
        public static string? GetKeyValue(string subject, string key, char delimiter)
        {
            if (subject.LastIndexOf(key) == -1)
                return null;

            string substr = subject.Substring(subject.LastIndexOf(key) + key.Length);

            if (!substr.Contains(delimiter))
                return null;

            return substr.Split(delimiter)[0];
        }
    }
}
