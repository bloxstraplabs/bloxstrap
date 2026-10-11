using System.Windows;

using Bloxstrap.UI.Elements.Base;
using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Dialogs
{
    public partial class SetupDialog : WpfUiWindow
    {
        private readonly Dictionary<string, string> _importSources = SettingsImporter.FindInstalls();

        private readonly FrameworkElement[] _steps;

        private int _step;

        private bool _imported;

        public SetupDialog()
        {
            InitializeComponent();

            _steps = new FrameworkElement[] { WelcomeStep, LookStep, FeaturesStep, PerformanceStep, DoneStep };

            if (_importSources.Any())
            {
                ImportSourceBox.ItemsSource = _importSources.Keys;
                ImportSourceBox.SelectedIndex = 0;
            }
            else
            {
                ImportOption.Visibility = Visibility.Collapsed;
            }

            ThemeBox.ItemsSource = Enum.GetValues<Theme>().Select(x => new KeyValuePair<Theme, string>(x, x == Theme.Default ? Strings.Common_SystemDefault : x.ToString())).ToList();
            ThemeBox.DisplayMemberPath = "Value";
            ThemeBox.SelectedValuePath = "Key";

            AccentBox.ItemsSource = AppearanceViewModel.AccentPresets.Keys;

            LoadChoices();
            ShowStep();
        }

        private void LoadChoices()
        {
            var settings = App.Settings.Prop;

            ThemeBox.SelectedValue = settings.Theme;
            AccentBox.SelectedItem = AppearanceViewModel.AccentPresets.FirstOrDefault(x => String.Equals(x.Value, settings.AccentColor, StringComparison.OrdinalIgnoreCase)).Key ?? Strings.Common_SystemDefault;

            DiscordToggle.IsChecked = settings.UseDiscordRichPresence;
            LocationToggle.IsChecked = settings.ShowServerDetails;
            MultiInstanceToggle.IsChecked = settings.MultiInstanceLaunching;
            PlaytimeToggle.IsChecked = settings.TrackGameStats;
            RejoinToggle.IsChecked = settings.AutoRejoin;
        }

        private void ShowStep()
        {
            for (int i = 0; i < _steps.Length; i++)
                _steps[i].Visibility = i == _step ? Visibility.Visible : Visibility.Collapsed;

            (string heading, string subheading) = _step switch
            {
                0 => (Strings.Setup_Welcome_Heading, Strings.Setup_Welcome_Subheading),
                1 => (Strings.Setup_Look_Heading, Strings.Setup_Look_Subheading),
                2 => (Strings.Setup_Features_Heading, Strings.Setup_Features_Subheading),
                3 => (Strings.Setup_Performance_Heading, Strings.Setup_Performance_Subheading),
                _ => (Strings.Setup_Done_Heading, Strings.Setup_Done_Subheading)
            };

            StepText.Text = String.Format(Strings.Setup_Step, _step + 1, _steps.Length);
            HeadingText.Text = heading;
            SubheadingText.Text = subheading;

            BackButton.Visibility = _step == 0 ? Visibility.Hidden : Visibility.Visible;
            SkipButton.Visibility = _step == _steps.Length - 1 ? Visibility.Hidden : Visibility.Visible;
            NextButton.Content = _step == _steps.Length - 1 ? Strings.Setup_Finish : Strings.Setup_Next;
        }

        private void Import()
        {
            const string LOG_IDENT = "SetupDialog::Import";

            if (_imported || ImportToggle.IsChecked != true || ImportSourceBox.SelectedItem is not string source || !_importSources.TryGetValue(source, out string? folder))
                return;

            try
            {
                SettingsImporter.ImportSettings(folder);
                SettingsImporter.ImportFastFlags(folder);
                SettingsImporter.ImportMods(folder);
                _imported = true;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }

            LoadChoices();
        }

        private void ApplyChoices()
        {
            var settings = App.Settings.Prop;

            if (ThemeBox.SelectedValue is Theme theme)
                settings.Theme = theme;

            if (AccentBox.SelectedItem is string accent && AppearanceViewModel.AccentPresets.TryGetValue(accent, out string? color))
                settings.AccentColor = color;

            settings.UseDiscordRichPresence = DiscordToggle.IsChecked == true;
            settings.ShowServerDetails = LocationToggle.IsChecked == true;
            settings.MultiInstanceLaunching = MultiInstanceToggle.IsChecked == true;
            settings.TrackGameStats = PlaytimeToggle.IsChecked == true;
            settings.AutoRejoin = RejoinToggle.IsChecked == true;

            if (PerformanceRadio.IsChecked == true)
            {
                App.FastFlags.SetPreset("Rendering.MSAA", "1");
                App.FastFlags.SetPreset("Geometry.Grass", "0");
                App.FastFlags.SetPresetEnum("Rendering.Mode", "D3D11", "True");
            }
            else if (QualityRadio.IsChecked == true)
            {
                App.FastFlags.SetPreset("Rendering.MSAA", "4");
                App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", "True");
                App.FastFlags.SetPreset("Rendering.TextureQuality.Level", "3");
                App.FastFlags.SetPreset("Rendering.FRMQuality", "21");
            }
        }

        private void Finish(bool apply)
        {
            if (apply)
                ApplyChoices();

            if (apply || _imported)
            {
                App.Settings.Save();
                App.FastFlags.Save();
            }

            App.State.Prop.SetupCompleted = true;
            App.State.Save();

            Close();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_step == 0)
                Import();

            if (_step == _steps.Length - 1)
            {
                Finish(true);
                return;
            }

            _step++;
            ShowStep();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_step == 0)
                return;

            _step--;
            ShowStep();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e) => Finish(false);
    }
}
