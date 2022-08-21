using System.Diagnostics;
using System.IO;

using Newtonsoft.Json.Linq;

namespace Bloxstrap.Helpers
{
    public class Updater
    {
        public static bool CheckInstalledVersion()
        {
            if (Environment.ProcessPath is null || !File.Exists(Directories.App) || Environment.ProcessPath == Directories.App)
                return false;

            // if downloaded version doesn't match, replace installed version with downloaded version 
            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);
            FileVersionInfo installedVersionInfo = FileVersionInfo.GetVersionInfo(Directories.App);

            if (installedVersionInfo.ProductVersion != currentVersionInfo.ProductVersion)
            {
                DialogResult result = MessageBox.Show(
                    $"The version of {Program.ProjectName} you've launched is newer than the version you currently have installed.\nWould you like to update your currently installed version?",
                    Program.ProjectName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    File.Delete(Directories.App);
                    File.Copy(Environment.ProcessPath, Directories.App);
                    return true;
                }
            }

            return false;
        }

        public static async Task Check()
        {
            if (Environment.ProcessPath is null)
                return;

            if (!Program.IsFirstRun && CheckInstalledVersion())
                return;

            if (!Program.Settings.CheckForUpdates)
                return;

            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);
            string currentVersion = $"Bloxstrap v{currentVersionInfo.ProductVersion}";
            string latestVersion;
            string releaseNotes;

            // get the latest version according to the latest github release info
            // it should contain the latest product version, which we can check against
            try
            {
                JObject releaseInfo = await Utilities.GetJson($"https://api.github.com/repos/{Program.ProjectRepository}/releases/latest");

                latestVersion = releaseInfo["name"].Value<string>();
                releaseNotes = releaseInfo["body"].Value<string>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to fetch latest version info! ({ex.Message})");
                return;
            }

            if (currentVersion != latestVersion)
            {
                DialogResult result = MessageBox.Show(
                    $"A new version of {Program.ProjectName} is available\n\n[{latestVersion}]\n{releaseNotes}\n\nWould you like to download it?",
                    Program.ProjectName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
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