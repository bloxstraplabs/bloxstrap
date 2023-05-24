using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Win32;

using Bloxstrap.Extensions;
using Bloxstrap.Models;
using Bloxstrap.Singletons;
using Bloxstrap.UI.BootstrapperDialogs;
using Bloxstrap.UI.Menu.Views;

namespace Bloxstrap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly CultureInfo CultureFormat = CultureInfo.InvariantCulture;

        public const string ProjectName = "Bloxstrap";
        public const string ProjectRepository = "pizzaboxer/bloxstrap";

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

        public static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

        // singletons
        public static readonly Logger Logger = new();
        public static readonly JsonManager<Settings> Settings = new();
        public static readonly JsonManager<State> State = new();
        public static readonly FastFlagManager FastFlags = new();
        public static readonly HttpClient HttpClient = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All });

        public static System.Windows.Forms.NotifyIcon Notification { get; private set; } = null!;

        // shorthand
        public static MessageBoxResult ShowMessageBox(string message, MessageBoxImage icon = MessageBoxImage.None, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            if (IsQuiet)
                return MessageBoxResult.None;

            return MessageBox.Show(message, ProjectName, buttons, icon);
        }

        public static void Terminate(int code = Bootstrapper.ERROR_SUCCESS)
        {
            Logger.WriteLine($"[App::Terminate] Terminating with exit code {code}");
            Settings.Save();
            State.Save();
            Notification.Dispose();
            Environment.Exit(code);
        }

        private void InitLog()
        {
            // if we're running for the first time or uninstalling, log to temp folder
            // else, log to bloxstrap folder

            bool isUsingTempDir = IsFirstRun || IsUninstall;
            string logdir = isUsingTempDir ? Path.Combine(Directories.LocalAppData, "Temp") : Path.Combine(Directories.Base, "Logs");
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
            int processId = Process.GetCurrentProcess().Id;

            Logger.Initialize(Path.Combine(logdir, $"{ProjectName}_{timestamp}_{processId}.log"));

            // clean up any logs older than a week
            if (!isUsingTempDir)
            {
                foreach (FileInfo log in new DirectoryInfo(logdir).GetFiles()) 
                {
                    if (log.LastWriteTimeUtc.AddDays(7) > DateTime.UtcNow)
                        continue;

                    Logger.WriteLine($"[App::InitLog] Cleaning up old log file '{log.Name}'");
                    log.Delete();
                }
            }
        }

        void GlobalExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            Logger.WriteLine("[App::OnStartup] An exception occurred when running the main thread");
            Logger.WriteLine($"[App::OnStartup] {e.Exception}");

            if (!IsQuiet)
                Settings.Prop.BootstrapperStyle.GetNew().ShowError($"{e.Exception.GetType()}: {e.Exception.Message}");

            Terminate(Bootstrapper.ERROR_INSTALL_FAILURE);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.WriteLine($"[App::OnStartup] Starting {ProjectName} v{Version}");

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

            // so this needs to be here because winforms moment
            // onclick events will not fire unless this is defined here in the main thread so uhhhhh
            // we'll show the icon if we're launching roblox since we're likely gonna be showing a
            // bunch of notifications, and always showing it just makes the most sense i guess since it
            // indicates that bloxstrap is running, even in the background
            Notification = new()
            {
                Icon = Bloxstrap.Properties.Resources.IconBloxstrap,
                Text = ProjectName,
                Visible = !IsMenuLaunch
            };

            // check if installed
            using (RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey($@"Software\{ProjectName}"))
            {
                if (registryKey is null)
                {
                    Logger.WriteLine("[App::OnStartup] Running first-time install");

                    BaseDirectory = Path.Combine(Directories.LocalAppData, ProjectName);
                    InitLog();

                    if (!IsQuiet)
                    {
                        IsSetupComplete = false;
                        FastFlags.Load();
                        new MainWindow().ShowDialog();
                    }
                }
                else
                {
                    IsFirstRun = false;
                    BaseDirectory = (string)registryKey.GetValue("InstallLocation")!;
                }
            }

            // exit if we don't click the install button on installation
            if (!IsSetupComplete)
            {
                Logger.WriteLine("[App::OnStartup] Installation cancelled!");
                Environment.Exit(Bootstrapper.ERROR_INSTALL_USEREXIT);
            }

            Directories.Initialize(BaseDirectory);

            // we shouldn't save settings on the first run until the first installation is finished,
            // just in case the user decides to cancel the install
            if (!IsFirstRun)
            {
                InitLog();
                Settings.Load();
                State.Load();
                FastFlags.Load();
            }

#if !DEBUG
            if (!IsUninstall && !IsFirstRun)
                Updater.CheckInstalledVersion();
#endif

            string commandLine = "";

            if (IsMenuLaunch)
            {
                Mutex mutex;

                try
                {
                    mutex = Mutex.OpenExisting("Bloxstrap_MenuMutex");
                    Logger.WriteLine("[App::OnStartup] Bloxstrap_MenuMutex mutex exists, aborting menu launch...");
                    Terminate();
                }
                catch
                {
                    // no mutex exists, continue to opening preferences menu
                    mutex = new(true, "Bloxstrap_MenuMutex");
                }

                if (Utilities.GetProcessCount(ProjectName) > 1)
                    ShowMessageBox($"{ProjectName} is currently running, likely as a background Roblox process. Please note that not all your changes will immediately apply until you close all currently open Roblox instances.", MessageBoxImage.Information);

                new MainWindow().ShowDialog();
            }
            else if (LaunchArgs.Length > 0)
            {
                if (LaunchArgs[0].StartsWith("roblox-player:"))
                {
                    commandLine = ProtocolHandler.ParseUri(LaunchArgs[0]);
                }
                else if (LaunchArgs[0].StartsWith("roblox:"))
                {
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

                // there's a bug here that i have yet to fix!
                // sometimes the task just terminates when the bootstrapper hasn't
                // actually finished, causing the bootstrapper to hang indefinitely
                // i have no idea how the fuck this happens, but it happens like VERY
                // rarely so i'm not too concerned by it
                // maybe one day ill find out why it happens
                Task bootstrapperTask = Task.Run(() => bootstrapper.Run()).ContinueWith(t =>
                {
                    Logger.WriteLine("[App::OnStartup] Bootstrapper task has finished");

                    if (t.IsFaulted)
                        Logger.WriteLine("[App::OnStartup] An exception occurred when running the bootstrapper");

                    if (t.Exception is null)
                        return;

                    Logger.WriteLine($"[App::OnStartup] {t.Exception}");

#if DEBUG
                    throw t.Exception;
#else
                    var exception = t.Exception.InnerExceptions.Count >= 1 ? t.Exception.InnerExceptions[0] : t.Exception;
                    dialog?.ShowError($"{exception.GetType()}: {exception.Message}");
                    Terminate(Bootstrapper.ERROR_INSTALL_FAILURE);
#endif
                });

                dialog?.ShowBootstrapper();
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
