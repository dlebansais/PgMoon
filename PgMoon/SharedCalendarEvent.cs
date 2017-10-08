using System;

namespace PgMoon
{
    public class SharedCalendarEvent
    {
        public SharedCalendarEvent(MoonPhase MoonPhase, int MoonMonth, DateTime PhaseStartTime, DateTime PhaseEndTime)
        {
            this.MoonPhase = MoonPhase;
            this.MoonMonth = MoonMonth;
            this.PhaseStartTime = PhaseStartTime;
            this.PhaseEndTime = PhaseEndTime;
        }

        public MoonPhase MoonPhase { get; private set; }
        public int MoonMonth { get; private set; }
        public DateTime PhaseStartTime { get; private set; }
        public DateTime PhaseEndTime { get; private set; }

        public static bool TryParse(DateTime? StartDate, DateTime? EndDate, out SharedCalendarEvent Event)
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                DateTime StartTime = StartDate.Value.ToUniversalTime();
                DateTime EndTime = EndDate.Value.ToUniversalTime();

                int MoonMonth;
                MoonPhase MoonPhase;
                DateTime PhaseStartTime;
                DateTime PhaseEndTime;
                double ProgressToFullMoon;
                DateTime NextFullMoonTime;
                PhaseCalculator.DateTimeToMoonPhase(StartTime, out MoonMonth, out MoonPhase, out PhaseStartTime, out PhaseEndTime, out ProgressToFullMoon, out NextFullMoonTime);

                if (PhaseStartTime == StartTime && PhaseEndTime == EndTime)
                {
                    Event = new SharedCalendarEvent(MoonPhase, MoonMonth, PhaseStartTime, PhaseEndTime);
                    return true;
                }
            }

            Event = null;
            return false;
        }
    }
}
