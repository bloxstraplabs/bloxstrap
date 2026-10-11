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

            string info = String.Format(
                Strings.Dialog_PlayerError_FailedLaunch,
                $"https://bloxstraplabs.com/wiki/help/roblox-crashes-or-does-not-launch/"
            );

            ShowMessageBox(info, MessageBoxImage.Error);
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

        public static void CycleCustomTheme()
        {
            var interval = App.Settings.Prop.CycleCustomThemesInterval switch
            {
                ThemeCycleInterval.Hourly => TimeSpan.FromHours(1),
                ThemeCycleInterval.Daily => TimeSpan.FromDays(1),
                ThemeCycleInterval.Weekly => TimeSpan.FromDays(7),
                _ => TimeSpan.Zero
            };

            if (DateTime.Now - App.State.Prop.LastThemeCycle < interval || !Directory.Exists(Paths.CustomThemes))
                return;

            var themes = Directory.GetDirectories(Paths.CustomThemes)
                .Where(x => File.Exists(Path.Combine(x, "Theme.xml")))
                .Select(Path.GetFileName)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (themes.Count < 2)
                return;

            int index = themes.FindIndex(x => String.Equals(x, App.Settings.Prop.SelectedCustomTheme, StringComparison.OrdinalIgnoreCase));

            App.Settings.Prop.SelectedCustomTheme = themes[(index + 1) % themes.Count];
            App.Settings.Save();

            App.State.Prop.LastThemeCycle = DateTime.Now;
            App.State.Save();
        }

        private static IBootstrapperDialog GetCustomBootstrapper()
        {
            const string LOG_IDENT = "Frontend::GetCustomBootstrapper";

            Directory.CreateDirectory(Paths.CustomThemes);

            try
            {
                if (App.Settings.Prop.SelectedCustomTheme == null)
                    throw new CustomThemeException("CustomTheme.Errors.NoThemeSelected");

                CustomDialog dialog = new CustomDialog();
                dialog.ApplyCustomTheme(App.Settings.Prop.SelectedCustomTheme);
                return dialog;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);

                if (!App.LaunchSettings.QuietFlag.Active)
                    ShowMessageBox(string.Format(Strings.CustomTheme_Errors_SetupFailed, ex.Message, App.ProjectName), MessageBoxImage.Error); // NOTE: this is the fallback theme name

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

        public static void ShowBalloonTip(string title, string message, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.None, int timeout = 5)
        {
            var notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = Properties.Resources.IconBloxstrap,
                Text = App.ProjectName,
                Visible = true
            };

            notifyIcon.ShowBalloonTip(timeout, title, message, icon);
        }
    }
}
