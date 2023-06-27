using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

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

        public static async Task<T?> GetJson<T>(string url)
        {
            try
            {
                App.Logger.WriteLine($"[Utilities::GetJson<{typeof(T).Name}>] Getting JSON from {url}!");
                string json = await App.HttpClient.GetStringAsync(url);
                App.Logger.WriteLine($"[Utilities::GetJson<{typeof(T).Name}>] Got JSON: {json}");
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine($"[Utilities::GetJson<{typeof(T).Name}>] Failed to deserialize JSON!");
                App.Logger.WriteLine($"[Utilities::GetJson<{typeof(T).Name}>] {ex}");
                return default;
            }
        }

        public static int VersionToNumber(string version)
        {
            // yes this is kinda stupid lol
            version = version.Replace("v", "").Replace(".", "");
            return Int32.Parse(version);
        }
    }
}
