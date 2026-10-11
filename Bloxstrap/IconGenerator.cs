using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bloxstrap
{
    public static class IconGenerator
    {
        public record Stop(double Offset, Color Color);

        private static readonly string[] SheetFolders =
        {
            @"ExtraContent\LuaPackages\Packages\_Index\FoundationImages\FoundationImages\SpriteSheets",
            @"ExtraContent\textures\ui\ImageSet"
        };

        public static List<string> FindSheets(string robloxDirectory) => SheetFolders
            .Select(x => Path.Combine(robloxDirectory, x))
            .Where(Directory.Exists)
            .SelectMany(x => Directory.EnumerateFiles(x, "img_set_*.png", SearchOption.AllDirectories))
            .Select(x => Path.GetRelativePath(robloxDirectory, x))
            .ToList();

        public static Color ColorAt(IReadOnlyList<Stop> stops, double t)
        {
            if (stops.Count == 1 || t <= stops[0].Offset)
                return stops[0].Color;

            for (int i = 1; i < stops.Count; i++)
            {
                if (t > stops[i].Offset)
                    continue;

                var a = stops[i - 1];
                var b = stops[i];
                double f = b.Offset - a.Offset <= 0 ? 0 : (t - a.Offset) / (b.Offset - a.Offset);

                return Color.FromRgb((byte)(a.Color.R + (b.Color.R - a.Color.R) * f), (byte)(a.Color.G + (b.Color.G - a.Color.G) * f), (byte)(a.Color.B + (b.Color.B - a.Color.B) * f));
            }

            return stops[^1].Color;
        }

        private static (byte[] Pixels, int Width, int Height, int Stride) Load(string path)
        {
            using var stream = File.OpenRead(path);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var bitmap = new FormatConvertedBitmap(decoder.Frames[0], PixelFormats.Bgra32, null, 0);

            int stride = bitmap.PixelWidth * 4;
            var pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            return (pixels, bitmap.PixelWidth, bitmap.PixelHeight, stride);
        }

        public static List<Int32Rect> FindIcons(byte[] pixels, int width, int height, int stride)
        {
            const int cell = 2;

            int cols = (width + cell - 1) / cell;
            int rows = (height + cell - 1) / cell;
            var filled = new bool[cols * rows];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (pixels[y * stride + x * 4 + 3] > 8)
                        filled[(y / cell) * cols + x / cell] = true;

            var seen = new bool[filled.Length];
            var icons = new List<Int32Rect>();
            var stack = new Stack<int>();

            for (int start = 0; start < filled.Length; start++)
            {
                if (!filled[start] || seen[start])
                    continue;

                int minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0;
                stack.Push(start);
                seen[start] = true;

                while (stack.Count > 0)
                {
                    int index = stack.Pop();
                    int cx = index % cols, cy = index / cols;

                    minX = Math.Min(minX, cx); maxX = Math.Max(maxX, cx);
                    minY = Math.Min(minY, cy); maxY = Math.Max(maxY, cy);

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = cx + dx, ny = cy + dy;

                            if (nx < 0 || ny < 0 || nx >= cols || ny >= rows)
                                continue;

                            int next = ny * cols + nx;

                            if (filled[next] && !seen[next])
                            {
                                seen[next] = true;
                                stack.Push(next);
                            }
                        }
                    }
                }

                int left = minX * cell, top = minY * cell;
                icons.Add(new Int32Rect(left, top, Math.Min((maxX + 1) * cell, width) - left, Math.Min((maxY + 1) * cell, height) - top));
            }

            return icons;
        }

        private static void Tint(byte[] pixels, int stride, Int32Rect area, IReadOnlyList<Stop> stops, double angle)
        {
            double radians = angle * Math.PI / 180;
            double dx = Math.Cos(radians), dy = Math.Sin(radians);
            double extent = (Math.Abs(dx) + Math.Abs(dy)) / 2;

            for (int y = area.Y; y < area.Y + area.Height; y++)
            {
                for (int x = area.X; x < area.X + area.Width; x++)
                {
                    int i = y * stride + x * 4;

                    if (pixels[i + 3] == 0)
                        continue;

                    double position = ((x - area.X + 0.5) / area.Width - 0.5) * dx + ((y - area.Y + 0.5) / area.Height - 0.5) * dy;
                    var color = ColorAt(stops, extent == 0 ? 0 : Math.Clamp(position / extent / 2 + 0.5, 0, 1));
                    double luminance = Math.Max(pixels[i], Math.Max(pixels[i + 1], pixels[i + 2])) / 255d;

                    pixels[i] = (byte)(color.B * luminance);
                    pixels[i + 1] = (byte)(color.G * luminance);
                    pixels[i + 2] = (byte)(color.R * luminance);
                }
            }
        }

        public static void Recolor(string source, string target, IReadOnlyList<Stop> stops, double angle, bool perIcon)
        {
            var (pixels, width, height, stride) = Load(source);

            var areas = perIcon ? FindIcons(pixels, width, height, stride) : new List<Int32Rect> { new(0, 0, width, height) };

            foreach (var area in areas)
                Tint(pixels, stride, area, stops, angle);

            var result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(result));

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            using var output = File.Create(target);
            encoder.Save(output);
        }

        public static List<BitmapSource> Preview(IEnumerable<string> sheets, IReadOnlyList<Stop> stops, double angle, int count)
        {
            var previews = new List<BitmapSource>();

            foreach (string sheet in sheets)
            {
                if (previews.Count >= count)
                    break;

                AddPreviews(sheet, stops, angle, count, previews);
            }

            return previews;
        }

        private static void AddPreviews(string sheet, IReadOnlyList<Stop> stops, double angle, int count, List<BitmapSource> previews)
        {
            var (pixels, width, height, stride) = Load(sheet);

            foreach (var area in FindIcons(pixels, width, height, stride).Where(x => x.Width >= 16 && x.Height >= 16 && x.Width <= 96 && x.Height <= 96).Take(count - previews.Count))
            {
                Tint(pixels, stride, area, stops, angle);

                var crop = new byte[area.Width * area.Height * 4];

                for (int y = 0; y < area.Height; y++)
                    Buffer.BlockCopy(pixels, (area.Y + y) * stride + area.X * 4, crop, y * area.Width * 4, area.Width * 4);

                var bitmap = BitmapSource.Create(area.Width, area.Height, 96, 96, PixelFormats.Bgra32, null, crop, area.Width * 4);
                bitmap.Freeze();
                previews.Add(bitmap);
            }
        }
    }
}
