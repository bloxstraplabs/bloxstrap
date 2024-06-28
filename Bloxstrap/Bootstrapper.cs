using System.Windows;
using System.Windows.Forms;

using Microsoft.Win32;

using Bloxstrap.Integrations;
using Bloxstrap.Resources;

namespace Bloxstrap
{
    public class Bootstrapper
    {
        #region Properties
        private const int ProgressBarMaximum = 10000;
      
        private const string AppSettings =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
            "<Settings>\r\n" +
            "	<ContentFolder>content</ContentFolder>\r\n" +
            "	<BaseUrl>http://www.roblox.com</BaseUrl>\r\n" +
            "</Settings>\r\n";

        private readonly CancellationTokenSource _cancelTokenSource = new();

        private bool FreshInstall => String.IsNullOrEmpty(_versionGuid);

        private string _playerFileName => _launchMode == LaunchMode.Player ? "RobloxPlayerBeta.exe" : "RobloxStudioBeta.exe";
        // TODO: change name
        private string _playerLocation => Path.Combine(_versionFolder, _playerFileName);

        private string _launchCommandLine;
        private LaunchMode _launchMode;

        private string _versionGuid
        {
            get
            {
                return _launchMode == LaunchMode.Player ? App.State.Prop.PlayerVersionGuid : App.State.Prop.StudioVersionGuid;
            }

            set
            {
                if (_launchMode == LaunchMode.Player)
                    App.State.Prop.PlayerVersionGuid = value;
                else
                   App.State.Prop.StudioVersionGuid = value;
            }
        }

        private int _distributionSize
        {
            get
            {
                return _launchMode == LaunchMode.Player ? App.State.Prop.PlayerSize : App.State.Prop.StudioSize;
            }

            set
            {
                if (_launchMode == LaunchMode.Player)
                    App.State.Prop.PlayerSize = value;
                else
                    App.State.Prop.StudioSize = value;
            }
        }

        private string _latestVersionGuid = null!;
        private PackageManifest _versionPackageManifest = null!;
        private string _versionFolder = null!;

        private bool _isInstalling = false;
        private double _progressIncrement;
        private long _totalDownloadedBytes = 0;
        private int _packagesExtracted = 0;
        private bool _cancelFired = false;

        private IReadOnlyDictionary<string, string> _packageDirectories;

        public IBootstrapperDialog? Dialog = null;

        public bool IsStudioLaunch => _launchMode != LaunchMode.Player;
        #endregion

        #region Core
        public Bootstrapper(string launchCommandLine, LaunchMode launchMode)
        {
            _launchCommandLine = launchCommandLine;
            _launchMode = launchMode;

            _packageDirectories = _launchMode == LaunchMode.Player ? PackageMap.Player : PackageMap.Studio;
        }

        private void SetStatus(string message)
        {
            App.Logger.WriteLine("Bootstrapper::SetStatus", message);

            string productName = "Roblox";

            if (_launchMode != LaunchMode.Player)
                productName += " Studio";

            message = message.Replace("{product}", productName);

            if (Dialog is not null)
                Dialog.Message = message;
        }

        private void UpdateProgressBar()
        {
            if (Dialog is null)
                return;

            int progressValue = (int)Math.Floor(_progressIncrement * _totalDownloadedBytes);

            // bugcheck: if we're restoring a file from a package, it'll incorrectly increment the progress beyond 100
            // too lazy to fix properly so lol
            progressValue = Math.Clamp(progressValue, 0, ProgressBarMaximum);

            Dialog.ProgressValue = progressValue;
        }
        
        public async Task Run()
        {
            const string LOG_IDENT = "Bootstrapper::Run";

            App.Logger.WriteLine(LOG_IDENT, "Running bootstrapper");

            if (App.LaunchSettings.IsUninstall)
            {
                Uninstall();
                return;
            }

            // connectivity check

            App.Logger.WriteLine(LOG_IDENT, "Performing connectivity check...");

            SetStatus(Resources.Strings.Bootstrapper_Status_Connecting);

            try
            {
                await RobloxDeployment.GetInfo(RobloxDeployment.DefaultChannel);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Connectivity check failed!");
                App.Logger.WriteException(LOG_IDENT, ex);

                string message = Resources.Strings.Bootstrapper_Connectivity_Preventing;

                if (ex.GetType() == typeof(HttpResponseException))
                    message = Resources.Strings.Bootstrapper_Connectivity_RobloxDown;
                else if (ex.GetType() == typeof(TaskCanceledException))
                    message = Resources.Strings.Bootstrapper_Connectivity_TimedOut;
                else if (ex.GetType() == typeof(AggregateException))
                    ex = ex.InnerException!;

                Frontend.ShowConnectivityDialog("Roblox", message, ex);

                App.Terminate(ErrorCode.ERROR_CANCELLED);
            }
            finally
            {
                App.Logger.WriteLine(LOG_IDENT, "Connectivity check finished");
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
                Mutex.OpenExisting("Bloxstrap_SingletonMutex").Close();
                App.Logger.WriteLine(LOG_IDENT, "Bloxstrap_SingletonMutex mutex exists, waiting...");
                SetStatus(Resources.Strings.Bootstrapper_Status_WaitingOtherInstances);
                mutexExists = true;
            }
            catch (Exception)
            {
                // no mutex exists
            }

            // wait for mutex to be released if it's not yet
            await using var mutex = new AsyncMutex(true, "Bloxstrap_SingletonMutex");
            await mutex.AcquireAsync(_cancelTokenSource.Token);

            // reload our configs since they've likely changed by now
            if (mutexExists)
            {
                App.Settings.Load();
                App.State.Load();
            }

            await CheckLatestVersion();

            // install/update roblox if we're running for the first time, needs updating, or the player location doesn't exist
            if (App.IsFirstRun || _latestVersionGuid != _versionGuid || !File.Exists(_playerLocation))
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

            if (App.IsFirstRun && App.LaunchSettings.IsNoLaunch)
                Dialog?.ShowSuccess(Resources.Strings.Bootstrapper_SuccessfullyInstalled);
            else if (!App.LaunchSettings.IsNoLaunch && !_cancelFired)
                await StartRoblox();
        }

