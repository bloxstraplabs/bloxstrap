using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bloxstrap.Helpers
{
    public class Utilities
    {
        public static void OpenWebsite(string website)
        {
            Process.Start(new ProcessStartInfo { FileName = website, UseShellExecute = true });
        }

		public static async Task<JObject> GetJson(string url)
		{
			using (HttpClient client = new())
			{
				client.DefaultRequestHeaders.Add("User-Agent", Program.ProjectRepository);

				string jsonString = await client.GetStringAsync(url);
				return (JObject)JsonConvert.DeserializeObject(jsonString);
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

        // quick and hacky way of getting a value from any key/value pair formatted list
        // (command line args, uri params, etc)
        public static string? GetKeyValue(string subject, string key, char delimiter)
        {
            if (subject.LastIndexOf(key) == -1)
                return null;

            string substr = subject.Substring(subject.LastIndexOf(key) + key.Length);

            if (substr.IndexOf(delimiter) == -1)
                return null;

            return substr.Split(delimiter)[0];
        }
    }
}
