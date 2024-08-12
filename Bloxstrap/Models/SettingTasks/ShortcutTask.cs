using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models.SettingTasks
{
    public class ShortcutTask : BaseTask, ISettingTask
    {
        public string ExeFlags { get; set; } = "";

        public string ShortcutPath { get; set; }

        public ShortcutTask(string shortcutPath)
        {
            ShortcutPath = shortcutPath;

            OriginalState = File.Exists(ShortcutPath);
        }

        public override void Execute()
        {
            if (NewState == OriginalState)
                return;

            if (NewState)
                Shortcut.Create(Paths.Application, ExeFlags, ShortcutPath);
            else
                File.Delete(ShortcutPath);

            OriginalState = NewState;
        }
    }
}
