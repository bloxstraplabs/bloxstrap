namespace Bloxstrap.Models.Persistable
{
    public class LastServer
    {
        public long PlaceId { get; set; }

        public long UniverseId { get; set; }

        public string JobId { get; set; } = "";

        public bool IsPublic { get; set; }

        public DateTime Joined { get; set; }
    }
}
