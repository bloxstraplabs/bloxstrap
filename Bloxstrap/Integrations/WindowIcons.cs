namespace Bloxstrap.Integrations
{
    public static class WindowIcons
    {
        public static System.Drawing.Icon? Load(RobloxWindowIcon preset, string customPath)
        {
            const string LOG_IDENT = "WindowIcons::Load";

            try
            {
                switch (preset)
                {
                    case RobloxWindowIcon.Default:
                        return null;

                    case RobloxWindowIcon.Custom:
                        return File.Exists(customPath) ? FromImage(File.ReadAllBytes(customPath)) : null;

                    default:
                        var bootstrapperIcon = Enum.Parse<BootstrapperIcon>(preset.ToString());
                        return new System.Drawing.Icon(bootstrapperIcon.GetIcon(), 64, 64);
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }

        public static System.Drawing.Icon FromImage(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var image = System.Drawing.Image.FromStream(stream);
            using var bitmap = new System.Drawing.Bitmap(image, 64, 64);

            return System.Drawing.Icon.FromHandle(bitmap.GetHicon());
        }
    }
}
