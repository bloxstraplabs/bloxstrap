using Bloxstrap.UI.Elements.Bootstrapper;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bloxstrap.UI.ViewModels.Editor
{
    public class BootstrapperEditorWindowViewModel : NotifyPropertyChangedViewModel
    {
        private CustomDialog? _dialog = null;

        public ICommand PreviewCommand => new RelayCommand(Preview);
        public ICommand SaveCommand => new RelayCommand(Save);
        public ICommand OpenThemeFolderCommand => new RelayCommand(OpenThemeFolder);

        public string Directory { get; set; } = "";

        public string Name { get; set; } = "";
        public string Title { get; set; } = "Editing \"Custom Theme\"";
        public string Code { get; set; } = "";

        private void Preview()
        {
            const string LOG_IDENT = "BootstrapperEditorWindowViewModel::Preview";

            try
            {
                CustomDialog dialog = new CustomDialog();

                dialog.ApplyCustomTheme(Name, Code);

                if (_dialog != null)
                    _dialog.CloseBootstrapper();
                _dialog = dialog;

                dialog.Message = Strings.Bootstrapper_StylePreview_TextCancel;
                dialog.CancelEnabled = true;
                dialog.ShowBootstrapper();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to preview custom theme");
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox($"Failed to preview theme: {ex.Message}", MessageBoxImage.Error, MessageBoxButton.OK);
            }
        }

        private void Save()
        {
            const string LOG_IDENT = "BootstrapperEditorWindowViewModel::Save";

            string path = Path.Combine(Paths.CustomThemes, Name, "Theme.xml");

            try
            {
                File.WriteAllText(path, Code);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to save custom theme");
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox($"Failed to save theme: {ex.Message}", MessageBoxImage.Error, MessageBoxButton.OK);
            }
        }

        private void OpenThemeFolder()
        {
            Process.Start("explorer.exe", Directory);
        }
    }
}
