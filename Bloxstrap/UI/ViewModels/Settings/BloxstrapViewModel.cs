using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System.Diagnostics;
using Bloxstrap.Utility;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class BloxstrapViewModel : NotifyPropertyChangedViewModel
    {
        public WebEnvironment[] WebEnvironments => Enum.GetValues<WebEnvironment>();

        public bool UpdateCheckingEnabled
        {
            get => App.Settings.Prop.CheckForUpdates;
            set => App.Settings.Prop.CheckForUpdates = value;
        }

        public Visibility UpdateCheckingVisibility => String.IsNullOrWhiteSpace(App.ProjectRepository) ? Visibility.Collapsed : Visibility.Visible;

        public bool AnalyticsEnabled
        {
            get => App.Settings.Prop.EnableAnalytics;
            set => App.Settings.Prop.EnableAnalytics = value;
        }

        public Visibility AnalyticsVisibility => String.IsNullOrWhiteSpace(App.ProjectRepository) ? Visibility.Collapsed : Visibility.Visible;

        public WebEnvironment WebEnvironment
        {
            get => App.Settings.Prop.WebEnvironment;
            set => App.Settings.Prop.WebEnvironment = value;
        }

        public Visibility WebEnvironmentVisibility => App.Settings.Prop.DeveloperMode ? Visibility.Visible : Visibility.Collapsed;

        public bool ShouldExportConfig { get; set; } = true;

        public bool ShouldExportLogs { get; set; } = true;

        public ICommand ExportDataCommand => new RelayCommand(ExportData);

        public ICommand CheckPlayerInstallationCommand => new RelayCommand(async () => await CheckPlayerInstallation());

        public ICommand RepairPlayerInstallationCommand => new RelayCommand(RepairPlayerInstallation);

        public ICommand ClearDownloadCacheCommand => new RelayCommand(ClearDownloadCache);

        private string _lumiFixStatus = Strings.Menu_LumiFix_StatusIdle;
        public string LumiFixStatus
        {
            get => _lumiFixStatus;
            private set
            {
                if (_lumiFixStatus == value)
                    return;

                _lumiFixStatus = value;
                OnPropertyChanged(nameof(LumiFixStatus));
            }
        }

        private bool _isChecking;
        public bool IsChecking
        {
            get => _isChecking;
            private set
            {
                if (_isChecking == value)
                    return;

                _isChecking = value;
                OnPropertyChanged(nameof(IsChecking));
                OnPropertyChanged(nameof(IsMaintenanceActionEnabled));
            }
        }

        public bool IsMaintenanceActionEnabled => !IsChecking;

        private async Task CheckPlayerInstallation()
        {
            if (IsChecking)
                return;

            IsChecking = true;
            LumiFixStatus = Strings.Menu_LumiFix_StatusChecking;

            try
            {
                var progress = new Progress<string>(message => LumiFixStatus = message);
                LumiFixResult result = await LumiFix.CheckPlayerInstallationAsync(progress);

                if (!result.IsInstalled)
                {
                    LumiFixStatus = Strings.Menu_LumiFix_StatusNotInstalled;
                    return;
                }

                if (result.ProblemCount == 0)
                {
                    LumiFixStatus = String.Format(Strings.Menu_LumiFix_StatusHealthy, result.FilesChecked);
                }
                else
                {
                    LumiFixStatus = String.Format(
                        Strings.Menu_LumiFix_StatusIssues,
                        result.MissingFiles,
                        result.ChangedFiles,
                        result.UnreadableFiles
                    );
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("BloxstrapViewModel::CheckPlayerInstallation", "LumiFix could not complete the installation check");
                App.Logger.WriteException("BloxstrapViewModel::CheckPlayerInstallation", ex);
                LumiFixStatus = String.Format(Strings.Menu_LumiFix_CheckFailed, ex.Message);
            }
            finally
            {
                IsChecking = false;
            }
        }

        private void RepairPlayerInstallation()
        {
            if (HasOtherLumiStrapInstance())
            {
                Frontend.ShowMessageBox(Strings.Menu_LumiFix_OtherInstanceRunning, MessageBoxImage.Information);
                return;
            }

            if (HasProcessByName(App.RobloxPlayerAppName))
            {
                Frontend.ShowMessageBox(Strings.Menu_LumiFix_RobloxRunning, MessageBoxImage.Information);
                return;
            }

            var result = Frontend.ShowMessageBox(
                Strings.Menu_LumiFix_RepairPrompt,
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo,
                MessageBoxResult.No
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                if (!File.Exists(Paths.Application))
                    throw new FileNotFoundException("LumiStrap could not find its installed executable.", Paths.Application);

                SaveCurrentSettings();

                Process.Start(new ProcessStartInfo
                {
                    FileName = Paths.Application,
                    Arguments = "-player -force -nolaunch",
                    UseShellExecute = true
                });

                App.Terminate();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BloxstrapViewModel::RepairPlayerInstallation", ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_LumiFix_RepairFailed, ex.Message), MessageBoxImage.Error);
            }
        }

        private void ClearDownloadCache()
        {
            if (HasOtherLumiStrapInstance())
            {
                Frontend.ShowMessageBox(Strings.Menu_LumiFix_OtherInstanceRunning, MessageBoxImage.Information);
                return;
            }

            string[] files;
            try
            {
                files = Directory.Exists(Paths.Downloads)
                    ? Directory.GetFiles(Paths.Downloads, "*", SearchOption.TopDirectoryOnly)
                        .Where(file => IsPackageCacheFile(Path.GetFileName(file)))
                        .ToArray()
                    : Array.Empty<string>();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                App.Logger.WriteException("BloxstrapViewModel::ClearDownloadCache", ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_LumiFix_CacheFailed, ex.Message), MessageBoxImage.Error);
                return;
            }

            if (files.Length == 0)
            {
                Frontend.ShowMessageBox(Strings.Menu_LumiFix_CacheEmpty, MessageBoxImage.Information);
                return;
            }

            long totalBytes = files.Sum(file =>
            {
                try { return new FileInfo(file).Length; }
                catch { return 0L; }
            });

            var result = Frontend.ShowMessageBox(
                String.Format(Strings.Menu_LumiFix_CachePrompt, files.Length, totalBytes / 1024d / 1024d),
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo,
                MessageBoxResult.No
            );

            if (result != MessageBoxResult.Yes)
                return;

            int deleted = 0;
            int failed = 0;

            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                    deleted++;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    failed++;
                    App.Logger.WriteLine("BloxstrapViewModel::ClearDownloadCache", $"Could not delete '{file}'");
                    App.Logger.WriteException("BloxstrapViewModel::ClearDownloadCache", ex);
                }
            }

            Frontend.ShowMessageBox(
                String.Format(Strings.Menu_LumiFix_CacheCleared, deleted, failed),
                failed == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning
            );
        }

        private static bool HasOtherLumiStrapInstance()
        {
            foreach (Process process in Process.GetProcessesByName(App.ProjectName))
            {
                try
                {
                    if (process.Id != Environment.ProcessId)
                        return true;
                }
                catch (InvalidOperationException)
                {
                    // The process exited while LumiFix was checking for another instance.
                }
                finally
                {
                    process.Dispose();
                }
            }

            return false;
        }

        private static bool HasProcessByName(string processName)
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                try
                {
                    return true;
                }
                finally
                {
                    process.Dispose();
                }
            }

            return false;
        }

        private static bool IsPackageCacheFile(string filename)
            => filename.Length == 32 && filename.All(Uri.IsHexDigit);

        private static void SaveCurrentSettings()
        {
            const string LOG_IDENT = "BloxstrapViewModel::SaveCurrentSettings";

            App.Settings.Save();
            App.State.Save();
            App.FastFlags.Save();

            foreach (var pair in App.PendingSettingTasks)
            {
                if (!pair.Value.Changed)
                    continue;

                App.Logger.WriteLine(LOG_IDENT, $"Executing pending setting task '{pair.Key}' before Roblox repair");
                pair.Value.Execute();
            }

            App.PendingSettingTasks.Clear();
        }

        private void ExportData()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");

            var dialog = new SaveFileDialog 
            { 
                FileName = $"LumiStrap-export-{timestamp}.zip",
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip" 
            };

            if (dialog.ShowDialog() != true)
                return;

            using var memStream = new MemoryStream();
            using var zipStream = new ZipOutputStream(memStream);

            if (ShouldExportConfig)
            {
                var files = new List<string>()
                {
                    App.Settings.FileLocation,
                    App.State.FileLocation,
                    App.FastFlags.FileLocation
                };

                AddFilesToZipStream(zipStream, files, "Config/");
            }

            if (ShouldExportLogs && Directory.Exists(Paths.Logs))
            {
                var files = Directory.GetFiles(Paths.Logs)
                    .Where(x => !x.Equals(App.Logger.FileLocation, StringComparison.OrdinalIgnoreCase));

                AddFilesToZipStream(zipStream, files, "Logs/");
            }

            zipStream.CloseEntry();
            zipStream.Finish();
            memStream.Position = 0;

            using var outputStream = File.OpenWrite(dialog.FileName);
            memStream.CopyTo(outputStream);

            Process.Start("explorer.exe", $"/select,\"{dialog.FileName}\"");
        }

        private void AddFilesToZipStream(ZipOutputStream zipStream, IEnumerable<string> files, string directory)
        {
            const string LOG_IDENT = "BloxstrapViewModel::AddFilesToZipStream";

            foreach (string file in files)
            {
                if (!File.Exists(file))
                    continue;

                try
                {
                    using FileStream fileStream = File.OpenRead(file);

                    var entry = new ZipEntry(directory + Path.GetFileName(file));
                    entry.DateTime = DateTime.Now;

                    zipStream.PutNextEntry(entry);

                    fileStream.CopyTo(zipStream);
                }
                catch (IOException ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to open '{file}'");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }
        }
    }
}
