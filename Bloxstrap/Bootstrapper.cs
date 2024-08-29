using System.Windows;
using System.Windows.Forms;

using Microsoft.Win32;

using Bloxstrap.AppData;

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

        private IAppData AppData;

        private string _playerLocation => Path.Combine(_versionFolder, AppData.ExecutableName);

        private string _launchCommandLine = App.LaunchSettings.RobloxLaunchArgs;
        private LaunchMode _launchMode = App.LaunchSettings.RobloxLaunchMode;
        private bool _installWebView2;

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

        public IBootstrapperDialog? Dialog = null;

        public bool IsStudioLaunch => _launchMode != LaunchMode.Player;
        #endregion

        #region Core
        public Bootstrapper(bool installWebView2)
        {
            _installWebView2 = installWebView2;

            if (_launchMode == LaunchMode.Player)
                AppData = new RobloxPlayerData();
            else
                AppData = new RobloxStudioData();
        }

        private void SetStatus(string message)
        {
            App.Logger.WriteLine("Bootstrapper::SetStatus", message);

            message = message.Replace("{product}", AppData.ProductName);

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

            // connectivity check

            App.Logger.WriteLine(LOG_IDENT, "Performing connectivity check...");

            SetStatus(Strings.Bootstrapper_Status_Connecting);

            var connectionResult = await RobloxDeployment.InitializeConnectivity();

            if (connectionResult is not null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Connectivity check failed!");
                App.Logger.WriteException(LOG_IDENT, connectionResult);

                string message = Strings.Bootstrapper_Connectivity_Preventing;

                if (connectionResult.GetType() == typeof(HttpResponseException))
                    message = Strings.Bootstrapper_Connectivity_RobloxDown;
                else if (connectionResult.GetType() == typeof(TaskCanceledException))
                    message = Strings.Bootstrapper_Connectivity_TimedOut;
                else if (connectionResult.GetType() == typeof(AggregateException))
                    connectionResult = connectionResult.InnerException!;

                Frontend.ShowConnectivityDialog(Strings.Dialog_Connectivity_UnableToConnect, message, connectionResult);

                App.Terminate(ErrorCode.ERROR_CANCELLED);

                return;
            }

            App.Logger.WriteLine(LOG_IDENT, "Connectivity check finished");

            await RobloxDeployment.GetInfo(RobloxDeployment.DefaultChannel);

#if !DEBUG
            if (App.Settings.Prop.CheckForUpdates)
                await CheckForUpdates();
#endif

            // ensure only one instance of the bootstrapper is running at the time
            // so that we don't have stuff like two updates happening simultaneously

            bool mutexExists = false;

            try
            {
                Mutex.OpenExisting("Bloxstrap_SingletonMutex").Close();
                App.Logger.WriteLine(LOG_IDENT, "Bloxstrap_SingletonMutex mutex exists, waiting...");
                SetStatus(Strings.Bootstrapper_Status_WaitingOtherInstances);
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
            if (_latestVersionGuid != _versionGuid || !File.Exists(_playerLocation))
                await InstallLatestVersion();

            MigrateIntegrations();

            if (_installWebView2)
                await InstallWebView2();

            App.FastFlags.Save();
            await ApplyModifications();

            // TODO: move this to install/upgrade flow
            if (FreshInstall)
                RegisterProgramSize();

            CheckInstall();

            // at this point we've finished updating our configs
            App.Settings.Save();
            App.State.Save();

            await mutex.ReleaseAsync();

            if (!App.LaunchSettings.NoLaunchFlag.Active && !_cancelFired)
                StartRoblox();

            Dialog?.CloseBootstrapper();
        }

        private async Task CheckLatestVersion()
        {
            const string LOG_IDENT = "Bootstrapper::CheckLatestVersion";

            // before we do anything, we need to query our channel
            // if it's set in the launch uri, we need to use it and set the registry key for it
            // else, check if the registry key for it exists, and use it

            string channel = "production";

            using var key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\ROBLOX Corporation\\Environments\\{AppData.RegistryName}\\Channel");

            var match = Regex.Match(App.LaunchSettings.RobloxLaunchArgs, "channel:([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (match.Groups.Count == 2)
            {
                channel = match.Groups[1].Value.ToLowerInvariant();
            }
            else if (key.GetValue("www.roblox.com") is string value)
            {
                channel = value;
            }

            ClientVersion clientVersion;

            try
            {
                clientVersion = await RobloxDeployment.GetInfo(channel, AppData.BinaryType);
            }
            catch (HttpResponseException ex)
            {
                if (ex.ResponseMessage.StatusCode 
                    is not HttpStatusCode.Unauthorized 
                    and not HttpStatusCode.Forbidden 
                    and not HttpStatusCode.NotFound)
                    throw;

                App.Logger.WriteLine(LOG_IDENT, $"Changing channel from {channel} to {RobloxDeployment.DefaultChannel} because HTTP {(int)ex.ResponseMessage.StatusCode}");

                channel = RobloxDeployment.DefaultChannel;
                clientVersion = await RobloxDeployment.GetInfo(channel, AppData.BinaryType);
            }

            if (clientVersion.IsBehindDefaultChannel)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Changing channel from {channel} to {RobloxDeployment.DefaultChannel} because channel is behind production");

                channel = RobloxDeployment.DefaultChannel;
                clientVersion = await RobloxDeployment.GetInfo(channel, AppData.BinaryType);
            }

            key.SetValue("www.roblox.com", channel);

            _latestVersionGuid = clientVersion.VersionGuid;
            _versionFolder = Path.Combine(Paths.Versions, _latestVersionGuid);
            _versionPackageManifest = await PackageManifest.Get(_latestVersionGuid);
        }

        private void StartRoblox()
        {
            const string LOG_IDENT = "Bootstrapper::StartRoblox";

            SetStatus(Strings.Bootstrapper_Status_Starting);

            if (App.Settings.Prop.ForceRobloxLanguage)
            {
                var match = Regex.Match(_launchCommandLine, "gameLocale:([a-z_]+)", RegexOptions.CultureInvariant);

                if (match.Groups.Count == 2)
                    _launchCommandLine = _launchCommandLine.Replace("robloxLocale:en_us", $"robloxLocale:{match.Groups[1].Value}", StringComparison.InvariantCultureIgnoreCase);
            }

            // needed for the start event to fire
            if (!String.IsNullOrEmpty(_launchCommandLine))
                _launchCommandLine += " ";

            _launchCommandLine += "-isInstallerLaunch";

            var startInfo = new ProcessStartInfo()
            {
                FileName = _playerLocation,
                Arguments = _launchCommandLine,
                WorkingDirectory = _versionFolder
            };

            if (_launchMode == LaunchMode.StudioAuth)
            {
                Process.Start(startInfo);
                return;
            }

            using var startEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "www.roblox.com/robloxStartedEvent");

            // v2.2.0 - byfron will trip if we keep a process handle open for over a minute, so we're doing this now
            int gameClientPid;
            using (var gameClient = Process.Start(startInfo)!)
            {
                gameClientPid = gameClient.Id;
            }

            App.Logger.WriteLine(LOG_IDENT, $"Started Roblox (PID {gameClientPid}), waiting for start event");

            if (!startEvent.WaitOne(TimeSpan.FromSeconds(10)))
            {
                Frontend.ShowPlayerErrorDialog();
                return;
            }

            App.Logger.WriteLine(LOG_IDENT, "Start event signalled");

            var autoclosePids = new List<int>();

            // launch custom integrations now
            foreach (var integration in App.Settings.Prop.CustomIntegrations)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Launching custom integration '{integration.Name}' ({integration.Location} {integration.LaunchArgs} - autoclose is {integration.AutoClose})");

                int pid = 0;
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = integration.Location,
                        Arguments = integration.LaunchArgs.Replace("\r\n", " "),
                        WorkingDirectory = Path.GetDirectoryName(integration.Location),
                        UseShellExecute = true
                    })!;

                    pid = process.Id;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to launch integration '{integration.Name}'!");
                    App.Logger.WriteLine(LOG_IDENT, $"{ex.Message}");
                }

                if (integration.AutoClose && pid != 0)
                    autoclosePids.Add(pid);
            }

            
            string args = gameClientPid.ToString();

            if (autoclosePids.Any())
                args += $";{String.Join(',', autoclosePids)}";

            using (var ipl = new InterProcessLock("Watcher"))
            {
                if (ipl.IsAcquired)
                    Process.Start(Paths.Process, $"-watcher \"{args}\"");
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
                if (Directory.Exists(_versionFolder))
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

            ProtocolHandler.Register("roblox", "Roblox", Paths.Application, "-player \"%1\"");
            ProtocolHandler.Register("roblox-player", "Roblox", Paths.Application, "-player \"%1\"");
#if STUDIO_FEATURES
            ProtocolHandler.Register("roblox-studio", "Roblox", Paths.Application);
            ProtocolHandler.Register("roblox-studio-auth", "Roblox", Paths.Application);

            ProtocolHandler.RegisterRobloxPlace(Paths.Application);
            ProtocolHandler.RegisterExtension(".rbxl");
            ProtocolHandler.RegisterExtension(".rbxlx");
#endif
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

            var versionComparison = Utilities.CompareVersions(App.Version, releaseInfo.TagName);

            // check if we aren't using a deployed build, so we can update to one if a new version comes out
            if (versionComparison == VersionComparison.Equal && App.IsProductionBuild || versionComparison == VersionComparison.GreaterThan)
            {
                App.Logger.WriteLine(LOG_IDENT, $"No updates found");
                return;
            }

            SetStatus(Strings.Bootstrapper_Status_UpgradingBloxstrap);
            
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
                
                Process.Start(startInfo);

                App.Terminate();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the auto-updater");
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(
                    string.Format(Strings.Bootstrapper_AutoUpdateFailed, releaseInfo.TagName),
                    MessageBoxImage.Information
                );
            }
        }
