using System.Windows;

namespace Bloxstrap
{
    public class Updater
    {
        public static void CheckInstalledVersion()
        {
            if (Environment.ProcessPath is null || !File.Exists(Directories.Application) || Environment.ProcessPath == Directories.Application)
                return;

            // 2.0.0 downloads updates to <BaseFolder>/Updates so lol
            bool isAutoUpgrade = Environment.ProcessPath.StartsWith(Path.Combine(Directories.Base, "Updates")) || Environment.ProcessPath.StartsWith(Path.Combine(Directories.LocalAppData, "Temp"));

            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);

            if (MD5Hash.FromFile(Environment.ProcessPath) == MD5Hash.FromFile(Directories.Application))
                return;

            MessageBoxResult result;

            // silently upgrade version if the command line flag is set or if we're launching from an auto update
            if (App.IsUpgrade || isAutoUpgrade)
            {
                result = MessageBoxResult.Yes;
            }
            else
            {
                result = Controls.ShowMessageBox(
                    $"The version of {App.ProjectName} you've launched is different to the version you currently have installed.\nWould you like to upgrade your currently installed version?",
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo
                );
            }

            if (result != MessageBoxResult.Yes)
                return;

            // yes, this is EXTREMELY hacky, but the updater process that launched the
            // new version may still be open and so we have to wait for it to close
            int attempts = 0;
            while (attempts < 10)
            {
                attempts++;

                try
                {
                    File.Delete(Directories.Application);
                    break;
                }
                catch (Exception)
                {
                    if (attempts == 1)
                        App.Logger.WriteLine("[Updater::CheckInstalledVersion] Waiting for write permissions to update version");

                    Thread.Sleep(500);
                }
            }

            if (attempts == 10)
            {
                App.Logger.WriteLine("[Updater::CheckInstalledVersion] Failed to update! (Could not get write permissions after 5 seconds)");
                return;
            }

            File.Copy(Environment.ProcessPath, Directories.Application);

            Bootstrapper.Register();

            if (isAutoUpgrade)
            {
                App.NotifyIcon?.ShowAlert(
                    $"Bloxstrap has been upgraded to v{currentVersionInfo.ProductVersion}", 
                    "See what's new in this version", 
                    30, 
                    (_, _) => Utilities.ShellExecute($"https://github.com/{App.ProjectRepository}/releases/tag/v{currentVersionInfo.ProductVersion}")
                );
            }
            else if (!App.IsQuiet)
            {
                Controls.ShowMessageBox(
                    $"{App.ProjectName} has been updated to v{currentVersionInfo.ProductVersion}",
                    MessageBoxImage.Information,
                    MessageBoxButton.OK
                );

                Controls.ShowMenu();

                App.Terminate();
            }
        }
    }
}