using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Bloxstrap.Helpers
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string FileLocation => Path.Combine(Directories.Modifications, "ClientSettings\\ClientAppSettings.json");

        // we put any changes we want to make to fastflags here
        // these will apply after bloxstrap finishes installing or after the menu closes
        // to delete a fastflag, set the value to null
        public Dictionary<string, object?> Changes = new();

        // only one missing here is Metal because lol
        public static IReadOnlyDictionary<string, string> RenderingModes { get; set; } = new Dictionary<string, string>()
        {
            { "Automatic", "" },
            { "Direct3D 11", "FFlagDebugGraphicsPreferD3D11" },
            { "OpenGL", "FFlagDebugGraphicsPreferOpenGL" },
            { "Vulkan", "FFlagDebugGraphicsPreferVulkan" }
        };

        // this returns null if the fflag doesn't exist
        // this also returns as a string because deserializing an object doesn't
        // deserialize back into the original object type, it instead deserializes
        // as a "JsonElement" which is annoying
        public string? GetValue(string key)
        {
            // check if we have an updated change for it pushed first
            if (Changes.TryGetValue(key, out object? changedValue))
                return changedValue?.ToString();

            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();

            return null;
        }

        public void SetRenderingMode(string value)
        {
            foreach (var mode in RenderingModes)
            {
                if (value != "Automatic")
                    App.FastFlags.Changes[mode.Value] = null;
            }

            if (value != "Automatic")
                App.FastFlags.Changes[RenderingModes[value]] = true;
        }

        public override void Save()
        {
            App.Logger.WriteLine($"[FastFlagManager::Save] Attempting to save JSON to {FileLocation}...");

            if (Changes.Count == 0)
            {
                App.Logger.WriteLine($"[FastFlagManager::Save] No changes to apply, aborting.");
                return;
            }

            // reload for any changes made while the menu was open
            Load();

            foreach (var change in Changes)
            {
                if (change.Value is null)
                {
                    App.Logger.WriteLine($"[FastFlagManager::Save] Removing '{change.Key}'");
                    Prop.Remove(change.Key);
                    continue;
                }

                App.Logger.WriteLine($"[FastFlagManager::Save] Setting '{change.Key}' to {change.Value}");
                Prop[change.Key] = change.Value;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(FileLocation)!);
            File.WriteAllText(FileLocation, JsonSerializer.Serialize(Prop, new JsonSerializerOptions { WriteIndented = true }));

            Changes.Clear();

            App.Logger.WriteLine($"[FastFlagManager::Save] JSON saved!");
        }
    }
}
