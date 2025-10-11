using Bloxstrap.Enums.FlagPresets;
using Bloxstrap.Enums.GBSPresets;
using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Bloxstrap
{
    public class GBSEditor
    {
        public XDocument? Document { get; set; } = null!;

        public Dictionary<string, string> PresetPaths = new()
        {
            { "Rendering.FramerateCap", "{UserSettings}/int[@name='FramerateCap']" },
            { "Rendering.SavedQualityLevel", "{UserSettings}/token[@name='SavedQualityLevel']" }, // 0 is automatic
            
            { "User.MouseSensitivity", "{UserSettings}/float[@name='MouseSensitivity']"},
            { "User.VREnabled", "{UserSettings}/bool[@name='VREnabled']"},

            // mostly accessibility
            { "UI.Transparency", "{UserSettings}/float[@name='PreferredTransparency']" },
            { "UI.ReducedMotion", "{UserSettings}/bool[@name='ReducedMotion']" },
            { "UI.FontSize", "{UserSettings}/token[@name='PreferredTextSize']" }
        };

        // we are making it easier for ourselves
        // basically replacing {...} with a path
        // might expand in the future (studio support)
        public Dictionary<string, string> RootPaths = new()
        {
            { "UserSettings", "//Item[@class='UserGameSettings']/Properties" },
        };

        public static IReadOnlyDictionary<FontSize, string?> FontSizes => new Dictionary<FontSize, string?>
        {
            { FontSize.x1, "1" },
            { FontSize.x2, "2" },
            { FontSize.x3, "3" },
            { FontSize.x4, "4" }
        };

        public bool Loaded { get; set; } = false;

        public string FileLocation => Path.Combine(Paths.Roblox, "GlobalBasicSettings_13.xml");

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetPaths.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public string? GetPreset(string prefix)
        {
            if (!PresetPaths.ContainsKey(prefix))
                return null;

            return GetValue(PresetPaths[prefix]);
        }

        public void SetValue(string path, object? value)
        {
            path = ResolvePath(path);

            XElement? element = Document?.XPathSelectElement(path);
            if (element is null)
                return;

            element.Value = value?.ToString()!;
        }

        public string? GetValue(string path)
        {
            path = ResolvePath(path);

            return Document?.XPathSelectElement(path)?.Value;
        }

        public void Load()
        {
            string LOG_IDENT = "GBSEditor::Load";

            App.Logger.WriteLine(LOG_IDENT, $"Loading from {FileLocation}...");

            if (!File.Exists(FileLocation)) // since the file gets created after roblox starts it might not exist yet  
                return;

            try
            {
                Document = XDocument.Load(FileLocation);
                Loaded = true;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to load!");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public virtual void Save()
        {
            string LOG_IDENT = "GBSEditor::Save";

            App.Logger.WriteLine(LOG_IDENT, $"Saving to {FileLocation}...");

            try
            {
                Document?.Save(FileLocation);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to save");
                App.Logger.WriteException(LOG_IDENT, ex);

                return;
            }

            App.Logger.WriteLine(LOG_IDENT, "Save complete!");
        }

        private string ResolvePath(string rawPath)
        {
            return Regex.Replace(rawPath, @"\{(.+?)\}", match =>
            {
                string key = match.Groups[1].Value;
                return RootPaths.TryGetValue(key, out var value) ? value : match.Value; ;
            });
        }
    }
}