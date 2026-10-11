using Bloxstrap.Models.SettingTasks.Base;

namespace Bloxstrap.Models.SettingTasks
{
    public class FileModTask : StringBaseTask
    {
        private readonly string[] _targets;

        public string PrimaryTarget => _targets[0];

        public FileModTask(string name, params string[] targets) : base("ModPreset", name)
        {
            _targets = targets.Select(x => Path.Combine(Paths.Modifications, x)).ToArray();

            if (File.Exists(PrimaryTarget))
                OriginalState = PrimaryTarget;
        }

        public override void Execute()
        {
            if (!String.IsNullOrEmpty(NewState))
            {
                if (String.Compare(NewState, PrimaryTarget, StringComparison.InvariantCultureIgnoreCase) != 0 && File.Exists(NewState))
                {
                    foreach (string target in _targets)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

                        Filesystem.AssertReadOnly(target);
                        File.Copy(NewState, target, true);
                    }
                }
            }
            else
            {
                foreach (string target in _targets.Where(File.Exists))
                {
                    Filesystem.AssertReadOnly(target);
                    File.Delete(target);
                }
            }

            OriginalState = NewState;
        }
    }
}
