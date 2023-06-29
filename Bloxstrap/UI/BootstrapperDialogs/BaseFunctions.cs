using System;
using System.Windows;

namespace Bloxstrap.UI.BootstrapperDialogs
{
    static class BaseFunctions
    {
        public static void ShowSuccess(string message, Action? callback)
        {
            Controls.ShowMessageBox(message, MessageBoxImage.Information);

            if (callback is not null)
                callback();

            App.Terminate();
        }

        public static void ShowError(string message)
        {
            Controls.ShowMessageBox($"An error occurred while starting Roblox\n\nDetails: {message}", MessageBoxImage.Error);
            App.Terminate(Bootstrapper.ERROR_INSTALL_FAILURE);
        }
    }
}
