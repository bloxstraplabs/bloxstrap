using System.Reflection;

namespace Bloxstrap.Models.SettingTasks
{
    public class ExtractIconsTask : BoolBaseTask
    {
        private string _path => Path.Combine(Paths.Base, Strings.Paths_Icons);

        public ExtractIconsTask() : base("ExtractIcons")
        {
            OriginalState = Directory.Exists(_path);
        }

        public override void Execute()
        {
            if (NewState)
            {
                Directory.CreateDirectory(_path);

                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames().Where(x => x.EndsWith(".ico"));

                foreach (string name in resourceNames)
                {
                    string path = Path.Combine(_path, name.Replace("Bloxstrap.Resources.", ""));
                    var stream = assembly.GetManifestResourceStream(name)!;

                    using var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);

                    Filesystem.AssertReadOnly(path);
                    File.WriteAllBytes(path, memoryStream.ToArray());
                }
            }
            else if (Directory.Exists(_path))
            {
                Directory.Delete(_path, true);
            }

            OriginalState = NewState;
        }
    }
}
