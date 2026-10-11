using Bloxstrap.Models.Entities;

namespace Bloxstrap
{
    public static class GameStatsTracker
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public static GameStats Load()
        {
            var manager = new JsonManager<GameStats>();
            manager.Load(false);
            return manager.Prop;
        }

        public static IEnumerable<GameStatsEntry> GetRecent(int count) =>
            Load().Games.Values.OrderByDescending(x => x.LastPlayed).Take(count);

        public static async Task Record(ActivityData activity)
        {
            const string LOG_IDENT = "GameStatsTracker::Record";

            if (activity.UniverseId == 0)
                return;

            var left = activity.TimeLeft ?? DateTime.Now;
            double seconds = (left - activity.TimeJoined).TotalSeconds;

            if (seconds <= 0)
                return;

            var details = activity.UniverseDetails ?? UniverseDetails.LoadFromCache(activity.UniverseId);

            if (details is null)
            {
                try
                {
                    await UniverseDetails.FetchSingle(activity.UniverseId);
                    details = UniverseDetails.LoadFromCache(activity.UniverseId);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            await _lock.WaitAsync();

            try
            {
                var manager = new JsonManager<GameStats>();
                manager.Load(false);

                if (!manager.Prop.Games.TryGetValue(activity.UniverseId, out var entry))
                {
                    entry = new GameStatsEntry { UniverseId = activity.UniverseId };
                    manager.Prop.Games[activity.UniverseId] = entry;
                }

                var root = activity.RootActivity ?? activity;

                entry.PlaceId = details?.Data.RootPlaceId is long rootPlace && rootPlace != 0 ? rootPlace : root.PlaceId;
                entry.Name = details?.Data.Name ?? (String.IsNullOrEmpty(entry.Name) ? entry.PlaceId.ToString() : entry.Name);
                entry.IconUrl = details?.Thumbnail.ImageUrl ?? entry.IconUrl;
                entry.TotalSeconds += seconds;
                entry.LastPlayed = left;

                if (activity.RootActivity is null)
                    entry.Sessions++;

                manager.Save();

                App.Logger.WriteLine(LOG_IDENT, $"Recorded {(int)seconds}s in {entry.Name}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
