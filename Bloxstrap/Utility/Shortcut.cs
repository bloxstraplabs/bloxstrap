using System.Windows;

namespace Bloxstrap.Utility
{
    internal static class Shortcut
    {
        private static AssemblyLoadStatus _loadStatus = AssemblyLoadStatus.NotAttempted;

        public static void Create(string exePath, string exeArgs, string lnkPath)
        {
            const string LOG_IDENT = "Shortcut::Create";

            if (File.Exists(lnkPath))
                return;

            try
            {
                ShellLink.Shortcut.CreateShortcut(exePath, exeArgs, exePath, 0).WriteToFile(lnkPath);

                if (_loadStatus != AssemblyLoadStatus.Successful)
                    _loadStatus = AssemblyLoadStatus.Successful;
            }
            catch (FileNotFoundException ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to create a shortcut for {lnkPath}!");
                App.Logger.WriteException(LOG_IDENT, ex);

                if (_loadStatus == AssemblyLoadStatus.Failed)
                    return;

                _loadStatus = AssemblyLoadStatus.Failed;

                Controls.ShowMessageBox(
                    $"{App.ProjectName} was unable to create shortcuts for the Desktop and Start menu. They will be created the next time Roblox is launched.",
                    MessageBoxImage.Information
                );
            }
        }
    }
}
