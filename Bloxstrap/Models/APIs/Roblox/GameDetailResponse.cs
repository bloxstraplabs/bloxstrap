namespace Bloxstrap.Models.APIs.Roblox
{

    public class GameDetailResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("rootPlaceId")]
        public long RootPlaceId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; } = null!;

        [JsonPropertyName("sourceDescription")]
        public string SourceDescription { get; set; } = null!;

        [JsonPropertyName("creator")]
        public GameCreator Creator { get; set; } = null!;

        [JsonPropertyName("price")]
        public long? Price { get; set; }

        [JsonPropertyName("allowedGearGenres")]
        public IEnumerable<string> AllowedGearGenres { get; set; } = null!;

        [JsonPropertyName("allowedGearCategories")]
        public IEnumerable<string> AllowedGearCategories { get; set; } = null!;

        [JsonPropertyName("isGenreEnforced")]
        public bool IsGenreEnforced { get; set; }

        [JsonPropertyName("copyingAllowed")]
        public bool CopyingAllowed { get; set; }

        [JsonPropertyName("playing")]
        public long Playing { get; set; }

        [JsonPropertyName("visits")]
        public long Visits { get; set; }

        [JsonPropertyName("maxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("updated")]
        public DateTime Updated { get; set; }

        [JsonPropertyName("studioAccessToApisAllowed")]
        public bool StudioAccessToApisAllowed { get; set; }

        [JsonPropertyName("createVipServersAllowed")]
        public bool CreateVipServersAllowed { get; set; }

        [JsonPropertyName("universeAvatarType")]
        public string UniverseAvatarType { get; set; } = null!;

        [JsonPropertyName("genre")]
        public string Genre { get; set; } = null!;

        [JsonPropertyName("isAllGenre")]
        public bool IsAllGenre { get; set; }

        [JsonPropertyName("isFavoritedByUser")]
        public bool IsFavoritedByUser { get; set; }

        [JsonPropertyName("favoritedCount")]
        public int FavoritedCount { get; set; }
    }
}
