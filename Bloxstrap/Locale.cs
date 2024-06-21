using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            if (identifier == "ar" || identifier == "he")
            {
                // TODO: credit the SO post i took this from
                EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler((window, _) =>
                {
                    ((Window)window).FlowDirection = FlowDirection.RightToLeft;
                }));
            }
        }
    }
}
