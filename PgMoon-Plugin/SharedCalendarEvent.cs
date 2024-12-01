namespace PgMoon;

using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
public class SharedCalendarEvent(MoonPhase moonPhase, int moonMonth, DateTime phaseStartTime, DateTime phaseEndTime)
{
    public MoonPhase MoonPhase { get; } = moonPhase;
    public int MoonMonth { get; } = moonMonth;
    public DateTime PhaseStartTime { get; } = phaseStartTime;
    public DateTime PhaseEndTime { get; } = phaseEndTime;

    public static bool TryParse(DateTime? startDate, DateTime? endDate, out SharedCalendarEvent? calendarEvent)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            DateTime StartTime = startDate.Value.ToUniversalTime();
            DateTime EndTime = endDate.Value.ToUniversalTime();

            PhaseCalculator.DateTimeToMoonPhase(StartTime, out int MoonMonth, out MoonPhase MoonPhase, out DateTime PhaseStartTime, out DateTime PhaseEndTime, out _, out _);

            if (PhaseStartTime == StartTime && PhaseEndTime == EndTime)
            {
                calendarEvent = new SharedCalendarEvent(MoonPhase, MoonMonth, PhaseStartTime, PhaseEndTime);
                return true;
            }
        }

        calendarEvent = null;
        return false;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
