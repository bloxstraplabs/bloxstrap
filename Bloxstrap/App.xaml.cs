using System.Reflection;
using System.Web;
using System.Windows;
using System.Windows.Threading;

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Bloxstrap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string ProjectName = "Bloxstrap";
        public const string ProjectRepository = "pizzaboxer/bloxstrap";
        public const string RobloxPlayerAppName = "RobloxPlayerBeta";
        public const string RobloxStudioAppName = "RobloxStudioBeta";

        // used only for communicating between app and menu - use Directories.Base for anything else
        public static string BaseDirectory = null!;
        public static string? CustomFontLocation;

        public static bool ShouldSaveConfigs { get; set; } = false;

        public static bool IsSetupComplete { get; set; } = true;
        public static bool IsFirstRun { get; set; } = true;

        public static LaunchSettings LaunchSettings { get; private set; } = null!;

        public static BuildMetadataAttribute BuildMetadata = Assembly.GetExecutingAssembly().GetCustomAttribute<BuildMetadataAttribute>()!;
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

        public static NotifyIconWrapper? NotifyIcon { get; private set; }

        public static readonly Logger Logger = new();

        public static readonly JsonManager<Settings> Settings = new();
        public static readonly JsonManager<State> State = new();
        public static readonly FastFlagManager FastFlags = new();

        public static readonly HttpClient HttpClient = new(
            new HttpClientLoggingHandler(
                new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }
            )
        );

#if RELEASE
        private static bool _showingExceptionDialog = false;
