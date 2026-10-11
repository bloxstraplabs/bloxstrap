using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class PlaytimeViewModel : NotifyPropertyChangedViewModel
    {
        public class GameRow
        {
            public GameStatsEntry Entry { get; init; } = null!;

            public string Name => Entry.Name;

            public string? IconUrl => Entry.IconUrl;

            public string Playtime => FormatDuration(Entry.TotalSeconds);

            public string Details => String.Format(Strings.Menu_Playtime_Details, Entry.Sessions, Entry.LastPlayed.ToString("d", CultureInfo.CurrentCulture));

            public double Share { get; init; }
        }

        public List<GameRow> Games { get; private set; } = new();

        public string TotalPlaytime { get; private set; } = "";

        public Visibility EmptyVisibility => Games.Any() ? Visibility.Collapsed : Visibility.Visible;

        public bool TrackGameStats
        {
            get => App.Settings.Prop.TrackGameStats;
            set => App.Settings.Prop.TrackGameStats = value;
        }

        public record BreakReminderOption(int Minutes, string Label);

        public IReadOnlyList<BreakReminderOption> BreakReminderOptions { get; } = new[] { 0, 30, 60, 90, 120, 180 }
            .Select(x => new BreakReminderOption(x, x == 0 ? Strings.Common_Off : FormatDuration(x * 60)))
            .ToList();

        public int BreakReminderMinutes
        {
            get => App.Settings.Prop.BreakReminderMinutes;
            set => App.Settings.Prop.BreakReminderMinutes = value;
        }

        public bool SessionRecap
        {
            get => App.Settings.Prop.SessionRecap;
            set => App.Settings.Prop.SessionRecap = value;
        }

        public ICommand ResetCommand => new RelayCommand(Reset);

        public PlaytimeViewModel()
        {
            LoadGames();
        }

        public static string FormatDuration(double seconds)
        {
            var time = TimeSpan.FromSeconds(seconds);

            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";

            return $"{time.Minutes}m";
        }

        private void LoadGames()
        {
            var entries = GameStatsTracker.Load().Games.Values.OrderByDescending(x => x.TotalSeconds).ToList();
            double max = entries.FirstOrDefault()?.TotalSeconds ?? 0;

            Games = entries.Select(x => new GameRow { Entry = x, Share = max > 0 ? x.TotalSeconds / max : 0 }).ToList();
            TotalPlaytime = FormatDuration(entries.Sum(x => x.TotalSeconds));

            OnPropertyChanged(nameof(Games));
            OnPropertyChanged(nameof(TotalPlaytime));
            OnPropertyChanged(nameof(EmptyVisibility));
        }

        private void Reset()
        {
            var result = Frontend.ShowMessageBox(Strings.Menu_Playtime_ResetConfirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            var manager = new JsonManager<GameStats>();
            manager.Load(false);
            manager.Prop.Games.Clear();
            manager.Save();

            LoadGames();
        }
    }
}
