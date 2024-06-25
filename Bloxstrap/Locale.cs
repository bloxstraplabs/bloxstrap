using System.Windows;

using Bloxstrap.Resources;

namespace Bloxstrap
{
    internal static class Locale
    {
        // TODO: put translated names
        public static readonly Dictionary<string, string> SupportedLocales = new()
        {
            { "nil", Strings.Enums_Theme_Default }, // /shrug
            { "en", "English" },
            { "en-US", "English (United States)" },
            { "ar", "Arabic" },
            { "bn", "Bengali" },
            { "bs", "Bosnian" },
            { "bg", "Bulgarian" },
            { "zh-CN", "Chinese (Simplified)" },
            { "zh-TW", "Chinese (Traditional)" },
            { "cs", "Czech" },
            { "dk", "Danish" },
            { "nl", "Dutch" },
            { "fl", "Filipino" },
            { "fi", "Finnish" },
            { "fr", "French" },
            { "de", "German" },
            { "he", "Hebrew" },
            { "hi", "Hindi" },
            { "hu", "Hungarian" },
            { "id", "Indonesian" },
            { "it", "Italian" },
            { "ja", "Japanese" },
            { "ko", "Korean" },
            { "lt", "Lithuanian" },
            { "no", "Norwegian" },
            { "pl", "Polish" },
            { "pt-BR", "Portuguese" },
            { "ro", "Romanian" },
            { "ru", "Russian" },
            { "es", "Spanish" },
            { "sv-SE", "Swedish" },
            { "th", "Thai" },
            { "tr", "Turkish" },
            { "uk", "Ukrainian" },
            { "vi", "Vietnamese" }
        };

        public static string GetIdentifierFromName(string language) => Locale.SupportedLocales.Where(x => x.Value == language).First().Key;

        public static void Set()
        {
            string identifier = App.Settings.Prop.Locale;

            if (!SupportedLocales.ContainsKey(identifier))
                identifier = "nil";

            if (identifier == "nil")
                return;

            App.CurrentCulture = new CultureInfo(identifier);

            CultureInfo.DefaultThreadCurrentUICulture = App.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = App.CurrentCulture;

            RoutedEventHandler? handler = null;

            if (identifier == "ar" || identifier == "he")
                handler = new((window, _) => ((Window)window).FlowDirection = FlowDirection.RightToLeft);
            else if (identifier == "th")
                handler = new((window, _) => ((Window)window).FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/Resources/Fonts/"), "./#Noto Sans Thai"));

            // https://supportcenter.devexpress.com/ticket/details/t905790/is-there-a-way-to-set-right-to-left-mode-in-wpf-for-the-whole-application
            if (handler is not null)
                EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, handler);
        }
    }
}
