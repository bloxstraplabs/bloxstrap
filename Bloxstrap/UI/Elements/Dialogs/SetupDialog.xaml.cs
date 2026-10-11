using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Wpf.Ui.Common;

using Bloxstrap.UI.ViewModels.Settings;

using Button = System.Windows.Controls.Button;
using Path = System.Windows.Shapes.Path;

namespace Bloxstrap.UI.Elements.Dialogs
{
    public partial class SetupDialog : Window
    {
        private class Feature
        {
            public SymbolRegular Icon;
            public string Title = "";
            public string Description = "";
            public Func<bool> Get = null!;
            public Action<bool> Set = null!;
            public ToggleButton? Tile;
        }

        private static readonly Color DefaultAccent = Color.FromRgb(0xF5, 0xB8, 0x00);

        private readonly Dictionary<string, string> _importSources = SettingsImporter.FindInstalls();

        private readonly List<(string Label, FrameworkElement Element)> _steps = new();

        private readonly List<Feature> _features;

        private readonly bool _animate = SystemParameters.ClientAreaAnimation && !App.Settings.Prop.DisableAnimations;

        private readonly List<RadioButton> _importChoices = new();

        private readonly List<(Theme Theme, RadioButton Button)> _themeChoices = new();

        private readonly List<(string Color, RadioButton Button)> _accentChoices = new();

        private RadioButton _balanced = null!, _performance = null!, _quality = null!;

        private int _step;

        private bool _imported;

        public SetupDialog()
        {
            InitializeComponent();

            var settings = App.Settings.Prop;

            _features = new()
            {
                new() { Icon = SymbolRegular.Chat24, Title = Strings.Setup_Feature_Discord_Title, Description = Strings.Setup_Feature_Discord_Description, Get = () => settings.UseDiscordRichPresence, Set = x => settings.UseDiscordRichPresence = x },
                new() { Icon = SymbolRegular.Globe24, Title = Strings.Setup_Feature_Location_Title, Description = Strings.Setup_Feature_Location_Description, Get = () => settings.ShowServerDetails, Set = x => settings.ShowServerDetails = x },
                new() { Icon = SymbolRegular.Window24, Title = Strings.Setup_Feature_MultiInstance_Title, Description = Strings.Setup_Feature_MultiInstance_Description, Get = () => settings.MultiInstanceLaunching, Set = x => settings.MultiInstanceLaunching = x },
                new() { Icon = SymbolRegular.Timer24, Title = Strings.Setup_Feature_Playtime_Title, Description = Strings.Setup_Feature_Playtime_Description, Get = () => settings.TrackGameStats, Set = x => settings.TrackGameStats = x },
                new() { Icon = SymbolRegular.ArrowRepeatAll24, Title = Strings.Setup_Feature_Rejoin_Title, Description = Strings.Setup_Feature_Rejoin_Description, Get = () => settings.AutoRejoin, Set = x => settings.AutoRejoin = x },
                new() { Icon = SymbolRegular.Keyboard24, Title = Strings.Setup_Feature_Socd_Title, Description = Strings.Setup_Feature_Socd_Description, Get = () => settings.SocdEnabled, Set = x => settings.SocdEnabled = x }
            };

            if (_importSources.Any())
                _steps.Add((Strings.Setup_Rail_Import, ImportStep));

            _steps.Add((Strings.Setup_Rail_Look, LookStep));
            _steps.Add((Strings.Setup_Rail_Features, FeaturesStep));
            _steps.Add((Strings.Setup_Rail_Performance, PerformanceStep));
            _steps.Add((Strings.Setup_Rail_Done, DoneStep));

            if (!_importSources.Any())
                ImportStep.Visibility = Visibility.Collapsed;

            BuildImportChoices();
            BuildThemeChoices();
            BuildAccentChoices();
            BuildFeatureChoices();
            BuildPerformanceChoices();
            LoadChoices();
        }

        // Building

