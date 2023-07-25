using System.Runtime.CompilerServices;

namespace Bloxstrap
{
    public class JsonManager<T> where T : new()
    {
        public T Prop { get; set; } = new();
        public virtual string FileLocation => Path.Combine(Directories.Base, $"{typeof(T).Name}.json");

        private string LOG_IDENT_CLASS => $"JsonManager<{typeof(T).Name}>";

        public virtual void Load()
        {
            string LOG_IDENT = $"{LOG_IDENT_CLASS}::Load";

            App.Logger.WriteLine(LOG_IDENT, $"Loading from {FileLocation}...");

            try
            {
                T? settings = JsonSerializer.Deserialize<T>(File.ReadAllText(FileLocation));

                if (settings is null)
                    throw new ArgumentNullException("Deserialization returned null");

                Prop = settings;

                App.Logger.WriteLine(LOG_IDENT, "Loaded successfully!");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to load!");
                App.Logger.WriteLine(LOG_IDENT, $"{ex.Message}");
            }
        }

        public virtual void Save()
        {
            string LOG_IDENT = $"{LOG_IDENT_CLASS}::Save";
            
            if (!App.ShouldSaveConfigs)
            {
                App.Logger.WriteLine(LOG_IDENT, "Save request ignored");
                return;
            }

            App.Logger.WriteLine(LOG_IDENT, $"Saving to {FileLocation}...");

            Directory.CreateDirectory(Path.GetDirectoryName(FileLocation)!);
            File.WriteAllText(FileLocation, JsonSerializer.Serialize(Prop, new JsonSerializerOptions { WriteIndented = true }));

            App.Logger.WriteLine(LOG_IDENT, "Save complete!");
        }
    }
}