        private async Task CheckLatestVersion()
        {
            const string LOG_IDENT = "Bootstrapper::CheckLatestVersion";

            ClientVersion clientVersion;

            string binaryType = _launchMode == LaunchMode.Player ? "WindowsPlayer" : "WindowsStudio64";

            try
            {
                clientVersion = await RobloxDeployment.GetInfo(App.Settings.Prop.Channel, binaryType: binaryType);
            }
            catch (HttpResponseException ex)
            {
                if (ex.ResponseMessage.StatusCode is not HttpStatusCode.Unauthorized and not HttpStatusCode.Forbidden and not HttpStatusCode.NotFound)
                    throw;

                App.Logger.WriteLine(LOG_IDENT, $"Reverting enrolled channel to {RobloxDeployment.DefaultChannel} because HTTP {(int)ex.ResponseMessage.StatusCode}");
                App.Settings.Prop.Channel = RobloxDeployment.DefaultChannel;
                clientVersion = await RobloxDeployment.GetInfo(App.Settings.Prop.Channel, binaryType: binaryType);
            }

            if (clientVersion.IsBehindDefaultChannel)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Changed Roblox channel from {App.Settings.Prop.Channel} to {RobloxDeployment.DefaultChannel}");

                App.Settings.Prop.Channel = RobloxDeployment.DefaultChannel;
                clientVersion = await RobloxDeployment.GetInfo(App.Settings.Prop.Channel, binaryType: binaryType);
            }

            _latestVersionGuid = clientVersion.VersionGuid;
            _versionFolder = Path.Combine(Paths.Versions, _latestVersionGuid);
            _versionPackageManifest = await PackageManifest.Get(_latestVersionGuid);
        }

        private async Task StartRoblox()
        {
            const string LOG_IDENT = "Bootstrapper::StartRoblox";

            SetStatus(Resources.Strings.Bootstrapper_Status_Starting);

            if (!File.Exists(Path.Combine(Paths.System, "mfplat.dll")))
            {
                Frontend.ShowMessageBox(
                    Resources.Strings.Bootstrapper_WMFNotFound, 
                    MessageBoxImage.Error
                );

                if (!App.LaunchSettings.IsQuiet)
                    Utilities.ShellExecute("https://support.microsoft.com/en-us/topic/media-feature-pack-list-for-windows-n-editions-c1c6fffa-d052-8338-7a79-a4bb980a700a");

                Dialog?.CloseBootstrapper();
                return;
            }

            if (_launchMode != LaunchMode.StudioAuth)
            {
                _launchCommandLine = _launchCommandLine.Replace("LAUNCHTIMEPLACEHOLDER", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());


                if (_launchCommandLine.StartsWith("roblox-player:1"))
                    _launchCommandLine += "+channel:";
                else
                    _launchCommandLine += " -channel ";

                if (App.Settings.Prop.Channel.ToLowerInvariant() == RobloxDeployment.DefaultChannel.ToLowerInvariant())
                    _launchCommandLine += "production";
                else
                    _launchCommandLine += App.Settings.Prop.Channel.ToLowerInvariant();

                if (App.Settings.Prop.ForceRobloxLanguage)
                {
                    var match = Regex.Match(_launchCommandLine, "gameLocale:([a-z_]+)");

                    if (match.Groups.Count == 2)
                        _launchCommandLine = _launchCommandLine.Replace("robloxLocale:en_us", $"robloxLocale:{match.Groups[1].Value}");
                }
            }

            // whether we should wait for roblox to exit to handle stuff in the background or clean up after roblox closes
            bool shouldWait = false;

            var startInfo = new ProcessStartInfo()
            {
                FileName = _playerLocation,
                Arguments = _launchCommandLine,
                WorkingDirectory = _versionFolder
            };

            if (_launchMode == LaunchMode.StudioAuth)
            {
                Process.Start(startInfo);
                Dialog?.CloseBootstrapper();
                return;
            }

            // v2.2.0 - byfron will trip if we keep a process handle open for over a minute, so we're doing this now
            int gameClientPid;
            using (Process gameClient = Process.Start(startInfo)!)
            {
                gameClientPid = gameClient.Id;
            }

            List<Process> autocloseProcesses = new();
            ActivityWatcher? activityWatcher = null;
            DiscordRichPresence? richPresence = null;

            App.Logger.WriteLine(LOG_IDENT, $"Started Roblox (PID {gameClientPid})");

            string eventName = _launchMode == LaunchMode.Player ? "www.roblox.com/robloxStartedEvent" : "www.roblox.com/robloxQTStudioStartedEvent";
            using (SystemEvent startEvent = new(eventName))
            {
                bool startEventFired = await startEvent.WaitForEvent();

                startEvent.Close();

                if (!startEventFired)
                    return;
            }

            if (App.Settings.Prop.EnableActivityTracking && _launchMode == LaunchMode.Player)
              App.NotifyIcon?.SetProcessId(gameClientPid);

            if (App.Settings.Prop.EnableActivityTracking)
            {
                activityWatcher = new(gameClientPid);
                shouldWait = true;

                App.NotifyIcon?.SetActivityWatcher(activityWatcher);

                if (App.Settings.Prop.UseDiscordRichPresence)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Using Discord Rich Presence");
                    richPresence = new(activityWatcher);

                    App.NotifyIcon?.SetRichPresenceHandler(richPresence);
                }
            }

