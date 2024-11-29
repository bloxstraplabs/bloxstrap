namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string ClassName => nameof(FastFlagManager);

        public override string LOG_IDENT_CLASS => ClassName;
        
        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

        public bool Changed => !OriginalProp.SequenceEqual(Prop);

        public readonly FFlagPresets PresetConfig;

        public FastFlagManager()
        {
            PresetConfig = JsonSerializer.Deserialize<FFlagPresets>(File.ReadAllText("C:\\Users\\pizzaboxer\\Documents\\Projects\\Bloxstrap\\PrototypeSchema.json"));
            Debug.WriteLine(PresetConfig);
        }

        // all fflags are stored as strings
        // to delete a flag, set the value as null
        public void SetValue(string key, object? value)
        {
            const string LOG_IDENT = "FastFlagManager::SetValue";

            if (value is null)
            {
                if (Prop.ContainsKey(key))
                    App.Logger.WriteLine(LOG_IDENT, $"Deletion of '{key}' is pending");

                Prop.Remove(key);
            }
            else
            {
                if (Prop.ContainsKey(key))
                {
                    if (value.ToString() == Prop[key].ToString())
                        return;

                    App.Logger.WriteLine(LOG_IDENT, $"Changing of '{key}' from '{Prop[key]}' to '{value}' is pending");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{value}' is pending");
                }

                Prop[key] = value.ToString()!;
            }
        }

        /// <summary>
        /// Returns null if the flag has not been set
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string? GetValue(string key)
        {
            if (PresetConfig.Flags.ContainsKey(key))
                key = PresetConfig.Flags[key];

            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();

            return null;
        }

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetConfig.Flags.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public bool CheckPresetValue(string prefix, string value)
        {
            var presets = PresetConfig.Flags.Where(x => x.Key.StartsWith(prefix));

            foreach (var preset in presets)
            {
                if (GetValue(preset.Value) != value)
                    return false;
            }

            return true;
        }

        public bool CheckPresetValue(KeyValuePair<string, string> entry) => CheckPresetValue(entry.Key, entry.Value);

        public override void Save()
        {
            // convert all flag values to strings before saving

            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;

            base.Save();

            // clone the dictionary
            OriginalProp = new(Prop);
        }

        public override void Load(bool alertFailure = true)
        {
            base.Load(alertFailure);

            // clone the dictionary
            OriginalProp = new(Prop);

            // TODO - remove when activity tracking has been revamped
            if (GetValue("Network.Log") != "7")
                SetPreset("Network.Log", "7");
        }
    }
}
