using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Bloxstrap.Extensions
{
    public static class IconEx
    {
        public static Icon GetSized(this Icon icon, int width, int height) => new(icon, new Size(width, height));

        public static ImageSource GetImageSource(this Icon icon, bool handleException = true)
        {
            using MemoryStream stream = new();
            icon.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            if (handleException)
            {
                try
                {
                    var decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    return decoder.Frames
                        .OrderByDescending(f => f.PixelWidth * f.PixelHeight)
                        .First();
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("IconEx::GetImageSource", ex);
                    Frontend.ShowMessageBox(string.Format(Strings.Dialog_IconLoadFailed, ex.Message));
                    return BootstrapperIcon.IconBloxstrap.GetIcon().GetImageSource(false);
                }
            }
            else
            {
                var decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return decoder.Frames
                    .OrderByDescending(f => f.PixelWidth * f.PixelHeight)
                    .First();
            }
        }
    }
}