            // launch custom integrations now
            foreach (CustomIntegration integration in App.Settings.Prop.CustomIntegrations)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Launching custom integration '{integration.Name}' ({integration.Location} {integration.LaunchArgs} - autoclose is {integration.AutoClose})");

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
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to launch integration '{integration.Name}'!");
                    App.Logger.WriteLine(LOG_IDENT, $"{ex.Message}");
                }
            }

            // event fired, wait for 3 seconds then close
            await Task.Delay(3000);
            Dialog?.CloseBootstrapper();

            // keep bloxstrap open in the background if needed
            if (!shouldWait)
                return;

            activityWatcher?.StartWatcher();

            App.Logger.WriteLine(LOG_IDENT, "Waiting for Roblox to close");

            while (Utilities.GetProcessesSafe().Any(x => x.Id == gameClientPid))
                await Task.Delay(1000);

            App.Logger.WriteLine(LOG_IDENT, $"Roblox has exited");

            richPresence?.Dispose();

            foreach (Process process in autocloseProcesses)
            {
                if (process.HasExited)
                    continue;

                App.Logger.WriteLine(LOG_IDENT, $"Autoclosing process '{process.ProcessName}' (PID {process.Id})");
                process.Kill();
            }
        }

        public void CancelInstall()
        {
            const string LOG_IDENT = "Bootstrapper::CancelInstall";

            if (!_isInstalling)
            {
                App.Terminate(ErrorCode.ERROR_CANCELLED);
                return;
            }

            if (_cancelFired)
                return;

            App.Logger.WriteLine(LOG_IDENT, "Cancelling install...");

            _cancelTokenSource.Cancel();
            _cancelFired = true;

            try
            {
                // clean up install
                if (App.IsFirstRun)
                    Directory.Delete(Paths.Base, true);
                else if (Directory.Exists(_versionFolder))
                    Directory.Delete(_versionFolder, true);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not fully clean up installation!");
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            Dialog?.CloseBootstrapper();

            App.Terminate(ErrorCode.ERROR_CANCELLED);
        }
        #endregion

        #region App Install
        public static void Register()
        {
            const string LOG_IDENT = "Bootstrapper::Register";

            using (RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{App.ProjectName}"))
            {
                applicationKey.SetValue("InstallLocation", Paths.Base);
            }

            // set uninstall key
            using (RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{App.ProjectName}"))
            {
                uninstallKey.SetValue("DisplayIcon", $"{Paths.Application},0");
                uninstallKey.SetValue("DisplayName", App.ProjectName);
                uninstallKey.SetValue("DisplayVersion", App.Version);

                if (uninstallKey.GetValue("InstallDate") is null)
                    uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

                uninstallKey.SetValue("InstallLocation", Paths.Base);
                uninstallKey.SetValue("NoRepair", 1);
                uninstallKey.SetValue("Publisher", "pizzaboxer");
                uninstallKey.SetValue("ModifyPath", $"\"{Paths.Application}\" -menu");
                uninstallKey.SetValue("QuietUninstallString", $"\"{Paths.Application}\" -uninstall -quiet");
                uninstallKey.SetValue("UninstallString", $"\"{Paths.Application}\" -uninstall");
                uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{App.ProjectRepository}");
                uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{App.ProjectRepository}/releases/latest");
            }

            App.Logger.WriteLine(LOG_IDENT, "Registered application");
        }

        public void RegisterProgramSize()
        {
            const string LOG_IDENT = "Bootstrapper::RegisterProgramSize";

            App.Logger.WriteLine(LOG_IDENT, "Registering approximate program size...");

            using RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{App.ProjectName}");

            // sum compressed and uncompressed package sizes and convert to kilobytes
            int distributionSize = (_versionPackageManifest.Sum(x => x.Size) + _versionPackageManifest.Sum(x => x.PackedSize)) / 1000;
            _distributionSize = distributionSize;

            int totalSize = App.State.Prop.PlayerSize + App.State.Prop.StudioSize;

            uninstallKey.SetValue("EstimatedSize", totalSize);

            App.Logger.WriteLine(LOG_IDENT, $"Registered as {totalSize} KB");
        }

        public static void CheckInstall()
        {
            const string LOG_IDENT = "Bootstrapper::CheckInstall";

            App.Logger.WriteLine(LOG_IDENT, "Checking install");

            // check if launch uri is set to our bootstrapper
            // this doesn't go under register, so we check every launch
            // just in case the stock bootstrapper changes it back

            ProtocolHandler.Register("roblox", "Roblox", Paths.Application);
            ProtocolHandler.Register("roblox-player", "Roblox", Paths.Application);
#if STUDIO_FEATURES
            ProtocolHandler.Register("roblox-studio", "Roblox", Paths.Application);
            ProtocolHandler.Register("roblox-studio-auth", "Roblox", Paths.Application);

            ProtocolHandler.RegisterRobloxPlace(Paths.Application);
            ProtocolHandler.RegisterExtension(".rbxl");
            ProtocolHandler.RegisterExtension(".rbxlx");
#endif

            if (Environment.ProcessPath is not null && Environment.ProcessPath != Paths.Application)
            {
                // in case the user is reinstalling
                if (File.Exists(Paths.Application) && App.IsFirstRun)
                {
                    Filesystem.AssertReadOnly(Paths.Application);
                    File.Delete(Paths.Application);
                }

                // check to make sure bootstrapper is in the install folder
                if (!File.Exists(Paths.Application))
                    File.Copy(Environment.ProcessPath, Paths.Application);
            }

            // this SHOULD go under Register(),
            // but then people who have Bloxstrap v1.0.0 installed won't have this without a reinstall
            // maybe in a later version?
            if (!Directory.Exists(Paths.StartMenu))
            {
                Directory.CreateDirectory(Paths.StartMenu);
            }
            else
            {
                // v2.0.0 - rebadge configuration menu as just "Bloxstrap Menu"
                string oldMenuShortcut = Path.Combine(Paths.StartMenu, $"Configure {App.ProjectName}.lnk");

                if (File.Exists(oldMenuShortcut))
                    File.Delete(oldMenuShortcut);
            }

            Utility.Shortcut.Create(Paths.Application, "", Path.Combine(Paths.StartMenu, "Play Roblox.lnk"));
            Utility.Shortcut.Create(Paths.Application, "-menu", Path.Combine(Paths.StartMenu, $"{App.ProjectName} Menu.lnk"));
#if STUDIO_FEATURES
            Utility.Shortcut.Create(Paths.Application, "-ide", Path.Combine(Paths.StartMenu, $"Roblox Studio ({App.ProjectName}).lnk"));
#endif

            if (App.Settings.Prop.CreateDesktopIcon)
            {
                try
                {
                    Utility.Shortcut.Create(Paths.Application, "", Path.Combine(Paths.Desktop, "Play Roblox.lnk"));

                    // one-time toggle, set it back to false
                    App.Settings.Prop.CreateDesktopIcon = false;
                }
                catch (Exception)
                {
                    // suppress, we likely just don't have write perms for the desktop folder
                }
            }
        }

        private async Task CheckForUpdates()
        {
            const string LOG_IDENT = "Bootstrapper::CheckForUpdates";
            
            // don't update if there's another instance running (likely running in the background)
            if (Process.GetProcessesByName(App.ProjectName).Count() > 1)
            {
                App.Logger.WriteLine(LOG_IDENT, $"More than one Bloxstrap instance running, aborting update check");
                return;
            }

            App.Logger.WriteLine(LOG_IDENT, $"Checking for updates...");

            GithubRelease? releaseInfo;
            try
            {
                releaseInfo = await Http.GetJson<GithubRelease>($"https://api.github.com/repos/{App.ProjectRepository}/releases/latest");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to fetch releases: {ex}");
                return;
            }

            if (releaseInfo is null || releaseInfo.Assets is null)
            {
                App.Logger.WriteLine(LOG_IDENT, $"No updates found");
                return;
            }

            int versionComparison = Utilities.CompareVersions(App.Version, releaseInfo.TagName);

            // check if we aren't using a deployed build, so we can update to one if a new version comes out
            if (versionComparison == 0 && App.BuildMetadata.CommitRef.StartsWith("tag") || versionComparison == 1)
            {
                App.Logger.WriteLine(LOG_IDENT, $"No updates found");
                return;
            }

            SetStatus(Resources.Strings.Bootstrapper_Status_UpgradingBloxstrap);
            
            try
            {
                // 64-bit is always the first option
                GithubReleaseAsset asset = releaseInfo.Assets[0];
                string downloadLocation = Path.Combine(Paths.LocalAppData, "Temp", asset.Name);

                App.Logger.WriteLine(LOG_IDENT, $"Downloading {releaseInfo.TagName}...");
                
                if (!File.Exists(downloadLocation))
                {
                    var response = await App.HttpClient.GetAsync(asset.BrowserDownloadUrl);

                    await using var fileStream = new FileStream(downloadLocation, FileMode.CreateNew);
                    await response.Content.CopyToAsync(fileStream);
                }

                App.Logger.WriteLine(LOG_IDENT, $"Starting {releaseInfo.TagName}...");

                ProcessStartInfo startInfo = new()
                {
                    FileName = downloadLocation,
                };

                foreach (string arg in App.LaunchSettings.Args)
                    startInfo.ArgumentList.Add(arg);
                
                App.Settings.Save();
                App.ShouldSaveConfigs = false;
                
                Process.Start(startInfo);

                App.Terminate();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the auto-updater");
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(
                    string.Format(Resources.Strings.Bootstrapper_AutoUpdateFailed, releaseInfo.TagName),
                    MessageBoxImage.Information
                );
            }
        }

        private void Uninstall()
        {
            const string LOG_IDENT = "Bootstrapper::Uninstall";
            
            // prompt to shutdown roblox if its currently running
            if (Process.GetProcessesByName(App.RobloxPlayerAppName).Any() || Process.GetProcessesByName(App.RobloxStudioAppName).Any())
            {
                App.Logger.WriteLine(LOG_IDENT, $"Prompting to shut down all open Roblox instances");
                
                MessageBoxResult result = Frontend.ShowMessageBox(
                    Resources.Strings.Bootstrapper_Uninstall_RobloxRunning,
                    MessageBoxImage.Information,
                    MessageBoxButton.OKCancel
                );

                if (result != MessageBoxResult.OK)
                    App.Terminate(ErrorCode.ERROR_CANCELLED);

                try
                {
                    foreach (Process process in Process.GetProcessesByName(App.RobloxPlayerAppName))
                    {
                        process.Kill();
                        process.Close();
                    }

#if STUDIO_FEATURES
                    foreach (Process process in Process.GetProcessesByName(App.RobloxStudioAppName))
                    {
                        process.Kill();
                        process.Close();
                    }
#endif
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to close process! {ex}");
                }

                App.Logger.WriteLine(LOG_IDENT, $"All Roblox processes closed");
            }
            
            SetStatus(Resources.Strings.Bootstrapper_Status_Uninstalling);

            App.ShouldSaveConfigs = false;
            bool robloxPlayerStillInstalled = true;
            bool robloxStudioStillInstalled = true;

            // check if stock bootstrapper is still installed
            using RegistryKey? bootstrapperKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\roblox-player");
            if (bootstrapperKey is null)
            {
                robloxPlayerStillInstalled = false;

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

#if STUDIO_FEATURES
            using RegistryKey? studioBootstrapperKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\roblox-studio");
            if (studioBootstrapperKey is null)
            {
                robloxStudioStillInstalled = false;

                ProtocolHandler.Unregister("roblox-studio");
                ProtocolHandler.Unregister("roblox-studio-auth");

                ProtocolHandler.Unregister("Roblox.Place");
                ProtocolHandler.Unregister(".rbxl");
                ProtocolHandler.Unregister(".rbxlx");
            }
            else
            {
                string studioLocation = (string?)studioBootstrapperKey.GetValue("InstallLocation") + "RobloxStudioBeta.exe"; // points to studio exe instead of bootstrapper
                ProtocolHandler.Register("roblox-studio", "Roblox", studioLocation);
                ProtocolHandler.Register("roblox-studio-auth", "Roblox", studioLocation);

                ProtocolHandler.RegisterRobloxPlace(studioLocation);
            }
#endif

            // if the folder we're installed to does not end with "Bloxstrap", we're installed to a user-selected folder
            // in which case, chances are they chose to install to somewhere they didn't really mean to (prior to the added warning in 2.4.0)
            // if so, we're walking on eggshells and have to ensure we only clean up what we need to clean up
            bool cautiousUninstall = !Paths.Base.ToLower().EndsWith(App.ProjectName.ToLower());

            var cleanupSequence = new List<Action>
            {
                () => Registry.CurrentUser.DeleteSubKey($@"Software\{App.ProjectName}"),
                () => Directory.Delete(Paths.StartMenu, true),
                () => File.Delete(Path.Combine(Paths.Desktop, "Play Roblox.lnk")),
                () => Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{App.ProjectName}")
            };

            if (cautiousUninstall)
            {
                cleanupSequence.Add(() => Directory.Delete(Paths.Downloads, true));
                cleanupSequence.Add(() => Directory.Delete(Paths.Modifications, true));
                cleanupSequence.Add(() => Directory.Delete(Paths.Versions, true));
                cleanupSequence.Add(() => Directory.Delete(Paths.Logs, true));
                
                cleanupSequence.Add(() => File.Delete(App.Settings.FileLocation));
                cleanupSequence.Add(() => File.Delete(App.State.FileLocation));
            }
            else
            {
                cleanupSequence.Add(() => Directory.Delete(Paths.Base, true));
            }

            string robloxFolder = Path.Combine(Paths.LocalAppData, "Roblox");

            if (!robloxPlayerStillInstalled && !robloxStudioStillInstalled && Directory.Exists(robloxFolder))
                cleanupSequence.Add(() => Directory.Delete(robloxFolder, true));

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

            Action? callback = null;

            if (Directory.Exists(Paths.Base))
            {
                callback = delegate
                {
                    // this is definitely one of the workaround hacks of all time
                    // could antiviruses falsely detect this as malicious behaviour though?
                    // "hmm whats this program doing running a cmd command chain quietly in the background that auto deletes an entire folder"

                    string deleteCommand;

                    if (cautiousUninstall)
                        deleteCommand = $"del /Q \"{Paths.Application}\"";
                    else
                        deleteCommand = $"del /Q \"{Paths.Base}\\*\" && rmdir \"{Paths.Base}\"";

                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c timeout 5 && {deleteCommand}",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                };
            }

            Dialog?.ShowSuccess(Resources.Strings.Bootstrapper_SuccessfullyUninstalled, callback);
        }
#endregion

        #region Roblox Install
        private async Task InstallLatestVersion()
        {
            const string LOG_IDENT = "Bootstrapper::InstallLatestVersion";
            
            _isInstalling = true;

            SetStatus(FreshInstall ? Resources.Strings.Bootstrapper_Status_Installing : Resources.Strings.Bootstrapper_Status_Upgrading);

            Directory.CreateDirectory(Paths.Base);
            Directory.CreateDirectory(Paths.Downloads);
            Directory.CreateDirectory(Paths.Versions);

            // package manifest states packed size and uncompressed size in exact bytes
            // packed size only matters if we don't already have the package cached on disk
            string[] cachedPackages = Directory.GetFiles(Paths.Downloads);
            int totalSizeRequired = _versionPackageManifest.Where(x => !cachedPackages.Contains(x.Signature)).Sum(x => x.PackedSize) + _versionPackageManifest.Sum(x => x.Size);
            
            if (Filesystem.GetFreeDiskSpace(Paths.Base) < totalSizeRequired)
            {
                Frontend.ShowMessageBox(
                    Resources.Strings.Bootstrapper_NotEnoughSpace, 
                    MessageBoxImage.Error
                );

                App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
                return;
            }

            if (Dialog is not null)
            {
                Dialog.CancelEnabled = true;
                Dialog.ProgressStyle = ProgressBarStyle.Continuous;

                Dialog.ProgressMaximum = ProgressBarMaximum;

                // compute total bytes to download
                _progressIncrement = (double)ProgressBarMaximum / _versionPackageManifest.Sum(package => package.PackedSize);
            }

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
                _ = Task.Run(() => ExtractPackage(package).ContinueWith(AsyncHelpers.ExceptionHandler, $"extracting {package.Name}"));
            }

            if (_cancelFired) 
                return;

            // allow progress bar to 100% before continuing (purely ux reasons lol)
            await Task.Delay(1000);

            if (Dialog is not null)
            {
                Dialog.ProgressStyle = ProgressBarStyle.Marquee;
                SetStatus(Resources.Strings.Bootstrapper_Status_Configuring);
            }

            // wait for all packages to finish extracting, with an exception for the webview2 runtime installer
            while (_packagesExtracted < _versionPackageManifest.Where(x => x.Name != "WebView2RuntimeInstaller.zip").Count())
            {
                await Task.Delay(100);
            }

            App.Logger.WriteLine(LOG_IDENT, "Writing AppSettings.xml...");
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
                        App.Logger.WriteLine(LOG_IDENT, $"Deleting unused package {filename}");
                        
                        try
                        {
                            File.Delete(filename);
                        }
                        catch (Exception ex)
                        {
                            App.Logger.WriteLine(LOG_IDENT, $"Failed to delete {filename}!");
                            App.Logger.WriteException(LOG_IDENT, ex);
                        }
                    }
                }

                string oldVersionFolder = Path.Combine(Paths.Versions, _versionGuid);

                // move old compatibility flags for the old location
                using (RegistryKey appFlagsKey = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers"))
                {
                    string oldGameClientLocation = Path.Combine(oldVersionFolder, _playerFileName);
                    string? appFlags = (string?)appFlagsKey.GetValue(oldGameClientLocation);

                    if (appFlags is not null)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Migrating app compatibility flags from {oldGameClientLocation} to {_playerLocation}...");
                        appFlagsKey.SetValue(_playerLocation, appFlags);
                        appFlagsKey.DeleteValue(oldGameClientLocation);
                    }
                }
            }

            _versionGuid = _latestVersionGuid;

            // delete any old version folders
            // we only do this if roblox isnt running just in case an update happened
            // while they were launching a second instance or something idk
#if STUDIO_FEATURES
            if (!Process.GetProcessesByName(App.RobloxPlayerAppName).Any() && !Process.GetProcessesByName(App.RobloxStudioAppName).Any())
#else
            if (!Process.GetProcessesByName(App.RobloxPlayerAppName).Any())
#endif
            {
                foreach (DirectoryInfo dir in new DirectoryInfo(Paths.Versions).GetDirectories())
                {
                    if (dir.Name == App.State.Prop.PlayerVersionGuid || dir.Name == App.State.Prop.StudioVersionGuid || !dir.Name.StartsWith("version-"))
                        continue;

                    App.Logger.WriteLine(LOG_IDENT, $"Removing old version folder for {dir.Name}");

                    try
                    {
                        dir.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to delete version folder!");
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                }
            }

            // don't register program size until the program is registered, which will be done after this
            if (!App.IsFirstRun && !FreshInstall)
                RegisterProgramSize();

            if (Dialog is not null)
                Dialog.CancelEnabled = false;

            _isInstalling = false;
        }
        
        private async Task InstallWebView2()
        {
            const string LOG_IDENT = "Bootstrapper::InstallWebView2";
            
            // check if the webview2 runtime needs to be installed
            // webview2 can either be installed be per-user or globally, so we need to check in both hklm and hkcu
            // https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution#detect-if-a-suitable-webview2-runtime-is-already-installed

            using RegistryKey? hklmKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            using RegistryKey? hkcuKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");

            if (hklmKey is not null || hkcuKey is not null)
                return;

            App.Logger.WriteLine(LOG_IDENT, "Installing runtime...");

            string baseDirectory = Path.Combine(_versionFolder, "WebView2RuntimeInstaller");

            if (!Directory.Exists(baseDirectory))
            {
                Package? package = _versionPackageManifest.Find(x => x.Name == "WebView2RuntimeInstaller.zip");

                if (package is null)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Aborted runtime install because package does not exist, has WebView2 been added in this Roblox version yet?");
                    return;
                }

                await ExtractPackage(package);
            }

            SetStatus(Resources.Strings.Bootstrapper_Status_InstallingWebView2);

            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = baseDirectory,
                FileName = Path.Combine(baseDirectory, "MicrosoftEdgeWebview2Setup.exe"),
                Arguments = "/silent /install"
            };

            await Process.Start(startInfo)!.WaitForExitAsync();

            App.Logger.WriteLine(LOG_IDENT, "Finished installing runtime");
        }

        public static void MigrateIntegrations()
        {
            // v2.2.0 - remove rbxfpsunlocker
            string rbxfpsunlocker = Path.Combine(Paths.Integrations, "rbxfpsunlocker");

            if (Directory.Exists(rbxfpsunlocker))
                Directory.Delete(rbxfpsunlocker, true);

            // v2.3.0 - remove reshade
            string injectorLocation = Path.Combine(Paths.Modifications, "dxgi.dll");
            string configLocation = Path.Combine(Paths.Modifications, "ReShade.ini");

            if (File.Exists(injectorLocation))
            {
                Frontend.ShowMessageBox(
                    Resources.Strings.Bootstrapper_HyperionUpdateInfo,
                    MessageBoxImage.Warning
                );

                File.Delete(injectorLocation);
            }

            if (File.Exists(configLocation))
                File.Delete(configLocation);
        }

        private async Task ApplyModifications()
        {
            const string LOG_IDENT = "Bootstrapper::ApplyModifications";
            
            if (Process.GetProcessesByName(_playerFileName[..^4]).Any())
            {
                App.Logger.WriteLine(LOG_IDENT, "Roblox is running, aborting mod check");
                return;
            }

            SetStatus(Resources.Strings.Bootstrapper_Status_ApplyingModifications);

            // set executable flags for fullscreen optimizations
            App.Logger.WriteLine(LOG_IDENT, "Checking executable flags...");
            using (RegistryKey appFlagsKey = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers"))
            {
                string flag = " DISABLEDXMAXIMIZEDWINDOWEDMODE";
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
                    App.Logger.WriteLine(LOG_IDENT, $"Deleting flag '{flag.Trim()}'");

                    // if there's more than one space, there's more flags set we need to preserve
                    if (appFlags.Split(' ').Length > 2)
                        appFlagsKey.SetValue(_playerLocation, appFlags.Remove(appFlags.IndexOf(flag), flag.Length));
                    else
                        appFlagsKey.DeleteValue(_playerLocation);
                }

                // hmm, maybe make a unified handler for this? this is just lazily copy pasted from above

                flag = " RUNASADMIN";
                appFlags = (string?)appFlagsKey.GetValue(_playerLocation);

                if (appFlags is not null && appFlags.Contains(flag))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Deleting flag '{flag.Trim()}'");
                    
                    // if there's more than one space, there's more flags set we need to preserve
                    if (appFlags.Split(' ').Length > 2)
                        appFlagsKey.SetValue(_playerLocation, appFlags.Remove(appFlags.IndexOf(flag), flag.Length));
                    else
                        appFlagsKey.DeleteValue(_playerLocation);
                }
            }

            // handle file mods
            App.Logger.WriteLine(LOG_IDENT, "Checking file mods...");

            // manifest has been moved to State.json
            File.Delete(Path.Combine(Paths.Base, "ModManifest.txt"));

            List<string> modFolderFiles = new();

            if (!Directory.Exists(Paths.Modifications))
                Directory.CreateDirectory(Paths.Modifications);

            // cursors
            await CheckModPreset(App.Settings.Prop.CursorType == CursorType.From2006, new Dictionary<string, string>
            {
                { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2006.ArrowCursor.png" },
                { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2006.ArrowFarCursor.png" }
            });

            await CheckModPreset(App.Settings.Prop.CursorType == CursorType.From2013, new Dictionary<string, string>
            {
                { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2013.ArrowCursor.png" },
                { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2013.ArrowFarCursor.png" }
            });

            // character sounds
            await CheckModPreset(App.Settings.Prop.UseOldDeathSound, @"content\sounds\ouch.ogg", "Sounds.OldDeath.ogg");

            await CheckModPreset(App.Settings.Prop.UseOldCharacterSounds, new Dictionary<string, string>
            {
                { @"content\sounds\action_footsteps_plastic.mp3", "Sounds.OldWalk.mp3" },
                { @"content\sounds\action_jump.mp3",              "Sounds.OldJump.mp3" },
                { @"content\sounds\action_get_up.mp3",            "Sounds.OldGetUp.mp3" },
                { @"content\sounds\action_falling.mp3",           "Sounds.Empty.mp3" },
                { @"content\sounds\action_jump_land.mp3",         "Sounds.Empty.mp3" },
                { @"content\sounds\action_swim.mp3",              "Sounds.Empty.mp3" },
                { @"content\sounds\impact_water.mp3",             "Sounds.Empty.mp3" }
            });

            // Mobile.rbxl
            await CheckModPreset(App.Settings.Prop.UseOldAvatarBackground, @"ExtraContent\places\Mobile.rbxl", "OldAvatarBackground.rbxl");

            // emoji presets are downloaded remotely from github due to how large they are
            string contentFonts = Path.Combine(Paths.Modifications, "content\\fonts");
            string emojiFontLocation = Path.Combine(contentFonts, "TwemojiMozilla.ttf");
            string emojiFontHash = File.Exists(emojiFontLocation) ? MD5Hash.FromFile(emojiFontLocation) : "";

            if (App.Settings.Prop.EmojiType == EmojiType.Default && EmojiTypeEx.Hashes.Values.Contains(emojiFontHash))
            {
                App.Logger.WriteLine(LOG_IDENT, "Reverting to default emoji font");

                File.Delete(emojiFontLocation);
            }
            else if (App.Settings.Prop.EmojiType != EmojiType.Default && emojiFontHash != App.Settings.Prop.EmojiType.GetHash())
            {
                App.Logger.WriteLine(LOG_IDENT, $"Configuring emoji font as {App.Settings.Prop.EmojiType}");
                
                if (emojiFontHash != "")
                    File.Delete(emojiFontLocation);

                Directory.CreateDirectory(contentFonts);

                try
                {
                    var response = await App.HttpClient.GetAsync(App.Settings.Prop.EmojiType.GetUrl());
                    response.EnsureSuccessStatusCode();
                    await using var fileStream = new FileStream(emojiFontLocation, FileMode.CreateNew);
                    await response.Content.CopyToAsync(fileStream);
                }
                catch (HttpRequestException ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to fetch emoji preset from Github");
                    App.Logger.WriteException(LOG_IDENT, ex);
                    Frontend.ShowMessageBox(string.Format(Strings.Bootstrapper_EmojiPresetFetchFailed, App.Settings.Prop.EmojiType), MessageBoxImage.Warning);
                    App.Settings.Prop.EmojiType = EmojiType.Default;
                }
            }

            // check custom font mod
            // instead of replacing the fonts themselves, we'll just alter the font family manifests

            string modFontFamiliesFolder = Path.Combine(Paths.Modifications, "content\\fonts\\families");

            if (App.IsFirstRun && App.CustomFontLocation is not null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Paths.CustomFont)!);
                File.Copy(App.CustomFontLocation, Paths.CustomFont, true);
            }

            if (File.Exists(Paths.CustomFont))
            {
                App.Logger.WriteLine(LOG_IDENT, "Begin font check");

                Directory.CreateDirectory(modFontFamiliesFolder);

                foreach (string jsonFilePath in Directory.GetFiles(Path.Combine(_versionFolder, "content\\fonts\\families")))
                {
                    string jsonFilename = Path.GetFileName(jsonFilePath);
                    string modFilepath = Path.Combine(modFontFamiliesFolder, jsonFilename);

                    if (File.Exists(modFilepath))
                        continue;

                    App.Logger.WriteLine(LOG_IDENT, $"Setting font for {jsonFilename}");

                    FontFamily? fontFamilyData = JsonSerializer.Deserialize<FontFamily>(File.ReadAllText(jsonFilePath));

                    if (fontFamilyData is null)
                        continue;

                    foreach (FontFace fontFace in fontFamilyData.Faces)
                        fontFace.AssetId = "rbxasset://fonts/CustomFont.ttf";

                    File.WriteAllText(modFilepath, JsonSerializer.Serialize(fontFamilyData, new JsonSerializerOptions { WriteIndented = true }));
                }

                App.Logger.WriteLine(LOG_IDENT, "End font check");
            }
            else if (Directory.Exists(modFontFamiliesFolder))
            {
                Directory.Delete(modFontFamiliesFolder, true);
            }

            foreach (string file in Directory.GetFiles(Paths.Modifications, "*.*", SearchOption.AllDirectories))
            {
                // get relative directory path
                string relativeFile = file.Substring(Paths.Modifications.Length + 1);

                // v1.7.0 - README has been moved to the preferences menu now
                if (relativeFile == "README.txt")
                {
                    File.Delete(file);
                    continue;
                }

                if (!App.Settings.Prop.UseFastFlagManager && String.Equals(relativeFile, "ClientSettings\\ClientAppSettings.json", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (relativeFile.EndsWith(".lock"))
                    continue;

                modFolderFiles.Add(relativeFile);

                string fileModFolder = Path.Combine(Paths.Modifications, relativeFile);
                string fileVersionFolder = Path.Combine(_versionFolder, relativeFile);

                if (File.Exists(fileVersionFolder) && MD5Hash.FromFile(fileModFolder) == MD5Hash.FromFile(fileVersionFolder))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"{relativeFile} already exists in the version folder, and is a match");
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(fileVersionFolder)!);

                Filesystem.AssertReadOnly(fileVersionFolder);
                File.Copy(fileModFolder, fileVersionFolder, true);
                Filesystem.AssertReadOnly(fileVersionFolder);

                App.Logger.WriteLine(LOG_IDENT, $"{relativeFile} has been copied to the version folder");
            }

            // the manifest is primarily here to keep track of what files have been
            // deleted from the modifications folder, so that we know when to restore the original files from the downloaded packages
            // now check for files that have been deleted from the mod folder according to the manifest
            foreach (string fileLocation in App.State.Prop.ModManifest)
            {
                if (modFolderFiles.Contains(fileLocation))
                    continue;

                var package = _packageDirectories.SingleOrDefault(x => x.Value != "" && fileLocation.StartsWith(x.Value));

                // package doesn't exist, likely mistakenly placed file
                if (String.IsNullOrEmpty(package.Key))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"{fileLocation} was removed as a mod but does not belong to a package");

                    string versionFileLocation = Path.Combine(_versionFolder, fileLocation);

                    if (File.Exists(versionFileLocation))
                        File.Delete(versionFileLocation);

                    continue;
                }

                // restore original file
                string fileName = fileLocation.Substring(package.Value.Length);
                await ExtractFileFromPackage(package.Key, fileName);

                App.Logger.WriteLine(LOG_IDENT, $"{fileLocation} was removed as a mod, restored from {package.Key}");
            }

            App.State.Prop.ModManifest = modFolderFiles;
            App.State.Save();

            App.Logger.WriteLine(LOG_IDENT, $"Finished checking file mods");
        }

        private static async Task CheckModPreset(bool condition, string location, string name)
        {
            string LOG_IDENT = $"Bootstrapper::CheckModPreset.{name}";

            string fullLocation = Path.Combine(Paths.Modifications, location);
            string fileHash = File.Exists(fullLocation) ? MD5Hash.FromFile(fullLocation) : "";

            if (!condition && fileHash == "")
                return;

            byte[] embeddedData = string.IsNullOrEmpty(name) ? Array.Empty<byte>() : await Resource.Get(name);
            string embeddedHash = MD5Hash.FromBytes(embeddedData);

            if (!condition)
            {
                if (fileHash == embeddedHash)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Deleting '{location}' as preset is disabled, and mod file matches preset");

                    Filesystem.AssertReadOnly(fullLocation);
                    File.Delete(fullLocation);
                }
                
                return;
            }

            if (fileHash != embeddedHash)
            {       
                App.Logger.WriteLine(LOG_IDENT, $"Writing '{location}' as preset is enabled, and mod file does not exist or does not match preset");

                Directory.CreateDirectory(Path.GetDirectoryName(fullLocation)!);

                if (File.Exists(fullLocation))
                {
                    Filesystem.AssertReadOnly(fullLocation);
                    File.Delete(fullLocation);
                }

                await File.WriteAllBytesAsync(fullLocation, embeddedData);
            }
        }

        private static async Task CheckModPreset(bool condition, Dictionary<string, string> mapping)
        {
            foreach (var pair in mapping)
                await CheckModPreset(condition, pair.Key, pair.Value);
        }

        private async Task DownloadPackage(Package package)
        {
            string LOG_IDENT = $"Bootstrapper::DownloadPackage.{package.Name}";
            
            if (_cancelFired)
                return;

            string packageUrl = RobloxDeployment.GetLocation($"/{_latestVersionGuid}-{package.Name}");
            string packageLocation = Path.Combine(Paths.Downloads, package.Signature);
            string robloxPackageLocation = Path.Combine(Paths.LocalAppData, "Roblox", "Downloads", package.Signature);

            if (File.Exists(packageLocation))
            {
                FileInfo file = new(packageLocation);

                string calculatedMD5 = MD5Hash.FromFile(packageLocation);

                if (calculatedMD5 != package.Signature)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Package is corrupted ({calculatedMD5} != {package.Signature})! Deleting and re-downloading...");
                    file.Delete();
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Package is already downloaded, skipping...");

                    _totalDownloadedBytes += package.PackedSize;
                    UpdateProgressBar();

                    return;
                }
            }
            else if (File.Exists(robloxPackageLocation))
            {
                // let's cheat! if the stock bootstrapper already previously downloaded the file,
                // then we can just copy the one from there

                App.Logger.WriteLine(LOG_IDENT, $"Found existing copy at '{robloxPackageLocation}'! Copying to Downloads folder...");
                File.Copy(robloxPackageLocation, packageLocation);

                _totalDownloadedBytes += package.PackedSize;
                UpdateProgressBar();

                return;
            }

            if (File.Exists(packageLocation))
                return;

            const int maxTries = 5;

            App.Logger.WriteLine(LOG_IDENT, "Downloading...");

            var buffer = new byte[4096];

            for (int i = 1; i <= maxTries; i++)
            {
                if (_cancelFired)
                    return;

                int totalBytesRead = 0;

                try
                {
                    var response = await App.HttpClient.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead, _cancelTokenSource.Token);
                    await using var stream = await response.Content.ReadAsStreamAsync(_cancelTokenSource.Token);
                    await using var fileStream = new FileStream(packageLocation, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Delete);

                    while (true)
                    {
                        if (_cancelFired)
                        {
                            stream.Close();
                            fileStream.Close();
                            return;
                        }

                        int bytesRead = await stream.ReadAsync(buffer, _cancelTokenSource.Token);

                        if (bytesRead == 0)
                            break;

                        totalBytesRead += bytesRead;

                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), _cancelTokenSource.Token);

                        _totalDownloadedBytes += bytesRead;
                        UpdateProgressBar();
                    }

                    string hash = MD5Hash.FromStream(fileStream);

                    if (hash != package.Signature)
                        throw new ChecksumFailedException($"Failed to verify download of {packageUrl}\n\nGot signature: {hash}\n\nPackage has been downloaded to {packageLocation}\n\nPlease send the file shown above in a bug report.");

                    App.Logger.WriteLine(LOG_IDENT, $"Finished downloading! ({totalBytesRead} bytes total)");
                    break;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"An exception occurred after downloading {totalBytesRead} bytes. ({i}/{maxTries})");
                    App.Logger.WriteException(LOG_IDENT, ex);

                    if (i >= maxTries || ex.GetType() == typeof(ChecksumFailedException))
                        throw;

                    if (File.Exists(packageLocation))
                        File.Delete(packageLocation);

                    _totalDownloadedBytes -= totalBytesRead;
                    UpdateProgressBar();

                    // attempt download over HTTP
                    // this isn't actually that unsafe - signatures were fetched earlier over HTTPS
                    // so we've already established that our signatures are legit, and that there's very likely no MITM anyway
                    if (ex.GetType() == typeof(IOException) && !packageUrl.StartsWith("http://"))
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Retrying download over HTTP...");
                        packageUrl = packageUrl.Replace("https://", "http://");
                    }
                }
            }
        }

        private Task ExtractPackage(Package package)
        {
            const string LOG_IDENT = "Bootstrapper::ExtractPackage";

            if (_cancelFired)
                return Task.CompletedTask;

            string packageLocation = Path.Combine(Paths.Downloads, package.Signature);
            string packageFolder = Path.Combine(_versionFolder, _packageDirectories[package.Name]);

            App.Logger.WriteLine(LOG_IDENT, $"Extracting {package.Name}...");

            var fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
            fastZip.ExtractZip(packageLocation, packageFolder, null);

            App.Logger.WriteLine(LOG_IDENT, $"Finished extracting {package.Name}");

            _packagesExtracted += 1;

            return Task.CompletedTask;
        }

        private async Task ExtractFileFromPackage(string packageName, string fileName)
        {
            Package? package = _versionPackageManifest.Find(x => x.Name == packageName);

            if (package is null)
                return;

            await DownloadPackage(package);

            using ZipArchive archive = ZipFile.OpenRead(Path.Combine(Paths.Downloads, package.Signature));

            ZipArchiveEntry? entry = archive.Entries.FirstOrDefault(x => x.FullName == fileName);

            if (entry is null)
                return;

            string extractionPath = Path.Combine(_versionFolder, _packageDirectories[package.Name], entry.FullName);
            entry.ExtractToFile(extractionPath, true);
        }
#endregion
    }
}
