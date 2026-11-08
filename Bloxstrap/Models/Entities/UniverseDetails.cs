namespace Bloxstrap.Models.Entities
{
    /// <summary>
    /// Explicit loading. Load from cache before and after a fetch.
    /// </summary>
    public class UniverseDetails
    {
        private static readonly List<UniverseDetails> _cache = new();

        public GameDetailResponse Data { get; set; } = null!;

        /// <summary>
        /// Returns data for a 128x128 icon
        /// </summary>
        public ThumbnailResponse Thumbnail { get; set; } = null!;

        public static UniverseDetails? LoadFromCache(long id)
        {
            return _cache.FirstOrDefault(x => x.Data?.Id == id);
        }

        public static Task FetchSingle(long id) => FetchBulk(id.ToString());

        public static async Task FetchBulk(string ids)
        {
            var gameDetailResponse = await Http.GetJson<ApiArrayResponse<GameDetailResponse>>($"https://games.roblox.com/v1/games?universeIds={ids}");

            if (gameDetailResponse?.Data is null || !gameDetailResponse.Data.Any())
                return;

            ApiArrayResponse<ThumbnailResponse>? universeThumbnailResponse = null;
            try
            {
                universeThumbnailResponse = await Http.GetJson<ApiArrayResponse<ThumbnailResponse>>(
                    $"https://thumbnails.roblox.com/v1/games/icons?universeIds={ids}&returnPolicy=PlaceHolder&size=128x128&format=Png&isCircular=false");
            }
            catch (Exception)
            {
                // Thumbnail endpoint failures should not prevent loading game details
            }

            CacheUniverseEntries(ids, gameDetailResponse.Data, universeThumbnailResponse?.Data);
        }

        private static void CacheUniverseEntries(string ids, IEnumerable<GameDetailResponse> details, IEnumerable<ThumbnailResponse>? thumbnails)
        {
            foreach (string strId in ids.Split(','))
            {
                if (!long.TryParse(strId.Trim(), out long id))
                    continue;

                var gameDetail = details.FirstOrDefault(x => x.Id == id);
                if (gameDetail is null)
                    continue;

                var thumbnail = thumbnails?.FirstOrDefault(x => x.TargetId == id)
                    ?? new ThumbnailResponse { TargetId = id, ImageUrl = "roblox" };

                _cache.RemoveAll(x => x.Data?.Id == id);
                _cache.Add(new UniverseDetails
                {
                    Data = gameDetail,
                    Thumbnail = thumbnail
                });
            }
        }
    }
}
