using Microsoft.Win32;
using System.Diagnostics;
using Bloxstrap.Helpers;

namespace Bloxstrap
{
    internal static class Program
    {
        public const StringComparison StringFormat = StringComparison.InvariantCulture;

        // ideally for the application website, i would prefer something other than my own hosted website?
        // i don't really have many other options though - github doesn't make much sense for something like this

        public const string ProjectName = "Bloxstrap";
        public const string ProjectRepository = "pizzaboxer/bloxstrap";
        public const string BaseUrlApplication = "https://bloxstrap.pizzaboxer.xyz";
        public const string BaseUrlSetup = "https://s3.amazonaws.com/setup.roblox.com";
        
        public static string? BaseDirectory;
        public static string LocalAppData { get; private set; }
        public static string FilePath { get; private set; }
        public static bool IsFirstRun { get; private set; } = false;

        public static SettingsFormat Settings;
        public static SettingsManager SettingsManager = new();

        public static void Exit()
        {
            SettingsManager.Save();
            Environment.Exit(0);
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // ensure only one is running
            Process[] processes = Process.GetProcessesByName(ProjectName);
            if (processes.Length > 1)
                return;

            // Task.Run(() => Updater.CheckForUpdates()).Wait();
            // return;

            LocalAppData = Environment.GetEnvironmentVariable("localappdata");

            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey($@"Software\{ProjectName}");

            if (registryKey is null)
            {
                IsFirstRun = true;
                Settings = SettingsManager.Settings;
                Application.Run(new Dialogs.Preferences());
            }
            else
            {
                BaseDirectory = (string?)registryKey.GetValue("InstallLocation");
                registryKey.Close();
            }

            // selection dialog was closed
            // (this doesnt account for the registry value not existing but thats basically never gonna happen)
            if (BaseDirectory is null)
                return;

            SettingsManager.SaveLocation = Path.Combine(BaseDirectory, "Settings.json");
            FilePath = Path.Combine(BaseDirectory, $"{ProjectName}.exe");

            // we shouldn't save settings on the first run until the first installation is finished,
            // just in case the user decides to cancel the install
            if (!IsFirstRun)
            {
                Settings = SettingsManager.Settings;
                SettingsManager.ShouldSave = true;
            }

            string commandLine = "";

            if (args.Length > 0)
            {
                if (args[0] == "-preferences")
                {
                    Application.Run(new Dialogs.Preferences());
                }
                else if (args[0].StartsWith("roblox-player:"))
                {
                    commandLine = Protocol.Parse(args[0]);
                }
                else if (args[0].StartsWith("roblox:"))
                {
                    commandLine = $"--app --deeplink {args[0]}";
                }
                else
                {
                    commandLine = String.Join(" ", args);
                }
            }
            else
            {
                commandLine = "--app";
            }

            if (!String.IsNullOrEmpty(commandLine))
                new Bootstrapper(Settings.BootstrapperStyle, commandLine);

            SettingsManager.Save();
        }
    }
}
