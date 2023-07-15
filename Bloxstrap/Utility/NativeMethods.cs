using System.Runtime.InteropServices;

namespace Bloxstrap.Utility
{
    static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")] 
        public static extern bool FlashWindow(IntPtr hWnd, bool bInvert);
    }
}
