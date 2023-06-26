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

using Bloxstrap.Enums;
using Bloxstrap.Extensions;
using Bloxstrap.Integrations;
using Bloxstrap.Models;
using Bloxstrap.Tools;
using Bloxstrap.UI.BootstrapperDialogs;

namespace Bloxstrap
{
    public class Bootstrapper
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

            // the runtime installer is only extracted if it needs installing
            { "WebView2.zip",                  @"" },
            { "WebView2RuntimeInstaller.zip",  @"WebView2RuntimeInstaller\" },

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
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
            "<Settings>\r\n" +
            "	<ContentFolder>content</ContentFolder>\r\n" +
            "	<BaseUrl>http://www.roblox.com</BaseUrl>\r\n" +
            "</Settings>\r\n";

        private readonly CancellationTokenSource _cancelTokenSource = new();

        private static bool FreshInstall => String.IsNullOrEmpty(App.State.Prop.VersionGuid);
        private static string DesktopShortcutLocation => Path.Combine(Directories.Desktop, "Play Roblox.lnk");

        private string _playerLocation => Path.Combine(_versionFolder, "RobloxPlayerBeta.exe");

        private string _launchCommandLine;

        private string _latestVersionGuid = null!;
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
        public Bootstrapper(string launchCommandLine)
        {
            _launchCommandLine = launchCommandLine;
        }

        private void SetStatus(string message)
        {
            App.Logger.WriteLine($"[Bootstrapper::SetStatus] {message}");

            // yea idk
            if (App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.ByfronDialog)
                message = message.Replace("...", "");

            if (Dialog is not null)
                Dialog.Message = message;
        }

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

            // ensure only one instance of the bootstrapper is running at the time
            // so that we don't have stuff like two updates happening simultaneously

            bool mutexExists = false;

            try
            {
                Mutex.OpenExisting("Bloxstrap_BootstrapperMutex").Close();
                App.Logger.WriteLine("[Bootstrapper::Run] Bloxstrap_BootstrapperMutex mutex exists, waiting...");
                mutexExists = true;
            }
            catch (Exception)
            {
                // no mutex exists
            }

            // wait for mutex to be released if it's not yet
            await using AsyncMutex mutex = new("Bloxstrap_BootstrapperMutex");
            await mutex.AcquireAsync(_cancelTokenSource.Token);

            // reload our configs since they've likely changed by now
            if (mutexExists)
            {
                App.Settings.Load();
                App.State.Load();
            }

            await CheckLatestVersion();

            CheckInstallMigration();

            // install/update roblox if we're running for the first time, needs updating, or the player location doesn't exist
            if (App.IsFirstRun || _latestVersionGuid != App.State.Prop.VersionGuid || !File.Exists(_playerLocation))
                await InstallLatestVersion();

            if (App.IsFirstRun)
                App.ShouldSaveConfigs = true;

            MigrateIntegrations();

            await InstallWebView2();

            App.FastFlags.Save();
            await ApplyModifications();

            if (App.IsFirstRun || FreshInstall)
            {
                Register();
                RegisterProgramSize();
            }

            CheckInstall();

            // at this point we've finished updating our configs
            App.Settings.Save();
            App.State.Save();
            App.ShouldSaveConfigs = false;

            await mutex.ReleaseAsync();

            if (App.IsFirstRun && App.IsNoLaunch)
                Dialog?.ShowSuccess($"{App.ProjectName} has successfully installed");
            else if (!App.IsNoLaunch && !_cancelFired)
                await StartRoblox();
        }

