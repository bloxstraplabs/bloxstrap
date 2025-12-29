namespace Bloxstrap.Models.Persistable
{
    public class DistributionState
    {
        public string VersionGuid { get; set; } = string.Empty;

        public Dictionary<string, string> PackageHashes { get; set; } = new();

        public int Size { get; set; }

        public List<string> ModManifest { get; set; } = new();
    }
}
