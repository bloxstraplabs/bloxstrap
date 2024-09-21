using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Bloxstrap.Extensions
{
    public static class IconEx
    {
        public static Icon GetSized(this Icon icon, int width, int height) => new(icon, new Size(width, height));

        public static ImageSource GetImageSource(this Icon icon)
        {
            const string LOG_IDENT = "IconEx::GetImageSource";

            try
            {
                using MemoryStream stream = new();
                icon.Save(stream);
                return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to get ImageSource");
                App.Logger.WriteException(LOG_IDENT, ex);

                // return fallback image
                return Utilities.GetEmptyBitmap();
            }
        }
    }
}
