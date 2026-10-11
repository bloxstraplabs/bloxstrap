using System.Runtime.InteropServices;

namespace Bloxstrap.Integrations
{
    public sealed class SocdResolver : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const uint WM_QUIT = 0x0012;
        private const uint LLKHF_INJECTED = 0x10;
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const nuint Marker = 0x52455354;

        private sealed class Axis
        {
            public readonly uint KeyA;
            public readonly uint KeyB;
            public readonly List<uint> Held = new();
            public uint Active;

            public Axis(uint keyA, uint keyB)
            {
                KeyA = keyA;
                KeyB = keyB;
            }

            public bool Owns(uint key) => key == KeyA || key == KeyB;
        }

        private readonly int _processId;
        private readonly SocdMode _mode;
        private readonly Axis[] _axes;
        private readonly object _lock = new();

        private Thread? _thread;
        private uint _threadId;
        private IntPtr _hook = IntPtr.Zero;
        private HookProc? _hookProc;
        private bool _disposed;

        public bool Enabled { get; private set; } = true;

        public SocdResolver(int processId, SocdLayout layout, SocdMode mode)
        {
            _processId = processId;
            _mode = mode;

            _axes = layout switch
            {
                SocdLayout.ZQSD => new[] { new Axis(0x51, 0x44), new Axis(0x5A, 0x53) },
                SocdLayout.ESDF => new[] { new Axis(0x53, 0x46), new Axis(0x45, 0x44) },
                SocdLayout.ArrowKeys => new[] { new Axis(0x25, 0x27), new Axis(0x26, 0x28) },
                _ => new[] { new Axis(0x41, 0x44), new Axis(0x57, 0x53) }
            };

            _thread = new Thread(Run) { IsBackground = true, Name = "SOCD" };
            _thread.Start();
        }

        public void SetEnabled(bool enabled)
        {
            lock (_lock)
            {
                Enabled = enabled;

                if (!enabled)
                    Reset();
            }
        }

        private void Run()
        {
            _threadId = GetCurrentThreadId();
            _hookProc = Callback;
            _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(null), 0);

            if (_hook == IntPtr.Zero)
            {
                App.Logger.WriteLine("SocdResolver::Run", $"Failed to install keyboard hook ({Marshal.GetLastWin32Error()})");
                return;
            }

            App.Logger.WriteLine("SocdResolver::Run", $"Keyboard hook installed (mode={_mode})");

            while (GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }

            UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }

        private IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return CallNextHookEx(_hook, nCode, wParam, lParam);

            var data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

            if ((data.flags & LLKHF_INJECTED) != 0 || data.dwExtraInfo == Marker)
                return CallNextHookEx(_hook, nCode, wParam, lParam);

            int message = wParam.ToInt32();
            bool down = message == WM_KEYDOWN || message == WM_SYSKEYDOWN;
            bool up = message == WM_KEYUP || message == WM_SYSKEYUP;

            lock (_lock)
            {
                Axis? axis = null;

                foreach (var candidate in _axes)
                {
                    if (candidate.Owns(data.vkCode))
                    {
                        axis = candidate;
                        break;
                    }
                }

                if (!Enabled || axis is null || (!down && !up))
                    return CallNextHookEx(_hook, nCode, wParam, lParam);

                if (!IsRobloxForeground())
                {
                    Reset();
                    return CallNextHookEx(_hook, nCode, wParam, lParam);
                }

                if (down)
                {
                    if (axis.Held.Contains(data.vkCode))
                        return (IntPtr)1;

                    axis.Held.Add(data.vkCode);
                }
                else
                {
                    axis.Held.Remove(data.vkCode);
                }

                Apply(axis);
            }

            return (IntPtr)1;
        }

        private void Apply(Axis axis)
        {
            uint desired = axis.Held.Count switch
            {
                0 => 0,
                1 => axis.Held[0],
                _ => _mode switch
                {
                    SocdMode.FirstInput => axis.Held[0],
                    SocdMode.Neutral => 0,
                    _ => axis.Held[^1]
                }
            };

            if (desired == axis.Active)
                return;

            if (axis.Active != 0)
                Send(axis.Active, false);

            if (desired != 0)
                Send(desired, true);

            axis.Active = desired;
        }

        private void Reset()
        {
            foreach (var axis in _axes)
            {
                if (axis.Active != 0)
                    Send(axis.Active, false);

                axis.Active = 0;
                axis.Held.Clear();
            }
        }

        private bool IsRobloxForeground()
        {
            GetWindowThreadProcessId(GetForegroundWindow(), out uint pid);
            return pid == _processId;
        }

        private static void Send(uint key, bool down)
        {
            uint flags = down ? 0 : KEYEVENTF_KEYUP;

            if (key is >= 0x25 and <= 0x28)
                flags |= KEYEVENTF_EXTENDEDKEY;

            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)key,
                    wScan = (ushort)MapVirtualKey(key, 0),
                    dwFlags = flags,
                    dwExtraInfo = Marker
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            lock (_lock)
                Reset();

            if (_threadId != 0)
                PostThreadMessage(_threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);

            _thread?.Join(TimeSpan.FromSeconds(2));

            GC.SuppressFinalize(this);
        }

        // Native

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public nuint dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public nuint dwExtraInfo;
            private readonly uint _padding1;
            private readonly uint _padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int x;
            public int y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);
    }
}
