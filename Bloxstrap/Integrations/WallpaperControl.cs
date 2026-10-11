using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace Bloxstrap.Integrations
{
    public static class WallpaperControl
    {
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        private const string DesktopKey = @"Control Panel\Desktop";
        private const string HistoryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers";
        private const int HistoryEntries = 5;

        private static readonly string[] WallpaperApps = { "wallpaper32", "wallpaper64", "Lively", "LivelyUI", "Livelywpf" };

        private static readonly Dictionary<string, (string Style, string Tile)> Styles = new()
        {
            ["fill"] = ("10", "0"),
            ["fit"] = ("6", "0"),
            ["stretch"] = ("2", "0"),
            ["tile"] = ("0", "1"),
            ["center"] = ("0", "0"),
            ["span"] = ("22", "0")
        };

        private class SavedState
        {
            public string Wallpaper = "";
            public string Style = "10";
            public string Tile = "0";
            public string[] History = new string[HistoryEntries];
        }

        private static SavedState? _saved;

        private static readonly List<string> _closedApps = new();

        public static void Set(string path, string? style)
        {
            const string LOG_IDENT = "WallpaperControl::Set";

            try
            {
                if (_saved is null)
                {
                    Backup();
                    CloseWallpaperApps();
                }

                Apply(path, (style ?? "fill").ToLowerInvariant());
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public static void Reset()
        {
            const string LOG_IDENT = "WallpaperControl::Reset";

            if (_saved is null)
                return;

            try
            {
                var saved = _saved;

                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, saved.Wallpaper, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                using (var key = Registry.CurrentUser.OpenSubKey(DesktopKey, true))
                {
                    key?.SetValue("WallpaperStyle", saved.Style);
                    key?.SetValue("TileWallpaper", saved.Tile);
                }

                RestoreHistory();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
            finally
            {
                _saved = null;
                RestartWallpaperApps();
            }
        }

        private static void Apply(string path, string style)
        {
            if (!Styles.TryGetValue(style, out var values))
                values = Styles["fill"];

            using (var key = Registry.CurrentUser.OpenSubKey(DesktopKey, true))
            {
                key?.SetValue("WallpaperStyle", values.Style);
                key?.SetValue("TileWallpaper", values.Tile);
            }

            if (!SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE))
                App.Logger.WriteLine("WallpaperControl::Apply", $"SystemParametersInfo failed ({Marshal.GetLastWin32Error()})");

            RestoreHistory();
        }

        private static void Backup()
        {
            var state = new SavedState();

            using (var history = Registry.CurrentUser.OpenSubKey(HistoryKey))
            {
                for (int i = 0; i < HistoryEntries; i++)
                    state.History[i] = history?.GetValue($"BackgroundHistoryPath{i}") as string ?? "";
            }

            using (var desktop = Registry.CurrentUser.OpenSubKey(DesktopKey))
            {
                state.Wallpaper = desktop?.GetValue("WallPaper") as string ?? state.History[0];
                state.Style = desktop?.GetValue("WallpaperStyle") as string ?? "10";
                state.Tile = desktop?.GetValue("TileWallpaper") as string ?? "0";
            }

            _saved = state;
        }

        private static void RestoreHistory()
        {
            if (_saved is null)
                return;

            using var history = Registry.CurrentUser.OpenSubKey(HistoryKey, true);

            for (int i = 0; i < HistoryEntries; i++)
            {
                if (!String.IsNullOrEmpty(_saved.History[i]))
                    history?.SetValue($"BackgroundHistoryPath{i}", _saved.History[i], RegistryValueKind.String);
            }
        }

        private static void CloseWallpaperApps()
        {
            foreach (string name in WallpaperApps)
            {
                foreach (var process in Process.GetProcessesByName(name))
                {
                    try
                    {
                        string? exe = process.MainModule?.FileName;

                        if (!String.IsNullOrEmpty(exe) && !_closedApps.Contains(exe))
                            _closedApps.Add(exe);

                        process.CloseMainWindow();

                        if (!process.WaitForExit(3000))
                            process.Kill();
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteException("WallpaperControl::CloseWallpaperApps", ex);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
        }

        private static void RestartWallpaperApps()
        {
            foreach (string exe in _closedApps.Where(File.Exists))
            {
                try
                {
                    Process.Start(exe);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("WallpaperControl::RestartWallpaperApps", ex);
                }
            }

            _closedApps.Clear();
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    }
}
