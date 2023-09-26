namespace PgMoon;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
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
using RegistryTools;
using TaskbarIconHost;
using Tracing;

/// <summary>
/// Represents the application main window.
/// </summary>
public partial class MainWindow : Popup, INotifyPropertyChanged, IDisposable
{
    #region Init
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="settings">The registry settings.</param>
    /// <param name="logger">an interface to log events asynchronously.</param>
    public MainWindow(Settings settings, ITracer logger)
    {
        InitializeComponent();
        DataContext = this;

        Settings = settings;
        Logger = logger;

        Placement = PlacementMode.Absolute;
        LastClosedTime = DateTime.MinValue;
        InitMoonPhase();
        InitCalendar();
        InitMushroomFarming();
        InitRahuBoat();
        InitDarkChapel();
        InitSharedCalendar();
    }

    /// <summary>
    /// Gets the registry settings.
    /// </summary>
    public Settings Settings { get; private set; }

    /// <summary>
    /// Gets an interface to log events asynchronously.
    /// </summary>
    public ITracer Logger { get; private set; }
    #endregion

    #region Properties
    /// <summary>
    /// Gets a value indicating whether the application is running as administrator.
    /// </summary>
    public bool IsElevated
    {
        get
        {
            if (!IsElevatedInternal.HasValue)
            {
                WindowsIdentity? wi = WindowsIdentity.GetCurrent();
                if (wi is not null)
                {
                    WindowsPrincipal wp = new(wi);
                    IsElevatedInternal = wp.IsInRole(WindowsBuiltInRole.Administrator);
                }
                else
                    IsElevatedInternal = false;
            }

            return IsElevatedInternal.Value;
        }
    }

    private bool? IsElevatedInternal;

    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    public string ToolTipText
    {
        get
        {
            string Result = $"{PhaseCalculator.MoonPhase.Name}\r\n{TimeToNextPhaseText}";

            if (ShowRahuBoat)
                Result += $"\r\n{CalendarEntry.RahuBoatDestinationShortText}: {PhaseCalculator.MoonPhase.RahuBoatDestination}";

            Result += $"\r\n{CalendarEntry.PortToCircleShortText}: {PhaseCalculator.MoonPhase.FastPortMushroomShortText}";

            return Result;
        }
    }
    #endregion

    #region Moon Phase
    private void InitMoonPhase()
    {
        PreviousUpdateChanged = true;
        UpdateMoonPhaseTimer = SafeTimer.Create(OnUpdateMoonPhase, TimeSpan.FromSeconds(60), Logger);
    }

    /// <summary>
    /// Gets the time to next phase.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public string TimeToNextPhaseText
#pragma warning restore CA1822 // Mark members as static
    {
        get { return FriendlyTimeString(PhaseCalculator.TimeToNextPhase, "Changing soon"); }
    }

    /// <summary>
    /// Gets the time to full moon.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public string TimeToFullMoonText
#pragma warning restore CA1822 // Mark members as static
    {
        get { return FriendlyTimeString(PhaseCalculator.TimeToFullMoon, "Soon"); }
    }

    private static string FriendlyTimeString(TimeSpan duration, string soonText)
    {
        string Result = string.Empty;

        if (duration.TotalDays >= 1)
        {
            int Days = (int)duration.TotalDays;
            duration -= TimeSpan.FromDays(Days);

            if (Days > 1)
                Result += $"{Days.ToString(CultureInfo.InvariantCulture)} days";
            else
                Result += $"{Days.ToString(CultureInfo.InvariantCulture)} day";
        }

        if (duration.TotalHours >= 1)
        {
            int Hours = (int)duration.TotalHours;
            duration -= TimeSpan.FromHours(Hours);

            if (Result.Length > 0)
                Result += ", ";

            if (Hours > 1)
                Result += $"{Hours.ToString(CultureInfo.InvariantCulture)} hours";
            else
                Result += $"{Hours.ToString(CultureInfo.InvariantCulture)} hour";
        }

        if (Result.Length == 0)
            return soonText;
        else
            return $"{Result} left";
    }

