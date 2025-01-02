using System.Windows.Input;
using Bloxstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class ChatLogsViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public Dictionary<int, ActivityData.UserMessage>? MessageLogs { get; private set; }

        public IEnumerable<KeyValuePair<int, ActivityData.UserMessage>>? MessageLogsCollection => MessageLogs;

        public GenericTriState LoadState { get; private set; } = GenericTriState.Unknown;

        public string Error { get; private set; } = String.Empty;

        public ICommand CloseWindowCommand => new RelayCommand(RequestClose);

        public EventHandler? RequestCloseEvent;

        public ChatLogsViewModel(ActivityWatcher activityWatcher)
        {
            _activityWatcher = activityWatcher;

            _activityWatcher.OnNewMessageRequest += (_, _) => LoadData();

            LoadData();
        }

        private void LoadData()
        {
            LoadState = GenericTriState.Unknown;
            OnPropertyChanged(nameof(LoadState));

            MessageLogs = new Dictionary<int, ActivityData.UserMessage>(_activityWatcher.MessageLogs);

            OnPropertyChanged(nameof(MessageLogsCollection));

            LoadState = GenericTriState.Successful;
            OnPropertyChanged(nameof(LoadState));
        }

        private void RequestClose() => RequestCloseEvent?.Invoke(this, EventArgs.Empty);
    }
}
