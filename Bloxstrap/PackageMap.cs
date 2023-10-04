using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap
{
    internal class PackageMap
    {
        public static IReadOnlyDictionary<string, string> Player
        {
            get { return CombineDictionaries(_common, _playerOnly); }
        }

        public static IReadOnlyDictionary<string, string> Studio
        {
            get { return CombineDictionaries(_common, _studioOnly); }
        }

        // in case a new package is added, you can find the corresponding directory
        // by opening the stock bootstrapper in a hex editor
        // TODO - there ideally should be a less static way to do this that's not hardcoded?
        private static IReadOnlyDictionary<string, string> _common = new Dictionary<string, string>()
        {
            { "Libraries.zip",                 @"" },
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

            { "extracontent-luapackages.zip",  @"ExtraContent\LuaPackages\" },
            { "extracontent-translations.zip", @"ExtraContent\translations\" },
            { "extracontent-models.zip",       @"ExtraContent\models\" },
            { "extracontent-textures.zip",     @"ExtraContent\textures\" },
            { "extracontent-places.zip",       @"ExtraContent\places\" },
        };

        private static IReadOnlyDictionary<string, string> _playerOnly = new Dictionary<string, string>()
        {
            { "RobloxApp.zip", @"" }
        };

        private static IReadOnlyDictionary<string, string> _studioOnly = new Dictionary<string, string>()
        {
            { "RobloxStudio.zip",                @"" },
            { "ApplicationConfig.zip",           @"ApplicationConfig\" },
            { "content-studio_svg_textures.zip", @"content\studio_svg_textures\"},
            { "content-qt_translations.zip",     @"content\qt_translations\" },
            { "content-api-docs.zip",            @"content\api_docs\" },
            { "extracontent-scripts.zip",        @"ExtraContent\scripts\" },
            { "BuiltInPlugins.zip",              @"BuiltInPlugins\" },
            { "BuiltInStandalonePlugins.zip",    @"BuiltInStandalonePlugins\" },
            { "LibrariesQt5.zip",                @"" },
            { "Plugins.zip",                     @"Plugins\" },
            { "Qml.zip",                         @"Qml\" },
            { "StudioFonts.zip",                 @"StudioFonts\" },
        };

        private static Dictionary<string, string> CombineDictionaries(IReadOnlyDictionary<string, string> d1, IReadOnlyDictionary<string, string> d2)
        {
            Dictionary<string, string> newD = new Dictionary<string, string>();

            foreach (var d in d1)
                newD[d.Key] = d.Value;

            foreach (var d in d2)
                newD[d.Key] = d.Value;

            return newD;
        }
    }
}
