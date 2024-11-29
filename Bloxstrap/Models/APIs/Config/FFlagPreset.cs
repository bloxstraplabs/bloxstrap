namespace Bloxstrap.Models.APIs.Config
{
    // technically an entity, whatever
    public class FFlagPreset
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string? HelpLink { get; set; }

        public string Type { get; set; } = null!;

        /// <summary>
        /// Specific to TextBox and Toggle
        /// </summary>
        public Dictionary<string, string>? Apply { get; set; }

        #region ComboBox
        // Data
        public Dictionary<string, Dictionary<string, string>>? Options { get; set; }

        // Frontend
        public List<string>? ComboBoxEntries => Options?.Keys.Prepend("Common.Default").ToList();

        public string ComboBoxSelection 
        { 
            get
            {
                if (Options is null || ComboBoxEntries is null)
                    return "";

                foreach (var optionEntry in Options)
                {
                    bool matches = true;

                    foreach (var flagEntry in optionEntry.Value)
                    {
                        if (matches && !App.FastFlags.CheckPresetValue(flagEntry))
                            matches = false;
                    }

                    if (matches)
                        return optionEntry.Key;
                }

                return ComboBoxEntries[0];
            }

            set
            {
                if (Options is null || ComboBoxEntries is null)
                    throw new InvalidOperationException();

                if (value == ComboBoxEntries[0])
                {
                    // get all flags that this preset sets
                    var flags = Options.Values.SelectMany(x => x.Keys).Distinct().ToList();

                    foreach (string flag in flags)
                        App.FastFlags.SetPreset(flag, null);
                }
                else
                {
                    foreach (var entry in Options[value])
                        App.FastFlags.SetPreset(entry.Key, entry.Value);
                }
            }
        }
        #endregion

        #region TextBox
        // TODO: filtering (i dont know how tf thats gonna work)

        // Data
        public string? InputFilter { get; set; }

        public string? Subject { get; set; }

        public string? DefaultValue { get; set; }

        // Frontend
        public string TextBoxValue
        {
            get
            {
                if (Subject is null || DefaultValue is null)
                    return "";

                return App.FastFlags.GetValue(Subject) ?? DefaultValue;
            }

            set
            {
                if (Apply is null || DefaultValue is null)
                    throw new InvalidOperationException();

                if (InputFilter is not null && !Regex.IsMatch(value, InputFilter))
                {
                    value = TextBoxValue;
                    return;
                }

                foreach (var entry in Apply)
                {
                    if (value == DefaultValue)
                        App.FastFlags.SetPreset(entry.Key, null);
                    else
                        App.FastFlags.SetPreset(entry.Key, String.Format(entry.Value, value));
                }
            }
        }
        #endregion

        #region Toggle
        public string? EnabledIf { get; set; }

        public bool ToggleEnabled
        {
            get
            {
                if (EnabledIf is null)
                    return false;

                return new FlexParser(EnabledIf).Evaluate();
            }

            set
            {
                if (Apply is null)
                    throw new InvalidOperationException();

                foreach (var entry in Apply)
                    App.FastFlags.SetPreset(entry.Key, value ? entry.Value : null);
            }
        }
        #endregion
    }
}
