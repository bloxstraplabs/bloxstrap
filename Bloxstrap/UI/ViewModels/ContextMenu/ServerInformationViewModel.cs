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
        public string ServerType => string.Format(
            Resources.Strings.ContextMenu_ServerInformation_TypeFormat,
            Resources.Strings.ResourceManager.GetStringSafe($"Enums.ServerType.{_activityWatcher.ActivityServerType}"));
        public string ServerLocation { get; private set; } = Resources.Strings.ContextMenu_ServerInformation_Loading;
        public string UdmuxProxied => _activityWatcher.ActivityMachineUDMUX ? Resources.Strings.Common_Yes : Resources.Strings.Common_No;

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
