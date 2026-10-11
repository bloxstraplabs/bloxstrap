using System.Collections.Concurrent;

namespace Bloxstrap.Models.Entities
{
    public class UniverseDetails
    {
        private static readonly ConcurrentDictionary<long, UniverseDetails> _cache = new();

        private static readonly ConcurrentDictionary<string, Task> _pending = new();

        public GameDetailResponse Data { get; set; } = null!;

        public ThumbnailResponse Thumbnail { get; set; } = null!;

        public static UniverseDetails? LoadFromCache(long id) => _cache.TryGetValue(id, out var details) ? details : null;

        public static Task FetchSingle(long id) => FetchBulk(id.ToString());

        public static Task FetchBulk(string ids)
        {
            var missing = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(long.Parse)
                .Where(x => !_cache.ContainsKey(x))
                .Distinct()
                .ToList();

            if (!missing.Any())
                return Task.CompletedTask;

            string key = String.Join(',', missing);

            return _pending.GetOrAdd(key, _ => Fetch(key));
        }

        private static async Task Fetch(string ids)
        {
            try
            {
                var gameDetailResponse = await Http.GetJson<ApiArrayResponse<GameDetailResponse>>($"https://games.roblox.com/v1/games?universeIds={ids}");

                if (!gameDetailResponse.Data.Any())
                    return;

                var universeThumbnailResponse = await Http.GetJson<ApiArrayResponse<ThumbnailResponse>>($"https://thumbnails.roblox.com/v1/games/icons?universeIds={ids}&returnPolicy=PlaceHolder&size=128x128&format=Png&isCircular=false");

                if (!universeThumbnailResponse.Data.Any())
                    throw new InvalidHTTPResponseException("Roblox API for Game Thumbnails returned invalid data");

                foreach (var data in gameDetailResponse.Data)
                {
                    var thumbnail = universeThumbnailResponse.Data.FirstOrDefault(x => x.TargetId == data.Id);

                    if (thumbnail is not null)
                        _cache[data.Id] = new UniverseDetails { Data = data, Thumbnail = thumbnail };
                }
            }
            finally
            {
                _pending.TryRemove(ids, out _);
            }
        }
    }
}
