using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class ServerInformationViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Window _window;
        private readonly RobloxActivity _activityWatcher;

        public string InstanceId => _activityWatcher.ActivityJobId;
        public string ServerLocation { get; private set; } = "Loading, please wait...";

        public ICommand CopyInstanceIdCommand => new RelayCommand(CopyInstanceId);
        public ICommand CloseWindowCommand => new RelayCommand(_window.Close);

        public ServerInformationViewModel(Window window, RobloxActivity activityWatcher)
        {
            _window = window;
            _activityWatcher = activityWatcher;

            Task.Run(async () =>
            {
                ServerLocation = await _activityWatcher.GetServerLocation();
                OnPropertyChanged(nameof(ServerLocation));
            });
        }

        private void CopyInstanceId() => Clipboard.SetText(InstanceId);
    }
}
