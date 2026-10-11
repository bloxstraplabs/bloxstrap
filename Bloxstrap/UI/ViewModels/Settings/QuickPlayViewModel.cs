using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class QuickPlayGame
    {
        public long UniverseId { get; set; }

        public long PlaceId { get; set; }

        public string Name { get; set; } = "";

        public string? IconUrl { get; set; }

        public string Details { get; set; } = "";
    }

    public class QuickPlayServer
    {
        public long PlaceId { get; set; }

        public string JobId { get; set; } = "";

        public string Players { get; set; } = "";

        public string Details { get; set; } = "";
    }

    public class QuickPlayViewModel : NotifyPropertyChangedViewModel
    {
        private class SearchResponse
        {
            [JsonPropertyName("searchResults")]
            public List<SearchGroup> SearchResults { get; set; } = new();
        }

        private class SearchGroup
        {
            [JsonPropertyName("contentGroupType")]
            public string ContentGroupType { get; set; } = "";

            [JsonPropertyName("contents")]
            public List<SearchContent> Contents { get; set; } = new();
        }

        private class SearchContent
        {
            [JsonPropertyName("universeId")]
            public long UniverseId { get; set; }
        }

        private class UniverseIdResponse
        {
            [JsonPropertyName("universeId")]
            public long? UniverseId { get; set; }
        }

        private class PlacesResponse
        {
            [JsonPropertyName("data")]
            public List<PlaceEntry> Data { get; set; } = new();
        }

        private class PlaceEntry
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; } = "";
        }

        private class ServersResponse
        {
            [JsonPropertyName("data")]
            public List<ServerEntry> Data { get; set; } = new();
        }

        private class ServerEntry
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = "";

            [JsonPropertyName("playing")]
            public int Playing { get; set; }

            [JsonPropertyName("maxPlayers")]
            public int MaxPlayers { get; set; }

            [JsonPropertyName("ping")]
            public int? Ping { get; set; }

            [JsonPropertyName("fps")]
            public double? Fps { get; set; }
        }

        private readonly LastServer? _lastServer = GameStatsTracker.LoadLastServer();

        public string Query { get; set; } = "";

        public bool IsBusy { get; private set; }

        public string StatusText { get; private set; } = "";

        public Visibility StatusVisibility => String.IsNullOrEmpty(StatusText) ? Visibility.Collapsed : Visibility.Visible;

        public ObservableCollection<QuickPlayGame> Results { get; } = new();

        public ObservableCollection<QuickPlayGame> RecentGames { get; } = new();

        public Visibility RecentVisibility => RecentGames.Any() ? Visibility.Visible : Visibility.Collapsed;

        public QuickPlayGame? SelectedGame { get; private set; }

        public Visibility SelectedVisibility => SelectedGame is null ? Visibility.Collapsed : Visibility.Visible;

        public ObservableCollection<QuickPlayGame> Subplaces { get; } = new();

        public ObservableCollection<QuickPlayServer> Servers { get; } = new();

        public bool CanRejoin => _lastServer is { IsPublic: true } && !String.IsNullOrEmpty(_lastServer.JobId);

        public string LastServerText => _lastServer is null
            ? Strings.Menu_QuickPlay_Rejoin_None
            : String.Format(Strings.Menu_QuickPlay_Rejoin_Details, _lastServer.Joined.ToString("g"));

        public ICommand SearchCommand => new AsyncRelayCommand(Search);

        public ICommand JoinGameCommand => new RelayCommand<QuickPlayGame>(game => Join(game?.PlaceId ?? 0, null));

        public ICommand ShowGameCommand => new AsyncRelayCommand<QuickPlayGame>(ShowGame);

        public ICommand JoinServerCommand => new RelayCommand<QuickPlayServer>(server => Join(server?.PlaceId ?? 0, server?.JobId));

        public ICommand RefreshServersCommand => new AsyncRelayCommand(() => LoadServers(SelectedGame?.PlaceId ?? 0));

        public ICommand RejoinCommand => new RelayCommand(() => Join(_lastServer?.PlaceId ?? 0, _lastServer?.JobId));

        public QuickPlayViewModel()
        {
            if (!App.Settings.Prop.TrackGameStats)
                return;

            foreach (var entry in GameStatsTracker.GetRecent(6))
            {
                RecentGames.Add(new QuickPlayGame
                {
                    UniverseId = entry.UniverseId,
                    PlaceId = entry.PlaceId,
                    Name = entry.Name,
                    IconUrl = entry.IconUrl,
                    Details = String.Format(Strings.Menu_QuickPlay_LastPlayed, entry.LastPlayed.ToString("g"))
                });
            }
        }

        private void SetStatus(string text)
        {
            StatusText = text;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusVisibility));
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            OnPropertyChanged(nameof(IsBusy));
        }

        private static void Join(long placeId, string? jobId)
        {
            if (placeId == 0)
                return;

            string deeplink = $"roblox://experiences/start?placeId={placeId}";

            if (!String.IsNullOrEmpty(jobId))
                deeplink += $"&gameInstanceId={jobId}";

            Process.Start(Paths.Process, deeplink);
        }

        private static async Task<List<QuickPlayGame>> LoadGames(IEnumerable<long> universeIds)
        {
            var ids = universeIds.Where(x => x != 0).Distinct().Take(20).ToList();

            if (!ids.Any())
                return new();

            await UniverseDetails.FetchBulk(String.Join(',', ids));

            var games = new List<QuickPlayGame>();

            foreach (long id in ids)
            {
                var details = UniverseDetails.LoadFromCache(id);

                if (details is null)
                    continue;

                games.Add(new QuickPlayGame
                {
                    UniverseId = id,
                    PlaceId = details.Data.RootPlaceId,
                    Name = details.Data.Name,
                    IconUrl = details.Thumbnail.ImageUrl,
                    Details = String.Format(Strings.Menu_QuickPlay_Playing, details.Data.Playing.ToString("N0"), details.Data.Creator.Name)
                });
            }

            return games;
        }

        private async Task Search()
        {
            const string LOG_IDENT = "QuickPlayViewModel::Search";

            string query = Query.Trim();

            if (String.IsNullOrEmpty(query) || IsBusy)
                return;

            SetBusy(true);
            SetStatus(Strings.Menu_QuickPlay_Searching);
            Results.Clear();

            try
            {
                List<long> universeIds;

                if (GameShortcuts.ParsePlaceId(query) is long placeId)
                {
                    var universe = await Http.GetJson<UniverseIdResponse>($"https://apis.roblox.com/universes/v1/places/{placeId}/universe");
                    universeIds = universe.UniverseId is long id ? new() { id } : new();
                }
                else
                {
                    var response = await Http.GetJson<SearchResponse>($"https://apis.roblox.com/search-api/omni-search?searchQuery={Uri.EscapeDataString(query)}&sessionId={Guid.NewGuid()}&pageType=all");

                    universeIds = response.SearchResults
                        .Where(x => x.ContentGroupType == "Game")
                        .SelectMany(x => x.Contents)
                        .Select(x => x.UniverseId)
                        .ToList();
                }

                foreach (var game in await LoadGames(universeIds))
                    Results.Add(game);

                SetStatus(Results.Any() ? "" : Strings.Menu_QuickPlay_NoResults);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                SetStatus(String.Format(Strings.Menu_QuickPlay_Error, ex.Message));
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ShowGame(QuickPlayGame? game)
        {
            const string LOG_IDENT = "QuickPlayViewModel::ShowGame";

            if (game is null)
                return;

            SelectedGame = game;
            OnPropertyChanged(nameof(SelectedGame));
            OnPropertyChanged(nameof(SelectedVisibility));

            Subplaces.Clear();

            try
            {
                var places = await Http.GetJson<PlacesResponse>($"https://develop.roblox.com/v1/universes/{game.UniverseId}/places?limit=50");

                foreach (var place in places.Data)
                {
                    Subplaces.Add(new QuickPlayGame
                    {
                        UniverseId = game.UniverseId,
                        PlaceId = place.Id,
                        Name = place.Name,
                        Details = place.Id == game.PlaceId ? Strings.Menu_QuickPlay_StartPlace : place.Id.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            await LoadServers(game.PlaceId);
        }

        private async Task LoadServers(long placeId)
        {
            const string LOG_IDENT = "QuickPlayViewModel::LoadServers";

            if (placeId == 0)
                return;

            Servers.Clear();

            try
            {
                var servers = await Http.GetJson<ServersResponse>($"https://games.roblox.com/v1/games/{placeId}/servers/Public?limit=25&sortOrder=Desc&excludeFullGames=true");

                foreach (var server in servers.Data)
                {
                    Servers.Add(new QuickPlayServer
                    {
                        PlaceId = placeId,
                        JobId = server.Id,
                        Players = $"{server.Playing}/{server.MaxPlayers}",
                        Details = server.Ping is int ping ? String.Format(Strings.Menu_QuickPlay_Ping, ping) : ""
                    });
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}
