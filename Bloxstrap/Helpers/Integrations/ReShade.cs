using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Bloxstrap.Models;

using IniParser;
using IniParser.Model;
using System.Diagnostics;
using System.Xml.Linq;

namespace Bloxstrap.Helpers.Integrations
{
    internal class ReShade
    {
        // i havent even started this and i know for a fact this is gonna be a mess of an integration lol
        // there's a lot of nuances involved in how reshade functionality is supposed to work (shader management, config management, etc)
        // it's gonna be a bit of a pain in the ass, and i'm expecting a lot of bugs to arise from this...
        // well, looks like v1.7.0 is gonna be held back for quite a while lol

        // also, this is going to be fairly restrictive without a lot of heavy work
        // reshade's official installer gives you a list of shader packs and lets you choose which ones you want to install
        // and here we're effectively choosing for the user... hm...
        // i mean, it should be fine? importing shaders is still gonna be a thing, though maybe not as simple, but most people would be looking to use extravi's presets anyway

        // based on the shaders we have installed, we're gonna have to parse and adjust this... yay.............
        private static readonly string StockConfig =
            "[APP]\r\n" +
            "ForceFullscreen=0\r\n" +
            "ForceVsync=0\r\n" +
            "ForceWindowed=0\r\n" +
            "\r\n" +
            "[GENERAL]\r\n" +
            "EffectSearchPaths=..\\..\\ReShade\\Shaders\r\n" +
            "PreprocessorDefinitions=RESHADE_DEPTH_LINEARIZATION_FAR_PLANE=1000.0,RESHADE_DEPTH_INPUT_IS_UPSIDE_DOWN=0,RESHADE_DEPTH_INPUT_IS_REVERSED=1,RESHADE_DEPTH_INPUT_IS_LOGARITHMIC=0\r\n" +
            "PresetPath=..\\..\\ReShade\\Presets\\ReShadePreset.ini\r\n" +
            "TextureSearchPaths=..\\..\\ReShade\\Textures\r\n" +
            "\r\n" +
            "[INPUT]\r\n" +
            "GamepadNavigation=1\r\n" +
            "KeyOverlay=36,0,0,0\r\n";

        // this is a list of selectable shaders to download:
        // this should be formatted as { FolderName, GithubRepositoryUrl }
        private static readonly IReadOnlyDictionary<string, string> Shaders = new Dictionary<string, string>()
        {
            { "Stock",     "https://github.com/crosire/reshade-shaders/archive/refs/heads/master.zip" },

            // shaders required for extravi's presets:
            { "AlucardDH", "https://github.com/AlucardDH/dh-reshade-shaders/archive/refs/heads/master.zip" },
            { "AstrayFX",  "https://github.com/BlueSkyDefender/AstrayFX/archive/refs/heads/master.zip" },
            { "Depth3D",   "https://github.com/BlueSkyDefender/Depth3D/archive/refs/heads/master.zip" },
            { "Glamarye",  "https://github.com/rj200/Glamarye_Fast_Effects_for_ReShade/archive/refs/heads/main.zip" },
            { "NiceGuy",   "https://github.com/mj-ehsan/NiceGuy-Shaders/archive/refs/heads/main.zip" },
            { "prod80",    "https://github.com/prod80/prod80-ReShade-Repository/archive/refs/heads/master.zip" },
            { "qUINT",     "https://github.com/martymcmodding/qUINT/archive/refs/heads/master.zip" },
        };

        private static readonly string[] ExtraviPresetsShaders = new string[]
        {
            "AlucardDH",
            "AstrayFX",
            "Depth3D",
            "Glamarye",
            "NiceGuy",
            "prod80",
            "qUINT",
        };

        private static string GetSearchPath(string type, string name)
        {
            return $",..\\..\\ReShade\\{type}\\{name}";
        }