        private async Task CheckLatestVersion()
        {
            SetStatus("Connecting to Roblox...");

            ClientVersion clientVersion = await RobloxDeployment.GetInfo(App.Settings.Prop.Channel);

            // briefly check if current channel is suitable to use
            if (App.Settings.Prop.Channel.ToLowerInvariant() != RobloxDeployment.DefaultChannel.ToLowerInvariant() && App.Settings.Prop.ChannelChangeMode != ChannelChangeMode.Ignore)
            {
                string? switchDefaultPrompt = null;
                ClientVersion? defaultChannelInfo = null;

                App.Logger.WriteLine($"[Bootstrapper::CheckLatestVersion] Checking if current channel is suitable to use...");

                if (String.IsNullOrEmpty(switchDefaultPrompt))
                {
                    // this SUCKS
                    defaultChannelInfo = await RobloxDeployment.GetInfo(RobloxDeployment.DefaultChannel);
                    int defaultChannelVersion = Int32.Parse(defaultChannelInfo.Version.Split('.')[1]);
                    int currentChannelVersion = Int32.Parse(clientVersion.Version.Split('.')[1]);

                    if (currentChannelVersion < defaultChannelVersion)
                        switchDefaultPrompt = $"Your current preferred channel ({App.Settings.Prop.Channel}) appears to no longer be receiving updates. Would you like to switch to {RobloxDeployment.DefaultChannel}?";
                }

                if (!String.IsNullOrEmpty(switchDefaultPrompt))
                {
                    MessageBoxResult result = App.Settings.Prop.ChannelChangeMode == ChannelChangeMode.Automatic ? MessageBoxResult.Yes : App.ShowMessageBox(switchDefaultPrompt, MessageBoxImage.Question, MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        App.Settings.Prop.Channel = RobloxDeployment.DefaultChannel;
                        App.Logger.WriteLine($"[DeployManager::SwitchToDefault] Changed Roblox release channel from {App.Settings.Prop.Channel} to {RobloxDeployment.DefaultChannel}");

                        if (defaultChannelInfo is null)
                            defaultChannelInfo = await RobloxDeployment.GetInfo(RobloxDeployment.DefaultChannel);

                        clientVersion = defaultChannelInfo;
                    }
                }
            }

            _latestVersionGuid = clientVersion.VersionGuid;
            _versionFolder = Path.Combine(Directories.Versions, _latestVersionGuid);
            _versionPackageManifest = await PackageManifest.Get(_latestVersionGuid);
        }

