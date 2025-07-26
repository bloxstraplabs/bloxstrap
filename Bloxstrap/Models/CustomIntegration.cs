namespace Bloxstrap.Models
{
    public class CustomIntegration
    {
        public string Name { get; set; } = "";
        public string Location { get; set; } = "";
        public string LaunchArgs { get; set; } = "";
        public int Delay { get; set; } = 0;
        public bool PreLaunch { get; set; } = false;
        public bool AutoClose { get; set; } = true;
    }
}
