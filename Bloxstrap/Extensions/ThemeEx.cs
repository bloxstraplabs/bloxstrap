using Windows.UI.ViewManagement;

namespace Bloxstrap.Extensions
{
    public static class ThemeEx
    {
        public static Theme GetFinal(this Theme dialogTheme)
        {
            if (dialogTheme != Theme.Default)
                return dialogTheme;

            var settings = new UISettings();
            var background = settings.GetColorValue(UIColorType.Background);
            if (((5 * background.G) + (2 * background.R) + background.B) < (8 * 128))
                return Theme.Dark;

            return Theme.Light;
        }
    }
}
