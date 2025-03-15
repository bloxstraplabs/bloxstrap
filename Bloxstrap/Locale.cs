using System.Windows;

namespace Bloxstrap
{
    internal static class Locale
    {
        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.InvariantCulture;

        public static bool RightToLeft { get; private set; } = false;

        private static readonly List<string> _rtlLocales = new() { "ar", "he", "fa" };

        public static readonly Dictionary<string, string> SupportedLocales = new()
        {
            { "nil", Strings.Common_SystemDefault },
            { "en", "English" },
            { "en-US", "English (United States)" },
#if QA_BUILD
            { "sq", "Albanian" }, // Albanian (TODO: translate string)
#endif
            { "ar", "العربية" }, // Arabic
            { "bg", "Български" }, // Bulgarian
#if QA_BUILD
            { "bn", "বাংলা" }, // Bengali
            { "bs", "Bosanski" }, // Bosnian
#endif
            { "cs", "Čeština" }, // Czech
            { "de", "Deutsch" }, // German
#if QA_BUILD
            { "da", "Dansk" }, // Danish
#endif
            { "es-ES", "Español" }, // Spanish
#if QA_BUILD
            { "el", "Ελληνικά" }, // Greek
#endif
            { "fa", "فارسی" }, // Persian
            { "fi", "Suomi" }, // Finnish
            { "fil", "Filipino" }, // Filipino
            { "fr", "Français" }, // French
#if QA_BUILD
            { "he", "עברית‎" }, // Hebrew
            { "hi", "Hindi (Latin)" }, // Hindi
#endif
            { "hr", "Hrvatski" }, // Croatian
            { "hu", "Magyar" }, // Hungarian
#if QA_BUILD
            { "is", "Icelandic" }, // Icelandic (TODO: translate string)
#endif
            { "id", "Bahasa Indonesia" }, // Indonesian
            { "it", "Italiano" }, // Italian
            { "ja", "日本語" }, // Japanese
            { "ko", "한국어" }, // Korean
#if QA_BUILD
            { "lv", "Latviešu" }, // Latvian
#endif
            { "lt", "Lietuvių" }, // Lithuanian
            { "ms", "Malay" }, // Malay
            { "nl", "Nederlands" }, // Dutch
#if QA_BUILD
            { "et", "Eesti Keel" }, // Estonian
            { "no", "Bokmål" }, // Norwegian
#endif
            { "pl", "Polski" }, // Polish
            { "pt-BR", "Português (Brasil)" }, // Portuguese, Brazilian
            { "ro", "Română" }, // Romanian
            { "ru", "Русский" }, // Russian
            { "sv-SE", "Svenska" }, // Swedish
            { "th", "ภาษาไทย" }, // Thai
            { "tr", "Türkçe" }, // Turkish
            { "uk", "Українська" }, // Ukrainian
            { "vi", "Tiếng Việt" }, // Vietnamese
            { "zh-CN", "中文 (简体)" }, // Chinese Simplified
#if QA_BUILD
            { "zh-HK", "中文 (廣東話)" }, // Chinese Traditional, Hong Kong
#endif
            { "zh-TW", "中文 (繁體)" } // Chinese Traditional
        };

        public static string GetIdentifierFromName(string language) => SupportedLocales.FirstOrDefault(x => x.Value == language).Key ?? "nil";

        public static List<string> GetLanguages()
        {
            var languages = new List<string>();
            
            languages.AddRange(SupportedLocales.Values.Take(3));
            languages.AddRange(SupportedLocales.Values.Where(x => !languages.Contains(x)).OrderBy(x => x));
            languages[0] = Strings.Common_SystemDefault; // set again for any locale changes

            return languages;
        }

        public static void Set(string identifier)
        {
            if (!SupportedLocales.ContainsKey(identifier))
                identifier = "nil";

            if (identifier == "nil")
            {
                CurrentCulture = Thread.CurrentThread.CurrentUICulture;
            }
            else
            {
                CurrentCulture = new CultureInfo(identifier);

                CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;
                Thread.CurrentThread.CurrentUICulture = CurrentCulture;
            }

            RightToLeft = _rtlLocales.Any(CurrentCulture.Name.StartsWith);
        }

        public static void Initialize()
        {
            Set("nil");

            // https://supportcenter.devexpress.com/ticket/details/t905790/is-there-a-way-to-set-right-to-left-mode-in-wpf-for-the-whole-application
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, _) =>
            {
                var window = (Window)sender;

                if (RightToLeft)
                {
                    window.FlowDirection = FlowDirection.RightToLeft;

                    if (window.ContextMenu is not null)
                        window.ContextMenu.FlowDirection = FlowDirection.RightToLeft;
                }
                else if (CurrentCulture.Name.StartsWith("th"))
                {
                    window.FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/Resources/Fonts/"), "./#Noto Sans Thai");
                }

#if QA_BUILD
                window.BorderBrush = System.Windows.Media.Brushes.Red;
                window.BorderThickness = new Thickness(4);
#endif
            }));
        }
    }
}
