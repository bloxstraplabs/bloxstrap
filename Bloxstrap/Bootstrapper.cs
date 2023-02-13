using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;

using Microsoft.Win32;

using Bloxstrap.Dialogs;
using Bloxstrap.Helpers;
using Bloxstrap.Helpers.Integrations;
using Bloxstrap.Helpers.RSMM;
using Bloxstrap.Models;

namespace Bloxstrap
{
    public partial class Bootstrapper
    {
        #region Properties

        // https://learn.microsoft.com/en-us/windows/win32/msi/error-codes
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_INSTALL_USEREXIT = 1602;
        public const int ERROR_INSTALL_FAILURE = 1603;

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

        private const string AppSettings =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<Settings>\n" +
            "	<ContentFolder>content</ContentFolder>\n" +
            "	<BaseUrl>http://www.roblox.com</BaseUrl>\n" +
            "</Settings>\n";

        private readonly CancellationTokenSource _cancelTokenSource = new();

        private static bool FreshInstall => String.IsNullOrEmpty(App.State.Prop.VersionGuid);

        private string? _launchCommandLine;

        private string _versionGuid = null!;
        private PackageManifest _versionPackageManifest = null!;
        private string _versionFolder = null!;

        private bool _isInstalling = false;
        private double _progressIncrement;
        private long _totalDownloadedBytes = 0;
        private int _packagesExtracted = 0;
        private bool _cancelFired = false;

        public IBootstrapperDialog? Dialog = null;
        #endregion

        #region Core
        public Bootstrapper(string? launchCommandLine = null)
        {
            _launchCommandLine = launchCommandLine;
        }

        private void SetStatus(string message)
        {
            App.Logger.WriteLine($"[Bootstrapper::SetStatus] {message}");

            if (Dialog is not null)
                Dialog.Message = message;
        }

        public async Task Run()
        {
            App.Logger.WriteLine("[Bootstrapper::Run] Running bootstrapper");

            if (App.IsUninstall)
            {
                Uninstall();
                return;
            }

#if !DEBUG
            if (!App.IsFirstRun && App.Settings.Prop.CheckForUpdates)
                await CheckForUpdates();
#endif

            await CheckLatestVersion();

            // if bloxstrap is installing for the first time but is running, prompt to close roblox
            // if roblox needs updating but is running, ignore update for now
            if (!Directory.Exists(_versionFolder) && CheckIfRunning(true) || App.State.Prop.VersionGuid != _versionGuid && !CheckIfRunning(false))
                await InstallLatestVersion();

            if (App.IsFirstRun)
                App.ShouldSaveConfigs = true;

            await ApplyModifications();

            if (App.IsFirstRun || FreshInstall)
                Register();

            CheckInstall();

            await RbxFpsUnlocker.CheckInstall();
            
            App.Settings.Save();
            App.State.Save();

            if (App.IsFirstRun && App.IsNoLaunch)
                Dialog?.ShowSuccess($"{App.ProjectName} has successfully installed");
            else if (!App.IsNoLaunch)
                await StartRoblox();
        }

        private async Task CheckForUpdates()
        {
            string currentVersion = $"{App.ProjectName} v{App.Version}";

            App.Logger.WriteLine($"[Bootstrapper::CheckForUpdates] Checking for {App.ProjectName} updates...");

            var releaseInfo = await Utilities.GetJson<GithubRelease>($"https://api.github.com/repos/{App.ProjectRepository}/releases/latest");

            if (releaseInfo?.Assets is null || currentVersion == releaseInfo.Name)
            {
                App.Logger.WriteLine($"[Bootstrapper::CheckForUpdates] No updates found");
                return;
            }

            SetStatus($"Getting the latest {App.ProjectName}...");

            // 64-bit is always the first option
            GithubReleaseAsset asset = releaseInfo.Assets[Environment.Is64BitOperatingSystem ? 0 : 1];
            string downloadLocation = Path.Combine(Directories.Updates, asset.Name);

            Directory.CreateDirectory(Directories.Updates);

            App.Logger.WriteLine($"[Bootstrapper::CheckForUpdates] Downloading {releaseInfo.Name}...");

            if (!File.Exists(downloadLocation))
            {
                var response = await App.HttpClient.GetAsync(asset.BrowserDownloadUrl);

                await using var fileStream = new FileStream(Path.Combine(Directories.Updates, asset.Name), FileMode.CreateNew);
                await response.Content.CopyToAsync(fileStream);
            }

            App.Logger.WriteLine($"[Bootstrapper::CheckForUpdates] Starting {releaseInfo.Name}...");

            ProcessStartInfo startInfo = new()
            {
                FileName = downloadLocation,
            };

            foreach (string arg in App.LaunchArgs)
                startInfo.ArgumentList.Add(arg);

            App.Settings.Save();

            Process.Start(startInfo);

            Environment.Exit(0);
        }

