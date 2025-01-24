using System.Windows;

using Bloxstrap.UI.Elements.Bootstrapper;
using Bloxstrap.UI.Elements.Dialogs;

namespace Bloxstrap.UI
{
    static class Frontend
    {
        public static MessageBoxResult ShowMessageBox(string message, MessageBoxImage icon = MessageBoxImage.None, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            App.Logger.WriteLine("Frontend::ShowMessageBox", message);

            if (App.LaunchSettings.QuietFlag.Active)
                return defaultResult;

            return ShowFluentMessageBox(message, icon, buttons);
        }

        public static void ShowPlayerErrorDialog(bool crash = false)
        {
            if (App.LaunchSettings.QuietFlag.Active)
                return;

            string topLine = Strings.Dialog_PlayerError_FailedLaunch;

            if (crash)
                topLine = Strings.Dialog_PlayerError_Crash;

            string info = String.Format(
                Strings.Dialog_PlayerError_HelpInformation,
                $"https://github.com/{App.ProjectRepository}/wiki/Roblox-crashes-or-does-not-launch",
                $"https://github.com/{App.ProjectRepository}/wiki/Switching-between-Roblox-and-Bloxstrap"
            );

            ShowMessageBox($"{topLine}\n\n{info}", MessageBoxImage.Error);
        }

        public static void ShowExceptionDialog(Exception exception)
        {
            if (App.LaunchSettings.QuietFlag.Active)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                new ExceptionDialog(exception).ShowDialog();
            });
        }

        public static void ShowConnectivityDialog(string title, string description, MessageBoxImage image, Exception exception)
        {
            if (App.LaunchSettings.QuietFlag.Active)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                new ConnectivityDialog(title, description, image, exception).ShowDialog();
            });
        }

        private static IBootstrapperDialog GetCustomBootstrapper()
        {
            const string LOG_IDENT = "Frontend::GetCustomBootstrapper";

            Directory.CreateDirectory(Paths.CustomThemes);

            try
            {
                if (App.Settings.Prop.SelectedCustomTheme == null)
                    throw new Exception("No custom theme selected");

                CustomDialog dialog = new CustomDialog();
                dialog.ApplyCustomTheme(App.Settings.Prop.SelectedCustomTheme);
                return dialog;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                if (!App.LaunchSettings.QuietFlag.Active)
                    Frontend.ShowMessageBox($"Failed to setup custom bootstrapper: {ex.Message}.\nDefaulting to Fluent.", MessageBoxImage.Error);

                return GetBootstrapperDialog(BootstrapperStyle.FluentDialog);
            }
        }

        public static IBootstrapperDialog GetBootstrapperDialog(BootstrapperStyle style)
        {
            return style switch
            {
                BootstrapperStyle.VistaDialog => new VistaDialog(),
                BootstrapperStyle.LegacyDialog2008 => new LegacyDialog2008(),
                BootstrapperStyle.LegacyDialog2011 => new LegacyDialog2011(),
                BootstrapperStyle.ProgressDialog => new ProgressDialog(),
                BootstrapperStyle.ClassicFluentDialog => new ClassicFluentDialog(),
                BootstrapperStyle.ByfronDialog => new ByfronDialog(),
                BootstrapperStyle.FluentDialog => new FluentDialog(false),
                BootstrapperStyle.FluentAeroDialog => new FluentDialog(true),
                BootstrapperStyle.CustomDialog => GetCustomBootstrapper(),
                _ => new FluentDialog(false)
            };
        }

        private static MessageBoxResult ShowFluentMessageBox(string message, MessageBoxImage icon, MessageBoxButton buttons)
        {
            return Application.Current.Dispatcher.Invoke(new Func<MessageBoxResult>(() =>
            {
                var messagebox = new FluentMessageBox(message, icon, buttons);
                messagebox.ShowDialog();
                return messagebox.Result;
            }));
        }
    }
}
