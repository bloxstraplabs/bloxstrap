namespace Bloxstrap.Models.Persistable
{
    public class State
    {
        public string CurrentVersion { get; set; } = "None";

        public bool PromptWebView2Install { get; set; } = true;

        public AppState Player { get; set; } = new();

        public AppState Studio { get; set; } = new();

        public WindowState SettingsWindow { get; set; } = new();

        public List<string> ModManifest { get; set; } = new();
    }
}
