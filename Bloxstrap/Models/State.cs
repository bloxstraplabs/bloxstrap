namespace Bloxstrap.Models
{
    public class State
    {
        public string VersionGuid { get; set; } = "";
        public List<string> ModManifest { get; set; } = new();
    }
}
