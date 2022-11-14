using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace Bloxstrap.Helpers
{
    public class Utilities
    {
        public static void OpenWebsite(string website)
        {
            Process.Start(new ProcessStartInfo { FileName = website, UseShellExecute = true });
        }

        public static async Task<T?> GetJson<T>(string url)
        {
            var request = await Bootstrapper.Client.SendAsync(new()
            {
                RequestUri = new Uri(url),
                Headers = {
                    { "User-Agent", Program.ProjectRepository }
                }
            });
            var json = await request.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
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
