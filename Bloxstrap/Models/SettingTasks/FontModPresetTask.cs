using Bloxstrap.Models.SettingTasks.Base;

namespace Bloxstrap.Models.SettingTasks
{
    public class FontModPresetTask : StringBaseTask
    {
        public string? GetFileHash()
        {
            if (!File.Exists(Paths.CustomFont))
                return null;

            using var fileStream = File.OpenRead(Paths.CustomFont);
            return MD5Hash.Stringify(App.MD5Provider.ComputeHash(fileStream));
        }

        public FontModPresetTask() : base("ModPreset", "TextFont")
        {
            if (File.Exists(Paths.CustomFont))
                OriginalState = Paths.CustomFont;
        }

        public override void Execute()
        {
            if (!String.IsNullOrEmpty(NewState))
            {
                if (String.Compare(NewState, Paths.CustomFont, StringComparison.InvariantCultureIgnoreCase) != 0 && File.Exists(NewState))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Paths.CustomFont)!);

                    Filesystem.AssertReadOnly(Paths.CustomFont);
                    File.Copy(NewState, Paths.CustomFont, true);
                }
            }
            else if (File.Exists(Paths.CustomFont))
            {
                Filesystem.AssertReadOnly(Paths.CustomFont);
                File.Delete(Paths.CustomFont);
            }

            OriginalState = NewState;
        }
    }
}
