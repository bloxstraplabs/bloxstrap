using System.Windows.Input;
using Bloxstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class ServerHistoryViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public List<ActivityData>? GameHistory { get; private set; }

        public GenericTriState LoadState { get; private set; } = GenericTriState.Unknown;

        public string Error { get; private set; } = String.Empty;

        public ICommand CloseWindowCommand => new RelayCommand(RequestClose);
        
        public EventHandler? RequestCloseEvent;

        public ServerHistoryViewModel(ActivityWatcher activityWatcher)
        {
            _activityWatcher = activityWatcher;

            _activityWatcher.OnGameLeave += (_, _) => LoadData();

            LoadData();
        }

        private async void LoadData()
        {
            LoadState = GenericTriState.Unknown;
            OnPropertyChanged(nameof(LoadState));

            var entries = _activityWatcher.History.Where(x => x.UniverseDetails is null);

            if (entries.Any())
            {
                string universeIds = String.Join(',', entries.Select(x => x.UniverseId).Distinct());

                try
                {
                    await UniverseDetails.FetchBulk(universeIds);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("ServerHistoryViewModel::LoadData", ex);
                    
                    Error = ex.Message;
                    OnPropertyChanged(nameof(Error));

                    LoadState = GenericTriState.Failed;
                    OnPropertyChanged(nameof(LoadState));

                    return;
                }

                foreach (var entry in entries)
                    entry.UniverseDetails = UniverseDetails.LoadFromCache(entry.UniverseId);
            }

            GameHistory = new(_activityWatcher.History);

            var consolidatedJobIds = new List<ActivityData>();

            // consolidate activity entries from in-universe teleports
            // the time left of the latest activity gets moved to the root activity
            // the job id of the latest public server activity gets moved to the root activity
            foreach (var entry in _activityWatcher.History)
            {
                if (entry.RootActivity is not null)
                {
                    if (entry.RootActivity.TimeLeft < entry.TimeLeft)
                        entry.RootActivity.TimeLeft = entry.TimeLeft;

                    if (entry.ServerType == ServerType.Public && !consolidatedJobIds.Contains(entry))
                    {
                        entry.RootActivity.JobId = entry.JobId;
                        consolidatedJobIds.Add(entry);
                    }

                    GameHistory.Remove(entry);
                }
            }

            OnPropertyChanged(nameof(GameHistory));

            LoadState = GenericTriState.Successful;
            OnPropertyChanged(nameof(LoadState));
        }

        private void RequestClose() => RequestCloseEvent?.Invoke(this, EventArgs.Empty);
    }
}
