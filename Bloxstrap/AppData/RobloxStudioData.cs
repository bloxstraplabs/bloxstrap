namespace Bloxstrap.AppData
{
    public class RobloxStudioData : CommonAppData, IAppData
    {
        public string ProductName => "Roblox Studio";

        public string BinaryType => "WindowsStudio64";

        public string RegistryName => "RobloxStudio";

        public override string ExecutableName => "RobloxStudioBeta.exe";

        public string StartEvent => "www.roblox.com/robloxStudioStartedEvent";

        public override string Directory => Path.Combine(Paths.Roblox, "Studio");
        
        public AppState State => App.State.Prop.Studio;

        public override IReadOnlyDictionary<string, string> PackageDirectoryMap { get; set; } = new Dictionary<string, string>()
        {
            { "RobloxStudio.zip",                @"" },
            { "redist.zip",                      @"" },
            { "LibrariesQt5.zip",                @"" },
            
            { "content-studio_svg_textures.zip", @"content\studio_svg_textures\"},
            { "content-qt_translations.zip",     @"content\qt_translations\" },
            { "content-api-docs.zip",            @"content\api_docs\" },

            { "extracontent-scripts.zip",        @"ExtraContent\scripts\" },

            { "BuiltInPlugins.zip",              @"BuiltInPlugins\" },
            { "BuiltInStandalonePlugins.zip",    @"BuiltInStandalonePlugins\" },

            { "ApplicationConfig.zip",           @"ApplicationConfig\" },
            { "Plugins.zip",                     @"Plugins\" },
            { "Qml.zip",                         @"Qml\" },
            { "StudioFonts.zip",                 @"StudioFonts\" }
        };
    }
}
