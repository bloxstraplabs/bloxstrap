using System.IO;

namespace Bloxstrap.Helpers
{
    class Directories
    {
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
