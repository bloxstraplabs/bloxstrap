using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

using Bloxstrap.Models;

namespace Bloxstrap
{
    public class SettingsManager
    {
        public SettingsFormat Settings = new();
        public bool ShouldSave = false;
        private bool IsSaving = false;

        private string? _saveLocation;
        public string? SaveLocation
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
                    var settings = JsonSerializer.Deserialize<SettingsFormat>(settingsJson);

                    if (settings is null)
                        throw new Exception("Deserialization returned null");

                    Settings = settings;
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

            if (!ShouldSave || SaveLocation is null)
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
