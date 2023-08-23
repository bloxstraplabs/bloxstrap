using System.Windows;
using System.Windows.Input;
using Bloxstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class ServerInformationViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Window _window;
        private readonly ActivityWatcher _activityWatcher;

        public string InstanceId => _activityWatcher.ActivityJobId;
        public string ServerType => $"{_activityWatcher.ActivityServerType} server";
        public string ServerLocation { get; private set; } = "Loading, please wait...";
        public string UdmuxProxied => _activityWatcher.ActivityMachineUDMUX ? "Yes" : "No";

        public ICommand CopyInstanceIdCommand => new RelayCommand(CopyInstanceId);
        public ICommand CloseWindowCommand => new RelayCommand(_window.Close);

        public ServerInformationViewModel(Window window, ActivityWatcher activityWatcher)
        {
            _window = window;
            _activityWatcher = activityWatcher;

            Task.Run(async () =>
            {
                ServerLocation = await _activityWatcher.GetServerLocation();
                OnPropertyChanged(nameof(ServerLocation));
            });
        }

        private void CopyInstanceId() => Clipboard.SetDataObject(InstanceId);
    }
}
