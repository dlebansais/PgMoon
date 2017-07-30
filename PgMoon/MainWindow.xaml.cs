using Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PgMoon
{
    public partial class MainWindow : Popup, INotifyPropertyChanged, IDisposable
    {
        #region Init
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Placement = PlacementMode.Absolute;
            InitMoonPhase();
            InitTaskbarIcon();
        }

        public bool IsElevated
        {
            get
            {
                WindowsIdentity wi = WindowsIdentity.GetCurrent();
                if (wi != null)
                {
                    WindowsPrincipal wp = new WindowsPrincipal(wi);
                    if (wp != null)
                        return wp.IsInRole(WindowsBuiltInRole.Administrator);
                }

                return false;
            }
        }
        #endregion

        #region Taskbar Icon
        private void InitTaskbarIcon()
        {
            System.Drawing.Icon Icon = LoadIcon("Taskbar.ico");
            string ToolTipText = MoonPhaseName();
            ContextMenu ContextMenu = LoadContextMenu();

            TaskbarIcon = TaskbarIcon.Create(Icon, ToolTipText, ContextMenu, this);
        }

        private System.Drawing.Icon LoadIcon(string IconName)
        {
            foreach (string ResourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                if (ResourceName.EndsWith(IconName))
                {
                    using (Stream rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
                    {
                        System.Drawing.Icon Result = new System.Drawing.Icon(rs);
                        return Result;
                    }
                }

            return null;
        }

        private System.Drawing.Bitmap LoadBitmap(string IconName)
        {
            foreach (string ResourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                if (ResourceName.EndsWith(IconName))
                {
                    using (Stream rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
                    {
                        System.Drawing.Bitmap Result = new System.Drawing.Bitmap(rs);
                        return Result;
                    }
                }

            return null;
        }

        private ContextMenu LoadContextMenu()
        {
            ContextMenu Result = new ContextMenu();

            MenuItem ExitMenu = LoadNotificationMenuItem("Exit", "ExitCommand");
            MenuItem LoadAtStartup = LoadNotificationMenuItem("Load at startup", "LoadAtStartupCommand");

            if (IsElevated)
            {
                string ExeName = Assembly.GetExecutingAssembly().Location;
                if (Scheduler.IsTaskActive(ExeName))
                    LoadAtStartup.IsChecked = true;
            }
            else
                LoadAtStartup.Icon = LoadBitmap("UAC-16.png");

            AddContextMenu(Result, LoadAtStartup, true);
            AddContextMenuSeparator(Result);
            AddContextMenu(Result, ExitMenu, true);

            return Result;
        }

        private MenuItem LoadNotificationMenuItem(string MenuHeader, string CommandResourceName)
        {
            MenuItem Result = new MenuItem();
            Result.Header = MenuHeader;
            Result.Command = FindResource(CommandResourceName) as RoutedCommand;
            Result.Icon = null;

            return Result;
        }

        private void AddContextMenu(ContextMenu Menu, MenuItem Item, bool IsVisible)
        {
            Item.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            Menu.Items.Add(Item);
        }

        private void AddContextMenuSeparator(ContextMenu Menu)
        {
            Menu.Items.Add(new Separator());
        }

        public TaskbarIcon TaskbarIcon { get; private set; }
        #endregion

        #region Events
        private void OnOpened(object sender, EventArgs e)
        {
            System.Drawing.Point FormsMousePosition = System.Windows.Forms.Control.MousePosition;
            Point MousePosition = new Point(FormsMousePosition.X, FormsMousePosition.Y);

            Rect WorkArea = SystemParameters.WorkArea;

            double WorkScreenWidth = WorkArea.Right - WorkArea.Left;
            double WorkScreenHeight = WorkArea.Bottom - WorkArea.Top;
            double CurrentScreenWidth = Taskbar.CurrentScreen.Bounds.Right - Taskbar.CurrentScreen.Bounds.Left;
            double CurrentScreenHeight = Taskbar.CurrentScreen.Bounds.Bottom - Taskbar.CurrentScreen.Bounds.Top;

            double RatioX = WorkScreenWidth / CurrentScreenWidth;
            double RatioY = WorkScreenHeight / CurrentScreenHeight;

            FrameworkElement MainChild = Child as FrameworkElement;
            Size PopupSize = new Size((int)(MainChild.Width / RatioX), (int)(MainChild.Height / RatioY));

            Point RelativePosition = Taskbar.GetRelativePosition(MousePosition, PopupSize);

            RelativePosition = new Point(RelativePosition.X * RatioX, RelativePosition.Y * RatioY);

            HorizontalOffset = RelativePosition.X;
            VerticalOffset = RelativePosition.Y;

            HwndSource source = (HwndSource)HwndSource.FromVisual(Child);
            SetWindowPos(source.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
            SetForegroundWindow(source.Handle);
        }

        public void OnDeactivated()
        {
            IsOpen = false;
        }

        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void OnLoadAtStartup(object sender, ExecutedRoutedEventArgs e)
        {
            TaskbarIcon SenderIcon = e.Parameter as TaskbarIcon;

            if (IsElevated)
            {
                bool IsChecked;
                if (SenderIcon.ToggleChecked(e.Command, out IsChecked))
                    InstallLoad(IsChecked);
            }
            else
                MessageBox.Show("To have this program loaded at startup, please exit and restart it as administrator.", "Administrator privileges required", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnExit(object sender, ExecutedRoutedEventArgs e)
        {
            IsOpen = false;

            using (TaskbarIcon Icon = TaskbarIcon)
            {
                TaskbarIcon = null;
            }

            Application.Current.Shutdown();
        }
        #endregion

        #region Window Handle Management
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_SHOWWINDOW = 0x0040;
        const int SWP_NOACTIVATE = 0x0010;
        const int HWND_TOPMOST = -1;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        #endregion

        #region Load at startup
        private void InstallLoad(bool Install)
        {
            string ExeName = Assembly.GetExecutingAssembly().Location;

            if (Install)
                Scheduler.AddTask(ExeName);
            else
                Scheduler.RemoveTask(ExeName);
        }
        #endregion

        #region Moon Phase
        private void InitMoonPhase()
        {
            MoonPhases Phase;
            float PhaseProgress;
            if (GetMoonPhaseFromServer(out Phase, out PhaseProgress))
            {
                MoonPhase = Phase;
                MoonPhaseProgress = PhaseProgress;
            }

            UpdateMoonPhaseTimer = new Timer(new TimerCallback(UpdateMoonPhaseTimerCallback));
            TimeSpan UpdateInterval = TimeSpan.FromMinutes(1);
            UpdateMoonPhaseTimer.Change(UpdateInterval, UpdateInterval);
        }

        public MoonPhases MoonPhase
        {
            get { return _MoonPhase; }
            set
            {
                if (_MoonPhase != value)
                {
                    _MoonPhase = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private MoonPhases _MoonPhase;

        public double MoonPhaseProgress
        {
            get { return _MoonPhaseProgress; }
            set
            {
                if (_MoonPhaseProgress != value)
                {
                    _MoonPhaseProgress = value;
                    NotifyThisPropertyChanged();
                    NotifyPropertyChanged(nameof(MoonPhaseTimeLeft));
                }
            }
        }
        public string MoonPhaseTimeLeft
        {
            get
            {
                if (double.IsNaN(_MoonPhaseProgress) || !(_MoonPhaseProgress >= 0 && _MoonPhaseProgress < 100))
                    return null;

                TimeSpan Duration = TimeSpan.FromDays(0.0369125 * _MoonPhaseProgress);

                string Result = "";

                if (Duration.TotalDays >= 1)
                {
                    int Days = (int)Duration.TotalDays;
                    Duration -= TimeSpan.FromDays(Days);

                    if (Days > 1)
                        Result += Days.ToString() + " days";
                    else
                        Result += Days.ToString() + " day";
                }

                if (Duration.TotalHours >= 1)
                {
                    int Hours = (int)Duration.TotalHours;
                    Duration -= TimeSpan.FromHours(Hours);

                    if (Result.Length > 0)
                        Result += ", ";

                    if (Hours > 1)
                        Result += Hours.ToString() + " hours";
                    else
                        Result += Hours.ToString() + " hour";
                }

                if (Result.Length == 0)
                    return "Changing soon";
                else
                    return Result + " left";
            }
        }
        private double _MoonPhaseProgress;

        private void UpdateMoonPhaseTimerCallback(object Parameter)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new UpdateMoonPhaseHandler(OnUpdateMoonPhase));
        }

        private delegate void UpdateMoonPhaseHandler();

        private void OnUpdateMoonPhase()
        {
            UpdateMoonPhaseNow();
        }

        private void UpdateMoonPhaseNow()
        {
            MoonPhases Phase;
            float PhaseProgress;
            if (GetMoonPhaseFromServer(out Phase, out PhaseProgress))
            {
                MoonPhase = Phase;
                MoonPhaseProgress = PhaseProgress;

                if (TaskbarIcon != null)
                    TaskbarIcon.UpdateToolTipText(MoonPhaseName());
            }
        }

        private bool GetMoonPhaseFromServer(out MoonPhases Phase, out float PhaseProgress)
        {
            Phase = MoonPhases.Unknown;
            PhaseProgress = float.NaN;
            bool Success = false;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest Request = HttpWebRequest.Create("https://www.timeanddate.com/moon/phases/usa/orlando") as HttpWebRequest;
            using (WebResponse Response = Request.GetResponse())
            {
                using (Stream ResponseStream = Response.GetResponseStream())
                {
                    using (StreamReader Reader = new StreamReader(ResponseStream, Encoding.ASCII))
                    {
                        string Content = Reader.ReadToEnd();

                        string Pattern = "cur-moon-percent";
                        int Index = Content.IndexOf(Pattern);
                        if (Index >= 0)
                        {
                            int PhaseIndex = Content.IndexOf("%", Index);
                            if (PhaseIndex <= Index + Pattern.Length + 5 && Content.Length > 14)
                            {
                                string ProgressString = "";
                                int ProgressIndex = PhaseIndex - 1;
                                while (ProgressIndex > Index && ((Content[ProgressIndex] >= '0' && Content[ProgressIndex] <= '9') || Content[ProgressIndex] == '.'))
                                {
                                    ProgressString = Content[ProgressIndex].ToString() + ProgressString;
                                    ProgressIndex--;
                                }

                                PhaseIndex += 16;
                                if (Content[PhaseIndex] == '>')
                                {
                                    int EndPhaseIndex = Content.IndexOf("<", PhaseIndex);
                                    if (EndPhaseIndex > PhaseIndex && EndPhaseIndex < PhaseIndex + 50)
                                    {
                                        string PhaseString = Content.Substring(PhaseIndex + 1, EndPhaseIndex - PhaseIndex - 1);
                                        PhaseString = PhaseString.Trim();
                                        if (!PhaseString.Contains("Moon"))
                                            PhaseString = PhaseString + " Moon";

                                        foreach (KeyValuePair<MoonPhases, string> Entry in MoonPhaseToStringConverter.MoonPhaseTable)
                                            if (PhaseString == Entry.Value)
                                            {
                                                Phase = Entry.Key;
                                                float.TryParse(ProgressString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out PhaseProgress);
                                                Success = true;
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Success;
        }

        private string MoonPhaseName()
        {
            string Result;
            if (MoonPhaseToStringConverter.MoonPhaseTable.ContainsKey(MoonPhase))
                Result = MoonPhaseToStringConverter.MoonPhaseTable[MoonPhase];
            else
                Result = null;

            return Result;
        }

        private Timer UpdateMoonPhaseTimer;
        #endregion

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        ///     Implements the PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameter is mandatory with [CallerMemberName]")]
        internal void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Implementation of IDisposable
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
                DisposeNow();
        }

        private void DisposeNow()
        {
            using (TaskbarIcon ToRemove = TaskbarIcon)
            {
                TaskbarIcon = null;
            }

            if (UpdateMoonPhaseTimer != null)
            {
                UpdateMoonPhaseTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                UpdateMoonPhaseTimer.Dispose();
                UpdateMoonPhaseTimer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MainWindow()
        {
            Dispose(false);
        }
        #endregion
    }
}
