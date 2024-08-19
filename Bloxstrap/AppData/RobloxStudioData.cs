using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.AppData
{
    public class RobloxStudioData : CommonAppData, IAppData
    {
        public string ProductName { get; } = "Roblox Studio";

        public string BinaryType { get; } = "WindowsStudio64";

        public string RegistryName { get; } = "RobloxStudio";

        public string ExecutableName { get; } = "RobloxStudioBeta.exe";

        public string StartEvent { get; } = "www.roblox.com/robloxStudioStartedEvent";

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
