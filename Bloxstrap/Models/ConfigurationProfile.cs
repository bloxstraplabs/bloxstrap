namespace Bloxstrap.Models
{
    public sealed class ConfigurationProfile
    {
        public string Name { get; set; } = String.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public bool IsAutomaticUpdateBackup { get; set; }

        public Persistable.Settings Settings { get; set; } = new();

        public Dictionary<string, string> FastFlags { get; set; } = new();

        [JsonIgnore]
        public string FilePath { get; set; } = String.Empty;
    }
}
