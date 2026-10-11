using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;

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

        public bool AnalyticsEnabled
        {
            get => App.Settings.Prop.EnableAnalytics;
            set => App.Settings.Prop.EnableAnalytics = value;
        }

        public WebEnvironment WebEnvironment
        {
            get => App.Settings.Prop.WebEnvironment;
            set => App.Settings.Prop.WebEnvironment = value;
        }

        public Visibility WebEnvironmentVisibility => App.Settings.Prop.DeveloperMode ? Visibility.Visible : Visibility.Collapsed;

        public Dictionary<string, string> ImportSources { get; } = SettingsImporter.FindInstalls();

        public string? SelectedImportSource { get; set; } = SettingsImporter.FindInstalls().Keys.FirstOrDefault();

        public Visibility ImportVisibility => ImportSources.Any() ? Visibility.Visible : Visibility.Collapsed;

        public bool ShouldImportSettings { get; set; } = true;

        public bool ShouldImportFastFlags { get; set; } = true;

        public bool ShouldImportMods { get; set; } = true;

        public ICommand ImportCommand => new RelayCommand(Import);

        private void Import()
        {
            const string LOG_IDENT = "BloxstrapViewModel::Import";

            if (SelectedImportSource is null || !ImportSources.TryGetValue(SelectedImportSource, out string? folder))
                return;

            var result = Frontend.ShowMessageBox(String.Format(Strings.Menu_Bloxstrap_Import_Confirm, SelectedImportSource), MessageBoxImage.Question, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                if (ShouldImportSettings)
                    SettingsImporter.ImportSettings(folder);

                if (ShouldImportFastFlags)
                    SettingsImporter.ImportFastFlags(folder);

                if (ShouldImportMods)
                    SettingsImporter.ImportMods(folder);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                return;
            }

            Frontend.ShowMessageBox(String.Format(Strings.Menu_Bloxstrap_Import_Done, SelectedImportSource), MessageBoxImage.Information);
        }

        public ICommand CreateBackupCommand => new RelayCommand(CreateBackup);

        public ICommand RestoreBackupCommand => new RelayCommand(RestoreBackup);

        private void CreateBackup()
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"{App.ProjectName}-backup-{DateTime.Now:yyyy-MM-dd}.zip",
                Filter = $"{Strings.Menu_Mods_ModPack_Files}|*.zip"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                Backups.CreateBackup(dialog.FileName);
                Frontend.ShowMessageBox(Strings.Menu_Bloxstrap_Backup_Created, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BloxstrapViewModel::CreateBackup", ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        }

        private void RestoreBackup()
        {
            var dialog = new OpenFileDialog { Filter = $"{Strings.Menu_Mods_ModPack_Files}|*.zip" };

            if (dialog.ShowDialog() != true)
                return;

            var result = Frontend.ShowMessageBox(Strings.Menu_Bloxstrap_Backup_RestoreConfirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                Backups.RestoreBackup(dialog.FileName);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BloxstrapViewModel::RestoreBackup", ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                return;
            }

            Frontend.ShowMessageBox(Strings.Menu_Bloxstrap_Backup_Restored, MessageBoxImage.Information);
            App.Terminate();
        }

        public bool ShouldExportConfig { get; set; } = true;

        public bool ShouldExportLogs { get; set; } = true;

        public ICommand ExportDataCommand => new RelayCommand(ExportData);

        private void ExportData()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");

            var dialog = new SaveFileDialog 
            { 
                FileName = $"{App.ProjectName}-export-{timestamp}.zip",
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

        public bool UpdateToPreReleases
        {
            get => App.Settings.Prop.UpdateToPreReleases;
            set => App.Settings.Prop.UpdateToPreReleases = value;
        }

        public ICommand ResetSettingsCommand => new RelayCommand(() =>
        {
            var result = Frontend.ShowMessageBox(Strings.Menu_Bloxstrap_Reset_Confirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            App.Settings.Prop = new();
            App.Settings.Save();

            Frontend.ShowMessageBox(Strings.Menu_Bloxstrap_Reset_Done, MessageBoxImage.Information);
        });
    }
}
