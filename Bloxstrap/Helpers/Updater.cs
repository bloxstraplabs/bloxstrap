using System.Diagnostics;
using System.IO;

using Bloxstrap.Dialogs.Menu;
using System.Windows;
using System;

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

            File.Delete(Directories.App);
            File.Copy(Environment.ProcessPath, Directories.App);
                
            Bootstrapper.Register();
                
            if (App.IsQuiet || isAutoUpgrade)
                return;
                
            App.ShowMessageBox(
                $"{App.ProjectName} has been updated to v{currentVersionInfo.ProductVersion}",
                MessageBoxImage.Information,
                MessageBoxButton.OK
            );

            new Preferences().ShowDialog();
            App.Terminate();
        }
    }
}