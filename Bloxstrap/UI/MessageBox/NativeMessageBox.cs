using System.Windows;

namespace Bloxstrap.UI.MessageBox
{
    static class NativeMessageBox
    {
        public static MessageBoxResult Show(string message, MessageBoxImage icon = MessageBoxImage.None, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            if (App.IsQuiet)
                return defaultResult;

            return System.Windows.MessageBox.Show(message, App.ProjectName, buttons, icon);
        }
    }
}
