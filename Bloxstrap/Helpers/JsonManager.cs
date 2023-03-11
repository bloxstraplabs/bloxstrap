using Bloxstrap.Models;
using Bloxstrap.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading;

namespace Bloxstrap.Helpers
{
    public class JsonManager<T> where T : new()
    {
        public T Prop { get; set; } = new();
        public virtual string FileLocation => AltFileLocation ?? Path.Combine(Directories.Base, $"{typeof(T).Name}.json");
        public string? AltFileLocation { get; set; }

        public void Load()
        {
            App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Load] Loading JSON from {FileLocation}...");

            try
            {
                T? settings = JsonSerializer.Deserialize<T>(File.ReadAllText(FileLocation));
                Prop = settings ?? throw new ArgumentNullException("Deserialization returned null");
                App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Load] JSON loaded successfully!");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Load] Failed to load JSON! ({ex.Message})");
            }
        }

        public virtual void Save()
        {
            App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Save] Attempting to save JSON to {FileLocation}...");

            if (!App.ShouldSaveConfigs)
            {
                App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Save] Aborted save (ShouldSave set to false)");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(FileLocation)!);
            File.WriteAllText(FileLocation, JsonSerializer.Serialize(Prop, new JsonSerializerOptions { WriteIndented = true }));

            App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Save] JSON saved!");
        }
    }
}
