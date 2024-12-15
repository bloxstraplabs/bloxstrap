namespace Bloxstrap.Models.Persistable
{
    public class State
    {
        public bool IgnoreOutdatedChannel { get; set; } = false;
        public bool WatcherRunning { get; set; } = false;

        public bool PromptWebView2Install { get; set; } = true;

        public AppState Player { get; set; } = new();

        public AppState Studio { get; set; } = new();

        public WindowState SettingsWindow { get; set; } = new();

        public List<string> ModManifest { get; set; } = new();
    }
}
