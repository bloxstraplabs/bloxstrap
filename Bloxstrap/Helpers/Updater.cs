using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

using Bloxstrap.Properties;
using Bloxstrap.Views;
using System.Text.Json;
using System.Net;

namespace Bloxstrap.Helpers
{
    public class Updater
    {
        public static void CheckIsSuccessfulyUpdate()
        {
            string updatePath = Path.Combine(Directories.Base, "Bloxstrap-Update-Version.exe");

            if (!File.Exists(updatePath) || !File.Exists(Directories.Application))
                return;

            var applicationInfo = new FileInfo(Directories.Application);
            var updateInfo = new FileInfo(updatePath);

            if (applicationInfo.Length == updateInfo.Length)
            {
                App.ShowMessageBox("Successfully update Bloxstrap", MessageBoxImage.Information);
            }
            else
            {
                App.ShowMessageBox("Failed to update Bloxstrap", MessageBoxImage.Error);
            }


            File.Delete(updatePath);
        }

        public static async void CheckForUpdate()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            string url = $"https://api.github.com/repos/{App.ProjectRepository}/releases/latest";
            var response = await httpClient.GetAsync(url);
            var responseCOntent = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(responseCOntent);

            var tagValue = jsonDoc.RootElement.GetProperty("tag_name").GetString().Replace("v", "");

            if (tagValue == App.Version)
                return;

            MessageBoxResult result;

            if (App.IsUpgrade)
            {
                result = MessageBoxResult.Yes;
            }
            else
            {
                result = App.ShowMessageBox(
                    "Would you like to update to the latest version of Bloxstrap?",
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo
                );
            }

            if (result != MessageBoxResult.Yes)
                return;

            string fileName = "";

            /// Why i didn't do it in short hand if else
            if (IntPtr.Size == 8)
            {
                fileName = $"Bloxstrap-v{tagValue}-x64.exe";
            }
            else
            {
                fileName = $"Bloxstrap-v{tagValue}-x86.exe";
            }

            string downloadUrl = $"https://github.com/pizzaboxer/bloxstrap/releases/download/v{tagValue}/{fileName}";
            string downloadPath = Path.Combine(Directories.Base, "Bloxstrap-Update-Version.exe");

            WebClient client = new WebClient();
            client.DownloadFile(downloadUrl, downloadPath);
            App.Logger.WriteLine("[Updater::CheckForUpdate] Downloaded new bloxstrap version: " + downloadPath);
            App.Logger.WriteLine("[Updater::CheckForUpdate] Restarting bloxstrap to update...");
            
            /// Use ping command to wait 5 s before replace because it impossible to replace
            /// file without closing it process.
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/c ping 127.0.0.1 -n 5 > nul && copy /y {downloadPath} {Directories.Application}";
            ///startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            
            Process.Start(startInfo);
            App.Terminate();
        }
    }
}