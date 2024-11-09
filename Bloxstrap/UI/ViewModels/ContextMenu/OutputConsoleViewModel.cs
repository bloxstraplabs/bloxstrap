using System.Windows.Input;
using Bloxstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class OutputConsoleViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public List<ActivityData>? GameHistory { get; private set; }

        public GenericTriState LoadState { get; private set; } = GenericTriState.Unknown;

        public string Error { get; private set; } = String.Empty;

        public ICommand CloseWindowCommand => new RelayCommand(RequestClose);
        
        public EventHandler? RequestCloseEvent;

        public OutputConsoleViewModel(ActivityWatcher activityWatcher)
        {
            _activityWatcher = activityWatcher;

            _activityWatcher.OnGameLeave += (_, _) => LoadData();

            LoadData();
        }

        private void LoadData()
        {
            

            OnPropertyChanged(nameof(GameHistory));

            LoadState = GenericTriState.Successful;
            OnPropertyChanged(nameof(LoadState));
        }

        private void RequestClose() => RequestCloseEvent?.Invoke(this, EventArgs.Empty);
    }
}
