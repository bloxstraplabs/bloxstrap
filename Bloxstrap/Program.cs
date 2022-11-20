using System.Diagnostics;
using System.Globalization;
using System.IO;

using Microsoft.Win32;

using Bloxstrap.Enums;
using Bloxstrap.Helpers;
using Bloxstrap.Models;
using Bloxstrap.Dialogs;
using System.Net.Http;
using System.Net;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System;

namespace Bloxstrap
{
    internal static class Program
    {
        public const StringComparison StringFormat = StringComparison.InvariantCulture;
        public static readonly CultureInfo CultureFormat = CultureInfo.InvariantCulture;

        public const string ProjectName = "Bloxstrap";
        public const string ProjectRepository = "pizzaboxer/bloxstrap";

        #region base64 stuff
        // TODO: using IPFS as a reliable method for static asset storage instead of base64?
        public const string Base64OldDeathSound = "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EEIIIYSMMeeggxBCCKWUUkopGWPOQQghhBBKKKWEEjrnoIMQQgmllFJKKaV0zjkIIYQQSimllFJK6SCEEEIIpZRSSimllNJBCCGEUEoppZRSSiklhBBCCKWUUkoppZRSSgghhBBKKaWUUkoppZQQQgillFJKKaWUUkopIYQQSimllFJKKaWUUkIIpZRSSimllFJKKaWEEEoppZRSSimllFJKCaGUUkoppZRSSimllBJKKaWUUkoppZRSSikllFJKKaWUUkoppZRSSiillFJKKaWUUkoppZRQSimllFJKKaWUUkopoZRSSimllFJKKaWUUgoAADpwAAAIMKLSQuw048ojcEQhwwRUaMhKACAcAABABDoIIYQQQggRcxBCCCGEEEKImIMQQgghhBBCCCGEEEIIpZRSSimllFJKKaWUUkoppZRSSimllFJKKaWUUkoppZRSSimllFJKKaWUUkoppZRSSimllFJKKaWUUkoppZRSSimllFJKKaWUUkoppZRSSimllFJKKaWUUkoppZRSSimllFJKKaWUUkoppRQAdZnhABg9YeMMK0lnhaPBhYasBADSAgAAYxhjjCnIpLMWY60NYxBCB52EFGqoJaaGMQghdFBKSi22WHMGoaRSSkktxliDzT2DUEoppaQWY605F+NBSCWl1GKrteccjO4glJJSSjHWmnPuvWjQSUmptVpz7j0HXzwIpaTWWow9Bx+MMKKUlmKssdYcfBFGGFFKSy3GmnvNvRhjhEopxlp7zrnnXIwRPqUWY6659x58LsL44mLMOffigw8+CGGMkDHm2HPwvRdjjA/CyFxzLsIY44swwvggbK25B1+MEUYYY3zvNfigezHCCCOMMcII3XPRRfhijDFGGF+EAQC5EQ4AiAtGElJnGVYaceMJGCKQQkNWAQAxAAAEMcYgpJBSSinFGGOMMcYYY4wxxhhjjDHGnGPOOeecAADABAcAgAAr2JVZWrVR3NRJXvRB4BM6YjMy5FIqZnIi6JEaarES7NAKbvACsNCQlQAAGQAA5KSUlFotGkLKQWk1iMgg5STFJCJjkILSgqeQMYhJyh1jCiEFqXbQMYUUoxpSCplSCmqqOYaOMagxJ+FSCaUGAABAEAAgICQAwABBwQwAMDhAGDkQ6AggcGgDAAxEyExgUAgNDjIB4AEiQioASExQlC50QQgRpIsgiwcu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";
        public const string Base64OldArrowCursor = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAQAAAAAYLlVAAAAAmJLR0QA/4ePzL8AAAAJcEhZcwAACxMAAAsTAQCanBgAAAAHdElNRQfdBwoWHS0d8XaOAAAAeUlEQVRo3u3WyQmAUBAEUUMx/yRdEFH/dhG6EKsieKeZniYzMzMzMzMzs5ctWzBgZgk7ACUcAJBwAjDCBYAIdwBCeAIAQgmIE2pAmNACRAltQJDQA8QIfUCIMAJECGNAgFAforI/nWL4GcHvGB4k8CSDRyk8y83s860lExWMmEMyvAAAAABJRU5ErkJggg==";
        public const string Base64OldArrowFarCursor = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAQAAAAAYLlVAAAAAmJLR0QA/4ePzL8AAAAJcEhZcwAACxMAAAsTAQCanBgAAAAHdElNRQfdBwoWHwRtdYxgAAAAfElEQVRo3u3WwQmAMBQEUetPtXYQERE10VyEHcSZNPBOPztNZmZmZmZmZmYvK7VUGDCzhBWAEjYASNgBGOEAQIQzACFcAQChBcQJPSBMuANECfeAIOEJECM8A0KEESBCGAMChP4Qte9Ppxj+jODvGB4k8CSDRyk8y83s8y1ZdnQ0Empj3AAAAABJRU5ErkJggg==";
        #endregion

        public static string BaseDirectory = null!;
        public static bool IsFirstRun { get; private set; } = false;
        public static bool IsQuiet { get; private set; } = false;
        public static bool IsUninstall { get; private set; } = false;
        public static bool IsNoLaunch { get; private set; } = false;

        public static string LocalAppData { get; private set; } = null!;
        public static string StartMenu { get; private set; } = null!;

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

            HttpClient.Timeout = TimeSpan.FromMinutes(5);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", ProjectRepository);

            LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            StartMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", ProjectName);

            if (args.Length > 0)
            {
                if (Array.IndexOf(args, "-quiet") != -1)
                    IsQuiet = true;

                if (Array.IndexOf(args, "-uninstall") != -1)
                    IsUninstall = true;

                if (Array.IndexOf(args, "-nolaunch") != -1)
                    IsNoLaunch = true;
            }

                // check if installed
            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey($@"Software\{ProjectName}");

            if (registryKey is null)
            {
                IsFirstRun = true;
                Settings = SettingsManager.Settings;

                if (IsQuiet)
                    BaseDirectory = Path.Combine(LocalAppData, ProjectName);
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
            Updater.Check().Wait();
#endif

            string commandLine = "";

#if false //DEBUG
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
                    commandLine = String.Join(" ", args);
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
