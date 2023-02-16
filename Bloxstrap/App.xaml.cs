using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

using Bloxstrap.Dialogs;
using Bloxstrap.Enums;
using Bloxstrap.Helpers;
using Bloxstrap.Models;
using Bloxstrap.Views;

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
        public static readonly DeployManager DeployManager = new();
        public static readonly JsonManager<Settings> Settings = new();
        public static readonly JsonManager<State> State = new();
        public static readonly HttpClient HttpClient = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All });

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
            Environment.Exit(code);
        }

        private void InitLog()
        {
            // if we're running for the first time or uninstalling, log to temp folder
            // else, log to bloxstrap folder

            string logdir = IsFirstRun || IsUninstall ? Path.Combine(Directories.LocalAppData, "Temp") : Path.Combine(Directories.Base, "Logs");
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
            Logger.Initialize(Path.Combine(logdir, $"{ProjectName}_{timestamp}.log"));
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
            }

#if !DEBUG
            if (!IsUninstall && !IsFirstRun)
                Updater.CheckInstalledVersion();
#endif

            string commandLine = "";

            if (IsMenuLaunch)
            {
#if !DEBUG
                    if (Process.GetProcessesByName(ProjectName).Length > 1)
                    {
                        ShowMessageBox($"{ProjectName} is currently running. Please close any currently open Bloxstrap or Roblox window before opening the menu.", MessageBoxImage.Error);
                        Environment.Exit(0);
                    }
#endif

                new MainWindow().ShowDialog();
            }
            else if (LaunchArgs.Length > 0)
            {
                if (LaunchArgs[0].StartsWith("roblox-player:"))
                {
                    commandLine = Protocol.ParseUri(LaunchArgs[0]);
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

                DeployManager.SetChannel(Settings.Prop.Channel);

                // start bootstrapper and show the bootstrapper modal if we're not running silently
                Bootstrapper bootstrapper = new Bootstrapper(commandLine);
                IBootstrapperDialog? dialog = null;

                if (!IsQuiet)
                {
                    dialog = Settings.Prop.BootstrapperStyle.GetNew();
                    bootstrapper.Dialog = dialog;
                    dialog.Bootstrapper = bootstrapper;
                }

                Task bootstrapperTask = Task.Run(() => bootstrapper.Run()).ContinueWith(t =>
                {
                    Logger.WriteLine("[App::OnStartup] Bootstrapper task has finished");

                    if (t.Exception is null)
                        return;

                    Logger.WriteLine("[App::OnStartup] An exception occurred when running the bootstrapper");
                    Logger.WriteLine($"[App::OnStartup] {t.Exception}");

#if DEBUG
                    throw t.Exception;
#else
                    var exception = t.Exception.InnerExceptions.Count >= 1 ? t.Exception.InnerExceptions[0] : t.Exception;
                    dialog?.ShowError($"{exception.GetType()}: {exception.Message}");
#endif
                });

                dialog?.ShowBootstrapper();
                bootstrapperTask.Wait();
            }

            Terminate();
        }
    }
}
