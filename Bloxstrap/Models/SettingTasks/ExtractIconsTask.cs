using System.Reflection;
using System.Windows.Markup;

namespace Bloxstrap.Models.SettingTasks
{
    public class ExtractIconsTask : BoolBaseTask
    {
        public ExtractIconsTask() : base("ExtractIcons")
        {
            OriginalState = Directory.Exists(Paths.Icons);
        }

        public override void Execute()
        {
            if (NewState)
            {
                Directory.CreateDirectory(Paths.Icons);

                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames().Where(x => x.EndsWith(".ico"));

                foreach (string name in resourceNames)
                {
                    string path = Path.Combine(Paths.Icons, name.Replace("Bloxstrap.Resources.", ""));
                    var stream = assembly.GetManifestResourceStream(name)!;

                    using var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);

                    Filesystem.AssertReadOnly(path);
                    File.WriteAllBytes(path, memoryStream.ToArray());
                }
            }
            else if (Directory.Exists(Paths.Icons))
            {
                Directory.Delete(Paths.Icons, true);
            }

            OriginalState = NewState;
        }
    }
}
