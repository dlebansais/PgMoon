using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace PgMoon
{
    public static class Taskbar
    {
        #region Init
        static Taskbar()
        {
            UpdateLocation();
        }

        public static void UpdateLocation()
        {
            TaskbarHandle = IntPtr.Zero;
            CurrentScreen = null;
            FirstScreen = null;

            IntPtr hwnd;
            if (GetSystemTrayHandle(out hwnd))
            {
                TaskbarHandle = hwnd;

                RECT TrayRect, NotificationAreaRect, IconAreaRect;
                if (GetSystemTrayRect(out TrayRect, out NotificationAreaRect, out IconAreaRect))
                {
                    foreach (Screen s in Screen.AllScreens)
                    {
                        if (FirstScreen == null)
                            FirstScreen = s;

                        if (s.Bounds.Contains(TrayRect.Left, TrayRect.Top))
                        {
                            CurrentScreen = s;
                            break;
                        }
                    }
                }
            }
        }

        private static IntPtr TaskbarHandle;
        public static Screen CurrentScreen;
        public static Screen FirstScreen;
        #endregion

        // From a position, and a window size, all in screen coordinates, return the position the window should take
        // to be on the edge of the task bar. In screen coordinates.
        public static Point GetRelativePosition(Point Position, Size Size)
        {
            RECT TrayRect, NotificationAreaRect, IconAreaRect;
            if (CurrentScreen == null ||!GetSystemTrayRect(out TrayRect, out NotificationAreaRect, out IconAreaRect))
                return new Point(0, 0);

            // Use the full taskbar rectangle.
            RECT TaskbarRect = TrayRect;

            double X;
            double Y;

            // If the potion isn't within the taskbar (shouldn't happen), default to bottom.
            if (!(Position.X >= TaskbarRect.Left && Position.X < TaskbarRect.Right && Position.Y >= TaskbarRect.Top && Position.Y < TaskbarRect.Bottom))
                AlignedToBottom(Position, Size, TaskbarRect, out X, out Y);

            else
            {
                // Otherwise, check where the taskbar is, and calculate an aligned position.
                switch (GetTaskBarLocation(TaskbarRect))
                {
                    case TaskBarLocation.Top:
                        AlignedToTop(Position, Size, TaskbarRect, out X, out Y);
                        break;

                    default:
                    case TaskBarLocation.Bottom:
                        AlignedToBottom(Position, Size, TaskbarRect, out X, out Y);
                        break;

                    case TaskBarLocation.Left:
                        AlignedToLeft(Position, Size, TaskbarRect, out X, out Y);
                        break;

                    case TaskBarLocation.Right:
                        AlignedToRight(Position, Size, TaskbarRect, out X, out Y);
                        break;
                }
            }

            return new Point(X, Y);
        }

        private enum TaskBarLocation { Top, Bottom, Left, Right }

        private static TaskBarLocation GetTaskBarLocation(RECT TaskbarRect)
        {
            Point TaskbarCenter = new Point((TaskbarRect.Left + TaskbarRect.Right) / 2, (TaskbarRect.Top + TaskbarRect.Bottom) / 2);

            bool IsTop = (TaskbarCenter.Y < CurrentScreen.WorkingArea.Top + (CurrentScreen.WorkingArea.Bottom - CurrentScreen.WorkingArea.Top) / 4);
            bool IsBottom = (TaskbarCenter.Y >= CurrentScreen.WorkingArea.Bottom - (CurrentScreen.WorkingArea.Bottom - CurrentScreen.WorkingArea.Top) / 4);
            bool IsLeft = (TaskbarCenter.X < CurrentScreen.WorkingArea.Left + (CurrentScreen.WorkingArea.Right - CurrentScreen.WorkingArea.Left) / 4);
            bool IsRight = (TaskbarCenter.X >= CurrentScreen.WorkingArea.Right - (CurrentScreen.WorkingArea.Right - CurrentScreen.WorkingArea.Left) / 4);

            if (IsTop && !IsLeft && !IsRight)
                return TaskBarLocation.Top;

            else if (IsBottom && !IsLeft && !IsRight)
                return TaskBarLocation.Bottom;

            else if (IsLeft && !IsTop && !IsBottom)
                return TaskBarLocation.Left;

            else if (IsRight && !IsTop && !IsBottom)
                return TaskBarLocation.Right;

            else
                return TaskBarLocation.Bottom;
        }

        private static void AlignedToLeft(Point Position, Size Size, RECT TaskbarRect, out double X, out double Y)
        {
            X = TaskbarRect.Right;
            Y = Position.Y - Size.Height / 2;
        }

        private static void AlignedToRight(Point Position, Size Size, RECT TaskbarRect, out double X, out double Y)
        {
            X = TaskbarRect.Left - Size.Width;
            Y = Position.Y - Size.Height / 2;
        }

        private static void AlignedToTop(Point Position, Size Size, RECT TaskbarRect, out double X, out double Y)
        {
            X = Position.X - Size.Width / 2;
            Y = TaskbarRect.Bottom;
        }

        private static void AlignedToBottom(Point Position, Size Size, RECT TaskbarRect, out double X, out double Y)
        {
            X = Position.X - Size.Width / 2;
            Y = TaskbarRect.Top - Size.Height;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private static bool GetSystemTrayHandle(out IntPtr hwnd)
        {
            hwnd = IntPtr.Zero;

            IntPtr hWndTray = FindWindow("Shell_TrayWnd", null);
            if (hWndTray != IntPtr.Zero)
            {
                hwnd = hWndTray;

                hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "TrayNotifyWnd", null);
                if (hWndTray != IntPtr.Zero)
                {
                    hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "SysPager", null);
                    if (hWndTray != IntPtr.Zero)
                    {
                        hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "ToolbarWindow32", null);
                        if (hWndTray != IntPtr.Zero)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool GetSystemTrayRect(out RECT TrayRect, out RECT NotificationAreaRect, out RECT IconAreaRect)
        {
            TrayRect = new RECT() { Left = 0, Top = 0, Right = 0, Bottom = 0 };
            NotificationAreaRect = new RECT() { Left = 0, Top = 0, Right = 0, Bottom = 0 };
            IconAreaRect = new RECT() { Left = 0, Top = 0, Right = 0, Bottom = 0 };

            IntPtr hWndTray = FindWindow("Shell_TrayWnd", null);
            if (hWndTray != IntPtr.Zero)
            {
                GetWindowRect(hWndTray, ref TrayRect);

                hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "TrayNotifyWnd", null);
                if (hWndTray != IntPtr.Zero)
                {
                    GetWindowRect(hWndTray, ref NotificationAreaRect);

                    hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "SysPager", null);
                    if (hWndTray != IntPtr.Zero)
                    {
                        hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "ToolbarWindow32", null);
                        if (hWndTray != IntPtr.Zero)
                        {
                            GetWindowRect(hWndTray, ref IconAreaRect);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool ToScreen(ref Point Position)
        {
            POINT p1 = new POINT() { X = 0, Y = 0 };
            POINT p2 = new POINT() { X = 1000, Y = 1000 };

            if (TaskbarHandle != IntPtr.Zero && ClientToScreen(TaskbarHandle, ref p1) && ClientToScreen(TaskbarHandle, ref p2))
            {
                double RatioX = (double)(p2.X - p1.X) / 1000;
                double RatioY = (double)(p2.Y - p1.Y) / 1000;

                Position = new Point(Position.X * RatioX, Position.Y * RatioY);
                return true;
            }

            return false;
        }

        public static bool ToScreen(ref Size Size)
        {
            POINT p1 = new POINT() { X = 0, Y = 0 };
            POINT p2 = new POINT() { X = 1000, Y = 1000 };

            if (TaskbarHandle != IntPtr.Zero && ClientToScreen(TaskbarHandle, ref p1) && ClientToScreen(TaskbarHandle, ref p2))
            {
                double RatioX = (double)(p2.X - p1.X) / 1000;
                double RatioY = (double)(p2.Y - p1.Y) / 1000;

                Size = new Size(Size.Width * RatioX, Size.Height * RatioY);
                return true;
            }

            return false;
        }

        public static bool ToClient(ref Point Position)
        {
            POINT p1 = new POINT() { X = 0, Y = 0 };
            POINT p2 = new POINT() { X = 1000, Y = 1000 };

            if (TaskbarHandle != IntPtr.Zero && ScreenToClient(TaskbarHandle, ref p1) && ScreenToClient(TaskbarHandle, ref p2))
            {
                double RatioX = (double)(p2.X - p1.X) / 1000;
                double RatioY = (double)(p2.Y - p1.Y) / 1000;

                Position = new Point(Position.X * RatioX, Position.Y * RatioY);
                return true;
            }

            return false;
        }

        public static bool ToClient(ref Size Size)
        {
            POINT p1 = new POINT() { X = 0, Y = 0 };
            POINT p2 = new POINT() { X = 1000, Y = 1000 };

            if (TaskbarHandle != IntPtr.Zero && ScreenToClient(TaskbarHandle, ref p1) && ScreenToClient(TaskbarHandle, ref p2))
            {
                double RatioX = (double)(p2.X - p1.X) / 1000;
                double RatioY = (double)(p2.Y - p1.Y) / 1000;

                Size = new Size(Size.Width * RatioX, Size.Height * RatioY);
                return true;
            }

            return false;
        }
    }
}
