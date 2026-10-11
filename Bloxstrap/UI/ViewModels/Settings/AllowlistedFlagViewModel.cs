using System.Windows;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class AllowlistedFlagViewModel : NotifyPropertyChangedViewModel
    {
        public string Name { get; }

        public string Description => Strings.ResourceManager.GetString($"Menu.FastFlags.Allowlist.{Name}") ?? "";

        public bool IsBool => FastFlagAllowlist.IsBoolFlag(Name);

        public Visibility BoolVisibility => IsBool ? Visibility.Visible : Visibility.Collapsed;

        public Visibility NumberVisibility => IsBool ? Visibility.Collapsed : Visibility.Visible;

        public AllowlistedFlagViewModel(string name)
        {
            Name = name;
        }

        public bool BoolValue
        {
            get => String.Equals(App.FastFlags.GetValue(Name), "True", StringComparison.OrdinalIgnoreCase);
            set => App.FastFlags.SetValue(Name, value ? "True" : null);
        }

        public string NumberValue
        {
            get => App.FastFlags.GetValue(Name) ?? "";
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    App.FastFlags.SetValue(Name, null);
                else if (Int32.TryParse(value, out int number))
                    App.FastFlags.SetValue(Name, number);

                OnPropertyChanged(nameof(NumberValue));
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(BoolValue));
            OnPropertyChanged(nameof(NumberValue));
        }
    }
}