        private async Task StartRoblox()
        {
            SetStatus("Starting Roblox...");

            if (_launchCommandLine == "--app" && App.Settings.Prop.UseDisableAppPatch)
            {
                Utilities.OpenWebsite("https://www.roblox.com/games");
                Dialog?.CloseBootstrapper();
                return;
            }

            _launchCommandLine = _launchCommandLine.Replace("LAUNCHTIMEPLACEHOLDER", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());

            if (App.Settings.Prop.Channel.ToLowerInvariant() != RobloxDeployment.DefaultChannel.ToLowerInvariant())
                _launchCommandLine += " -channel " + App.Settings.Prop.Channel.ToLowerInvariant();

            // whether we should wait for roblox to exit to handle stuff in the background or clean up after roblox closes
            bool shouldWait = false;

            // v2.2.0 - byfron will trip if we keep a process handle open for over a minute, so we're doing this now
            int gameClientPid;
            using (Process gameClient = Process.Start(_playerLocation, _launchCommandLine))
            {
                gameClientPid = gameClient.Id;
            }

            List<Process> autocloseProcesses = new();
            RobloxActivity? activityWatcher = null;
            DiscordRichPresence? richPresence = null;
            ServerNotifier? serverNotifier = null;

            App.Logger.WriteLine($"[Bootstrapper::StartRoblox] Started Roblox (PID {gameClientPid})");

            using (SystemEvent startEvent = new("www.roblox.com/robloxStartedEvent"))
            {
                bool startEventFired = await startEvent.WaitForEvent();

                startEvent.Close();

                if (!startEventFired)
                    return;
            }

            if (App.Settings.Prop.UseDiscordRichPresence || App.Settings.Prop.ShowServerDetails)
            {
                activityWatcher = new();
                shouldWait = true;
            }

            if (App.Settings.Prop.UseDiscordRichPresence)
            {
                App.Logger.WriteLine("[Bootstrapper::StartRoblox] Using Discord Rich Presence");
                richPresence = new(activityWatcher!);
            }

            if (App.Settings.Prop.ShowServerDetails)
            {
                App.Logger.WriteLine("[Bootstrapper::StartRoblox] Using server details notifier");
                serverNotifier = new(activityWatcher!);
            }

            // launch custom integrations now
            foreach (CustomIntegration integration in App.Settings.Prop.CustomIntegrations)
            {
                App.Logger.WriteLine($"[Bootstrapper::StartRoblox] Launching custom integration '{integration.Name}' ({integration.Location} {integration.LaunchArgs} - autoclose is {integration.AutoClose})");

                try
                {
                    Process process = Process.Start(integration.Location, integration.LaunchArgs);

                    if (integration.AutoClose)
                    {
                        shouldWait = true;
                        autocloseProcesses.Add(process);
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine($"[Bootstrapper::StartRoblox] Failed to launch integration '{integration.Name}'! ({ex.Message})");
                }
            }

            // event fired, wait for 3 seconds then close
            await Task.Delay(3000);
            Dialog?.CloseBootstrapper();

            // keep bloxstrap open in the background if needed
            if (!shouldWait)
                return;

            activityWatcher?.StartWatcher();

            App.Logger.WriteLine("[Bootstrapper::StartRoblox] Waiting for Roblox to close");

            while (Process.GetProcesses().Any(x => x.Id == gameClientPid))
                await Task.Delay(1000);

            App.Logger.WriteLine($"[Bootstrapper::StartRoblox] Roblox has exited");

            richPresence?.Dispose();

            foreach (Process process in autocloseProcesses)
            {
                if (process.HasExited)
                    continue;

                App.Logger.WriteLine($"[Bootstrapper::StartRoblox] Autoclosing process '{process.ProcessName}' (PID {process.Id})");
                process.Kill();
            }
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
            catch (Exception ex)
            {
                App.Logger.WriteLine("[Bootstrapper::CancelInstall] Could not fully clean up installation!");
                App.Logger.WriteLine($"[Bootstrapper::CancelInstall] {ex}");
            }

            App.Terminate(ERROR_INSTALL_USEREXIT);
        }
        #endregion

        #region App Install
        public static void Register()
        {
            using (RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{App.ProjectName}"))
            {
                applicationKey.SetValue("InstallLocation", Directories.Base);
            }

            // set uninstall key
            using (RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{App.ProjectName}"))
            {
                uninstallKey.SetValue("DisplayIcon", $"{Directories.Application},0");
                uninstallKey.SetValue("DisplayName", App.ProjectName);
                uninstallKey.SetValue("DisplayVersion", App.Version);

                if (uninstallKey.GetValue("InstallDate") is null)
                    uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

                uninstallKey.SetValue("InstallLocation", Directories.Base);
                uninstallKey.SetValue("NoRepair", 1);
                uninstallKey.SetValue("Publisher", "pizzaboxer");
                uninstallKey.SetValue("ModifyPath", $"\"{Directories.Application}\" -menu");
                uninstallKey.SetValue("QuietUninstallString", $"\"{Directories.Application}\" -uninstall -quiet");
                uninstallKey.SetValue("UninstallString", $"\"{Directories.Application}\" -uninstall");
                uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{App.ProjectRepository}");
                uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{App.ProjectRepository}/releases/latest");
            }

            App.Logger.WriteLine("[Bootstrapper::StartRoblox] Registered application");
        }

        public void RegisterProgramSize()
        {
            App.Logger.WriteLine("[Bootstrapper::RegisterProgramSize] Registering approximate program size...");

            using RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{App.ProjectName}");

            // sum compressed and uncompressed package sizes and convert to kilobytes
            int totalSize = (_versionPackageManifest.Sum(x => x.Size) + _versionPackageManifest.Sum(x => x.PackedSize)) / 1000;

            uninstallKey.SetValue("EstimatedSize", totalSize);

            App.Logger.WriteLine($"[Bootstrapper::RegisterProgramSize] Registered as {totalSize} KB");
        }

        private void CheckInstallMigration()
        {
            // check if we've changed our install location since the last time we started
            // in which case, we'll have to copy over all our folders so we don't lose any mods and stuff

            using RegistryKey? applicationKey = Registry.CurrentUser.OpenSubKey($@"Software\{App.ProjectName}", true);

            string? oldInstallLocation = (string?)applicationKey?.GetValue("OldInstallLocation");

            if (applicationKey is null || oldInstallLocation is null || oldInstallLocation == Directories.Base)
                return;

            SetStatus("Migrating install location...");

            if (Directory.Exists(oldInstallLocation))
            {
                App.Logger.WriteLine($"[Bootstrapper::CheckInstallMigration] Moving all files in {oldInstallLocation} to {Directories.Base}...");

                foreach (string oldFileLocation in Directory.GetFiles(oldInstallLocation, "*.*", SearchOption.AllDirectories))
                {
                    string relativeFile = oldFileLocation.Substring(oldInstallLocation.Length + 1);
                    string newFileLocation = Path.Combine(Directories.Base, relativeFile);
                    string? newDirectory = Path.GetDirectoryName(newFileLocation);

                    try
                    {
                        if (!String.IsNullOrEmpty(newDirectory))
                            Directory.CreateDirectory(newDirectory);

                        File.Move(oldFileLocation, newFileLocation, true);
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine($"[Bootstrapper::CheckInstallMigration] Failed to move {oldFileLocation} to {newFileLocation}! {ex}");
                    }
                }

                try
                {
                    Directory.Delete(oldInstallLocation, true);
                    App.Logger.WriteLine("[Bootstrapper::CheckInstallMigration] Deleted old install location");
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine($"[Bootstrapper::CheckInstallMigration] Failed to delete old install location! {ex}");
                }
            }

            applicationKey.DeleteValue("OldInstallLocation");

            // allow shortcuts to be re-registered
            if (Directory.Exists(Directories.StartMenu))
                Directory.Delete(Directories.StartMenu, true);

            if (File.Exists(DesktopShortcutLocation))
            {
                File.Delete(DesktopShortcutLocation);
                App.Settings.Prop.CreateDesktopIcon = true;
            }

            App.Logger.WriteLine("[Bootstrapper::CheckInstallMigration] Finished migrating install location!");
        }

        public static void CheckInstall()
        {
            App.Logger.WriteLine("[Bootstrapper::StartRoblox] Checking install");

            // check if launch uri is set to our bootstrapper
            // this doesn't go under register, so we check every launch
            // just in case the stock bootstrapper changes it back

            ProtocolHandler.Register("roblox", "Roblox", Directories.Application);
            ProtocolHandler.Register("roblox-player", "Roblox", Directories.Application);

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

                ShellLink.Shortcut.CreateShortcut(Directories.Application, "-menu", Directories.Application, 0)
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
                    ShellLink.Shortcut.CreateShortcut(Directories.Application, "-menu", Directories.Application, 0)
                        .WriteToFile(newMenuShortcut);
            }

            if (App.Settings.Prop.CreateDesktopIcon)
            {
                if (!File.Exists(DesktopShortcutLocation))
                {
                    ShellLink.Shortcut.CreateShortcut(Directories.Application, "", Directories.Application, 0)
                        .WriteToFile(DesktopShortcutLocation);
                }

                // one-time toggle, set it back to false
                App.Settings.Prop.CreateDesktopIcon = false;
            }
        }

        private async Task CheckForUpdates()
        {
            // don't update if there's another instance running (likely running in the background)
            if (Utilities.GetProcessCount(App.ProjectName) > 1)
            {
                App.Logger.WriteLine($"[Bootstrapper::CheckForUpdates] More than one Bloxstrap instance running, aborting update check");
                return;
            }

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
            GithubReleaseAsset asset = releaseInfo.Assets[0];
            string downloadLocation = Path.Combine(Directories.LocalAppData, "Temp", asset.Name);

            App.Logger.WriteLine($"[Bootstrapper::CheckForUpdates] Downloading {releaseInfo.Name}...");

            if (!File.Exists(downloadLocation))
            {
                var response = await App.HttpClient.GetAsync(asset.BrowserDownloadUrl);

                await using var fileStream = new FileStream(downloadLocation, FileMode.CreateNew);
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

        private void Uninstall()
        {
            // prompt to shutdown roblox if its currently running
            if (Utilities.CheckIfRobloxRunning())
            {
                App.Logger.WriteLine($"[Bootstrapper::Uninstall] Prompting to shut down all open Roblox instances");
                
                Dialog?.PromptShutdown();

                try
                {
                    foreach (Process process in Process.GetProcessesByName("RobloxPlayerBeta"))
                    {
                        process.CloseMainWindow();
                        process.Close();
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine($"[Bootstrapper::ShutdownIfRobloxRunning] Failed to close process! {ex}");
                }

                App.Logger.WriteLine($"[Bootstrapper::Uninstall] All Roblox processes closed");
            }
            
            SetStatus($"Uninstalling {App.ProjectName}...");

            //App.Settings.ShouldSave = false;
            App.ShouldSaveConfigs = false;

            // check if stock bootstrapper is still installed
            RegistryKey? bootstrapperKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\roblox-player");
            if (bootstrapperKey is null)
            {
                ProtocolHandler.Unregister("roblox");
                ProtocolHandler.Unregister("roblox-player");
            }
            else
            {
                // revert launch uri handler to stock bootstrapper

                string bootstrapperLocation = (string?)bootstrapperKey.GetValue("InstallLocation") + "RobloxPlayerLauncher.exe";

                ProtocolHandler.Register("roblox", "Roblox", bootstrapperLocation);
                ProtocolHandler.Register("roblox-player", "Roblox", bootstrapperLocation);
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
            catch (Exception ex) 
            {
                App.Logger.WriteLine($"Could not fully uninstall! ({ex})");
            }

            Action? callback = null;

            if (Directory.Exists(Directories.Base))
            {
                callback = () =>
                {
                    // this is definitely one of the workaround hacks of all time
                    // could antiviruses falsely detect this as malicious behaviour though?
                    // "hmm whats this program doing running a cmd command chain quietly in the background that auto deletes an entire folder"

                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c timeout 5 && del /Q \"{Directories.Base}\\*\" && rmdir \"{Directories.Base}\"",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                };
            }

            Dialog?.ShowSuccess($"{App.ProjectName} has succesfully uninstalled", callback);
        }
        #endregion

        #region Roblox Install
        private async Task InstallLatestVersion()
        {
            _isInstalling = true;

            SetStatus(FreshInstall ? "Installing Roblox..." : "Upgrading Roblox...");

            Directory.CreateDirectory(Directories.Base);
            Directory.CreateDirectory(Directories.Downloads);
            Directory.CreateDirectory(Directories.Versions);

            // package manifest states packed size and uncompressed size in exact bytes
            // packed size only matters if we don't already have the package cached on disk
            string[] cachedPackages = Directory.GetFiles(Directories.Downloads);
            int totalSizeRequired = _versionPackageManifest.Where(x => !cachedPackages.Contains(x.Signature)).Sum(x => x.PackedSize) + _versionPackageManifest.Sum(x => x.Size);
            
            if (Utilities.GetFreeDiskSpace(Directories.Base) < totalSizeRequired)
            {
                App.ShowMessageBox($"{App.ProjectName} does not have enough disk space to download and install Roblox. Please free up some disk space and try again.", MessageBoxImage.Error);
                App.Terminate(ERROR_INSTALL_FAILURE);
                return;
            }

            if (Dialog is not null)
            {
                Dialog.CancelEnabled = true;
                Dialog.ProgressStyle = ProgressBarStyle.Continuous;
            }

            // compute total bytes to download
            _progressIncrement = (double)100 / _versionPackageManifest.Sum(package => package.PackedSize);

            foreach (Package package in _versionPackageManifest)
            {
                if (_cancelFired)
                    return;

                // download all the packages synchronously
                await DownloadPackage(package);

                // we'll extract the runtime installer later if we need to
                if (package.Name == "WebView2RuntimeInstaller.zip")
                    continue;

                // extract the package immediately after download asynchronously
                // discard is just used to suppress the warning
                Task _ = ExtractPackage(package);
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

            // wait for all packages to finish extracting, with an exception for the webview2 runtime installer
            while (_packagesExtracted < _versionPackageManifest.Where(x => x.Name != "WebView2RuntimeInstaller.zip").Count())
            {
                await Task.Delay(100);
            }

            string appSettingsLocation = Path.Combine(_versionFolder, "AppSettings.xml");
            await File.WriteAllTextAsync(appSettingsLocation, AppSettings);

            if (_cancelFired)
                return;

            if (!FreshInstall)
            {
                // let's take this opportunity to delete any packages we don't need anymore
                foreach (string filename in cachedPackages)
                {
                    if (!_versionPackageManifest.Exists(package => filename.Contains(package.Signature)))
                    {
                        App.Logger.WriteLine($"[Bootstrapper::InstallLatestVersion] Deleting unused package {filename}");
                        File.Delete(filename);
                    }
                }

                string oldVersionFolder = Path.Combine(Directories.Versions, App.State.Prop.VersionGuid);

                // move old compatibility flags for the old location
                using (RegistryKey appFlagsKey = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers"))
                {
                    string oldGameClientLocation = Path.Combine(oldVersionFolder, "RobloxPlayerBeta.exe");
                    string? appFlags = (string?)appFlagsKey.GetValue(oldGameClientLocation);

                    if (appFlags is not null)
                    {
                        App.Logger.WriteLine($"[Bootstrapper::InstallLatestVersion] Migrating app compatibility flags from {oldGameClientLocation} to {_playerLocation}...");
                        appFlagsKey.SetValue(_playerLocation, appFlags);
                        appFlagsKey.DeleteValue(oldGameClientLocation);
                    }
                }

                // delete any old version folders
                // we only do this if roblox isnt running just in case an update happened
                // while they were launching a second instance or something idk
                if (!Process.GetProcessesByName(App.RobloxAppName).Any())
                {
                    foreach (DirectoryInfo dir in new DirectoryInfo(Directories.Versions).GetDirectories())
                    {
                        if (dir.Name == _latestVersionGuid || !dir.Name.StartsWith("version-"))
                            continue;

                        App.Logger.WriteLine($"[Bootstrapper::InstallLatestVersion] Removing old version folder for {dir.Name}");
                        dir.Delete(true);
                    }
                }
            }

            App.State.Prop.VersionGuid = _latestVersionGuid;

            // don't register program size until the program is registered, which will be done after this
            if (!App.IsFirstRun && !FreshInstall)
                RegisterProgramSize();

            if (Dialog is not null)
                Dialog.CancelEnabled = false;

            _isInstalling = false;
        }
        
        private async Task InstallWebView2()
        {
            // check if the webview2 runtime needs to be installed
            // webview2 can either be installed be per-user or globally, so we need to check in both hklm and hkcu
            // https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution#detect-if-a-suitable-webview2-runtime-is-already-installed

            using RegistryKey? hklmKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            using RegistryKey? hkcuKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");

            if (hklmKey is not null || hkcuKey is not null)
                return;

            App.Logger.WriteLine($"[Bootstrapper::InstallWebView2] Installing runtime...");

            string baseDirectory = Path.Combine(_versionFolder, "WebView2RuntimeInstaller");

            if (!Directory.Exists(baseDirectory))
            {
                Package? package = _versionPackageManifest.Find(x => x.Name == "WebView2RuntimeInstaller.zip");

                if (package is null)
                {
                    App.Logger.WriteLine($"[Bootstrapper::InstallWebView2] Aborted runtime install because package does not exist, has WebView2 been added in this Roblox version yet?");
                    return;
                }

                await ExtractPackage(package);
            }

            SetStatus("Installing WebView2, please wait...");

            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = baseDirectory,
                FileName = Path.Combine(baseDirectory, "MicrosoftEdgeWebview2Setup.exe"),
                Arguments = "/silent /install"
            };

            await Process.Start(startInfo)!.WaitForExitAsync();

            App.Logger.WriteLine($"[Bootstrapper::InstallWebView2] Finished installing runtime");
        }

        public static void MigrateIntegrations()
        {
            // v2.2.0 - remove rbxfpsunlocker
            string rbxfpsunlocker = Path.Combine(Directories.Integrations, "rbxfpsunlocker");

            if (Directory.Exists(rbxfpsunlocker))
                Directory.Delete(rbxfpsunlocker, true);

            // v2.3.0 - remove reshade
            string injectorLocation = Path.Combine(Directories.Modifications, "dxgi.dll");
            string configLocation = Path.Combine(Directories.Modifications, "ReShade.ini");

            if (File.Exists(injectorLocation))
            {
                App.ShowMessageBox(
                    "Roblox has now finished rolling out the new game client update, featuring 64-bit support and the Hyperion anticheat. ReShade does not work with this update, and so it has now been disabled and removed from Bloxstrap.\n\n"+
                    "Your ReShade configuration files will still be saved, and you can locate them by opening the folder where Bloxstrap is installed to, and navigating to the Integrations folder. You can choose to delete these if you want.", 
                    MessageBoxImage.Warning
                );

                File.Delete(injectorLocation);
            }

            if (File.Exists(configLocation))
                File.Delete(configLocation);
        }

        private async Task ApplyModifications()
        {
            SetStatus("Applying Roblox modifications...");

            // set executable flags for fullscreen optimizations
            App.Logger.WriteLine("[Bootstrapper::ApplyModifications] Checking executable flags...");
            using (RegistryKey appFlagsKey = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers"))
            {
                const string flag = " DISABLEDXMAXIMIZEDWINDOWEDMODE";
                string? appFlags = (string?)appFlagsKey.GetValue(_playerLocation);

                if (App.Settings.Prop.DisableFullscreenOptimizations)
                {
                    if (appFlags is null)
                        appFlagsKey.SetValue(_playerLocation, $"~{flag}");
                    else if (!appFlags.Contains(flag))
                        appFlagsKey.SetValue(_playerLocation, appFlags + flag);
                }
                else if (appFlags is not null && appFlags.Contains(flag))
                {
                    // if there's more than one space, there's more flags set we need to preserve
                    if (appFlags.Split(' ').Length > 2)
                        appFlagsKey.SetValue(_playerLocation, appFlags.Remove(appFlags.IndexOf(flag), flag.Length));
                    else
                        appFlagsKey.DeleteValue(_playerLocation);
                }
            }

            // handle file mods
            App.Logger.WriteLine("[Bootstrapper::ApplyModifications] Checking file mods...");
            string modFolder = Path.Combine(Directories.Modifications);

            // manifest has been moved to State.json
            File.Delete(Path.Combine(Directories.Base, "ModManifest.txt"));

            List<string> modFolderFiles = new();

            if (!Directory.Exists(modFolder))
                Directory.CreateDirectory(modFolder);

            await CheckModPreset(App.Settings.Prop.UseOldDeathSound, @"content\sounds\ouch.ogg", "OldDeath.ogg");
            await CheckModPreset(App.Settings.Prop.UseOldMouseCursor, @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png", "OldCursor.png");
            await CheckModPreset(App.Settings.Prop.UseOldMouseCursor, @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "OldFarCursor.png");
            await CheckModPreset(App.Settings.Prop.UseOldCharacterSounds, @"content\sounds\action_footsteps_plastic.mp3", "OldWalk.mp3");
            await CheckModPreset(App.Settings.Prop.UseOldCharacterSounds, @"content\sounds\action_jump.mp3", "OldJump.mp3");
            await CheckModPreset(App.Settings.Prop.UseOldCharacterSounds, @"content\sounds\action_falling.mp3", "Empty.mp3");
            await CheckModPreset(App.Settings.Prop.UseOldCharacterSounds, @"content\sounds\action_jump_land.mp3", "Empty.mp3");
            await CheckModPreset(App.Settings.Prop.UseOldCharacterSounds, @"content\sounds\action_swim.mp3", "Empty.mp3");
            await CheckModPreset(App.Settings.Prop.UseOldCharacterSounds, @"content\sounds\impact_water.mp3", "Empty.mp3");
            await CheckModPreset(App.Settings.Prop.UseDisableAppPatch, @"ExtraContent\places\Mobile.rbxl", "");

            // emoji presets are downloaded remotely from github due to how large they are
            string emojiFontLocation = Path.Combine(Directories.Modifications, "content\\fonts\\TwemojiMozilla.ttf");

            if (App.Settings.Prop.PreferredEmojiType == EmojiType.Default && App.State.Prop.CurrentEmojiType != EmojiType.Default)
            {
                if (File.Exists(emojiFontLocation))
                    File.Delete(emojiFontLocation);

                App.State.Prop.CurrentEmojiType = EmojiType.Default;
            }
            else if (App.Settings.Prop.PreferredEmojiType != EmojiType.Default && App.State.Prop.CurrentEmojiType != App.Settings.Prop.PreferredEmojiType)
            {
                if (File.Exists(emojiFontLocation))
                    File.Delete(emojiFontLocation);

                string remoteEmojiLocation = App.Settings.Prop.PreferredEmojiType.GetRemoteLocation();

                var response = await App.HttpClient.GetAsync(remoteEmojiLocation);
                await using var fileStream = new FileStream(emojiFontLocation, FileMode.CreateNew);
                await response.Content.CopyToAsync(fileStream);

                App.State.Prop.CurrentEmojiType = App.Settings.Prop.PreferredEmojiType;
            }

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
                    packageDirectory = PackageDirectories.First(x => x.Value != "" && fileLocation.StartsWith(x.Value));
                }
                catch (InvalidOperationException)
                {
                    // package doesn't exist, likely mistakenly placed file
                    string versionFileLocation = Path.Combine(_versionFolder, fileLocation);

                    if (File.Exists(versionFileLocation))
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
            byte[] binaryData = string.IsNullOrEmpty(name) ? Array.Empty<byte>() : await Resource.Get(name);

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

            string packageUrl = RobloxDeployment.GetLocation($"/{_latestVersionGuid}-{package.Name}");
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

        private async Task ExtractPackage(Package package)
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
