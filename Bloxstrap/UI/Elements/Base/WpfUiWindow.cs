using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Bloxstrap.UI.Elements.Base
{
    public abstract class WpfUiWindow : UiWindow
    {
        private readonly IThemeService _themeService = new ThemeService();

        protected virtual bool AllowCustomBackground => false;

        public WpfUiWindow()
        {
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            const int customThemeIndex = 2; // index for CustomTheme merged dictionary

            var themeType = App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light;

            _themeService.SetTheme(themeType);

            if (ModGenerator.TryParseColor(App.Settings.Prop.AccentColor, out var accent))
                Accent.Apply(accent, themeType);
            else
                _themeService.SetSystemAccent();

            // there doesn't seem to be a way to query the name for merged dictionaries
            var dict = new ResourceDictionary { Source = new Uri($"pack://application:,,,/UI/Style/{Enum.GetName(App.Settings.Prop.Theme.GetFinal())}.xaml") };
            Application.Current.Resources.MergedDictionaries[customThemeIndex] = dict;

            if (!String.IsNullOrWhiteSpace(App.Settings.Prop.AppFont))
                FontFamily = new System.Windows.Media.FontFamily(App.Settings.Prop.AppFont);
            else
                ClearValue(FontFamilyProperty);

            if (IsInitialized)
                ApplyCustomBackground();

#if QA_BUILD
            this.BorderBrush = System.Windows.Media.Brushes.Red;
            this.BorderThickness = new Thickness(4);
#endif
        }

        private Brush? GetCustomBackground()
        {
            const string LOG_IDENT = "WpfUiWindow::GetCustomBackground";

            string image = App.Settings.Prop.BackgroundImage;

            if (!String.IsNullOrEmpty(image) && File.Exists(image))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(image);
                    bitmap.EndInit();

                    var stretch = App.Settings.Prop.BackgroundStretch switch
                    {
                        BackgroundStretch.Fill => Stretch.Fill,
                        BackgroundStretch.Uniform => Stretch.Uniform,
                        BackgroundStretch.None => Stretch.None,
                        _ => Stretch.UniformToFill
                    };

                    return new ImageBrush(bitmap) { Stretch = stretch, Opacity = App.Settings.Prop.BackgroundOpacity };
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to load background image {image}");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            string[] gradient = App.Settings.Prop.BackgroundGradient.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (gradient.Length == 2 && ModGenerator.TryParseColor(gradient[0], out var start) && ModGenerator.TryParseColor(gradient[1], out var end))
                return new LinearGradientBrush(start, end, 45) { Opacity = App.Settings.Prop.BackgroundOpacity };

            return null;
        }

        private void ApplyCustomBackground()
        {
            if (!AllowCustomBackground)
                return;

            var custom = GetCustomBackground();

            if (custom is null)
            {
                SetResourceReference(BackgroundProperty, "ApplicationBackgroundBrush");
                return;
            }

            var themeBackground = TryFindResource("ApplicationBackgroundBrush") as Brush;

            var group = new DrawingBrush
            {
                Stretch = Stretch.UniformToFill,
                Drawing = new DrawingGroup
                {
                    Children =
                    {
                        new GeometryDrawing(themeBackground ?? Brushes.Black, null, new RectangleGeometry(new Rect(0, 0, 16, 9))),
                        new GeometryDrawing(custom, null, new RectangleGeometry(new Rect(0, 0, 16, 9)))
                    }
                }
            };

            Background = group;
            WindowBackdropType = BackgroundType.None;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (App.Settings.Prop.WPFSoftwareRender || App.LaunchSettings.NoGPUFlag.Active)
            {
                if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }

            if (WindowBackdropType == BackgroundType.Mica)
            {
                WindowBackdropType = App.Settings.Prop.WindowBackdrop switch
                {
                    Enums.WindowBackdrop.Acrylic => BackgroundType.Acrylic,
                    Enums.WindowBackdrop.Aero => BackgroundType.Aero,
                    Enums.WindowBackdrop.None => BackgroundType.None,
                    _ => BackgroundType.Mica
                };
            }

            ApplyCustomBackground();

            base.OnSourceInitialized(e);
        }
    }
}
