using System.Runtime.CompilerServices;

namespace Bloxstrap
{
    public class JsonManager<T> where T : class, new()
    {
        public T OriginalProp { get; set; } = new();
        
        public T Prop { get; set; } = new();

        public virtual string ClassName => typeof(T).Name;

        public virtual string FileLocation => Path.Combine(Paths.Base, $"{ClassName}.json");

        public virtual string LOG_IDENT_CLASS => $"JsonManager<{ClassName}>";

        public virtual void Load(bool alertFailure = true)
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
                App.Logger.WriteException(LOG_IDENT, ex);

                if (alertFailure)
                {
                    string message = "";

                    if (ClassName == nameof(Settings))
                        message = Strings.JsonManager_SettingsLoadFailed;
                    else if (ClassName == nameof(FastFlagManager))
                        message = Strings.JsonManager_FastFlagsLoadFailed;

                    if (!String.IsNullOrEmpty(message))
                        Frontend.ShowMessageBox($"{message}\n\n{ex.GetType()}: {ex.Message}", System.Windows.MessageBoxImage.Warning);
                }

                Save();
            }
        }

        public virtual void Save()
        {
            string LOG_IDENT = $"{LOG_IDENT_CLASS}::Save";
            
            App.Logger.WriteLine(LOG_IDENT, $"Saving to {FileLocation}...");

            Directory.CreateDirectory(Path.GetDirectoryName(FileLocation)!);
            File.WriteAllText(FileLocation, JsonSerializer.Serialize(Prop, new JsonSerializerOptions { WriteIndented = true }));

            App.Logger.WriteLine(LOG_IDENT, "Save complete!");
        }
    }
}
