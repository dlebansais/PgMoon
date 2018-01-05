using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Media;
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
            InitCalendar();
            InitMushroomFarming();
            InitRahuBoat();
            InitDarkChapel();
            InitSharedCalendar();
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
            MenuHeaderTable = new Dictionary<ICommand, string>();
            LoadAtStartupCommand = InitMenuCommand("LoadAtStartupCommand", LoadAtStartupHeader);
            ShowCalendarCommand = InitMenuCommand("ShowCalendarCommand", "Show Calendar");
            ShowMushroomFarmingCommand = InitMenuCommand("ShowMushroomFarmingCommand", "Show Mushrooms");
            ShowRahuBoatCommand = InitMenuCommand("ShowRahuBoatCommand", "Show Rahu Boat");
            ShowDarkChapelCommand = InitMenuCommand("ShowDarkChapelCommand", "Show Dark Chapel");
            SharedCalendarCommand = InitMenuCommand("SharedCalendarCommand", "Share the calendar...");
            ExitCommand = InitMenuCommand("ExitCommand", "Exit");

            System.Drawing.Icon Icon = LoadIcon("Taskbar.ico");
            ContextMenu ContextMenu = LoadContextMenu();

            TaskbarIcon = TaskbarIcon.Create(Icon, ToolTipText, ContextMenu, this);
            TaskbarIcon.MenuOpening += OnMenuOpening;
        }

        private ICommand InitMenuCommand(string CommandName, string Header)
        {
            ICommand Command = FindResource(CommandName) as ICommand;
            MenuHeaderTable.Add(Command, Header);
            return Command;
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

            MenuItem LoadAtStartup;
            string ExeName = Assembly.GetExecutingAssembly().Location;
            if (Scheduler.IsTaskActive(ExeName))
            {
                if (IsElevated)
                {
                    LoadAtStartup = LoadNotificationMenuItem(LoadAtStartupCommand);
                    LoadAtStartup.IsChecked = true;
                }
                else
                {
                    LoadAtStartup = LoadNotificationMenuItem(LoadAtStartupCommand, RemoveFromStartupHeader);
                    LoadAtStartup.Icon = LoadBitmap("UAC-16.png");
                }
            }
            else
            {
                LoadAtStartup = LoadNotificationMenuItem(LoadAtStartupCommand);

                if (!IsElevated)
                    LoadAtStartup.Icon = LoadBitmap("UAC-16.png");
            }

            MenuItem ShowDarkChapelMenu = LoadNotificationMenuItem(ShowDarkChapelCommand);
            ShowDarkChapelMenu.IsChecked = ShowDarkChapel;
            MenuItem ShowRahuBoatMenu = LoadNotificationMenuItem(ShowRahuBoatCommand);
            ShowRahuBoatMenu.IsChecked = ShowRahuBoat;
            MenuItem ShowMushroomFarmingMenu = LoadNotificationMenuItem(ShowMushroomFarmingCommand);
            ShowMushroomFarmingMenu.IsChecked = ShowMushroomFarming;
            MenuItem ShowCalendarMenu = LoadNotificationMenuItem(ShowCalendarCommand);
            ShowCalendarMenu.IsChecked = ShowCalendar;
            MenuItem SharedCalendarMenu = LoadNotificationMenuItem(SharedCalendarCommand);
            MenuItem ExitMenu = LoadNotificationMenuItem(ExitCommand);

            AddContextMenu(Result, LoadAtStartup, true);
            AddContextMenuSeparator(Result);
            AddContextMenu(Result, ShowDarkChapelMenu, true);
            AddContextMenu(Result, ShowRahuBoatMenu, true);
            AddContextMenu(Result, ShowMushroomFarmingMenu, true);
            AddContextMenu(Result, ShowCalendarMenu, true);
            AddContextMenuSeparator(Result);
            AddContextMenu(Result, SharedCalendarMenu, true);
            AddContextMenuSeparator(Result);
            AddContextMenu(Result, ExitMenu, true);

            return Result;
        }

        private MenuItem LoadNotificationMenuItem(ICommand Command)
        {
            MenuItem Result = new MenuItem();
            Result.Header = MenuHeaderTable[Command];
            Result.Command = Command;
            Result.Icon = null;

            return Result;
        }

        private MenuItem LoadNotificationMenuItem(ICommand Command, string MenuHeader)
        {
            MenuItem Result = new MenuItem();
            Result.Header = MenuHeader;
            Result.Command = Command;
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

        private void OnMenuOpening(object sender, EventArgs e)
        {
            TaskbarIcon SenderIcon = sender as TaskbarIcon;
            string ExeName = Assembly.GetExecutingAssembly().Location;

            if (IsElevated)
                SenderIcon.Check(LoadAtStartupCommand, Scheduler.IsTaskActive(ExeName));
            else
            {
                if (Scheduler.IsTaskActive(ExeName))
                    SenderIcon.SetText(LoadAtStartupCommand, RemoveFromStartupHeader);
                else
                    SenderIcon.SetText(LoadAtStartupCommand, LoadAtStartupHeader);
            }
        }

        public TaskbarIcon TaskbarIcon { get; private set; }
        private static readonly string LoadAtStartupHeader = "Load at startup";
        private static readonly string RemoveFromStartupHeader = "Remove from startup";
        private ICommand LoadAtStartupCommand;
        private ICommand ShowCalendarCommand;
        private ICommand ShowMushroomFarmingCommand;
        private ICommand ShowRahuBoatCommand;
        private ICommand ShowDarkChapelCommand;
        private ICommand SharedCalendarCommand;
        private ICommand ExitCommand;
        private Dictionary<ICommand, string> MenuHeaderTable;
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
            UpdateMoonPhaseTimer = new Timer(new TimerCallback(UpdateMoonPhaseTimerCallback));
            TimeSpan UpdateInterval = TimeSpan.FromSeconds(60);
            PreviousUpdateChanged = true;
            UpdateMoonPhaseTimer.Change(UpdateInterval, UpdateInterval);
        }

        public string TimeToNextPhaseText
        {
            get { return FriendlyTimeString(PhaseCalculator.TimeToNextPhase, "Changing soon"); }
        }

        public string TimeToFullMoonText
        {
            get { return FriendlyTimeString(PhaseCalculator.TimeToFullMoon, "Soon"); }
        }

        private string FriendlyTimeString(TimeSpan Duration, string SoonText)
        {
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
                return SoonText;
            else
                return Result + " left";
        }

        public bool IsFullMoon
        {
            get { return PhaseCalculator.MoonPhase == MoonPhase.FullMoon; }
        }

        public bool IsNextPhaseFullMoon
        {
            get { return PhaseCalculator.MoonPhase == MoonPhase.WaxingGibbousMoon; }
        }

        public string ToolTipText
        {
            get
            {
                string Result = PhaseCalculator.MoonPhase.Name + "\r\n" + TimeToNextPhaseText;
                if (ShowRahuBoat)
                    Result += "\r\n" + CalendarEntry.RahuBoatDestinationShortText + ": " + PhaseCalculator.MoonPhase.RahuBoatDestination;
                Result += "\r\n" + CalendarEntry.PortToCircleShortText + ": " + PhaseCalculator.MoonPhase.FastPortMushroomShortText;

                return Result;
            }
        }

        private void UpdateMoonPhaseTimerCallback(object Parameter)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new UpdateMoonPhaseHandler(OnUpdateMoonPhase));
        }

        private delegate void UpdateMoonPhaseHandler();

        private void OnUpdateMoonPhase()
        {
            //App.IncreaseNow(); //Debug only

            PhaseCalculator.Update();
            NotifyPropertyChanged(nameof(TimeToNextPhaseText));
            NotifyPropertyChanged(nameof(TimeToFullMoonText));
            NotifyPropertyChanged(nameof(IsFullMoon));
            NotifyPropertyChanged(nameof(IsNextPhaseFullMoon));
            NotifyPropertyChanged(nameof(ToolTipText));

            if (TaskbarIcon != null)
                TaskbarIcon.UpdateToolTipText(ToolTipText);

            DateTime LastTimeKey = DateTime.MinValue;
            DateTime LastTime = DateTime.MinValue;

            foreach (KeyValuePair<DateTime, CalendarEntry> Entry in CalendarEntryTable)
            {
                CalendarEntry Item = Entry.Value;
                Item.Update();

                LastTimeKey = Entry.Key;
                LastTime = Item.EndTime;
            }

            if (LastTime < App.Now() && !PreviousUpdateChanged)
            {
                CalendarStartTime = LastTimeKey;
                BuildCalendar();
            }

            PreviousUpdateChanged = false;

            PostSharedEvents();
        }

        private Timer UpdateMoonPhaseTimer;
        private bool PreviousUpdateChanged;
        #endregion

        #region Calendar
        private void InitCalendar()
        {
            _ShowCalendar = App.GetSettingBool(ShowCalendarSettingName, true);
            _CalendarStartTime = App.Now();
            BuildCalendar();
        }

        public bool ShowCalendar
        {
            get { return _ShowCalendar; }
            set
            {
                if (_ShowCalendar != value)
                {
                    _ShowCalendar = value;
                    NotifyThisPropertyChanged();

                    App.SetSettingBool(ShowCalendarSettingName, value);
                }
            }
        }
        private bool _ShowCalendar;

        public DateTime CalendarStartTime
        {
            get { return _CalendarStartTime; }
            set
            {
                if (_CalendarStartTime != value)
                {
                    _CalendarStartTime = value;
                    NotifyThisPropertyChanged();
                    NotifyPropertyChanged(nameof(CalendarStartTimeYear));
                }
            }
        }
        public string CalendarStartTimeYear
        {
            get
            {
                int Year = CalendarStartTime.Year;
                if (Year == App.Now().Year)
                    return "";
                else
                    return CalendarStartTime.Year.ToString();
            }
        }
        private DateTime _CalendarStartTime;

        public ObservableCollection<CalendarEntry> CalendarEntryList { get; private set; } = new ObservableCollection<CalendarEntry>();
        private Dictionary<DateTime, CalendarEntry> CalendarEntryTable = new Dictionary<DateTime, CalendarEntry>();

        private void BuildCalendar()
        {
            CalendarEntryList.Clear();

            DateTime Time = CalendarStartTime;
            for (int i = 0; i < 5; i++)
            {
                CalendarEntry CalendarEntry;

                if (!CalendarEntryTable.ContainsKey(Time))
                {
                    int MoonMonth;
                    MoonPhase MoonPhase;
                    DateTime PhaseStartTime;
                    DateTime PhaseEndTime;
                    double ProgressToFullMoon;
                    DateTime NextFullMoonTime;
                    PhaseCalculator.DateTimeToMoonPhase(Time, out MoonMonth, out MoonPhase, out PhaseStartTime, out PhaseEndTime, out ProgressToFullMoon, out NextFullMoonTime);

                    if (!CalendarEntryTable.ContainsKey(PhaseStartTime))
                    {
                        CalendarEntry NewCalendarEntry = new CalendarEntry(MoonMonth, MoonPhase, PhaseStartTime, PhaseEndTime, MushroomInfoList);
                        CalendarEntryTable.Add(PhaseStartTime, NewCalendarEntry);
                    }

                    Time = PhaseStartTime;
                }

                CalendarEntry = CalendarEntryTable[Time];
                CalendarEntryList.Add(CalendarEntry);

                Time = CalendarEntry.EndTime;
            }
        }

        private void OnShowCalendar(object sender, ExecutedRoutedEventArgs e)
        {
            TaskbarIcon SenderIcon = e.Parameter as TaskbarIcon;

            bool IsChecked;
            if (SenderIcon.ToggleChecked(e.Command, out IsChecked))
                ShowCalendar = IsChecked;
        }

        private void OnCalendarUp(object sender, MouseButtonEventArgs e)
        {
            CalendarStartTime = CalendarEntryList[0].StartTime - TimeSpan.FromHours(1);
            BuildCalendar();
            PreviousUpdateChanged = true;
        }

        private void OnCalendarUpPage(object sender, MouseButtonEventArgs e)
        {
            CalendarStartTime = CalendarEntryList[0].StartTime - (CalendarEntryList[CalendarEntryList.Count - 1].StartTime - CalendarEntryList[0].EndTime);
            BuildCalendar();
            PreviousUpdateChanged = true;
        }

        private void OnCalendarDown(object sender, MouseButtonEventArgs e)
        {
            CalendarStartTime = CalendarEntryList[0].EndTime + TimeSpan.FromHours(1);
            BuildCalendar();
            PreviousUpdateChanged = true;
        }

        private void OnCalendarDownPage(object sender, MouseButtonEventArgs e)
        {
            CalendarStartTime = CalendarEntryList[CalendarEntryList.Count - 1].StartTime + TimeSpan.FromHours(1);
            BuildCalendar();
            PreviousUpdateChanged = true;
        }

        private void OnCalendarDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CalendarStartTime = App.Now();
            BuildCalendar();
            PreviousUpdateChanged = true;
        }

        private void OnCalendarMouseWheel(object sender, MouseWheelEventArgs e)
        {
            CalendarStartTime = CalendarEntryList[0].EndTime - TimeSpan.FromHours(e.Delta);
            BuildCalendar();
            PreviousUpdateChanged = true;
        }

        private void OnDisplayCurrent(object sender, ExecutedRoutedEventArgs e)
        {
            CalendarStartTime = App.Now();
            BuildCalendar();
            PreviousUpdateChanged = true;
        }

        private static readonly string ShowCalendarSettingName = "ShowCalendar";
        private const int CalendarPageIncrement = 80;
        #endregion

        #region Mushroom Farming
        private void InitMushroomFarming()
        {
            _ShowMushroomFarming = App.GetSettingBool(ShowMushroomFarmingSettingName, true);
            _IsMushroomListLarge = false;
            _IsLocked = App.GetSettingBool(IsLockedSettingName, false);

            LoadMushroomInfoList();

            bool IsMushroomListInitialized = App.IsBoolKeySet(MushroomListInitializedName);
            App.SetSettingBool(MushroomListInitializedName, true);

            if (MushroomInfoList.Count == 0 && !IsMushroomListInitialized)
                ResetMushroomListToDefault(false);

            MushroomInfoList.Add(new MushroomInfo("", "", null, null));
            MushroomInfoList.CollectionChanged += OnMushroomInfoListChanged;

            string ApplicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PgJsonParse");
            MushroomNameFile = Path.Combine(ApplicationFolder, "Mushrooms.txt");
            UpdateMushroomNameListTimer = new Timer(new TimerCallback(UpdateMushroomNameListTimerCallback));
            UpdateMushroomNameListTimer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        private void LoadMushroomInfoList()
        {
            MushroomInfoList.Clear();

            string MushroomListSetting = App.GetSettingString(MushroomListSettingName, null);
            if (MushroomListSetting != null)
            {
                string[] SplitMushroomListSetting = MushroomListSetting.Split(MushroomListSeparator);
                foreach (string s in SplitMushroomListSetting)
                {
                    string[] Line = s.Split(MushroomSeparator);
                    if (Line.Length >= 1)
                    {
                        string Name = Line[0].Trim();
                        if (Name.Length > 0)
                        {
                            int SelectedPhase1, SelectedPhase2;
                            if (Line.Length >= 3)
                            {
                                int PhaseIndex1;
                                int PhaseIndex2;

                                if (int.TryParse(Line[1].Trim(), out PhaseIndex1) && PhaseIndex1 >= 0 && PhaseIndex1 + 1 < MoonPhase.MoonPhaseList.Count)
                                    SelectedPhase1 = PhaseIndex1;
                                else
                                    SelectedPhase1 = -1;

                                if (int.TryParse(Line[2].Trim(), out PhaseIndex2) && PhaseIndex2 >= 0 && PhaseIndex2 + 1 < MoonPhase.MoonPhaseList.Count)
                                    SelectedPhase2 = PhaseIndex2;
                                else
                                    SelectedPhase2 = -1;
                            }
                            else
                            {
                                SelectedPhase1 = -1;
                                SelectedPhase2 = -1;
                            }

                            string Comment = ((Line.Length > 3) ? Line[3] : "");

                            MoonPhase RobustGrowthPhase1 = (SelectedPhase1 >= 0 ? MoonPhase.MoonPhaseList[SelectedPhase1] : null);
                            MoonPhase RobustGrowthPhase2 = (SelectedPhase2 >= 0 ? MoonPhase.MoonPhaseList[SelectedPhase2] : null);
                            MushroomInfoList.Add(new MushroomInfo(Name, Comment, RobustGrowthPhase1, RobustGrowthPhase2));
                        }
                    }
                }
            }
        }

        private void SaveMushroomInfoList()
        {
            string Setting = "";

            foreach (MushroomInfo Info in MushroomInfoList)
            {
                if (Info.Name.Length == 0)
                    continue;

                string Line = "";
                Line += Info.Name;
                Line += MushroomSeparator;
                if (Info.SelectedMoonPhase1 >= 0)
                    Line += Info.SelectedMoonPhase1.ToString();
                Line += MushroomSeparator;
                if (Info.SelectedMoonPhase2 >= 0)
                    Line += Info.SelectedMoonPhase2.ToString();
                Line += MushroomSeparator;
                Line += Info.Comment;

                if (Setting.Length > 0)
                    Setting += MushroomListSeparator;
                Setting += Line;
            }

            App.SetSettingString(MushroomListSettingName, Setting);
        }

        private void ResetMushroomListToDefault(bool KeepComment)
        {
            List<MushroomInfo> NewList = new List<MushroomInfo>();

            AddMushroomToList(NewList, MoonPhase.ParasolMushroomLongName, MoonPhase.FullMoon, MoonPhase.WaningCrescentMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.MycenaMushroomLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.FirstQuarterMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.BoletusMushroomLongName, MoonPhase.NewMoon, MoonPhase.WaningGibbousMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.FieldMushroomLongName, MoonPhase.WaxingGibbousMoon, MoonPhase.LastQuarterMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.BlusherMushroomLongName, MoonPhase.NewMoon, MoonPhase.WaningGibbousMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.GoblinPuffballLongName, MoonPhase.NewMoon, MoonPhase.WaxingGibbousMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.MilkCapMushroomLongName, MoonPhase.FullMoon, MoonPhase.WaningCrescentMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.BloodMushroomLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.LastQuarterMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.CoralMushroomLongName, MoonPhase.FirstQuarterMoon, MoonPhase.WaxingGibbousMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.IocaineMushroomLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.FirstQuarterMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.GroxmakMushroomLongName, MoonPhase.WaxingGibbousMoon, MoonPhase.LastQuarterMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.PorciniMushroomLongName, MoonPhase.FullMoon, MoonPhase.WaningGibbousMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.BlackFootMorelLongName, MoonPhase.NewMoon, MoonPhase.WaningCrescentMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.PixiesParasolLongName, MoonPhase.FirstQuarterMoon, MoonPhase.WaxingGibbousMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.FlyAmanitaLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.FullMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.BlastcapMushroomLongName, MoonPhase.FullMoon, MoonPhase.WaningGibbousMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.ChargedMyceliumLongName, null, null, KeepComment);
            AddMushroomToList(NewList, MoonPhase.FalseAgaricLongName, MoonPhase.WaningCrescentMoon, MoonPhase.LastQuarterMoon, KeepComment);
            AddMushroomToList(NewList, MoonPhase.WizardsMushroomLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.FirstQuarterMoon, KeepComment);

            MushroomInfoList.Clear();
            foreach (MushroomInfo Item in NewList)
                MushroomInfoList.Add(Item);
        }

        private void AddMushroomToList(List<MushroomInfo> NewList, string Name, MoonPhase RobustGrowthPhase1, MoonPhase RobustGrowthPhase2, bool KeepComment)
        {
            string Comment = "";

            if (KeepComment)
            {
                foreach (MushroomInfo Item in MushroomInfoList)
                    if (Item.Name == Name)
                    {
                        Comment = Item.Comment;
                        break;
                    }
            }

            NewList.Add(new MushroomInfo(Name, Comment, RobustGrowthPhase1, RobustGrowthPhase2));
        }

        public bool ShowMushroomFarming
        {
            get { return _ShowMushroomFarming; }
            set
            {
                if (_ShowMushroomFarming != value)
                {
                    _ShowMushroomFarming = value;
                    NotifyThisPropertyChanged();

                    App.SetSettingBool(ShowMushroomFarmingSettingName, value);
                }
            }
        }
        private bool _ShowMushroomFarming;

        public bool IsMushroomListSmall
        {
            get { return MushroomInfoList.Count < 2; }
        }

        public bool IsMushroomListLarge
        {
            get { return _IsMushroomListLarge; }
            set
            {
                if (_IsMushroomListLarge != value)
                {
                    _IsMushroomListLarge = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private bool _IsMushroomListLarge;

        public bool IsLocked
        {
            get { return _IsLocked; }
            set
            {
                if (_IsLocked != value)
                {
                    _IsLocked = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private bool _IsLocked;

        private void OnMushroomNameFileChanged(object sender, FileSystemEventArgs e)
        {
            UpdateMushroomNameListTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        }

        private void UpdateMushroomNameListTimerCallback(object Parameter)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new UpdateMushroomNameListHandler(OnUpdateMushroomNameList));
        }

        private delegate void UpdateMushroomNameListHandler();
        private void OnUpdateMushroomNameList()
        {
            if (MushroomNameList != null)
                MushroomNameList.Clear();

            if (!File.Exists(MushroomNameFile))
                return;

            try
            {
                using (FileStream fs = new FileStream(MushroomNameFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.ASCII))
                    {
                        for (;;)
                        {
                            string MushroomName = sr.ReadLine();
                            if (MushroomName == null || MushroomName.Length == 0)
                                break;

                            if (MushroomNameList == null)
                                MushroomNameList = new ObservableCollection<string>();

                            MushroomNameList.Add(MushroomName);
                        }
                    }
                }

                if (MushroomNameList != null && MushroomNameFileWatcher == null)
                {
                    string FolderPath = Path.GetDirectoryName(MushroomNameFile);
                    string FileName = Path.GetFileName(MushroomNameFile);
                    MushroomNameFileWatcher = new FileSystemWatcher(FolderPath, FileName);
                    MushroomNameFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                    MushroomNameFileWatcher.Changed += new FileSystemEventHandler(OnMushroomNameFileChanged);
                    MushroomNameFileWatcher.EnableRaisingEvents = true;
                }
            }
            catch
            {
            }
        }

        private void OnMushroomInfoListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(IsMushroomListSmall));
        }

        private void OnShowMushroomFarming(object sender, ExecutedRoutedEventArgs e)
        {
            TaskbarIcon SenderIcon = e.Parameter as TaskbarIcon;

            bool IsChecked;
            if (SenderIcon.ToggleChecked(e.Command, out IsChecked))
                ShowMushroomFarming = IsChecked;
        }

        private void OnMushroomListUp(object sender, MouseButtonEventArgs e)
        {
            ScrollViewer listviewMushrooms = LocateSibling(sender, nameof(listviewMushrooms)) as ScrollViewer;
            listviewMushrooms.LineUp();
        }

        private void OnMushroomListUpPage(object sender, MouseButtonEventArgs e)
        {
            ScrollViewer listviewMushrooms = LocateSibling(sender, nameof(listviewMushrooms)) as ScrollViewer;
            listviewMushrooms.PageUp();
        }

        private void OnMushroomListDown(object sender, MouseButtonEventArgs e)
        {
            ScrollViewer listviewMushrooms = LocateSibling(sender, nameof(listviewMushrooms)) as ScrollViewer;
            listviewMushrooms.LineDown();
        }

        private void OnMushroomListDownPage(object sender, MouseButtonEventArgs e)
        {
            ScrollViewer listviewMushrooms = LocateSibling(sender, nameof(listviewMushrooms)) as ScrollViewer;
            listviewMushrooms.PageDown();
        }

        private void OnLockMushroomList(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement Element = sender as FrameworkElement;
            ContextMenu ContextMenu = Element.ContextMenu;
            ContextMenu.IsOpen = true;
            ContextMenu.PlacementTarget = this;
            ContextMenu.Closed += OnContextMenuClosed;
        }

        private void OnLock(object sender, ExecutedRoutedEventArgs e)
        {
            IsLocked = true;
        }

        private void OnUnlock(object sender, ExecutedRoutedEventArgs e)
        {
            IsLocked = false;
        }

        private void OnResetToDefaultKeepComment(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("This command will attempts to preserve comments, but all your other changes to the mushroom farming settings will be lost.\r\n\r\nAre you sure?", "Reset Mushroom List To Default", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            ResetMushroomListToDefault(true);
        }

        private void OnResetToDefaultAll(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("All your changes to the mushroom farming settings will be lost.\r\n\r\nAre you sure?", "Reset Mushroom List To Default", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            ResetMushroomListToDefault(false);
        }

        private object LocateSibling(object sender, string ElementName)
        {
            FrameworkElement CurrentElement = sender as FrameworkElement;
            object SiblingElement = null;

            while (CurrentElement != null && SiblingElement == null)
            {
                CurrentElement = VisualTreeHelper.GetParent(CurrentElement) as FrameworkElement;
                if (CurrentElement != null)
                    SiblingElement = LogicalTreeHelper.FindLogicalNode(CurrentElement, ElementName);
            }

            return SiblingElement;
        }

        private void OnMushroomListSizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement Control = sender as FrameworkElement;
            if (!double.IsNaN(Control.ActualHeight) && !double.IsNaN(Control.MaxHeight))
            {
                IsMushroomListLarge = (Control.ActualHeight >= Control.MaxHeight);
            }
        }

        private void OnMushroomNameValidationError(object sender, ValidationErrorEventArgs e)
        {
            ComboBox Control = sender as ComboBox;
            MushroomInfo Line = Control.DataContext as MushroomInfo;
            Line.Name = "";
        }

        private void OnMushroomNameLostFocus(object sender, RoutedEventArgs e)
        {
            if (MushroomInfoList.Count > 0 && MushroomInfoList.Count < MaxMushroomRows)
            {
                if (MushroomInfoList[MushroomInfoList.Count - 1].Name.Length > 0)
                    MushroomInfoList.Add(new MushroomInfo("", "", null, null));
                else
                {
                    bool Continue = true;

                    while (Continue)
                    {
                        Continue = false;

                        for (int i = 0; i + 1 < MushroomInfoList.Count; i++)
                        {
                            MushroomInfo Item = MushroomInfoList[i];
                            if (Item.Name.Length == 0 && Item.SelectedMoonPhase1 < 0 && Item.SelectedMoonPhase2 < 0)
                            {
                                MushroomInfoList.Remove(Item);
                                Continue = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void OnUnlockMushroomList(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement Element = sender as FrameworkElement;
            ContextMenu ContextMenu = Element.ContextMenu;
            ContextMenu.IsOpen = true;
            ContextMenu.PlacementTarget = this;
            ContextMenu.Closed += OnContextMenuClosed;
        }

        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            ContextMenu ContextMenu = sender as ContextMenu;
            ContextMenu.Closed -= OnContextMenuClosed;
            ContextMenu.PlacementTarget = null;
        }

        private static readonly string ShowMushroomFarmingSettingName = "ShowMushroomFarming";
        private static readonly string MushroomListSettingName = "MushroomList";
        private static readonly string MushroomListInitializedName = "MushroomListInitialized";
        private static readonly string IsLockedSettingName = "MushroomListLocked";
        private const int MaxMushroomRows = 40;
        private const char MushroomListSeparator = '\u2551';
        private const char MushroomSeparator = '\u2550';
        public ObservableCollection<MushroomInfo> MushroomInfoList { get; private set; } = new ObservableCollection<MushroomInfo>();
        private string MushroomNameFile;
        public ObservableCollection<string> MushroomNameList { get; private set; } = null;
        private FileSystemWatcher MushroomNameFileWatcher = null;
        private Timer UpdateMushroomNameListTimer;
        #endregion

        #region Rahu Boat
        private void InitRahuBoat()
        {
            _ShowRahuBoat = App.GetSettingBool(ShowRahuBoatSettingName, true);
        }

        public bool ShowRahuBoat
        {
            get { return _ShowRahuBoat; }
            set
            {
                if (_ShowRahuBoat != value)
                {
                    _ShowRahuBoat = value;
                    NotifyThisPropertyChanged();

                    App.SetSettingBool(ShowRahuBoatSettingName, value);

                    if (TaskbarIcon != null)
                        TaskbarIcon.UpdateToolTipText(ToolTipText);
                }
            }
        }
        private bool _ShowRahuBoat;

        private void OnShowRahuBoat(object sender, ExecutedRoutedEventArgs e)
        {
            TaskbarIcon SenderIcon = e.Parameter as TaskbarIcon;

            bool IsChecked;
            if (SenderIcon.ToggleChecked(e.Command, out IsChecked))
                ShowRahuBoat = IsChecked;
        }

        private static readonly string ShowRahuBoatSettingName = "ShowRahuBoat";
        #endregion

        #region Dark Chapel
        private void InitDarkChapel()
        {
            _ShowDarkChapel = App.GetSettingBool(ShowDarkChapelSettingName, true);
        }

        public bool ShowDarkChapel
        {
            get { return _ShowDarkChapel; }
            set
            {
                if (_ShowDarkChapel != value)
                {
                    _ShowDarkChapel = value;
                    NotifyThisPropertyChanged();

                    App.SetSettingBool(ShowDarkChapelSettingName, value);
                }
            }
        }
        private bool _ShowDarkChapel;

        private void OnShowDarkChapel(object sender, ExecutedRoutedEventArgs e)
        {
            TaskbarIcon SenderIcon = e.Parameter as TaskbarIcon;

            bool IsChecked;
            if (SenderIcon.ToggleChecked(e.Command, out IsChecked))
                ShowDarkChapel = IsChecked;
        }

        private static readonly string ShowDarkChapelSettingName = "ShowDarkChapel";
        #endregion

        #region Shared Calendar
        private void InitSharedCalendar()
        {
            ReadPostTime();
        }

        private void OnSharedCalendar(object sender, ExecutedRoutedEventArgs e)
        {
            string ExeName = Assembly.GetExecutingAssembly().Location;
            string ExeFolder = Path.GetDirectoryName(ExeName);
            string[] GoogleAssemblies = Directory.GetFiles(ExeFolder, "Google.*.dll");
            if (GoogleAssemblies.Length == 0)
            {
                MessageBox.Show("This feature is not available without Google assemblies.\r\n\r\nPlease return to the site from where this application was downloaded, to find instructions on how to get them.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ShareCalendarWindow Dlg = new ShareCalendarWindow();
            bool? Result = Dlg.ShowDialog();
            if (Result.HasValue && Result.Value)
            {
                if (Dlg.IsEventActive)
                {
                    if (PostTime == DateTime.MaxValue)
                    {
                        PostTime = App.Now();
                        App.SetSettingString("SharedCalendarPost", PostTime.ToString());
                    }

                    PostSharedEvents();
                }
                else
                {
                    PostTime = DateTime.MaxValue;
                    App.SetSettingString("SharedCalendarPost", null);
                }
            }
        }

        private void ReadPostTime()
        {
            PostTime = DateTime.MaxValue;

            string SharedCalendarPost = App.GetSettingString("SharedCalendarPost", null);
            if (SharedCalendarPost != null)
                DateTime.TryParse(SharedCalendarPost, out PostTime);
        }

        private void PostSharedEvents()
        {
            DateTime Now = App.Now();
            if (PostTime <= Now)
            {
                PostTime = Now + TimeSpan.FromHours(12);
                App.SetSettingString("SharedCalendarPost", PostTime.ToString());

                ShareCalendarWindow Dlg = new ShareCalendarWindow();
                Dlg.PostSharedEvents(MushroomInfoList);
            }
        }

        private DateTime PostTime;
        #endregion

        #region Events
        private void OnOpened(object sender, EventArgs e)
        {
            if (Taskbar.CurrentScreen != null)
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
                Size PopupSize = new Size((int)(MainChild.ActualWidth / RatioX), (int)(MainChild.ActualHeight / RatioY));

                Point RelativePosition = Taskbar.GetRelativePosition(MousePosition, PopupSize);

                RelativePosition = new Point(RelativePosition.X * RatioX, RelativePosition.Y * RatioY);

                HorizontalOffset = RelativePosition.X;
                VerticalOffset = RelativePosition.Y;
            }
            else
            {
                HorizontalOffset = 0;
                VerticalOffset = 0;
            }

            HwndSource source = (HwndSource)HwndSource.FromVisual(Child);
            if (!IsTopMostSet)
            {
                IsTopMostSet = true;
                SetWindowPos(source.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
            }

            SetForegroundWindow(source.Handle);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SaveMushroomInfoList();
            App.SetSettingBool(IsLockedSettingName, IsLocked);
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
            {
                string ExeName = Assembly.GetExecutingAssembly().Location;

                if (Scheduler.IsTaskActive(ExeName))
                {
                    RemoveFromStartupWindow Dlg = new RemoveFromStartupWindow();
                    Dlg.ShowDialog();
                }
                else
                {
                    LoadAtStartupWindow Dlg = new LoadAtStartupWindow();
                    Dlg.ShowDialog();
                }
            }
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

        private bool IsTopMostSet = false;
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

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        ///     Implements the PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameter is mandatory with [CallerMemberName]")]
        internal void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

            if (UpdateMushroomNameListTimer != null)
            {
                UpdateMushroomNameListTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                UpdateMushroomNameListTimer.Dispose();
                UpdateMushroomNameListTimer = null;
            }

            if (MushroomNameFileWatcher != null)
            {
                MushroomNameFileWatcher.EnableRaisingEvents = false;
                MushroomNameFileWatcher.Dispose();
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
