using System.Windows;

using Microsoft.Win32;

namespace Bloxstrap
{
    internal class InstallChecker : IDisposable
    {
        private RegistryKey? _registryKey;
        private string? _installLocation;

        internal InstallChecker()
        {
            _registryKey = Registry.CurrentUser.OpenSubKey($"Software\\{App.ProjectName}", true);

            if (_registryKey is not null)
                _installLocation = (string?)_registryKey.GetValue("InstallLocation");
        }

        internal void Check()
        {
            const string LOG_IDENT = "InstallChecker::Check";

            if (_registryKey is null || _installLocation is null)
            {
                if (!File.Exists("Settings.json") || !File.Exists("State.json"))
                {
                    FirstTimeRun();
                    return;
                }

                _installLocation = Path.GetDirectoryName(Paths.Process)!;

                App.Logger.WriteLine(LOG_IDENT, $"Registry key is likely malformed. Setting install location as '{_installLocation}'");

                if (_registryKey is null)
                    _registryKey = Registry.CurrentUser.CreateSubKey($"Software\\{App.ProjectName}");

                _registryKey.SetValue("InstallLocation", _installLocation);
            }

            // check if drive that bloxstrap was installed to was removed from system, or had its drive letter changed

            if (!Directory.Exists(_installLocation))
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not find install location. Checking if drive has changed...");

                bool driveExists = false;
                string driveName = _installLocation[..3];
                string? newDriveName = null;

                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.Name == driveName)
                        driveExists = true;
                    else if (Directory.Exists(_installLocation.Replace(driveName, drive.Name)))
                        newDriveName = drive.Name;
                }

                if (newDriveName is not null)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Drive has changed from {driveName} to {newDriveName}");

                    Controls.ShowMessageBox(
                        $"{App.ProjectName} has detected a drive letter change and has reconfigured its install location from the {driveName} drive to the {newDriveName} drive.\n" +
                        "\n" +
                        $"While {App.ProjectName} will continue to work, it's recommended that you change the drive leter back to its original value as other installed applications can experience similar issues.",
                        MessageBoxImage.Warning,
                        MessageBoxButton.OK
                    );

                    _installLocation = _installLocation.Replace(driveName, newDriveName);
                    _registryKey.SetValue("InstallLocation", _installLocation);
                }
                else if (!driveExists)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Drive {driveName} does not exist anymore, and has likely been removed");

                    var result = Controls.ShowMessageBox(
                        $"{App.ProjectName} was originally installed to the {driveName} drive, but it appears to no longer be present. Would you like to continue and carry out a fresh install?",
                        MessageBoxImage.Warning,
                        MessageBoxButton.OKCancel
                    );

                    if (result != MessageBoxResult.OK)
                        App.Terminate();

                    FirstTimeRun();
                    return;
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, "Drive has not changed, folder was likely moved or deleted");
                }
            }

            App.BaseDirectory = _installLocation;
            App.IsFirstRun = false;
        }

        public void Dispose()
        {
            _registryKey?.Dispose();
            GC.SuppressFinalize(this);
        }

        private static void FirstTimeRun()
        {
            const string LOG_IDENT = "InstallChecker::FirstTimeRun";

            App.Logger.WriteLine(LOG_IDENT, "Running first-time install");

            App.BaseDirectory = Path.Combine(Paths.LocalAppData, App.ProjectName);
            App.Logger.Initialize(true);

            if (App.IsQuiet)
                return;

            App.IsSetupComplete = false;

            App.FastFlags.Load();
            Controls.ShowMenu();

            // exit if we don't click the install button on installation
            if (App.IsSetupComplete)
                return;
             
            App.Logger.WriteLine(LOG_IDENT, "Installation cancelled!");
            App.Terminate(ErrorCode.ERROR_CANCELLED);
        }

        internal static void CheckUpgrade()
        {
            const string LOG_IDENT = "InstallChecker::CheckUpgrade";

            if (!File.Exists(Paths.Application) || Paths.Process == Paths.Application)
                return;

            // 2.0.0 downloads updates to <BaseFolder>/Updates so lol
            bool isAutoUpgrade = Paths.Process.StartsWith(Path.Combine(Paths.Base, "Updates")) || Paths.Process.StartsWith(Path.Combine(Paths.LocalAppData, "Temp"));

            FileVersionInfo existingVersionInfo = FileVersionInfo.GetVersionInfo(Paths.Application);
            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(Paths.Process);

            if (MD5Hash.FromFile(Paths.Process) == MD5Hash.FromFile(Paths.Application))
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

            Filesystem.AssertReadOnly(Paths.Application);

            // yes, this is EXTREMELY hacky, but the updater process that launched the
            // new version may still be open and so we have to wait for it to close
            int attempts = 0;
            while (attempts < 10)
            {
                attempts++;

                try
                {
                    File.Delete(Paths.Application);
                    break;
                }
                catch (Exception)
                {
                    if (attempts == 1)
                        App.Logger.WriteLine(LOG_IDENT, "Waiting for write permissions to update version");

                    Thread.Sleep(500);
                }
            }

            if (attempts == 10)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to update! (Could not get write permissions after 5 seconds)");
                return;
            }

            File.Copy(Paths.Process, Paths.Application);

            Bootstrapper.Register();

            // update migrations

            if (App.BuildMetadata.CommitRef.StartsWith("tag") && existingVersionInfo.ProductVersion == "2.4.0")
            {
                App.FastFlags.SetValue("DFFlagDisableDPIScale", null);
                App.FastFlags.SetValue("DFFlagVariableDPIScale2", null);
                App.FastFlags.Save();
            }

            if (isAutoUpgrade)
            {
                App.NotifyIcon?.ShowAlert(
                    $"{App.ProjectName} has been upgraded to v{currentVersionInfo.ProductVersion}",
                    "See what's new in this version",
                    30,
                    (_, _) => Utilities.ShellExecute($"https://github.com/{App.ProjectRepository}/releases/tag/v{currentVersionInfo.ProductVersion}")
                );
            }
            else if (!App.IsQuiet)
            {
                Controls.ShowMessageBox(
                    $"{App.ProjectName} has been upgraded to v{currentVersionInfo.ProductVersion}",
                    MessageBoxImage.Information,
                    MessageBoxButton.OK
                );

                Controls.ShowMenu();

                App.Terminate();
            }
        }
    }
}
