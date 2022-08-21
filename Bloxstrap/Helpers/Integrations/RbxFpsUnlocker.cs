using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

using Newtonsoft.Json.Linq;

namespace Bloxstrap.Helpers.Integrations
{
    internal class RbxFpsUnlocker
    {
        public const string ProjectRepository = "axstin/rbxfpsunlocker";

        // default settings but with QuickStart set to true and CheckForUpdates set to false
        private static readonly string Settings = 
            "UnlockClient=true\n" +
            "UnlockStudio=false\n" +
            "FPSCapValues=[30.000000, 60.000000, 75.000000, 120.000000, 144.000000, 165.000000, 240.000000, 360.000000]\n" +
            "FPSCapSelection=0\n" +
            "FPSCap=0.000000\n" +
            "CheckForUpdates=false\n" +
            "NonBlockingErrors=true\n" +
            "SilentErrors=false\n" +
            "QuickStart=true\n";

        public static async Task CheckInstall()
        {
            if (Program.BaseDirectory is null)
                return;

            string folderLocation = Path.Combine(Program.BaseDirectory, "Integrations", "rbxfpsunlocker");
            string fileLocation = Path.Combine(folderLocation, "rbxfpsunlocker.exe");
            string settingsLocation = Path.Combine(folderLocation, "settings");

            if (!Program.Settings.RFUEnabled)
            {
                if (Directory.Exists(folderLocation))
                {
                    Directory.Delete(folderLocation, true);
                }

                return;
            }

            DateTime lastReleasePublish;
            string downloadUrl;

            try
            {
                JObject releaseInfo = await Utilities.GetJson($"https://api.github.com/repos/{ProjectRepository}/releases/latest");

                // so... rbxfpsunlocker does not actually have any version info for the executable
                // meaning the best way we can check for a new version is comparing time last download to time last release published
                lastReleasePublish = DateTime.Parse(releaseInfo["created_at"].Value<string>());
                downloadUrl = releaseInfo["assets"][0]["browser_download_url"].Value<string>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to fetch latest version info! ({ex.Message})");
                return;
            }

            Directory.CreateDirectory(folderLocation);

            if (File.Exists(fileLocation))
            {
                DateTime lastDownload = File.GetCreationTimeUtc(fileLocation);

                // no new release published, return
                if (lastDownload > lastReleasePublish)
                    return;

                File.Delete(fileLocation);
            }

            Debug.WriteLine("Installing/Updating rbxfpsunlocker...");

            using (HttpClient client = new())
            {
                byte[] bytes = await client.GetByteArrayAsync(downloadUrl);

                using (MemoryStream zipStream = new MemoryStream(bytes))
                {
                    ZipArchive zip = new ZipArchive(zipStream);
                    zip.ExtractToDirectory(folderLocation, true);
                }
            }

            if (!File.Exists(settingsLocation))
            {
                await File.WriteAllTextAsync(settingsLocation, Settings);
            }
        }
    }
}
