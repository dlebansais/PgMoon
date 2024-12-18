﻿namespace PgMoon;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a moon phase calculator.
/// </summary>
public partial class PhaseCalculator
{
    #region Init
    static PhaseCalculator()
    {
        Singleton = new PhaseCalculator();
    }

    private PhaseCalculator()
    {
        CalculateBasicMoonPhase(MainWindow.Now());
    }

    public static PhaseCalculator Singleton { get; }
    #endregion

    #region Client Interface
    public static void Update() => Singleton.CalculateBasicMoonPhase(MainWindow.Now());

    public static bool IsCurrent(int checkMoonMonth, MoonPhase checkMoonPhase) => checkMoonMonth == MoonMonth && checkMoonPhase == MoonPhase;
    #endregion

    #region Properties
    public static int MoonMonth
    {
        get => MoonMonthInternal;
        set
        {
            if (MoonMonthInternal != value)
            {
                MoonMonthInternal = value;
                NotifyThisPropertyChanged();
            }
        }
    }

    private static int MoonMonthInternal;

    public static MoonPhase MoonPhase
    {
        get => MoonPhaseInternal;
        set
        {
            if (MoonPhaseInternal != value)
            {
                MoonPhaseInternal = value;
                NotifyThisPropertyChanged();

                MoonPhase.UpdateCurrent();
            }
        }
    }

    private static MoonPhase MoonPhaseInternal = MoonPhase.NullMoonPhase;

    public static double ProgressWithinPhase
    {
        get => ProgressWithinPhaseInternal;
        set
        {
            if (ProgressWithinPhaseInternal != value)
            {
                ProgressWithinPhaseInternal = value;
                NotifyThisPropertyChanged();
            }
        }
    }

    private static double ProgressWithinPhaseInternal;

    public static double ProgressToFullMoon
    {
        get => ProgressToFullMoonInternal;
        set
        {
            if (ProgressToFullMoonInternal != value)
            {
                ProgressToFullMoonInternal = value;
                NotifyThisPropertyChanged();
            }
        }
    }

    private static double ProgressToFullMoonInternal;

    public static TimeSpan TimeToNextPhase
    {
        get => TimeToNextPhaseInternal;
        set
        {
            if (TimeToNextPhaseInternal != value)
            {
                TimeToNextPhaseInternal = value;
                NotifyThisPropertyChanged();
            }
        }
    }

    private static TimeSpan TimeToNextPhaseInternal;

    public static TimeSpan TimeToFullMoon
    {
        get => TimeToFullMoonInternal;
        set
        {
            if (TimeToFullMoonInternal != value)
            {
                TimeToFullMoonInternal = value;
                NotifyThisPropertyChanged();
            }
        }
    }

    private static TimeSpan TimeToFullMoonInternal;
    #endregion

    #region Client Interface
    public static void DateTimeToMoonPhase(DateTime time, out int pMoonMonth, out MoonPhase pMoonPhase, out DateTime pPhaseStartTime, out DateTime pPhaseEndTime, out double pProgressToFullMoon, out DateTime pNextFullMoonTime)
    {
        int ip = corrected_moon_phase(time);
        TimeSpan time_increment = TimeSpan.FromMinutes(10);

        pMoonPhase = MoonPhase.MoonPhaseList[ip];

        int previp;
        DateTime PhaseStartTime = time;
        do
        {
            PhaseStartTime -= time_increment;
            previp = corrected_moon_phase(PhaseStartTime);
        }
        while (previp == ip);

        PhaseStartTime += time_increment;
        pPhaseStartTime = PhaseStartTime;

        pMoonMonth = (int)(to_time_t(PhaseStartTime) / 2551442.861);

        int nextip;
        DateTime PhaseEndTime = time;
        do
        {
            PhaseEndTime += time_increment;
            nextip = corrected_moon_phase(PhaseEndTime);
        }
        while (nextip == ip);

        pPhaseEndTime = PhaseEndTime;

        if (ip != 4)
        {
            DateTime AfterFullMoonStartTime = PhaseStartTime;
            DateTime FullMoonEndTime = PhaseEndTime;

            if (previp != 4)
            {
                do
                {
                    AfterFullMoonStartTime -= time_increment;
                    previp = corrected_moon_phase(AfterFullMoonStartTime);
                }
                while (previp != 4);

                AfterFullMoonStartTime += time_increment;
            }

            if (nextip != 4)
            {
                do
                {
                    FullMoonEndTime += time_increment;
                    nextip = corrected_moon_phase(FullMoonEndTime);
                }
                while (nextip != 4);
            }

            double Total = FullMoonEndTime.Ticks - AfterFullMoonStartTime.Ticks;
            double Current = time.Ticks - AfterFullMoonStartTime.Ticks;

            pProgressToFullMoon = Current / Total;
            pNextFullMoonTime = FullMoonEndTime;
        }
        else
        {
            pProgressToFullMoon = 0;
            pNextFullMoonTime = DateTime.MinValue;
        }
    }
    #endregion

    #region Unix Time
    private static readonly DateTime UnixReferenceTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
    private static long to_time_t(DateTime time) => (long)(time - UnixReferenceTime).TotalSeconds;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1300 // Element should begin with upper-case letter

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles
    private static DateTime from_time_t(long time_t) => UnixReferenceTime + TimeSpan.FromSeconds(time_t);
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore SA1300 // Element should begin with upper-case letter
    #endregion

