using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Bloxstrap.AppData;
using Bloxstrap.UI.Elements.Settings;
using Bloxstrap.UI.Elements.Settings.Pages;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public sealed class StudioViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Page _page;

        public bool IsStudioInstalled => App.IsStudioInstalled;
        public string StudioVersion => App.IsStudioInstalled ? new RobloxStudioData().DistributionState.VersionGuid : LumiFeatureStrings.Menu_Studio_NotInstalled;
        public string StudioStatus => App.IsStudioInstalled ? LumiFeatureStrings.Menu_Studio_Installed : LumiFeatureStrings.Menu_Studio_NotInstalled;
        public ICommand LaunchStudioCommand => new RelayCommand(LaunchStudio);
        public ICommand OpenStudioFolderCommand => new RelayCommand(OpenStudioFolder);
        public ICommand OpenFlagEditorCommand => new RelayCommand(OpenFlagEditor);

        public StudioViewModel(Page page) => _page = page;

        private void LaunchStudio()
        {
            if (!App.IsStudioInstalled)
            {
                Frontend.ShowMessageBox(LumiFeatureStrings.Menu_Studio_NotInstalled, MessageBoxImage.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Paths.Application,
                    Arguments = "-studio",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("StudioViewModel::LaunchStudio", ex);
                Frontend.ShowMessageBox(String.Format(LumiFeatureStrings.Menu_Studio_LaunchFailed, ex.Message), MessageBoxImage.Error);
            }
        }

        private void OpenStudioFolder()
        {
            if (!App.IsStudioInstalled)
                return;

            string path = new RobloxStudioData().Directory;
            if (!Directory.Exists(path))
            {
                Frontend.ShowMessageBox(LumiFeatureStrings.Menu_Studio_FolderMissing, MessageBoxImage.Information);
                return;
            }

            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
        }

        private void OpenFlagEditor()
        {
            if (Window.GetWindow(_page) is MainWindow mainWindow)
                mainWindow.Navigate(typeof(FastFlagEditorPage));
        }
    }
}
