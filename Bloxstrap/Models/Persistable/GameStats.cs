namespace Bloxstrap.Models.Persistable
{
    public class GameStats
    {
        public Dictionary<long, GameStatsEntry> Games { get; set; } = new();
    }

    public class GameStatsEntry
    {
        public long UniverseId { get; set; }

        public long PlaceId { get; set; }

        public string Name { get; set; } = "";

        public string? IconUrl { get; set; }

        public double TotalSeconds { get; set; }

        public int Sessions { get; set; }

        public DateTime LastPlayed { get; set; }
    }
}