        private async Task CheckLatestVersion()
        {
            SetStatus("Connecting to Roblox...");

            ClientVersion clientVersion = await DeployManager.GetLastDeploy(App.Settings.Prop.Channel);
            _versionGuid = clientVersion.VersionGuid;
            _versionFolder = Path.Combine(Directories.Versions, _versionGuid);
            _versionPackageManifest = await PackageManifest.Get(_versionGuid);
        }

        private bool CheckIfRunning(bool shutdown)
        {
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");

            if (processes.Length == 0)
                return false;

            if (shutdown)
            {
                Dialog?.PromptShutdown();

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
            string startEventName = App.ProjectName.Replace(" ", "") + "StartEvent";

            SetStatus("Starting Roblox...");

            if (_launchCommandLine == "--app" && App.Settings.Prop.UseDisableAppPatch)
            {
                Utilities.OpenWebsite("https://www.roblox.com/games");
                return;
            }

            // launch time isn't really required for all launches, but it's usually just safest to do this
            _launchCommandLine += " --launchtime=" + DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (App.Settings.Prop.Channel.ToLower() != DeployManager.DefaultChannel.ToLower())
                _launchCommandLine += " -channel " + App.Settings.Prop.Channel.ToLower();

            _launchCommandLine  += " -startEvent " + startEventName;

            bool shouldWait = false;
            Process gameClient = Process.Start(Path.Combine(_versionFolder, "RobloxPlayerBeta.exe"), _launchCommandLine);
            Process? rbxFpsUnlocker = null;
            DiscordRichPresence? richPresence = null;
            Mutex? singletonMutex = null;

            using (SystemEvent startEvent = new(startEventName))
            {
                bool startEventFired = await startEvent.WaitForEvent();

                startEvent.Close();

                if (!startEventFired)
                    return;
            }
            
            if (App.Settings.Prop.RFUEnabled && Process.GetProcessesByName("rbxfpsunlocker").Length == 0)
            {
                App.Logger.WriteLine("[Bootstrapper::StartRoblox] Using rbxfpsunlocker");

                ProcessStartInfo startInfo = new() 
                { 
                    WorkingDirectory = Path.Combine(Directories.Integrations, "rbxfpsunlocker"),
                    FileName = Path.Combine(Directories.Integrations, @"rbxfpsunlocker\rbxfpsunlocker.exe")
                }; 
                
                rbxFpsUnlocker = Process.Start(startInfo);
                
                if (App.Settings.Prop.RFUAutoclose) 
                    shouldWait = true;
            }

            if (App.Settings.Prop.UseDiscordRichPresence)
            {
                App.Logger.WriteLine("[Bootstrapper::StartRoblox] Using Discord Rich Presence");
                richPresence = new DiscordRichPresence();
                shouldWait = true;
            }

            if (App.Settings.Prop.MultiInstanceLaunching)
            {
                App.Logger.WriteLine("[Bootstrapper::StartRoblox] Creating singleton mutex");
                // this might be a bit problematic since this mutex will be released when the first launched instance is closed...
                singletonMutex = new Mutex(true, "ROBLOX_singletonMutex");
                shouldWait = true;
            }

            // event fired, wait for 3 seconds then close
            await Task.Delay(3000);
            Dialog?.CloseBootstrapper();

            // keep bloxstrap open in the background if needed
            if (!shouldWait)
                return;

            richPresence?.MonitorGameActivity();

            App.Logger.WriteLine("[Bootstrapper::StartRoblox] Waiting for Roblox to close");
            await gameClient.WaitForExitAsync();

            richPresence?.Dispose();

            if (App.Settings.Prop.RFUAutoclose)
                rbxFpsUnlocker?.Kill();
        }

        public void CancelInstall()
        {
            if (!_isInstalling)
            {
                App.Terminate(ERROR_INSTALL_USEREXIT);
                return;
            }

            App.Logger.WriteLine("[Bootstrapper::CancelInstall] Cancelling install...");

            _cancelTokenSource.Cancel();
            _cancelFired = true;

            try
            {
                // clean up install
                if (App.IsFirstRun)
                    Directory.Delete(Directories.Base, true);
                else if (Directory.Exists(_versionFolder))
                    Directory.Delete(_versionFolder, true);
            }
            catch (Exception e)
            {
                App.Logger.WriteLine("[Bootstrapper::CancelInstall] Could not fully clean up installation!");
                App.Logger.WriteLine($"[Bootstrapper::CancelInstall] {e}");
            }

            App.Terminate(ERROR_INSTALL_USEREXIT);
        }
#endregion

        #region App Install
        public static void Register()
        {
            RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{App.ProjectName}");

            // new install location selected, delete old one
            string? oldInstallLocation = (string?)applicationKey.GetValue("OldInstallLocation");
            if (!String.IsNullOrEmpty(oldInstallLocation) && oldInstallLocation != Directories.Base)
            {
                try
                {
                    if (Directory.Exists(oldInstallLocation))
                        Directory.Delete(oldInstallLocation, true);
                }
                catch (Exception)
                {
                    // ignored
                }

                applicationKey.DeleteValue("OldInstallLocation");
            }

            applicationKey.SetValue("InstallLocation", Directories.Base);
            applicationKey.Close();

            // set uninstall key
            RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{App.ProjectName}");
            uninstallKey.SetValue("DisplayIcon", $"{Directories.Application},0");
            uninstallKey.SetValue("DisplayName", App.ProjectName);
            uninstallKey.SetValue("DisplayVersion", App.Version);

            if (uninstallKey.GetValue("InstallDate") is null)
                uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

            uninstallKey.SetValue("InstallLocation", Directories.Base);
            uninstallKey.SetValue("NoRepair", 1);
            uninstallKey.SetValue("Publisher", "pizzaboxer");
            uninstallKey.SetValue("ModifyPath", $"\"{Directories.Application}\" -preferences");
            uninstallKey.SetValue("QuietUninstallString", $"\"{Directories.Application}\" -uninstall -quiet");
            uninstallKey.SetValue("UninstallString", $"\"{Directories.Application}\" -uninstall");
            uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{App.ProjectRepository}");
            uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{App.ProjectRepository}/releases/latest");
            uninstallKey.Close();

            App.Logger.WriteLine("[Bootstrapper::StartRoblox] Registered application version");
        }

        public static void CheckInstall()
        {
            App.Logger.WriteLine("[Bootstrapper::StartRoblox] Checking install");

            // check if launch uri is set to our bootstrapper
            // this doesn't go under register, so we check every launch
            // just in case the stock bootstrapper changes it back

            Protocol.Register("roblox", "Roblox", Directories.Application);
            Protocol.Register("roblox-player", "Roblox", Directories.Application);

            // in case the user is reinstalling
            if (File.Exists(Directories.Application) && App.IsFirstRun)
                File.Delete(Directories.Application);

            // check to make sure bootstrapper is in the install folder
            if (!File.Exists(Directories.Application) && Environment.ProcessPath is not null)
                File.Copy(Environment.ProcessPath, Directories.Application);

            // this SHOULD go under Register(),
            // but then people who have Bloxstrap v1.0.0 installed won't have this without a reinstall
            // maybe in a later version?
            if (!Directory.Exists(Directories.StartMenu))
            {
                Directory.CreateDirectory(Directories.StartMenu);

                ShellLink.Shortcut.CreateShortcut(Directories.Application, "", Directories.Application, 0)
                    .WriteToFile(Path.Combine(Directories.StartMenu, "Play Roblox.lnk"));

                ShellLink.Shortcut.CreateShortcut(Directories.Application, "-preferences", Directories.Application, 0)
                    .WriteToFile(Path.Combine(Directories.StartMenu, $"{App.ProjectName} Menu.lnk"));
            }
            else
            {
                // v2.0.0 - rebadge configuration menu as just "Bloxstrap Menu"
                string oldMenuShortcut = Path.Combine(Directories.StartMenu, $"Configure {App.ProjectName}.lnk");
                string newMenuShortcut = Path.Combine(Directories.StartMenu, $"{App.ProjectName} Menu.lnk");

                if (File.Exists(oldMenuShortcut))
                    File.Delete(oldMenuShortcut);

                if (!File.Exists(newMenuShortcut))
                    ShellLink.Shortcut.CreateShortcut(Directories.Application, "-preferences", Directories.Application, 0)
                        .WriteToFile(newMenuShortcut);
            }

            if (App.Settings.Prop.CreateDesktopIcon)
            {
                if (!File.Exists(Path.Combine(Directories.Desktop, "Play Roblox.lnk")))
                {
                    ShellLink.Shortcut.CreateShortcut(Directories.Application, "", Directories.Application, 0)
                        .WriteToFile(Path.Combine(Directories.Desktop, "Play Roblox.lnk"));
                }

                // one-time toggle, set it back to false
                App.Settings.Prop.CreateDesktopIcon = false;
            }
        }

        private void Uninstall()
        {
            CheckIfRunning(true);

            SetStatus($"Uninstalling {App.ProjectName}...");

            //App.Settings.ShouldSave = false;
            App.ShouldSaveConfigs = false;

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
                Registry.CurrentUser.DeleteSubKey($@"Software\{App.ProjectName}");

                // delete start menu folder
                Directory.Delete(Directories.StartMenu, true);

                // delete desktop shortcut
                File.Delete(Path.Combine(Directories.Desktop, "Play Roblox.lnk"));

                // delete uninstall key
                Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{App.ProjectName}");

                // delete installation folder
                // (should delete everything except bloxstrap itself)
                Directory.Delete(Directories.Base, true);
            }
            catch (Exception e) 
            {
                App.Logger.WriteLine($"Could not fully uninstall! ({e})");
            }

            Dialog?.ShowSuccess($"{App.ProjectName} has succesfully uninstalled");
        }
#endregion

