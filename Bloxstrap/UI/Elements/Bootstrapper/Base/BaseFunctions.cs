using System.Windows;

namespace Bloxstrap.UI.Elements.Bootstrapper.Base
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
    }
}
