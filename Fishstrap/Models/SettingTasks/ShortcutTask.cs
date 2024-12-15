namespace Bloxstrap.Models.SettingTasks
{
    public class ShortcutTask : BoolBaseTask
    {
        private string _shortcutPath;

        private string _exeFlags;

        public ShortcutTask(string name, string lnkFolder, string lnkName, string exeFlags = "") : base("Shortcut", name)
        {
            _shortcutPath = Path.Combine(lnkFolder, lnkName);
            _exeFlags = exeFlags;

            OriginalState = File.Exists(_shortcutPath);
        }

        public override void Execute()
        {
            if (NewState)
                Shortcut.Create(Paths.Application, _exeFlags, _shortcutPath);
            else if (File.Exists(_shortcutPath))
                File.Delete(_shortcutPath);

            OriginalState = NewState;
        }
    }
}