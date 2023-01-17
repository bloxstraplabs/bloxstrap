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
        #region Config
        private static readonly string StockConfig =
            "[APP]\r\n" +
            "ForceFullscreen=0\r\n" +
            "ForceVsync=0\r\n" +
            "ForceWindowed=0\r\n" +
            "\r\n" +
            "[GENERAL]\r\n" +
            "EffectSearchPaths=..\\..\\ReShade\\Shaders\r\n" +
            "PerformanceMode=1\r\n" +
            "PreprocessorDefinitions=RESHADE_DEPTH_LINEARIZATION_FAR_PLANE=1000.0,RESHADE_DEPTH_INPUT_IS_UPSIDE_DOWN=0,RESHADE_DEPTH_INPUT_IS_REVERSED=1,RESHADE_DEPTH_INPUT_IS_LOGARITHMIC=0\r\n" +
            "PresetPath=..\\..\\ReShade\\Presets\\ReShadePreset.ini\r\n" +
            "TextureSearchPaths=..\\..\\ReShade\\Textures\r\n" +
            "\r\n" +
            "[INPUT]\r\n" +
            "ForceShortcutModifiers=1\r\n" +
            "InputProcessing=2\r\n" +
            "GamepadNavigation=1\r\n" +
            "KeyEffects=117,0,1,0\r\n" +
            "KeyNextPreset=0,0,0,0\r\n" +
            "KeyOverlay=9,0,1,0\r\n" +
            "KeyPerformanceMode=0,0,0,0\r\n" +
            "KeyPreviousPreset=0,0,0,0\r\n" +
            "KeyReload=0,0,0,0\r\n" +
            "KeyScreenshot=44,0,0,0\r\n" +
            "\r\n" +
            "[SCREENSHOT]\r\n" +
            "SavePath=..\\..\\ReShade\\Screenshots\r\n" +
            "\r\n" +
            "[STYLE]\r\n" +
            "Alpha=1.000000\r\n" +
            "Border=0.862745,0.862745,0.862745,0.300000\r\n" +
            "BorderShadow=0.000000,0.000000,0.000000,0.000000\r\n" +
            "Button=0.156863,0.313726,0.941177,0.440000\r\n" +
            "ButtonActive=0.156863,0.313726,0.941177,1.000000\r\n" +
            "ButtonHovered=0.156863,0.313726,0.941177,0.860000\r\n" +
            "CheckMark=0.156863,0.313726,0.941177,0.800000\r\n" +
            "ChildBg=0.109804,0.109804,0.109804,0.000000\r\n" +
            "ChildRounding=6.000000\r\n" +
            "ColFPSText=1.000000,1.000000,0.784314,1.000000\r\n" +
            "DockingEmptyBg=0.200000,0.200000,0.200000,1.000000\r\n" +
            "DockingPreview=0.156863,0.313726,0.941177,0.532000\r\n" +
            "DragDropTarget=1.000000,1.000000,0.000000,0.900000\r\n" +
            "EditorFont=..\\..\\ReShade\\Fonts\\Hack-Regular.ttf\r\n" +
            "EditorFontSize=18\r\n" +
            "EditorStyleIndex=0\r\n" +
            "Font=..\\..\\ReShade\\Fonts\\NunitoSans-Regular.ttf\r\n" +
            "FontSize=18\r\n" +
            "FPSScale=1.000000\r\n" +
            "FrameBg=0.109804,0.109804,0.109804,1.000000\r\n" +
            "FrameBgActive=0.156863,0.313726,0.941177,1.000000\r\n" +
            "FrameBgHovered=0.156863,0.313726,0.941177,0.680000\r\n" +
            "FrameRounding=6.000000\r\n" +
            "GrabRounding=6.000000\r\n" +
            "Header=0.156863,0.313726,0.941177,0.760000\r\n" +
            "HeaderActive=0.156863,0.313726,0.941177,1.000000\r\n" +
            "HeaderHovered=0.156863,0.313726,0.941177,0.860000\r\n" +
            "MenuBarBg=0.109804,0.109804,0.109804,0.570000\r\n" +
            "ModalWindowDimBg=0.800000,0.800000,0.800000,0.350000\r\n" +
            "NavHighlight=0.260000,0.590000,0.980000,1.000000\r\n" +
            "NavWindowingDimBg=0.800000,0.800000,0.800000,0.200000\r\n" +
            "NavWindowingHighlight=1.000000,1.000000,1.000000,0.700000\r\n" +
            "PlotHistogram=0.862745,0.862745,0.862745,0.630000\r\n" +
            "PlotHistogramHovered=0.156863,0.313726,0.941177,1.000000\r\n" +
            "PlotLines=0.862745,0.862745,0.862745,0.630000\r\n" +
            "PlotLinesHovered=0.156863,0.313726,0.941177,1.000000\r\n" +
            "PopupBg=0.047059,0.047059,0.047059,0.920000\r\n" +
            "PopupRounding=6.000000\r\n" +
            "ResizeGrip=0.156863,0.313726,0.941177,0.200000\r\n" +
            "ResizeGripActive=0.156863,0.313726,0.941177,1.000000\r\n" +
            "ResizeGripHovered=0.156863,0.313726,0.941177,0.780000\r\n" +
            "ScrollbarBg=0.109804,0.109804,0.109804,1.000000\r\n" +
            "ScrollbarGrab=0.156863,0.313726,0.941177,0.310000\r\n" +
            "ScrollbarGrabActive=0.156863,0.313726,0.941177,1.000000\r\n" +
            "ScrollbarGrabHovered=0.156863,0.313726,0.941177,0.780000\r\n" +
            "ScrollbarRounding=6.000000\r\n" +
            "Separator=0.862745,0.862745,0.862745,0.320000\r\n" +
            "SeparatorActive=0.862745,0.862745,0.862745,1.000000\r\n" +
            "SeparatorHovered=0.862745,0.862745,0.862745,0.780000\r\n" +
            "SliderGrab=0.156863,0.313726,0.941177,0.240000\r\n" +
            "SliderGrabActive=0.156863,0.313726,0.941177,1.000000\r\n" +
            "StyleIndex=3\r\n" +
            "Tab=0.156863,0.313726,0.941177,0.440000\r\n" +
            "TabActive=0.156863,0.313726,0.941177,1.000000\r\n" +
            "TabHovered=0.156863,0.313726,0.941177,0.860000\r\n" +
            "TableBorderLight=0.230000,0.230000,0.250000,1.000000\r\n" +
            "TableBorderStrong=0.310000,0.310000,0.350000,1.000000\r\n" +
            "TableHeaderBg=0.190000,0.190000,0.200000,1.000000\r\n" +
            "TableRowBg=0.000000,0.000000,0.000000,0.000000\r\n" +
            "TableRowBgAlt=1.000000,1.000000,1.000000,0.060000\r\n" +
            "TabRounding=6.000000\r\n" +
            "TabUnfocused=0.156863,0.313726,0.941177,0.448000\r\n" +
            "TabUnfocusedActive=0.156863,0.313726,0.941177,0.780000\r\n" +
            "Text=0.862745,0.862745,0.862745,1.000000\r\n" +
            "TextDisabled=0.862745,0.862745,0.862745,0.580000\r\n" +
            "TextSelectedBg=0.156863,0.313726,0.941177,0.430000\r\n" +
            "TitleBg=0.156863,0.313726,0.941177,0.450000\r\n" +
            "TitleBgActive=0.156863,0.313726,0.941177,0.580000\r\n" +
            "TitleBgCollapsed=0.156863,0.313726,0.941177,0.350000\r\n" +
            "WindowBg=0.047059,0.047059,0.047059,1.000000\r\n" +
            "WindowRounding=6.000000";
        #endregion

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

            {
                byte[] bytes = await Program.HttpClient.GetByteArrayAsync(downloadUrl);

                using MemoryStream zipStream = new(bytes);
                using ZipArchive archive = new(zipStream);

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith('/'))
                        continue;

                    // github branch zips have a root folder of the name of the branch, so let's just remove that
                    string fullPath = entry.FullName.Substring(entry.FullName.IndexOf('/') + 1);

                    // skip file if it's not in the Shaders or Textures folder
                    if (!fullPath.StartsWith("Shaders") && !fullPath.StartsWith("Textures"))
                        continue;

                    // ingore shaders with compiler errors
                    if (fullPath.EndsWith("dh_Lain.fx") || fullPath.EndsWith("dh_rtgi.fx"))
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

            byte[] bytes = await Program.HttpClient.GetByteArrayAsync("https://github.com/Extravi/extravi.github.io/raw/main/update/reshade-presets.zip");

            using MemoryStream zipStream = new(bytes);
            using ZipArchive archive = new(zipStream);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith('/'))
                    continue;

                // remove containing folder
                string filename = entry.FullName.Substring(entry.FullName.IndexOf('/') + 1);

                await Task.Run(() => entry.ExtractToFile(Path.Combine(Directories.ReShade, "Presets", filename), true));
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
            Directory.CreateDirectory(Path.Combine(Directories.ReShade, "Fonts"));
            Directory.CreateDirectory(Path.Combine(Directories.ReShade, "Screenshots"));
            Directory.CreateDirectory(Path.Combine(Directories.ReShade, "Shaders"));
            Directory.CreateDirectory(Path.Combine(Directories.ReShade, "Textures"));
            Directory.CreateDirectory(Path.Combine(Directories.ReShade, "Presets"));

            if (!Program.Settings.UseReShadeExtraviPresets)
            {
                UninstallExtraviPresets();
                Program.Settings.ExtraviPresetsVersion = "";
            }

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

            // the version manfiest contains the version of reshade available for download and the last date the presets were updated
            var versionManifest = await Utilities.GetJson<ReShadeVersionManifest>("https://raw.githubusercontent.com/Extravi/extravi.github.io/main/update/version.json");
            bool shouldFetchReShade = false;

            if (!File.Exists(injectorLocation))
            {
                shouldFetchReShade = true;
            }
            else if (versionManifest is not null)
            {
                // check if an update for reshade is available
                FileVersionInfo injectorVersionInfo = FileVersionInfo.GetVersionInfo(injectorLocation);

                if (injectorVersionInfo.ProductVersion != versionManifest.ReShade)
                    shouldFetchReShade = true;
            }

            if (shouldFetchReShade)
            {
                Debug.WriteLine("[ReShade] Installing ReShade...");

                {
                    byte[] bytes = await Program.HttpClient.GetByteArrayAsync("https://github.com/Extravi/extravi.github.io/raw/main/update/dxgi.zip");
                    using MemoryStream zipStream = new(bytes);
                    using ZipArchive archive = new(zipStream);
                    archive.ExtractToDirectory(Directories.Modifications, true);
                }

                // we also gotta download the editor fonts
                if (Utilities.IsDirectoryEmpty(Path.Combine(Directories.ReShade, "Fonts")))
                {
                    byte[] bytes = await Program.HttpClient.GetByteArrayAsync("https://github.com/Extravi/extravi.github.io/raw/main/update/config.zip");

                    using MemoryStream zipStream = new(bytes);
                    using ZipArchive archive = new(zipStream);

                    foreach (ZipArchiveEntry entry in archive.Entries.Where(x => x.FullName.EndsWith(".ttf")))
                        entry.ExtractToFile(Path.Combine(Directories.ReShade, "Fonts", entry.FullName));
                }
            }

            // and write the stock config if we need to
            if (!File.Exists(configLocation))
                await File.WriteAllTextAsync(configLocation, StockConfig);

            await DownloadShaders("Stock");

            if (Program.Settings.UseReShadeExtraviPresets && Program.Settings.ExtraviPresetsVersion != versionManifest!.Presets)
            {
                await InstallExtraviPresets();
                Program.Settings.ExtraviPresetsVersion = versionManifest.Presets;
            }

            SynchronizeConfigFile();
        }
    }
}
