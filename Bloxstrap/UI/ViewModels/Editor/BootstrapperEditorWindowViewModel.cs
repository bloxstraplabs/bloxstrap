﻿using Bloxstrap.UI.Elements.Bootstrapper;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace Bloxstrap.UI.ViewModels.Editor
{
    public class BootstrapperEditorWindowViewModel : NotifyPropertyChangedViewModel
    {
        private CustomDialog? _dialog = null;

        public ICommand PreviewCommand => new RelayCommand(Preview);
        public ICommand SaveCommand => new RelayCommand(Save);
        public ICommand OpenThemeFolderCommand => new RelayCommand(OpenThemeFolder);

        public Action<bool, string> ThemeSavedCallback { get; set; } = null!;

        public string Directory { get; set; } = "";

        public string Name { get; set; } = "";
        public string Title { get; set; } = "Editing \"Custom Theme\"";
        public string Code { get; set; } = "";

        public bool CodeChanged { get; set; } = false;

        private void Preview()
        {
            const string LOG_IDENT = "BootstrapperEditorWindowViewModel::Preview";

            try
            {
                CustomDialog dialog = new CustomDialog();

                dialog.ApplyCustomTheme(Name, Code);

                _dialog?.CloseBootstrapper();
                _dialog = dialog;

                dialog.Message = Strings.Bootstrapper_StylePreview_TextCancel;
                //dialog.TaskbarProgressState = TaskbarItemProgressState.Indeterminate;
                dialog.CancelEnabled = true;
                dialog.ShowBootstrapper();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to preview custom theme");
                App.Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(string.Format(Strings.CustomTheme_Editor_Errors_PreviewFailed, ex.Message), MessageBoxImage.Error, MessageBoxButton.OK);
            }
        }

        private void Save()
        {
            const string LOG_IDENT = "BootstrapperEditorWindowViewModel::Save";

            string path = Path.Combine(Directory, "Theme.xml");

            try
            {
                File.WriteAllText(path, Code);
                CodeChanged = false;
                ThemeSavedCallback.Invoke(true, Strings.CustomTheme_Editor_Save_Success_Description);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to save custom theme");
                App.Logger.WriteException(LOG_IDENT, ex);

                //Frontend.ShowMessageBox($"Failed to save theme: {ex.Message}", MessageBoxImage.Error, MessageBoxButton.OK);
                ThemeSavedCallback.Invoke(false, ex.Message);
            }
        }

        private void OpenThemeFolder()
        {
            Process.Start("explorer.exe", Directory);
        }
    }
}
