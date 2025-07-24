namespace Bloxstrap
{
    static class Paths
    {
        // note that these are directories that aren't tethered to the basedirectory
        // so these can safely be called before initialization
        public static string Temp => Path.Combine(Path.GetTempPath(), App.ProjectName);
        public static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public static string WindowsStartMenu => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
        public static string System => Environment.GetFolderPath(Environment.SpecialFolder.System);

        public static string Process => Environment.ProcessPath!;

        public static string TempUpdates => Path.Combine(Temp, "Updates");
        public static string TempLogs => Path.Combine(Temp, "Logs");

        public static string Base { get; private set; } = "";
        public static string Downloads { get; private set; } = "";

        public static string SavedFlagProfiles { get; private set; } = "";
        public static string Logs { get; private set; } = "";
        public static string Integrations { get; private set; } = "";
        public static string Versions { get; private set; } = "";
        public static string Modifications { get; private set; } = "";
        public static string Roblox { get; private set; } = "";
        public static string CustomThemes { get; private set; } = "";

        // cleaner paths
        public static string RobloxLogs { get; private set; } = "";
        public static string RobloxCache { get; private set; } = "";

        public static string Application { get; private set; } = "";

        public static string CustomFont => Path.Combine(Modifications, "content\\fonts\\CustomFont.ttf");

        public static bool Initialized => !String.IsNullOrEmpty(Base);

        public static void Initialize(string baseDirectory)
        {
            Base = baseDirectory;
            Downloads = Path.Combine(Base, "Downloads");
            SavedFlagProfiles = Path.Combine(Base, "SavedFlagProfiles");
            Logs = Path.Combine(Base, "Logs");
            Integrations = Path.Combine(Base, "Integrations");
            Versions = Path.Combine(Base, "Versions");
            Modifications = Path.Combine(Base, "Modifications");
            CustomThemes = Path.Combine(Base, "CustomThemes");
            Roblox = Path.Combine(Base, "Roblox");

            RobloxLogs = Path.Combine(Roblox, "logs");
            RobloxCache = Path.Combine(Path.GetTempPath(), "Roblox");

            Application = Path.Combine(Base, $"{App.ProjectName}.exe");
        }
    }
}
