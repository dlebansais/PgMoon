namespace PgMoon
{
    using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
    public class SharedCalendarEvent
    {
        public SharedCalendarEvent(MoonPhase moonPhase, int moonMonth, DateTime phaseStartTime, DateTime phaseEndTime)
        {
            MoonPhase = moonPhase;
            MoonMonth = moonMonth;
            PhaseStartTime = phaseStartTime;
            PhaseEndTime = phaseEndTime;
        }

        public MoonPhase MoonPhase { get; private set; }
        public int MoonMonth { get; private set; }
        public DateTime PhaseStartTime { get; private set; }
        public DateTime PhaseEndTime { get; private set; }

        public static bool TryParse(DateTime? startDate, DateTime? endDate, out SharedCalendarEvent? calendarEvent)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTime StartTime = startDate.Value.ToUniversalTime();
                DateTime EndTime = endDate.Value.ToUniversalTime();

                int MoonMonth;
                MoonPhase MoonPhase;
                DateTime PhaseStartTime;
                DateTime PhaseEndTime;
                double ProgressToFullMoon;
                DateTime NextFullMoonTime;
                PhaseCalculator.DateTimeToMoonPhase(StartTime, out MoonMonth, out MoonPhase, out PhaseStartTime, out PhaseEndTime, out ProgressToFullMoon, out NextFullMoonTime);

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
}
