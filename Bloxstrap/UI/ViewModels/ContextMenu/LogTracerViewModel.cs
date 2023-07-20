using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class LogTracerViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Window _window;
        private readonly RobloxActivity _activityWatcher;

        public ICommand CloseWindowCommand => new RelayCommand(_window.Close);

        public string LogLocation => _activityWatcher.LogFilename;
        public string LogContents { get; private set; } = "";

        public LogTracerViewModel(Window window, RobloxActivity activityWatcher) 
        { 
            _window = window;
            _activityWatcher = activityWatcher;

            _activityWatcher.OnLogEntry += (_, message) =>
            {
                LogContents += message += "\r\n";
                OnPropertyChanged(nameof(LogContents));
            };
        }
    }
}
