namespace Bloxstrap.Models
{
    public class State
    {
        public string LastEnrolledChannel { get; set; } = "";
        public bool ShowFFlagEditorWarning { get; set; } = true;

        [Obsolete("Use PlayerVersionGuid instead", true)]
        public string VersionGuid { set { PlayerVersionGuid = value; } }
        public string PlayerVersionGuid { get; set; } = "";
        public string StudioVersionGuid { get; set; } = "";

        public int PlayerSize { get; set; } = 0;
        public int StudioSize { get; set; } = 0;

        public List<string> ModManifest { get; set; } = new();
    }
}
