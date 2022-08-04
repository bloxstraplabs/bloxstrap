using System.Diagnostics;
using System.Text.Json;
using Bloxstrap.Enums;

namespace Bloxstrap
{
    public class SettingsFormat
    {
        public string VersionGuid { get; set; }
        public bool UseOldDeathSound { get; set; } = true;
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.ProgressDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
    }

    public class SettingsManager
    {
        public SettingsFormat Settings = new();
        public bool ShouldSave = false;

        private string _saveLocation;
        public string SaveLocation
        {
            get => _saveLocation;

            set
            {
                if (!String.IsNullOrEmpty(_saveLocation))
                    return;

                _saveLocation = value;

                string settingsJson = "";

                if (File.Exists(_saveLocation))
                    settingsJson = File.ReadAllText(_saveLocation);

                Debug.WriteLine(settingsJson);

                try
                {
                    Settings = JsonSerializer.Deserialize<SettingsFormat>(settingsJson);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to fetch settings! Reverting to defaults... ({ex.Message})");
                    // Settings = new();
                }
            }
        }

        public void Save()
        {
            Debug.WriteLine("Attempting to save...");

            string SettingsJson = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            Debug.WriteLine(SettingsJson);

            if (!ShouldSave)
            {
                Debug.WriteLine("ShouldSave set to false, not saving...");
                return;
            }

            // save settings
            File.WriteAllText(SaveLocation, SettingsJson);
        }
    }
}
