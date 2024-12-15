namespace Bloxstrap.Models.APIs.Roblox
{

    /// <summary>
    /// Roblox.Games.Api.Models.Response.GameDetailResponse
    /// Response model for getting the game detail
    /// </summary>
    public class GameDetailResponse
    {
        /// <summary>
        /// The game universe id
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// The game root place id
        /// </summary>
        [JsonPropertyName("rootPlaceId")]
        public long RootPlaceId { get; set; }

        /// <summary>
        /// The game name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The game description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        /// <summary>
        /// The game name in the source language, if different from the returned name.
        /// </summary>
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; } = null!;

        /// <summary>
        /// The game description in the source language, if different from the returned description.
        /// </summary>
        [JsonPropertyName("sourceDescription")]
        public string SourceDescription { get; set; } = null!;

        [JsonPropertyName("creator")]
        public GameCreator Creator { get; set; } = null!;

        /// <summary>
        /// The game paid access price
        /// </summary>
        [JsonPropertyName("price")]
        public long? Price { get; set; }

        /// <summary>
        /// The game allowed gear genres
        /// </summary>
        [JsonPropertyName("allowedGearGenres")]
        public IEnumerable<string> AllowedGearGenres { get; set; } = null!;

        /// <summary>
        /// The game allowed gear categoris
        /// </summary>
        [JsonPropertyName("allowedGearCategories")]
        public IEnumerable<string> AllowedGearCategories { get; set; } = null!;

        /// <summary>
        /// The game allows place to be copied
        /// </summary>
        [JsonPropertyName("isGenreEnforced")]
        public bool IsGenreEnforced { get; set; }

        /// <summary>
        /// The game allows place to be copied
        /// </summary>
        [JsonPropertyName("copyingAllowed")]
        public bool CopyingAllowed { get; set; }

        /// <summary>
        /// Current player count of the game
        /// </summary>
        [JsonPropertyName("playing")]
        public long Playing { get; set; }

        /// <summary>
        /// The total visits to the game
        /// </summary>
        [JsonPropertyName("visits")]
        public long Visits { get; set; }

        /// <summary>
        /// The game max players
        /// </summary>
        [JsonPropertyName("maxPlayers")]
        public int MaxPlayers { get; set; }

        /// <summary>
        /// The game created time
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// The game updated time
        /// </summary>
        [JsonPropertyName("updated")]
        public DateTime Updated { get; set; }

        /// <summary>
        /// The setting of IsStudioAccessToApisAllowed of the universe
        /// </summary>
        [JsonPropertyName("studioAccessToApisAllowed")]
        public bool StudioAccessToApisAllowed { get; set; }

        /// <summary>
        /// Gets or sets the flag to indicate whether the create vip servers button should be allowed in game details page
        /// </summary>
        [JsonPropertyName("createVipServersAllowed")]
        public bool CreateVipServersAllowed { get; set; }

        /// <summary>
        /// Avatar type. Possible values are MorphToR6, MorphToR15, and PlayerChoice ['MorphToR6' = 1, 'PlayerChoice' = 2, 'MorphToR15' = 3]
        /// </summary>
        [JsonPropertyName("universeAvatarType")]
        public string UniverseAvatarType { get; set; } = null!;

        /// <summary>
        /// The game genre display name
        /// </summary>
        [JsonPropertyName("genre")]
        public string Genre { get; set; } = null!;

        /// <summary>
        /// Is this game all genre.
        /// </summary>
        [JsonPropertyName("isAllGenre")]
        public bool IsAllGenre { get; set; }

        /// <summary>
        /// Is this game favorited by user.
        /// </summary>
        [JsonPropertyName("isFavoritedByUser")]
        public bool IsFavoritedByUser { get; set; }

        /// <summary>
        /// Game number of favorites.
        /// </summary>
        [JsonPropertyName("favoritedCount")]
        public int FavoritedCount { get; set; }
    }
}