        private static TextBlock Text(string text, string style, FrameworkElement owner) => new() { Text = text, Style = (Style)owner.FindResource(style) };

        private void BuildImportChoices()
        {
            bool first = true;

            foreach (var source in _importSources.Keys)
            {
                var content = new StackPanel();
                content.Children.Add(Text(String.Format(Strings.Setup_Import_From, source), "CardTitle", this));
                content.Children.Add(Text(Strings.Setup_Import_From_Description, "CardText", this));

                var button = new RadioButton { GroupName = "Import", Style = (Style)FindResource("ChoiceCard"), Content = content, Tag = source, IsChecked = first };
                _importChoices.Add(button);
                ImportChoices.Children.Add(button);
                first = false;
            }

            var fresh = new StackPanel();
            fresh.Children.Add(Text(Strings.Setup_Import_Fresh_Title, "CardTitle", this));
            fresh.Children.Add(Text(Strings.Setup_Import_Fresh_Description, "CardText", this));

            var freshButton = new RadioButton { GroupName = "Import", Style = (Style)FindResource("ChoiceCard"), Content = fresh };
            _importChoices.Add(freshButton);
            ImportChoices.Children.Add(freshButton);
        }

        private static Border WindowPreview(Brush background, Brush bar, Brush line)
        {
            var stack = new StackPanel();
            stack.Children.Add(new Border { Height = 10, Background = bar, CornerRadius = new CornerRadius(6, 6, 0, 0) });

            var body = new StackPanel { Margin = new Thickness(10, 9, 10, 0) };
            body.Children.Add(new Border { Height = 6, Width = 54, HorizontalAlignment = HorizontalAlignment.Left, CornerRadius = new CornerRadius(3), Background = line });
            body.Children.Add(new Border { Height = 6, Width = 38, Margin = new Thickness(0, 6, 0, 0), HorizontalAlignment = HorizontalAlignment.Left, CornerRadius = new CornerRadius(3), Background = line });
            stack.Children.Add(body);

            return new Border { Height = 62, CornerRadius = new CornerRadius(6), Background = background, Child = stack, ClipToBounds = true };
        }

