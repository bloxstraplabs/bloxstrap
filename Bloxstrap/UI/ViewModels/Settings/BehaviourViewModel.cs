using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        public bool ConfirmLaunches
        {
            get => App.Settings.Prop.ConfirmLaunches;
            set => App.Settings.Prop.ConfirmLaunches = value;
        }

        public bool BackgroundUpdates
        {
            get => App.Settings.Prop.BackgroundUpdatesEnabled;
            set => App.Settings.Prop.BackgroundUpdatesEnabled = value;
        }

        public bool IsRobloxInstallationMissing => !App.IsPlayerInstalled && !App.IsStudioInstalled;

        public bool ForceRobloxReinstallation
        {
            get => App.State.Prop.ForceReinstall || IsRobloxInstallationMissing;
            set => App.State.Prop.ForceReinstall = value;
        }

        public ICommand CleanRobloxCacheCommand => new RelayCommand(CleanRobloxCache);

        private void CleanRobloxCache()
        {
            const string LOG_IDENT = "BehaviourViewModel::CleanRobloxCache";

            var processes = new List<Process>();
            
            if (!string.IsNullOrEmpty(App.PlayerState.Prop.VersionGuid))
                processes.AddRange(Process.GetProcessesByName(App.RobloxPlayerAppName));

            if (App.IsStudioInstalled)
                processes.AddRange(Process.GetProcessesByName(App.RobloxStudioAppName));

            if (processes.Any())
            {
                Frontend.ShowMessageBox(Strings.Menu_Integrations_Custom_LaunchArgs_Placeholder, MessageBoxImage.Error);
                return;
            }

            IEnumerable<FileInfo> files = Enumerable.Empty<FileInfo>();
            IEnumerable<string> dirs = Enumerable.Empty<string>();

            // all the cache folders i know of
            string robloxTempFolder = Path.Combine(Path.GetTempPath(), "Roblox");
            string robloxStorageFolder = Path.Combine(Paths.LocalAppData, "Roblox", "rbx-storage");
            string dbFile = Path.Combine(Paths.LocalAppData, "Roblox", "rbx-storage.db"); // the other "rbx-storage" files are irrelevant

            if (Directory.Exists(robloxTempFolder))
                dirs = dirs.Concat(Directory.GetDirectories(robloxTempFolder));

            if (Directory.Exists(robloxStorageFolder))
                dirs = dirs.Concat(Directory.GetDirectories(robloxStorageFolder));

            if (File.Exists(dbFile))
                files = files.Concat(new[] { new FileInfo(dbFile) });

            if (!files.Any() && !dirs.Any())
            {
                Frontend.ShowMessageBox(Strings.Dialog_CacheCleaner_Cleaned, MessageBoxImage.Information);
                return;
            }

            try
            {
                foreach (FileInfo file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to delete file '{file.Name}'!");
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                }

                foreach (string dir in dirs)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to delete directory '{Path.GetFileName(dir)}'!");
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                }

                Frontend.ShowMessageBox(Strings.Dialog_CacheCleaner_Cleaned, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to clean the cache!");
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(Strings.Dialog_CacheCleaner_FailedToClean, MessageBoxImage.Error);
            }
        }
    }
}
