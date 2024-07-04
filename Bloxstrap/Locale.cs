using System.Windows;

using Bloxstrap.Resources;

namespace Bloxstrap
{
    internal static class Locale
    {
        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.InvariantCulture;

        public static bool RightToLeft { get; private set; } = false;

        public static readonly Dictionary<string, string> SupportedLocales = new()
        {
            { "nil", Strings.Common_SystemDefault },
            { "en", "English" },
            { "en-US", "English (United States)" },
            { "ar", "العربية" },
            { "bn", "বাংলা" },
            { "bs", "Босански" },
            { "bg", "Български" },
            { "zh-CN", "中文 (简体)" },
            { "zh-HK", "中文 (廣東話)" },
            { "zh-TW", "中文 (繁體)" },
            { "cs", "Čeština" },
            // { "dk", "Dansk" },
            { "nl", "Nederlands" },
            { "fl", "Filipino" },
            { "fi", "Suomi" },
            { "fr", "Français" },
            { "de", "Deutsch" },
            { "he", "עברית‎" },
            { "hr", "Hrvatski" },
            // { "hi", "हिन्दी" },
            { "hu", "Magyar" },
            // { "id", "Bahasa Indonesia" },
            // { "it", "Italiano" },
            { "ja", "日本語" },
            { "ko", "한국어" },
            { "lt", "Lietuvių" },
            // { "no", "Bokmål" },
            { "pl", "Polski" },
            { "pt-BR", "Português (Brasil)" },
            { "ro", "Română" },
            { "ru", "Русский" },
            { "es", "Español" },
            { "sv-SE", "Svenska" },
            { "th", "ภาษาไทย" },
            { "tr", "Türkçe" },
            { "uk", "Yкраїньска" },
            { "vi", "Tiếng Việt" }
        };

        public static string GetIdentifierFromName(string language) => SupportedLocales.FirstOrDefault(x => x.Value == language).Key ?? "nil";

        public static List<string> GetLanguages()
        {
            var languages = new List<string>();
            
            languages.AddRange(SupportedLocales.Values.Take(3));
            languages.AddRange(SupportedLocales.Values.Where(x => !languages.Contains(x)).OrderBy(x => x));

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

            RightToLeft = CurrentCulture.Name.StartsWith("ar") || CurrentCulture.Name.StartsWith("he");
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
            }));
        }
    }
}