        private void BuildThemeChoices()
        {
            var light = new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3));
            var lightBar = new SolidColorBrush(Color.FromRgb(0xE2, 0xE2, 0xE2));
            var lightLine = new SolidColorBrush(Color.FromRgb(0xC8, 0xC8, 0xC8));
            var dark = new SolidColorBrush(Color.FromRgb(0x24, 0x24, 0x28));
            var darkBar = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x38));
            var darkLine = new SolidColorBrush(Color.FromRgb(0x4A, 0x4A, 0x52));

            var split = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            split.GradientStops.Add(new GradientStop(light.Color, 0.5));
            split.GradientStops.Add(new GradientStop(dark.Color, 0.5));

            var splitBar = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            splitBar.GradientStops.Add(new GradientStop(lightBar.Color, 0.5));
            splitBar.GradientStops.Add(new GradientStop(darkBar.Color, 0.5));

            var options = new (Theme Theme, string Label, Border Preview)[]
            {
                (Theme.Default, Strings.Common_SystemDefault, WindowPreview(split, splitBar, darkLine)),
                (Theme.Light, Strings.Setup_Theme_Light, WindowPreview(light, lightBar, lightLine)),
                (Theme.Dark, Strings.Setup_Theme_Dark, WindowPreview(dark, darkBar, darkLine))
            };

            foreach (var option in options)
            {
                var content = new StackPanel();
                content.Children.Add(option.Preview);
                content.Children.Add(new TextBlock { Text = option.Label, Margin = new Thickness(0, 10, 0, 0), FontSize = 14, FontWeight = FontWeights.SemiBold });

                var button = new RadioButton { GroupName = "Theme", Style = (Style)FindResource("ChoiceCard"), Content = content, Margin = new Thickness(0, 0, 10, 0) };
                _themeChoices.Add((option.Theme, button));
                ThemeChoices.Children.Add(button);
            }
        }

        private void BuildAccentChoices()
        {
            foreach (var preset in AppearanceViewModel.AccentPresets)
            {
                Color color = String.IsNullOrEmpty(preset.Value) ? DefaultAccent : (Color)ColorConverter.ConvertFromString(preset.Value);

                var button = new RadioButton { GroupName = "Accent", Style = (Style)FindResource("Swatch"), Background = new SolidColorBrush(color), ToolTip = String.IsNullOrEmpty(preset.Value) ? "reistrap" : preset.Key };
                button.Checked += (_, _) => SetAccent(color);

                _accentChoices.Add((preset.Value, button));
                AccentChoices.Children.Add(button);
            }
        }

        private void BuildFeatureChoices()
        {
            foreach (var feature in _features)
            {
                var content = new StackPanel();
                content.Children.Add(new Wpf.Ui.Controls.SymbolIcon { Symbol = feature.Icon, FontSize = 22, HorizontalAlignment = HorizontalAlignment.Left, Foreground = (Brush)FindResource("AccentBrush") });
                content.Children.Add(new TextBlock { Text = feature.Title, Margin = new Thickness(0, 10, 0, 0), Style = (Style)FindResource("CardTitle") });
                content.Children.Add(Text(feature.Description, "CardText", this));

                var tile = new ToggleButton { Style = (Style)FindResource("FeatureTile"), Content = content };

                feature.Tile = tile;
                FeatureChoices.Children.Add(tile);
            }
        }

        private RadioButton PerformanceCard(SymbolRegular icon, string title, string description)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var iconHolder = new Border { Width = 44, Height = 44, CornerRadius = new CornerRadius(12), Background = (Brush)FindResource("SurfaceHoverBrush"), Margin = new Thickness(0, 0, 16, 0), VerticalAlignment = VerticalAlignment.Center };
            var symbol = new Wpf.Ui.Controls.SymbolIcon { Symbol = icon, FontSize = 22, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            symbol.SetResourceReference(Control.ForegroundProperty, "AccentBrush");
            iconHolder.Child = symbol;
            grid.Children.Add(iconHolder);

            var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            text.Children.Add(Text(title, "CardTitle", this));
            text.Children.Add(Text(description, "CardText", this));
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            var button = new RadioButton { GroupName = "Performance", Style = (Style)FindResource("ChoiceCard"), Content = grid };
            PerformanceChoices.Children.Add(button);

            return button;
        }

        private void BuildPerformanceChoices()
        {
            _balanced = PerformanceCard(SymbolRegular.Sparkle24, Strings.Setup_Performance_Default_Title, Strings.Setup_Performance_Default_Description);
            _performance = PerformanceCard(SymbolRegular.Flash24, Strings.Setup_Performance_Fps_Title, Strings.Setup_Performance_Fps_Description);
            _quality = PerformanceCard(SymbolRegular.Diamond24, Strings.Setup_Performance_Quality_Title, Strings.Setup_Performance_Quality_Description);
            _balanced.IsChecked = true;
        }

        private void LoadChoices()
        {
            var settings = App.Settings.Prop;

            foreach (var choice in _themeChoices)
                choice.Button.IsChecked = choice.Theme == settings.Theme;

            var accent = _accentChoices.FirstOrDefault(x => String.Equals(x.Color, settings.AccentColor, StringComparison.OrdinalIgnoreCase));
            (accent.Button ?? _accentChoices[0].Button).IsChecked = true;

            foreach (var feature in _features)
                feature.Tile!.IsChecked = feature.Get();
        }

        // Accent

        private void SetAccent(Color color)
        {
            Resources["AccentBrush"] = new SolidColorBrush(color);
            Resources["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x26, color.R, color.G, color.B));

            foreach (var feature in _features)
            {
                if (feature.Tile?.Content is StackPanel panel && panel.Children[0] is Wpf.Ui.Controls.SymbolIcon icon)
                    icon.Foreground = (Brush)Resources["AccentBrush"];
            }
        }

        // Steps

        private void BuildRail()
        {
            StepList.Children.Clear();

            for (int i = 0; i < _steps.Count; i++)
            {
                bool done = i < _step;
                bool current = i == _step;

                var circle = new Grid { Width = 26, Height = 26 };
                var ellipse = new Ellipse { StrokeThickness = 1.5 };

                if (done)
                {
                    ellipse.SetResourceReference(Shape.FillProperty, "AccentBrush");
                    circle.Children.Add(ellipse);
                    circle.Children.Add(new Path { Data = Geometry.Parse("M 8,13.5 L 11.5,17 L 18.5,9.5"), Stroke = new SolidColorBrush(Color.FromRgb(0x14, 0x14, 0x14)), StrokeThickness = 2.2, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round, StrokeLineJoin = PenLineJoin.Round });
                }
                else
                {
                    if (current)
                        ellipse.SetResourceReference(Shape.StrokeProperty, "AccentBrush");
                    else
                        ellipse.Stroke = (Brush)FindResource("DimBrush");

                    circle.Children.Add(ellipse);

                    var number = new TextBlock { Text = (i + 1).ToString(), FontSize = 12, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                    if (current)
                        number.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
                    else
                        number.Foreground = (Brush)FindResource("DimBrush");

                    circle.Children.Add(number);
                }

                var label = new TextBlock
                {
                    Text = _steps[i].Label,
                    Margin = new Thickness(12, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = current ? FontWeights.SemiBold : FontWeights.Normal,
                    Foreground = current ? new SolidColorBrush(Color.FromRgb(0xF2, 0xF1, 0xEC)) : (Brush)FindResource(done ? "MutedBrush" : "DimBrush")
                };

                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 22) };
                row.Children.Add(circle);
                row.Children.Add(label);

                StepList.Children.Add(row);
            }
        }

        private void ShowStep(int direction)
        {
            for (int i = 0; i < _steps.Count; i++)
                _steps[i].Element.Visibility = i == _step ? Visibility.Visible : Visibility.Collapsed;

            bool last = _step == _steps.Count - 1;

            BackButton.Visibility = _step == 0 ? Visibility.Hidden : Visibility.Visible;
            NextButton.Content = last ? Strings.Setup_Finish : Strings.Setup_Next;

            BuildRail();

            if (_animate)
            {
                var translate = (TranslateTransform)StepHost.RenderTransform;
                var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

                translate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(36 * direction, 0, TimeSpan.FromMilliseconds(320)) { EasingFunction = ease });
                StepHost.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(260)) { EasingFunction = ease });
            }

            if (last)
                PlayDoneAnimation();
        }

        // Animation

        private void Animate(UIElement element, DependencyProperty property, double from, double to, int delayMs, int durationMs, IEasingFunction? ease = null)
        {
            var animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = ease ?? new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.HoldEnd
            };

            element.BeginAnimation(property, animation);
        }

        private void Animate(Animatable target, DependencyProperty property, double from, double to, int delayMs, int durationMs, IEasingFunction? ease = null)
        {
            var animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = ease ?? new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.HoldEnd
            };

            target.BeginAnimation(property, animation);
        }

        private void PlayIntro()
        {
            var tiles = new (Rectangle Tile, double X, double Y)[]
            {
                (TileTL, -70, -70),
                (TileTR, 70, -70),
                (TileBL, -70, 70),
                (TileBR, 70, 70)
            };

            var chipScale = (ScaleTransform)Chip.RenderTransform;
            var textTranslate = (TranslateTransform)WelcomeText.RenderTransform;

            if (!_animate)
            {
                foreach (var tile in tiles)
                    tile.Tile.Opacity = 1;

                chipScale.ScaleX = chipScale.ScaleY = 1;
                WelcomeText.Opacity = 1;
                textTranslate.Y = 0;
                return;
            }

            var back = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 };

            for (int i = 0; i < tiles.Length; i++)
            {
                var translate = (TranslateTransform)tiles[i].Tile.RenderTransform;
                int delay = 150 + i * 90;

                Animate(tiles[i].Tile, OpacityProperty, 0, 1, delay, 260);
                Animate(translate, TranslateTransform.XProperty, tiles[i].X, 0, delay, 520, back);
                Animate(translate, TranslateTransform.YProperty, tiles[i].Y, 0, delay, 520, back);
            }

            var pop = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.8 };
            Animate(chipScale, ScaleTransform.ScaleXProperty, 0, 1, 680, 380, pop);
            Animate(chipScale, ScaleTransform.ScaleYProperty, 0, 1, 680, 380, pop);

            Animate(WelcomeText, OpacityProperty, 0, 1, 880, 500);
            Animate(textTranslate, TranslateTransform.YProperty, 18, 0, 880, 600);

            var drift = new SineEase { EasingMode = EasingMode.EaseInOut };

            GlowA.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(-180, -40, TimeSpan.FromSeconds(9)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = drift });
            GlowA.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(-240, -150, TimeSpan.FromSeconds(11)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = drift });
            GlowB.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(580, 460, TimeSpan.FromSeconds(10)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = drift });
            GlowB.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(320, 240, TimeSpan.FromSeconds(12)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = drift });
        }

        private void PlayDoneAnimation()
        {
            var scale = (ScaleTransform)DoneCircle.RenderTransform;

            if (!_animate)
            {
                scale.ScaleX = scale.ScaleY = 1;
                DoneCheck.StrokeDashOffset = 0;
                return;
            }

            var pop = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.6 };
            Animate(scale, ScaleTransform.ScaleXProperty, 0, 1, 120, 420, pop);
            Animate(scale, ScaleTransform.ScaleYProperty, 0, 1, 120, 420, pop);
            Animate(DoneCheck, Shape.StrokeDashOffsetProperty, 10, 0, 420, 420);
        }

        // Saving

        private void Import()
        {
            const string LOG_IDENT = "SetupDialog::Import";

            if (_imported || _importChoices.FirstOrDefault(x => x.IsChecked == true)?.Tag is not string source || !_importSources.TryGetValue(source, out string? folder))
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

            var theme = _themeChoices.FirstOrDefault(x => x.Button.IsChecked == true);

            if (theme.Button is not null)
                settings.Theme = theme.Theme;

            var accent = _accentChoices.FirstOrDefault(x => x.Button.IsChecked == true);

            if (accent.Button is not null)
                settings.AccentColor = accent.Color;

            foreach (var feature in _features)
                feature.Set(feature.Tile!.IsChecked == true);

            if (_performance.IsChecked == true)
            {
                App.FastFlags.SetPreset("Rendering.MSAA", "1");
                App.FastFlags.SetPreset("Geometry.Grass", "0");
                App.FastFlags.SetPresetEnum("Rendering.Mode", "D3D11", "True");
            }
            else if (_quality.IsChecked == true)
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

            App.State.Prop.ShowWelcome = false;
            App.State.Save();

            Close();
        }

        // Events

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int round = 2;
            DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 33, ref round, sizeof(int));

            PlayIntro();
            Activate();
        }

        private void Background_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && e.OriginalSource is not ButtonBase)
                DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Finish(false);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StepsView.Visibility = Visibility.Visible;
            ShowStep(1);

            if (!_animate)
            {
                WelcomeView.Visibility = Visibility.Collapsed;
                StepsView.Opacity = 1;
                return;
            }

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (_, _) => WelcomeView.Visibility = Visibility.Collapsed;
            WelcomeView.BeginAnimation(OpacityProperty, fadeOut);

            Animate(StepsView, OpacityProperty, 0, 1, 120, 320);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_steps[_step].Element == ImportStep)
                Import();

            if (_step == _steps.Count - 1)
            {
                Finish(true);
                return;
            }

            _step++;
            ShowStep(1);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_step == 0)
                return;

            _step--;
            ShowStep(-1);
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e) => Finish(false);
    }
}
