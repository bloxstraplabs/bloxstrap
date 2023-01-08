using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;

using Newtonsoft.Json.Linq;

using Bloxstrap.Models;
using Bloxstrap.Dialogs;

namespace Bloxstrap.Helpers
{
    public class Updater
    {
        public static void CheckInstalledVersion()
        {
            if (Environment.ProcessPath is null || !File.Exists(Directories.App) || Environment.ProcessPath == Directories.App)
                return;

            bool isAutoUpgrade = Environment.ProcessPath.StartsWith(Directories.Updates);

            // if downloaded version doesn't match, replace installed version with downloaded version 
            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);
            FileVersionInfo installedVersionInfo = FileVersionInfo.GetVersionInfo(Directories.App);

            if (installedVersionInfo.ProductVersion == currentVersionInfo.ProductVersion)
                return;


            DialogResult result;

            // silently upgrade version if the command line flag is set or if we're launching from an auto update
            if (Program.IsUpgrade || isAutoUpgrade)
            {
                result = DialogResult.Yes;
            }
            else
            {
                result = Program.ShowMessageBox(
                    $"The version of {Program.ProjectName} you've launched is different to the version you currently have installed.\nWould you like to upgrade your currently installed version?",
                    MessageBoxIcon.Question,
                    MessageBoxButtons.YesNo
                );
            }


            if (result != DialogResult.Yes)
                return;

            File.Delete(Directories.App);
            File.Copy(Environment.ProcessPath, Directories.App);
                
            Bootstrapper.Register();
                
            if (Program.IsQuiet || isAutoUpgrade)
                return;
                
            Program.ShowMessageBox(
                $"{Program.ProjectName} has been updated to v{currentVersionInfo.ProductVersion}",
                MessageBoxIcon.Information,
                MessageBoxButtons.OK
            );

            new Preferences().ShowDialog();
            Program.Exit();
        }
    }
}