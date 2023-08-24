using System.Reflection;
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
        public const string RobloxAppName = "RobloxPlayerBeta";

        // used only for communicating between app and menu - use Directories.Base for anything else
        public static string BaseDirectory = null!;
        public static string? CustomFontLocation;

        public static bool ShouldSaveConfigs { get; set; } = false;

        public static bool IsSetupComplete { get; set; } = true;
        public static bool IsFirstRun { get; set; } = true;

        public static bool IsQuiet { get; private set; } = false;
        public static bool IsUninstall { get; private set; } = false;
        public static bool IsNoLaunch { get; private set; } = false;
        public static bool IsUpgrade { get; private set; } = false;
        public static bool IsMenuLaunch { get; private set; } = false;
        public static string[] LaunchArgs { get; private set; } = null!;

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

        private static bool _showingExceptionDialog = false;

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

            if (!IsQuiet)
                Controls.ShowExceptionDialog(exception);

            Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
#endif
        }

        protected override void OnStartup(StartupEventArgs e)
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

            LaunchArgs = e.Args;

            HttpClient.Timeout = TimeSpan.FromSeconds(30);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", ProjectRepository);

            if (LaunchArgs.Length > 0)
            {
                if (Array.IndexOf(LaunchArgs, "-preferences") != -1 || Array.IndexOf(LaunchArgs, "-menu") != -1)
                {
                    Logger.WriteLine(LOG_IDENT, "Started with IsMenuLaunch flag");
                    IsMenuLaunch = true;
                }

                if (Array.IndexOf(LaunchArgs, "-quiet") != -1)
                {
                    Logger.WriteLine(LOG_IDENT, "Started with IsQuiet flag");
                    IsQuiet = true;
                }

                if (Array.IndexOf(LaunchArgs, "-uninstall") != -1)
                {
                    Logger.WriteLine(LOG_IDENT, "Started with IsUninstall flag");
                    IsUninstall = true;
                }

                if (Array.IndexOf(LaunchArgs, "-nolaunch") != -1)
                {
                    Logger.WriteLine(LOG_IDENT, "Started with IsNoLaunch flag");
                    IsNoLaunch = true;
                }

                if (Array.IndexOf(LaunchArgs, "-upgrade") != -1)
                {
                    Logger.WriteLine(LOG_IDENT, "Bloxstrap started with IsUpgrade flag");
                    IsUpgrade = true;
                }
            }
            
            using (var checker = new InstallChecker())
            {
                checker.Check();
            }

            Paths.Initialize(BaseDirectory);

            // we shouldn't save settings on the first run until the first installation is finished,
            // just in case the user decides to cancel the install
            if (!IsFirstRun)
            {
                Logger.Initialize(IsUninstall);

                if (!Logger.Initialized)
                {
                    Logger.WriteLine(LOG_IDENT, "Possible duplicate launch detected, terminating.");
                    Terminate();
                }

                Settings.Load();
                State.Load();
                FastFlags.Load();
            }

            if (!IsUninstall && !IsMenuLaunch)
                NotifyIcon = new();

#if !DEBUG
            if (!IsUninstall && !IsFirstRun)
                InstallChecker.CheckUpgrade();
#endif

            string commandLine = "";

            if (IsMenuLaunch)
            {
                Process? menuProcess = Process.GetProcesses().Where(x => x.MainWindowTitle == $"{ProjectName} Menu").FirstOrDefault();

                if (menuProcess is not null)
                {
                    var handle = menuProcess.MainWindowHandle;
                    Logger.WriteLine(LOG_IDENT, $"Found an already existing menu window with handle {handle}");
                    PInvoke.SetForegroundWindow((HWND)handle);
                }
                else
                {
                    if (Process.GetProcessesByName(ProjectName).Length > 1 && !IsQuiet)
                        Controls.ShowMessageBox(
                            $"{ProjectName} is currently running, likely as a background Roblox process. Please note that not all your changes will immediately apply until you close all currently open Roblox instances.", 
                            MessageBoxImage.Information
                        );

                    Controls.ShowMenu();
                }
            }
            else if (LaunchArgs.Length > 0)
            {
                if (LaunchArgs[0].StartsWith("roblox-player:"))
                {
                    commandLine = ProtocolHandler.ParseUri(LaunchArgs[0]);
                }
                else if (LaunchArgs[0].StartsWith("roblox:"))
                {
                    if (Settings.Prop.UseDisableAppPatch)
                        Controls.ShowMessageBox(
                            "Roblox was launched via a deeplink, however the desktop app is required for deeplink launching to work. Because you've opted to disable the desktop app, it will temporarily be re-enabled for this launch only.", 
                            MessageBoxImage.Information
                        );

                    commandLine = $"--app --deeplink {LaunchArgs[0]}";
                }
                else
                {
                    commandLine = "--app";
                }
            }
            else
            {
                commandLine = "--app";
            }

            if (!String.IsNullOrEmpty(commandLine))
            {
                if (!IsFirstRun)
                    ShouldSaveConfigs = true;
                
                // start bootstrapper and show the bootstrapper modal if we're not running silently
                Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper");
                Bootstrapper bootstrapper = new(commandLine);
                IBootstrapperDialog? dialog = null;

                if (!IsQuiet)
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

                if (Settings.Prop.MultiInstanceLaunching)
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

                Task bootstrapperTask = Task.Run(() => bootstrapper.Run()).ContinueWith(t =>
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

                if (!IsNoLaunch && Settings.Prop.EnableActivityTracking)
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
            }

            Logger.WriteLine(LOG_IDENT, "Successfully reached end of main thread. Terminating...");

            Terminate();
        }
    }
}
