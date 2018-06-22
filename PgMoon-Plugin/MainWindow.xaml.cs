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
using TaskbarIconHost;

namespace PgMoon
{
    public partial class MainWindow : Popup, INotifyPropertyChanged, IDisposable
    {
        #region Init
        public MainWindow(TaskbarIconHost.IPluginSettings settings)
        {
            InitializeComponent();
            DataContext = this;

            Settings = settings;

            Placement = PlacementMode.Absolute;
            LastClosedTime = DateTime.MinValue;
            InitMoonPhase();
            InitCalendar();
            InitMushroomFarming();
            InitRahuBoat();
            InitDarkChapel();
            InitSharedCalendar();
        }

        public TaskbarIconHost.IPluginSettings Settings { get; private set; }
        #endregion

        #region Properties
        public bool IsElevated
        {
            get
            {
                if (!_IsElevated.HasValue)
                {
                    WindowsIdentity wi = WindowsIdentity.GetCurrent();
                    if (wi != null)
                    {
                        WindowsPrincipal wp = new WindowsPrincipal(wi);
                        if (wp != null)
                            _IsElevated = wp.IsInRole(WindowsBuiltInRole.Administrator);
                        else
                            _IsElevated = false;
                    }
                    else
                        _IsElevated = false;
                }

                return _IsElevated.Value;
            }
        }
        private bool? _IsElevated;

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

        public bool GetIsToolTipChanged()
        {
            bool Result = IsToolTipChanged;
            IsToolTipChanged = false;

            return Result;
        }

        public bool IsNextPhaseFullMoon
        {
            get { return PhaseCalculator.MoonPhase == MoonPhase.WaxingGibbousMoon; }
        }

