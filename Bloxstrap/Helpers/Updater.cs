using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

using Bloxstrap.Properties;
using Bloxstrap.Views;

namespace Bloxstrap.Helpers
{
    public class Updater
    {
        public static void CheckInstalledVersion()
        {
            if (Environment.ProcessPath is null || !File.Exists(Directories.Application) || Environment.ProcessPath == Directories.Application)
                return;

            // 2.0.0 downloads updates to <BaseFolder>/Updates so lol
            bool isAutoUpgrade = Environment.ProcessPath.StartsWith(Path.Combine(Directories.Base, "Updates")) || Environment.ProcessPath.StartsWith(Path.Combine(Directories.LocalAppData, "Temp"));

            // if downloaded version doesn't match, replace installed version with downloaded version 
            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);
            FileVersionInfo installedVersionInfo = FileVersionInfo.GetVersionInfo(Directories.Application);

            if (installedVersionInfo.ProductVersion == currentVersionInfo.ProductVersion)
                return;

            MessageBoxResult result;

            // silently upgrade version if the command line flag is set or if we're launching from an auto update
            if (App.IsUpgrade || isAutoUpgrade)
            {
                result = MessageBoxResult.Yes;
            }
            else
            {
                result = App.ShowMessageBox(
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
                NotifyIcon notification = new()
                {
                    Icon = Resources.IconBloxstrap,
                    Text = "Bloxstrap",
                    Visible = true,
                    BalloonTipTitle = $"Bloxstrap has been upgraded to v{currentVersionInfo.ProductVersion}",
                    BalloonTipText = "Click here to see what's new in this version"
                };

                notification.BalloonTipClicked += (_, _) => Utilities.OpenWebsite($"https://github.com/{App.ProjectRepository}/releases/tag/v{currentVersionInfo.ProductVersion}");
                notification.ShowBalloonTip(30);

                Task.Run(() =>
                {
                    Task.Delay(30000).Wait();
                    notification.Dispose();
                });
            }
            else if (!App.IsQuiet)
            {
                App.ShowMessageBox(
                    $"{App.ProjectName} has been updated to v{currentVersionInfo.ProductVersion}",
                    MessageBoxImage.Information,
                    MessageBoxButton.OK
                );

				new MainWindow().ShowDialog();
				App.Terminate();
			}
        }
    }
}