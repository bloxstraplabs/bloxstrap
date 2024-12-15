using System.Windows.Input;
using Bloxstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class OutputConsoleViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public Dictionary<int, ActivityData.UserLog>? PlayerLogs { get; private set; }

        public IEnumerable<KeyValuePair<int, ActivityData.UserLog>>? PlayerLogsCollection => PlayerLogs;

        public GenericTriState LoadState { get; private set; } = GenericTriState.Unknown;

        public string Error { get; private set; } = String.Empty;

        public ICommand CloseWindowCommand => new RelayCommand(RequestClose);

        public EventHandler? RequestCloseEvent;

        public OutputConsoleViewModel(ActivityWatcher activityWatcher)
        {
            _activityWatcher = activityWatcher;

            _activityWatcher.OnNewPlayerRequest += (_, _) => LoadData();

            LoadData();
        }

        private void LoadData()
        {
            LoadState = GenericTriState.Unknown;
            OnPropertyChanged(nameof(LoadState));

            PlayerLogs = new Dictionary<int, ActivityData.UserLog>(_activityWatcher.PlayerLogs);

            OnPropertyChanged(nameof(PlayerLogsCollection));

            LoadState = GenericTriState.Successful;
            OnPropertyChanged(nameof(LoadState));
        }

        private void RequestClose() => RequestCloseEvent?.Invoke(this, EventArgs.Empty);
    }
}
