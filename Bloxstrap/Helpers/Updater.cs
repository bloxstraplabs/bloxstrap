using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;

using Newtonsoft.Json.Linq;

using Bloxstrap.Models;

namespace Bloxstrap.Helpers
{
    public class Updater
    {
        public static void CheckInstalledVersion()
        {
            if (Environment.ProcessPath is null || !File.Exists(Directories.App) || Environment.ProcessPath == Directories.App)
                return;

            // if downloaded version doesn't match, replace installed version with downloaded version 
            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);
            FileVersionInfo installedVersionInfo = FileVersionInfo.GetVersionInfo(Directories.App);

            if (installedVersionInfo.ProductVersion != currentVersionInfo.ProductVersion)
            {
                DialogResult result = Program.ShowMessageBox(
                    $"The version of {Program.ProjectName} you've launched is different to the version you currently have installed.\nWould you like to update your currently installed version?",
                    MessageBoxIcon.Question,
                    MessageBoxButtons.YesNo
                );

                if (result == DialogResult.Yes)
                {
                    File.Delete(Directories.App);
                    File.Copy(Environment.ProcessPath, Directories.App);

                    Program.ShowMessageBox(
                        $"{Program.ProjectName} has been updated to v{currentVersionInfo.ProductVersion}",
                        MessageBoxIcon.Information,
                        MessageBoxButtons.OK
                    );

                    Environment.Exit(0);
                }
            }

            return;
        }

        public static async Task Check()
        {
            if (Environment.ProcessPath is null)
                return;

            if (!Program.IsFirstRun)
                CheckInstalledVersion();

            if (!Program.Settings.CheckForUpdates)
                return;

            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);
            string currentVersion = $"Bloxstrap v{currentVersionInfo.ProductVersion}";
            string latestVersion;
            string releaseNotes;

            var releaseInfo = await Utilities.GetJson<GithubRelease>($"https://api.github.com/repos/{Program.ProjectRepository}/releases/latest");

            if (releaseInfo is null || releaseInfo.Name is null || releaseInfo.Body is null)
                return;

            latestVersion = releaseInfo.Name;
            releaseNotes = releaseInfo.Body;

            if (currentVersion != latestVersion)
            {
                DialogResult result = Program.ShowMessageBox(
                    $"A new version of {Program.ProjectName} is available\n\n[{latestVersion}]\n{releaseNotes}\n\nWould you like to download it?",
                    MessageBoxIcon.Question,
                    MessageBoxButtons.YesNo
                ); 

                if (result == DialogResult.Yes)
                {
                    Utilities.OpenWebsite($"https://github.com/{Program.ProjectRepository}/releases/latest");
                    Program.Exit();
                }
            }
        }
    }
}