namespace Bloxstrap.Models
{
    public class AppState
    {
        public string VersionGuid { get; set; } = String.Empty;

        public Dictionary<string, string> PackageHashes { get; set; } = new();

        public int Size { get; set; }
    }
}
