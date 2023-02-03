using System;
using System.IO;

namespace Bloxstrap.Helpers
{
    class Directories
    {
        // note that these are directories that aren't tethered to the basedirectory
        // so these can safely be called before initialization
        public static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public static string StartMenu => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", App.ProjectName);
        public static string MyPictures => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        public static string Base { get; private set; } = "";
        public static string Downloads { get; private set; } = "";
        public static string Integrations { get; private set; } = "";
        public static string Versions { get; private set; } = "";
        public static string Modifications { get; private set; } = "";
        public static string Updates { get; private set; } = "";

        public static string Application { get; private set; } = "";

        public static bool Initialized => String.IsNullOrEmpty(Base);

        public static void Initialize(string baseDirectory)
        {
            Base = baseDirectory;
            Downloads = Path.Combine(Base, "Downloads");
            Integrations = Path.Combine(Base, "Integrations");
            Versions = Path.Combine(Base, "Versions");
            Modifications = Path.Combine(Base, "Modifications");
            Updates = Path.Combine(Base, "Updates");

            Application = Path.Combine(Base, $"{App.ProjectName}.exe");
        }
    }
}
