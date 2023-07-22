using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class LogTracerViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Window _window;
        private readonly RobloxActivity _activityWatcher;
        private int _lineNumber = 1;

        public ICommand CloseWindowCommand => new RelayCommand(_window.Close);
        public ICommand LocateLogFileCommand => new RelayCommand(LocateLogFile);

        public string LogFilename => Path.GetFileName(_activityWatcher.LogLocation);
        public string LogContents { get; private set; } = "";

        public LogTracerViewModel(Window window, RobloxActivity activityWatcher) 
        { 
            _window = window;
            _activityWatcher = activityWatcher;

            _activityWatcher.OnLogEntry += (_, message) =>
            {
                LogContents += $"{_lineNumber}: {message}\r\n";
                OnPropertyChanged(nameof(LogContents));

                _lineNumber += 1;
            };
        }

        private void LocateLogFile() => Process.Start("explorer.exe", $"/select,\"{_activityWatcher.LogLocation}\"");
    }
}