#endif

        public static void Terminate(ErrorCode exitCode = ErrorCode.ERROR_SUCCESS)
        {
            if (IsFirstRun)
            {
                if (exitCode == ErrorCode.ERROR_CANCELLED)
                    exitCode = ErrorCode.ERROR_INSTALL_USEREXIT;
            }

            int exitCodeNum = (int)exitCode;

            Logger.WriteLine("App::Terminate", $"Terminating with exit code {exitCodeNum} ({exitCode})");

            Settings.Save();
            State.Save();
            NotifyIcon?.Dispose();

            Environment.Exit(exitCodeNum);
        }

        void GlobalExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            Logger.WriteLine("App::GlobalExceptionHandler", "An exception occurred");

            FinalizeExceptionHandling(e.Exception);
        }

        public static void FinalizeExceptionHandling(Exception exception, bool log = true)
        {
            if (log)
                Logger.WriteException("App::FinalizeExceptionHandling", exception);

#if DEBUG
            throw exception;
#else
            if (_showingExceptionDialog)
                return;

            _showingExceptionDialog = true;

            if (!LaunchSettings.IsQuiet)
                Frontend.ShowExceptionDialog(exception);

            Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
#endif
        }

        private void StartupFinished()
        {
            const string LOG_IDENT = "App::StartupFinished";

            Logger.WriteLine(LOG_IDENT, "Successfully reached end of main thread. Terminating...");

            Terminate();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            const string LOG_IDENT = "App::OnStartup";

            base.OnStartup(e);

            Logger.WriteLine(LOG_IDENT, $"Starting {ProjectName} v{Version}");

            if (String.IsNullOrEmpty(BuildMetadata.CommitHash))
                Logger.WriteLine(LOG_IDENT, $"Compiled {BuildMetadata.Timestamp.ToFriendlyString()} from {BuildMetadata.Machine}");
            else
                Logger.WriteLine(LOG_IDENT, $"Compiled {BuildMetadata.Timestamp.ToFriendlyString()} from commit {BuildMetadata.CommitHash} ({BuildMetadata.CommitRef})");

            Logger.WriteLine(LOG_IDENT, $"Loaded from {Paths.Process}");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            LaunchSettings = new LaunchSettings(e.Args);

            using (var checker = new InstallChecker())
            {
                checker.Check();
            }

            Paths.Initialize(BaseDirectory);

            // we shouldn't save settings on the first run until the first installation is finished,
            // just in case the user decides to cancel the install
            if (!IsFirstRun)
            {
                Settings.Load();
                State.Load();
                FastFlags.Load();
            }

            HttpClient.Timeout = TimeSpan.FromSeconds(30);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", ProjectRepository);

            // TEMPORARY FILL-IN FOR NEW FUNCTIONALITY
            // REMOVE WHEN LARGER REFACTORING IS DONE
            await RobloxDeployment.InitializeConnectivity();

            // disallow running as administrator except for uninstallation
            if (Utilities.IsAdministrator && !LaunchSettings.IsUninstall)
            {
                Frontend.ShowMessageBox(Bloxstrap.Resources.Strings.Bootstrapper_RanInAdminMode, MessageBoxImage.Error);
                Terminate(ErrorCode.ERROR_INVALID_FUNCTION);
                return;
            }

            if (LaunchSettings.IsUninstall && IsFirstRun)
            {
                Frontend.ShowMessageBox(Bloxstrap.Resources.Strings.Bootstrapper_FirstRunUninstall, MessageBoxImage.Error);
                Terminate(ErrorCode.ERROR_INVALID_FUNCTION);
                return;
            }

            // we shouldn't save settings on the first run until the first installation is finished,
            // just in case the user decides to cancel the install
            if (!IsFirstRun)
            {
                Logger.Initialize(LaunchSettings.IsUninstall);

                if (!Logger.Initialized)
                {
                    Logger.WriteLine(LOG_IDENT, "Possible duplicate launch detected, terminating.");
                    Terminate();
                }
            }

            if (!LaunchSettings.IsUninstall && !LaunchSettings.IsMenuLaunch)
                NotifyIcon = new();

#if !DEBUG
            if (!LaunchSettings.IsUninstall && !IsFirstRun)
                InstallChecker.CheckUpgrade();
#endif

            if (LaunchSettings.IsMenuLaunch)
            {
                Process? menuProcess = Utilities.GetProcessesSafe().Where(x => x.MainWindowTitle == $"{ProjectName} Menu").FirstOrDefault();

                if (menuProcess is not null)
                {
                    var handle = menuProcess.MainWindowHandle;
                    Logger.WriteLine(LOG_IDENT, $"Found an already existing menu window with handle {handle}");
                    PInvoke.SetForegroundWindow((HWND)handle);
                }
                else
                {
                    bool showAlreadyRunningWarning = Process.GetProcessesByName(ProjectName).Length > 1 && !LaunchSettings.IsQuiet;
                    Frontend.ShowMenu(showAlreadyRunningWarning);
                }

                StartupFinished();
                return;
            }

            if (!IsFirstRun)
                ShouldSaveConfigs = true;

            // start bootstrapper and show the bootstrapper modal if we're not running silently
            Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper");
            Bootstrapper bootstrapper = new(LaunchSettings.RobloxLaunchArgs, LaunchSettings.RobloxLaunchMode);
            IBootstrapperDialog? dialog = null;

            if (!LaunchSettings.IsQuiet)
            {
                Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper dialog");
                dialog = Settings.Prop.BootstrapperStyle.GetNew();
                bootstrapper.Dialog = dialog;
                dialog.Bootstrapper = bootstrapper;
            }

            // handle roblox singleton mutex for multi-instance launching
            // note we're handling it here in the main thread and NOT in the
            // bootstrapper as handling mutexes in async contexts suuuuuucks

            Mutex? singletonMutex = null;

            if (Settings.Prop.MultiInstanceLaunching && LaunchSettings.RobloxLaunchMode == LaunchMode.Player)
            {
                Logger.WriteLine(LOG_IDENT, "Creating singleton mutex");

                try
                {
                    Mutex.OpenExisting("ROBLOX_singletonMutex");
                    Logger.WriteLine(LOG_IDENT, "Warning - singleton mutex already exists!");
                }
                catch
                {
                    // create the singleton mutex before the game client does
                    singletonMutex = new Mutex(true, "ROBLOX_singletonMutex");
                }
            }

            Task bootstrapperTask = Task.Run(async () => await bootstrapper.Run()).ContinueWith(t =>
            {
                Logger.WriteLine(LOG_IDENT, "Bootstrapper task has finished");

                // notifyicon is blocking main thread, must be disposed here
                NotifyIcon?.Dispose();

                if (t.IsFaulted)
                    Logger.WriteLine(LOG_IDENT, "An exception occurred when running the bootstrapper");

                if (t.Exception is null)
                    return;

                Logger.WriteException(LOG_IDENT, t.Exception);

                Exception exception = t.Exception;

#if !DEBUG
                if (t.Exception.GetType().ToString() == "System.AggregateException")
                    exception = t.Exception.InnerException!;
#endif

                FinalizeExceptionHandling(exception, false);
            });

            // this ordering is very important as all wpf windows are shown as modal dialogs, mess it up and you'll end up blocking input to one of them
            dialog?.ShowBootstrapper();

            if (!LaunchSettings.IsNoLaunch && Settings.Prop.EnableActivityTracking)
                NotifyIcon?.InitializeContextMenu();

            Logger.WriteLine(LOG_IDENT, "Waiting for bootstrapper task to finish");

            bootstrapperTask.Wait();

            if (singletonMutex is not null)
            {
                Logger.WriteLine(LOG_IDENT, "We have singleton mutex ownership! Running in background until all Roblox processes are closed");

                // we've got ownership of the roblox singleton mutex!
                // if we stop running, everything will screw up once any more roblox instances launched
                while (Process.GetProcessesByName("RobloxPlayerBeta").Any())
                    Thread.Sleep(5000);
            }

            StartupFinished();
        }
    }
}
