namespace Bloxstrap.Models
{
    public class State
    {
        public bool ShowFFlagEditorWarning { get; set; } = true;
        
        public bool PromptWebView2Install { get; set; } = true;

        public AppState Player { get; set; } = new();
        
        public AppState Studio { get; set; } = new();

        public WindowState SettingsWindow { get; set; } = new();

        public List<string> ModManifest { get; set; } = new();
    }
}
