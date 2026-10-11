namespace Bloxstrap.Models.Persistable
{
    public class State
    {
        public bool PromptWebView2Install { get; set; } = true;

        public bool ForceReinstall { get; set; } = false;

        public bool ShowWelcome { get; set; } = false;

        public DateTime LastThemeCycle { get; set; } = DateTime.MinValue;

        public WindowState SettingsWindow { get; set; } = new();
    }
}