        #region Roblox Install
        private void UpdateProgressbar()
        {
            int newProgress = (int)Math.Floor(_progressIncrement * _totalDownloadedBytes);

            // bugcheck: if we're restoring a file from a package, it'll incorrectly increment the progress beyond 100
            // too lazy to fix properly so lol
            if (newProgress > 100)
                return;

            if (Dialog is not null)
                Dialog.ProgressValue = newProgress;
        }

        private async Task InstallLatestVersion()
        {
            _isInstalling = true;

            SetStatus(FreshInstall ? "Installing Roblox..." : "Upgrading Roblox...");

            // check if we have at least 300 megabytes of free disk space
            if (Utilities.GetFreeDiskSpace(Directories.Base) < 1024*1024*300)
            {
                App.ShowMessageBox($"{App.ProjectName} requires at least 300 MB of disk space to install Roblox. Please free up some disk space and try again.", MessageBoxImage.Error);
                App.Terminate(ERROR_INSTALL_FAILURE);
                return;
            }

            Directory.CreateDirectory(Directories.Base);

            if (Dialog is not null)
            {
                Dialog.CancelEnabled = true;
                Dialog.ProgressStyle = ProgressBarStyle.Continuous;
            }

            // compute total bytes to download
            _progressIncrement = (double)100 / _versionPackageManifest.Sum(package => package.PackedSize);

            Directory.CreateDirectory(Directories.Downloads);
            Directory.CreateDirectory(Directories.Versions);

            foreach (Package package in _versionPackageManifest)
            {
                if (_cancelFired)
                    return;

                // download all the packages synchronously
                await DownloadPackage(package);

                // extract the package immediately after download asynchronously
                ExtractPackage(package);
            }

            if (_cancelFired) 
                return;

            // allow progress bar to 100% before continuing (purely ux reasons lol)
            await Task.Delay(1000);

            if (Dialog is not null)
            {
                Dialog.ProgressStyle = ProgressBarStyle.Marquee;
                SetStatus("Configuring Roblox...");
            }

            // wait for all packages to finish extracting
            while (_packagesExtracted < _versionPackageManifest.Count)
            {
                await Task.Delay(100);
            }

            string appSettingsLocation = Path.Combine(_versionFolder, "AppSettings.xml");
            await File.WriteAllTextAsync(appSettingsLocation, AppSettings);

            if (_cancelFired)
                return;

            if (!FreshInstall)
            {
                ReShade.SynchronizeConfigFile();

                // let's take this opportunity to delete any packages we don't need anymore
                foreach (string filename in Directory.GetFiles(Directories.Downloads))
                {
                    if (!_versionPackageManifest.Exists(package => filename.Contains(package.Signature)))
                    {
                        App.Logger.WriteLine($"Deleting unused package {filename}");
                        File.Delete(filename);
                    }
                }

                string oldVersionFolder = Path.Combine(Directories.Versions, App.State.Prop.VersionGuid);

                if (_versionGuid != App.State.Prop.VersionGuid && Directory.Exists(oldVersionFolder))
                {
                    // and also to delete our old version folder
                    Directory.Delete(oldVersionFolder, true);
                }
            }

            if (Dialog is not null)
                Dialog.CancelEnabled = false;

            App.State.Prop.VersionGuid = _versionGuid;

            _isInstalling = false;
        }

