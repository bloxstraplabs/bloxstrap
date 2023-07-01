using System;
using System.Drawing;
using System.Windows;

using Bloxstrap.Enums;
using Bloxstrap.UI.Menu.Views;
using Bloxstrap.UI.MessageBox;

namespace Bloxstrap.UI
{
    static class Controls
    {
        public static void ShowMenu() => new MainWindow().ShowDialog();

        public static MessageBoxResult ShowMessageBox(string message, MessageBoxImage icon = MessageBoxImage.None, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            switch (App.Settings.Prop.BootstrapperStyle)
            {
                case BootstrapperStyle.FluentDialog:
                case BootstrapperStyle.ByfronDialog:
                    return FluentMessageBox.Show(message, icon, buttons, defaultResult);

                default:
                    return NativeMessageBox.Show(message, icon, buttons, defaultResult);
            }
        }

        public static void ShowExceptionDialog(Exception exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new ExceptionDialog(exception).ShowDialog();
            });
        }
    }
}
