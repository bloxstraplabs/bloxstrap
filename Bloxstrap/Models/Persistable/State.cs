namespace Bloxstrap.Models.Persistable
{
    public class State
    {
        public bool PromptWebView2Install { get; set; } = true;

        public bool ForceReinstall { get; set; } = false;

        public bool SetupCompleted { get; set; } = false;

        public WindowState SettingsWindow { get; set; } = new();
    }
}