        private async Task ApplyModifications()
        {
            SetStatus("Applying Roblox modifications...");

            string modFolder = Path.Combine(Directories.Modifications);

            // manifest has been moved to State.json
            File.Delete(Path.Combine(Directories.Base, "ModManifest.txt"));

            List<string> modFolderFiles = new();

            if (!Directory.Exists(modFolder))
                Directory.CreateDirectory(modFolder);

            await CheckModPreset(App.Settings.Prop.UseOldDeathSound, @"content\sounds\ouch.ogg", "OldDeath.ogg");
            await CheckModPreset(App.Settings.Prop.UseOldMouseCursor, @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png", "OldCursor.png");
            await CheckModPreset(App.Settings.Prop.UseOldMouseCursor, @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "OldFarCursor.png");
            await CheckModPreset(App.Settings.Prop.UseDisableAppPatch, @"ExtraContent\places\Mobile.rbxl", "");

            await ReShade.CheckModifications();

            foreach (string file in Directory.GetFiles(modFolder, "*.*", SearchOption.AllDirectories))
            {
                // get relative directory path
                string relativeFile = file.Substring(modFolder.Length + 1);

                // v1.7.0 - README has been moved to the preferences menu now
                if (relativeFile == "README.txt")
                {
                    File.Delete(file);
                    continue;
                }

                modFolderFiles.Add(relativeFile);
            }

            // copy and overwrite
            foreach (string file in modFolderFiles)
            {
                string fileModFolder = Path.Combine(modFolder, file);
                string fileVersionFolder = Path.Combine(_versionFolder, file);

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

            // the manifest is primarily here to keep track of what files have been
            // deleted from the modifications folder, so that we know when to restore the original files from the downloaded packages
            // now check for files that have been deleted from the mod folder according to the manifest
            foreach (string fileLocation in App.State.Prop.ModManifest)
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
                    string versionFileLocation = Path.Combine(_versionFolder, fileLocation);

                    File.Delete(versionFileLocation);

                    continue;
                }

                // restore original file
                string fileName = fileLocation.Substring(packageDirectory.Value.Length);
                ExtractFileFromPackage(packageDirectory.Key, fileName);
            }

            App.State.Prop.ModManifest = modFolderFiles;
            App.State.Save();
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
            if (_cancelFired)
                return;

            string packageUrl = $"{DeployManager.BaseUrl}/{_versionGuid}-{package.Name}";
            string packageLocation = Path.Combine(Directories.Downloads, package.Signature);
            string robloxPackageLocation = Path.Combine(Directories.LocalAppData, "Roblox", "Downloads", package.Signature);

            if (File.Exists(packageLocation))
            {
                FileInfo file = new(packageLocation);

                string calculatedMD5 = Utilities.MD5File(packageLocation);
                if (calculatedMD5 != package.Signature)
                {
                    App.Logger.WriteLine($"[Bootstrapper::DownloadPackage] {package.Name} is corrupted ({calculatedMD5} != {package.Signature})! Deleting and re-downloading...");
                    file.Delete();
                }
                else
                {
                    App.Logger.WriteLine($"[Bootstrapper::DownloadPackage] {package.Name} is already downloaded, skipping...");
                    _totalDownloadedBytes += package.PackedSize;
                    UpdateProgressbar();
                    return;
                }
            }
            else if (File.Exists(robloxPackageLocation))
            {
                // let's cheat! if the stock bootstrapper already previously downloaded the file,
                // then we can just copy the one from there

                App.Logger.WriteLine($"[Bootstrapper::DownloadPackage] Found existing version of {package.Name} ({robloxPackageLocation})! Copying to Downloads folder...");
                File.Copy(robloxPackageLocation, packageLocation);
                _totalDownloadedBytes += package.PackedSize;
                UpdateProgressbar();
                return;
            }

            if (!File.Exists(packageLocation))
            {
                App.Logger.WriteLine($"[Bootstrapper::DownloadPackage] Downloading {package.Name} ({package.Signature})...");

                {
                    var response = await App.HttpClient.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead, _cancelTokenSource.Token);
                    var buffer = new byte[4096];

                    await using var stream = await response.Content.ReadAsStreamAsync(_cancelTokenSource.Token);
                    await using var fileStream = new FileStream(packageLocation, FileMode.CreateNew, FileAccess.Write, FileShare.Delete); 
                    
                    while (true)
                    {
                        if (_cancelFired)
                        {
                            stream.Close();
                            fileStream.Close();
                            return;
                        }

                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cancelTokenSource.Token);

                        if (bytesRead == 0)
                            break; // we're done

                        await fileStream.WriteAsync(buffer, 0, bytesRead, _cancelTokenSource.Token);

                        _totalDownloadedBytes += bytesRead;
                        UpdateProgressbar();
                    }
                }

