using System.Diagnostics;
using System.Text.Json;
using Bloxstrap.Enums;

namespace Bloxstrap
{
    public class SettingsFormat
    {
        public string VersionGuid { get; set; }

        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.ProgressDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = false;
        public bool UseOldDeathSound { get; set; } = true;
        public bool UseOldMouseCursor { get; set; } = false;
    }

    public class SettingsManager
    {
        public SettingsFormat Settings = new();
        public bool ShouldSave = false;
        private bool IsSaving = false;

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
            if (IsSaving)
            {
                // sometimes Save() is called at the same time from both Main() and Exit(),
                // so this is here to avoid the program exiting before saving

                Thread.Sleep(1000);
                return;
            }

            IsSaving = true;

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

            IsSaving = false;
        }
    }
}