        public static void SynchronizeConfigFile()
        {
            Debug.WriteLine($"[ReShade] Synchronizing configuration file...");

            // yeah, this is going to be a bit of a pain
            // keep in mind the config file is going to be in two places: the mod folder and the version folder
            // so we have to make sure the two below scenaros work flawlessly:
            //  - if the user manually updates their reshade config in the mod folder, it must be copied to the version folder
            //  - if the user updates their reshade settings ingame, the updated config must be copied to the mod folder
            // the easiest way to manage this is to just compare the modification dates of the two
            // anyway, this is where i'm expecting most of the bugs to arise from
            // config synchronization will be done whenever roblox updates or whenever we launch roblox

            string modFolderConfigPath = Path.Combine(Directories.Modifications, "ReShade.ini");
            string versionFolderConfigPath = Path.Combine(Directories.Versions, Program.Settings.VersionGuid, "ReShade.ini");

            // we shouldn't be here if the mod config doesn't already exist
            if (!File.Exists(modFolderConfigPath))
            {
                Debug.WriteLine($"[ReShade] ReShade.ini in modifications folder does not exist, aborting sync");
                return;
            }

            // copy to the version folder if it doesn't already exist there
            if (!File.Exists(versionFolderConfigPath))
            {
                Debug.WriteLine($"[ReShade] ReShade.ini in version folder does not exist, synchronized with modifications folder");
                File.Copy(modFolderConfigPath, versionFolderConfigPath);
            }

            // if both the mod and version configs match, then we don't need to do anything
            if (Utilities.MD5File(modFolderConfigPath) == Utilities.MD5File(versionFolderConfigPath))
            {
                Debug.WriteLine($"[ReShade] ReShade.ini in version and modifications folder match");
                return;
            }

            FileInfo modFolderConfigFile = new(modFolderConfigPath);
            FileInfo versionFolderConfigFile = new(versionFolderConfigPath);

            if (modFolderConfigFile.LastWriteTime > versionFolderConfigFile.LastWriteTime)
            {
                // overwrite version config if mod config was modified most recently
                Debug.WriteLine($"[ReShade] ReShade.ini in version folder is older, synchronized with modifications folder");
                File.Copy(modFolderConfigPath, versionFolderConfigPath, true);
            }
            else if (versionFolderConfigFile.LastWriteTime > modFolderConfigFile.LastWriteTime)
            {
                // overwrite mod config if version config was modified most recently
                Debug.WriteLine($"[ReShade] ReShade.ini in modifications folder is older, synchronized with version folder");
                File.Copy(versionFolderConfigPath, modFolderConfigPath, true);
            }
        }

        public static async Task DownloadShaders(string name)
        {
            string downloadUrl = Shaders.First(x => x.Key == name).Value;

            // not all shader packs have a textures folder, so here we're determining if they exist purely based on if they have a Shaders folder
            if (Directory.Exists(Path.Combine(Directories.ReShade, "Shaders", name)))
                return;

            Debug.WriteLine($"[ReShade] Downloading shaders for {name}");

            byte[] bytes = await Program.HttpClient.GetByteArrayAsync(downloadUrl);

            using (MemoryStream zipStream = new(bytes))
            {
                using (ZipArchive archive = new(zipStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith('/'))
                            continue;

                        // github branch zips have a root folder of the name of the branch, so let's just remove that
                        string fullPath = entry.FullName.Substring(entry.FullName.IndexOf('/') + 1);

                        // skip file if it's not in the Shaders or Textures folder
                        if (!fullPath.StartsWith("Shaders") && !fullPath.StartsWith("Textures"))
                            continue;

                        // and now we do it again because of how we're handling folder management
                        // e.g. reshade-shaders-master/Shaders/Vignette.fx should go to ReShade/Shaders/Stock/Vignette.fx
                        // so in this case, relativePath should just be "Vignette.fx"
                        string relativePath = fullPath.Substring(fullPath.IndexOf('/') + 1);

                        // now we stitch it all together
                        string extractionPath = Path.Combine(
                            Directories.ReShade,
                            fullPath.StartsWith("Shaders") ? "Shaders" : "Textures",
                            name,
                            relativePath
                        );

                        // make sure the folder that we're extracting it to exists
                        Directory.CreateDirectory(Path.GetDirectoryName(extractionPath)!);

                        // and now extract
                        await Task.Run(() => entry.ExtractToFile(extractionPath));
                    }
                }
            }

            // now we have to update ReShade.ini and add the installed shaders to the search paths
            FileIniDataParser parser = new();
            IniData data = parser.ReadFile(Path.Combine(Directories.Modifications, "ReShade.ini"));

            if (!data["GENERAL"]["EffectSearchPaths"].Contains(name))
                data["GENERAL"]["EffectSearchPaths"] += GetSearchPath("Shaders", name);

            // not every shader pack has a textures folder
            if (Directory.Exists(Path.Combine(Directories.ReShade, "Textures", name)) && !data["GENERAL"]["TextureSearchPaths"].Contains(name))
                data["GENERAL"]["TextureSearchPaths"] += GetSearchPath("Textures", name);

            parser.WriteFile(Path.Combine(Directories.Modifications, "ReShade.ini"), data);
        }

        public static void DeleteShaders(string name)
        {
            Debug.WriteLine($"[ReShade] Deleting shaders for {name}");

            string shadersPath = Path.Combine(Directories.ReShade, "Shaders", name);
            string texturesPath = Path.Combine(Directories.ReShade, "Textures", name);

            if (Directory.Exists(shadersPath))
                Directory.Delete(shadersPath, true);

            if (Directory.Exists(texturesPath))
                Directory.Delete(texturesPath, true);

            string configFile = Path.Combine(Directories.Modifications, "ReShade.ini");

            if (!File.Exists(configFile))
                return;

            // now we have to update ReShade.ini and remove the installed shaders from the search paths
            FileIniDataParser parser = new();
            IniData data = parser.ReadFile(configFile);

            string configShaderSearchPaths = data["GENERAL"]["EffectSearchPaths"];
            string configTextureSearchPaths = data["GENERAL"]["TextureSearchPaths"];

            if (configShaderSearchPaths.Contains(name))
            {
                string searchPath = GetSearchPath("Shaders", name);
                data["GENERAL"]["EffectSearchPaths"] = configShaderSearchPaths.Remove(configShaderSearchPaths.IndexOf(searchPath), searchPath.Length);
            }

            if (configTextureSearchPaths.Contains(name))
            {
                string searchPath = GetSearchPath("Textures", name);
                data["GENERAL"]["TextureSearchPaths"] = configTextureSearchPaths.Remove(configTextureSearchPaths.IndexOf(searchPath), searchPath.Length);
            }

            parser.WriteFile(configFile, data);
        }

