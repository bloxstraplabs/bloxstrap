using System.DirectoryServices;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Windows.Media.Animation;
using Bloxstrap.Resources;
using Microsoft.Win32;

namespace Bloxstrap
{
    internal class Installer
    {
        private static string DesktopShortcut => Path.Combine(Paths.Desktop, "Bloxstrap.lnk");

        private static string StartMenuShortcut => Path.Combine(Paths.WindowsStartMenu, "Bloxstrap.lnk");

        public string InstallLocation = Path.Combine(Paths.LocalAppData, "Bloxstrap");

        public bool CreateDesktopShortcuts = true;

        public bool CreateStartMenuShortcuts = true;

        public bool IsImplicitInstall = false;

        public string InstallLocationError { get; set; } = "";

        public void DoInstall()
        {
            // should've been created earlier from the write test anyway
            Directory.CreateDirectory(InstallLocation);

            Paths.Initialize(InstallLocation);

            if (!IsImplicitInstall)
            {
                Filesystem.AssertReadOnly(Paths.Application);
                File.Copy(Paths.Process, Paths.Application, true);
            }

            // TODO: registry access checks, i'll need to look back on issues to see what the error looks like
            using (var uninstallKey = Registry.CurrentUser.CreateSubKey(App.UninstallKey))
            {
                uninstallKey.SetValue("DisplayIcon", $"{Paths.Application},0");
                uninstallKey.SetValue("DisplayName", App.ProjectName);

                uninstallKey.SetValue("DisplayVersion", App.Version);

                if (uninstallKey.GetValue("InstallDate") is null)
                    uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

                uninstallKey.SetValue("InstallLocation", Paths.Base);
                uninstallKey.SetValue("NoRepair", 1);
                uninstallKey.SetValue("Publisher", "pizzaboxer");
                uninstallKey.SetValue("ModifyPath", $"\"{Paths.Application}\" -settings");
                uninstallKey.SetValue("QuietUninstallString", $"\"{Paths.Application}\" -uninstall -quiet");
                uninstallKey.SetValue("UninstallString", $"\"{Paths.Application}\" -uninstall");
                uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{App.ProjectRepository}");
                uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{App.ProjectRepository}/releases/latest");
            }

            // only register player, for the scenario where the user installs bloxstrap, closes it,
            // and then launches from the website expecting it to work
            // studio can be implicitly registered when it's first launched manually
            ProtocolHandler.Register("roblox", "Roblox", Paths.Application, "-player \"%1\"");
            ProtocolHandler.Register("roblox-player", "Roblox", Paths.Application, "-player \"%1\"");

            // TODO: implicit installation needs to reregister studio

            if (CreateDesktopShortcuts)
                Shortcut.Create(Paths.Application, "", DesktopShortcut);

            if (CreateStartMenuShortcuts)
                Shortcut.Create(Paths.Application, "", StartMenuShortcut);

            // existing configuration persisting from an earlier install
            App.Settings.Load();
            App.State.Load();
            App.FastFlags.Load();
        }

        private bool ValidateLocation()
        {
            // prevent from installing to the root of a drive
            if (InstallLocation.Length <= 3)
                return false;

            // unc path, just to be safe
            if (InstallLocation.StartsWith("\\\\"))
                return false;

            // prevent from installing to a onedrive folder
            if (InstallLocation.Contains("OneDrive", StringComparison.InvariantCultureIgnoreCase))
                return false;

            // prevent from installing to an essential user profile folder (e.g. Documents, Downloads, Contacts idk)
            if (String.Compare(Directory.GetParent(InstallLocation)?.FullName, Paths.UserProfile, StringComparison.InvariantCultureIgnoreCase) == 0)
                return false;

            return true;
        }

        public bool CheckInstallLocation()
        {
            if (string.IsNullOrEmpty(InstallLocation))
            {
                InstallLocationError = Strings.Menu_InstallLocation_NotSet;
            }
            else if (!ValidateLocation())
            {
                InstallLocationError = Strings.Menu_InstallLocation_CantInstall;
            }
            else
            {
                if (!IsImplicitInstall 
                    && !InstallLocation.EndsWith(App.ProjectName, StringComparison.InvariantCultureIgnoreCase)
                    && Directory.Exists(InstallLocation)
                    && Directory.EnumerateFileSystemEntries(InstallLocation).Any())
                {
                    string suggestedChange = Path.Combine(InstallLocation, App.ProjectName);

                    MessageBoxResult result = Frontend.ShowMessageBox(
                        String.Format(Strings.Menu_InstallLocation_NotEmpty, suggestedChange),
                        MessageBoxImage.Warning,
                        MessageBoxButton.YesNoCancel,
                        MessageBoxResult.Yes
                    );

                    if (result == MessageBoxResult.Yes)
                        InstallLocation = suggestedChange;
                    else if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                        return false;
                }

                try
                {
                    // check if we can write to the directory (a bit hacky but eh)
                    string testFile = Path.Combine(InstallLocation, $"{App.ProjectName}WriteTest.txt");

                    Directory.CreateDirectory(InstallLocation);
                    File.WriteAllText(testFile, "");
                    File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    InstallLocationError = Strings.Menu_InstallLocation_NoWritePerms;
                }
                catch (Exception ex)
                {
                    InstallLocationError = ex.Message;
                }
            }

            return String.IsNullOrEmpty(InstallLocationError);
        }

