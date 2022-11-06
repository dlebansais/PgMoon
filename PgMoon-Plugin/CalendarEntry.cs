namespace PgMoon;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a moment in time defined by its moon phase.
/// </summary>
public class CalendarEntry : INotifyPropertyChanged
{
    #region Constants
    /// <summary>
    /// Gets the destination of the Rahu boat (long text).
    /// </summary>
    public static string RahuBoatDestinationLongText => "Rahu Boat Destination";

    /// <summary>
    /// Gets the destination of the Rahu boat (short text).
    /// </summary>
    public static string RahuBoatDestinationShortText => "Rahu Boat";

    /// <summary>
    /// Gets the Mushroom Circle Recall (long text).
    /// </summary>
    public static string PortToCircleLongText => "Mushroom Circle Recall";

    /// <summary>
    /// Gets the Mushroom Circle Recall (short text).
    /// </summary>
    public static string PortToCircleShortText => "Circle";
    #endregion

    #region Init
    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarEntry"/> class.
    /// </summary>
    /// <param name="moonMonth">The Moon month.</param>
    /// <param name="moonPhase">The Moon phase.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="mushroomInfoList">The list of mushroom info.</param>
    public CalendarEntry(int moonMonth, MoonPhase moonPhase, DateTime startTime, DateTime endTime, ICollection<MushroomInfo> mushroomInfoList)
    {
        MoonMonth = moonMonth;
        MoonPhase = moonPhase;
        StartTime = startTime;
        EndTime = endTime;
        MushroomInfoList = mushroomInfoList;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets the Moon month.
    /// </summary>
    public int MoonMonth { get; }

    /// <summary>
    /// Gets the Moon phase.
    /// </summary>
    public MoonPhase MoonPhase { get; }

    /// <summary>
    /// Gets the start time.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// Gets the end time.
    /// </summary>
    public DateTime EndTime { get; }

    /// <summary>
    /// Gets the list of mushroom info.
    /// </summary>
    public ICollection<MushroomInfo> MushroomInfoList { get; }

    /// <summary>
    /// Gets a value indicating whether this entry is the current one.
    /// </summary>
    public bool IsCurrent { get { return PhaseCalculator.IsCurrent(MoonMonth, MoonPhase); } }

    /// <summary>
    /// Gets the entry summary.
    /// </summary>
    public string Summary
    {
        get
        {
            string Result = string.Empty;
            string GrowingRobustly = string.Empty;

            foreach (MushroomInfo Item in MushroomInfoList)
                if (Item.RobustGrowthPhase1 == MoonPhase || Item.RobustGrowthPhase2 == MoonPhase)
                {
                    if (GrowingRobustly.Length > 0)
                        GrowingRobustly += ", ";

                    GrowingRobustly += Item.Name;
                }

            if (GrowingRobustly.Length > 0)
                Result += $"Growing Robustly: {GrowingRobustly}\r\n";

            Result += $"{RahuBoatDestinationLongText}: {MoonPhase.RahuBoatDestination}\r\n";
            Result += $"{PortToCircleLongText}: {MoonPhase.FastPortMushroomLongText}";

            return Result;
        }
    }
    #endregion

    #region Client Interface
    /// <summary>
    /// Updates properties.
    /// </summary>
    public void Update()
    {
        NotifyPropertyChanged(nameof(IsCurrent));
    }
    #endregion

    #region Implementation
    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{MoonPhase} - {EndTime.ToLocalTime().ToString(CultureInfo.CurrentCulture)}";
    }
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
}