        public static async Task InstallExtraviPresets()
        {
            Debug.WriteLine("[ReShade] Installing Extravi's presets...");

            foreach (string name in ExtraviPresetsShaders)
                await DownloadShaders(name);

            int count = new DirectoryInfo(Path.Combine(Directories.ReShade, "Presets")).GetFiles().Where(x => x.Name.StartsWith("Extravi")).Count();

            // there should be at least 7 presets beginning with "Extravi", if there aren't then presume they're not installed
            if (count >= 7)
            {
                Debug.WriteLine("[ReShade] Extravi's presets are already installed, aborting");
                return;
            }

            // we're also gonna need some sort of versioning for this somehow so that extravi can update the presets ota
            byte[] bytes = await Program.HttpClient.GetByteArrayAsync("https://github.com/Extravi/extravi.github.io/raw/main/update/reshade-presets.zip");

            using (MemoryStream zipStream = new(bytes))
            {
                using (ZipArchive archive = new(zipStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith('/'))
                            continue;

                        // github branch zips have a root folder of the name of the branch, so let's just remove that
                        string filename = entry.FullName.Substring(entry.FullName.IndexOf('/') + 1);

                        await Task.Run(() => entry.ExtractToFile(Path.Combine(Directories.ReShade, "Presets", filename), true));
                    }
                }
            }
        }

        public static void UninstallExtraviPresets()
        {
            Debug.WriteLine("[ReShade] Uninstalling Extravi's presets...");

            FileInfo[] presets = new DirectoryInfo(Path.Combine(Directories.ReShade, "Presets")).GetFiles();

            foreach (FileInfo preset in presets)
            {
                if (preset.Name.StartsWith("Extravi"))
                    preset.Delete();
            }

            foreach (string name in ExtraviPresetsShaders)
                DeleteShaders(name);
        }

        public static async Task CheckModifications()
        {
            Debug.WriteLine("[ReShade] Checking ReShade modifications... ");
            
            string injectorLocation = Path.Combine(Directories.Modifications, "dxgi.dll");
            string configLocation = Path.Combine(Directories.Modifications, "ReShade.ini");

            // initialize directories
            Directory.CreateDirectory(Directories.ReShade);
            Directory.CreateDirectory(Path.Combine(Directories.ReShade, "Shaders"));
            Directory.CreateDirectory(Path.Combine(Directories.ReShade, "Textures"));
            Directory.CreateDirectory(Path.Combine(Directories.ReShade, "Presets"));

            if (!Program.Settings.UseReShadeExtraviPresets)
                UninstallExtraviPresets();

            if (!Program.Settings.UseReShade)
            {
                Debug.WriteLine("[ReShade] Uninstalling ReShade...");

                // delete any stock config files

                if (File.Exists(injectorLocation))
                    File.Delete(injectorLocation);

                if (File.Exists(configLocation))
                    File.Delete(configLocation);

                DeleteShaders("Stock");

                return;
            }

            // first, let's check to make sure the injector dll is downloaded
            if (!File.Exists(injectorLocation))
            {
                Debug.WriteLine("[ReShade] Installing ReShade...");

                // here we're downloading the dll through extravi's repository as reshade doesn't officially distribute binaries
                // uhhh... i'm not sure how we're gonna handle checking for updates? there's not exactly a version number to check here...
                // only way i can think of is to check the latest commit to the file but thats messy and requires the github api fsdkjhgusedjfzlikohskeolgdfazszwhs\aripy\aws;j/riows\ajuygh
                // i think i might have to (or get extravi to) host the binary version somewhere
                byte[] bytes = await Program.HttpClient.GetByteArrayAsync("https://github.com/Extravi/extravi.github.io/raw/main/update/dxgi.zip");

                using (MemoryStream zipStream = new(bytes))
                {
                    using (ZipArchive zip = new(zipStream))
                    {
                        zip.ExtractToDirectory(Directories.Modifications, true);
                    }
                }
            }

            // and write the stock config if we need to
            if (!File.Exists(configLocation))
                await File.WriteAllTextAsync(configLocation, StockConfig);

            await DownloadShaders("Stock");

            if (Program.Settings.UseReShadeExtraviPresets)
                await InstallExtraviPresets();

            SynchronizeConfigFile();
        }
    }
}
