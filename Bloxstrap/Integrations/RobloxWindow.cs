using System.Runtime.InteropServices;

namespace Bloxstrap.Integrations
{
    public class RobloxWindow
    {
        // Native

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public int Width => Right - Left;

            public int Height => Bottom - Top;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X, Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;
        }

        private const int GWL_STYLE = -16;
        private const long WS_CAPTION = 0x00C00000;
        private const long WS_THICKFRAME = 0x00040000;
        private const long WS_MINIMIZEBOX = 0x00020000;
        private const long WS_MAXIMIZEBOX = 0x00010000;
        private const long WS_SYSMENU = 0x00080000;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO info);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        // Properties

        private readonly int _processId;

        public IntPtr Handle { get; private set; }

        public bool IsValid => Handle != IntPtr.Zero && IsWindow(Handle);

        public bool IsForeground => IsValid && GetForegroundWindow() == Handle && !IsIconic(Handle);

        public RobloxWindow(int processId)
        {
            _processId = processId;
        }

        // Functions

        public bool Refresh()
        {
            if (IsValid)
                return true;

            try
            {
                using var process = Process.GetProcessById(_processId);
                Handle = process.MainWindowHandle;
            }
            catch (ArgumentException)
            {
                Handle = IntPtr.Zero;
            }

            return IsValid;
        }

        public RECT? GetClientScreenRect()
        {
            if (!IsValid || !GetClientRect(Handle, out var client))
                return null;

            var origin = new POINT();

            if (!ClientToScreen(Handle, ref origin))
                return null;

            return new RECT { Left = origin.X, Top = origin.Y, Right = origin.X + client.Width, Bottom = origin.Y + client.Height };
        }

        public void ApplyTitle(string title)
        {
            if (!IsValid || String.IsNullOrEmpty(title))
                return;

            var current = new StringBuilder(256);
            GetWindowText(Handle, current, current.Capacity);

            if (current.ToString() != title)
                SetWindowText(Handle, title);
        }

        public void ApplyBorderless()
        {
            if (!IsValid || IsIconic(Handle))
                return;

            long style = GetWindowLongPtr(Handle, GWL_STYLE).ToInt64();

            if ((style & WS_CAPTION) == 0)
                return;

            long frame = WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU;
            SetWindowLongPtr(Handle, GWL_STYLE, new IntPtr(style & ~frame));

            var info = new MONITORINFO { Size = Marshal.SizeOf<MONITORINFO>() };

            if (GetMonitorInfo(MonitorFromWindow(Handle, MONITOR_DEFAULTTONEAREST), ref info))
            {
                var monitor = info.Monitor;
                SetWindowPos(Handle, IntPtr.Zero, monitor.Left, monitor.Top, monitor.Width, monitor.Height, SWP_FRAMECHANGED | SWP_NOZORDER);
            }
        }
    }
}
