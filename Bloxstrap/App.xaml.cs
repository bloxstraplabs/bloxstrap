using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Win32;

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

        public static bool ShouldSaveConfigs { get; set; } = false;
        public static bool IsSetupComplete { get; set; } = true;
        public static bool IsFirstRun { get; private set; } = true;
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

        public static readonly HttpClient HttpClient = new(new HttpClientLoggingHandler(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }));

        public static void Terminate(ErrorCode exitCode = ErrorCode.ERROR_SUCCESS)
        {
            if (IsFirstRun)
            {
                if (exitCode == ErrorCode.ERROR_CANCELLED)
                    exitCode = ErrorCode.ERROR_INSTALL_USEREXIT;
            }

            int exitCodeNum = (int)exitCode;

            Logger.WriteLine($"[App::Terminate] Terminating with exit code {exitCodeNum} ({exitCode})");

            Settings.Save();
            State.Save();
            NotifyIcon?.Dispose();

            Environment.Exit(exitCodeNum);
        }

        void GlobalExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            Logger.WriteLine("[App::OnStartup] An exception occurred when running the main thread");
            Logger.WriteLine($"[App::OnStartup] {e.Exception}");

            FinalizeExceptionHandling(e.Exception);
        }

        void FinalizeExceptionHandling(Exception exception)
        {
#if DEBUG
            throw exception;
#else
            if (!IsQuiet)
                Controls.ShowExceptionDialog(exception);

            Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
#endif
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.WriteLine($"[App::OnStartup] Starting {ProjectName} v{Version}");

            if (String.IsNullOrEmpty(BuildMetadata.CommitHash))
                Logger.WriteLine($"[App::OnStartup] Compiled {BuildMetadata.Timestamp.ToFriendlyString()} from {BuildMetadata.Machine}");
            else
                Logger.WriteLine($"[App::OnStartup] Compiled {BuildMetadata.Timestamp.ToFriendlyString()} from commit {BuildMetadata.CommitHash} ({BuildMetadata.CommitRef})");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            LaunchArgs = e.Args;

            HttpClient.Timeout = TimeSpan.FromMinutes(5);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", ProjectRepository);

            if (LaunchArgs.Length > 0)
            {
                if (Array.IndexOf(LaunchArgs, "-preferences") != -1 || Array.IndexOf(LaunchArgs, "-menu") != -1)
                {
                    Logger.WriteLine("[App::OnStartup] Started with IsMenuLaunch flag");
                    IsMenuLaunch = true;
                }

                if (Array.IndexOf(LaunchArgs, "-quiet") != -1)
                {
                    Logger.WriteLine("[App::OnStartup] Started with IsQuiet flag");
                    IsQuiet = true;
                }

                if (Array.IndexOf(LaunchArgs, "-uninstall") != -1)
                {
                    Logger.WriteLine("[App::OnStartup] Started with IsUninstall flag");
                    IsUninstall = true;
                }

                if (Array.IndexOf(LaunchArgs, "-nolaunch") != -1)
                {
                    Logger.WriteLine("[App::OnStartup] Started with IsNoLaunch flag");
                    IsNoLaunch = true;
                }

                if (Array.IndexOf(LaunchArgs, "-upgrade") != -1)
                {
                    Logger.WriteLine("[App::OnStartup] Bloxstrap started with IsUpgrade flag");
                    IsUpgrade = true;
                }
            }

            // check if installed
            using (RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey($@"Software\{ProjectName}"))
            {
                string? installLocation = null;
                
                if (registryKey is not null)
                    installLocation = (string?)registryKey.GetValue("InstallLocation");

                if (registryKey is null || installLocation is null)
                {
                    Logger.WriteLine("[App::OnStartup] Running first-time install");

                    BaseDirectory = Path.Combine(Directories.LocalAppData, ProjectName);
                    Logger.Initialize(true);

                    if (!IsQuiet)
                    {
                        IsSetupComplete = false;
                        FastFlags.Load();
                        Controls.ShowMenu();
                    }
                }
                else
                {
                    IsFirstRun = false;
                    BaseDirectory = installLocation;
                }
            }

            // exit if we don't click the install button on installation
            if (!IsSetupComplete)
            {
                Logger.WriteLine("[App::OnStartup] Installation cancelled!");
                Terminate(ErrorCode.ERROR_CANCELLED);
            }

            Directories.Initialize(BaseDirectory);

            // we shouldn't save settings on the first run until the first installation is finished,
            // just in case the user decides to cancel the install
            if (!IsFirstRun)
            {
                Logger.Initialize(IsUninstall);

                if (!Logger.Initialized)
                {
                    Logger.WriteLine("[App::OnStartup] Possible duplicate launch detected, terminating.");
                    Terminate();
                }

                Settings.Load();
                State.Load();
                FastFlags.Load();
            }

            if (!IsMenuLaunch)
                NotifyIcon = new();

#if !DEBUG
            if (!IsUninstall && !IsFirstRun)
                Updater.CheckInstalledVersion();
#endif

            string commandLine = "";

            if (IsMenuLaunch)
            {
                Process? menuProcess = Process.GetProcesses().Where(x => x.MainWindowTitle == $"{ProjectName} Menu").FirstOrDefault();

                if (menuProcess is not null)
                {
                    IntPtr handle = menuProcess.MainWindowHandle;
                    Logger.WriteLine($"[App::OnStartup] Found an already existing menu window with handle {handle}");
                    NativeMethods.SetForegroundWindow(handle);
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
                Logger.WriteLine($"[App::OnStartup] Initializing bootstrapper");
                Bootstrapper bootstrapper = new(commandLine);
                IBootstrapperDialog? dialog = null;

                if (!IsQuiet)
                {
                    Logger.WriteLine($"[App::OnStartup] Initializing bootstrapper dialog");
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
                    Logger.WriteLine("[App::OnStartup] Creating singleton mutex");

                    try
                    {
                        Mutex.OpenExisting("ROBLOX_singletonMutex");
                        Logger.WriteLine("[App::OnStartup] Warning - singleton mutex already exists!");
                    }
                    catch
                    {
                        // create the singleton mutex before the game client does
                        singletonMutex = new Mutex(true, "ROBLOX_singletonMutex");
                    }
                }

                Task bootstrapperTask = Task.Run(() => bootstrapper.Run());

                bootstrapperTask.ContinueWith(t =>
                {
                    Logger.WriteLine("[App::OnStartup] Bootstrapper task has finished");

                    // notifyicon is blocking main thread, must be disposed here
                    NotifyIcon?.Dispose();

                    if (t.IsFaulted)
                        Logger.WriteLine("[App::OnStartup] An exception occurred when running the bootstrapper");

                    if (t.Exception is null)
                        return;

                    Logger.WriteLine($"[App::OnStartup] {t.Exception}");

                    Exception exception = t.Exception;

#if !DEBUG
                    if (t.Exception.GetType().ToString() == "System.AggregateException")
                    exception = t.Exception.InnerException!;
#endif

                    FinalizeExceptionHandling(exception);
                });

                // this ordering is very important as all wpf windows are shown as modal dialogs, mess it up and you'll end up blocking input to one of them
                dialog?.ShowBootstrapper();

                if (Settings.Prop.EnableActivityTracking)
                    NotifyIcon?.InitializeContextMenu();

                bootstrapperTask.Wait();

                if (singletonMutex is not null)
                {
                    Logger.WriteLine($"[App::OnStartup] We have singleton mutex ownership! Running in background until all Roblox processes are closed");

                    // we've got ownership of the roblox singleton mutex!
                    // if we stop running, everything will screw up once any more roblox instances launched
                    while (Process.GetProcessesByName("RobloxPlayerBeta").Any())
                        Thread.Sleep(5000);
                }
            }

            Logger.WriteLine($"[App::OnStartup] Successfully reached end of main thread. Terminating...");

            Terminate();
        }
    }
}
