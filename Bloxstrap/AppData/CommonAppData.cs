using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.AppData
{
    public abstract class CommonAppData
    {
        // in case a new package is added, you can find the corresponding directory
        // by opening the stock bootstrapper in a hex editor
        private IReadOnlyDictionary<string, string> _commonMap { get; } = new Dictionary<string, string>()
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

        public virtual IReadOnlyDictionary<string, string> PackageDirectoryMap { get; set; }

        public CommonAppData()
        {
            if (PackageDirectoryMap is null)
            {
                PackageDirectoryMap = _commonMap;
                return;
            }

            var merged = new Dictionary<string, string>();

            foreach (var entry in _commonMap)
                merged[entry.Key] = entry.Value;

            foreach (var entry in PackageDirectoryMap)
                merged[entry.Key] = entry.Value;

            PackageDirectoryMap = merged;
        }
    }
}
