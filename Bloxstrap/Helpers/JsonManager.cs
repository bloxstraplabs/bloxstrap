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
        public T Prop { get; set; } = new T();
        //public bool ShouldSave { get; set; } = true;
        public string FileLocation => Path.Combine(Directories.Base, $"{typeof(T).Name}.json");
        //public string? FileLocation { get; set; } = null;

        public void Load()
        {
            //if (String.IsNullOrEmpty(FileLocation))
            //    throw new ArgumentNullException("No FileLocation has been set");

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

        public void Save()
        {
            App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Save] Attempting to save JSON to {FileLocation}...");

            //if (!ShouldSave || String.IsNullOrEmpty(FileLocation))
            //{
            //    App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>] Aborted save (ShouldSave set to false or FileLocation not set)");
            //    return;
            //}

            //if (!ShouldSave)
            if (!App.ShouldSaveConfigs)
            {
                App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Save] Aborted save (ShouldSave set to false)");
                return;
            }

            string json = JsonSerializer.Serialize(Prop, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileLocation, json);

            App.Logger.WriteLine($"[JsonManager<{typeof(T).Name}>::Save] JSON saved!");
        }
    }
}
