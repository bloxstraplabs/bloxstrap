using Microsoft.Win32;

namespace Bloxstrap.Extensions
{
    public static class RegistryKeyEx
    {
        public static void SetValueSafe(this RegistryKey registryKey, string? name, object value)
        {
            try
            {
                App.Logger.WriteLine("RegistryKeyEx::SetValueSafe", $"Writing '{value}' to {registryKey}\\{name}");
                registryKey.SetValue(name, value);
            }
            catch (UnauthorizedAccessException)
            {
                Frontend.ShowMessageBox(Strings.Dialog_RegistryWriteError, System.Windows.MessageBoxImage.Error);
                App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
            }
        }

        public static void DeleteValueSafe(this RegistryKey registryKey, string name)
        {
            try
            {
                App.Logger.WriteLine("RegistryKeyEx::DeleteValueSafe", $"Deleting {registryKey}\\{name}");
                registryKey.DeleteValue(name);
            }
            catch (UnauthorizedAccessException)
            {
                Frontend.ShowMessageBox(Strings.Dialog_RegistryWriteError, System.Windows.MessageBoxImage.Error);
                App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
            }
        }
    }
}
