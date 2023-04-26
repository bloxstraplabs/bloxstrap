using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Bloxstrap.Extensions
{
    public static class IconEx
    {
        public static Icon GetSized(this Icon icon, int width, int height) => new(icon, new Size(width, height));

        public static ImageSource GetImageSource(this Icon icon)
        {
            using MemoryStream stream = new();
            icon.Save(stream);
            return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }
    }
}
