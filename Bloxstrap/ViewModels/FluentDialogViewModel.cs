using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Bloxstrap.Dialogs;
using Bloxstrap.Enums;

namespace Bloxstrap.ViewModels
{
    class FluentDialogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private readonly IBootstrapperDialog _dialog;

        public ICommand CancelInstallCommand => new RelayCommand(CancelInstall);

        public string Icon { get; set; } = App.Settings.Prop.BootstrapperIcon.GetPackUri();
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
