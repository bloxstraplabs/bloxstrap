using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models.APIs.Config
{

    // those are the default directories
    // these should still be updated since remote data could fail to load
    public class PackageMaps
    {
        [JsonPropertyName("common")]
        public Dictionary<string, string> CommonPackageMap { get; set; } = new Dictionary<string, string>()
        {
            { "Libraries.zip",                 @"" },
            { "redist.zip",                    @"" },
            { "shaders.zip",                   @"shaders\" },
            { "ssl.zip",                       @"ssl\" },

            // the runtime installer is only extracted if it needs installing
            { "WebView2.zip",                  @"" },
            { "WebView2RuntimeInstaller.zip",  @"WebView2RuntimeInstaller\" },

            { "content-avatar.zip",            @"content\avatar\" },
            { "content-configs.zip",           @"content\configs\" },
            { "content-fonts.zip",             @"content\fonts\" },
            { "content-sky.zip",               @"content\sky\" },
            { "content-sounds.zip",            @"content\sounds\" },
            { "content-textures2.zip",         @"content\textures\" },
            { "content-models.zip",            @"content\models\" },

            { "content-textures3.zip",         @"PlatformContent\pc\textures\" },
            { "content-terrain.zip",           @"PlatformContent\pc\terrain\" },
            { "content-platform-fonts.zip",    @"PlatformContent\pc\fonts\" },
            { "content-platform-dictionaries.zip", @"PlatformContent\pc\shared_compression_dictionaries\" },

            { "extracontent-luapackages.zip",  @"ExtraContent\LuaPackages\" },
            { "extracontent-translations.zip", @"ExtraContent\translations\" },
            { "extracontent-models.zip",       @"ExtraContent\models\" },
            { "extracontent-textures.zip",     @"ExtraContent\textures\" },
            { "extracontent-places.zip",       @"ExtraContent\places\" },
        };


        [JsonPropertyName("player")]
        public Dictionary<string, string> PlayerPackageMap { get; set; } = new Dictionary<string, string>()
        {
            { "RobloxApp.zip", @"" }
        };

        [JsonPropertyName("studio")]
        public Dictionary<string, string> StudioPackageMap { get; set; } = new Dictionary<string, string>()
        {
            { "RobloxStudio.zip",                @"" },
            { "LibrariesQt5.zip",                @"" },

            { "content-studio_svg_textures.zip", @"content\studio_svg_textures\"},
            { "content-qt_translations.zip",     @"content\qt_translations\" },
            { "content-api-docs.zip",            @"content\api_docs\" },

            { "extracontent-scripts.zip",        @"ExtraContent\scripts\" },

            { "studiocontent-models.zip",        @"StudioContent\models\" },
            { "studiocontent-textures.zip",      @"StudioContent\textures\" },

            { "BuiltInPlugins.zip",              @"BuiltInPlugins\" },
            { "BuiltInStandalonePlugins.zip",    @"BuiltInStandalonePlugins\" },

            { "ApplicationConfig.zip",           @"ApplicationConfig\" },
            { "Plugins.zip",                     @"Plugins\" },
            { "Qml.zip",                         @"Qml\" },
            { "StudioFonts.zip",                 @"StudioFonts\" },
            { "RibbonConfig.zip",                @"RibbonConfig\" }
        };

        // allows us to index package maps with dictionary[key]
        public Dictionary<string, string> this[string key] =>
        key switch
        {
            "common" => CommonPackageMap,
            "player" => PlayerPackageMap,
            "studio" => StudioPackageMap,

            _ => null!
        };
    }
}