#endregion

        #region Roblox Install
        private async Task InstallLatestVersion()
        {
            const string LOG_IDENT = "Bootstrapper::InstallLatestVersion";
            
            _isInstalling = true;

            SetStatus(FreshInstall ? Strings.Bootstrapper_Status_Installing : Strings.Bootstrapper_Status_Upgrading);

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
                    Strings.Bootstrapper_NotEnoughSpace, 
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
                SetStatus(Strings.Bootstrapper_Status_Configuring);
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
                    string oldGameClientLocation = Path.Combine(oldVersionFolder, AppData.ExecutableName);
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
            if (!FreshInstall)
                RegisterProgramSize();

            if (Dialog is not null)
                Dialog.CancelEnabled = false;

            _isInstalling = false;
        }

        private async Task InstallWebView2()
        {
            const string LOG_IDENT = "Bootstrapper::InstallWebView2";

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

            SetStatus(Strings.Bootstrapper_Status_InstallingWebView2);

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
                    Strings.Bootstrapper_HyperionUpdateInfo,
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
            
            if (Process.GetProcessesByName(AppData.ExecutableName[..^4]).Any())
            {
                App.Logger.WriteLine(LOG_IDENT, "Roblox is running, aborting mod check");
                return;
            }

            SetStatus(Strings.Bootstrapper_Status_ApplyingModifications);

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

            // check custom font mod
            // instead of replacing the fonts themselves, we'll just alter the font family manifests

            string modFontFamiliesFolder = Path.Combine(Paths.Modifications, "content\\fonts\\families");

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

                var package = AppData.PackageDirectoryMap.SingleOrDefault(x => x.Value != "" && fileLocation.StartsWith(x.Value));

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
                        throw new ChecksumFailedException($"Failed to verify download of {packageUrl}\n\nExpected hash: {package.Signature}\nGot hash: {hash}");

                    App.Logger.WriteLine(LOG_IDENT, $"Finished downloading! ({totalBytesRead} bytes total)");
                    break;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"An exception occurred after downloading {totalBytesRead} bytes. ({i}/{maxTries})");
                    App.Logger.WriteException(LOG_IDENT, ex);

                    if (ex.GetType() == typeof(ChecksumFailedException))
                    {
                        Frontend.ShowConnectivityDialog(
                            Strings.Dialog_Connectivity_UnableToDownload,
                            String.Format(Strings.Dialog_Connectivity_UnableToDownloadReason, "[https://github.com/pizzaboxer/bloxstrap/wiki/Bloxstrap-is-unable-to-download-Roblox](https://github.com/pizzaboxer/bloxstrap/wiki/Bloxstrap-is-unable-to-download-Roblox)"),
                            ex
                        );

                        App.Terminate(ErrorCode.ERROR_CANCELLED);
                    }
                    else if (i >= maxTries)
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
            string packageFolder = Path.Combine(_versionFolder, AppData.PackageDirectoryMap[package.Name]);

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

            string extractionPath = Path.Combine(_versionFolder, AppData.PackageDirectoryMap[package.Name], entry.FullName);
            entry.ExtractToFile(extractionPath, true);
        }
#endregion
    }
}
