using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using Bloxstrap.Models;

using IniParser;
using IniParser.Model;

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

        private static string BaseDirectory => Path.Combine(Directories.Integrations, "ReShade");
        private static string FontsFolder => Path.Combine(BaseDirectory, "Fonts");
        private static string PresetsFolder => Path.Combine(BaseDirectory, "Presets");
        private static string ShadersFolder => Path.Combine(BaseDirectory, "Shaders");
        private static string TexturesFolder => Path.Combine(BaseDirectory, "Textures");
        private static string ConfigLocation => Path.Combine(Directories.Modifications, "ReShade.ini");

        // the base url that we're fetching all our remote configs and resources and stuff from
        private const string BaseUrl = "https://raw.githubusercontent.com/Extravi/extravi.github.io/main/update";

        // this is a list of selectable shaders to download
        private static readonly List<ReShadeShaderConfig> Shaders = new()
        {
            new ReShadeShaderConfig { Name = "Stock", DownloadLocation = "https://github.com/crosire/reshade-shaders/archive/refs/heads/master.zip" },

            // shaders required for extravi's presets:
            new ReShadeShaderConfig { Name = "AstrayFX", DownloadLocation = "https://github.com/BlueSkyDefender/AstrayFX/archive/refs/heads/master.zip" },
            new ReShadeShaderConfig { Name = "Brussell", DownloadLocation = "https://github.com/brussell1/Shaders/archive/refs/heads/master.zip" },
			new ReShadeShaderConfig { Name = "Depth3D", DownloadLocation = "https://github.com/BlueSkyDefender/Depth3D/archive/refs/heads/master.zip" },
            new ReShadeShaderConfig { Name = "Glamarye", DownloadLocation = "https://github.com/rj200/Glamarye_Fast_Effects_for_ReShade/archive/refs/heads/main.zip" },
            new ReShadeShaderConfig { Name = "NiceGuy", DownloadLocation = "https://github.com/mj-ehsan/NiceGuy-Shaders/archive/refs/heads/main.zip" },
            new ReShadeShaderConfig { Name = "prod80", DownloadLocation = "https://github.com/prod80/prod80-ReShade-Repository/archive/refs/heads/master.zip" },
            new ReShadeShaderConfig { Name = "qUINT", DownloadLocation = "https://github.com/martymcmodding/qUINT/archive/refs/heads/master.zip" },
            new ReShadeShaderConfig { Name = "StockLegacy", DownloadLocation = "https://github.com/crosire/reshade-shaders/archive/refs/heads/legacy.zip" },
			new ReShadeShaderConfig { Name = "SweetFX", DownloadLocation = "https://github.com/CeeJayDK/SweetFX/archive/refs/heads/master.zip" },

            // these ones needs some additional configuration

            new ReShadeShaderConfig 
            { 
                Name = "AlucardDH", 
                DownloadLocation = "https://github.com/AlucardDH/dh-reshade-shaders/archive/refs/heads/master.zip",
                ExcludedFiles = new List<string>()
                {
                    // compiler error
                    "Shaders/dh_rtgi.fx"
                }
            },

			new ReShadeShaderConfig 
            { 
                Name = "Stormshade", 
                DownloadLocation = "https://github.com/cyrie/Stormshade/archive/refs/heads/master.zip", 
                BaseFolder = "reshade-shaders/",
                ExcludedFiles = new List<string>()
                {
                    // these file names conflict with effects in the stock reshade config
                    "Shaders/AmbientLight.fx",
					"Shaders/Clarity.fx",
					"Shaders/DOF.fx",
					"Shaders/DPX.fx",
					"Shaders/FilmGrain.fx",
					"Shaders/FineSharp.fx",
					"Shaders/FXAA.fx",
					"Shaders/FXAA.fxh",
					"Shaders/LumaSharpen.fx",
					"Shaders/MXAO.fx",
					"Shaders/ReShade.fxh",
					"Shaders/Vibrance.fx",
					"Shaders/Vignette.fx"
				}
            },
		};

        private static readonly string[] ExtraviPresetsShaders = new string[]
        {
            "AlucardDH",
            "Brussell",
            "AstrayFX",
            "Brussell",
			"Depth3D",
            "Glamarye",
            "NiceGuy",
            "prod80",
            "qUINT",
            "StockLegacy",
			"Stormshade",
			"SweetFX",
        };

        private static string GetSearchPath(string type, string name)
        {
            return $",..\\..\\Integrations\\ReShade\\{type}\\{name}";
        }

        public static async Task DownloadConfig()
        {
            App.Logger.WriteLine("[ReShade::DownloadConfig] Downloading/Upgrading config file...");

            {
                byte[] bytes = await App.HttpClient.GetByteArrayAsync($"{BaseUrl}/config.zip");

                using MemoryStream zipStream = new(bytes);
                using ZipArchive archive = new(zipStream);


                archive.Entries.First(x => x.FullName == "ReShade.ini").ExtractToFile(ConfigLocation, true);

                // when we extract the file we have to make sure the last modified date is overwritten
                // or else it will synchronize with the config in the version folder
                // really the config adjustments below should do this for us, but this is just to be safe
                File.SetLastWriteTime(ConfigLocation, DateTime.Now);

                // we also gotta download the editor fonts
                foreach (ZipArchiveEntry entry in archive.Entries.Where(x => x.FullName.EndsWith(".ttf")))
                    entry.ExtractToFile(Path.Combine(FontsFolder, entry.FullName), true);
            }

            // now we have to adjust the config file to use the paths that we need
            // some of these can be removed later when the config file is better adjusted for bloxstrap by default

            FileIniDataParser parser = new();
            IniData data = parser.ReadFile(ConfigLocation);

            data["GENERAL"]["EffectSearchPaths"] = "..\\..\\Integrations\\ReShade\\Shaders";
            data["GENERAL"]["TextureSearchPaths"] = "..\\..\\Integrations\\ReShade\\Textures";
            data["GENERAL"]["PresetPath"] = data["GENERAL"]["PresetPath"].Replace(".\\reshade-presets\\", "..\\..\\Integrations\\ReShade\\Presets\\");
            //data["SCREENSHOT"]["SavePath"] = "..\\..\\ReShade\\Screenshots";
            data["SCREENSHOT"]["SavePath"] = Path.Combine(Directories.MyPictures, "Roblox-ReShade");
            data["STYLE"]["EditorFont"] = data["STYLE"]["EditorFont"].Replace(".\\", "..\\..\\Integrations\\ReShade\\Fonts\\");
            data["STYLE"]["Font"] = data["STYLE"]["Font"].Replace(".\\", "..\\..\\Integrations\\ReShade\\Fonts\\");

            // add search paths for shaders and textures

            foreach (string name in Directory.GetDirectories(ShadersFolder).Select(x => Path.GetRelativePath(ShadersFolder, x)).ToArray())
                data["GENERAL"]["EffectSearchPaths"] += GetSearchPath("Shaders", name);

            foreach (string name in Directory.GetDirectories(TexturesFolder).Select(x => Path.GetRelativePath(TexturesFolder, x)).ToArray())
                data["GENERAL"]["TextureSearchPaths"] += GetSearchPath("Textures", name);

            parser.WriteFile(ConfigLocation, data);
        }

        public static void SynchronizeConfigFile()
        {
            App.Logger.WriteLine($"[ReShade::SynchronizeConfigFile] Synchronizing configuration file...");

            // yeah, this is going to be a bit of a pain
            // keep in mind the config file is going to be in two places: the mod folder and the version folder
            // so we have to make sure the two below scenaros work flawlessly:
            //  - if the user manually updates their reshade config in the mod folder or it gets updated, it must be copied to the version folder
            //  - if the user updates their reshade settings ingame, the updated config must be copied to the mod folder
            // the easiest way to manage this is to just compare the modification dates of the two
            // anyway, this is where i'm expecting most of the bugs to arise from
            // config synchronization will be done whenever roblox updates or whenever we launch roblox

            string modFolderConfigPath = ConfigLocation;
            string versionFolderConfigPath = Path.Combine(Directories.Versions, App.State.Prop.VersionGuid, "ReShade.ini");

            // we shouldn't be here if the mod config doesn't already exist
            if (!File.Exists(modFolderConfigPath))
            {
                App.Logger.WriteLine($"[ReShade::SynchronizeConfigFile] ReShade.ini in modifications folder does not exist, aborting sync");
                return;
            }

            // copy to the version folder if it doesn't already exist there
            if (!File.Exists(versionFolderConfigPath))
            {
                App.Logger.WriteLine($"[ReShade::SynchronizeConfigFile] ReShade.ini in version folder does not exist, synchronized with modifications folder");
                File.Copy(modFolderConfigPath, versionFolderConfigPath);
            }

            // if both the mod and version configs match, then we don't need to do anything
            if (Utilities.MD5File(modFolderConfigPath) == Utilities.MD5File(versionFolderConfigPath))
            {
                App.Logger.WriteLine($"[ReShade::SynchronizeConfigFile] ReShade.ini in version and modifications folder match");
                return;
            }

            FileInfo modFolderConfigFile = new(modFolderConfigPath);
            FileInfo versionFolderConfigFile = new(versionFolderConfigPath);

            if (modFolderConfigFile.LastWriteTime > versionFolderConfigFile.LastWriteTime)
            {
                // overwrite version config if mod config was modified most recently
                App.Logger.WriteLine($"[ReShade::SynchronizeConfigFile] ReShade.ini in version folder is older, synchronized with modifications folder");
                File.Copy(modFolderConfigPath, versionFolderConfigPath, true);
            }
            else if (versionFolderConfigFile.LastWriteTime > modFolderConfigFile.LastWriteTime)
            {
                // overwrite mod config if version config was modified most recently
                App.Logger.WriteLine($"[ReShade::SynchronizeConfigFile] ReShade.ini in modifications folder is older, synchronized with version folder");
                File.Copy(versionFolderConfigPath, modFolderConfigPath, true);
            }
        }

        public static async Task DownloadShaders(string name)
        {
            ReShadeShaderConfig config = Shaders.First(x => x.Name == name);

            // not all shader packs have a textures folder, so here we're determining if they exist purely based on if they have a Shaders folder
            if (Directory.Exists(Path.Combine(ShadersFolder, name)))
                return;

            App.Logger.WriteLine($"[ReShade::DownloadShaders] Downloading shaders for {name}");

            {
                byte[] bytes = await App.HttpClient.GetByteArrayAsync(config.DownloadLocation);

                using MemoryStream zipStream = new(bytes);
                using ZipArchive archive = new(zipStream);

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith('/') || !entry.FullName.Contains(config.BaseFolder))
                        continue;

                    string fullPath = entry.FullName.Substring(entry.FullName.IndexOf(config.BaseFolder) + config.BaseFolder.Length);

                    // skip file if it's not in the Shaders or Textures folder
                    if (!fullPath.StartsWith("Shaders") && !fullPath.StartsWith("Textures"))
                        continue;

                    if (config.ExcludedFiles.Contains(fullPath))
                        continue;

                    // and now we do it again because of how we're handling folder management
                    // e.g. reshade-shaders-master/Shaders/Vignette.fx should go to ReShade/Shaders/Stock/Vignette.fx
                    // so in this case, relativePath should just be "Vignette.fx"
                    string relativePath = fullPath.Substring(fullPath.IndexOf('/') + 1);

                    // now we stitch it all together
                    string extractionPath = Path.Combine(
                        BaseDirectory,
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
            IniData data = parser.ReadFile(ConfigLocation);

            if (!data["GENERAL"]["EffectSearchPaths"].Contains(name))
                data["GENERAL"]["EffectSearchPaths"] += GetSearchPath("Shaders", name);

            // not every shader pack has a textures folder
            if (Directory.Exists(Path.Combine(TexturesFolder, name)) && !data["GENERAL"]["TextureSearchPaths"].Contains(name))
                data["GENERAL"]["TextureSearchPaths"] += GetSearchPath("Textures", name);

            parser.WriteFile(ConfigLocation, data);
        }

        public static void DeleteShaders(string name)
        {
            App.Logger.WriteLine($"[ReShade::DeleteShaders] Deleting shaders for {name}");

            string shadersPath = Path.Combine(ShadersFolder, name);
            string texturesPath = Path.Combine(TexturesFolder, name);

            if (Directory.Exists(shadersPath))
                Directory.Delete(shadersPath, true);

            if (Directory.Exists(texturesPath))
                Directory.Delete(texturesPath, true);

            if (!File.Exists(ConfigLocation))
                return;

            // now we have to update ReShade.ini and remove the installed shaders from the search paths
            FileIniDataParser parser = new();
            IniData data = parser.ReadFile(ConfigLocation);

            string shaderSearchPaths = data["GENERAL"]["EffectSearchPaths"];
            string textureSearchPaths = data["GENERAL"]["TextureSearchPaths"];

            if (shaderSearchPaths.Contains(name))
            {
                string searchPath = GetSearchPath("Shaders", name);
                data["GENERAL"]["EffectSearchPaths"] = shaderSearchPaths.Remove(shaderSearchPaths.IndexOf(searchPath), searchPath.Length);
            }

            if (textureSearchPaths.Contains(name))
            {
                string searchPath = GetSearchPath("Textures", name);
                data["GENERAL"]["TextureSearchPaths"] = textureSearchPaths.Remove(textureSearchPaths.IndexOf(searchPath), searchPath.Length);
            }

            parser.WriteFile(ConfigLocation, data);
        }

        public static async Task InstallExtraviPresets()
        {
            App.Logger.WriteLine("[ReShade::InstallExtraviPresets] Installing Extravi's presets...");

            foreach (string name in ExtraviPresetsShaders)
                await DownloadShaders(name);

            byte[] bytes = await App.HttpClient.GetByteArrayAsync($"{BaseUrl}/reshade-presets.zip");

            using MemoryStream zipStream = new(bytes);
            using ZipArchive archive = new(zipStream);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith('/'))
                    continue;

                // remove containing folder
                string filename = entry.FullName.Substring(entry.FullName.IndexOf('/') + 1);

                await Task.Run(() => entry.ExtractToFile(Path.Combine(PresetsFolder, filename), true));
            }
        }

        public static void UninstallExtraviPresets()
        {
            if (!Directory.Exists(PresetsFolder))
                return;

            App.Logger.WriteLine("[ReShade::UninstallExtraviPresets] Uninstalling Extravi's ReShade presets...");

            FileInfo[] presets = new DirectoryInfo(PresetsFolder).GetFiles();

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
            App.Logger.WriteLine("[ReShade::CheckModifications] Checking ReShade modifications...");

            string injectorLocation = Path.Combine(Directories.Modifications, "dxgi.dll");

            if (!App.Settings.Prop.UseReShadeExtraviPresets && !String.IsNullOrEmpty(App.State.Prop.ExtraviReShadePresetsVersion))
            {
                if (Utilities.GetProcessCount("RobloxPlayerBeta") > 0)
                    return;

                UninstallExtraviPresets();

                App.State.Prop.ExtraviReShadePresetsVersion = "";
                App.State.Save();
            }

            if (!App.Settings.Prop.UseReShade)
            {
                if (Utilities.GetProcessCount("RobloxPlayerBeta") > 0)
                    return;

                App.Logger.WriteLine("[ReShade::CheckModifications] ReShade is not enabled");

                // we should already be uninstalled
                // we want to ensure this is done one-time only as this could possibly interfere with other rendering hooks using dxgi.dll
                if (String.IsNullOrEmpty(App.State.Prop.ReShadeConfigVersion))
                    return;

                App.Logger.WriteLine("[ReShade::CheckModifications] Uninstalling ReShade...");

                // delete any stock config files
                File.Delete(injectorLocation);
                File.Delete(ConfigLocation);

                if (Directory.Exists(BaseDirectory))
                    Directory.Delete(BaseDirectory, true);

                App.State.Prop.ReShadeConfigVersion = "";
                App.State.Save();

                return;
            }

            // the version manfiest contains the version of reshade available for download and the last date the presets were updated
            var versionManifest = await Utilities.GetJson<ReShadeVersionManifest>("https://raw.githubusercontent.com/Extravi/extravi.github.io/main/update/version.json");
            bool shouldFetchReShade = false;
            bool shouldFetchConfig = false;

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

				// UPDATE CHECK - if we're upgrading to reshade 5.7.0, or we have extravi's presets
                // enabled with a known shader downloaded (like AlucardDH) but without stormshade downloaded (5.7.0+ specific),
                // we need to redownload all our shaders fresh
				if (
                    injectorVersionInfo.ProductVersion != versionManifest.ReShade && versionManifest.ReShade == "5.7.0" || 
                    App.Settings.Prop.UseReShadeExtraviPresets && Directory.Exists(Path.Combine(ShadersFolder, "AlucardDH")) && !Directory.Exists(Path.Combine(ShadersFolder, "Stormshade"))
                )
				{
					Directory.Delete(ShadersFolder, true);
					Directory.Delete(TexturesFolder, true);
                    App.State.Prop.ExtraviReShadePresetsVersion = "";
				}
			}
            else
            {
                App.Logger.WriteLine("[ReShade::CheckModifications] versionManifest is null!");
            }

			// we're about to download - initialize directories
			Directory.CreateDirectory(BaseDirectory);
			Directory.CreateDirectory(FontsFolder);
			Directory.CreateDirectory(ShadersFolder);
			Directory.CreateDirectory(TexturesFolder);
			Directory.CreateDirectory(PresetsFolder);

			// check if we should download a fresh copy of the config
			// extravi may need to update the config ota, in which case we'll redownload it
			if (!File.Exists(ConfigLocation) || versionManifest is not null && App.State.Prop.ReShadeConfigVersion != versionManifest.ConfigFile)
                shouldFetchConfig = true;

            if (shouldFetchReShade)
            {
                App.Logger.WriteLine("[ReShade::CheckModifications] Installing/Upgrading ReShade...");

				{
					byte[] bytes = await App.HttpClient.GetByteArrayAsync($"{BaseUrl}/dxgi.zip");
                    using MemoryStream zipStream = new(bytes);
                    using ZipArchive archive = new(zipStream);
                    archive.ExtractToDirectory(Directories.Modifications, true);
                }
            }

            if (shouldFetchConfig)
            {
                await DownloadConfig();

                if (versionManifest is not null)
                {
                    App.State.Prop.ReShadeConfigVersion = versionManifest.ConfigFile;
                    App.State.Save();
                }
            }

            await DownloadShaders("Stock");

            if (App.Settings.Prop.UseReShadeExtraviPresets && App.State.Prop.ExtraviReShadePresetsVersion != versionManifest!.Presets)
            {
                await InstallExtraviPresets();
                App.State.Prop.ExtraviReShadePresetsVersion = versionManifest.Presets;
                App.State.Save();
            }

            SynchronizeConfigFile();
        }
    }
}
