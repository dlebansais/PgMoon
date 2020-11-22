namespace PgMoon
{
    using System;
    using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
    public static class NativeMethods
    {
        #region Window Handle Management
        [DllImport("User32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

#pragma warning disable CA1707 // Identifiers should not contain underscores
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int HWND_TOPMOST = -1;
#pragma warning restore CA1707 // Identifiers should not contain underscores

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        internal static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        #endregion
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
}
