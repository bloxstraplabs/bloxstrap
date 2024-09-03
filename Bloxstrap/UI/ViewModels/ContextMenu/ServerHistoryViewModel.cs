using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using Bloxstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class ServerHistoryViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public List<ActivityHistoryEntry>? ActivityHistory { get; private set; }

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
            var entries = _activityWatcher.ActivityHistory.Where(x => !x.DetailsLoaded);

            if (entries.Any())
            {
                string universeIds = String.Join(',', entries.Select(x => x.UniverseId));

                var gameDetailResponse = await Http.GetJson<ApiArrayResponse<GameDetailResponse>>($"https://games.roblox.com/v1/games?universeIds={universeIds}");

                if (gameDetailResponse is null || !gameDetailResponse.Data.Any())
                    return;

                var universeThumbnailResponse = await Http.GetJson<ApiArrayResponse<ThumbnailResponse>>($"https://thumbnails.roblox.com/v1/games/icons?universeIds={universeIds}&returnPolicy=PlaceHolder&size=128x128&format=Png&isCircular=false");

                if (universeThumbnailResponse is null || !universeThumbnailResponse.Data.Any())
                    return;

                foreach (var entry in entries)
                {
                    entry.GameName = gameDetailResponse.Data.Where(x => x.Id == entry.UniverseId).Select(x => x.Name).First();
                    entry.GameThumbnail = universeThumbnailResponse.Data.Where(x => x.TargetId == entry.UniverseId).Select(x => x.ImageUrl).First();
                    entry.DetailsLoaded = true;
                }
            }

            ActivityHistory = new(_activityWatcher.ActivityHistory);
            OnPropertyChanged(nameof(ActivityHistory));
        }

        private void RequestClose() => RequestCloseEvent?.Invoke(this, EventArgs.Empty);
    }
}
