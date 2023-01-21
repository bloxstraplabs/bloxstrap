using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Reflection;

using Microsoft.Win32;

using Bloxstrap.Enums;
using Bloxstrap.Helpers;
using Bloxstrap.Models;
using Bloxstrap.Dialogs.Menu;


namespace Bloxstrap
{
    internal static class Program
    {
        public const StringComparison StringFormat = StringComparison.InvariantCulture;
        public static readonly CultureInfo CultureFormat = CultureInfo.InvariantCulture;

        public const string ProjectName = "Bloxstrap";
        public const string ProjectRepository = "pizzaboxer/bloxstrap";

        public static string BaseDirectory = null!;
        public static bool IsFirstRun { get; private set; } = false;
        public static bool IsQuiet { get; private set; } = false;
        public static bool IsUninstall { get; private set; } = false;
        public static bool IsNoLaunch { get; private set; } = false;
        public static bool IsUpgrade { get; private set; } = false;
        public static string[] LaunchArgs { get; private set; } = null!;


        public static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

        public static SettingsManager SettingsManager = new();
        public static SettingsFormat Settings = SettingsManager.Settings;
        public static readonly HttpClient HttpClient = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All });

        // shorthand
        public static DialogResult ShowMessageBox(string message, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            if (IsQuiet)
                return DialogResult.None;

            return MessageBox.Show(message, ProjectName, buttons, icon);
        }

        public static void Exit(int code = Bootstrapper.ERROR_SUCCESS)
        {
            SettingsManager.Save();
            Environment.Exit(code);
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

            LaunchArgs = args;

            HttpClient.Timeout = TimeSpan.FromMinutes(5);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", ProjectRepository);

            if (args.Length > 0)
            {
                if (Array.IndexOf(args, "-quiet") != -1)
                    IsQuiet = true;

                if (Array.IndexOf(args, "-uninstall") != -1)
                    IsUninstall = true;

                if (Array.IndexOf(args, "-nolaunch") != -1)
                    IsNoLaunch = true;

                if (Array.IndexOf(args, "-upgrade") != -1)
                    IsUpgrade = true;
            }

                // check if installed
            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey($@"Software\{ProjectName}");

            if (registryKey is null)
            {
                IsFirstRun = true;
                Settings = SettingsManager.Settings;

                if (IsQuiet)
                    BaseDirectory = Path.Combine(Directories.LocalAppData, ProjectName);
                else
                    new Preferences().ShowDialog();
            }
            else
            {
                BaseDirectory = (string)registryKey.GetValue("InstallLocation")!;
                registryKey.Close();
            }

            // preferences dialog was closed, and so base directory was never set
            // (this doesnt account for the registry value not existing but thats basically never gonna happen)
            if (String.IsNullOrEmpty(BaseDirectory))
                return;

            Directories.Initialize(BaseDirectory);

            SettingsManager.SaveLocation = Path.Combine(Directories.Base, "Settings.json");

            // we shouldn't save settings on the first run until the first installation is finished,
            // just in case the user decides to cancel the install
            if (!IsFirstRun)
            {
                Settings = SettingsManager.Settings;
                SettingsManager.ShouldSave = true;
            }

#if !DEBUG
            if (!IsUninstall && !IsFirstRun)
                Updater.CheckInstalledVersion();
#endif

            string commandLine = "";

#if false//DEBUG
            new Preferences().ShowDialog();
#else
            if (args.Length > 0)
            {
                if (args[0] == "-preferences")
                {
                    if (Process.GetProcessesByName(ProjectName).Length > 1)
                    {
                        ShowMessageBox($"{ProjectName} is already running. Please close any currently open Bloxstrap or Roblox window before opening the configuration menu.", MessageBoxIcon.Error);
                        return;
                    }

                    new Preferences().ShowDialog();
                }
                else if (args[0].StartsWith("roblox-player:"))
                {
                    commandLine = Protocol.ParseUri(args[0]);
                }
                else if (args[0].StartsWith("roblox:"))
                {
                    commandLine = $"--app --deeplink {args[0]}";
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
#endif

            if (!String.IsNullOrEmpty(commandLine))
            {
                DeployManager.Channel = Settings.Channel;
                Settings.BootstrapperStyle.Show(new Bootstrapper(commandLine));
            }

            SettingsManager.Save();
        }
    }
}
