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
        public bool ShouldSave { get; set; } = true;
        public string FileLocation => Path.Combine(Directories.Base, $"{typeof(T).Name}.json");
        //public string? FileLocation { get; set; } = null;

        public void Load()
        {
            //if (String.IsNullOrEmpty(FileLocation))
            //    throw new ArgumentNullException("No FileLocation has been set");

            Debug.WriteLine($"[JsonManager<{typeof(T).Name}>] Loading JSON from {FileLocation}...");

            try
            {
                T? settings = JsonSerializer.Deserialize<T>(File.ReadAllText(FileLocation));
                Prop = settings ?? throw new ArgumentNullException("Deserialization returned null");
                Debug.WriteLine($"[JsonManager<{typeof(T).Name}>] JSON loaded successfully!");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JsonManager<{typeof(T).Name}>] Failed to load JSON! ({ex.Message})");
            }
        }

        public void Save()
        {
            Debug.WriteLine($"[JsonManager<{typeof(T).Name}>] Attempting to save JSON to {FileLocation}...");

            //if (!ShouldSave || String.IsNullOrEmpty(FileLocation))
            //{
            //    Debug.WriteLine($"[JsonManager<{typeof(T).Name}>] Aborted save (ShouldSave set to false or FileLocation not set)");
            //    return;
            //}

            if (!ShouldSave)
            {
                Debug.WriteLine($"[JsonManager<{typeof(T).Name}>] Aborted save (ShouldSave set to false)");
                return;
            }

            string json = JsonSerializer.Serialize(Prop, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileLocation, json);

            Debug.WriteLine($"[JsonManager<{typeof(T).Name}>] JSON saved!");
        }
    }
}
