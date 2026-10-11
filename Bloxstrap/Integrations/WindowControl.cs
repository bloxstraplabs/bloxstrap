using System.Runtime.InteropServices;
using System.Windows.Forms;

using Bloxstrap.Models.BloxstrapRPC;

using Message = Bloxstrap.Models.BloxstrapRPC.Message;

namespace Bloxstrap.Integrations
{
    public class WindowControl : IDisposable
    {
        // Native

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const uint WM_SETTEXT = 0x000C;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_LAYERED = 0x00080000L;
        private const long WS_EX_TRANSPARENT = 0x00000020L;
        private const long WS_BORDER = 0x00800000L;
        private const long WS_CAPTION = 0x00C00000L;
        private const long WS_THICKFRAME = 0x00040000L;
        private const long WS_SYSMENU = 0x00080000L;
        private const long WS_MINIMIZEBOX = 0x00020000L;
        private const long WS_MAXIMIZEBOX = 0x00010000L;
        private const uint LWA_COLORKEY = 0x00000001;
        private const uint LWA_ALPHA = 0x00000002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOSENDCHANGING = 0x0400;
        private const uint SWP_ASYNCWINDOWPOS = 0x4000;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_SHOWNA = 8;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr value);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attribute, ref uint value, int size);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string className, string? windowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childAfter, string className, string? windowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // Properties

        private const string PermissionsFolder = "content\\bloxstrap";

        private static readonly string[] AllowedWithoutPermission = { "SetWindowTitle" };

        private readonly ActivityWatcher _activityWatcher;

        private readonly int _processId;

        private readonly string _robloxDirectory;

        private readonly JsonManager<WindowPermissions> _permissions = new();

        private IntPtr _window;

        private long _originalExStyle;

        private bool _gameAllowed;

        private long _lastPrompted;

        private bool _changedWindow;

        private bool _changedWallpaper;

        private bool? _desktopIconsVisible;

        private int _screenWidth, _screenHeight, _monitorX, _monitorY;

        private float _widthMultiplier = 1, _heightMultiplier = 1;

        private int _scaleWidth = 1280, _scaleHeight = 720;

        private int _x, _y, _width, _height;

        private int _startX, _startY, _startWidth, _startHeight;

        private int _setX = -1, _setY = -1, _setWidth = -1, _setHeight = -1;

        private ulong _wallpaperAsset;

        public bool Active { get; private set; }

        public bool TitleChanged { get; private set; }

        public Action<string, string, int>? ShowNotification { get; set; }

        public static string GetPermissionsPath(string robloxDirectory) => Path.Combine(robloxDirectory, PermissionsFolder);

        public WindowControl(ActivityWatcher activityWatcher, int processId, string robloxDirectory)
        {
            _activityWatcher = activityWatcher;
            _processId = processId;
            _robloxDirectory = robloxDirectory;

            _permissions.Load(false);

            WriteEnabledMarker(robloxDirectory);

            _activityWatcher.OnRPCMessage += (_, message) => OnMessage(message);
            _activityWatcher.OnGameJoin += (_, _) => UpdateExposedPermissions();
            _activityWatcher.OnGameLeave += (_, _) =>
            {
                _lastPrompted = 0;
                Stop();
            };
        }

        public static void WriteEnabledMarker(string robloxDirectory)
        {
            try
            {
                string folder = GetPermissionsPath(robloxDirectory);
                Directory.CreateDirectory(folder);

                using var bitmap = new System.Drawing.Bitmap(1, 1);
                bitmap.SetPixel(0, 0, System.Drawing.Color.White);
                bitmap.Save(Path.Combine(folder, "enabled.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("WindowControl::WriteEnabledMarker", ex);
            }
        }

        private bool IsAllowed(long universeId) => App.Settings.Prop.WindowControlAllGames || _permissions.Prop.Allowed.Contains(universeId);

        public void UpdateExposedPermissions()
        {
            const string LOG_IDENT = "WindowControl::UpdateExposedPermissions";

            try
            {
                string folder = GetPermissionsPath(_robloxDirectory);
                Directory.CreateDirectory(folder);

                foreach (var file in new DirectoryInfo(folder).GetFiles().Where(x => x.Name != "enabled.png"))
                    file.Delete();

                long universeId = _activityWatcher.Data.UniverseId;
                _gameAllowed = IsAllowed(universeId);

                if (!_gameAllowed || universeId == 0)
                    return;

                var settings = App.Settings.Prop;
                bool[] permissions =
                {
                    settings.WindowControlMove,
                    settings.WindowControlTitle,
                    settings.WindowControlTransparency,
                    settings.WindowControlColor,
                    settings.WindowControlWallpaper,
                    settings.WindowControlDesktop
                };

                using var bitmap = new System.Drawing.Bitmap(permissions.Length, 1);

                for (int i = 0; i < permissions.Length; i++)
                    bitmap.SetPixel(i, 0, permissions[i] ? System.Drawing.Color.White : System.Drawing.Color.Transparent);

                bitmap.Save(Path.Combine(folder, $"{universeId}.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private void RequestPermission()
        {
            long universeId = _activityWatcher.Data.UniverseId;

            if (universeId == 0 || IsAllowed(universeId) || _permissions.Prop.Blocked.Contains(universeId) || _lastPrompted == universeId)
                return;

            _lastPrompted = universeId;

            string gameName = _activityWatcher.Data.UniverseDetails?.Data.Name ?? universeId.ToString();

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var dialog = new UI.Elements.Dialogs.WindowPermissionDialog(gameName);
                dialog.ShowDialog();

                _permissions.Load(false);

                if (dialog.Allowed)
                {
                    if (!_permissions.Prop.Allowed.Contains(universeId))
                        _permissions.Prop.Allowed.Add(universeId);
                }
                else if (dialog.NeverAsk)
                {
                    if (!_permissions.Prop.Blocked.Contains(universeId))
                        _permissions.Prop.Blocked.Add(universeId);
                }

                _permissions.Save();
                UpdateExposedPermissions();
            });
        }

        private bool FindRobloxWindow()
        {
            if (_window != IntPtr.Zero)
                return true;

            try
            {
                using var process = Process.GetProcessById(_processId);
                _window = process.MainWindowHandle;
            }
            catch (ArgumentException)
            {
                _window = IntPtr.Zero;
            }

            if (_window == IntPtr.Zero)
                return false;

            _originalExStyle = GetWindowLongPtr(_window, GWL_EXSTYLE).ToInt64();
            SaveWindow();

            return true;
        }

        private void UpdateMonitor()
        {
            if (App.Settings.Prop.WindowControlMonitor == WindowMonitorMode.All)
            {
                var virtualScreen = SystemInformation.VirtualScreen;
                var primary = Screen.PrimaryScreen!.Bounds;

                _screenWidth = virtualScreen.Width;
                _screenHeight = virtualScreen.Height;
                _monitorX = virtualScreen.X;
                _monitorY = virtualScreen.Y;
                _widthMultiplier = primary.Width / (float)_screenWidth;
                _heightMultiplier = primary.Height / (float)_screenHeight;
                return;
            }

            var bounds = Screen.FromHandle(_window).Bounds;

            _screenWidth = bounds.Width;
            _screenHeight = bounds.Height;
            _monitorX = bounds.X;
            _monitorY = bounds.Y;
            _widthMultiplier = 1;
            _heightMultiplier = 1;
        }

        private void SaveWindow()
        {
            GetWindowRect(_window, out var rect);

            _x = _startX = _setX = rect.Left;
            _y = _startY = _setY = rect.Top;
            _width = _startWidth = _setWidth = rect.Right - rect.Left;
            _height = _startHeight = _setHeight = rect.Bottom - rect.Top;

            UpdateMonitor();
        }

        private void MoveWindow(int x, int y, int width, int height)
        {
            bool moved = x != _setX || y != _setY;
            bool resized = width != _setWidth || height != _setHeight;

            if (!moved && !resized)
                return;

            _setX = x;
            _setY = y;
            _setWidth = width;
            _setHeight = height;

            uint flags = SWP_NOZORDER | SWP_ASYNCWINDOWPOS | SWP_NOSENDCHANGING;

            if (!moved)
                flags |= SWP_NOMOVE;

            if (!resized)
                flags |= SWP_NOSIZE;

            SetWindowPos(_window, IntPtr.Zero, x, y, width, height, flags);
        }

        private void SetBorderless(bool borderless)
        {
            long style = GetWindowLongPtr(_window, GWL_STYLE).ToInt64();
            long frame = WS_CAPTION | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

            style = borderless ? style & ~(frame | WS_BORDER) : style | frame;

            SetWindowLongPtr(_window, GWL_STYLE, new IntPtr(style));
            SetWindowPos(_window, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_ASYNCWINDOWPOS | SWP_NOSENDCHANGING | SWP_FRAMECHANGED);
        }

        private void SetColor(int attribute, string hex)
        {
            uint value = Convert.ToUInt32(hex.TrimStart('#'), 16);
            uint disableDarkMode = 0;

            if (DwmSetWindowAttribute(_window, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref disableDarkMode, sizeof(uint)) != 0)
                DwmSetWindowAttribute(_window, DWMWA_USE_IMMERSIVE_DARK_MODE, ref disableDarkMode, sizeof(uint));

            DwmSetWindowAttribute(_window, attribute, ref value, sizeof(uint));
        }

        private void ResetColors()
        {
            bool dark = App.Settings.Prop.Theme.GetFinal() == Theme.Dark;

            SetColor(DWMWA_CAPTION_COLOR, dark ? "1F1F1F" : "FFFFFF");
            SetColor(DWMWA_BORDER_COLOR, "1F1F1F");
        }

        private void Stop()
        {
            Active = false;
            _activityWatcher.PollDelay = 1000;

            if (_window == IntPtr.Zero)
                return;

            if (_changedWindow)
            {
                MoveWindow(_startX, _startY, _startWidth, _startHeight);
                SetWindowLongPtr(_window, GWL_EXSTYLE, new IntPtr(_originalExStyle));
                SetBorderless(false);
                _changedWindow = false;
            }

            if (TitleChanged)
            {
                SendMessage(_window, WM_SETTEXT, IntPtr.Zero, "Roblox");
                TitleChanged = false;
            }

            if (App.Settings.Prop.WindowControlColor)
                ResetColors();

            SetTaskbarVisible(true);
            SetDesktopIconsVisible(true);

            if (_changedWallpaper)
            {
                WallpaperControl.Reset();
                _changedWallpaper = false;
            }
        }

        private static T? Read<T>(Message message)
        {
            try
            {
                return message.Data.Deserialize<T>();
            }
            catch
            {
                return default;
            }
        }

        private void OnMessage(Message message)
        {
            const string LOG_IDENT = "WindowControl::OnMessage";

            var settings = App.Settings.Prop;

            if (message.Command == "RequestWindowPermission")
            {
                RequestPermission();
                return;
            }

            if (!_gameAllowed && !AllowedWithoutPermission.Contains(message.Command))
                return;

            if (!FindRobloxWindow())
                return;

            try
            {
                switch (message.Command)
                {
                    case "StartWindow":
                        Active = true;
                        _activityWatcher.PollDelay = Math.Max(1000 / Math.Clamp(settings.WindowControlFps, 1, 240), 1);
                        SaveWindow();
                        break;

                    case "StopWindow":
                        Stop();
                        break;

                    case "ResetWindow":
                        _x = _startX;
                        _y = _startY;
                        _width = _startWidth;
                        _height = _startHeight;
                        MoveWindow(_startX, _startY, _startWidth, _startHeight);
                        break;

                    case "SetWindow":
                    {
                        if (!settings.WindowControlMove || Read<WindowMessage>(message) is not WindowMessage data)
                            break;

                        if (data.Reset == true)
                        {
                            Stop();
                            break;
                        }

                        if (!_changedWindow)
                            SaveWindow();

                        if (data.ScaleWidth > 0)
                            _scaleWidth = data.ScaleWidth.Value;

                        if (data.ScaleHeight > 0)
                            _scaleHeight = data.ScaleHeight.Value;

                        float scaleX = _screenWidth / (float)_scaleWidth;
                        float scaleY = _screenHeight / (float)_scaleHeight;

                        if (data.Width is int width)
                            _width = (int)(width * scaleX);

                        if (data.Height is int height)
                            _height = (int)(height * scaleY);

                        if (data.X is int x)
                            _x = (int)(x * scaleX + (_width - _width * _widthMultiplier) / 2);

                        if (data.Y is int y)
                            _y = (int)(y * scaleY + (_height - _height * _heightMultiplier) / 2);

                        _changedWindow = true;
                        MoveWindow(_x + _monitorX, _y + _monitorY, (int)(_width * _widthMultiplier), (int)(_height * _heightMultiplier));
                        break;
                    }

                    case "SetWindowTitle":
                    {
                        if (!settings.WindowControlTitle)
                            break;

                        string title = Read<string>(message) ?? "Roblox";

                        SendMessage(_window, WM_SETTEXT, IntPtr.Zero, title.Length > 256 ? title[..256] : title);
                        TitleChanged = title != "Roblox";
                        break;
                    }

                    case "SetWindowTransparency":
                    {
                        if (!settings.WindowControlTransparency || Read<WindowTransparencyMessage>(message) is not WindowTransparencyMessage data)
                            break;

                        byte alpha = (byte)(Math.Clamp(data.Transparency ?? 1, 0, 1) * 255);
                        uint color = data.Color is null ? 0 : Convert.ToUInt32(data.Color.TrimStart('#'), 16);
                        uint mode = data.UseAlpha == true ? LWA_ALPHA : LWA_COLORKEY;

                        _changedWindow = true;

                        if (alpha == 255)
                        {
                            SetWindowLongPtr(_window, GWL_EXSTYLE, new IntPtr(_originalExStyle));
                        }
                        else
                        {
                            SetWindowLongPtr(_window, GWL_EXSTYLE, new IntPtr((_originalExStyle | WS_EX_LAYERED) & ~WS_EX_TRANSPARENT));
                            SetLayeredWindowAttributes(_window, color, alpha, mode);
                        }

                        break;
                    }

                    case "SetWindowBorderless":
                    {
                        if (!settings.WindowControlMove)
                            break;

                        SetBorderless(Read<bool>(message));
                        _changedWindow = true;
                        break;
                    }

                    case "SetWindowColor":
                    {
                        if (!settings.WindowControlColor || Read<WindowColorMessage>(message) is not WindowColorMessage data)
                            break;

                        if (data.Reset == true)
                        {
                            ResetColors();
                            break;
                        }

                        if (data.Caption is not null)
                            SetColor(DWMWA_CAPTION_COLOR, data.Caption);

                        if (data.Border is not null)
                            SetColor(DWMWA_BORDER_COLOR, data.Border);

                        break;
                    }

                    case "CacheWallpaper":
                    {
                        if (!settings.WindowControlWallpaper || Read<ulong?>(message) is not ulong assetId || assetId == 0)
                            break;

                        _ = RobloxAssets.GetImage(assetId).ContinueWith(task =>
                        {
                            if (task.Exception is not null)
                                App.Logger.WriteException(LOG_IDENT, task.Exception);
                        });

                        break;
                    }

                    case "SetWallpaper":
                    {
                        if (!settings.WindowControlWallpaper || Read<WallpaperMessage>(message) is not WallpaperMessage data)
                            break;

                        if (data.Reset == true || data.AssetId is not ulong assetId || assetId == 0)
                        {
                            if (_changedWallpaper)
                                WallpaperControl.Reset();

                            _changedWallpaper = false;
                            break;
                        }

                        _wallpaperAsset = assetId;

                        _ = RobloxAssets.GetImage(assetId).ContinueWith(task =>
                        {
                            if (task.Exception is not null)
                            {
                                App.Logger.WriteException(LOG_IDENT, task.Exception);
                                return;
                            }

                            if (_wallpaperAsset != assetId || !Active && !_gameAllowed)
                                return;

                            WallpaperControl.Set(task.Result, data.Style);
                            _changedWallpaper = true;
                        });

                        break;
                    }

                    case "SetDesktopVisibility":
                    {
                        if (!settings.WindowControlDesktop || Read<DesktopVisibilityMessage>(message) is not DesktopVisibilityMessage data)
                            break;

                        if (data.Taskbar is bool taskbar)
                            SetTaskbarVisible(taskbar);

                        if (data.DesktopIcons is bool icons)
                            SetDesktopIconsVisible(icons);

                        break;
                    }

                    case "SendNotification":
                    {
                        if (Read<NotificationMessage>(message) is not NotificationMessage data)
                            break;

                        ShowNotification?.Invoke(data.Title ?? "", data.Caption ?? "", Math.Clamp(data.Duration ?? 5, 1, 30));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to run {message.Command}");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static void SetTaskbarVisible(bool visible)
        {
            IntPtr taskbar = FindWindow("Shell_TrayWnd", null);

            if (taskbar != IntPtr.Zero)
                ShowWindow(taskbar, visible ? SW_SHOW : SW_HIDE);

            IntPtr secondary = IntPtr.Zero;

            while ((secondary = FindWindowEx(IntPtr.Zero, secondary, "Shell_SecondaryTrayWnd", null)) != IntPtr.Zero)
                ShowWindow(secondary, visible ? SW_SHOW : SW_HIDE);
        }

        private void SetDesktopIconsVisible(bool visible)
        {
            if (_desktopIconsVisible == false)
                return;

            IntPtr view = FindWindowEx(FindWindow("Progman", null), IntPtr.Zero, "SHELLDLL_DefView", null);

            if (view == IntPtr.Zero)
            {
                IntPtr worker = IntPtr.Zero;

                do
                {
                    worker = FindWindowEx(IntPtr.Zero, worker, "WorkerW", null);
                    view = FindWindowEx(worker, IntPtr.Zero, "SHELLDLL_DefView", null);
                }
                while (worker != IntPtr.Zero && view == IntPtr.Zero);
            }

            IntPtr icons = view == IntPtr.Zero ? IntPtr.Zero : FindWindowEx(view, IntPtr.Zero, "SysListView32", "FolderView");

            if (icons == IntPtr.Zero)
                return;

            _desktopIconsVisible ??= IsWindowVisible(icons);

            if (_desktopIconsVisible == true)
                ShowWindow(icons, visible ? SW_SHOWNA : SW_HIDE);
        }

        public void Dispose()
        {
            Stop();

            try
            {
                string folder = GetPermissionsPath(_robloxDirectory);

                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("WindowControl::Dispose", ex);
            }

            RobloxAssets.ClearCache();

            GC.SuppressFinalize(this);
        }
    }
}
