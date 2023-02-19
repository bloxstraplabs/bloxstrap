using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Bloxstrap.Dialogs;
using Bloxstrap.Enums;
using Bloxstrap.Helpers.Extensions;

namespace Bloxstrap.ViewModels
{
    class FluentDialogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private readonly IBootstrapperDialog _dialog;

        public ICommand CancelInstallCommand => new RelayCommand(CancelInstall);

        public string Title => App.Settings.Prop.BootstrapperTitle;
        public ImageSource Icon { get; set; } = App.Settings.Prop.BootstrapperIcon.GetIcon().GetImageSource();
        public string Message { get; set; } = "Please wait...";
        public bool ProgressIndeterminate { get; set; } = true;
        public int ProgressValue { get; set; } = 0;

        public Visibility CancelButtonVisibility { get; set; } = Visibility.Collapsed;
        public string CancelButtonText { get; set; } = "Cancel";

        public FluentDialogViewModel(IBootstrapperDialog dialog)
        {
            _dialog = dialog;
        }

        private void CancelInstall()
        {
            _dialog.Bootstrapper?.CancelInstall();
            _dialog.CloseBootstrapper();
        }
    }
}