        public static void DoUninstall(bool keepData)
        {
            const string LOG_IDENT = "Installer::DoUninstall";

            var processes = new List<Process>();
            processes.AddRange(Process.GetProcessesByName(App.RobloxPlayerAppName));

#if STUDIO_FEATURES
            processes.AddRange(Process.GetProcessesByName(App.RobloxStudioAppName));
#endif

            // prompt to shutdown roblox if its currently running
            if (processes.Any())
            {
                var result = Frontend.ShowMessageBox(
                    Strings.Bootstrapper_Uninstall_RobloxRunning,
                    MessageBoxImage.Information,
                    MessageBoxButton.OKCancel,
                    MessageBoxResult.OK
                );

                if (result != MessageBoxResult.OK)
                    App.Terminate(ErrorCode.ERROR_CANCELLED);

                try
                {
                    foreach (var process in processes)
                    {
                        process.Kill();
                        process.Close();
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to close process! {ex}");
                }
            }

            string robloxFolder = Path.Combine(Paths.LocalAppData, "Roblox");
            bool playerStillInstalled = true;
            bool studioStillInstalled = true;

            // check if stock bootstrapper is still installed
            using var playerKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\roblox-player");
            var playerFolder = playerKey?.GetValue("InstallLocation");

            if (playerKey is null || playerFolder is not string)
            {
                playerStillInstalled = false;

                ProtocolHandler.Unregister("roblox");
                ProtocolHandler.Unregister("roblox-player");
            }
            else
            {
                // revert launch uri handler to stock bootstrapper
                string playerPath = Path.Combine((string)playerFolder, "RobloxPlayerBeta.exe");

                ProtocolHandler.Register("roblox", "Roblox", playerPath);
                ProtocolHandler.Register("roblox-player", "Roblox", playerPath);
            }

            using RegistryKey? studioBootstrapperKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\roblox-studio");
            if (studioBootstrapperKey is null)
            {
                studioStillInstalled = false;

#if STUDIO_FEATURES
                ProtocolHandler.Unregister("roblox-studio");
                ProtocolHandler.Unregister("roblox-studio-auth");

                ProtocolHandler.Unregister("Roblox.Place");
                ProtocolHandler.Unregister(".rbxl");
                ProtocolHandler.Unregister(".rbxlx");
#endif
            }
#if STUDIO_FEATURES
            else
            {
                string studioLocation = (string?)studioBootstrapperKey.GetValue("InstallLocation") + "RobloxStudioBeta.exe"; // points to studio exe instead of bootstrapper
                ProtocolHandler.Register("roblox-studio", "Roblox", studioLocation);
                ProtocolHandler.Register("roblox-studio-auth", "Roblox", studioLocation);

                ProtocolHandler.RegisterRobloxPlace(studioLocation);
            }
#endif



            var cleanupSequence = new List<Action>
            {
                () =>
                {
                    foreach (var file in Directory.GetFiles(Paths.Desktop).Where(x => x.EndsWith("lnk")))
                    {
                        var shortcut = ShellLink.Shortcut.ReadFromFile(file);

                        if (shortcut.ExtraData.EnvironmentVariableDataBlock?.TargetUnicode == Paths.Application)
                            File.Delete(file);
                    }
                },

                () => File.Delete(StartMenuShortcut),

                () => Directory.Delete(Paths.Versions, true),
                () => Directory.Delete(Paths.Downloads, true),
            };

            if (!keepData)
            {
                cleanupSequence.AddRange(new List<Action>
                {
                    () => Directory.Delete(Paths.Modifications, true),
                    () => Directory.Delete(Paths.Logs, true),

                    () => File.Delete(App.Settings.FileLocation),
                    () => File.Delete(App.State.FileLocation), // TODO: maybe this should always be deleted? not sure yet
                });
            }

            bool deleteFolder = false;

            if (Directory.Exists(Paths.Base))
            {
                var folderFiles = Directory.GetFiles(Paths.Base);
                deleteFolder = folderFiles.Length == 1 && folderFiles.First().EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase);
            }

            if (deleteFolder)
                cleanupSequence.Add(() => Directory.Delete(Paths.Base, true));

            if (!playerStillInstalled && !studioStillInstalled && Directory.Exists(robloxFolder))
                cleanupSequence.Add(() => Directory.Delete(robloxFolder, true));

            cleanupSequence.Add(() => Registry.CurrentUser.DeleteSubKey(App.UninstallKey));

            foreach (var process in cleanupSequence)
            {
                try
                {
                    process();
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Encountered exception when running cleanup sequence (#{cleanupSequence.IndexOf(process)})");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            if (Directory.Exists(Paths.Base))
            {
                // this is definitely one of the workaround hacks of all time

                string deleteCommand;

                if (deleteFolder)
                    deleteCommand = $"del /Q \"{Paths.Base}\\*\" && rmdir \"{Paths.Base}\"";
                else
                    deleteCommand = $"del /Q \"{Paths.Application}\"";

                Process.Start(new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c timeout 5 && {deleteCommand}",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
        }

        public static void HandleUpgrade()
        {
            const string LOG_IDENT = "Installer::HandleUpgrade";

            if (!File.Exists(Paths.Application) || Paths.Process == Paths.Application)
                return;

            // 2.0.0 downloads updates to <BaseFolder>/Updates so lol
            // TODO: 2.8.0 will download them to <Temp>/Bloxstrap/Updates
            bool isAutoUpgrade = Paths.Process.StartsWith(Path.Combine(Paths.Base, "Updates")) || Paths.Process.StartsWith(Path.Combine(Paths.LocalAppData, "Temp"));

            var existingVer = FileVersionInfo.GetVersionInfo(Paths.Application).ProductVersion;
            var currentVer = FileVersionInfo.GetVersionInfo(Paths.Process).ProductVersion;

            if (MD5Hash.FromFile(Paths.Process) == MD5Hash.FromFile(Paths.Application))
                return;

            // silently upgrade version if the command line flag is set or if we're launching from an auto update
            if (!App.LaunchSettings.UpgradeFlag.Active && !isAutoUpgrade)
            {
                var result = Frontend.ShowMessageBox(
                    Strings.InstallChecker_VersionDifferentThanInstalled,
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo
                );

                if (result != MessageBoxResult.Yes)
                    return;
            }

            Filesystem.AssertReadOnly(Paths.Application);

            // TODO: make this use a mutex somehow
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

            using (var uninstallKey = Registry.CurrentUser.CreateSubKey(App.UninstallKey))
            {
                uninstallKey.SetValue("DisplayVersion", App.Version);
            }

            // update migrations

            if (existingVer is not null)
            {
                if (Utilities.CompareVersions(existingVer, "2.5.0") == VersionComparison.LessThan)
                {
                    App.FastFlags.SetValue("DFFlagDisableDPIScale", null);
                    App.FastFlags.SetValue("DFFlagVariableDPIScale2", null);
                }

                if (Utilities.CompareVersions(existingVer, "2.5.1") == VersionComparison.LessThan)
                {
                    App.FastFlags.SetValue("FIntDebugForceMSAASamples", null);

                    if (App.FastFlags.GetPreset("UI.Menu.Style.DisableV2") is not null)
                        App.FastFlags.SetPreset("UI.Menu.Style.ABTest", false);
                }

                if (Utilities.CompareVersions(existingVer, "2.6.0") == VersionComparison.LessThan)
                {
                    if (App.Settings.Prop.UseDisableAppPatch)
                    {
                        try
                        {
                            File.Delete(Path.Combine(Paths.Modifications, "ExtraContent\\places\\Mobile.rbxl"));
                        }
                        catch (Exception ex)
                        {
                            App.Logger.WriteException(LOG_IDENT, ex);
                        }

                        App.Settings.Prop.EnableActivityTracking = true;
                    }

                    if (App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.ClassicFluentDialog)
                        App.Settings.Prop.BootstrapperStyle = BootstrapperStyle.FluentDialog;

                    _ = int.TryParse(App.FastFlags.GetPreset("Rendering.Framerate"), out int x);
                    if (x == 0)
                    {
                        App.FastFlags.SetPreset("Rendering.Framerate", null);
                    }
                }

                if (Utilities.CompareVersions(existingVer, "2.8.0") == VersionComparison.LessThan)
                {
                    string oldDesktopPath = Path.Combine(Paths.Desktop, "Play Roblox.lnk");
                    string oldStartPath = Path.Combine(Paths.WindowsStartMenu, "Bloxstrap");

                    if (File.Exists(oldDesktopPath))
                        File.Move(oldDesktopPath, DesktopShortcut);

                    if (Directory.Exists(oldStartPath))
                    {
                        try
                        {
                            Directory.Delete(oldStartPath, true);
                        }
                        catch (Exception ex)
                        {
                            App.Logger.WriteException(LOG_IDENT, ex);
                        }

                        Shortcut.Create(Paths.Application, "", StartMenuShortcut);
                    }

                    Registry.CurrentUser.DeleteSubKeyTree("Software\\Bloxstrap", false);

                    ProtocolHandler.Register("roblox", "Roblox", Paths.Application, "-player \"%1\"");
                    ProtocolHandler.Register("roblox-player", "Roblox", Paths.Application, "-player \"%1\"");
                }

                App.Settings.Save();
                App.FastFlags.Save();
            }

            if (isAutoUpgrade)
            {
                Utilities.ShellExecute($"https://github.com/{App.ProjectRepository}/wiki/Release-notes-for-Bloxstrap-v{currentVer}");
            }
            else
            {
                Frontend.ShowMessageBox(
                    string.Format(Strings.InstallChecker_Updated, currentVer),
                    MessageBoxImage.Information,
                    MessageBoxButton.OK
                );
            }
        }
    }
}
