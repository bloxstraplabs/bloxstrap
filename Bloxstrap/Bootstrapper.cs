using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

using Microsoft.Win32;

using Bloxstrap.Enums;
using Bloxstrap.Dialogs.BootstrapperDialogs;
using Bloxstrap.Helpers;
using Bloxstrap.Helpers.Integrations;
using Bloxstrap.Helpers.RSMM;
using Bloxstrap.Models;
using System.Net;
using Bloxstrap.Properties;

namespace Bloxstrap
{
    public partial class Bootstrapper
    {
        #region Properties

        // https://learn.microsoft.com/en-us/windows/win32/msi/error-codes
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_INSTALL_USEREXIT = 1602;
        public const int ERROR_INSTALL_FAILURE = 1603;
        public const int ERROR_PRODUCT_UNINSTALLED = 1614;

        // in case a new package is added, you can find the corresponding directory
        // by opening the stock bootstrapper in a hex editor
        // TODO - there ideally should be a less static way to do this that's not hardcoded?
        private static readonly IReadOnlyDictionary<string, string> PackageDirectories = new Dictionary<string, string>()
        {
            { "RobloxApp.zip",                 @"" },
            { "shaders.zip",                   @"shaders\" },
            { "ssl.zip",                       @"ssl\" },

            { "content-avatar.zip",            @"content\avatar\" },
            { "content-configs.zip",           @"content\configs\" },
            { "content-fonts.zip",             @"content\fonts\" },
            { "content-sky.zip",               @"content\sky\" },
            { "content-sounds.zip",            @"content\sounds\" },
            { "content-textures2.zip",         @"content\textures\" },
            { "content-models.zip",            @"content\models\" },

            { "content-textures3.zip",         @"PlatformContent\pc\textures\" },
            { "content-terrain.zip",           @"PlatformContent\pc\terrain\" },
            { "content-platform-fonts.zip",    @"PlatformContent\pc\fonts\" },

            { "extracontent-luapackages.zip",  @"ExtraContent\LuaPackages\" },
            { "extracontent-translations.zip", @"ExtraContent\translations\" },
            { "extracontent-models.zip",       @"ExtraContent\models\" },
            { "extracontent-textures.zip",     @"ExtraContent\textures\" },
            { "extracontent-places.zip",       @"ExtraContent\places\" },
        };

        private static readonly string AppSettings =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<Settings>\n" +
            "	<ContentFolder>content</ContentFolder>\n" +
            "	<BaseUrl>http://www.roblox.com</BaseUrl>\n" +
            "</Settings>\n";

        private static readonly string ModReadme =
            "This is where you can modify your Roblox files while preserving modifications\n" +
            "whenever Roblox updates.\n" +
            "\n" +
            "For example, Modifications\\content\\sounds\\ouch.ogg will\n" +
            "automatically overwrite Versions\\version-xx...\\content\\sounds\\ouch.ogg\n" +
            "\n" +
            "If you remove a file mod from here, Bloxstrap will restore the stock version\n" +
            "of the file the next time it's launched.\n" +
            "\n" +
            "Any files added here to the root modification directory are ignored, so be sure\n" +
            "that they're inside a folder.";

        private string? LaunchCommandLine;

        private string VersionGuid = null!;
        private PackageManifest VersionPackageManifest = null!;
        private string VersionFolder = null!;

        private readonly bool FreshInstall;

        private double ProgressIncrement;
        private long TotalBytes = 0;
        private long TotalDownloadedBytes = 0;
        private int PackagesExtracted = 0;
        private bool CancelFired = false;

        public IBootstrapperDialog Dialog = null!;
        #endregion

        #region Core
        public Bootstrapper(string? launchCommandLine = null)
        {
            LaunchCommandLine = launchCommandLine;
            FreshInstall = String.IsNullOrEmpty(Program.Settings.VersionGuid);
        }

        // this is called from BootstrapperStyleForm.SetupDialog()
        public async Task Run()
        {
            if (Program.IsQuiet)
                Dialog.CloseDialog();

            if (Program.IsUninstall)
            {
                Uninstall();
                return;
            }

#if !DEBUG
            if (!Program.IsFirstRun && Program.Settings.CheckForUpdates)
                await CheckForUpdates();
#endif

            await CheckLatestVersion();

            // if bloxstrap is installing for the first time but is running, prompt to close roblox
            // if roblox needs updating but is running, ignore update for now
            if (!Directory.Exists(VersionFolder) && CheckIfRunning(true) || Program.Settings.VersionGuid != VersionGuid && !CheckIfRunning(false))
                await InstallLatestVersion();

            await ApplyModifications();

            if (Program.IsFirstRun)
                Program.SettingsManager.ShouldSave = true;

            if (Program.IsFirstRun || FreshInstall)
                Register();

            CheckInstall();

            await RbxFpsUnlocker.CheckInstall();
            
            Program.SettingsManager.Save();

            if (Program.IsFirstRun && Program.IsNoLaunch)
                Dialog.ShowSuccess($"{Program.ProjectName} has successfully installed");
            else if (!Program.IsNoLaunch)
                await StartRoblox();
        }

        private async Task CheckForUpdates()
        {
            string currentVersion = $"Bloxstrap v{Program.Version}";

            var releaseInfo = await Utilities.GetJson<GithubRelease>($"https://api.github.com/repos/{Program.ProjectRepository}/releases/latest");

            if (releaseInfo is null || releaseInfo.Name is null || releaseInfo.Assets is null || currentVersion == releaseInfo.Name)
                return;

            Dialog.Message = "Getting the latest Bloxstrap...";

            // 64-bit is always the first option
            GithubReleaseAsset asset = releaseInfo.Assets[Environment.Is64BitOperatingSystem ? 0 : 1];
            string downloadLocation = Path.Combine(Directories.Updates, asset.Name);

            Directory.CreateDirectory(Directories.Updates);

            Debug.WriteLine($"Downloading {releaseInfo.Name}...");

            if (!File.Exists(downloadLocation))
            {
                var response = await Program.HttpClient.GetAsync(asset.BrowserDownloadUrl);

                using (var fileStream = new FileStream(Path.Combine(Directories.Updates, asset.Name), FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }

            Debug.WriteLine($"Starting {releaseInfo.Name}...");

            ProcessStartInfo startInfo = new()
            {
                FileName = downloadLocation,
            };

            foreach (string arg in Program.LaunchArgs)
                startInfo.ArgumentList.Add(arg);

            Program.SettingsManager.Save();

            Process.Start(startInfo);

            Environment.Exit(0);
        }

        private async Task CheckLatestVersion()
        {
            Dialog.Message = "Connecting to Roblox...";

            ClientVersion clientVersion = await DeployManager.GetLastDeploy(Program.Settings.Channel);
            VersionGuid = clientVersion.VersionGuid;
            VersionFolder = Path.Combine(Directories.Versions, VersionGuid);
            VersionPackageManifest = await PackageManifest.Get(VersionGuid);
        }

        private bool CheckIfRunning(bool shutdown)
        {
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");

            if (processes.Length == 0)
                return false;

            if (shutdown)
            {
                Dialog.PromptShutdown();

                try
                {
                    // try/catch just in case process was closed before prompt was answered

                    foreach (Process process in processes)
                    {
                        process.CloseMainWindow();
                        process.Close();
                    }
                }
                catch (Exception) { }
            }

            return true;
        }

        private async Task StartRoblox()
        {
            string startEventName = Program.ProjectName.Replace(" ", "") + "StartEvent";

            Dialog.Message = "Starting Roblox...";

            if (LaunchCommandLine == "--app" && Program.Settings.UseDisableAppPatch)
            {
                Utilities.OpenWebsite("https://www.roblox.com/games");
                return;
            }

            // launch time isn't really required for all launches, but it's usually just safest to do this
            LaunchCommandLine += " --launchtime=" + DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (Program.Settings.Channel.ToLower() != DeployManager.DefaultChannel.ToLower())
                LaunchCommandLine += " -channel " + Program.Settings.Channel.ToLower();

            LaunchCommandLine  += " -startEvent " + startEventName;

            using (SystemEvent startEvent = new(startEventName))
            {
                bool shouldWait = false;

                Process gameClient = Process.Start(Path.Combine(VersionFolder, "RobloxPlayerBeta.exe"), LaunchCommandLine);
                Process? rbxFpsUnlocker = null;
                DiscordRichPresence? richPresence = null;

                bool startEventFired = await startEvent.WaitForEvent();

                startEvent.Close();

                if (!startEventFired)
                    return;

                if (Program.Settings.RFUEnabled && Process.GetProcessesByName("rbxfpsunlocker").Length == 0)
                {
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = Path.Combine(Directories.Integrations, @"rbxfpsunlocker\rbxfpsunlocker.exe"),
                        WorkingDirectory = Path.Combine(Directories.Integrations, "rbxfpsunlocker")
                    };

                    rbxFpsUnlocker = Process.Start(startInfo);

                    if (Program.Settings.RFUAutoclose)
                        shouldWait = true;
                }

                // event fired, wait for 3 seconds then close
                await Task.Delay(3000);

                // now we move onto handling rich presence
                if (Program.Settings.UseDiscordRichPresence)
                {
                    richPresence = new DiscordRichPresence();
                    richPresence.MonitorGameActivity();

                    shouldWait = true;
                }

                if (!shouldWait)
                    return;

                // keep bloxstrap open in the background
                Dialog.CloseDialog();
                await gameClient.WaitForExitAsync();

                if (richPresence is not null)
                    richPresence.Dispose();

                if (Program.Settings.RFUAutoclose && rbxFpsUnlocker is not null)
                    rbxFpsUnlocker.Kill();
            }
        }

        public void CancelButtonClicked()
        {
            if (!Dialog.CancelEnabled)
            {
                Program.Exit(ERROR_INSTALL_USEREXIT);
                return;
            }

            CancelFired = true;

            try
            {
                if (Program.IsFirstRun)
                    Directory.Delete(Directories.Base, true);
                else if (Directory.Exists(VersionFolder))
                    Directory.Delete(VersionFolder, true);
            }
            catch (Exception) { }
 
            Program.Exit(ERROR_INSTALL_USEREXIT);
        }
#endregion

#region App Install
        public static void Register()
        {
            RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{Program.ProjectName}");

            // new install location selected, delete old one
            string? oldInstallLocation = (string?)applicationKey.GetValue("OldInstallLocation");
            if (!String.IsNullOrEmpty(oldInstallLocation) && oldInstallLocation != Directories.Base)
            {
                try
                {
                    if (Directory.Exists(oldInstallLocation))
                        Directory.Delete(oldInstallLocation, true);
                }
                catch (Exception) { }

                applicationKey.DeleteValue("OldInstallLocation");
            }

            applicationKey.SetValue("InstallLocation", Directories.Base);
            applicationKey.Close();

            // set uninstall key
            RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Program.ProjectName}");
            uninstallKey.SetValue("DisplayIcon", $"{Directories.App},0");
            uninstallKey.SetValue("DisplayName", Program.ProjectName);
            uninstallKey.SetValue("DisplayVersion", Program.Version);

            if (uninstallKey.GetValue("InstallDate") is null)
                uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

            uninstallKey.SetValue("InstallLocation", Directories.Base);
            uninstallKey.SetValue("NoRepair", 1);
            uninstallKey.SetValue("Publisher", "pizzaboxer");
            uninstallKey.SetValue("ModifyPath", $"\"{Directories.App}\" -preferences");
            uninstallKey.SetValue("QuietUninstallString", $"\"{Directories.App}\" -uninstall -quiet");
            uninstallKey.SetValue("UninstallString", $"\"{Directories.App}\" -uninstall");
            uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{Program.ProjectRepository}");
            uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{Program.ProjectRepository}/releases/latest");
            uninstallKey.Close();
        }

        public static void CheckInstall()
        {
            // check if launch uri is set to our bootstrapper
            // this doesn't go under register, so we check every launch
            // just in case the stock bootstrapper changes it back

            Protocol.Register("roblox", "Roblox", Directories.App);
            Protocol.Register("roblox-player", "Roblox", Directories.App);

            // in case the user is reinstalling
            if (File.Exists(Directories.App) && Program.IsFirstRun)
                File.Delete(Directories.App);

            // check to make sure bootstrapper is in the install folder
            if (!File.Exists(Directories.App) && Environment.ProcessPath is not null)
                File.Copy(Environment.ProcessPath, Directories.App);

            // this SHOULD go under Register(),
            // but then people who have Bloxstrap v1.0.0 installed won't have this without a reinstall
            // maybe in a later version?
            if (!Directory.Exists(Program.StartMenu))
            {
                Directory.CreateDirectory(Program.StartMenu);

                ShellLink.Shortcut.CreateShortcut(Directories.App, "", Directories.App, 0)
                    .WriteToFile(Path.Combine(Program.StartMenu, "Play Roblox.lnk"));

                ShellLink.Shortcut.CreateShortcut(Directories.App, "-preferences", Directories.App, 0)
                    .WriteToFile(Path.Combine(Program.StartMenu, $"Configure {Program.ProjectName}.lnk"));
            }
        }

        private void Uninstall()
        {
            CheckIfRunning(true);

            Dialog.Message = $"Uninstalling {Program.ProjectName}...";

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
                Directory.Delete(Program.StartMenu, true);

                // delete uninstall key
                Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Program.ProjectName}");

                // delete installation folder
                // (should delete everything except bloxstrap itself)
                Directory.Delete(Directories.Base, true);
            }
            catch (Exception) { }

            Dialog.ShowSuccess($"{Program.ProjectName} has succesfully uninstalled");

            Program.Exit();
        }
#endregion

#region Roblox Install
        private void UpdateProgressbar()
        {
            int newProgress = (int)Math.Floor(ProgressIncrement * TotalDownloadedBytes);
            Dialog.ProgressValue = newProgress;
        }

        private async Task InstallLatestVersion()
        {
            if (FreshInstall)
                Dialog.Message = "Installing Roblox...";
            else
                Dialog.Message = "Upgrading Roblox...";

            // check if we have at least 300 megabytes of free disk space
            if (Utilities.GetFreeDiskSpace(Directories.Base) < 1024*1024*300)
            {
                Program.ShowMessageBox($"{Program.ProjectName} requires at least 300 MB of disk space to install Roblox. Please free up some disk space and try again.", MessageBoxIcon.Error);
                Program.Exit(ERROR_INSTALL_FAILURE);
                return;
            }

            Directory.CreateDirectory(Directories.Base);

            Dialog.CancelEnabled = true;
            Dialog.ProgressStyle = ProgressBarStyle.Continuous;

            // compute total bytes to download

            foreach (Package package in VersionPackageManifest)
                TotalBytes += package.PackedSize;

            ProgressIncrement = (double)1 / TotalBytes * 100;

            Directory.CreateDirectory(Directories.Downloads);
            Directory.CreateDirectory(Directories.Versions);

            foreach (Package package in VersionPackageManifest)
            {
                // download all the packages synchronously
                await DownloadPackage(package);

                // extract the package immediately after download
                ExtractPackage(package);
            }

            // allow progress bar to 100% before continuing (purely ux reasons lol)
            await Task.Delay(1000);

            Dialog.ProgressStyle = ProgressBarStyle.Marquee;

            Dialog.Message = "Configuring Roblox...";

            // wait for all packages to finish extracting
            while (PackagesExtracted < VersionPackageManifest.Count)
            {
                await Task.Delay(100);
            }

            string appSettingsLocation = Path.Combine(VersionFolder, "AppSettings.xml");
            await File.WriteAllTextAsync(appSettingsLocation, AppSettings);

            if (!FreshInstall)
            {
                ReShade.SynchronizeConfigFile();

                // let's take this opportunity to delete any packages we don't need anymore
                foreach (string filename in Directory.GetFiles(Directories.Downloads))
                {
                    if (!VersionPackageManifest.Exists(package => filename.Contains(package.Signature)))
                        File.Delete(filename);
                }

                string oldVersionFolder = Path.Combine(Directories.Versions, Program.Settings.VersionGuid);

                if (VersionGuid != Program.Settings.VersionGuid && Directory.Exists(oldVersionFolder))
                {
                    // and also to delete our old version folder
                    Directory.Delete(oldVersionFolder, true);
                }
            }

            Dialog.CancelEnabled = false;

            Program.Settings.VersionGuid = VersionGuid;
        }

        private async Task ApplyModifications()
        {
            Dialog.Message = "Applying Roblox modifications...";

            string modFolder = Path.Combine(Directories.Modifications);
            string manifestFile = Path.Combine(Directories.Base, "ModManifest.txt");

            List<string> manifestFiles = new();
            List<string> modFolderFiles = new();

            if (!Directory.Exists(modFolder))
            {
                Directory.CreateDirectory(modFolder);
                await File.WriteAllTextAsync(Path.Combine(modFolder, "README.txt"), ModReadme);
            }

            await CheckModPreset(Program.Settings.UseOldDeathSound, @"content\sounds\ouch.ogg", "OldDeath.ogg");
            await CheckModPreset(Program.Settings.UseOldMouseCursor, @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png", "OldCursor.png");
            await CheckModPreset(Program.Settings.UseOldMouseCursor, @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "OldFarCursor.png");
            await CheckModPreset(Program.Settings.UseDisableAppPatch, @"ExtraContent\places\Mobile.rbxl", "");

            await ReShade.CheckModifications();

            foreach (string file in Directory.GetFiles(modFolder, "*.*", SearchOption.AllDirectories))
            {
                // get relative directory path
                string relativeFile = file.Substring(modFolder.Length + 1);

                // ignore files placed in the root directory as long as they're not ini or dll files
                if (!relativeFile.Contains('\\') && !relativeFile.EndsWith(".ini") && !relativeFile.EndsWith(".dll"))
                    continue;

                modFolderFiles.Add(relativeFile);
            }

            // the manifest is primarily here to keep track of what files have been
            // deleted from the modifications folder, so that we know when to restore the
            // original files from the downloaded packages

            if (File.Exists(manifestFile))
                manifestFiles = (await File.ReadAllLinesAsync(manifestFile)).ToList();
            else
                manifestFiles = modFolderFiles;

            // copy and overwrite
            foreach (string file in modFolderFiles)
            {
                string fileModFolder = Path.Combine(modFolder, file);
                string fileVersionFolder = Path.Combine(VersionFolder, file);

                if (File.Exists(fileVersionFolder))
                {
                    if (Utilities.MD5File(fileModFolder) == Utilities.MD5File(fileVersionFolder))
                        continue;
                }

                string? directory = Path.GetDirectoryName(fileVersionFolder);

                if (directory is null)
                    continue;

                Directory.CreateDirectory(directory);

                File.Copy(fileModFolder, fileVersionFolder, true);
                File.SetAttributes(fileVersionFolder, File.GetAttributes(fileModFolder) & ~FileAttributes.ReadOnly);
            }

            // now check for files that have been deleted from the mod folder according to the manifest
            foreach (string fileLocation in manifestFiles)
            {
                if (modFolderFiles.Contains(fileLocation))
                    continue;

                KeyValuePair<string, string> packageDirectory;

                try
                {
                    packageDirectory = PackageDirectories.First(x => x.Key != "RobloxApp.zip" && fileLocation.StartsWith(x.Value));
                }
                catch (InvalidOperationException)
                {
                    // package doesn't exist, likely mistakenly placed file
                    string versionFileLocation = Path.Combine(VersionFolder, fileLocation);

                    if (File.Exists(versionFileLocation))
                        File.Delete(versionFileLocation);

                    continue;
                }

                // restore original file
                string fileName = fileLocation.Substring(packageDirectory.Value.Length);
                ExtractFileFromPackage(packageDirectory.Key, fileName);
            }

            File.WriteAllLines(manifestFile, modFolderFiles);
        }

        private static async Task CheckModPreset(bool condition, string location, string name)
        {
            string modFolderLocation = Path.Combine(Directories.Modifications, location);
            byte[] binaryData = string.IsNullOrEmpty(name) ? Array.Empty<byte>() : await ResourceHelper.Get(name);

            if (condition)
            {
                if (!File.Exists(modFolderLocation))
                {
                    string? directory = Path.GetDirectoryName(modFolderLocation);

                    if (directory is null)
                        return;

                    Directory.CreateDirectory(directory);

                    await File.WriteAllBytesAsync(modFolderLocation, binaryData);
                }
            }
            else if (File.Exists(modFolderLocation) && Utilities.MD5File(modFolderLocation) == Utilities.MD5Data(binaryData))
            {
                File.Delete(modFolderLocation);
            }
        }

        private async Task DownloadPackage(Package package)
        {
            string packageUrl = $"{DeployManager.BaseUrl}/{VersionGuid}-{package.Name}";
            string packageLocation = Path.Combine(Directories.Downloads, package.Signature);
            string robloxPackageLocation = Path.Combine(Program.LocalAppData, "Roblox", "Downloads", package.Signature);

            if (File.Exists(packageLocation))
            {
                FileInfo file = new(packageLocation);

                string calculatedMD5 = Utilities.MD5File(packageLocation);
                if (calculatedMD5 != package.Signature)
                {
                    Debug.WriteLine($"{package.Name} is corrupted ({calculatedMD5} != {package.Signature})! Deleting and re-downloading...");
                file.Delete();
                }
                else
                {
                    Debug.WriteLine($"{package.Name} is already downloaded, skipping...");
                    TotalDownloadedBytes += package.PackedSize;
                    UpdateProgressbar();
                    return;
                }
            }
            else if (File.Exists(robloxPackageLocation))
            {
                // let's cheat! if the stock bootstrapper already previously downloaded the file,
                // then we can just copy the one from there

                Debug.WriteLine($"Found existing version of {package.Name} ({robloxPackageLocation})! Copying to Downloads folder...");
                File.Copy(robloxPackageLocation, packageLocation);
                TotalDownloadedBytes += package.PackedSize;
                UpdateProgressbar();
                return;
            }

            if (!File.Exists(packageLocation))
            {
                Debug.WriteLine($"Downloading {package.Name}...");

                if (CancelFired)
                    return;

                var response = await Program.HttpClient.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead);

                var buffer = new byte[8192];

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(packageLocation, FileMode.CreateNew))
                {
                    while (true)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break; // we're done

                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                        TotalDownloadedBytes += bytesRead;
                        UpdateProgressbar();
                    }
                }

                Debug.WriteLine($"Finished downloading {package.Name}!");
            }
        }

        private async void ExtractPackage(Package package)
        {
            if (CancelFired)
                return;

            string packageLocation = Path.Combine(Directories.Downloads, package.Signature);
            string packageFolder = Path.Combine(VersionFolder, PackageDirectories[package.Name]);
            string extractPath;
            string? directory;

            Debug.WriteLine($"Extracting {package.Name} to {packageFolder}...");

            using (ZipArchive archive = await Task.Run(() => ZipFile.OpenRead(packageLocation)))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (CancelFired)
                        return;

                    if (entry.FullName.EndsWith('\\'))
                        continue;

                    extractPath = Path.Combine(packageFolder, entry.FullName);

                    //Debug.WriteLine($"[{package.Name}] Writing {extractPath}...");

                    directory = Path.GetDirectoryName(extractPath);

                    if (directory is null)
                        continue;

                    Directory.CreateDirectory(directory);

                    if (File.Exists(extractPath))
                        File.Delete(extractPath);

                    await Task.Run(() => entry.ExtractToFile(extractPath));
                }
            }

            Debug.WriteLine($"Finished extracting {package.Name}");

            PackagesExtracted += 1;
        }

        private void ExtractFileFromPackage(string packageName, string fileName)
        {
            Package? package = VersionPackageManifest.Find(x => x.Name == packageName);

            if (package is null)
                return;

            DownloadPackage(package).GetAwaiter().GetResult();

            string packageLocation = Path.Combine(Directories.Downloads, package.Signature);
            string packageFolder = Path.Combine(VersionFolder, PackageDirectories[package.Name]);

            using (ZipArchive archive = ZipFile.OpenRead(packageLocation))
            {
                ZipArchiveEntry? entry = archive.Entries.Where(x => x.FullName == fileName).FirstOrDefault();

                if (entry is null)
                    return;

                string fileLocation = Path.Combine(packageFolder, entry.FullName);

                if (File.Exists(fileLocation))
                    File.Delete(fileLocation);

                entry.ExtractToFile(fileLocation);
            }
        }
#endregion
    }
}