    #region Calculator
    private const double OrbitalPeriod = 27.321661;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
    private static int corrected_moon_phase(DateTime utcTime)
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1300 // Element should begin with upper-case letter
    {
        // Reference is not GMT but EST, subject to a daylight saving time different than local.
        // LunationTable is UTC time.
        TimeZoneInfo EST = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        DateTime EstTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, EST);

        // Fallback for very early times.
        if (EstTime < LunationTable[0].NewMoon)
            return (7 - (int)((LunationTable[0].NewMoon - EstTime).TotalDays / OrbitalPeriod)) % 8;

        DateTime[] Moons = new DateTime[9];

        for (int i = 0; i < LunationTable.Length; i++)
        {
            // Gets dates for this cycle.
            Moons[0] = LunationTable[i].NewMoon;
            Moons[1] = LunationTable[i].WaxingCrescentMoon;
            Moons[2] = LunationTable[i].QuarterMoon;
            Moons[3] = LunationTable[i].WaxingGibbousMoon;
            Moons[4] = LunationTable[i].FullMoon;
            Moons[5] = LunationTable[i].WaningGibbousMoon;
            Moons[6] = LunationTable[i].LastQuarterMoon;
            Moons[7] = LunationTable[i].WaningCrescentMoon;
            Moons[8] = i + 1 < LunationTable.Length ? LunationTable[i + 1].NewMoon : LunationTable[i].NewMoon + LunationTable[i].Duration;

            for (int j = 0; j < 8; j++)
            {
                if (utcTime >= Moons[j] && utcTime < Moons[j + 1])
                    return j;
            }
        }

        // Fallback for very late times.
        return ((int)((EstTime - Moons[4]).TotalDays / OrbitalPeriod)) % 8;
    }

#pragma warning disable IDE0051 // Remove unused private members
    private static int QuarterCycleMoonPhase(DateTime startTime, DateTime time, DateTime endTime)
#pragma warning restore IDE0051 // Remove unused private members
    {
        // Gets the end time of the first phase of this quarter cycle.
        DateTime FirstPhaseEndTime = startTime + TimeSpan.FromDays(3);

        // Still within this first phase?
        if (time < FirstPhaseEndTime)
        {
            return 0;
        }
        else
        {
            // Gets the start time of the last phase of this quarter cycle.
            DateTime LastPhaseStartTime = endTime;

            // Already within this last phase? If not, we're in between (a Waxing or Waning phase).
            return time >= LastPhaseStartTime ? 2 : 1;
        }
    }

#pragma warning disable CA1822 // Mark members as static
    private void CalculateBasicMoonPhase(DateTime time)
#pragma warning restore CA1822 // Mark members as static
    {
        DateTimeToMoonPhase(time, out int NewMoonMonth, out MoonPhase NewMoonPhase, out DateTime NewPhaseStartTime, out DateTime NewPhaseEndTime, out double NewProgressToFullMoon, out DateTime NewNextFullMoonTime);

        double NewProgressWithinPhase = (time - NewPhaseStartTime).TotalSeconds / (NewPhaseEndTime - NewPhaseStartTime).TotalSeconds;
        TimeSpan NewTimeToNextPhase = NewPhaseEndTime - time;
        TimeSpan NewTimeToFullMoon = NewNextFullMoonTime - time;

        MoonMonth = NewMoonMonth;
        MoonPhase = NewMoonPhase;
        ProgressWithinPhase = NewProgressWithinPhase;
        TimeToNextPhase = NewTimeToNextPhase;
        ProgressToFullMoon = NewProgressToFullMoon;
        TimeToFullMoon = NewTimeToFullMoon;
    }
    #endregion

    #region Moon Phase Calendar
    private static DateTime ParseDateTime(string s)
    {
        DateTime Result = DateTime.Parse(s, CultureInfo.InvariantCulture);
        DateTime NewResult = DateTime.Parse(s, CultureInfo.GetCultureInfo("en-US"));

#if DEBUGDATE
        using FileStream Stream = new("lunation.txt", IsCreated ? FileMode.Append : FileMode.Create, FileAccess.Write);
        using StreamWriter Writer = new(Stream);

        if (!IsCreated)
            IsCreated = true;

        string Tag = string.Empty;
        if (Result.Hour > 23 || Result.Hour < 1)
            Tag = " <==";

        string Line = $"Result: {Result}, New Result: {NewResult}{Tag}";
        Writer.WriteLine(Line);
#endif
        return Result;
    }

#if DEBUGDATE
    private static bool IsCreated;
#endif

    private static TimeSpan ParseTimeSpan(string s)
    {
        TimeSpan Result = TimeSpan.Parse(s, CultureInfo.InvariantCulture);
        return Result;
    }
    #endregion

    #region Implementation of STATIC INotifyPropertyChanged
    /// <summary>
    ///     Implements the PropertyChanged event.
    /// </summary>
    public static event EventHandler<PropertyChangedEventArgs>? StaticPropertyChanged;

    internal static void NotifyPropertyChanged(string propertyName) => StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameter is mandatory with [CallerMemberName]")]
    internal static void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "") => StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
