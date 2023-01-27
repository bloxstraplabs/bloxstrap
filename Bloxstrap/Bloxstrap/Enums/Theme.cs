using Microsoft.Win32;

namespace Bloxstrap.Enums
{
    public enum Theme
    {
        Default,
        Light,
        Dark
    }

    public static class DialogThemeEx
    {
        public static Theme GetFinal(this Theme dialogTheme)
        {
            if (dialogTheme != Theme.Default)
                return dialogTheme;

            RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");

            if (key is not null)
            {
                var value = key.GetValue("AppsUseLightTheme");

                if (value is not null && (int)value == 0)
                    return Theme.Dark;
            }

            return Theme.Light;
        }
    }
}
