using System.Runtime.InteropServices;
using System.Windows.Shell;

namespace Bloxstrap.UI.Utility
{
    // Modified from https://github.com/PowerShell/PSReadLine/blob/e9122d38e932614393ff61faf57d6518990d7226/PSReadLine/PlatformWindows.cs#L704
    internal static class TaskbarProgress
    {
        private enum TaskbarStates
        {
            NoProgress = 0,
            Indeterminate = 0x1,
            Normal = 0x2,
            Error = 0x4,
            Paused = 0x8,
        }

        [ComImport()]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig]
            int HrInit();

            [PreserveSig]
            int AddTab(IntPtr hwnd);

            [PreserveSig]
            int DeleteTab(IntPtr hwnd);

            [PreserveSig]
            int ActivateTab(IntPtr hwnd);

            [PreserveSig]
            int SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            [PreserveSig]
            int MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            // ITaskbarList3
            [PreserveSig]
            int SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);

            [PreserveSig]
            int SetProgressState(IntPtr hwnd, TaskbarStates state);

            // N.B. for copy/pasters: we've left out the rest of the ITaskbarList3 methods...
        }

        [ComImport()]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance
        {
        }

        private static Lazy<ITaskbarList3?> _taskbarInstance = new Lazy<ITaskbarList3?>(() =>
        {
            ITaskbarList3 taskbar = (ITaskbarList3)new TaskbarInstance();
            try
            {
                bool hasInitialised = taskbar.HrInit() == 0; // reduce pointless calls by checking if we initialised properly
                return hasInitialised ? taskbar : null;
            }
            catch (NotImplementedException)
            {
                return null;
            }
        });

        private static TaskbarStates ConvertEnum(TaskbarItemProgressState state)
        {
            return state switch
            {
                TaskbarItemProgressState.None => TaskbarStates.NoProgress,
                TaskbarItemProgressState.Indeterminate => TaskbarStates.Indeterminate,
                TaskbarItemProgressState.Normal => TaskbarStates.Normal,
                TaskbarItemProgressState.Error => TaskbarStates.Error,
                TaskbarItemProgressState.Paused => TaskbarStates.Paused,
                _ => throw new Exception($"Unrecognised TaskbarItemProgressState: {state}")
            };
        }

        private static int SetProgressState(IntPtr windowHandle, TaskbarStates taskbarState)
        {
            return _taskbarInstance.Value?.SetProgressState(windowHandle, taskbarState) ?? 1;
        }

        public static int SetProgressValue(IntPtr windowHandle, int progressValue, int progressMax)
        {
            return _taskbarInstance.Value?.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax) ?? 1;
        }

        public static int SetProgressState(IntPtr windowHandle, TaskbarItemProgressState taskbarState)
        {
            return SetProgressState(windowHandle, ConvertEnum(taskbarState));
        }

        /// <summary>
        /// Will assume windowHandle is Process.GetCurrentProcess().MainWindowHandle
        /// </summary>
        public static int SetProgressState(TaskbarItemProgressState taskbarState)
        {
            return SetProgressState(Process.GetCurrentProcess().MainWindowHandle, ConvertEnum(taskbarState));
        }

        /// <summary>
        /// Will assume windowHandle is Process.GetCurrentProcess().MainWindowHandle
        /// </summary>
        public static int SetProgressValue(int progressValue, int progressMax)
        {
            return SetProgressValue(Process.GetCurrentProcess().MainWindowHandle, progressValue, progressMax);
        }
    }
}
