using Microsoft.Win32;
using Bloxstrap.Helpers;

namespace Bloxstrap
{
    partial class Bootstrapper
    {
        public static void Register()
        {
            if (Program.BaseDirectory is null)
                return;

            RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{Program.ProjectName}");

            // new install location selected, delete old one
            string? oldInstallLocation = (string?)applicationKey.GetValue("OldInstallLocation");
            if (!String.IsNullOrEmpty(oldInstallLocation) && oldInstallLocation != Program.BaseDirectory)
            {
                try
                {
                    if (Directory.Exists(oldInstallLocation))
                        Directory.Delete(oldInstallLocation, true);
                }
                catch (Exception) { }

                applicationKey.DeleteValue("OldInstallLocation");
            }

            applicationKey.SetValue("InstallLocation", Program.BaseDirectory);
            applicationKey.Close();

            // set uninstall key
            RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Program.ProjectName}");
            uninstallKey.SetValue("DisplayIcon", $"{Program.FilePath},0");
            uninstallKey.SetValue("DisplayName", Program.ProjectName);
            uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
            uninstallKey.SetValue("InstallLocation", Program.BaseDirectory);
            uninstallKey.SetValue("NoRepair", 1);
            uninstallKey.SetValue("Publisher", Program.ProjectName);
            uninstallKey.SetValue("ModifyPath", $"\"{Program.FilePath}\" -preferences");
            uninstallKey.SetValue("UninstallString", $"\"{Program.FilePath}\" -uninstall");
            uninstallKey.Close();
        }

        public static void CheckInstall()
        {
            // check if launch uri is set to our bootstrapper
            // this doesn't go under register, so we check every launch
            // just in case the stock bootstrapper changes it back

            Protocol.Register("roblox", "Roblox", Program.FilePath);
            Protocol.Register("roblox-player", "Roblox", Program.FilePath);

            // in case the user is reinstalling
            if (File.Exists(Program.FilePath) && Program.IsFirstRun)
                File.Delete(Program.FilePath);

            // check to make sure bootstrapper is in the install folder
            if (!File.Exists(Program.FilePath) && Environment.ProcessPath is not null)
                File.Copy(Environment.ProcessPath, Program.FilePath);

            // this SHOULD go under Register(),
            // but then people who have Bloxstrap v1.0.0 installed won't have this without a reinstall
            // maybe in a later version?
            if (!Directory.Exists(Program.StartMenuDirectory))
            {
                Directory.CreateDirectory(Program.StartMenuDirectory);

                ShellLink.Shortcut.CreateShortcut(Program.FilePath, "", Program.FilePath, 0)
                    .WriteToFile(Path.Combine(Program.StartMenuDirectory, "Play Roblox.lnk"));

                ShellLink.Shortcut.CreateShortcut(Program.FilePath, "-preferences", Program.FilePath, 0)
                    .WriteToFile(Path.Combine(Program.StartMenuDirectory, "Configure Bloxstrap.lnk"));
            }
        }

        private void Uninstall()
        {
            if (Program.BaseDirectory is null)
                return;

            CheckIfRunning();

            Message = $"Uninstalling {Program.ProjectName}...";

            Program.SettingsManager.ShouldSave = false;

            // check if stock bootstrapper is still installed
            RegistryKey? bootstrapperKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\roblox-player");
            if (bootstrapperKey is null)
            {
                Protocol.Unregister("roblox");
                Protocol.Unregister("roblox-player");
            }
            else
            {
                // revert launch uri handler to stock bootstrapper

                string bootstrapperLocation = (string?)bootstrapperKey.GetValue("InstallLocation") + "RobloxPlayerLauncher.exe";

                Protocol.Register("roblox", "Roblox", bootstrapperLocation);
                Protocol.Register("roblox-player", "Roblox", bootstrapperLocation);
            }

            try
            {
                // delete application key
                Registry.CurrentUser.DeleteSubKey($@"Software\{Program.ProjectName}");

                // delete start menu folder
                Directory.Delete(Program.StartMenuDirectory, true);

                // delete uninstall key
                Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Program.ProjectName}");

                // delete installation folder
                // (should delete everything except bloxstrap itself)
                Directory.Delete(Program.BaseDirectory, true);
            }
            catch (Exception) { }

            ShowSuccess($"{Program.ProjectName} has been uninstalled");
        }
    }
}