    /// <summary>
    /// Gets a value indicating whether the current Moon phase is Full Moon.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public bool IsFullMoon
#pragma warning restore CA1822 // Mark members as static
    {
        get { return PhaseCalculator.MoonPhase == MoonPhase.FullMoon; }
    }

    /// <summary>
    /// Gets a value indicating whether the tooltip changed asn resets this flag.
    /// </summary>
    /// <returns>True if the tooltip changed; otherwise, false.</returns>
    public bool GetIsToolTipChanged()
    {
        bool Result = IsToolTipChanged;
        IsToolTipChanged = false;

        return Result;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the tooltip changed.
    /// </summary>
#pragma warning disable CA1721 // Property names should not match get methods
    public bool IsToolTipChanged { get; set; }
#pragma warning restore CA1721 // Property names should not match get methods

    /// <summary>
    /// Gets a value indicating whether the next Moon phase is Full Moon.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public bool IsNextPhaseFullMoon
#pragma warning restore CA1822 // Mark members as static
    {
        get { return PhaseCalculator.MoonPhase == MoonPhase.WaxingGibbousMoon; }
    }

    private delegate void UpdateMoonPhaseHandler();

    private void OnUpdateMoonPhase()
    {
        UpdateMoonPhaseTimer?.NotifyCallbackCalled();

        // IncreaseNow(); //Debug only
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

    private SafeTimer? UpdateMoonPhaseTimer;
    private bool PreviousUpdateChanged;
    #endregion

    #region Calendar
    private void InitCalendar()
    {
        ShowCalendarInternal = Settings.GetBool(ShowCalendarSettingName, true);
        CalendarStartTimeInternal = Now();
        BuildCalendar();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the calendar should be shown.
    /// </summary>
    public bool ShowCalendar
    {
        get { return ShowCalendarInternal; }
        set
        {
            if (ShowCalendarInternal != value)
            {
                ShowCalendarInternal = value;
                NotifyThisPropertyChanged();

                Settings.SetBool(ShowCalendarSettingName, value);
            }
        }
    }

    private bool ShowCalendarInternal;

    /// <summary>
    /// Gets or sets the calendar start time.
    /// </summary>
    public DateTime CalendarStartTime
    {
        get { return CalendarStartTimeInternal; }
        set
        {
            if (CalendarStartTimeInternal != value)
            {
                CalendarStartTimeInternal = value;
                NotifyThisPropertyChanged();
                NotifyPropertyChanged(nameof(CalendarStartTimeYear));
            }
        }
    }

    private DateTime CalendarStartTimeInternal;

    /// <summary>
    /// Gets the calendar start time year.
    /// </summary>
    public string CalendarStartTimeYear
    {
        get
        {
            int Year = CalendarStartTime.Year;

            if (Year == Now().Year)
                return string.Empty;
            else
                return CalendarStartTime.Year.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Gets the list of calendar entries.
    /// </summary>
    public ObservableCollection<CalendarEntry> CalendarEntryList { get; private set; } = new ObservableCollection<CalendarEntry>();

    private void BuildCalendar()
    {
        CalendarEntryList.Clear();

        DateTime Time = CalendarStartTime;

        for (int i = 0; i < 5; i++)
        {
            CalendarEntry CalendarEntry;

            if (!CalendarEntryTable.ContainsKey(Time))
            {
                PhaseCalculator.DateTimeToMoonPhase(Time, out int MoonMonth, out MoonPhase MoonPhase, out DateTime PhaseStartTime, out DateTime PhaseEndTime, out _, out _);

                if (!CalendarEntryTable.ContainsKey(PhaseStartTime))
                {
                    CalendarEntry NewCalendarEntry = new(MoonMonth, MoonPhase, PhaseStartTime, PhaseEndTime, MushroomInfoList);
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

    private const string ShowCalendarSettingName = "ShowCalendar";
    private Dictionary<DateTime, CalendarEntry> CalendarEntryTable = new();
    #endregion

    #region Mushroom Farming
    private void InitMushroomFarming()
    {
        ShowMushroomFarmingInternal = Settings.GetBool(ShowMushroomFarmingSettingName, true);
        IsMushroomListLargeInternal = false;
        IsLockedInternal = Settings.GetBool(IsLockedSettingName, false);

        LoadMushroomInfoList();

        bool IsMushroomListInitialized = Settings.IsValueSet(MushroomListInitializedName);
        Settings.SetBool(MushroomListInitializedName, true);

        if (MushroomInfoList.Count == 0 && !IsMushroomListInitialized)
            ResetMushroomListToDefault(false);

        MushroomInfoList.Add(new MushroomInfo(Dispatcher, string.Empty, string.Empty, null, null));
        MushroomInfoList.CollectionChanged += OnMushroomInfoListChanged;

        string ApplicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PgJsonParse");
        MushroomNameFile = Path.Combine(ApplicationFolder, "Mushrooms.txt");
        UpdateMushroomNameListTimer = new Timer(new TimerCallback(UpdateMushroomNameListTimerCallback));
        UpdateMushroomNameListTimer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
    }

    private void LoadMushroomInfoList()
    {
        MushroomInfoList.Clear();

        string? MushroomListSetting = Settings.GetString(MushroomListSettingName, string.Empty);
        if (MushroomListSetting is not null)
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
                            if (int.TryParse(Line[1].Trim(), out int PhaseIndex1) && PhaseIndex1 >= 0 && PhaseIndex1 + 1 < MoonPhase.MoonPhaseList.Count)
                                SelectedPhase1 = PhaseIndex1;
                            else
                                SelectedPhase1 = -1;

                            if (int.TryParse(Line[2].Trim(), out int PhaseIndex2) && PhaseIndex2 >= 0 && PhaseIndex2 + 1 < MoonPhase.MoonPhaseList.Count)
                                SelectedPhase2 = PhaseIndex2;
                            else
                                SelectedPhase2 = -1;
                        }
                        else
                        {
                            SelectedPhase1 = -1;
                            SelectedPhase2 = -1;
                        }

                        string Comment = (Line.Length > 3) ? Line[3] : string.Empty;

                        MoonPhase? RobustGrowthPhase1 = SelectedPhase1 >= 0 ? MoonPhase.MoonPhaseList[SelectedPhase1] : null;
                        MoonPhase? RobustGrowthPhase2 = SelectedPhase2 >= 0 ? MoonPhase.MoonPhaseList[SelectedPhase2] : null;
                        MushroomInfoList.Add(new MushroomInfo(Dispatcher, Name, Comment, RobustGrowthPhase1, RobustGrowthPhase2));
                    }
                }
            }
        }
    }

    private void SaveMushroomInfoList()
    {
        string Setting = string.Empty;

        foreach (MushroomInfo Info in MushroomInfoList)
        {
            if (Info.Name.Length == 0)
                continue;

            string Line = string.Empty;

            Line += Info.Name;
            Line += MushroomSeparator;

            if (Info.SelectedMoonPhase1 >= 0)
                Line += Info.SelectedMoonPhase1.ToString(CultureInfo.InvariantCulture);

            Line += MushroomSeparator;

            if (Info.SelectedMoonPhase2 >= 0)
                Line += Info.SelectedMoonPhase2.ToString(CultureInfo.InvariantCulture);

            Line += MushroomSeparator;
            Line += Info.Comment;

            if (Setting.Length > 0)
                Setting += MushroomListSeparator;

            Setting += Line;
        }

        Settings.SetString(MushroomListSettingName, Setting);
    }

    private void ResetMushroomListToDefault(bool keepComment)
    {
        List<MushroomInfo> NewList = new();

        AddMushroomToList(NewList, MoonPhase.ParasolMushroomLongName, MoonPhase.FullMoon, MoonPhase.WaningCrescentMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.MycenaMushroomLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.FirstQuarterMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.BoletusMushroomLongName, MoonPhase.NewMoon, MoonPhase.WaningGibbousMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.FieldMushroomLongName, MoonPhase.WaxingGibbousMoon, MoonPhase.LastQuarterMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.BlusherMushroomLongName, MoonPhase.NewMoon, MoonPhase.WaningGibbousMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.GoblinPuffballLongName, MoonPhase.NewMoon, MoonPhase.WaxingGibbousMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.MilkCapMushroomLongName, MoonPhase.FullMoon, MoonPhase.WaningCrescentMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.BloodMushroomLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.LastQuarterMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.CoralMushroomLongName, MoonPhase.FirstQuarterMoon, MoonPhase.WaxingGibbousMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.IocaineMushroomLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.FirstQuarterMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.GroxmakMushroomLongName, MoonPhase.WaxingGibbousMoon, MoonPhase.LastQuarterMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.PorciniMushroomLongName, MoonPhase.FullMoon, MoonPhase.WaningGibbousMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.BlackFootMorelLongName, MoonPhase.NewMoon, MoonPhase.WaningCrescentMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.PixiesParasolLongName, MoonPhase.FirstQuarterMoon, MoonPhase.WaxingGibbousMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.FlyAmanitaLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.FullMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.BlastcapMushroomLongName, MoonPhase.FullMoon, MoonPhase.WaningGibbousMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.ChargedMyceliumLongName, MoonPhase.NewMoon, MoonPhase.WaxingGibbousMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.FalseAgaricLongName, MoonPhase.WaningCrescentMoon, MoonPhase.LastQuarterMoon, keepComment);
        AddMushroomToList(NewList, MoonPhase.WizardsMushroomLongName, MoonPhase.WaxingCrescentMoon, MoonPhase.FirstQuarterMoon, keepComment);

        MushroomInfoList.Clear();
        foreach (MushroomInfo Item in NewList)
            MushroomInfoList.Add(Item);
    }

    private void AddMushroomToList(List<MushroomInfo> newList, string name, MoonPhase? robustGrowthPhase1, MoonPhase? robustGrowthPhase2, bool keepComment)
    {
        string Comment = string.Empty;

        if (keepComment)
        {
            foreach (MushroomInfo Item in MushroomInfoList)
                if (Item.Name == name)
                {
                    Comment = Item.Comment;
                    break;
                }
        }

        newList.Add(new MushroomInfo(Dispatcher, name, Comment, robustGrowthPhase1, robustGrowthPhase2));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the mushroom farming should be shown.
    /// </summary>
    public bool ShowMushroomFarming
    {
        get { return ShowMushroomFarmingInternal; }
        set
        {
            if (ShowMushroomFarmingInternal != value)
            {
                ShowMushroomFarmingInternal = value;
                NotifyThisPropertyChanged();

                Settings.SetBool(ShowMushroomFarmingSettingName, value);
            }
        }
    }

    private bool ShowMushroomFarmingInternal;

    /// <summary>
    /// Gets a value indicating whether the mushroom list is small.
    /// </summary>
    public bool IsMushroomListSmall
    {
        get { return MushroomInfoList.Count < 2; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the mushroom list is large.
    /// </summary>
    public bool IsMushroomListLarge
    {
        get { return IsMushroomListLargeInternal; }
        set
        {
            if (IsMushroomListLargeInternal != value)
            {
                IsMushroomListLargeInternal = value;
                NotifyThisPropertyChanged();
            }
        }
    }

    private bool IsMushroomListLargeInternal;

    /// <summary>
    /// Gets or sets a value indicating whether the mushroom list is locked.
    /// </summary>
    public bool IsLocked
    {
        get { return IsLockedInternal; }
        set
        {
            if (IsLockedInternal != value)
            {
                IsLockedInternal = value;
                NotifyThisPropertyChanged();
            }
        }
    }

    private bool IsLockedInternal;

    /// <summary>
    /// Gets the mushroom info list.
    /// </summary>
    public ObservableCollection<MushroomInfo> MushroomInfoList { get; } = new();

    /// <summary>
    /// Gets the mushroom name list.
    /// </summary>
    public ObservableCollection<string>? MushroomNameList { get; private set; }

    private void OnMushroomNameFileChanged(object sender, FileSystemEventArgs e)
    {
        UpdateMushroomNameListTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
    }

    private void UpdateMushroomNameListTimerCallback(object? parameter)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, OnUpdateMushroomNameList);
    }

    private void OnUpdateMushroomNameList()
    {
        if (MushroomNameList is not null)
            MushroomNameList.Clear();

        if (!File.Exists(MushroomNameFile))
            return;

        try
        {
            using (FileStream Stream = new(MushroomNameFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader Reader = new(Stream, Encoding.ASCII))

            for (; ;)
            {
                string? MushroomName = Reader.ReadLine();

                if (MushroomName is null || MushroomName.Length == 0)
                    break;

                if (MushroomNameList is null)
                    MushroomNameList = new ObservableCollection<string>();

                MushroomNameList.Add(MushroomName);
            }

            if (MushroomNameList is not null && MushroomNameFileWatcher is null)
            {
                Contracts.Contract.RequireNotNull(Path.GetDirectoryName(MushroomNameFile), out string FolderPath);
                Contracts.Contract.RequireNotNull(Path.GetFileName(MushroomNameFile), out string FileName);

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

    private void OnMushroomInfoListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyPropertyChanged(nameof(IsMushroomListSmall));
    }

    private void OnMushroomListUp(object sender, MouseButtonEventArgs e)
    {
        ScrollViewer listviewMushrooms = (ScrollViewer)LocateSibling(sender, nameof(listviewMushrooms));
        listviewMushrooms.LineUp();
    }

    private void OnMushroomListUpPage(object sender, MouseButtonEventArgs e)
    {
        ScrollViewer listviewMushrooms = (ScrollViewer)LocateSibling(sender, nameof(listviewMushrooms));
        listviewMushrooms.PageUp();
    }

    private void OnMushroomListDown(object sender, MouseButtonEventArgs e)
    {
        ScrollViewer listviewMushrooms = (ScrollViewer)LocateSibling(sender, nameof(listviewMushrooms));
        listviewMushrooms.LineDown();
    }

    private void OnMushroomListDownPage(object sender, MouseButtonEventArgs e)
    {
        ScrollViewer listviewMushrooms = (ScrollViewer)LocateSibling(sender, nameof(listviewMushrooms));
        listviewMushrooms.PageDown();
    }

    private void OnLockMushroomList(object sender, MouseButtonEventArgs e)
    {
        FrameworkElement Element = (FrameworkElement)sender;
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

    private static object LocateSibling(object sender, string elementName)
    {
        FrameworkElement? CurrentElement = sender as FrameworkElement;
        object? SiblingElement = null;

        while (CurrentElement is not null && SiblingElement is null)
        {
            CurrentElement = VisualTreeHelper.GetParent(CurrentElement) as FrameworkElement;

            if (CurrentElement is not null)
                SiblingElement = LogicalTreeHelper.FindLogicalNode(CurrentElement, elementName);
        }

        return SiblingElement !;
    }

    private void OnMushroomListSizeChanged(object sender, SizeChangedEventArgs e)
    {
        FrameworkElement Control = (FrameworkElement)sender;

        if (!double.IsNaN(Control.ActualHeight) && !double.IsNaN(Control.MaxHeight))
            IsMushroomListLarge = Control.ActualHeight >= Control.MaxHeight;
    }

    private void OnMushroomNameValidationError(object sender, ValidationErrorEventArgs e)
    {
        ComboBox Control = (ComboBox)sender;
        MushroomInfo Line = (MushroomInfo)Control.DataContext;
        Line.Name = string.Empty;
    }

    private void OnMushroomNameLostFocus(object sender, RoutedEventArgs e)
    {
        if (MushroomInfoList.Count > 0 && MushroomInfoList.Count < MaxMushroomRows)
        {
            if (MushroomInfoList[MushroomInfoList.Count - 1].Name.Length > 0)
                MushroomInfoList.Add(new MushroomInfo(Dispatcher, string.Empty, string.Empty, null, null));
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
        FrameworkElement Element = (FrameworkElement)sender;
        ContextMenu ContextMenu = Element.ContextMenu;
        ContextMenu.IsOpen = true;
        ContextMenu.PlacementTarget = this;
        ContextMenu.Closed += OnContextMenuClosed;
    }

    private void OnContextMenuClosed(object sender, RoutedEventArgs e)
    {
        ContextMenu ContextMenu = (ContextMenu)sender;
        ContextMenu.Closed -= OnContextMenuClosed;
        ContextMenu.PlacementTarget = null;
    }

    private const string ShowMushroomFarmingSettingName = "ShowMushroomFarming";
    private const string MushroomListSettingName = "MushroomList";
    private const string MushroomListInitializedName = "MushroomListInitialized";
    private const string IsLockedSettingName = "MushroomListLocked";
    private const int MaxMushroomRows = 40;
    private const char MushroomListSeparator = '\u2551';
    private const char MushroomSeparator = '\u2550';
    private string MushroomNameFile = string.Empty;
    private FileSystemWatcher? MushroomNameFileWatcher;
    private Timer UpdateMushroomNameListTimer = new(new TimerCallback((object? state) => { }));
    #endregion

    #region Rahu Boat
    private void InitRahuBoat()
    {
        ShowRahuBoatInternal = Settings.GetBool(ShowRahuBoatSettingName, true);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the rahu boat should be shown.
    /// </summary>
    public bool ShowRahuBoat
    {
        get { return ShowRahuBoatInternal; }
        set
        {
            if (ShowRahuBoatInternal != value)
            {
                ShowRahuBoatInternal = value;
                NotifyThisPropertyChanged();

                Settings.SetBool(ShowRahuBoatSettingName, value);
                IsToolTipChanged = true;
            }
        }
    }

    private bool ShowRahuBoatInternal;

    private const string ShowRahuBoatSettingName = "ShowRahuBoat";
    #endregion

    #region Dark Chapel
    private void InitDarkChapel()
    {
        ShowDarkChapelInternal = Settings.GetBool(ShowDarkChapelSettingName, true);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the dark chapel statue should be shown.
    /// </summary>
    public bool ShowDarkChapel
    {
        get { return ShowDarkChapelInternal; }
        set
        {
            if (ShowDarkChapelInternal != value)
            {
                ShowDarkChapelInternal = value;
                NotifyThisPropertyChanged();

                Settings.SetBool(ShowDarkChapelSettingName, value);
            }
        }
    }

    private bool ShowDarkChapelInternal;

    private const string ShowDarkChapelSettingName = "ShowDarkChapel";
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

    /// <summary>
    /// Handles the shared calendar event.
    /// </summary>
    public void OnSharedCalendar()
    {
        using ShareCalendarWindow Dlg = new(Settings);
        bool? Result = Dlg.ShowDialog();
        if (Result.HasValue && Result.Value)
        {
            if (Dlg.IsEventActive)
            {
                if (PostTime == DateTime.MaxValue)
                {
                    PostTime = Now();
                    Settings.SetString("SharedCalendarPost", PostTime.ToString(CultureInfo.InvariantCulture));
                }

                PostSharedEvents();
            }
            else
            {
                PostTime = DateTime.MaxValue;
                Settings.SetString("SharedCalendarPost", string.Empty);
            }
        }
    }

    private void ReadPostTime()
    {
        PostTime = DateTime.MaxValue;

        string SharedCalendarPost = Settings.GetString("SharedCalendarPost", string.Empty);
        if (SharedCalendarPost is not null)
            DateTime.TryParse(SharedCalendarPost, CultureInfo.InvariantCulture, DateTimeStyles.None, out PostTime);
    }

    private void PostSharedEvents()
    {
        DateTime _Now = Now();
        if (PostTime <= _Now)
        {
            PostTime = _Now + TimeSpan.FromHours(12);
            Settings.SetString("SharedCalendarPost", PostTime.ToString(CultureInfo.InvariantCulture));

            using ShareCalendarWindow Dlg = new(Settings);
            Dlg.PostSharedEvents(MushroomInfoList);
        }
    }

    private DateTime PostTime;
    #endregion

    #region Events
    private void OnOpened(object sender, EventArgs e)
    {
        Point RelativePosition = TaskbarTools.TaskbarLocation.GetRelativePosition((FrameworkElement)Child);
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

        HwndSource source = (HwndSource)PresentationSource.FromVisual(Child);

        if (!IsTopMostSet)
        {
            IsTopMostSet = true;
            NativeMethods.SetWindowPos(source.Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW | NativeMethods.SWP_NOACTIVATE);
        }

        NativeMethods.SetForegroundWindow(source.Handle);
    }

    private void OnClosed(object sender, EventArgs e)
    {
        SaveMushroomInfoList();
        Settings.SetBool(IsLockedSettingName, IsLocked);
        LastClosedTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Handles the deactivated event.
    /// </summary>
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

    /// <summary>
    /// Handles the incon clicked event.
    /// </summary>
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
    private bool IsTopMostSet;
    #endregion

    #region Current Time
    /// <summary>
    /// Use this for debugging purpose only.
    /// </summary>
    public static void IncreaseNow()
    {
        TimeOffset += TimeSpan.FromDays(1);
    }

    /// <summary>
    /// Gets the current time with the offset applied.
    /// </summary>
    public static DateTime Now()
    {
        return DateTime.UtcNow + TimeOffset;
    }

    private static TimeSpan TimeOffset = TimeSpan.Zero;
    #endregion

    #region Implementation of INotifyPropertyChanged
    /// <summary>
    /// Implements the PropertyChanged event.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Invoke handlers of the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Invoke handlers of the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Implementation of IDisposable
    /// <summary>
    /// Called when an object should release its resources.
    /// </summary>
    /// <param name="isDisposing">Indicates if resources must be disposed now.</param>
    protected virtual void Dispose(bool isDisposing)
    {
        if (!IsDisposed)
        {
            IsDisposed = true;

            if (isDisposing)
                DisposeNow();
        }
    }

    /// <summary>
    /// Called when an object should release its resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="MainWindow"/> class.
    /// </summary>
    ~MainWindow()
    {
        Dispose(false);
    }

    /// <summary>
    /// True after <see cref="Dispose(bool)"/> has been invoked.
    /// </summary>
    private bool IsDisposed;

    /// <summary>
    /// Disposes of every reference that must be cleaned up.
    /// </summary>
    private void DisposeNow()
    {
        SafeTimer.Destroy(ref UpdateMoonPhaseTimer);

        using (UpdateMushroomNameListTimer)
        {
            UpdateMushroomNameListTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        if (MushroomNameFileWatcher is not null)
        {
            MushroomNameFileWatcher.EnableRaisingEvents = false;
            MushroomNameFileWatcher.Dispose();
            MushroomNameFileWatcher = null;
        }
    }
    #endregion
}