                App.Logger.WriteLine($"[Bootstrapper::DownloadPackage] Finished downloading {package.Name}!");
            }
        }

        private async void ExtractPackage(Package package)
        {
            if (_cancelFired)
                return;

            string packageLocation = Path.Combine(Directories.Downloads, package.Signature);
            string packageFolder = Path.Combine(_versionFolder, PackageDirectories[package.Name]);
            string extractPath;

            App.Logger.WriteLine($"[Bootstrapper::ExtractPackage] Extracting {package.Name} to {packageFolder}...");

            using (ZipArchive archive = await Task.Run(() => ZipFile.OpenRead(packageLocation)))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (_cancelFired)
                        return;

                    if (entry.FullName.EndsWith('\\'))
                        continue;

                    extractPath = Path.Combine(packageFolder, entry.FullName);

                    //App.Logger.WriteLine($"[{package.Name}] Writing {extractPath}...");

                    string? directory = Path.GetDirectoryName(extractPath);

                    if (directory is null)
                        continue;

                    Directory.CreateDirectory(directory);

                    await Task.Run(() => entry.ExtractToFile(extractPath, true));
                }
            }

            App.Logger.WriteLine($"[Bootstrapper::ExtractPackage] Finished extracting {package.Name}");

            _packagesExtracted += 1;
        }

        private void ExtractFileFromPackage(string packageName, string fileName)
        {
            Package? package = _versionPackageManifest.Find(x => x.Name == packageName);

            if (package is null)
                return;

            DownloadPackage(package).GetAwaiter().GetResult();

            string packageLocation = Path.Combine(Directories.Downloads, package.Signature);
            string packageFolder = Path.Combine(_versionFolder, PackageDirectories[package.Name]);

            using ZipArchive archive = ZipFile.OpenRead(packageLocation);

            ZipArchiveEntry? entry = archive.Entries.FirstOrDefault(x => x.FullName == fileName);

            if (entry is null)
                return;

            string fileLocation = Path.Combine(packageFolder, entry.FullName);
                
            File.Delete(fileLocation);

            entry.ExtractToFile(fileLocation);
        }
#endregion
    }
}
