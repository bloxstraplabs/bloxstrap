using System.Windows;

using Bloxstrap.UI.Elements.Dialogs;

using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Bloxstrap
{
    public static class LaunchHandler
    {
        public static void ProcessNextAction(NextAction action, bool isUnfinishedInstall = false)
        {
            switch (action)
            {
                case NextAction.LaunchSettings:
                    LaunchSettings();
                    break;

                case NextAction.LaunchRoblox:
                    LaunchRoblox();
                    break;

                default:
                    App.Terminate(isUnfinishedInstall ? ErrorCode.ERROR_INSTALL_USEREXIT : ErrorCode.ERROR_SUCCESS);
                    break;
            }
        }

        public static void ProcessLaunchArgs()
        {
            // this order is specific

            if (App.LaunchSettings.IsUninstall)
                LaunchUninstaller();
            else if (App.LaunchSettings.IsMenuLaunch)
                LaunchSettings();
            else if (App.LaunchSettings.IsRobloxLaunch)
                LaunchRoblox();
            else if (!App.LaunchSettings.IsQuiet)
                LaunchMenu();
        }

        public static void LaunchInstaller()
        {
            using var interlock = new InterProcessLock("Installer");

            if (!interlock.IsAcquired)
            {
                Frontend.ShowMessageBox(Strings.Dialog_AlreadyRunning_Installer, MessageBoxImage.Stop);
                return;
            }

            if (App.LaunchSettings.IsUninstall)
            {
                Frontend.ShowMessageBox(Strings.Bootstrapper_FirstRunUninstall, MessageBoxImage.Error);
                App.Terminate(ErrorCode.ERROR_INVALID_FUNCTION);
                return;
            }

            if (App.LaunchSettings.IsQuiet)
            {
                var installer = new Installer();

                if (!installer.CheckInstallLocation())
                    App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);

                installer.DoInstall();

                interlock.Dispose();

                ProcessLaunchArgs();
            }
            else
            {
                new LanguageSelectorDialog().ShowDialog();

                var installer = new UI.Elements.Installer.MainWindow();
                installer.ShowDialog();

                interlock.Dispose();

                ProcessNextAction(installer.CloseAction, !installer.Finished);
            }
            
        }

        public static void LaunchUninstaller()
        {
            using var interlock = new InterProcessLock("Uninstaller");

            if (!interlock.IsAcquired)
            {
                Frontend.ShowMessageBox(Strings.Dialog_AlreadyRunning_Uninstaller, MessageBoxImage.Stop);
                return;
            }

            bool confirmed = false;
            bool keepData = true;

            if (App.LaunchSettings.IsQuiet)
            {
                confirmed = true;
            }
            else
            {
                var dialog = new UninstallerDialog();
                dialog.ShowDialog();

                confirmed = dialog.Confirmed;
                keepData = dialog.KeepData;
            }

            if (!confirmed)
                return;

            Installer.DoUninstall(keepData);

            Frontend.ShowMessageBox(Strings.Bootstrapper_SuccessfullyUninstalled, MessageBoxImage.Information);
        }

        public static void LaunchSettings()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchSettings";

            using var interlock = new InterProcessLock("Settings");

            if (interlock.IsAcquired)
            {
                bool showAlreadyRunningWarning = Process.GetProcessesByName(App.ProjectName).Length > 1;
                new UI.Elements.Settings.MainWindow(showAlreadyRunningWarning).ShowDialog();
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, $"Found an already existing menu window");
             
                var process = Utilities.GetProcessesSafe().Where(x => x.MainWindowTitle == Strings.Menu_Title).FirstOrDefault();

                if (process is not null)
                    PInvoke.SetForegroundWindow((HWND)process.MainWindowHandle);
            }
        }

        public static void LaunchMenu()
        {
            var dialog = new LaunchMenuDialog();
            dialog.ShowDialog();

            ProcessNextAction(dialog.CloseAction);
        }

        public static void LaunchRoblox()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchRoblox";

            bool installWebView2 = false;

            if (!File.Exists(Path.Combine(Paths.System, "mfplat.dll")))
            {
                Frontend.ShowMessageBox(Strings.Bootstrapper_WMFNotFound, MessageBoxImage.Error);

                if (!App.LaunchSettings.IsQuiet)
                    Utilities.ShellExecute("https://support.microsoft.com/en-us/topic/media-feature-pack-list-for-windows-n-editions-c1c6fffa-d052-8338-7a79-a4bb980a700a");

                App.Terminate(ErrorCode.ERROR_FILE_NOT_FOUND);
            }

            {
                using var hklmKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
                using var hkcuKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");

                if (hklmKey is null && hkcuKey is null)
                    installWebView2 = Frontend.ShowMessageBox(Strings.Bootstrapper_WebView2NotFound, MessageBoxImage.Warning, MessageBoxButton.YesNo, MessageBoxResult.Yes) == MessageBoxResult.Yes;
            }

            if (App.Settings.Prop.ConfirmLaunches && Mutex.TryOpenExisting("ROBLOX_singletonMutex", out var _))
            {
                // this currently doesn't work very well since it relies on checking the existence of the singleton mutex
                // which often hangs around for a few seconds after the window closes
                // it would be better to have this rely on the activity tracker when we implement IPC in the planned refactoring

                var result = Frontend.ShowMessageBox(Strings.Bootstrapper_ConfirmLaunch, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                {
                    App.Terminate();
                    return;
                }
            }

            App.NotifyIcon = new();

            // start bootstrapper and show the bootstrapper modal if we're not running silently
            App.Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper");
            var bootstrapper = new Bootstrapper(App.LaunchSettings.RobloxLaunchArgs, App.LaunchSettings.RobloxLaunchMode, installWebView2);
            IBootstrapperDialog? dialog = null;

            if (!App.LaunchSettings.IsQuiet)
            {
                App.Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper dialog");
                dialog = App.Settings.Prop.BootstrapperStyle.GetNew();
                bootstrapper.Dialog = dialog;
                dialog.Bootstrapper = bootstrapper;
            }

            Task bootstrapperTask = Task.Run(async () => await bootstrapper.Run()).ContinueWith(t =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Bootstrapper task has finished");

                // notifyicon is blocking main thread, must be disposed here
                App.NotifyIcon?.Dispose();

                if (t.IsFaulted)
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the bootstrapper");

                if (t.Exception is null)
                    return;

                App.Logger.WriteException(LOG_IDENT, t.Exception);

                Exception exception = t.Exception;

#if !DEBUG
                if (t.Exception.GetType().ToString() == "System.AggregateException")
                    exception = t.Exception.InnerException!;
#endif

                App.FinalizeExceptionHandling(exception, false);
            });

            // this ordering is very important as all wpf windows are shown as modal dialogs, mess it up and you'll end up blocking input to one of them
            dialog?.ShowBootstrapper();

            if (!App.LaunchSettings.IsNoLaunch && App.Settings.Prop.EnableActivityTracking)
                App.NotifyIcon?.InitializeContextMenu();

            App.Logger.WriteLine(LOG_IDENT, "Waiting for bootstrapper task to finish");

            bootstrapperTask.Wait();
        }
    }
}
