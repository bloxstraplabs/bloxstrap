using System.Windows;
using System.Windows.Input;
using Bloxstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class ServerInformationViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public string InstanceId => _activityWatcher.ActivityJobId;

        public string ServerType => Strings.ResourceManager.GetStringSafe($"Enums.ServerType.{_activityWatcher.ActivityServerType}");

        public string ServerLocation { get; private set; } = Strings.ContextMenu_ServerInformation_Loading;

        public ICommand CopyInstanceIdCommand => new RelayCommand(CopyInstanceId);

        public ICommand CloseWindowCommand => new RelayCommand(RequestClose);

        public EventHandler? RequestCloseEvent;

        public ServerInformationViewModel(Watcher watcher)
        {
            _activityWatcher = watcher.ActivityWatcher!;

            Task.Run(async () =>
            {
                ServerLocation = await _activityWatcher.GetServerLocation();
                OnPropertyChanged(nameof(ServerLocation));
            });
        }

        private void CopyInstanceId() => Clipboard.SetDataObject(InstanceId);

        private void RequestClose() => RequestCloseEvent?.Invoke(this, EventArgs.Empty);
    }
}
