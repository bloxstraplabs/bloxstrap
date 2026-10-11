using System.Windows.Media;
using System.Windows.Media.Imaging;

using Bloxstrap.AppData;

namespace Bloxstrap
{
    public static class ModGenerator
    {
        public const string CursorFolder = @"content\textures\Cursors\KeyboardMouse";

        public static readonly string[] CursorFiles =
        {
            $@"{CursorFolder}\ArrowCursor.png",
            $@"{CursorFolder}\ArrowFarCursor.png"
        };

        public const string ShiftLockFile = @"content\textures\MouseLockedCursor.png";

        public const string DeathSoundFile = @"content\sounds\ouch.ogg";

        public static readonly string[] EmoteWheelFiles = new[]
        {
            "SelectedGradient.png", "SelectedGradient@2x.png", "SelectedGradient@3x.png",
            "SelectedLine.png", "SelectedLine@2x.png", "SelectedLine@3x.png"
        }.Select(x => $@"content\textures\ui\Emotes\Large\{x}").ToArray();

        public static IEnumerable<string> CursorSetFiles => CursorFiles.Append(ShiftLockFile);

        private static string OriginalsFolder => Path.Combine(Paths.Base, "ModGenerator", "Originals");

        public static string CursorSetsFolder => Path.Combine(Paths.Base, "CursorSets");

        public static bool TryParseColor(string hex, out Color color)
        {
            color = default;
            hex = hex.Trim();

            if (!hex.StartsWith('#'))
                hex = '#' + hex;

            if (hex.Length != 7)
                return false;

            try
            {
                color = (Color)ColorConverter.ConvertFromString(hex);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string? GetOriginal(string relativePath)
        {
            string cached = Path.Combine(OriginalsFolder, relativePath);

            if (File.Exists(cached))
                return cached;

            var playerData = new RobloxPlayerData();
            string installed = Path.Combine(playerData.Directory, relativePath);

            if (!File.Exists(installed) || playerData.DistributionState.ModManifest.Contains(relativePath))
                return null;

            Directory.CreateDirectory(Path.GetDirectoryName(cached)!);
            File.Copy(installed, cached, true);

            return cached;
        }

        public static int Recolor(IEnumerable<string> relativePaths, Color color)
        {
            const string LOG_IDENT = "ModGenerator::Recolor";

            int written = 0;

            foreach (string relativePath in relativePaths)
            {
                string? source = GetOriginal(relativePath);

                if (source is null)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Could not find an unmodified copy of {relativePath}");
                    continue;
                }

                string target = Path.Combine(Paths.Modifications, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                Filesystem.AssertReadOnly(target);

                TintImage(source, target, color);
                written++;

                App.Logger.WriteLine(LOG_IDENT, $"Wrote {relativePath}");
            }

            return written;
        }

        public static void RemoveGenerated(IEnumerable<string> relativePaths)
        {
            foreach (string target in relativePaths.Select(x => Path.Combine(Paths.Modifications, x)).Where(File.Exists))
            {
                Filesystem.AssertReadOnly(target);
                File.Delete(target);
            }
        }

        private static void TintImage(string source, string target, Color color)
        {
            BitmapSource bitmap;

            using (var stream = File.OpenRead(source))
            {
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                bitmap = new FormatConvertedBitmap(decoder.Frames[0], PixelFormats.Bgra32, null, 0);
            }

            int stride = bitmap.PixelWidth * 4;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                double luminance = (pixels[i] * 0.114 + pixels[i + 1] * 0.587 + pixels[i + 2] * 0.299) / 255;

                pixels[i]     = (byte)(color.B * luminance);
                pixels[i + 1] = (byte)(color.G * luminance);
                pixels[i + 2] = (byte)(color.R * luminance);
            }

            var result = BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null, pixels, stride);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(result));

            using var output = File.Create(target);
            encoder.Save(output);
        }

        public static IEnumerable<string> GetCursorSets()
        {
            if (!Directory.Exists(CursorSetsFolder))
                return Enumerable.Empty<string>();

            return Directory.GetDirectories(CursorSetsFolder).Select(x => Path.GetFileName(x)!).OrderBy(x => x);
        }

        public static bool SaveCursorSet(string name)
        {
            string folder = Path.Combine(CursorSetsFolder, name);
            bool any = false;

            if (Directory.Exists(folder))
                Directory.Delete(folder, true);

            foreach (string relativePath in CursorSetFiles)
            {
                string source = Path.Combine(Paths.Modifications, relativePath);

                if (!File.Exists(source))
                    continue;

                Directory.CreateDirectory(folder);
                File.Copy(source, Path.Combine(folder, Path.GetFileName(relativePath)), true);
                any = true;
            }

            return any;
        }

        public static void ApplyCursorSet(string name)
        {
            string folder = Path.Combine(CursorSetsFolder, name);

            foreach (string relativePath in CursorSetFiles)
            {
                string source = Path.Combine(folder, Path.GetFileName(relativePath));
                string target = Path.Combine(Paths.Modifications, relativePath);

                Filesystem.AssertReadOnly(target);

                if (File.Exists(source))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    File.Copy(source, target, true);
                }
                else if (File.Exists(target))
                {
                    File.Delete(target);
                }
            }
        }

        public static void DeleteCursorSet(string name) => Directory.Delete(Path.Combine(CursorSetsFolder, name), true);
    }
}
