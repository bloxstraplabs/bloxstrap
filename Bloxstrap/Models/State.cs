namespace Bloxstrap.Models
{
    public class State
    {
        public string LastEnrolledChannel { get; set; } = "";
        [Obsolete("Use PlayerVersionGuid", true)]
        public string VersionGuid { set { PlayerVersionGuid = value; } }
        public string PlayerVersionGuid { get; set; } = "";
        public string StudioVersionGuid { get; set; } = "";
        public List<string> ModManifest { get; set; } = new();
    }
}