        private void UpdateMoonPhaseTimerCallback(object Parameter)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new UpdateMoonPhaseHandler(OnUpdateMoonPhase));
        }

        private delegate void UpdateMoonPhaseHandler();

        private void OnUpdateMoonPhase()
        {
            //IncreaseNow(); //Debug only

            PhaseCalculator.Update();
            NotifyPropertyChanged(nameof(TimeToNextPhaseText));
            NotifyPropertyChanged(nameof(TimeToFullMoonText));
            NotifyPropertyChanged(nameof(IsFullMoon));
            NotifyPropertyChanged(nameof(IsNextPhaseFullMoon));
            NotifyPropertyChanged(nameof(ToolTipText));

            IsToolTipChanged = true;

            DateTime LastTimeKey = DateTime.MinValue;
            DateTime LastTime = DateTime.MinValue;

            foreach (KeyValuePair<DateTime, CalendarEntry> Entry in CalendarEntryTable)
            {
                CalendarEntry Item = Entry.Value;
                Item.Update();

                LastTimeKey = Entry.Key;
                LastTime = Item.EndTime;
            }

            if (LastTime < Now() && !PreviousUpdateChanged)
            {
                CalendarStartTime = LastTimeKey;
                BuildCalendar();
            }

            PreviousUpdateChanged = false;

            PostSharedEvents();
        }

        private Timer UpdateMoonPhaseTimer;
        private bool PreviousUpdateChanged;
        private bool IsToolTipChanged;
        #endregion

        #region Calendar
        private void InitCalendar()
        {
            _ShowCalendar = Settings.GetSettingBool(ShowCalendarSettingName, true);
            _CalendarStartTime = Now();
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

                    Settings.SetSettingBool(ShowCalendarSettingName, value);
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
                if (Year == Now().Year)
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
            CalendarStartTime = Now();
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
            CalendarStartTime = Now();
            BuildCalendar();
            PreviousUpdateChanged = true;
        }

        private static readonly string ShowCalendarSettingName = "ShowCalendar";
        private const int CalendarPageIncrement = 80;
        #endregion

        #region Mushroom Farming
        private void InitMushroomFarming()
        {
            _ShowMushroomFarming = Settings.GetSettingBool(ShowMushroomFarmingSettingName, true);
            _IsMushroomListLarge = false;
            _IsLocked = Settings.GetSettingBool(IsLockedSettingName, false);

            LoadMushroomInfoList();

            bool IsMushroomListInitialized = Settings.IsBoolKeySet(MushroomListInitializedName);
            Settings.SetSettingBool(MushroomListInitializedName, true);

            if (MushroomInfoList.Count == 0 && !IsMushroomListInitialized)
                ResetMushroomListToDefault(false);

            MushroomInfoList.Add(new MushroomInfo(Dispatcher, "", "", null, null));
            MushroomInfoList.CollectionChanged += OnMushroomInfoListChanged;

            string ApplicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PgJsonParse");
            MushroomNameFile = Path.Combine(ApplicationFolder, "Mushrooms.txt");
            UpdateMushroomNameListTimer = new Timer(new TimerCallback(UpdateMushroomNameListTimerCallback));
            UpdateMushroomNameListTimer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        private void LoadMushroomInfoList()
        {
            MushroomInfoList.Clear();

            string MushroomListSetting = Settings.GetSettingString(MushroomListSettingName, null);
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
                            MushroomInfoList.Add(new MushroomInfo(Dispatcher, Name, Comment, RobustGrowthPhase1, RobustGrowthPhase2));
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

            Settings.SetSettingString(MushroomListSettingName, Setting);
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

            NewList.Add(new MushroomInfo(Dispatcher, Name, Comment, RobustGrowthPhase1, RobustGrowthPhase2));
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

                    Settings.SetSettingBool(ShowMushroomFarmingSettingName, value);
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
                    MushroomInfoList.Add(new MushroomInfo(Dispatcher, "", "", null, null));
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
            _ShowRahuBoat = Settings.GetSettingBool(ShowRahuBoatSettingName, true);
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

                    Settings.SetSettingBool(ShowRahuBoatSettingName, value);
                    IsToolTipChanged = true;
                }
            }
        }
        private bool _ShowRahuBoat;

        private static readonly string ShowRahuBoatSettingName = "ShowRahuBoat";
        #endregion

        #region Dark Chapel
        private void InitDarkChapel()
        {
            _ShowDarkChapel = Settings.GetSettingBool(ShowDarkChapelSettingName, true);
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

                    Settings.SetSettingBool(ShowDarkChapelSettingName, value);
                }
            }
        }
        private bool _ShowDarkChapel;

        private static readonly string ShowDarkChapelSettingName = "ShowDarkChapel";
        #endregion

        #region Shared Calendar
        private void InitSharedCalendar()
        {
            ReadPostTime();
        }

        private void OnSharedCalendar(object sender, ExecutedRoutedEventArgs e)
        {
            OnSharedCalendar();
        }

        public void OnSharedCalendar()
        {
            string ExeName = Assembly.GetExecutingAssembly().Location;
            string ExeFolder = Path.GetDirectoryName(ExeName);
            string[] GoogleAssemblies = Directory.GetFiles(ExeFolder, "Google.*.dll");
            if (GoogleAssemblies.Length == 0)
            {
                MessageBox.Show("This feature is not available without Google assemblies.\r\n\r\nPlease return to the site from where this application was downloaded, to find instructions on how to get them.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ShareCalendarWindow Dlg = new ShareCalendarWindow(Settings);
            bool? Result = Dlg.ShowDialog();
            if (Result.HasValue && Result.Value)
            {
                if (Dlg.IsEventActive)
                {
                    if (PostTime == DateTime.MaxValue)
                    {
                        PostTime = Now();
                        Settings.SetSettingString("SharedCalendarPost", PostTime.ToString());
                    }

                    PostSharedEvents();
                }
                else
                {
                    PostTime = DateTime.MaxValue;
                    Settings.SetSettingString("SharedCalendarPost", null);
                }
            }
        }

        private void ReadPostTime()
        {
            PostTime = DateTime.MaxValue;

            string SharedCalendarPost = Settings.GetSettingString("SharedCalendarPost", null);
            if (SharedCalendarPost != null)
                DateTime.TryParse(SharedCalendarPost, out PostTime);
        }

        private void PostSharedEvents()
        {
            DateTime _Now = Now();
            if (PostTime <= _Now)
            {
                PostTime = _Now + TimeSpan.FromHours(12);
                Settings.SetSettingString("SharedCalendarPost", PostTime.ToString());

                ShareCalendarWindow Dlg = new ShareCalendarWindow(Settings);
                Dlg.PostSharedEvents(MushroomInfoList);
            }
        }

        private DateTime PostTime;
        #endregion

        #region Events
        private void OnOpened(object sender, EventArgs e)
        {
            Point RelativePosition = Taskbar.GetRelativePosition(Child as FrameworkElement);
            if (!double.IsNaN(RelativePosition.X) && !double.IsNaN(RelativePosition.X))
            {
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
            Settings.SetSettingBool(IsLockedSettingName, IsLocked);
            LastClosedTime = DateTime.UtcNow;
        }

        public void OnDeactivated()
        {
            IsOpen = false;
        }

        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void OnExit(object sender, ExecutedRoutedEventArgs e)
        {
            IsOpen = false;
            Application.Current.Shutdown();
        }

        public void IconClicked()
        {
            if (!IsOpen)
            {
                // We rely on time to avoid a flickering popup.
                if ((DateTime.UtcNow - LastClosedTime).TotalSeconds >= 1.0)
                    IsOpen = true;
                else
                    LastClosedTime = DateTime.MinValue;
            }
        }

        private DateTime LastClosedTime;
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

        #region Current Time
        // Use this for debugging purpose only.
        public static void IncreaseNow()
        {
            TimeOffset += TimeSpan.FromDays(1);
        }

        public static DateTime Now()
        {
            return DateTime.UtcNow + TimeOffset;
        }

        private static TimeSpan TimeOffset = TimeSpan.Zero;
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
