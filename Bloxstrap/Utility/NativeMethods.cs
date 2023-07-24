using System.Runtime.InteropServices;

namespace Bloxstrap.Utility
{
    static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")] 
        public static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // i only bothered to add the constants that im using lol

        public const int GWL_EXSTYLE = -20;

        public const int WS_EX_TOOLWINDOW = 0x00000080;
    }
}
