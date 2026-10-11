using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Integrations;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class RegionServerItem
    {
        public string ServerId { get; init; } = "";

        public string Location { get; init; } = "";

        public string Details { get; init; } = "";
    }

    public class RegionSelectorViewModel : NotifyPropertyChangedViewModel
    {
        private static readonly ServerRegion AutomaticRegion = new() { City = Strings.Common_Automatic };

        private long _placeId;

        private int? _nextCursor;

        public ObservableCollection<ServerRegion> Regions { get; } = new();

        public ObservableCollection<ServerRegion> PreferredRegions { get; } = new() { AutomaticRegion };

        public ObservableCollection<RegionServerItem> Servers { get; } = new();

        public string GameQuery { get; set; } = "";

        public ServerRegion? SelectedRegion { get; set; }

        public string StatusText { get; private set; } = "";

        public Visibility StatusVisibility => String.IsNullOrEmpty(StatusText) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility LoadMoreVisibility => _nextCursor is null ? Visibility.Collapsed : Visibility.Visible;

        public ICommand SearchCommand => new AsyncRelayCommand(() => Search(false));

        public ICommand LoadMoreCommand => new AsyncRelayCommand(() => Search(true));

        public ICommand JoinServerCommand => new RelayCommand<RegionServerItem>(Join);

        public bool BetterMatchmaking
        {
            get => App.Settings.Prop.BetterMatchmaking;
            set => App.Settings.Prop.BetterMatchmaking = value;
        }

        public bool RandomizeMatchmaking
        {
            get => App.Settings.Prop.RandomizeMatchmaking;
            set => App.Settings.Prop.RandomizeMatchmaking = value;
        }

        public ServerRegion? PreferredRegion
        {
            get => PreferredRegions.FirstOrDefault(x => x.Key == App.Settings.Prop.PreferredRegion) ?? AutomaticRegion;
            set => App.Settings.Prop.PreferredRegion = value is null || value == AutomaticRegion ? "" : value.Key;
        }

        public RegionSelectorViewModel()
        {
            Task.Run(LoadRegions);
        }

        private async Task LoadRegions()
        {
            try
            {
                var regions = await ServerRegions.GetRegions();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var region in regions)
                    {
                        Regions.Add(region);
                        PreferredRegions.Add(region);
                    }

                    SelectedRegion = Regions.FirstOrDefault(x => x.Key == App.Settings.Prop.PreferredRegion) ?? Regions.FirstOrDefault();

                    OnPropertyChanged(nameof(SelectedRegion));
                    OnPropertyChanged(nameof(PreferredRegion));
                });

                var nearest = (await ServerRegions.GetNearestRegions()).FirstOrDefault();

                if (nearest is not null && String.IsNullOrEmpty(App.Settings.Prop.PreferredRegion))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedRegion = Regions.FirstOrDefault(x => x.Key == nearest.Key);
                        OnPropertyChanged(nameof(SelectedRegion));
                    });
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("RegionSelectorViewModel::LoadRegions", ex);
                SetStatus(String.Format(Strings.Menu_RegionSelector_RegionsFailed, ex.Message));
            }
        }

        private void SetStatus(string text)
        {
            StatusText = text;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusVisibility));
        }

        private async Task Search(bool more)
        {
            const string LOG_IDENT = "RegionSelectorViewModel::Search";

            if (SelectedRegion is null || String.IsNullOrWhiteSpace(GameQuery))
                return;

            try
            {
                if (!more)
                {
                    Servers.Clear();
                    _nextCursor = null;

                    SetStatus(Strings.Menu_RegionSelector_Searching);

                    _placeId = GameShortcuts.ParsePlaceId(GameQuery) ?? await GameShortcuts.FindPlaceByName(GameQuery) ?? 0;

                    if (_placeId == 0)
                    {
                        SetStatus(Strings.Menu_Shortcuts_Game_Invalid);
                        return;
                    }
                }

                var (servers, nextCursor) = await ServerRegions.GetServers(_placeId, SelectedRegion, more ? _nextCursor : null);

                foreach (var server in servers)
                {
                    string uptime = server.FirstSeen is DateTime seen
                        ? String.Format(Strings.Menu_RegionSelector_Uptime, PlaytimeViewModel.FormatDuration((DateTime.UtcNow - seen.ToUniversalTime()).TotalSeconds))
                        : "";

                    Servers.Add(new RegionServerItem
                    {
                        ServerId = server.ServerId,
                        Location = $"{server.City}, {server.Country}",
                        Details = uptime
                    });
                }

                _nextCursor = servers.Any() ? nextCursor : null;
                OnPropertyChanged(nameof(LoadMoreVisibility));

                SetStatus(Servers.Any() ? "" : Strings.Menu_RegionSelector_NoServers);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                SetStatus(String.Format(Strings.Menu_QuickPlay_Error, ex.Message));
            }
        }

        private void Join(RegionServerItem? server)
        {
            if (server is null || _placeId == 0)
                return;

            Process.Start(Paths.Process, $"roblox://experiences/start?placeId={_placeId}&gameInstanceId={server.ServerId}");
        }
    }
}
