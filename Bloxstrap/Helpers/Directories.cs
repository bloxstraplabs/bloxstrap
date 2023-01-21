using System.IO;

namespace Bloxstrap.Helpers
{
    class Directories
    {
        // note that these are directories that aren't tethered to the basedirectory
        // so these can safely be called before initialization
        public static string LocalAppData { get => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
        public static string Desktop { get => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); }
        public static string StartMenu { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", Program.ProjectName); }

        public static string Base { get; private set; } = "";
        public static string Downloads { get; private set; } = "";
        public static string Integrations { get; private set; } = "";
        public static string Versions { get; private set; } = "";
        public static string Modifications { get; private set; } = "";
        public static string Updates { get; private set; } = "";
        public static string ReShade { get; private set; } = "";

        public static string App { get; private set; } = "";

        public static bool Initialized { get => String.IsNullOrEmpty(Base); }

        public static void Initialize(string baseDirectory)
        {
            Base = baseDirectory;
            Downloads = Path.Combine(Base, "Downloads");
            Integrations = Path.Combine(Base, "Integrations");
            Versions = Path.Combine(Base, "Versions");
            Modifications = Path.Combine(Base, "Modifications");
            Updates = Path.Combine(Base, "Updates");
            ReShade = Path.Combine(Base, "ReShade");

            App = Path.Combine(Base, $"{Program.ProjectName}.exe");
        }
    }
}
