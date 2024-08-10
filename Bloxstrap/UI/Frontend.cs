using System.Windows;

using Bloxstrap.UI.Elements.Bootstrapper;
using Bloxstrap.UI.Elements.Dialogs;
using Bloxstrap.UI.Elements.Settings;
using Bloxstrap.UI.Elements.Installer;
using System.Drawing;

namespace Bloxstrap.UI
{
    static class Frontend
    {
        public static MessageBoxResult ShowMessageBox(string message, MessageBoxImage icon = MessageBoxImage.None, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            App.Logger.WriteLine("Frontend::ShowMessageBox", message);

            if (App.LaunchSettings.IsQuiet)
                return defaultResult;

            if (!App.LaunchSettings.IsRobloxLaunch)
                return ShowFluentMessageBox(message, icon, buttons);

            switch (App.Settings.Prop.BootstrapperStyle)
            {
                case BootstrapperStyle.FluentDialog:
                case BootstrapperStyle.ClassicFluentDialog:
                case BootstrapperStyle.FluentAeroDialog:
                case BootstrapperStyle.ByfronDialog:
                    return ShowFluentMessageBox(message, icon, buttons);

                default:
                    return MessageBox.Show(message, App.ProjectName, buttons, icon);
            }
        }

        public static void ShowExceptionDialog(Exception exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new ExceptionDialog(exception).ShowDialog();
            });
        }

        public static void ShowConnectivityDialog(string title, string description, Exception exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new ConnectivityDialog(title, description, exception).ShowDialog();
            });
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
